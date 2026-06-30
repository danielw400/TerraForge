using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TerraForge.Engine.RenderingBridge
{
    public sealed class WebSocketBridge : IDisposable
    {
        private readonly RenderingBridge _renderingBridge;
        private readonly HttpListener _listener;
        private readonly List<WebSocket> _clients = new List<WebSocket>();
        private readonly object _clientsLock = new object();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly int _broadcastIntervalMs;
        private Task? _acceptLoopTask;
        private Task? _broadcastLoopTask;

        public WebSocketBridge(RenderingBridge renderingBridge, string httpPrefix = "http://localhost:3000/", int broadcastIntervalMs = 250)
        {
            _renderingBridge = renderingBridge ?? throw new ArgumentNullException(nameof(renderingBridge));
            _broadcastIntervalMs = Math.Max(50, broadcastIntervalMs);
            _listener = new HttpListener();
            _listener.Prefixes.Add(httpPrefix);
        }

        public void Start()
        {
            if (_listener.IsListening) return;

            Console.WriteLine("[Network] Connecting...");
            _listener.Start();
            _acceptLoopTask = Task.Run(() => AcceptLoopAsync(_cancellationTokenSource.Token));
            _broadcastLoopTask = Task.Run(() => BroadcastLoopAsync(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (!_listener.IsListening) return;

            _cancellationTokenSource.Cancel();
            _listener.Stop();

            lock (_clientsLock)
            {
                foreach (var client in _clients.ToList())
                {
                    try
                    {
                        client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait();
                    }
                    catch
                    {
                        // ignore cleanup errors
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }

                _clients.Clear();
            }
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WebSocketBridge] Accept error: {ex}");
                    continue;
                }

                _ = Task.Run(() => HandleContextAsync(context, cancellationToken));
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                var path = request.Url?.AbsolutePath ?? "/";

                if (path.Equals("/ws", StringComparison.OrdinalIgnoreCase) && request.IsWebSocketRequest)
                {
                    await AcceptWebSocketClientAsync(context, cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    if (path.Equals("/frame", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteJsonResponseAsync(response, _renderingBridge.CollectFrameStateJson()).ConfigureAwait(false);
                        return;
                    }

                    if (path.Equals("/frame/initial", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteJsonResponseAsync(response, _renderingBridge.CollectInitialSnapshotJson()).ConfigureAwait(false);
                        return;
                    }
                }

                response.StatusCode = 404;
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketBridge] Context handling error: {ex}");
            }
        }

        private static async Task WriteJsonResponseAsync(HttpListenerResponse response, string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes.AsMemory(0, bytes.Length)).ConfigureAwait(false);
            response.Close();
        }

        private async Task AcceptWebSocketClientAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            WebSocketContext? socketContext = null;

            try
            {
                socketContext = await context.AcceptWebSocketAsync(subProtocol: null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketBridge] WebSocket accept failed: {ex}");
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            var socket = socketContext.WebSocket;

            lock (_clientsLock)
            {
                foreach (var existing in _clients.ToList())
                {
                    if (existing.State == WebSocketState.Open)
                    {
                        existing.Abort();
                    }
                }

                _clients.Add(socket);
            }

            Console.WriteLine("[Network] Connected.");

            try
            {
                await SendInitialSnapshotAsync(socket, cancellationToken).ConfigureAwait(false);
                await ReceiveLoopAsync(socket, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                lock (_clientsLock)
                {
                    _clients.Remove(socket);
                }

                Console.WriteLine("[Network] Connection lost.");
                socket.Dispose();
            }
        }

        private async Task ReceiveLoopAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[1024]);

            while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None).ConfigureAwait(false);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
                    {
                        var rawMessage = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                        HandleIncomingMessage(rawMessage);
                    }
                }
                catch (WebSocketException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Network] Receive error: {ex}");
                    break;
                }
            }
        }

        private static void HandleIncomingMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                using var document = JsonDocument.Parse(message);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (document.RootElement.TryGetProperty("action", out var actionProperty) &&
                        actionProperty.ValueKind == JsonValueKind.String)
                    {
                        Console.WriteLine($"[Network] Received input command: {actionProperty.GetString()}");
                    }
                    else if (document.RootElement.TryGetProperty("type", out var typeProperty) &&
                             typeProperty.ValueKind == JsonValueKind.String)
                    {
                        Console.WriteLine($"[Network] Received message type: {typeProperty.GetString()}");
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore non-JSON messages for now; future input formats can be added here.
            }
        }

        private async Task BroadcastLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_broadcastIntervalMs, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    List<WebSocket> clientsCopy;
                    lock (_clientsLock)
                    {
                        clientsCopy = new List<WebSocket>(_clients);
                    }

                    foreach (var client in clientsCopy)
                    {
                        if (client.State != WebSocketState.Open)
                        {
                            lock (_clientsLock)
                            {
                                _clients.Remove(client);
                            }

                            Console.WriteLine("[Network] Connection lost.");
                            continue;
                        }

                        await SendFrameAsync(client, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Network] Frame update failed: {ex}");
                }
            }
        }

        private async Task SendInitialSnapshotAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return;
            }

            var json = _renderingBridge.CollectInitialSnapshotJson();
            await SendPayloadAsync(webSocket, json, cancellationToken).ConfigureAwait(false);
            Console.WriteLine("[Network] InitialSnapshot sent.");
        }

        private async Task SendFrameAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return;
            }

            var json = _renderingBridge.CollectFrameStateJson();
            await SendPayloadAsync(webSocket, json, cancellationToken).ConfigureAwait(false);
            Console.WriteLine("[Network] FrameUpdate sent.");
        }

        private static async Task SendPayloadAsync(WebSocket webSocket, string json, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            try
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
            }
            catch (WebSocketException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
        }
    }
}

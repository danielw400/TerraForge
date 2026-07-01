using System;
using System.Text.Json;
using TerraForge.Engine.RenderingBridge.Dtos;
using TerraForge.Game;

namespace TerraForge.Engine.RenderingBridge
{
    public sealed class RenderingBridge
    {
        private readonly WorldStatePublisher _publisher;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public RenderingBridge(WorldStatePublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public FrameUpdateDto CollectFrameState()
        {
            return _publisher.PublishFrameState();
        }

        public FrameUpdateDto CollectInitialSnapshot()
        {
            return _publisher.PublishInitialSnapshot();
        }

        public string CollectFrameStateJson()
        {
            var frame = CollectFrameState();
            return JsonSerializer.Serialize(frame, JsonOptions);
        }

        public string CollectInitialSnapshotJson()
        {
            var snapshot = CollectInitialSnapshot();
            return JsonSerializer.Serialize(snapshot, JsonOptions);
        }
    }
}

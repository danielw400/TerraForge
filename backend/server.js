import express from 'express';
import cors from 'cors';
import { WebSocketServer } from 'ws';
import { WorldSimulator } from './worldSimulator.js';

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

const worldSim = new WorldSimulator();

/**
 * GET /frame
 * Returns a FrameUpdateDto with chunks, player state, zombies, and camera
 * (simulating RenderingBridge.CollectFrameStateJson())
 */
app.get('/frame', (req, res) => {
    const frame = worldSim.generateFrame();
    res.json(frame);
});

/**
 * GET /frame/initial
 * Returns initial snapshot (useful for debugging)
 */
app.get('/frame/initial', (req, res) => {
    const frame = worldSim.generateInitialSnapshot();
    res.json(frame);
});

/**
 * GET /health
 * Simple health check
 */
app.get('/health', (req, res) => {
    res.json({ status: 'ok', time: new Date().toISOString() });
});

const server = app.listen(PORT, () => {
    console.log(`🎮 TerraForge dev server running on http://localhost:${PORT}`);
    console.log(`📡 /frame endpoint: http://localhost:${PORT}/frame`);
    console.log(`📡 /frame/initial endpoint: http://localhost:${PORT}/frame/initial`);
    console.log(`📡 WebSocket endpoint: ws://localhost:${PORT}/ws`);
    console.log(`💚 /health endpoint: http://localhost:${PORT}/health`);
});

const wss = new WebSocketServer({ noServer: true });

server.on('upgrade', (request, socket, head) => {
    if (request.url === '/ws') {
        wss.handleUpgrade(request, socket, head, ws => {
            wss.emit('connection', ws, request);
        });
    } else {
        socket.destroy();
    }
});

wss.on('connection', ws => {
    console.log('[DevServer] WebSocket client connected');
    ws.send(JSON.stringify(worldSim.generateFrame()));

    ws.on('message', (data) => {
        try {
            const message = JSON.parse(data.toString());
            if (message && message.type === 'input' && Array.isArray(message.commands)) {
                worldSim.applyInputCommands(message.commands);
            }
        } catch (err) {
            console.warn('[DevServer] Failed to parse WebSocket message', err);
        }
    });

    ws.on('close', () => {
        console.log('[DevServer] WebSocket client disconnected');
    });

    ws.on('error', error => {
        console.warn('[DevServer] WebSocket error', error);
    });
});

const broadcastInterval = setInterval(() => {
    const frame = worldSim.generateFrame();
    const payload = JSON.stringify(frame);
    for (const client of wss.clients) {
        if (client.readyState === client.OPEN) {
            client.send(payload);
        }
    }
}, 250);

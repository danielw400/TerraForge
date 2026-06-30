import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';
import { createScene, createCamera, createRenderer, createGrid, createReferenceCube } from './scene.js';
import { NetworkClient } from './network.js';
import { ChunkRenderer } from './chunkRenderer.js';
import { CameraController } from './cameraController.js';
import { EntityRenderer } from './entityRenderer.js';
import { BrowserInputProvider } from './inputProvider.js';

const container = document.getElementById('canvas-container');
const statusBackend = document.getElementById('status-backend');
const statusWebSocket = document.getElementById('status-websocket');
const statusSync = document.getElementById('status-sync');
const statChunks = document.getElementById('stat-chunks');
const statEntities = document.getElementById('stat-entities');
const statZombies = document.getElementById('stat-zombies');
const statPlayerPos = document.getElementById('stat-player-pos');
const statFps = document.getElementById('stat-fps');
const statLatency = document.getElementById('stat-latency');
const scene = createScene();
const camera = createCamera(container);
const renderer = createRenderer(container);
const grid = createGrid();
const cube = createReferenceCube();

scene.add(grid);
scene.add(cube);

// create chunk renderer instance and hook into scene
const chunkRenderer = new ChunkRenderer(scene, { chunkSize: 16, blockScale: 1 });

// create entity renderer for zombies and player
const entityRenderer = new EntityRenderer(scene, { debug: false, defaultScale: 1.0 });

// --- CameraController: third-person follow or explicit camera state ---
const cameraController = new CameraController(camera, {
    thirdPersonDistance: 8.0,
    thirdPersonHeight: 6.0,
    smoothFactor: 0.15,
    debug: false
});

// --- Network: carregar snapshot inicial com fallback e WebSocket em tempo real ---
const net = new NetworkClient();
const inputProvider = new BrowserInputProvider(container);
const enterGameButton = document.getElementById('enter-game-button');
const inputFlushIntervalMs = 100;
const pendingInputCommands = [];

const backendBaseUrl = NetworkClient.getBackendBaseUrl();
const backendFrameUrl = `${backendBaseUrl.replace(/\/+$/, '')}/frame`;
const backendHealthUrl = `${backendBaseUrl.replace(/\/+$/, '')}/health`;
const backendWebSocketUrl = NetworkClient.getWebSocketUrl(backendFrameUrl);

let lastFrameTime = 0;
let fps = 0;
let lastLatency = null;

console.log('[Network] Backend base URL', backendBaseUrl);
console.log('[Network] Backend frame URL', backendFrameUrl);
console.log('[Network] Backend health URL', backendHealthUrl);
console.log('[Network] Backend WebSocket URL', backendWebSocketUrl);

function applyEntities(frame) {
    if (!frame) return;

    currentFrame = frame;
    entityRenderer.updatePlayer(frame.player);
    entityRenderer.updateZombies(frame.zombies);
    entityRenderer.updateEnvironmentObjects(frame.environmentObjects);
}

function applyNetworkFrame(frame) {
    if (!frame) return;
    chunkRenderer.applyFrame(frame);
    applyEntities(frame);
    if (!backendCameraInitialized && frame.camera && frame.camera.position) {
        camera.position.set(frame.camera.position.x, frame.camera.position.y, frame.camera.position.z);
        if (frame.camera.target) {
            camera.lookAt(frame.camera.target.x, frame.camera.target.y, frame.camera.target.z);
        }
        backendCameraInitialized = true;
    }
    updateWorldStats(frame);
}

let currentFrame = null;
let backendCameraInitialized = false;
let backendAvailable = false;

net.onFrame(frame => {
    if (frame && frame.serverTime) {
        lastLatency = Date.now() - new Date(frame.serverTime).getTime();
    }
    applyNetworkFrame(frame);
});
function sendPendingInputCommands() {
    if (!net.isWebSocketConnected() || pendingInputCommands.length === 0) {
        return;
    }

    const batch = pendingInputCommands.splice(0, pendingInputCommands.length);
    const sent = net.sendInputCommands(batch);
    if (!sent) {
        pendingInputCommands.unshift(...batch);
    }
}

function flushInputCommands() {
    const commands = inputProvider.drainCommands();
    if (!Array.isArray(commands) || commands.length === 0) {
        return;
    }

    pendingInputCommands.push(...commands);
    if (net.isWebSocketConnected()) {
        sendPendingInputCommands();
    }
}

net.onStatus(status => {
    if (typeof status !== 'string') {
        return;
    }

    console.log('[App] Network status:', status);

    if (status.includes('Backend connected')) {
        setConnectionStatus('Connected', 'Disconnected', 'HTTP Polling');
    } else if (status.includes('Backend disconnected')) {
        setConnectionStatus('Disconnected', 'Disconnected', 'Stopped');
    } else if (status.includes('WebSocket connected')) {
        setConnectionStatus('Connected', 'Connected', 'Running');
    } else if (status.includes('WebSocket reconnected')) {
        setConnectionStatus('Connected', 'Reconnecting', 'Running');
    } else if (status.includes('WebSocket disconnected')) {
        setConnectionStatus('Connected', 'Disconnected', 'HTTP Polling');
    } else if (status.includes('HTTP polling active')) {
        setConnectionStatus('Connected', 'Disconnected', 'HTTP Polling');
    }

    if (status.includes('WebSocket connected') || status.includes('WebSocket reconnected')) {
        sendPendingInputCommands();
    }
});

async function initializeBackend() {
    console.log('[Network] Backend URL:', backendBaseUrl);
    console.log('[Network] Backend frame URL:', backendFrameUrl);
    console.log('[Network] Backend health URL:', backendHealthUrl);

    const health = await net.checkHealth(backendHealthUrl);
    if (!health.ok) {
        console.warn('[App] Backend health check failed, using offline snapshot');
        console.log('[Network] Offline Mode Enabled');
        setConnectionStatus('Disconnected', 'Disconnected', 'Stopped');
        await loadOfflineSnapshot();
        return;
    }

    console.log('[Network] Backend health verified');
    setConnectionStatus('Connected', 'Disconnected', 'HTTP Polling');

    if (backendWebSocketUrl) {
        try {
            await net.connectWebSocket(backendWebSocketUrl, { pollingUrl: backendFrameUrl, retryIntervalMs: 1000 });
            backendAvailable = true;
            console.log('[Network] WebSocket Connected');
            return;
        } catch (err) {
            console.warn('[App] WebSocket unavailable, using HTTP polling');
            console.log('[Network] Using HTTP Polling');
        }
    }

    try {
        const frame = await net.fetchFrame(backendFrameUrl);
        backendAvailable = true;
        console.log('[App] Backend HTTP snapshot loaded successfully');
        applyNetworkFrame(frame);
        net.startPolling(backendFrameUrl, 250);
        setConnectionStatus('Connected', 'Disconnected', 'HTTP Polling');
    } catch (err) {
        console.warn('[App] Backend HTTP unavailable:', err);
        console.log('[Network] Offline Mode Enabled');
        setConnectionStatus('Disconnected', 'Disconnected', 'Stopped');
        await loadOfflineSnapshot();
    }
}

function getStatusClass(label) {
    if (label === 'Connected' || label === 'Active' || label === 'HTTP Polling' || label === 'Running') {
        return 'status-connected';
    }
    if (label === 'Reconnecting') {
        return 'status-reconnecting';
    }
    return 'status-disconnected';
}

function setStatus(element, text, cssClass) {
    if (!element) return;
    element.textContent = text;
    element.className = `status-value ${cssClass}`;
}

function setConnectionStatus(backend, websocket, sync) {
    setStatus(statusBackend, backend, getStatusClass(backend));
    setStatus(statusWebSocket, websocket, getStatusClass(websocket));
    setStatus(statusSync, sync, getStatusClass(sync));
}

setConnectionStatus('Disconnected', 'Disconnected', 'Stopped');

function updateWorldStats(frame) {
    if (!frame) return;
    statChunks.textContent = `${chunkRenderer.chunks.size}`;
    const zombiesCount = Array.isArray(frame.zombies) ? frame.zombies.length : 0;
    const environmentCount = Array.isArray(frame.environmentObjects) ? frame.environmentObjects.length : 0;
    const totalEntities = 1 + zombiesCount + environmentCount;
    statEntities.textContent = `${totalEntities}`;
    statZombies.textContent = `${zombiesCount}`;
    if (frame.player && frame.player.position) {
        const pos = frame.player.position;
        statPlayerPos.textContent = `${pos.x.toFixed(1)}, ${pos.y.toFixed(1)}, ${pos.z.toFixed(1)}`;
    }
    if (typeof fps === 'number') {
        statFps.textContent = `${Math.round(fps)}`;
    }
    if (lastLatency !== null) {
        statLatency.textContent = `${Math.round(lastLatency)} ms`;
    }
}

async function loadOfflineSnapshot() {
    try {
        const frame = await net.fetchFrame('initial-snapshot.json');
        console.log('[App] Initial snapshot loaded (offline mode)');
        applyNetworkFrame(frame);
    } catch (err) {
        console.warn('[App] Failed to load initial snapshot, trying sample frame:', err);
        try {
            const frame = await net.fetchFrame('sample-frame.json');
            console.log('[App] Sample snapshot loaded (offline mode)');
            applyNetworkFrame(frame);
        } catch (finalErr) {
            console.error('[App] Failed to load any snapshot:', finalErr);
        }
    }
}

function startBackendPolling() {
    if (net.isWebSocketConnected()) {
        return;
    }

    if (backendAvailable) {
        net.startPolling(backendFrameUrl, 250);
    }
}

initializeBackend();
inputProvider.start();
setInterval(flushInputCommands, inputFlushIntervalMs);

enterGameButton?.addEventListener('click', async () => {
    console.log('[App] Enter Game button clicked');
    await inputProvider.requestPointerLock();
});

window.addEventListener('resize', onResize);

let lastTime = 0;

function onResize() {
    camera.aspect = container.clientWidth / container.clientHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(container.clientWidth, container.clientHeight);
}

function animate(time) {
    const deltaMs = time - lastTime;
    const delta = deltaMs * 0.001;
    if (deltaMs > 0) {
        fps = 1000 / deltaMs;
    }
    lastTime = time;

    cube.rotation.y += delta * 0.45;
    cube.rotation.x += delta * 0.12;

    if (currentFrame && currentFrame.player && currentFrame.player.position) {
        const playerPos = currentFrame.player.position;
        const cameraState = currentFrame.camera || null;
        cameraController.update(playerPos, cameraState);
    }

    updateWorldStats(currentFrame);
    renderer.render(scene, camera);
    requestAnimationFrame(animate);
}

requestAnimationFrame(animate);

import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';
import { createScene, createCamera, createRenderer, createGrid, createReferenceCube } from './scene.js';
import { NetworkClient } from './network.js';
import { ChunkRenderer } from './chunkRenderer.js';
import { CameraController } from './cameraController.js';
import { EntityRenderer } from './entityRenderer.js';
import { BrowserInputProvider } from './inputProvider.js';

const container = document.getElementById('canvas-container');
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

console.log('[Network] Backend base URL', backendBaseUrl);
console.log('[Network] Backend frame URL', backendFrameUrl);
console.log('[Network] Backend health URL', backendHealthUrl);
console.log('[Network] Backend WebSocket URL', backendWebSocketUrl);

function applyEntities(frame) {
    if (!frame) return;

    currentFrame = frame;
    entityRenderer.updatePlayer(frame.player);
    entityRenderer.updateZombies(frame.zombies);
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
}

let currentFrame = null;
let backendCameraInitialized = false;
let backendAvailable = false;

net.onFrame(frame => {
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
    if (typeof status === 'string' && (status.includes('WebSocket connected') || status.includes('WebSocket disconnected') || status.includes('WebSocket reconnected'))) {
        console.log('[App] Network status:', status);
    }

    if (typeof status === 'string' && (status.includes('WebSocket connected') || status.includes('WebSocket reconnected'))) {
        sendPendingInputCommands();
    }
});

async function initializeBackend() {
    const healthOk = await net.checkHealth(backendHealthUrl);
    if (!healthOk) {
        console.warn('[App] Backend health check failed, will try offline snapshot');
        await loadOfflineSnapshot();
        return;
    }

    if (backendWebSocketUrl) {
        try {
            await net.connectWebSocket(backendWebSocketUrl, { pollingUrl: backendFrameUrl, retryIntervalMs: 1000 });
            backendAvailable = true;
            console.log('[App] Connected to backend WebSocket');
            return;
        } catch (err) {
            console.warn('[App] WebSocket backend unavailable, falling back to HTTP.');
        }
    }

    try {
        const frame = await net.fetchFrame(backendFrameUrl);
        backendAvailable = true;
        console.log('[App] Backend HTTP snapshot loaded successfully');
        applyNetworkFrame(frame);
        net.startPolling(backendFrameUrl, 250);
    } catch (err) {
        console.warn('[App] Backend HTTP unavailable:', err);
        await loadOfflineSnapshot();
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
    const delta = (time - lastTime) * 0.001;
    lastTime = time;

    cube.rotation.y += delta * 0.45;
    cube.rotation.x += delta * 0.12;

    if (currentFrame && currentFrame.player && currentFrame.player.position) {
        const playerPos = currentFrame.player.position;
        const cameraState = currentFrame.camera || null;
        cameraController.update(playerPos, cameraState);
    }

    renderer.render(scene, camera);
    requestAnimationFrame(animate);
}

requestAnimationFrame(animate);

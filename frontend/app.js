import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';
import { createScene, createCamera, createRenderer, createGrid, createReferenceCube } from './scene.js';
import { NetworkClient } from './network.js';
import { ChunkRenderer } from './chunkRenderer.js';
import { CameraController } from './cameraController.js';
import { EntityRenderer } from './entityRenderer.js';

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
const entityRenderer = new EntityRenderer(scene, { debug: true, defaultScale: 1.0 });

// --- CameraController: third-person follow or explicit camera state ---
const cameraController = new CameraController(camera, {
    thirdPersonDistance: 8.0,
    thirdPersonHeight: 6.0,
    smoothFactor: 0.15,
    debug: true
});

// --- Network: carregar snapshot inicial com fallback e WebSocket em tempo real ---
const net = new NetworkClient();

function getBackendUrl() {
    const params = new URLSearchParams(window.location.search);
    if (params.has('backendUrl')) {
        return params.get('backendUrl');
    }

    const stored = localStorage.getItem('terraforge-backend-url');
    if (stored) {
        return stored;
    }

    return 'http://localhost:3000/frame';
}

function createPlayerMesh() {
    const geo = new THREE.SphereGeometry(0.5, 12, 12);
    const mat = new THREE.MeshStandardMaterial({ color: 0x2233ff });
    const m = new THREE.Mesh(geo, mat);
    m.castShadow = true;
    m.receiveShadow = true;
    m.visible = false;
    scene.add(m);
    return m;
}

function applyEntities(frame) {
    if (!frame) return;

    currentFrame = frame;

    if (frame.player && frame.player.position) {
        playerMesh.visible = true;
        playerMesh.position.set(frame.player.position.x, frame.player.position.y, frame.player.position.z);
    }

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
let backendConnectedWithWs = false;
const backendFrameUrl = getBackendUrl();
const backendWebSocketUrl = NetworkClient.getWebSocketUrl(backendFrameUrl);

net.onFrame(frame => {
    applyNetworkFrame(frame);
});

net.onStatus(status => {
    console.log('[App] Network status:', status);
});

async function initializeBackend() {
    if (backendWebSocketUrl) {
        try {
            await net.connectWebSocket(backendWebSocketUrl);
            backendAvailable = true;
            backendConnectedWithWs = true;
            console.log('[App] Connected to backend WebSocket:', backendWebSocketUrl);
        } catch (err) {
            console.warn('[App] WebSocket backend unavailable, falling back to HTTP:', err);
        }
    }

    if (!backendAvailable) {
        try {
            const frame = await net.fetchFrame(backendFrameUrl);
            backendAvailable = true;
            console.log('[App] Backend HTTP snapshot loaded successfully:', backendFrameUrl);
            applyNetworkFrame(frame);
            startBackendPolling();
            return;
        } catch (err) {
            console.warn('[App] Backend HTTP unavailable:', err);
        }
    }

    await loadOfflineSnapshot();
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
    if (backendConnectedWithWs) {
        console.log('[App] WebSocket backend active; HTTP polling disabled.');
        return;
    }

    if (backendAvailable) {
        try {
            net.startPolling(backendFrameUrl, 250);
            console.log('[App] Backend polling enabled:', backendFrameUrl);
        } catch (err) {
            console.error('[App] Failed to start polling:', err);
        }
    } else {
        console.log('[App] Backend not available - polling disabled (offline mode)');
    }
}

initializeBackend();

window.addEventListener('resize', onResize);

let lastTime = 0;

function onResize() {
    camera.aspect = container.clientWidth / container.clientHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(container.clientWidth, container.clientHeight);
}

const playerMesh = createPlayerMesh();

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

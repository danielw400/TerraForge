# TerraForge Backend Integration — Quick Start

## Overview

This document guides you through running the TerraForge visualization pipeline:
- **Backend Dev Server**: Node.js server simulating `RenderingBridge` (endpoint: `http://localhost:3000/frame`)
- **Frontend**: Three.js web client rendering chunks, player, and zombies

## Prerequisites

- Node.js 14+
- Python 3 (for serving frontend static files)
- Browser with WebGL support

## Quick Start (5 minutes)

### Terminal 1: Start Backend Dev Server

```bash
cd backend
npm install
npm start
```

Expected output:
```
🎮 TerraForge dev server running on http://localhost:3000
📡 /frame endpoint: http://localhost:3000/frame
💚 /health endpoint: http://localhost:3000/health
```

### Terminal 2: Start Frontend

```bash
cd frontend
python3 -m http.server 8000
```

Then open your browser to: **http://localhost:8000**

You should see:
- ✅ Two chunks with varied blocks (stone, dirt, wood)
- ✅ Blue sphere (player) in center
- ✅ Red spheres (zombies) orbiting the player
- ✅ Live animation (entities moving)

## Troubleshooting

### "Backend unavailable" message

If you see this in the console, the frontend couldn't connect to `http://localhost:3000/frame`. Make sure:
1. Backend server is running (`npm start` in `backend/`)
2. Backend is on port 3000 (or set `BACKEND_URL` env var)
3. CORS is enabled (it is, by default in `server.js`)

**Fallback**: Frontend will use `initial-snapshot.json` (static test data)

### Entities not rendering

Check browser console for errors. Common issues:
- THREE module not loading (network error) — try refreshing
- ChunkRenderer error — backend JSON format mismatch

### Chunks not visible

Try zooming out with mouse wheel or adjusting camera position in console:
```javascript
camera.position.set(15, 15, 40);
camera.lookAt(2, 1.5, 2);
```

## Architecture

```
Backend Dev Server (Node.js)
    ↓ (HTTP GET /frame)
Frontend (Three.js in Browser)
    ├─ ChunkRenderer (pool-based InstancedMesh)
    ├─ Player Mesh (blue sphere)
    └─ Zombie Meshes (red spheres)
```

### Data Flow

1. Frontend boots → tries `http://localhost:3000/frame`
2. Backend returns `FrameUpdateDto` JSON (chunks, player, zombies, camera)
3. Frontend parses and applies to scene
4. Frontend polls `/frame` every 250ms for updates
5. Entities animate as server updates positions

## Next Steps

- [ ] Replace dev server simulation with real C# `RenderingBridge`
- [ ] Add WebSocket for real-time updates
- [ ] Implement player input (WASD, mouse look)
- [ ] Add greedy meshing for better performance
- [ ] Deploy to production with actual game logic

## Files

- `backend/server.js` — Main Express server
- `backend/worldSimulator.js` — Simulates RenderingBridge/world state
- `backend/package.json` — Dependencies
- `frontend/app.js` — Main Three.js app (calls NetworkClient)
- `frontend/network.js` — HTTP client for fetching frames
- `frontend/chunkRenderer.js` — Renders chunks with InstancedMesh

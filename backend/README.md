# Backend Integration Guide

## Setup

### Prerequisites
- Node.js 14+ (for dev server)
- npm or yarn

### Installation

```bash
cd backend
npm install
```

### Running the Dev Server

```bash
npm start
# or with npm scripts:
npm run dev
```

The server will start on `http://localhost:3000` with the following endpoints:

- **`GET /frame`** — Returns a live frame snapshot (player, chunks, zombies, camera)
- **`GET /frame/initial`** — Returns initial snapshot (static state)
- **`GET /health`** — Health check
- **`ws://localhost:3000/ws`** — WebSocket endpoint for live frame broadcast

### Example: Fetch a Frame

```bash
curl http://localhost:3000/frame | jq .
```

## Frontend Configuration

The frontend (`frontend/app.js`) will attempt to load the initial snapshot in this order:

1. Backend HTTP endpoint: `http://localhost:3000/frame`
2. Local snapshot: `frontend/initial-snapshot.json`
3. Sample snapshot: `frontend/sample-frame.json`

### Environment Variable (Optional)

To point to a different backend:

```bash
# In browser console or via .env
BACKEND_URL=http://your-backend:port/frame
```

## Development Workflow

### Terminal 1: Backend
```bash
cd backend
npm start
# Server listening on http://localhost:3000
```

### Terminal 2: Frontend
```bash
cd frontend
python3 -m http.server 8000
# Open http://localhost:8000 in browser
```

The frontend will automatically load live data from the backend server. Chunks, player, and zombies will animate as the server generates new positions each frame.

## Simulated Data

Currently `worldSimulator.js` generates:
- 2 static chunks (0,0,0) and (1,0,0) with random block types
- Player entity that slowly orbits around origin
- 2-3 zombie entities that orbit the player at distance ~18 units
- Camera positioned above and behind the player

This simulates what `RenderingBridge.CollectFrameStateJson()` would return from the real C# backend.

## Next Steps

1. **WebSocket Integration** — Replace polling with real-time updates
2. **C# Backend Integration** — Replace `worldSimulator.js` with actual `RenderingBridge` calls
3. **Persistent State** — Store world modifications and apply them incrementally
4. **Performance Optimization** — Implement delta updates (only changed chunks/entities per frame)

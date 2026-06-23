// Módulo network.js
// Responsável por buscar JSON de frame exportado pelo backend,
// interpretar os DTOs, e receber updates por WebSocket quando disponível.

export class NetworkClient {
    constructor() {
        this._frameListeners = new Set();
        this._statusListeners = new Set();
        this._pollIntervalId = null;
        this._ws = null;
        this._wsUrl = null;
    }

    async fetchFrame(url) {
        console.log(`[NetworkClient] Fetching frame from: ${url}`);
        try {
            const res = await fetch(url, { cache: 'no-store' });
            if (!res.ok) {
                const err = `HTTP ${res.status} ${res.statusText}`;
                console.error(`[NetworkClient] Fetch failed: ${err}`);
                throw new Error(`fetchFrame failed: ${err}`);
            }

            const json = await res.json();
            console.log(`[NetworkClient] Frame loaded successfully from: ${url}`);
            const frame = NetworkClient.parseFrame(json);
            this._emitFrame(frame);
            return frame;
        } catch (err) {
            console.error(`[NetworkClient] Fetch error: ${err.message}`);
            throw err;
        }
    }

    async connectWebSocket(url) {
        if (typeof WebSocket === 'undefined') {
            throw new Error('WebSocket is not available in this environment');
        }

        if (!url) {
            throw new Error('Invalid WebSocket URL');
        }

        if (this._ws) {
            this.disconnectWebSocket();
        }

        this._wsUrl = url;
        return new Promise((resolve, reject) => {
            const socket = new WebSocket(url);
            this._ws = socket;

            socket.addEventListener('open', () => {
                this._emitStatus(`WebSocket connected: ${url}`);
                console.log(`[NetworkClient] WebSocket connected to ${url}`);
                resolve();
            });

            socket.addEventListener('message', event => {
                try {
                    const json = JSON.parse(event.data);
                    const frame = NetworkClient.parseFrame(json);
                    if (frame) {
                        this._emitFrame(frame);
                    }
                } catch (error) {
                    console.error('[NetworkClient] WebSocket parse error:', error);
                }
            });

            socket.addEventListener('close', event => {
                this._emitStatus(`WebSocket closed (${event.code})`);
                console.warn('[NetworkClient] WebSocket closed:', event.code, event.reason);
                this._ws = null;
            });

            socket.addEventListener('error', error => {
                this._emitStatus('WebSocket error');
                console.error('[NetworkClient] WebSocket error', error);
                if (socket.readyState !== WebSocket.OPEN) {
                    reject(new Error('WebSocket connection failed'));
                }
            });
        });
    }

    disconnectWebSocket() {
        if (!this._ws) return;
        try {
            this._ws.close(1000, 'Client closing');
        } catch (err) {
            console.warn('[NetworkClient] Failed to close WebSocket cleanly', err);
        }
        this._ws = null;
        this._emitStatus('WebSocket disconnected');
    }

    isWebSocketConnected() {
        return this._ws && this._ws.readyState === WebSocket.OPEN;
    }

    startPolling(url, intervalMs = 1000) {
        if (this._pollIntervalId) this.stopPolling();
        console.log(`[NetworkClient] Starting polling: ${url} every ${intervalMs}ms`);
        this._emitStatus(`Polling HTTP backend: ${url}`);
        this._pollIntervalId = setInterval(() => {
            this.fetchFrame(url).catch(err => {
                console.warn(`[NetworkClient] Polling fetch error (retrying): ${err.message}`);
            });
        }, intervalMs);
    }

    stopPolling() {
        if (!this._pollIntervalId) return;
        clearInterval(this._pollIntervalId);
        this._pollIntervalId = null;
        this._emitStatus('Stopped HTTP polling');
    }

    onFrame(callback) {
        this._frameListeners.add(callback);
        return () => this._frameListeners.delete(callback);
    }

    onStatus(callback) {
        this._statusListeners.add(callback);
        return () => this._statusListeners.delete(callback);
    }

    _emitFrame(frame) {
        for (const cb of this._frameListeners) {
            try {
                cb(frame);
            } catch (e) {
                console.error('frame listener error', e);
            }
        }
    }

    _emitStatus(message) {
        console.log(`[NetworkClient] ${message}`);
        for (const cb of this._statusListeners) {
            try {
                cb(message);
            } catch (e) {
                console.error('status listener error', e);
            }
        }
    }

    static getWebSocketUrl(frameUrl) {
        if (!frameUrl) return null;

        try {
            const url = new URL(frameUrl, window.location.href);
            url.protocol = url.protocol === 'https:' ? 'wss:' : 'ws:';
            url.pathname = '/ws';
            url.search = '';
            url.hash = '';
            return url.toString();
        } catch (error) {
            console.warn('[NetworkClient] Invalid backend frame URL for WebSocket', frameUrl);
            return null;
        }
    }

    static parseFrame(json) {
        if (!json) return null;

        const frameSource = json.frame ?? json.snapshot ?? json.data ?? json;
        if (!frameSource || typeof frameSource !== 'object') return null;

        return {
            chunks: Array.isArray(frameSource.chunks) ? frameSource.chunks.map(NetworkClient.parseChunk) : [],
            chunkUnloads: Array.isArray(frameSource.chunkUnloads) ? frameSource.chunkUnloads.map(c => ({ coord: c.coord })) : [],
            player: frameSource.player ? NetworkClient.parsePlayer(frameSource.player) : null,
            zombies: Array.isArray(frameSource.zombies) ? frameSource.zombies.map(NetworkClient.parseZombie) : [],
            camera: frameSource.camera ? NetworkClient.parseCamera(frameSource.camera) : null,
            raw: frameSource
        };
    }

    static parseChunk(dto) {
        const blocks = Array.isArray(dto.blocks) ? new Uint16Array(dto.blocks) : new Uint16Array(0);
        return {
            coord: dto.coord,
            blocks,
            version: dto.version ?? 0,
            isDirty: !!dto.isDirty
        };
    }

    static parsePlayer(dto) {
        return {
            id: dto.id ?? 'player',
            position: dto.position ? { x: dto.position.x, y: dto.position.y, z: dto.position.z } : null,
            velocity: dto.velocity ? { x: dto.velocity.x, y: dto.velocity.y, z: dto.velocity.z } : null,
            state: dto.state ?? null,
            isGrounded: !!dto.isGrounded,
            health: typeof dto.health === 'number' ? dto.health : null
        };
    }

    static parseZombie(dto) {
        return {
            id: dto.id,
            type: dto.type,
            position: dto.position ? { x: dto.position.x, y: dto.position.y, z: dto.position.z } : null,
            state: dto.state,
            health: dto.health,
            targetPosition: dto.targetPosition ? { x: dto.targetPosition.x, y: dto.targetPosition.y, z: dto.targetPosition.z } : null
        };
    }

    static parseCamera(dto) {
        return {
            position: dto.position ? { x: dto.position.x, y: dto.position.y, z: dto.position.z } : null,
            target: dto.target ? { x: dto.target.x, y: dto.target.y, z: dto.target.z } : null
        };
    }
}

// Usage example (development):
// const client = new NetworkClient();
// client.onFrame(frame => console.log('frame', frame));
// client.connectWebSocket('ws://localhost:3000/ws');
// client.fetchFrame('/frame.json');
// client.startPolling('/frame.json', 1000);

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
        this._pollUrl = null;
        this._pollIntervalMs = 1000;
        this._reconnectTimeoutId = null;
        this._manualDisconnect = false;
    }

    async fetchFrame(url) {
        try {
            const res = await fetch(url, { cache: 'no-store' });
            if (!res.ok) {
                const err = `HTTP ${res.status} ${res.statusText}`;
                throw new Error(`fetchFrame failed: ${err}`);
            }

            const json = await res.json();
            const frame = NetworkClient.parseFrame(json);
            this._emitFrame(frame);
            return frame;
        } catch (err) {
            throw err;
        }
    }

    async connectWebSocket(url, options = {}) {
        if (typeof WebSocket === 'undefined') {
            throw new Error('WebSocket is not available in this environment');
        }

        if (!url) {
            throw new Error('Invalid WebSocket URL');
        }

        this._wsUrl = url;
        this._manualDisconnect = false;
        this._pollUrl = options.pollingUrl || this._pollUrl;
        this._pollIntervalMs = options.retryIntervalMs ?? this._pollIntervalMs;

        if (this._ws) {
            this.disconnectWebSocket({ stopReconnect: true });
        }

        return new Promise((resolve, reject) => {
            const socket = new WebSocket(url);
            this._ws = socket;
            let didResolve = false;

            const cleanup = () => {
                socket.removeEventListener('open', onOpen);
                socket.removeEventListener('message', onMessage);
                socket.removeEventListener('close', onClose);
                socket.removeEventListener('error', onError);
            };

            const onOpen = () => {
                cleanup();
                this._emitStatus('WebSocket connected');
                this._clearReconnect();
                this.stopPolling();
                didResolve = true;
                resolve();
            };

            const onMessage = event => {
                try {
                    const json = JSON.parse(event.data);
                    const frame = NetworkClient.parseFrame(json);
                    if (frame) {
                        this._emitFrame(frame);
                    }
                } catch (error) {
                    console.error('[NetworkClient] WebSocket parse error:', error);
                }
            };

            const onClose = event => {
                cleanup();
                if (this._ws === socket) {
                    this._ws = null;
                }
                this._emitStatus('WebSocket disconnected');
                if (!this._manualDisconnect) {
                    this._scheduleReconnect();
                    this.startPolling(this._pollUrl || this._wsUrl, this._pollIntervalMs);
                }
                if (!didResolve) {
                    reject(new Error('WebSocket connection closed')); 
                }
            };

            const onError = error => {
                this._emitStatus('WebSocket error');
                if (!didResolve && socket.readyState !== WebSocket.OPEN) {
                    cleanup();
                    reject(new Error('WebSocket connection failed'));
                }
            };

            socket.addEventListener('open', onOpen);
            socket.addEventListener('message', onMessage);
            socket.addEventListener('close', onClose);
            socket.addEventListener('error', onError);
        });
    }

    disconnectWebSocket({ stopReconnect = false } = {}) {
        if (!this._ws) return;
        this._manualDisconnect = stopReconnect;
        try {
            this._ws.close(1000, 'Client closing');
        } catch (err) {
            console.warn('[NetworkClient] Failed to close WebSocket cleanly', err);
        }
        this._ws = null;
        if (stopReconnect) {
            this._clearReconnect();
        }
    }

    isWebSocketConnected() {
        return this._ws && this._ws.readyState === WebSocket.OPEN;
    }

    startPolling(url, intervalMs = 1000) {
        if (!url) return;
        this._pollUrl = url;
        this._pollIntervalMs = intervalMs;

        if (this.isWebSocketConnected()) {
            return;
        }

        if (this._pollIntervalId) {
            return;
        }

        this._pollIntervalId = setInterval(() => {
            this.fetchFrame(this._pollUrl).catch(() => {
                // Silent retry on polling failure
            });
        }, this._pollIntervalMs);
    }

    stopPolling() {
        if (!this._pollIntervalId) return;
        clearInterval(this._pollIntervalId);
        this._pollIntervalId = null;
    }

    _scheduleReconnect() {
        if (this._reconnectTimeoutId || this._manualDisconnect || !this._wsUrl) {
            return;
        }

        this._reconnectTimeoutId = setTimeout(async () => {
            this._reconnectTimeoutId = null;
            try {
                await this.connectWebSocket(this._wsUrl);
                this._emitStatus('WebSocket reconnected');
            } catch (err) {
                this._scheduleReconnect();
            }
        }, this._pollIntervalMs);
    }

    _clearReconnect() {
        if (!this._reconnectTimeoutId) return;
        clearTimeout(this._reconnectTimeoutId);
        this._reconnectTimeoutId = null;
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
            rotation: dto.rotation ? {
                x: dto.rotation.x,
                y: dto.rotation.y,
                z: dto.rotation.z,
                w: dto.rotation.w,
                yaw: dto.rotation.yaw,
                pitch: dto.rotation.pitch,
                roll: dto.rotation.roll
            } : null,
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

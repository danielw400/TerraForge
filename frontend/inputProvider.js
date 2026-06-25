export class BrowserInputProvider {
    constructor(pointerLockElement = document.body) {
        this.pointerLockElement = pointerLockElement;
        this.commandBuffer = [];
        this.enabled = false;
        this.pointerLocked = false;
        this.pointerLockState = 'UNLOCKED';
        this.keyState = new Map();
        this._lastUnlockTimestamp = 0;

        this._onKeyDown = this._onKeyDown.bind(this);
        this._onKeyUp = this._onKeyUp.bind(this);
        this._onMouseMove = this._onMouseMove.bind(this);
        this._onPointerLockChange = this._onPointerLockChange.bind(this);
        this._onPointerLockError = this._onPointerLockError.bind(this);
        this._onWindowBlur = this._onWindowBlur.bind(this);
        this._onMouseDown = this._onMouseDown.bind(this);

        this.actionMap = {
            'w': 'moveForward',
            'arrowup': 'moveForward',
            's': 'moveBackward',
            'arrowdown': 'moveBackward',
            'a': 'moveLeft',
            'arrowleft': 'moveLeft',
            'd': 'moveRight',
            'arrowright': 'moveRight',
            'shift': 'run',
            ' ': 'jump'
        };
    }

    start() {
        if (this.enabled) return;
        this.enabled = true;

        window.addEventListener('keydown', this._onKeyDown, { passive: true });
        window.addEventListener('keyup', this._onKeyUp, { passive: true });
        window.addEventListener('blur', this._onWindowBlur, { passive: true });

        this.pointerLockElement.addEventListener('mousedown', this._onMouseDown, { passive: true });
        document.addEventListener('pointerlockchange', this._onPointerLockChange, false);
        document.addEventListener('pointerlockerror', this._onPointerLockError, false);
        document.addEventListener('mousemove', this._onMouseMove, { passive: true });
    }

    stop() {
        if (!this.enabled) return;
        this.enabled = false;

        window.removeEventListener('keydown', this._onKeyDown, { passive: true });
        window.removeEventListener('keyup', this._onKeyUp, { passive: true });
        window.removeEventListener('blur', this._onWindowBlur, { passive: true });

        this.pointerLockElement.removeEventListener('mousedown', this._onMouseDown, { passive: true });
        document.removeEventListener('pointerlockchange', this._onPointerLockChange, false);
        document.removeEventListener('pointerlockerror', this._onPointerLockError, false);
        document.removeEventListener('mousemove', this._onMouseMove, { passive: true });
        this._releaseAllKeys();
    }

    drainCommands() {
        const drained = this.commandBuffer.slice();
        this.commandBuffer.length = 0;
        return drained;
    }

    _enqueueCommand(command) {
        if (!command || !command.action) return;
        this.commandBuffer.push({
            action: command.action,
            value: command.value,
            timestamp: command.timestamp ?? Date.now()
        });
    }

    _onKeyDown(event) {
        const key = event.key.toLowerCase();
        const action = this.actionMap[key];
        if (!action) return;
        if (action === 'jump') {
            this._enqueueCommand({ action, value: true, timestamp: Date.now() });
            return;
        }

        if (this.keyState.get(action)) {
            return;
        }

        this.keyState.set(action, true);
        this._enqueueCommand({ action, value: true, timestamp: Date.now() });
    }

    _onKeyUp(event) {
        const key = event.key.toLowerCase();
        const action = this.actionMap[key];
        if (!action || action === 'jump') return;

        if (!this.keyState.get(action)) {
            return;
        }

        this.keyState.set(action, false);
        this._enqueueCommand({ action, value: false, timestamp: Date.now() });
    }

    async requestPointerLock() {
        if (this.pointerLockState !== 'UNLOCKED') {
            return false;
        }

        const now = Date.now();
        if (now - this._lastUnlockTimestamp < 250) {
            console.warn('[Input] Pointer Lock request blocked because lock was just released');
            return false;
        }

        if (!this.pointerLockElement || !this.pointerLockElement.requestPointerLock) {
            console.warn('[Input] Pointer Lock unavailable');
            return false;
        }

        this.pointerLockState = 'REQUESTING';
        console.log('[Input] Pointer Lock requested');

        try {
            await this.pointerLockElement.requestPointerLock();
            return true;
        } catch (error) {
            this.pointerLockState = 'UNLOCKED';
            console.warn('[Input] Pointer Lock blocked:', error);
            console.warn('[Input] Pointer Lock denied');
            return false;
        }
    }

    _onMouseDown() {
        if (this.pointerLockState !== 'UNLOCKED') {
            return;
        }

        this.requestPointerLock();
    }

    _onPointerLockChange() {
        const locked = document.pointerLockElement === this.pointerLockElement;
        const previousState = this.pointerLockState;

        this.pointerLocked = locked;
        this.pointerLockState = locked ? 'LOCKED' : 'UNLOCKED';

        if (locked) {
            console.log('[Input] Pointer Lock acquired');
        } else if (previousState === 'REQUESTING') {
            console.warn('[Input] Pointer Lock denied');
            this._lastUnlockTimestamp = Date.now();
        } else if (previousState === 'LOCKED') {
            console.log('[Input] Pointer Lock released');
            this._lastUnlockTimestamp = Date.now();
        }
    }

    _onPointerLockError() {
        this.pointerLockState = 'UNLOCKED';
        this.pointerLocked = false;
        console.warn('[Input] Pointer Lock denied');
    }

    _onMouseMove(event) {
        if (!this.enabled || !this.pointerLocked) return;
        const dx = event.movementX || 0;
        const dy = event.movementY || 0;
        if (dx === 0 && dy === 0) return;

        this._enqueueCommand({
            action: 'look',
            value: { dx, dy },
            timestamp: Date.now()
        });
    }

    _onWindowBlur() {
        this._releaseAllKeys();
    }

    _releaseAllKeys() {
        for (const [action, pressed] of this.keyState.entries()) {
            if (pressed) {
                this.keyState.set(action, false);
                this._enqueueCommand({ action, value: false, timestamp: Date.now() });
            }
        }
    }
}

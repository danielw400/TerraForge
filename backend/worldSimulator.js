/**
 * WorldSimulator
 * Simulates RenderingBridge behavior by generating snapshots of world state
 */
export class WorldSimulator {
    constructor() {
        this.time = 0;
        this.loadedChunks = new Set();
        this.playerHealth = 100.0;
        this.zombieCounter = 0;
        this.playerPosition = { x: 2.0, y: 1.5, z: 2.0 };
        this.playerVelocity = { x: 0.0, y: 0.0, z: 0.0 };
        this.playerRotation = { yaw: 0.0, pitch: 0.0 };
        this.playerIsGrounded = true;
        this.playerState = 'Idle';
        this.inputState = {
            moveForward: false,
            moveBackward: false,
            moveLeft: false,
            moveRight: false,
            run: false,
        };
        this.gravity = -24.0;
        this.jumpSpeed = 8.0;
        this.moveSpeed = 3.5;
        this.runMultiplier = 1.8;
    }

    applyInputCommands(commands) {
        if (!Array.isArray(commands)) return;

        for (const command of commands) {
            if (!command || typeof command.action !== 'string') continue;
            const action = command.action;
            const value = command.value;

            switch (action) {
                case 'moveForward':
                case 'moveBackward':
                case 'moveLeft':
                case 'moveRight':
                case 'run':
                    this.inputState[action] = !!value;
                    break;
                case 'jump':
                    if (value && this.playerIsGrounded) {
                        this.playerVelocity.y = this.jumpSpeed;
                        this.playerIsGrounded = false;
                        this.playerState = 'Jumping';
                    }
                    break;
                case 'look':
                    if (value && typeof value.dx === 'number' && typeof value.dy === 'number') {
                        const sensitivity = 0.0025;
                        this.playerRotation.yaw += value.dx * sensitivity;
                        this.playerRotation.pitch = Math.max(-Math.PI / 2.1, Math.min(Math.PI / 2.1, this.playerRotation.pitch + value.dy * sensitivity));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    _applyPlayerMovement(deltaTime) {
        const forward = this.inputState.moveForward ? 1 : 0;
        const backward = this.inputState.moveBackward ? 1 : 0;
        const left = this.inputState.moveLeft ? 1 : 0;
        const right = this.inputState.moveRight ? 1 : 0;
        const run = this.inputState.run ? this.runMultiplier : 1.0;

        const moveX = right - left;
        const moveZ = forward - backward;
        const speed = this.moveSpeed * run;

        if (moveX !== 0 || moveZ !== 0) {
            const angle = this.playerRotation.yaw;
            const sin = Math.sin(angle);
            const cos = Math.cos(angle);
            const worldX = (moveX * cos - moveZ * sin) * speed * deltaTime;
            const worldZ = (moveZ * cos + moveX * sin) * speed * deltaTime;
            this.playerPosition.x += worldX;
            this.playerPosition.z += worldZ;
            this.playerState = 'Running';
        } else if (this.playerIsGrounded) {
            this.playerState = 'Idle';
        }

        const gravityStep = this.gravity * deltaTime;
        this.playerVelocity.y += gravityStep;
        this.playerPosition.y += this.playerVelocity.y * deltaTime;

        if (this.playerPosition.y <= 1.5) {
            this.playerPosition.y = 1.5;
            this.playerVelocity.y = 0;
            this.playerIsGrounded = true;
            if (this.playerState === 'Jumping') {
                this.playerState = 'Idle';
            }
        }
    }

    generateFrame() {
        this.time += 1;
        const deltaTime = 0.25;
        this._applyPlayerMovement(deltaTime);

        const chunks = [
            this.generateChunk(0, 0, 0),
        ];
        const chunkUnloads = [];
        if (this.time % 100 < 50) {
            chunks.push(this.generateChunk(1, 0, 0));
        } else {
            chunkUnloads.push({ coord: { x: 1, y: 0, z: 0 } });
        }

        const player = {
            id: 'player',
            position: {
                x: this.playerPosition.x,
                y: this.playerPosition.y,
                z: this.playerPosition.z,
            },
            rotation: {
                yaw: this.playerRotation.yaw,
                pitch: this.playerRotation.pitch,
            },
            velocity: {
                x: this.playerVelocity.x,
                y: this.playerVelocity.y,
                z: this.playerVelocity.z,
            },
            state: this.playerState,
            isGrounded: this.playerIsGrounded,
            health: this.playerHealth,
        };

        // Generate 2-3 zombies orbiting the player
        const zombies = [];
        const zCount = 2;
        for (let i = 0; i < zCount; i++) {
            const angle = (i / zCount) * Math.PI * 2 + this.time * 0.02;
            const dist = 18.0;
            zombies.push({
                id: `z-${i}`,
                type: 'InfectadoComum',
                position: {
                    x: player.position.x + Math.cos(angle) * dist,
                    y: 1.0,
                    z: player.position.z + Math.sin(angle) * dist,
                },
                state: i % 2 === 0 ? 'Wandering' : 'Alert',
                health: 50.0 + i * 25,
                targetPosition: player.position,
            });
        }

        const camera = {
            position: {
                x: player.position.x + 10.0,
                y: 10.0,
                z: player.position.z + 30.0,
            },
            target: player.position,
        };

        return {
            chunks,
            chunkUnloads,
            player,
            zombies,
            camera,
        };
    }

    generateInitialSnapshot() {
        // Similar to generateFrame but always same state
        this.time = 0;
        return this.generateFrame();
    }

    generateChunk(cx, cy, cz) {
        // Generate a 4x4x4 chunk (64 voxels for simplicity, will scale to 16^3 = 4096 in real integration)
        // For now use 16^3 = 4096 blocks with sparse data
        const blockCount = 16 * 16 * 16; // 4096
        const blocks = new Array(blockCount).fill(0);

        // Fill bottom layer with stone/dirt/grass
        const side = 16;
        for (let x = 0; x < side; x++) {
            for (let z = 0; z < side; z++) {
                const idx = 0 * side * side + x * side + z; // y=0
                blocks[idx] = 2; // dirt
            }
        }

        // Add some variation
        if (cx === 0 && cy === 0 && cz === 0) {
            blocks[0] = 2; // stone at corner
            blocks[1] = 1; // more stone
            blocks[100] = 3; // grass in middle
        }

        if (cx === 1 && cy === 0 && cz === 0) {
            blocks[50] = 4; // wood blocks
            blocks[51] = 4;
            blocks[150] = 4;
        }

        return {
            coord: { x: cx, y: cy, z: cz },
            blocks: blocks,
            version: 1,
            isDirty: false,
        };
    }
}

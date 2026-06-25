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
    }

    generateFrame() {
        this.time += 1;
        
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
                x: 2.0 + Math.sin(this.time * 0.01) * 0.5,
                y: 1.5,
                z: 2.0 + Math.cos(this.time * 0.01) * 0.5,
            },
            velocity: { x: 0.0, y: 0.0, z: 0.0 },
            state: 'Idle',
            isGrounded: true,
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

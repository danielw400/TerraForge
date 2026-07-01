import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';

// ChunkRenderer: converte ChunkDto em geometria Three.js (abordagem simples)
// - Mantém um Group por chunk
// - Para cada voxel não-ar, cria um Mesh de cubo (padrão)
// - Estrutura preparada para trocar por InstancedMesh ou greedy meshing

export class ChunkRenderer {
    constructor(scene, options = {}) {
        this.scene = scene;
        this.chunkSize = options.chunkSize || 16;
        this.blockScale = options.blockScale || 1.0;
        this.chunks = new Map(); // key -> { coord, allocations, version }
        this.materialCache = new Map();
        this.geometryCache = new Map(); // cache geometries like box
        this.pools = new Map(); // blockId -> pool object for global InstancedMesh reuse
        this.tmpMat4 = new THREE.Matrix4();
    }

    // aplica um frame inteiro: adiciona/atualiza e remove chunks
    applyFrame(frame) {
        if (!frame) return;

        // remove unloaded
        if (Array.isArray(frame.chunkUnloads)) {
            for (const u of frame.chunkUnloads) {
                this.removeChunk(u.coord);
            }
        }

        // add/update chunks
        if (Array.isArray(frame.chunks)) {
            for (const c of frame.chunks) {
                this.addOrUpdateChunk(c);
            }
        }
    }

    // converte coord object to key
    _coordKey(coord) {
        return `${coord.x},${coord.y},${coord.z}`;
    }

    addOrUpdateChunk(chunkDto) {
        const key = this._coordKey(chunkDto.coord);
        const existing = this.chunks.get(key);
        const chunkVersion = chunkDto.version ?? 0;
        const chunkDirty = !!chunkDto.isDirty;

        if (existing && existing.version === chunkVersion && !chunkDirty) {
            return;
        }

        if (existing) {
            this.removeChunk(chunkDto.coord);
        }

        const blocks = chunkDto.blocks || [];
        const side = this._inferSideLength(blocks.length);
        const halfScale = this.blockScale * 0.5;

        const positionsById = new Map();
        let index = 0;
        for (let x = 0; x < side; x++) {
            for (let y = 0; y < side; y++) {
                for (let z = 0; z < side; z++) {
                    const blockId = blocks[index++] ?? 0;
                    if (!blockId) continue;
                    const worldX = chunkDto.coord.x * this.chunkSize + x;
                    const worldY = chunkDto.coord.y * this.chunkSize + y;
                    const worldZ = chunkDto.coord.z * this.chunkSize + z;
                    const bucket = positionsById.get(blockId) || { coords: [] };
                    bucket.coords.push(
                        worldX * this.blockScale + halfScale,
                        worldY * this.blockScale + halfScale,
                        worldZ * this.blockScale + halfScale
                    );
                    positionsById.set(blockId, bucket);
                }
            }
        }

        const chunkAllocations = [];
        for (const [blockId, bucket] of positionsById.entries()) {
            const coords = bucket.coords;
            const pool = this._ensurePool(blockId);
            const alloc = this._allocateSlots(pool, coords.length / 3, key);
            for (let i = 0; i < alloc.length; i++) {
                const slot = alloc[i];
                const px = coords[i * 3];
                const py = coords[i * 3 + 1];
                const pz = coords[i * 3 + 2];
                this.tmpMat4.makeTranslation(px, py, pz);
                slot.mesh.setMatrixAt(slot.index, this.tmpMat4);
                slot.mesh.instanceMatrix.needsUpdate = true;
                chunkAllocations.push({ blockId, meshIndex: slot.meshIndex, index: slot.index });
            }
        }

        this.chunks.set(key, { coord: chunkDto.coord, allocations: chunkAllocations, version: chunkDto.version ?? 0 });
    }

    // ensure a pool exists for a given blockId
    _ensurePool(blockId) {
        if (this.pools.has(blockId)) return this.pools.get(blockId);
        const geoKey = `box_${this.blockScale}`;
        let boxGeo = this.geometryCache.get(geoKey);
        if (!boxGeo) {
            boxGeo = new THREE.BoxGeometry(this.blockScale, this.blockScale, this.blockScale);
            this.geometryCache.set(geoKey, boxGeo);
        }
        const color = ChunkRenderer.blockColor(blockId);
        const material = this._getOrCreateMaterial(color);

        const initialCapacity = Math.max(256, this.chunkSize * this.chunkSize * this.chunkSize); // start big enough for a chunk
        const mesh = new THREE.InstancedMesh(boxGeo, material, initialCapacity);
        mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage);
        mesh.castShadow = false;
        mesh.receiveShadow = true;
        mesh.name = `pool-block-${blockId}-0`;
        this.scene.add(mesh);

        const pool = {
            blockId,
            geometry: boxGeo,
            material,
            meshes: [mesh],
            capacities: [initialCapacity],
            usedCounts: [0],
            ownerMaps: [new Map()] // per mesh index -> map(index->ownerChunkKey)
        };
        this.pools.set(blockId, pool);
        return pool;
    }

    // allocate `count` dense slots from pool, returning array of {mesh, meshIndex, index}
    _allocateSlots(pool, count, chunkKey) {
        const result = [];
        for (let attempt = 0; attempt < count; attempt++) {
            // try find a mesh with space
            let found = false;
            for (let m = 0; m < pool.meshes.length; m++) {
                const used = pool.usedCounts[m];
                const cap = pool.capacities[m];
                if (used < cap) {
                    const idx = used;
                    pool.usedCounts[m] = used + 1;
                    pool.ownerMaps[m].set(idx, chunkKey);
                    // ensure mesh.count reflects active instances
                    pool.meshes[m].count = pool.usedCounts[m];
                    result.push({ mesh: pool.meshes[m], meshIndex: m, index: idx });
                    found = true;
                    break;
                }
            }
            if (!found) {
                // expand pool by creating another mesh with same capacity doubled
                const newCap = pool.capacities[pool.capacities.length - 1] * 2;
                const mesh = new THREE.InstancedMesh(pool.geometry, pool.material, newCap);
                mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage);
                mesh.castShadow = false;
                mesh.receiveShadow = true;
                mesh.name = `pool-block-${pool.blockId}-${pool.meshes.length}`;
                this.scene.add(mesh);
                pool.meshes.push(mesh);
                pool.capacities.push(newCap);
                pool.usedCounts.push(0);
                pool.ownerMaps.push(new Map());
                // allocate from new mesh
                const m = pool.meshes.length - 1;
                const idx = 0;
                pool.usedCounts[m] = 1;
                pool.ownerMaps[m].set(0, chunkKey);
                pool.meshes[m].count = pool.usedCounts[m];
                result.push({ mesh: pool.meshes[m], meshIndex: m, index: idx });
            }
        }
        return result;
    }

    // free a previously allocated slot; uses dense swap with last used index
    _freeSlot(pool, meshIndex, index) {
        const meshes = pool.meshes;
        const ownerMaps = pool.ownerMaps;
        let used = pool.usedCounts[meshIndex];
        if (used === 0) return;
        const lastIndex = used - 1;
        const mesh = meshes[meshIndex];
        const ownerMap = ownerMaps[meshIndex];

        if (index !== lastIndex) {
            // move last instance into freed slot
            const tmpMat = new THREE.Matrix4();
            mesh.getMatrixAt(lastIndex, tmpMat);
            mesh.setMatrixAt(index, tmpMat);
            // update owner map for moved instance
            const movedOwnerChunkKey = ownerMap.get(lastIndex);
            ownerMap.set(index, movedOwnerChunkKey);
            ownerMap.delete(lastIndex);

            // update the chunk record that owned moved instance to point to new index
            // find chunk record and update its allocations
            const movedChunkKey = movedOwnerChunkKey;
            if (movedChunkKey) {
                const rec = this.chunks.get(movedChunkKey);
                if (rec && rec.allocations) {
                    for (const a of rec.allocations) {
                        if (a.meshIndex === meshIndex && a.index === lastIndex) {
                            a.index = index;
                            break;
                        }
                    }
                }
            }
        } else {
            ownerMap.delete(lastIndex);
        }

        pool.usedCounts[meshIndex] = used - 1;
        mesh.count = pool.usedCounts[meshIndex];
        mesh.instanceMatrix.needsUpdate = true;
    }

    removeChunk(coord) {
        const key = this._coordKey(coord);
        const existing = this.chunks.get(key);
        if (!existing) return;
        // free allocations back to pools
        if (Array.isArray(existing.allocations)) {
            for (const a of existing.allocations) {
                const pool = this.pools.get(a.blockId);
                if (!pool) continue;
                this._freeSlot(pool, a.meshIndex, a.index);
            }
        }
        this.chunks.delete(key);
    }

    clear() {
        // free all chunk allocations
        for (const [key, item] of Array.from(this.chunks.entries())) {
            if (item && Array.isArray(item.allocations)) {
                for (const a of item.allocations) {
                    const pool = this.pools.get(a.blockId);
                    if (pool) this._freeSlot(pool, a.meshIndex, a.index);
                }
            }
        }
        this.chunks.clear();

        // dispose pools and remove pooled meshes from scene
        for (const [blockId, pool] of this.pools.entries()) {
            for (const mesh of pool.meshes) {
                this.scene.remove(mesh);
                if (mesh.geometry && mesh.geometry.dispose) mesh.geometry.dispose();
                if (mesh.material) {
                    if (Array.isArray(mesh.material)) mesh.material.forEach(m => m && m.dispose && m.dispose());
                    else if (mesh.material.dispose) mesh.material.dispose();
                }
            }
        }
        this.pools.clear();
    }

    // cria um Mesh simples para o blockId
    _createBlockMesh(blockId) {
        const color = ChunkRenderer.blockColor(blockId);
        const material = this._getOrCreateMaterial(color);
        const geometry = new THREE.BoxGeometry(this.blockScale, this.blockScale, this.blockScale);
        const mesh = new THREE.Mesh(geometry, material);
        mesh.castShadow = false;
        mesh.receiveShadow = true;
        return mesh;
    }

    _getOrCreateMaterial(color) {
        const key = color.toString();
        if (this.materialCache.has(key)) return this.materialCache.get(key);
        const mat = new THREE.MeshStandardMaterial({ color });
        this.materialCache.set(key, mat);
        return mat;
    }

    // tenta inferir lado do chunk a partir do comprimento do array (fallback para chunkSize)
    _inferSideLength(length) {
        if (!length || length === 0) return 0;
        const side = Math.round(Math.cbrt(length));
        if (side * side * side === length) return side;
        return this.chunkSize;
    }

    static blockColor(id) {
        // mapeamento simples e extensível por id
        switch (id) {
            case 1: return 0x8b8b8b; // stone
            case 2: return 0x6e4b2c; // dirt
            case 3: return 0x2e8b57; // grass
            case 4: return 0x8b4513; // wood
            case 5: return 0x7fb3d5; // glass-ish
            case 6: return 0xc2b280; // sand
            case 7: return 0x2f6f6f; // swamp water
            case 10: return 0x2a5cff; // water
            default: return 0xffffff * (id % 2 === 0 ? 0.8 : 0.6); // fallback
        }
    }
}

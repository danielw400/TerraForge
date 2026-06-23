import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';

export class EntityRenderer {
    constructor(scene, options = {}) {
        this.scene = scene;
        this.entities = new Map();
        this.geometryCache = new Map();
        this.materialCache = new Map();
        this.debug = options.debug ?? true;
        this.defaultScale = options.defaultScale || 1.0;
    }

    updateZombies(zombies = []) {
        const seenIds = new Set();

        if (!Array.isArray(zombies)) zombies = [];

        for (const zombie of zombies) {
            if (!zombie || !zombie.id) continue;
            const id = zombie.id;
            seenIds.add(id);

            if (!this.entities.has(id)) {
                this._createZombieEntity(id, zombie);
            }

            const entity = this.entities.get(id);
            this._updateZombieEntity(entity, zombie);
        }

        // remove zombies that disappeared from the frame
        for (const existingId of Array.from(this.entities.keys())) {
            if (!seenIds.has(existingId)) {
                this._removeZombieEntity(existingId);
            }
        }
    }

    _createZombieEntity(id, zombie) {
        const container = new THREE.Object3D();
        container.name = `zombie-${id}`;

        const mesh = this._createZombieMesh(zombie);
        container.add(mesh);

        this.scene.add(container);

        const entity = {
            id,
            container,
            mesh,
            state: zombie.state || 'Unknown',
            lastPosition: null,
        };
        this.entities.set(id, entity);

        if (this.debug) console.log(`[EntityRenderer] Created zombie entity: ${id}`);
        return entity;
    }

    _createZombieMesh(zombie) {
        const geometry = this._getGeometry('zombieSphere', () => new THREE.SphereGeometry(0.45, 12, 12));
        const material = this._getMaterialForState(zombie.state);
        const mesh = new THREE.Mesh(geometry, material);
        mesh.castShadow = true;
        mesh.receiveShadow = true;
        mesh.scale.setScalar(this.defaultScale);
        return mesh;
    }

    _getGeometry(key, factory) {
        if (this.geometryCache.has(key)) {
            return this.geometryCache.get(key);
        }
        const geometry = factory();
        this.geometryCache.set(key, geometry);
        return geometry;
    }

    _getMaterialForState(state) {
        const key = `zombie-${state || 'Unknown'}`;
        if (this.materialCache.has(key)) {
            return this.materialCache.get(key);
        }

        const color = EntityRenderer.getStateColor(state);
        const material = new THREE.MeshStandardMaterial({ color });
        this.materialCache.set(key, material);
        return material;
    }

    _updateZombieEntity(entity, zombie) {
        const position = zombie.position;
        if (position) {
            entity.container.position.set(position.x, position.y, position.z);
        }

        if (zombie.targetPosition) {
            // face toward the target position if available
            const target = new THREE.Vector3(zombie.targetPosition.x, zombie.targetPosition.y, zombie.targetPosition.z);
            entity.container.lookAt(target);
        } else if (entity.lastPosition) {
            const current = entity.container.position;
            const delta = new THREE.Vector3().subVectors(current, entity.lastPosition);
            if (delta.lengthSq() > 0.0001) {
                const lookTarget = current.clone().add(delta);
                entity.container.lookAt(lookTarget);
            }
        }

        if (zombie.state && zombie.state !== entity.state) {
            entity.state = zombie.state;
            entity.mesh.material = this._getMaterialForState(zombie.state);
            if (this.debug) console.log(`[EntityRenderer] Zombie ${entity.id} changed state to ${entity.state}`);
        }

        if (this.debug) {
            console.log(`[EntityRenderer] Updated zombie ${entity.id}: pos=${position ? `${position.x.toFixed(2)},${position.y.toFixed(2)},${position.z.toFixed(2)}` : 'n/a'}, state=${entity.state}`);
        }

        entity.lastPosition = entity.container.position.clone();
    }

    _removeZombieEntity(id) {
        const entity = this.entities.get(id);
        if (!entity) return;
        this.scene.remove(entity.container);
        this.entities.delete(id);
        if (this.debug) console.log(`[EntityRenderer] Removed zombie entity: ${id}`);
    }

    dispose() {
        for (const entity of this.entities.values()) {
            this.scene.remove(entity.container);
        }
        this.entities.clear();
    }

    static getStateColor(state) {
        switch ((state || '').toLowerCase()) {
            case 'wandering': return 0xff3333;
            case 'alert': return 0xffa500;
            case 'chasing': return 0xff0000;
            default: return 0xcc0000;
        }
    }
}

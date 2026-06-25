import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';

export class EntityRenderer {
    constructor(scene, options = {}) {
        this.scene = scene;
        this.entities = new Map();
        this.playerEntity = null;
        this.geometryCache = new Map();
        this.materialCache = new Map();
        this.debug = options.debug ?? true;
        this.defaultScale = options.defaultScale || 1.0;
        this.tmpTarget = new THREE.Vector3();
        this.tmpDelta = new THREE.Vector3();
        this.tmpLookAt = new THREE.Vector3();
    }

    updatePlayer(player) {
        if (!player || !player.position) {
            if (this.playerEntity) {
                this.playerEntity.mesh.visible = false;
            }
            return;
        }

        if (!this.playerEntity) {
            this._createPlayerEntity(player);
        }

        const entity = this.playerEntity;
        const pos = player.position;
        entity.mesh.visible = true;
        entity.container.position.set(pos.x, pos.y, pos.z);

        if (player.rotation) {
            if ('w' in player.rotation && 'x' in player.rotation && 'y' in player.rotation && 'z' in player.rotation) {
                entity.container.quaternion.set(
                    player.rotation.x,
                    player.rotation.y,
                    player.rotation.z,
                    player.rotation.w
                );
            } else if ('yaw' in player.rotation || 'pitch' in player.rotation || 'roll' in player.rotation) {
                entity.container.rotation.set(
                    player.rotation.pitch || 0,
                    player.rotation.yaw || 0,
                    player.rotation.roll || 0
                );
            }
        }
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
            lastPosition: new THREE.Vector3(),
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

    _createPlayerEntity(player) {
        const container = new THREE.Object3D();
        container.name = 'player';

        const geometry = this._getGeometry('playerSphere', () => new THREE.SphereGeometry(0.5, 16, 16));
        const material = this._getMaterialForState('player');
        const mesh = new THREE.Mesh(geometry, material);
        mesh.castShadow = true;
        mesh.receiveShadow = true;
        mesh.scale.setScalar(this.defaultScale);
        mesh.visible = false;

        container.add(mesh);
        this.scene.add(container);

        this.playerEntity = {
            container,
            mesh,
            state: 'player'
        };
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
        const key = `${state || 'default'}`;
        if (this.materialCache.has(key)) {
            return this.materialCache.get(key);
        }

        const color = state === 'player' ? 0x2233ff : EntityRenderer.getStateColor(state);
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
            this.tmpTarget.set(zombie.targetPosition.x, zombie.targetPosition.y, zombie.targetPosition.z);
            entity.container.lookAt(this.tmpTarget);
        } else if (entity.lastPosition) {
            const current = entity.container.position;
            this.tmpDelta.subVectors(current, entity.lastPosition);
            if (this.tmpDelta.lengthSq() > 0.0001) {
                this.tmpLookAt.copy(current).add(this.tmpDelta);
                entity.container.lookAt(this.tmpLookAt);
            }
        }

        if (zombie.state && zombie.state !== entity.state) {
            entity.state = zombie.state;
            entity.mesh.material = this._getMaterialForState(zombie.state);
            if (this.debug) console.log(`[EntityRenderer] Zombie ${entity.id} changed state to ${entity.state}`);
        }

        entity.lastPosition.copy(entity.container.position);
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

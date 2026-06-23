import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.162.0/build/three.module.js';

/**
 * CameraController
 * Manages camera positioning and orientation
 * - Follows player in third-person if no explicit CameraStateDto
 * - Uses CameraStateDto if provided (higher priority)
 */
export class CameraController {
    constructor(camera, options = {}) {
        this.camera = camera;
        
        // Third-person follow params
        this.thirdPersonDistance = options.thirdPersonDistance || 8.0;
        this.thirdPersonHeight = options.thirdPersonHeight || 6.0;
        this.lookAheadDistance = options.lookAheadDistance || 2.0;
        
        // Smoothing
        this.smoothFactor = options.smoothFactor || 0.1;
        this.targetPos = new THREE.Vector3();
        this.targetLookAt = new THREE.Vector3();
        
        this.debug = options.debug !== false; // enable debug logs
    }

    /**
     * Update camera based on player and optional camera state DTO
     */
    update(playerPos, cameraState = null) {
        if (!playerPos) return;

        if (cameraState && cameraState.position) {
            // Use explicit camera state (highest priority)
            this._updateFromCameraState(cameraState);
            if (this.debug) console.log('[CameraController] Using explicit CameraStateDto');
        } else {
            // Follow player in third-person
            this._followPlayerThirdPerson(playerPos);
        }
    }

    _updateFromCameraState(cameraState) {
        // Set camera position directly from DTO
        if (cameraState.position) {
            this.targetPos.set(
                cameraState.position.x,
                cameraState.position.y,
                cameraState.position.z
            );
            if (this.debug) {
                console.log(`[CameraController] Explicit position: ${this.targetPos.toArray()}`);
            }
        }

        // Set look-at target
        if (cameraState.target) {
            this.targetLookAt.set(
                cameraState.target.x,
                cameraState.target.y,
                cameraState.target.z
            );
            if (this.debug) {
                console.log(`[CameraController] Explicit target: ${this.targetLookAt.toArray()}`);
            }
        }

        // Apply with smoothing
        this._applySmoothedTransform();
    }

    _followPlayerThirdPerson(playerPos) {
        // Position camera behind and above the player
        // Assume player is looking along some forward direction (default: +Z)
        
        const playerVec = new THREE.Vector3(playerPos.x, playerPos.y, playerPos.z);
        
        // Back direction (opposite of player forward, default -Z)
        const backDir = new THREE.Vector3(0, 0, -1); // looking towards +Z, so camera goes to -Z
        
        // Camera position: behind and above player
        this.targetPos.copy(playerVec);
        this.targetPos.add(backDir.multiplyScalar(this.thirdPersonDistance));
        this.targetPos.y += this.thirdPersonHeight;

        // Look-at point: slightly ahead of player
        this.targetLookAt.copy(playerVec);
        this.targetLookAt.add(new THREE.Vector3(0, 0, this.lookAheadDistance));
        
        if (this.debug) {
            console.log(`[CameraController] Third-person follow: pos=[${this.targetPos.toArray()}], target=[${this.targetLookAt.toArray()}]`);
        }

        // Apply with smoothing
        this._applySmoothedTransform();
    }

    _applySmoothedTransform() {
        // Smooth camera movement
        this.camera.position.lerp(this.targetPos, this.smoothFactor);
        
        // Look at target
        this.camera.lookAt(this.targetLookAt);
    }

    // Optional: disable debug logs
    setDebug(enabled) {
        this.debug = enabled;
    }

    // Optional: adjust third-person params
    setThirdPersonParams(distance, height) {
        this.thirdPersonDistance = distance;
        this.thirdPersonHeight = height;
    }
}

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
        this.playerVec = new THREE.Vector3();
        this.backDir = new THREE.Vector3(0, 0, -1);
        this.lookAheadDir = new THREE.Vector3(0, 0, 1);

        this.debug = options.debug !== false; // enable debug logs
    }

    /**
     * Update camera based on player and optional camera state DTO
     */
    update(playerPos, cameraState = null) {
        if (cameraState && cameraState.position) {
            // Use explicit camera state (highest priority)
            this._updateFromCameraState(cameraState);
            if (this.debug) console.log('[CameraController] Using explicit CameraStateDto');
            return;
        }

        if (playerPos) {
            // Follow player in third-person when no explicit camera state exists
            this._followPlayerThirdPerson(playerPos);
        }
    }

    _updateFromCameraState(cameraState) {
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

        this._applySmoothedTransform();
    }

    _followPlayerThirdPerson(playerPos) {
        this.playerVec.set(playerPos.x, playerPos.y, playerPos.z);

        this.targetPos.copy(this.playerVec);
        this.targetPos.addScaledVector(this.backDir, this.thirdPersonDistance);
        this.targetPos.y += this.thirdPersonHeight;

        this.targetLookAt.copy(this.playerVec);
        this.targetLookAt.addScaledVector(this.lookAheadDir, this.lookAheadDistance);

        if (this.debug) {
            console.log(`[CameraController] Third-person follow: pos=[${this.targetPos.toArray()}], target=[${this.targetLookAt.toArray()}]`);
        }

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

using System;
using TerraForge.Core;

namespace TerraForge.Game
{
    public sealed class PlayerController
    {
        private float _airbornePeakY;
        private bool _wasGroundedLastFrame;
        private readonly float _fallThreshold = 3.0f; // meters before damage
        private readonly float _fallDamageMultiplier = 8.0f; // damage per meter over threshold
        private readonly PlayerStats _stats;
        private readonly float _walkSpeed = 4.5f;
        private readonly float _runSpeed = 8.5f;
        private readonly float _crouchSpeed = 2.5f;
        private readonly float _swimSpeed = 3.5f;
        private readonly float _jumpForce = 6.5f;
        private readonly float _climbSpeed = 3.2f;
        private readonly float _gravity = -24f;

        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public PlayerState CurrentState { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsInWater { get; private set; }
        public PlayerStats Stats => _stats;

        public PlayerController(PlayerStats stats, Vector3 startPosition)
        {
            _stats = stats;
            Position = startPosition;
            Velocity = Vector3.Zero;
            CurrentState = PlayerState.Idle;
            _airbornePeakY = startPosition.Y;
            _wasGroundedLastFrame = true;
        }

        public float MaxSlopeAngleDegrees { get; set; } = 45f;
        public float SlideSpeed { get; set; } = 4.5f;

        public void Update(float deltaTime, PlayerInput input, Func<Vector3, Vector3, VoxelPhysics.SweptAabbResult> sweepCollision, Func<Vector3, bool> isClimbable, Func<Vector3, bool> isWater, Func<Vector3, (Vector3 normal, float angle)> getGroundInfo)
        {
            UpdateState(input);
            var moveDirection = GetMoveDirection(input);
            var speed = GetMovementSpeed(input);

            if (isClimbable(Position) && input.IsClimbing)
            {
                CurrentState = PlayerState.Climbing;
                Velocity = new Vector3(0, input.Vertical * _climbSpeed, 0);
                Position += Velocity * deltaTime;
                IsGrounded = false;
            }
            else if (isWater(Position))
            {
                CurrentState = PlayerState.Swimming;
                IsInWater = true;
                Velocity = new Vector3(moveDirection.X * _swimSpeed, input.Vertical * _swimSpeed * 0.75f, moveDirection.Z * _swimSpeed);
                Position += Velocity * deltaTime;
                IsGrounded = false;
            }
            else
            {
                IsInWater = false;
                if (IsGrounded && input.Jump)
                {
                    Velocity = new Vector3(Velocity.X, _jumpForce, Velocity.Z);
                    CurrentState = PlayerState.Jumping;
                    IsGrounded = false;
                }

                var groundNormal = new Vector3(0, 1, 0);
                var slopeAngle = 0f;
                if (getGroundInfo != null)
                {
                    var info = getGroundInfo(Position);
                    groundNormal = info.normal;
                    slopeAngle = info.angle;
                }

                var horizontalTarget = new Vector3(moveDirection.X * speed, 0, moveDirection.Z * speed);
                var intoNormal = new Vector3(Vector3.Dot(horizontalTarget, groundNormal) * groundNormal.X,
                                             Vector3.Dot(horizontalTarget, groundNormal) * groundNormal.Y,
                                             Vector3.Dot(horizontalTarget, groundNormal) * groundNormal.Z);
                var alongSlope = new Vector3(horizontalTarget.X - intoNormal.X,
                                             horizontalTarget.Y - intoNormal.Y,
                                             horizontalTarget.Z - intoNormal.Z);

                if (IsGrounded && slopeAngle > MaxSlopeAngleDegrees)
                {
                    CurrentState = PlayerState.Sliding;
                    var gravity = new Vector3(0, -1, 0);
                    var proj = new Vector3(gravity.X - Vector3.Dot(gravity, groundNormal) * groundNormal.X,
                                            gravity.Y - Vector3.Dot(gravity, groundNormal) * groundNormal.Y,
                                            gravity.Z - Vector3.Dot(gravity, groundNormal) * groundNormal.Z);
                    var slideDir = Vector3.Normalize(proj);
                    var slideVel = slideDir * (SlideSpeed * ((slopeAngle - MaxSlopeAngleDegrees) / (90f - MaxSlopeAngleDegrees) + 0.1f));
                    Velocity = new Vector3(slideVel.X, Velocity.Y + _gravity * deltaTime, slideVel.Z);
                }
                else
                {
                    Velocity = new Vector3(alongSlope.X, Velocity.Y + _gravity * deltaTime, alongSlope.Z);
                }

                var displacement = Velocity * deltaTime;
                var sweep = sweepCollision(Position, displacement);

                if (sweep.Hit)
                {
                    Position = sweep.Position;
                    if (sweep.Normal.Y > 0.65f)
                    {
                        if (!_wasGroundedLastFrame)
                        {
                            var fallDistance = _airbornePeakY - Position.Y;
                            if (fallDistance > _fallThreshold)
                            {
                                var damage = (fallDistance - _fallThreshold) * _fallDamageMultiplier;
                                _stats.AddHealth(-damage);
                            }
                        }

                        IsGrounded = true;
                        Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                        if (CurrentState == PlayerState.Falling || CurrentState == PlayerState.Jumping)
                        {
                            CurrentState = PlayerState.Idle;
                        }
                    }
                    else
                    {
                        IsGrounded = false;
                        if (Vector3.Dot(Velocity, sweep.Normal) < 0f)
                        {
                            var projection = Vector3.Dot(Velocity, sweep.Normal);
                            Velocity = new Vector3(Velocity.X - sweep.Normal.X * projection,
                                                   Velocity.Y - sweep.Normal.Y * projection,
                                                   Velocity.Z - sweep.Normal.Z * projection);
                        }
                    }

                    Position += sweep.Remainder;
                }
                else
                {
                    Position += displacement;
                    if (!_wasGroundedLastFrame)
                    {
                        if (Position.Y > _airbornePeakY) _airbornePeakY = Position.Y;
                    }

                    if (Velocity.Y < 0f)
                    {
                        CurrentState = PlayerState.Falling;
                    }

                    // keep grounded on small steps and gentle surfaces
                    IsGrounded = sweepCollision(Position, new Vector3(0, -0.05f, 0)).Hit;
                }
            }

            _stats.Update(deltaTime, input.IsRunning, input.IsCrouching, IsInWater);

            if (IsGrounded && !_wasGroundedLastFrame)
            {
                _airbornePeakY = Position.Y;
            }
            if (_wasGroundedLastFrame && !IsGrounded)
            {
                _airbornePeakY = Position.Y;
            }
            _wasGroundedLastFrame = IsGrounded;
        }

        private void UpdateState(PlayerInput input)
        {
            if (input.IsSwimming)
            {
                CurrentState = PlayerState.Swimming;
                return;
            }

            if (input.IsCrouching)
            {
                CurrentState = PlayerState.Crouching;
                return;
            }

            if (input.IsRunning)
            {
                CurrentState = PlayerState.Running;
                return;
            }

            if (input.Move != Vector3.Zero)
            {
                CurrentState = PlayerState.Walking;
                return;
            }

            CurrentState = PlayerState.Idle;
        }

        private float GetMovementSpeed(PlayerInput input)
        {
            if (IsInWater)
            {
                return _swimSpeed;
            }

            if (input.IsCrouching)
            {
                return _crouchSpeed;
            }

            return input.IsRunning ? _runSpeed : _walkSpeed;
        }

        private static Vector3 GetMoveDirection(PlayerInput input)
        {
            var direction = new Vector3(input.Move.X, 0, input.Move.Z);
            var length = MathF.Sqrt(direction.X * direction.X + direction.Z * direction.Z);
            if (length > 0f)
            {
                return new Vector3(direction.X / length, 0, direction.Z / length);
            }

            return Vector3.Zero;
        }
    }
}

using System;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class ZombieEntity
    {
        private readonly ZombieDefinition _definition;
        private float _attackCooldown;
        private float _investigationTimer;

        public ZombieType Type => _definition.Type;
        public ZombieState State { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Health { get; private set; }
        public float AwarenessRadius => _definition.SightRadius;
        public Vector3 LastKnownPlayerPosition { get; private set; }
        public Vector3 LastKnownTargetPosition { get; private set; }
        public Vector3 LastSoundPosition { get; private set; }
        public Vector3 LastLightPosition { get; private set; }
        public bool IsAlive => Health > 0f;
        public bool HasTarget => State == ZombieState.Chasing || State == ZombieState.Attacking || State == ZombieState.Investigating;

        public ZombieEntity(ZombieType type, Vector3 startPosition)
        {
            _definition = ZombieDefinition.Create(type);
            Position = startPosition;
            Velocity = Vector3.Zero;
            Health = _definition.MaxHealth;
            State = ZombieState.Idle;
            _attackCooldown = 0f;
            _investigationTimer = 0f;
            LastKnownPlayerPosition = startPosition;
            LastKnownTargetPosition = startPosition;
            LastSoundPosition = startPosition;
            LastLightPosition = startPosition;
        }

        public void Update(float deltaTime, ZombiePerception perception, IZombieNavigation navigation)
        {
            if (!IsAlive)
            {
                State = ZombieState.Dead;
                return;
            }

            _attackCooldown = MathF.Max(0f, _attackCooldown - deltaTime);
            if (State == ZombieState.Dead)
            {
                return;
            }

            if (perception.HasPlayerInSight)
            {
                LastKnownPlayerPosition = perception.PlayerPosition;
                LastKnownTargetPosition = perception.PlayerPosition;
                if (Vector3.Magnitude(perception.DirectionToPlayer) <= _definition.AttackRange)
                {
                    TransitionToState(ZombieState.Attacking);
                }
                else
                {
                    TransitionToState(ZombieState.Chasing);
                }
            }
            else if (perception.HasBaseTarget)
            {
                LastKnownTargetPosition = perception.BaseTarget.Position;
                var distanceToBase = Vector3.Magnitude(perception.BaseTarget.Position - Position);
                if (distanceToBase <= _definition.AttackRange)
                {
                    TransitionToState(ZombieState.Attacking);
                }
                else
                {
                    TransitionToState(ZombieState.Chasing);
                }
            }
            else if (perception.HasLoudSound && perception.SoundIntensity * _definition.SoundSensitivity >= 0.2f)
            {
                LastSoundPosition = perception.SoundPosition;
                LastKnownTargetPosition = perception.SoundPosition;
                TransitionToState(ZombieState.Investigating);
                _investigationTimer = _definition.InvestigationDurationSeconds;
            }
            else if (perception.HasBrightLight && !_definition.IgnoresLight)
            {
                LastLightPosition = perception.LightPosition;
                if (perception.LightIntensity >= _definition.LightAlertThreshold)
                {
                    LastKnownTargetPosition = perception.LightPosition;
                    TransitionToState(ZombieState.Investigating);
                    _investigationTimer = _definition.InvestigationDurationSeconds;
                }
                else if (perception.LightIntensity >= _definition.LightAvoidanceThreshold)
                {
                    TransitionToState(ZombieState.Fleeing);
                }
                else if (State == ZombieState.Idle || State == ZombieState.Wandering)
                {
                    LastKnownTargetPosition = perception.LightPosition;
                    TransitionToState(ZombieState.Investigating);
                    _investigationTimer = _definition.InvestigationDurationSeconds * 0.5f;
                }
            }
            else if (State == ZombieState.Investigating)
            {
                _investigationTimer -= deltaTime;
                if (_investigationTimer <= 0f)
                {
                    TransitionToState(ZombieState.Wandering);
                }
            }
            else if (State == ZombieState.Chasing || State == ZombieState.Attacking)
            {
                if (!perception.HasPlayerInSight && !perception.HasBaseTarget && _investigationTimer <= 0f)
                {
                    TransitionToState(ZombieState.Searching);
                    _investigationTimer = _definition.InvestigationDurationSeconds;
                }
            }
            else if (State == ZombieState.Idle)
            {
                TransitionToState(ZombieState.Wandering);
            }

            ExecuteState(deltaTime, perception, navigation);
        }

        private void TransitionToState(ZombieState newState)
        {
            if (State == newState) return;
            State = newState;
            if (newState == ZombieState.Investigating || newState == ZombieState.Searching)
            {
                _investigationTimer = _definition.InvestigationDurationSeconds;
            }
        }

        private void ExecuteState(float deltaTime, ZombiePerception perception, IZombieNavigation navigation)
        {
            switch (State)
            {
                case ZombieState.Idle:
                    Velocity = Vector3.Zero;
                    break;
                case ZombieState.Wandering:
                    Wander(deltaTime, navigation);
                    break;
                case ZombieState.Investigating:
                    Investigate(deltaTime, navigation);
                    break;
                case ZombieState.Chasing:
                    Chase(LastKnownTargetPosition, deltaTime, navigation);
                    break;
                case ZombieState.Searching:
                    Search(deltaTime, navigation);
                    break;
                case ZombieState.Attacking:
                    Attack(LastKnownTargetPosition, navigation);
                    break;
                case ZombieState.Fleeing:
                    Flee(perception.LightPosition, deltaTime, navigation);
                    break;
                case ZombieState.Dead:
                    Velocity = Vector3.Zero;
                    break;
            }

            if (State != ZombieState.Dead)
            {
                Position += Velocity * deltaTime;
            }
        }

        private void Wander(float deltaTime, IZombieNavigation navigation)
        {
            var target = navigation.GetWanderTarget(Position);
            MoveTowards(target, _definition.MoveSpeed, navigation);
        }

        private void Investigate(float deltaTime, IZombieNavigation navigation)
        {
            var target = LastKnownTargetPosition;
            if (Vector3.Magnitude(target - Position) < 1.2f)
            {
                _investigationTimer -= deltaTime;
                if (_investigationTimer <= 0f)
                {
                    TransitionToState(ZombieState.Wandering);
                }
            }
            else
            {
                MoveTowards(target, _definition.MoveSpeed, navigation);
            }
        }

        private void Search(float deltaTime, IZombieNavigation navigation)
        {
            var target = navigation.GetSearchTarget(Position, LastKnownTargetPosition);
            MoveTowards(target, _definition.MoveSpeed * 0.9f, navigation);
            _investigationTimer -= deltaTime;
            if (_investigationTimer <= 0f)
            {
                TransitionToState(ZombieState.Wandering);
            }
        }

        private void Chase(Vector3 targetPosition, float deltaTime, IZombieNavigation navigation)
        {
            MoveTowards(targetPosition, _definition.RunSpeed, navigation);
        }

        private void Flee(Vector3 lightPosition, float deltaTime, IZombieNavigation navigation)
        {
            var direction = Vector3.Normalize(Position - lightPosition);
            var fleeTarget = Position + direction * 4f;
            MoveTowards(fleeTarget, _definition.MoveSpeed, navigation);
        }

        private void Attack(Vector3 targetPosition, IZombieNavigation navigation)
        {
            Velocity = Vector3.Zero;
            if (_attackCooldown > 0f) return;

            if (Vector3.Magnitude(targetPosition - Position) <= _definition.AttackRange)
            {
                _attackCooldown = _definition.AttackCooldownSeconds;
                navigation.OnAttack(this, targetPosition, _definition.AttackDamage);
            }
            else
            {
                TransitionToState(ZombieState.Chasing);
            }
        }

        private void MoveTowards(Vector3 destination, float speed, IZombieNavigation navigation)
        {
            var direction = destination - Position;
            var normalized = Vector3.Normalize(direction);
            if (normalized == Vector3.Zero)
            {
                Velocity = Vector3.Zero;
                return;
            }

            Velocity = normalized * speed;
            Velocity = navigation.ApplyNavigation(Position, Velocity);
        }

        public void ApplyDamage(float damage)
        {
            var effectiveDamage = _definition.IsArmored ? damage * 0.75f : damage;
            Health -= effectiveDamage;
            if (Health <= 0f)
            {
                Health = 0f;
                State = ZombieState.Dead;
            }
        }
    }
}

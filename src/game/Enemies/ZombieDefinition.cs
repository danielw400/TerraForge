using System;
using TerraForge.Core;

namespace TerraForge.Game.Enemies
{
    public sealed class ZombieDefinition
    {
        public ZombieType Type { get; }
        public string Name { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float RunSpeed { get; }
        public float AttackDamage { get; }
        public float AttackRange { get; }
        public float HearingRadius { get; }
        public float SightRadius { get; }
        public float LightDetectionRadius { get; }
        public float LightAlertThreshold { get; }
        public float LightAvoidanceThreshold { get; }
        public float BaseAttackRadius { get; }
        public float AttackCooldownSeconds { get; }
        public float InvestigationDurationSeconds { get; }
        public float SoundSensitivity { get; }
        public bool IgnoresLight { get; }
        public bool IsArmored { get; }

        public ZombieDefinition(
            ZombieType type,
            string name,
            float maxHealth,
            float moveSpeed,
            float runSpeed,
            float attackDamage,
            float attackRange,
            float hearingRadius,
            float sightRadius,
            float lightDetectionRadius,
            float lightAlertThreshold,
            float lightAvoidanceThreshold,
            float baseAttackRadius,
            float attackCooldownSeconds,
            float investigationDurationSeconds,
            float soundSensitivity,
            bool ignoresLight,
            bool isArmored)
        {
            Type = type;
            Name = name;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            RunSpeed = runSpeed;
            AttackDamage = attackDamage;
            AttackRange = attackRange;
            HearingRadius = hearingRadius;
            SightRadius = sightRadius;
            LightDetectionRadius = lightDetectionRadius;
            LightAlertThreshold = lightAlertThreshold;
            LightAvoidanceThreshold = lightAvoidanceThreshold;
            BaseAttackRadius = baseAttackRadius;
            AttackCooldownSeconds = attackCooldownSeconds;
            InvestigationDurationSeconds = investigationDurationSeconds;
            SoundSensitivity = soundSensitivity;
            IgnoresLight = ignoresLight;
            IsArmored = isArmored;
        }

        public static ZombieDefinition Create(ZombieType type)
        {
            return type switch
            {
                ZombieType.InfectadoComum => new ZombieDefinition(
                    type: type,
                    name: "Infectado Comum",
                    maxHealth: 75f,
                    moveSpeed: 2.3f,
                    runSpeed: 4.5f,
                    attackDamage: 10f,
                    attackRange: 1.4f,
                    hearingRadius: 12f,
                    sightRadius: 14f,
                    lightDetectionRadius: 16f,
                    lightAlertThreshold: 0.35f,
                    lightAvoidanceThreshold: 0.75f,
                    baseAttackRadius: 18f,
                    attackCooldownSeconds: 1.25f,
                    investigationDurationSeconds: 4f,
                    soundSensitivity: 1.0f,
                    ignoresLight: false,
                    isArmored: false),

                ZombieType.Corredor => new ZombieDefinition(
                    type: type,
                    name: "Corredor",
                    maxHealth: 60f,
                    moveSpeed: 3.8f,
                    runSpeed: 6.8f,
                    attackDamage: 7f,
                    attackRange: 1.2f,
                    hearingRadius: 18f,
                    sightRadius: 16f,
                    lightDetectionRadius: 20f,
                    lightAlertThreshold: 0.25f,
                    lightAvoidanceThreshold: 0.85f,
                    baseAttackRadius: 20f,
                    attackCooldownSeconds: 0.9f,
                    investigationDurationSeconds: 3f,
                    soundSensitivity: 1.35f,
                    ignoresLight: false,
                    isArmored: false),

                ZombieType.Blindado => new ZombieDefinition(
                    type: type,
                    name: "Blindado",
                    maxHealth: 140f,
                    moveSpeed: 1.7f,
                    runSpeed: 3.2f,
                    attackDamage: 18f,
                    attackRange: 1.7f,
                    hearingRadius: 10f,
                    sightRadius: 12f,
                    lightDetectionRadius: 10f,
                    lightAlertThreshold: 0.6f,
                    lightAvoidanceThreshold: 0.92f,
                    baseAttackRadius: 16f,
                    attackCooldownSeconds: 1.7f,
                    investigationDurationSeconds: 5f,
                    soundSensitivity: 0.8f,
                    ignoresLight: false,
                    isArmored: true),

                ZombieType.Mutante => new ZombieDefinition(
                    type: type,
                    name: "Mutante",
                    maxHealth: 180f,
                    moveSpeed: 2.7f,
                    runSpeed: 5.5f,
                    attackDamage: 22f,
                    attackRange: 1.8f,
                    hearingRadius: 20f,
                    sightRadius: 18f,
                    lightDetectionRadius: 22f,
                    lightAlertThreshold: 0.15f,
                    lightAvoidanceThreshold: 1.0f,
                    baseAttackRadius: 24f,
                    attackCooldownSeconds: 1.1f,
                    investigationDurationSeconds: 6f,
                    soundSensitivity: 1.5f,
                    ignoresLight: true,
                    isArmored: true),

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }
    }
}

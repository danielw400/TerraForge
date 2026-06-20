using System.Collections.Generic;

namespace TerraForge.Core
{
    public static class BiomePalette
    {
        public static readonly Dictionary<BiomeType, BiomeDefinition> Definitions = new Dictionary<BiomeType, BiomeDefinition>
        {
            {
                BiomeType.Forest,
                new BiomeDefinition(
                    BiomeType.Forest,
                    0.02f,
                    20,
                    12,
                    3,
                    0.2f,
                    0.4f,
                    0.6f,
                    BlockTypeIds.Grass,
                    BlockTypeIds.Dirt,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Water)
            },
            {
                BiomeType.Desert,
                new BiomeDefinition(
                    BiomeType.Desert,
                    0.008f,
                    10,
                    8,
                    1,
                    0.05f,
                    0.2f,
                    0.1f,
                    BlockTypeIds.Sand,
                    BlockTypeIds.Sand,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Air)
            },
            {
                BiomeType.Mountains,
                new BiomeDefinition(
                    BiomeType.Mountains,
                    0.018f,
                    40,
                    28,
                    6,
                    0.1f,
                    0.35f,
                    0.6f,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Water)
            },
            {
                BiomeType.Swamp,
                new BiomeDefinition(
                    BiomeType.Swamp,
                    0.018f,
                    16,
                    10,
                    2,
                    0.3f,
                    0.45f,
                    0.35f,
                    BlockTypeIds.Grass,
                    BlockTypeIds.Mud,
                    BlockTypeIds.Stone,
                    BlockTypeIds.SwampWater)
            },
            {
                BiomeType.InfectedZone,
                new BiomeDefinition(
                    BiomeType.InfectedZone,
                    0.014f,
                    18,
                    14,
                    3,
                    0.15f,
                    0.35f,
                    0.8f,
                    BlockTypeIds.InfectedGrass,
                    BlockTypeIds.InfectedSoil,
                    BlockTypeIds.Stone,
                    BlockTypeIds.Water)
            }
        };
    }
}

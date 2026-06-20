namespace TerraForge.Core
{
    public sealed class BiomeDefinition
    {
        public BiomeType BiomeType { get; }
        public float HeightNoiseScale { get; }
        public int HeightBase { get; }
        public int HeightVariation { get; }
        public int CliffThreshold { get; }
        public float ForestDensity { get; }
        public float Roughness { get; }
        public float InfectionDensity { get; }
        public ushort SurfaceBlockId { get; }
        public ushort SubsurfaceBlockId { get; }
        public ushort BaseBlockId { get; }
        public ushort WaterBlockId { get; }

        public BiomeDefinition(
            BiomeType biomeType,
            float heightNoiseScale,
            int heightBase,
            int heightVariation,
            int cliffThreshold,
            float forestDensity,
            float roughness,
            float infectionDensity,
            ushort surfaceBlockId,
            ushort subsurfaceBlockId,
            ushort baseBlockId,
            ushort waterBlockId)
        {
            BiomeType = biomeType;
            HeightNoiseScale = heightNoiseScale;
            HeightBase = heightBase;
            HeightVariation = heightVariation;
            CliffThreshold = cliffThreshold;
            ForestDensity = forestDensity;
            Roughness = roughness;
            InfectionDensity = infectionDensity;
            SurfaceBlockId = surfaceBlockId;
            SubsurfaceBlockId = subsurfaceBlockId;
            BaseBlockId = baseBlockId;
            WaterBlockId = waterBlockId;
        }
    }
}

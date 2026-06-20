using System;

namespace TerraForge.Core
{
    public sealed class WorldGenerator
    {
        private readonly BlockRegistry _blockRegistry;
        private readonly int _seaLevel;
        private readonly SeededNoise _biomeNoise;
        private readonly SeededNoise _terrainNoise;
        private readonly SeededNoise _caveNoise;
        private readonly VegetationGenerator _vegetationGenerator;

        public WorldGenerator(BlockRegistry blockRegistry, int seed, int seaLevel = 32)
        {
            _blockRegistry = blockRegistry;
            _seaLevel = seaLevel;
            _biomeNoise = new SeededNoise(seed);
            _terrainNoise = new SeededNoise(seed + 10_000);
            _caveNoise = new SeededNoise(seed + 20_000);
            _vegetationGenerator = new VegetationGenerator(seed);
        }

        public Chunk GenerateChunk(ChunkCoord coord)
        {
            var chunk = new Chunk(coord);
            var baseX = coord.X * Chunk.ChunkSize;
            var baseY = coord.Y * Chunk.ChunkSize;
            var baseZ = coord.Z * Chunk.ChunkSize;

            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var z = 0; z < Chunk.ChunkSize; z++)
                {
                    var worldX = baseX + x;
                    var worldZ = baseZ + z;
                    var biome = GetBiome(worldX, worldZ);
                    var height = GetSurfaceHeight(worldX, worldZ, biome);

                    for (var y = 0; y < Chunk.ChunkSize; y++)
                    {
                        var worldY = baseY + y;
                        VoxelData voxel;

                        if (worldY <= height)
                        {
                            voxel = GetGroundVoxel(worldX, worldY, height, biome);

                            if (IsCave(worldX, worldY, worldZ, height, biome))
                            {
                                voxel = VoxelData.Empty;
                            }
                        }
                        else if (worldY <= _seaLevel)
                        {
                            voxel = new VoxelData(biome.WaterBlockId);
                        }
                        else
                        {
                            voxel = VoxelData.Empty;
                        }

                        chunk.SetBlock(x, y, z, voxel);
                    }
                }
            }

            _vegetationGenerator.PopulateChunk(chunk, baseX, baseY, baseZ, GetBiome, GetSurfaceHeight);
            return chunk;
        }

        private BiomeDefinition GetBiome(int x, int z)
        {
            var raw = _biomeNoise.GetNoise(x * 0.0005f, z * 0.0005f, 1f);
            var variation = _terrainNoise.GetNoise(x * 0.0018f, z * 0.0018f, 1f) * 0.18f;
            var value = raw + variation - 0.05f;

            if (value < -0.45f)
            {
                return BiomePalette.Definitions[BiomeType.Desert];
            }

            if (value < -0.15f)
            {
                return BiomePalette.Definitions[BiomeType.Swamp];
            }

            if (value < 0.20f)
            {
                return BiomePalette.Definitions[BiomeType.Forest];
            }

            if (value < 0.55f)
            {
                return BiomePalette.Definitions[BiomeType.Mountains];
            }

            return BiomePalette.Definitions[BiomeType.InfectedZone];
        }

        private int GetSurfaceHeight(int x, int z, BiomeDefinition biome)
        {
            var baseNoise = FractalNoise(x, z, biome.HeightNoiseScale, 5, 2f, 0.5f);
            var ridge = FractalNoise(x + 10_000, z - 10_000, biome.HeightNoiseScale * 0.7f, 3, 2.2f, 0.55f);
            var height = biome.HeightBase + (int)(baseNoise * biome.HeightVariation) + (int)(ridge * biome.HeightVariation * 0.75f);
            height += GetRiverInfluence(x, z);
            return Math.Max(2, height);
        }

        private int GetRiverInfluence(int x, int z)
        {
            var riverNoise = _biomeNoise.GetNoise(x * 0.0015f, z * 0.0015f, 1f);
            return Math.Abs(riverNoise) < 0.05f ? -4 : 0;
        }

        private float FractalNoise(int x, int z, float scale, int octaves, float lacunarity, float gain)
        {
            var amplitude = 1f;
            var frequency = scale;
            var value = 0f;
            var max = 0f;

            for (var i = 0; i < octaves; i++)
            {
                value += _terrainNoise.GetNoise(x * frequency, z * frequency, 1f) * amplitude;
                max += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }

            return max > 0f ? value / max : 0f;
        }

        private bool IsCave(int x, int y, int z, int surfaceHeight, BiomeDefinition biome)
        {
            if (y >= surfaceHeight - 2 || y < 3)
            {
                return false;
            }

            var caveValue = _caveNoise.GetNoise3D(x * 0.022f, y * 0.022f, z * 0.022f, 1f);
            var threshold = 0.52f + (y / 120f) * 0.12f - biome.Roughness * 0.14f;
            return caveValue > threshold;
        }

        private VoxelData GetGroundVoxel(int x, int y, int height, BiomeDefinition biome)
        {
            if (y == height)
            {
                if (biome.BiomeType == BiomeType.Swamp && height <= _seaLevel)
                {
                    return new VoxelData(biome.WaterBlockId);
                }

                return new VoxelData(biome.SurfaceBlockId);
            }

            var depth = height - y;
            if (depth <= 2)
            {
                return new VoxelData(biome.SubsurfaceBlockId);
            }

            if (biome.BiomeType == BiomeType.Swamp && y >= height - 4)
            {
                return new VoxelData(BlockTypeIds.Mud);
            }

            return new VoxelData(biome.BaseBlockId);
        }
    }
}

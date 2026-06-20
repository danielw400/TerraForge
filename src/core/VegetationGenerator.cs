using System;

namespace TerraForge.Core
{
    public sealed class VegetationGenerator
    {
        private readonly SeededNoise _vegetationNoise;

        public VegetationGenerator(int seed)
        {
            _vegetationNoise = new SeededNoise(seed + 30_000);
        }

        public void PopulateChunk(
            Chunk chunk,
            int baseX,
            int baseY,
            int baseZ,
            Func<int, int, BiomeDefinition> biomeLookup,
            Func<int, int, BiomeDefinition, int> surfaceHeightLookup)
        {
            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var z = 0; z < Chunk.ChunkSize; z++)
                {
                    var worldX = baseX + x;
                    var worldZ = baseZ + z;
                    var biome = biomeLookup(worldX, worldZ);
                    var surfaceHeight = surfaceHeightLookup(worldX, worldZ, biome);
                    var localSurfaceY = surfaceHeight - baseY;

                    if (localSurfaceY < 0 || localSurfaceY >= Chunk.ChunkSize)
                    {
                        continue;
                    }

                    if (!chunk.GetBlock(x, localSurfaceY, z).IsEmpty && localSurfaceY + 1 < Chunk.ChunkSize)
                    {
                        var above = chunk.GetBlock(x, localSurfaceY + 1, z);
                        if (!above.IsEmpty)
                        {
                            continue;
                        }
                    }

                    var positionNoise = _vegetationNoise.GetNoise(worldX * 0.014f, worldZ * 0.014f, 1f);
                    SpawnVegetation(chunk, x, localSurfaceY, z, worldX, worldZ, biome, positionNoise);
                }
            }
        }

        private void SpawnVegetation(
            Chunk chunk,
            int localX,
            int surfaceY,
            int localZ,
            int worldX,
            int worldZ,
            BiomeDefinition biome,
            float noise)
        {
            switch (biome.BiomeType)
            {
                case BiomeType.Forest:
                    if (noise > 0.35f && _vegetationNoise.GetNoise(worldX * 0.04f, worldZ * 0.04f, 1f) > 0.3f)
                    {
                        TryPlaceTree(chunk, localX, surfaceY, localZ);
                    }
                    else if (noise > 0.25f)
                    {
                        TryPlaceMushroom(chunk, localX, surfaceY, localZ);
                    }
                    break;
                case BiomeType.Desert:
                    if (noise > 0.4f)
                    {
                        TryPlaceCactus(chunk, localX, surfaceY, localZ);
                    }
                    break;
                case BiomeType.Swamp:
                    if (noise > 0.28f)
                    {
                        TryPlaceReed(chunk, localX, surfaceY, localZ);
                    }
                    else if (noise > 0.1f)
                    {
                        TryPlaceLilyPad(chunk, localX, surfaceY, localZ);
                    }
                    break;
                case BiomeType.Mountains:
                    if (noise > 0.5f && _vegetationNoise.GetNoise(worldX * 0.05f, worldZ * 0.05f, 1f) > 0.4f)
                    {
                        TryPlaceMushroom(chunk, localX, surfaceY, localZ);
                    }
                    break;
                case BiomeType.InfectedZone:
                    if (noise > 0.35f)
                    {
                        TryPlaceDeadLog(chunk, localX, surfaceY, localZ);
                    }
                    else if (noise > 0.2f)
                    {
                        TryPlaceMushroom(chunk, localX, surfaceY, localZ);
                    }
                    break;
            }
        }

        private void TryPlaceTree(Chunk chunk, int x, int y, int z)
        {
            var height = 4 + (int)(_vegetationNoise.GetNoise(x * 0.15f, z * 0.15f, 1f) * 2f);
            if (height < 4) height = 4;
            if (y + height + 2 >= Chunk.ChunkSize) return;
            if (!CanReplace(chunk, x, y + 1, z, height + 2)) return;

            for (var i = 1; i <= height; i++)
            {
                chunk.SetBlock(x, y + i, z, new VoxelData(BlockTypeIds.WoodLog));
            }

            var leafStart = y + height - 1;
            for (var ox = -2; ox <= 2; ox++)
            {
                for (var oz = -2; oz <= 2; oz++)
                {
                    for (var oy = 0; oy <= 2; oy++)
                    {
                        var px = x + ox;
                        var py = leafStart + oy;
                        var pz = z + oz;
                        if (!chunk.EqualsPosition(px, py, pz)) continue;
                        if (Math.Abs(ox) + Math.Abs(oz) + oy > 4) continue;
                        if (chunk.GetBlock(px, py, pz).IsEmpty)
                        {
                            chunk.SetBlock(px, py, pz, new VoxelData(BlockTypeIds.Leaves));
                        }
                    }
                }
            }
        }

        private void TryPlaceCactus(Chunk chunk, int x, int y, int z)
        {
            var height = 2 + (int)(_vegetationNoise.GetNoise(x * 0.2f, z * 0.2f, 1f) * 2f);
            if (height < 2) height = 2;
            if (y + height >= Chunk.ChunkSize) return;
            if (!CanReplace(chunk, x, y + 1, z, height)) return;

            for (var i = 1; i <= height; i++)
            {
                chunk.SetBlock(x, y + i, z, new VoxelData(BlockTypeIds.Cactus));
            }
        }

        private void TryPlaceLilyPad(Chunk chunk, int x, int y, int z)
        {
            if (y + 1 >= Chunk.ChunkSize) return;
            if (chunk.GetBlock(x, y, z).BlockId != BlockTypeIds.Water) return;
            if (!chunk.GetBlock(x, y + 1, z).IsEmpty) return;
            chunk.SetBlock(x, y + 1, z, new VoxelData(BlockTypeIds.LilyPad));
        }

        private void TryPlaceReed(Chunk chunk, int x, int y, int z)
        {
            if (y + 2 >= Chunk.ChunkSize) return;
            if (chunk.GetBlock(x, y, z).BlockId != BlockTypeIds.Water && chunk.GetBlock(x, y, z).IsEmpty) return;
            if (!chunk.GetBlock(x, y + 1, z).IsEmpty) return;
            chunk.SetBlock(x, y + 1, z, new VoxelData(BlockTypeIds.Reed));
        }

        private void TryPlaceDeadLog(Chunk chunk, int x, int y, int z)
        {
            if (x + 1 >= Chunk.ChunkSize || z + 1 >= Chunk.ChunkSize) return;
            chunk.SetBlock(x, y + 1, z, new VoxelData(BlockTypeIds.DeadLog));
            chunk.SetBlock(x + 1, y + 1, z, new VoxelData(BlockTypeIds.DeadLog));
        }

        private void TryPlaceMushroom(Chunk chunk, int x, int y, int z)
        {
            if (y + 1 >= Chunk.ChunkSize) return;
            if (!chunk.GetBlock(x, y + 1, z).IsEmpty) return;
            chunk.SetBlock(x, y + 1, z, new VoxelData(BlockTypeIds.Mushroom));
        }

        private bool CanReplace(Chunk chunk, int x, int startY, int z, int height)
        {
            for (var i = 0; i < height; i++)
            {
                var px = x;
                var py = startY + i;
                var pz = z;
                if (!chunk.EqualsPosition(px, py, pz)) return false;
                if (!chunk.GetBlock(px, py, pz).IsEmpty) return false;
            }

            return true;
        }
    }
}

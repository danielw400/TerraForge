using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerraForge.Core
{
    public sealed class ChunkManager
    {
        private readonly ConcurrentDictionary<ChunkCoord, Chunk> _chunks = new ConcurrentDictionary<ChunkCoord, Chunk>();
        private readonly WorldGenerator _generator;
        private readonly int _loadRadius;
        private readonly int _unloadRadius;

        public ChunkManager(WorldGenerator generator, int loadRadius = 4, int unloadRadius = 6)
        {
            _generator = generator;
            _loadRadius = loadRadius;
            _unloadRadius = unloadRadius;
        }

        public IReadOnlyCollection<Chunk> LoadedChunks => _chunks.Values;

        public void UpdateChunks(ChunkCoord playerChunk)
        {
            var requiredChunks = GetChunksWithinRadius(playerChunk, _loadRadius);
            var chunksToLoad = requiredChunks.Where(coord => !_chunks.ContainsKey(coord)).ToArray();

            Parallel.ForEach(chunksToLoad, coord =>
            {
                var chunk = _generator.GenerateChunk(coord);
                _chunks[coord] = chunk;
            });

            var unloadLimit = _unloadRadius * _unloadRadius;
            var chunksToUnload = _chunks.Keys.Where(coord => coord.DistanceSquared(playerChunk) > unloadLimit).ToArray();
            foreach (var coord in chunksToUnload)
            {
                _chunks.TryRemove(coord, out _);
            }
        }

        public Chunk? GetChunk(ChunkCoord coord)
        {
            _chunks.TryGetValue(coord, out var chunk);
            return chunk;
        }

        public VoxelData GetBlock(Vector3Int worldPosition)
        {
            var chunkCoord = ToChunkCoord(worldPosition);
            var localPos = ToLocalPosition(worldPosition);
            var chunk = GetChunk(chunkCoord);
            return chunk?.GetBlock(localPos.X, localPos.Y, localPos.Z) ?? VoxelData.Empty;
        }

        public void SetBlock(Vector3Int worldPosition, VoxelData voxel)
        {
            var chunkCoord = ToChunkCoord(worldPosition);
            var localPos = ToLocalPosition(worldPosition);
            var chunk = GetOrCreateChunk(chunkCoord);
            chunk.SetBlock(localPos.X, localPos.Y, localPos.Z, voxel);
        }

        public void DestroyBlock(Vector3Int worldPosition)
        {
            SetBlock(worldPosition, VoxelData.Empty);
        }

        private Chunk GetOrCreateChunk(ChunkCoord coord)
        {
            return _chunks.GetOrAdd(coord, _generator.GenerateChunk);
        }

        private static ChunkCoord ToChunkCoord(Vector3Int worldPos)
        {
            return new ChunkCoord(
                FloorDiv(worldPos.X, Chunk.ChunkSize),
                FloorDiv(worldPos.Y, Chunk.ChunkSize),
                FloorDiv(worldPos.Z, Chunk.ChunkSize));
        }

        private static Vector3Int ToLocalPosition(Vector3Int worldPos)
        {
            return new Vector3Int(
                Mod(worldPos.X, Chunk.ChunkSize),
                Mod(worldPos.Y, Chunk.ChunkSize),
                Mod(worldPos.Z, Chunk.ChunkSize));
        }

        private static int FloorDiv(int a, int b)
        {
            var div = a / b;
            if ((a ^ b) < 0 && a % b != 0) div--;
            return div;
        }

        private static int Mod(int a, int b)
        {
            var result = a % b;
            return result < 0 ? result + b : result;
        }

        private static IEnumerable<ChunkCoord> GetChunksWithinRadius(ChunkCoord center, int radius)
        {
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                for (var y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    for (var z = center.Z - radius; z <= center.Z + radius; z++)
                    {
                        yield return new ChunkCoord(x, y, z);
                    }
                }
            }
        }
    }
}

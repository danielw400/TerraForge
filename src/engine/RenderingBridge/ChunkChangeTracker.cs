using System;
using System.Collections.Generic;
using System.Linq;
using TerraForge.Core;
using TerraForge.Game;

namespace TerraForge.Engine.RenderingBridge
{
    public sealed class ChunkChangeTracker
    {
        private readonly ChunkManager _chunkManager;
        private readonly HashSet<ChunkCoord> _knownChunks = new HashSet<ChunkCoord>();

        public ChunkChangeTracker(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
        }

        public IReadOnlyCollection<Chunk> GetUpdatedChunks()
        {
            var updated = new List<Chunk>();
            foreach (var chunk in _chunkManager.LoadedChunks)
            {
                if (chunk.IsDirty || !_knownChunks.Contains(chunk.Coord))
                {
                    updated.Add(chunk);
                    _knownChunks.Add(chunk.Coord);
                }
            }

            return updated;
        }

        public IReadOnlyCollection<ChunkCoord> GetRemovedChunks()
        {
            var removed = _knownChunks.Except(_chunkManager.LoadedChunks.Select(c => c.Coord)).ToList();
            foreach (var coord in removed)
            {
                _knownChunks.Remove(coord);
            }

            return removed;
        }
    }
}

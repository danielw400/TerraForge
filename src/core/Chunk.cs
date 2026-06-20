using System;

namespace TerraForge.Core
{
    public sealed class Chunk
    {
        public const int ChunkSize = 16;
        private readonly VoxelData[,,] _voxels = new VoxelData[ChunkSize, ChunkSize, ChunkSize];

        public ChunkCoord Coord { get; }
        public bool IsDirty { get; private set; }

        public Chunk(ChunkCoord coord)
        {
            Coord = coord;
            IsDirty = true;
            InitializeEmpty();
        }

        private void InitializeEmpty()
        {
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkSize; z++)
                    {
                        _voxels[x, y, z] = VoxelData.Empty;
                    }
                }
            }
        }

        public VoxelData GetBlock(int x, int y, int z)
        {
            ValidateLocalCoordinates(x, y, z);
            return _voxels[x, y, z];
        }

        public void SetBlock(int x, int y, int z, VoxelData voxel)
        {
            ValidateLocalCoordinates(x, y, z);
            _voxels[x, y, z] = voxel;
            IsDirty = true;
        }

        public bool EqualsPosition(int x, int y, int z)
        {
            return x >= 0 && x < ChunkSize
                && y >= 0 && y < ChunkSize
                && z >= 0 && z < ChunkSize;
        }

        public void ClearDirtyFlag()
        {
            IsDirty = false;
        }

        private static void ValidateLocalCoordinates(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Local coordinates must be inside a chunk.");
            }
        }
    }
}

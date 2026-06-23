using TerraForge.Core;
using TerraForge.Engine.RenderingBridge.Dtos;

namespace TerraForge.Engine.RenderingBridge.Adapters
{
    /// <summary>
    /// Converte chunks do motor de jogo para DTOs de renderização.
    /// </summary>
    /// <example>
    /// var chunkDto = adapter.ToDto(chunk);
    /// </example>
    public sealed class ChunkDtoAdapter
    {
        public ChunkDto ToDto(Chunk chunk)
        {
            return new ChunkDto
            {
                Coord = ToDtoCoord(chunk.Coord),
                Blocks = ExtractBlockIds(chunk),
                Version = 0,
                IsDirty = chunk.IsDirty
            };
        }

        public ChunkCoordDto ToDtoCoord(ChunkCoord coord)
        {
            return new ChunkCoordDto
            {
                X = coord.X,
                Y = coord.Y,
                Z = coord.Z
            };
        }

        private static ushort[] ExtractBlockIds(Chunk chunk)
        {
            var blocks = new ushort[Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize];
            var index = 0;
            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var y = 0; y < Chunk.ChunkSize; y++)
                {
                    for (var z = 0; z < Chunk.ChunkSize; z++)
                    {
                        blocks[index++] = chunk.GetBlock(x, y, z).BlockId;
                    }
                }
            }

            return blocks;
        }
    }
}
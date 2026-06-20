using System;
using TerraForge.Core;

namespace TerraForge.Game
{
    public sealed class VoxelWorldAdapter
    {
        private readonly ChunkManager _chunkManager;
        private readonly Vector3 _halfExtents;

        public VoxelWorldAdapter(ChunkManager chunkManager, Vector3 halfExtents)
        {
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _halfExtents = halfExtents;
        }

        // Returns functions compatible with PlayerController expectations
        public Func<Vector3, Vector3, VoxelPhysics.SweptAabbResult> GetSweepCollisionFunction()
        {
            return (pos, displacement) => VoxelPhysics.SweepAabb(_chunkManager, pos, _halfExtents, displacement);
        }

        public Func<Vector3, bool> GetCollisionFunction()
        {
            return pos => VoxelPhysics.CheckAabbCollision(_chunkManager, pos, _halfExtents);
        }

        public Func<Vector3, bool> GetIsWaterFunction()
        {
            return pos =>
            {
                // check voxel at feet and at eye level
                var foot = Vector3Int.FloorToInt(pos);
                var head = Vector3Int.FloorToInt(new Vector3(pos.X, pos.Y + _halfExtents.Y * 1.5f, pos.Z));

                var footBlock = _chunkManager.GetBlock(foot);
                var headBlock = _chunkManager.GetBlock(head);

                return footBlock.BlockId == BlockTypeIds.Water || footBlock.BlockId == BlockTypeIds.SwampWater
                    || headBlock.BlockId == BlockTypeIds.Water || headBlock.BlockId == BlockTypeIds.SwampWater;
            };
        }

        public Func<Vector3, bool> GetIsClimbableFunction()
        {
            return pos =>
            {
                // simple heuristic: if any horizontal neighbour at chest height is a vertical solid (e.g., wood log)
                var chestPos = Vector3Int.FloorToInt(new Vector3(pos.X, pos.Y + 1f, pos.Z));
                var offsets = new (int x, int z)[] { (1,0), (-1,0), (0,1), (0,-1) };
                foreach (var o in offsets)
                {
                    var b = _chunkManager.GetBlock(new Vector3Int(chestPos.X + o.x, chestPos.Y, chestPos.Z + o.z));
                    if (!b.IsEmpty && (b.BlockId == BlockTypeIds.WoodLog || b.BlockId == BlockTypeIds.DeadLog)) return true;
                }

                return false;
            };
        }

        // Returns the approximate topmost surface Y coordinate at integer (x,z)
        public int GetSurfaceHeightAt(int x, int z, int searchTop = 256, int searchBottom = -64)
        {
            for (var y = searchTop; y >= searchBottom; y--)
            {
                var v = _chunkManager.GetBlock(new Vector3Int(x, y, z));
                if (!v.IsEmpty) return y;
            }
            return searchBottom;
        }

        // Returns ground normal and slope angle (degrees) at given world position
        public Func<Vector3, (Vector3 normal, float angle)> GetGroundInfoFunction()
        {
            return pos =>
            {
                var ix = (int)MathF.Floor(pos.X);
                var iz = (int)MathF.Floor(pos.Z);

                var hL = GetSurfaceHeightAt(ix - 1, iz);
                var hR = GetSurfaceHeightAt(ix + 1, iz);
                var hF = GetSurfaceHeightAt(ix, iz + 1);
                var hB = GetSurfaceHeightAt(ix, iz - 1);
                var hC = GetSurfaceHeightAt(ix, iz);

                var dx = (hR - hL) * 0.5f;
                var dz = (hF - hB) * 0.5f;

                var n = TerraForge.Core.Vector3.Normalize(new TerraForge.Core.Vector3(-dx, 1f, -dz));
                var angle = MathF.Acos(MathF.Clamp(n.Y, -1f, 1f)) * (180f / MathF.PI);
                return (n, angle);
            };
        }

        public (int x, int y, int z)? RaycastBlock(Vector3 origin, Vector3 direction, float maxDistance)
        {
            return VoxelPhysics.Raycast(_chunkManager, origin, direction, maxDistance);
        }

        public bool RemoveBlockAt(Vector3 origin, Vector3 direction, float maxDistance)
        {
            var hit = RaycastBlock(origin, direction, maxDistance);
            if (hit == null) return false;
            var (x, y, z) = hit.Value;
            _chunkManager.DestroyBlock(new Vector3Int(x, y, z));
            return true;
        }

        public bool PlaceBlockAt(Vector3 origin, Vector3 direction, ushort blockId, float maxDistance)
        {
            var hit = RaycastBlock(origin, direction, maxDistance);
            if (hit == null) return false;
            var (x, y, z) = hit.Value;
            // determine step direction to place on face: take sign of direction
            var sx = Math.Sign(direction.X);
            var sy = Math.Sign(direction.Y);
            var sz = Math.Sign(direction.Z);
            var placeX = x - sx;
            var placeY = y - sy;
            var placeZ = z - sz;

            var placePos = new Vector3Int(placeX, placeY, placeZ);
            _chunkManager.SetBlock(placePos, new VoxelData(blockId));
            return true;
        }

        public bool IsBlockAt(Vector3 worldPos)
        {
            var v = _chunkManager.GetBlock(Vector3Int.FloorToInt(worldPos));
            return !v.IsEmpty;
        }
    }
}

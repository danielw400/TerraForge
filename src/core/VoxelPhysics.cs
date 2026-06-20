using System;

namespace TerraForge.Core
{
    public static class VoxelPhysics
    {
        public readonly struct SweptAabbResult
        {
            public bool Hit { get; }
            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public float Time { get; }
            public Vector3 Remainder { get; }

            public SweptAabbResult(bool hit, Vector3 position, Vector3 normal, float time, Vector3 remainder)
            {
                Hit = hit;
                Position = position;
                Normal = normal;
                Time = time;
                Remainder = remainder;
            }
        }

        public static bool CheckAabbCollision(ChunkManager chunkManager, Vector3 position, Vector3 halfExtents)
        {
            var minX = (int)MathF.Floor(position.X - halfExtents.X);
            var maxX = (int)MathF.Floor(position.X + halfExtents.X);
            var minY = (int)MathF.Floor(position.Y - halfExtents.Y);
            var maxY = (int)MathF.Floor(position.Y + halfExtents.Y);
            var minZ = (int)MathF.Floor(position.Z - halfExtents.Z);
            var maxZ = (int)MathF.Floor(position.Z + halfExtents.Z);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    for (var z = minZ; z <= maxZ; z++)
                    {
                        var voxel = chunkManager.GetBlock(new Vector3Int(x, y, z));
                        if (!voxel.IsEmpty)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static SweptAabbResult SweepAabb(ChunkManager chunkManager, Vector3 currentPosition, Vector3 halfExtents, Vector3 displacement)
        {
            var destination = currentPosition + displacement;
            var startMin = currentPosition - halfExtents;
            var startMax = currentPosition + halfExtents;
            var endMin = destination - halfExtents;
            var endMax = destination + halfExtents;

            var broadMin = new Vector3(MathF.Min(startMin.X, endMin.X), MathF.Min(startMin.Y, endMin.Y), MathF.Min(startMin.Z, endMin.Z));
            var broadMax = new Vector3(MathF.Max(startMax.X, endMax.X), MathF.Max(startMax.Y, endMax.Y), MathF.Max(startMax.Z, endMax.Z));

            var startX = (int)MathF.Floor(broadMin.X);
            var endX = (int)MathF.Floor(broadMax.X);
            var startY = (int)MathF.Floor(broadMin.Y);
            var endY = (int)MathF.Floor(broadMax.Y);
            var startZ = (int)MathF.Floor(broadMin.Z);
            var endZ = (int)MathF.Floor(broadMax.Z);

            var earliestTime = 1f;
            var collisionNormal = Vector3.Zero;
            var hit = false;

            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    for (var z = startZ; z <= endZ; z++)
                    {
                        var voxel = chunkManager.GetBlock(new Vector3Int(x, y, z));
                        if (voxel.IsEmpty) continue;

                        var blockMin = new Vector3(x, y, z);
                        var blockMax = new Vector3(x + 1, y + 1, z + 1);
                        if (SweptAabbVsAabb(startMin, startMax, displacement, blockMin, blockMax, out var time, out var normal) && time < earliestTime)
                        {
                            earliestTime = time;
                            collisionNormal = normal;
                            hit = true;
                        }
                    }
                }
            }

            if (!hit)
            {
                return new SweptAabbResult(false, destination, Vector3.Zero, 1f, Vector3.Zero);
            }

            var impactPosition = currentPosition + displacement * earliestTime;
            var remainingMovement = displacement * (1f - earliestTime);
            var slideRemainder = remainingMovement - collisionNormal * Vector3.Dot(remainingMovement, collisionNormal);
            return new SweptAabbResult(true, impactPosition, collisionNormal, earliestTime, slideRemainder);
        }

        private static bool SweptAabbVsAabb(Vector3 movingMin, Vector3 movingMax, Vector3 displacement, Vector3 staticMin, Vector3 staticMax, out float time, out Vector3 normal)
        {
            time = 0f;
            normal = Vector3.Zero;

            var entry = 0f;
            var exit = 1f;
            var entryNormal = Vector3.Zero;

            if (!OverlapOnAxis(movingMin.X, movingMax.X, displacement.X, staticMin.X, staticMax.X, ref entry, ref exit, ref entryNormal, ref normal, 0)) return false;
            if (!OverlapOnAxis(movingMin.Y, movingMax.Y, displacement.Y, staticMin.Y, staticMax.Y, ref entry, ref exit, ref entryNormal, ref normal, 1)) return false;
            if (!OverlapOnAxis(movingMin.Z, movingMax.Z, displacement.Z, staticMin.Z, staticMax.Z, ref entry, ref exit, ref entryNormal, ref normal, 2)) return false;

            if (entry < 0f || entry > 1f) return false;

            time = entry;
            normal = entryNormal;
            return true;
        }

        private static bool OverlapOnAxis(float minA, float maxA, float d, float minB, float maxB, ref float entry, ref float exit, ref Vector3 entryNormal, ref Vector3 normal, int axis)
        {
            if (d == 0f)
            {
                if (maxA <= minB || minA >= maxB)
                {
                    return false;
                }
                return true;
            }

            var invD = 1f / d;
            var t1 = (minB - maxA) * invD;
            var t2 = (maxB - minA) * invD;
            var axisEntry = MathF.Min(t1, t2);
            var axisExit = MathF.Max(t1, t2);

            if (axisEntry > entry)
            {
                entry = axisEntry;
                entryNormal = GetAxisNormal(axis, t1 > t2 ? 1 : -1);
            }

            exit = MathF.Min(exit, axisExit);
            return entry <= exit;
        }

        private static Vector3 GetAxisNormal(int axis, int direction)
        {
            return axis switch
            {
                0 => new Vector3(direction, 0, 0),
                1 => new Vector3(0, direction, 0),
                2 => new Vector3(0, 0, direction),
                _ => Vector3.Zero,
            };
        }

        // Simple voxel raycast using DDA stepping. Returns first non-empty voxel coordinate or null.
        public static (int x, int y, int z)? Raycast(ChunkManager chunkManager, Vector3 origin, Vector3 direction, float maxDistance)
        {
            var dirLength = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
            if (dirLength < 1e-6f) return null;
            var dx = direction.X / dirLength;
            var dy = direction.Y / dirLength;
            var dz = direction.Z / dirLength;

            var x = (int)MathF.Floor(origin.X);
            var y = (int)MathF.Floor(origin.Y);
            var z = (int)MathF.Floor(origin.Z);

            var stepX = dx > 0 ? 1 : -1;
            var stepY = dy > 0 ? 1 : -1;
            var stepZ = dz > 0 ? 1 : -1;

            var tMaxX = IntBound(origin.X, dx);
            var tMaxY = IntBound(origin.Y, dy);
            var tMaxZ = IntBound(origin.Z, dz);

            var tDeltaX = MathF.Abs(1f / dx);
            var tDeltaY = MathF.Abs(1f / dy);
            var tDeltaZ = MathF.Abs(1f / dz);

            var distanceTraveled = 0f;
            while (distanceTraveled <= maxDistance)
            {
                var voxel = chunkManager.GetBlock(new Vector3Int(x, y, z));
                if (!voxel.IsEmpty) return (x, y, z);

                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x += stepX;
                        distanceTraveled = tMaxX;
                        tMaxX += tDeltaX;
                    }
                    else
                    {
                        z += stepZ;
                        distanceTraveled = tMaxZ;
                        tMaxZ += tDeltaZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        y += stepY;
                        distanceTraveled = tMaxY;
                        tMaxY += tDeltaY;
                    }
                    else
                    {
                        z += stepZ;
                        distanceTraveled = tMaxZ;
                        tMaxZ += tDeltaZ;
                    }
                }
            }

            return null;
        }

        private static float IntBound(float s, float ds)
        {
            if (MathF.Abs(ds) < 1e-6f)
            {
                return float.PositiveInfinity;
            }

            if (ds > 0)
            {
                return (MathF.Floor(s + 1) - s) / ds;
            }
            else
            {
                return (s - MathF.Floor(s)) / -ds;
            }
        }
    }
}

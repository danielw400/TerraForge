namespace TerraForge.Core
{
    public readonly struct Vector3
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero => new Vector3(0, 0, 0);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 a, float scale) => new Vector3(a.X * scale, a.Y * scale, a.Z * scale);

        public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static float Magnitude(Vector3 a) => MathF.Sqrt(Dot(a, a));
        public static Vector3 Normalize(Vector3 v)
        {
            var m = Magnitude(v);
            return m > 1e-6f ? new Vector3(v.X / m, v.Y / m, v.Z / m) : Vector3.Zero;
        }

    }

    public readonly struct Vector3Int
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3Int Zero => new Vector3Int(0, 0, 0);

        public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3Int FloorToInt(Vector3 position)
        {
            return new Vector3Int(
                (int)System.Math.Floor(position.X),
                (int)System.Math.Floor(position.Y),
                (int)System.Math.Floor(position.Z));
        }
    }

    public readonly struct ChunkCoord
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public ChunkCoord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static ChunkCoord operator +(ChunkCoord a, ChunkCoord b) => new ChunkCoord(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static ChunkCoord operator -(ChunkCoord a, ChunkCoord b) => new ChunkCoord(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public override bool Equals(object? obj) => obj is ChunkCoord other && other.X == X && other.Y == Y && other.Z == Z;
        public override int GetHashCode() => System.HashCode.Combine(X, Y, Z);

        public int DistanceSquared(ChunkCoord other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            var dz = Z - other.Z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}

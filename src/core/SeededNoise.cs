namespace TerraForge.Core
{
    public sealed class SeededNoise
    {
        private readonly int _seed;

        public SeededNoise(int seed)
        {
            _seed = seed;
        }

        public float GetNoise(float x, float y, float scale)
        {
            return SimplexNoise.Noise.CalcPixel2D(x + _seed, y - _seed, scale);
        }

        public float GetNoise3D(float x, float y, float z, float scale)
        {
            return SimplexNoise.Noise.CalcPixel3D(x + _seed, y - _seed, z + _seed, scale);
        }
    }
}

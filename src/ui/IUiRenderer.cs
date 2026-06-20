namespace TerraForge.UI
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public readonly struct UiColor
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }

        public UiColor(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static UiColor FromRgb(int r, int g, int b, float a = 1f)
        {
            return new UiColor(r / 255f, g / 255f, b / 255f, a);
        }

        public static UiColor FromHex(uint hex, float a = 1f)
        {
            var r = (int)((hex >> 16) & 0xFF);
            var g = (int)((hex >> 8) & 0xFF);
            var b = (int)(hex & 0xFF);
            return FromRgb(r, g, b, a);
        }

        public static readonly UiColor Transparent = new UiColor(0f, 0f, 0f, 0f);
    }

    public interface IUiRenderer
    {
        void DrawRectangle(float x, float y, float width, float height, UiColor color);
        void DrawBorder(float x, float y, float width, float height, float thickness, UiColor color);
        void DrawText(string text, float x, float y, UiColor color, float fontSize, TextAlignment alignment = TextAlignment.Left);
        float MeasureTextWidth(string text, float fontSize);
    }
}

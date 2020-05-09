using SkiaSharp;

namespace VectorTiles
{
    public struct Offset
    {
        public new static Offset Empty = new Offset(0, 0);

        public float X { get; set; }

        public float Y { get; set; }

        public Offset(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}

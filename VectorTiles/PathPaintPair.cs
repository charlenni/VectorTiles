using SkiaSharp;

namespace VectorTiles
{
    public class PathPaintPair
    {
        public PathPaintPair(SKPath path, IVectorPaint paint)
        {
            Path = path;
            Paint = paint;
        }

        public SKPath Path { get; }

        public IVectorPaint Paint { get; }
    }
}

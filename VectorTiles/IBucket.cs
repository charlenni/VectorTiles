using SkiaSharp;

namespace VectorTiles
{
    public interface IBucket
    {
        void OnDraw(SKCanvas canvas, EvaluationContext context);
    }
}

using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    public class MGLTextSymbol : Symbol
    {
        private SKRect envelope = SKRect.Empty;

        public override void OnCalcBoundings()
        {
        }

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (IsVisible)
            {

            }
        }
    }
}

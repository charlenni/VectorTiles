using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    public class MGLIconSymbol : Symbol
    {
        private SKRect envelope = SKRect.Empty;

        public SKImage Image { get; set; }

        public SKPoint ImagePoint { get; set; }

        public MGLPaint Paint;

        public override void OnCalcBoundings()
        {
        }

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (!IsVisible || Image == null)
                return;

            canvas.DrawImage(Image, ImagePoint, Paint.CreatePaint(context));
        }
    }
}

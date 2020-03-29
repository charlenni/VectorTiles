using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    public class MGLIconSymbol : Symbol
    {
        public SKImage Image { get; set; }

        public SKPoint ImagePoint { get; set; }

        public MGLPaint Paint;

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (Image == null)
                return;

            canvas.DrawImage(Image, ImagePoint, Paint.CreatePaint(context));
        }
    }
}

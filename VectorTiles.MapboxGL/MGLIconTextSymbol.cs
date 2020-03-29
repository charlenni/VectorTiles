using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    public class MGLIconTextSymbol : Symbol
    {
        public SKImage Image { get; set; }

        public SKPoint ImagePoint { get; set; }

        public MGLPaint Paint;

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (Image != null)
            {
                canvas.Save();
                canvas.Translate(ImagePoint.X - Image.Width / 2, ImagePoint.Y - Image.Height / 2);
                canvas.Scale(1 / canvas.TotalMatrix.ScaleX, 1 / canvas.TotalMatrix.ScaleY);
                canvas.DrawImage(Image, 0, 0, Paint.CreatePaint(context));
                canvas.Restore();
            }
        }
    }
}

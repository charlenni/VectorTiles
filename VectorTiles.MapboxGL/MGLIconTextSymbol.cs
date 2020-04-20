using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    public class MGLIconTextSymbol : Symbol
    {
        private SKRect envelope = SKRect.Empty;

        public SKImage Image { get; set; }

        public SKPoint ImagePoint { get; set; }

        public MGLPaint Paint;

        //public override ref SKRect Envelope { get => ref envelope; }

        public override void OnCalcBoundings()
        {
            envelope = new SKRect(ImagePoint.X - Image.Width / 2, ImagePoint.Y - Image.Height / 2, ImagePoint.X + Image.Width / 2, ImagePoint.Y + Image.Height / 2);
        }

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (!IsVisible)
                return;

            if (Image != null)
            {
                canvas.Save();
                canvas.Translate(ImagePoint.X, ImagePoint.Y);
                canvas.Scale(1 / canvas.TotalMatrix.ScaleX, 1 / canvas.TotalMatrix.ScaleY);
                canvas.DrawImage(Image, -Image.Width / 2, -Image.Height / 2, Paint.CreatePaint(context));
                canvas.Restore();
            }
        }
    }
}

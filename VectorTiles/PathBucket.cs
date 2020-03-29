using SkiaSharp;
using System.Collections.Generic;

using Rect = SkiaSharp.SKRect;

namespace VectorTiles
{
    public class PathBucket : IBucket
    {
        public PathBucket(IEnumerable<IVectorPaint> paints, Rect clipRect)
        {
            Path = new SKPath() { FillType = SKPathFillType.Winding };
            Paints = new List<IVectorPaint>();
            ClipRect = clipRect;

            foreach (var paint in paints)
                Paints.Add(paint);
        }

        public SKPath Path { get; }

        public List<IVectorPaint> Paints { get; }

        public Rect ClipRect { get; }

        public void AddElement(VectorElement element)
        {
            element.AddToPath(Path, ClipRect);
        }

        public void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (Path.IsEmpty)
                return;

            foreach (var paint in Paints)
            {
                canvas.DrawPath(Path, paint.CreatePaint(context));
            }
        }
    }
}

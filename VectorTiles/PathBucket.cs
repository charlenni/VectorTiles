using SkiaSharp;
using System.Collections.Generic;

using Rect = SkiaSharp.SKRect;

namespace VectorTiles
{
    public class PathBucket : IBucket
    {
        public PathBucket(IEnumerable<IVectorPaint> paints)
        {
            Path = new SKPath();
            Paths = new List<SKPath>();
            Paints = new List<IVectorPaint>();

            foreach (var paint in paints)
                Paints.Add(paint);
        }

        public List<SKPath> Paths { get; }

        public SKPath Path { get; }

        public List<IVectorPaint> Paints { get; }

        public void AddElement(VectorElement element)
        {
            if (element.Type == GeometryType.LineString)
                element.AddToPath(Path);
            else
                Paths.Add(element.CreatePath());
        }

        public void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (Paths.Count != 0)
            {
                foreach (var paint in Paints)
                {
                    var p = paint.CreatePaint(context);

                    foreach (var path in Paths)
                        canvas.DrawPath(path, p);
                }
            }
            else if (Path.PointCount != 0)
            {
                foreach (var paint in Paints)
                {
                    canvas.DrawPath(Path, paint.CreatePaint(context));
                }
            }
        }
    }
}

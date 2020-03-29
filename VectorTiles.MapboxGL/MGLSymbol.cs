using SkiaSharp;
using Point = SkiaSharp.SKPoint;

namespace VectorTiles.MapboxGL
{
    public abstract class MGLSymbol : Symbol
    {
        public MGLSymbol(VectorElement feature, Point point, IVectorStyleLayer style)
        {
            Point = point;
            Style = style;

            Class = feature.Tags.ContainsKey("class") ? feature.Tags["class"].ToString() : string.Empty;
            Subclass = feature.Tags.ContainsKey("subclass") ? feature.Tags["subclass"].ToString() : string.Empty;
            Rank = feature.Tags.ContainsKey("rank") ? int.Parse(feature.Tags["rank"].ToString()) : 0;
        }

        public Point Point { get; }

        public IVectorStyleLayer Style;

        public abstract override void OnDraw(SKCanvas canvas, EvaluationContext context);
    }
}

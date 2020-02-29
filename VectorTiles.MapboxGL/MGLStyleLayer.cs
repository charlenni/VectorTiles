using System.Collections.Generic;
using VectorTiles.Filter;

namespace VectorTiles.MapboxGL
{
    public class MGLStyleLayer : IVectorStyleLayer
    {
        public string Id { get; internal set; }

        public float MinZoom { get; internal set; }

        public float MaxZoom { get; internal set; }

        public string SourceLayer { get; internal set; }

        public StyleType Type { get; internal set; }

        public IFilter Filter { get; internal set; }

        public IEnumerable<IVectorPaint> Paints { get; internal set; } = new List<MGLPaint>();

        public bool IsVisible { get; internal set; } = true;

        public MGLStyleLayer()
        {
        }
    }
}

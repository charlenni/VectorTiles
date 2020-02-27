namespace VectorTiles.Filter
{
    public class TypeEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && feature.Type.Equals(Type);
        }
    }
}

namespace VectorTiles.Filter
{
    public class TypeNotEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeNotEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && !feature.Type.Equals(Type);
        }
    }
}

namespace VectorTiles.Filter
{
    public class HasIdentifierFilter : Filter
    {
        public HasIdentifierFilter()
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && !string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}

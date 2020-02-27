namespace VectorTiles.Filter
{
    public class NotHasIdentifierFilter : Filter
    {
        public NotHasIdentifierFilter()
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}

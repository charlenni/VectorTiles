namespace VectorTiles.Filter
{
    public class NotHasIdentifierFilter : Filter
    {
        public NotHasIdentifierFilter()
        {
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}

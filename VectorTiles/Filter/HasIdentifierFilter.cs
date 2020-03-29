namespace VectorTiles.Filter
{
    public class HasIdentifierFilter : Filter
    {
        public HasIdentifierFilter()
        {
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && !string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}

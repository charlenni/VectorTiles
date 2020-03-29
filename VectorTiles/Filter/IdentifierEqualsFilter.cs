namespace VectorTiles.Filter
{
    public class IdentifierEqualsFilter : Filter
    {
        public string Identifier { get; }

        public IdentifierEqualsFilter(string identifier)
        {
            Identifier = identifier;
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && feature.Id == Identifier;
        }
    }
}

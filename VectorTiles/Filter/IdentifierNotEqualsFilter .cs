namespace VectorTiles.Filter
{
    public class IdentifierNotEqualsFilter : Filter
    {
        public string Identifier { get; }

        public IdentifierNotEqualsFilter(string identifier)
        {
            Identifier = identifier;
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && feature.Id != Identifier;
        }
    }
}

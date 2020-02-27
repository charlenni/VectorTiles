namespace VectorTiles.Filter
{
    public class HasFilter : Filter
    {
        public string Key { get; }

        public HasFilter(string key)
        {
            Key = key;
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && feature.Tags.ContainsKey(Key);
        }
    }
}

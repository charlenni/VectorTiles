namespace VectorTiles.Filter
{
    public class NotHasFilter : Filter
    {
        public string Key { get; }

        public NotHasFilter(string key)
        {
            Key = key;
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && !feature.Tags.ContainsKey(Key);
        }
    }
}

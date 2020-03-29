namespace VectorTiles.Filter
{
    public class NotEqualsFilter : Filter
    {
        public string Key { get; }
        public object Value { get; }

        public NotEqualsFilter(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public override bool Evaluate(VectorElement feature)
        {
            return feature != null && !feature.Tags.ContainsKeyValue(Key, Value);
        }
    }
}

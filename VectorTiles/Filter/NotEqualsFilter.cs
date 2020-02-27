using Newtonsoft.Json.Linq;

namespace VectorTiles.Filter
{
    public class NotEqualsFilter : Filter
    {
        public string Key { get; }
        public JValue Value { get; }

        public NotEqualsFilter(string key, JValue value)
        {
            Key = key;
            Value = value;
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && !feature.Tags.ContainsKeyValue(Key, Value);
        }
    }
}

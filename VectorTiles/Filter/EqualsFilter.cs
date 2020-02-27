using Newtonsoft.Json.Linq;

namespace VectorTiles.Filter
{
    public class EqualsFilter : Filter
    {
        public string Key { get; }
        public JValue Value { get; }

        public EqualsFilter(string key, JValue value)
        {
            Key = key;
            Value = value;
        }
        
        public override bool Evaluate(VectorTileFeature feature)
        {
            return feature != null && feature.Tags.ContainsKeyValue(Key, Value);
        }
    }
}

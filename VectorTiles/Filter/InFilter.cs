using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VectorTiles.Filter
{
    public class InFilter : Filter
    {
        public string Key { get; }
        public IList<JValue> Values { get; }

        public InFilter(string key, IEnumerable<JValue> values)
        {
            Key = key;
            Values = new List<JValue>();

            foreach(var value in values)
                Values.Add(value);
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            if (feature == null || !feature.Tags.ContainsKey(Key) || feature.Tags[Key] == null)
                return false;

            foreach (var value in Values)
            {
                if (feature.Tags[Key].Equals(value))
                    return true;
            }

            return false;
        }
    }
}

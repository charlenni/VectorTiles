using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VectorTiles.Filter
{
    public class NotInFilter : Filter
    {
        public string Key { get; }
        public JArray Values { get; }

        public NotInFilter(string key, IEnumerable<JValue> values)
        {
            Key = key;
            Values = new JArray();

            foreach (var value in values)
                Values.Add(value);
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            if (feature == null || !feature.Tags.ContainsKey(Key) || feature.Tags[Key] == null)
                return true;

            foreach (var value in Values)
            {
                if (feature.Tags[Key].Equals(value))
                    return false;
            }

            return true;
        }
    }
}

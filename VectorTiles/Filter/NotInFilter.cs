using System.Collections.Generic;

namespace VectorTiles.Filter
{
    public class NotInFilter : Filter
    {
        public string Key { get; }
        public List<object> Values { get; }

        public NotInFilter(string key, IEnumerable<object> values)
        {
            Key = key;
            Values = new List<object>();

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

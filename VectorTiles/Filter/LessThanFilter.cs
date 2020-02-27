using Newtonsoft.Json.Linq;

namespace VectorTiles.Filter
{
    public class LessThanFilter : BinaryFilter
    {
        public LessThanFilter(string key, JValue value) : base(key, value)
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            if (feature == null || !feature.Tags.ContainsKey(Key))
                return false;

            if (feature.Tags[Key].Type == JTokenType.Float ||
                feature.Tags[Key].Type == JTokenType.Integer)
                return (float) feature.Tags[Key] < (float) Value;

            return false;
        }
    }
}

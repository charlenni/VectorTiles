namespace VectorTiles.Filter
{
    public class LessThanFilter : BinaryFilter
    {
        public LessThanFilter(string key, object value) : base(key, value)
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            if (feature == null || !feature.Tags.ContainsKey(Key))
                return false;

            if (feature.Tags[Key] is float ||
                feature.Tags[Key] is int)
                return (float) feature.Tags[Key] < (float) Value;

            return false;
        }
    }
}

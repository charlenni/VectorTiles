namespace VectorTiles.Filter
{
    public class LessThanFilter : BinaryFilter
    {
        public LessThanFilter(string key, object value) : base(key, value)
        {
        }

        public override bool Evaluate(VectorElement feature)
        {
            if (feature == null || !feature.Tags.ContainsKey(Key))
                return false;

            if (feature.Tags[Key] is float)
                return (float)feature.Tags[Key] < (float)Value;

            if (feature.Tags[Key] is long)
                return (long)feature.Tags[Key] < (long)Value;

            return false;
        }
    }
}

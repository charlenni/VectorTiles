namespace VectorTiles.Filter
{
    public abstract class Filter : IFilter
    {
        public abstract bool Evaluate(VectorElement feature);
    }
}

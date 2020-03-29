namespace VectorTiles.Filter
{
    public interface IFilter
    {
        bool Evaluate(VectorElement feature);
    }
}

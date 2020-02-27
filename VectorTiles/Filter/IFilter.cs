namespace VectorTiles.Filter
{
    public interface IFilter
    {
        bool Evaluate(VectorTileFeature feature);
    }
}

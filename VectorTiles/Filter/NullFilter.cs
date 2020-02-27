namespace VectorTiles.Filter
{
    public class NullFilter : Filter
    {
        public override bool Evaluate(VectorTileFeature feature)
        {
            return true;
        }
    }
}

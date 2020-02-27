namespace VectorTiles.Filter
{
    public class ExpressionFilter : Filter
    {
        public override bool Evaluate(VectorTileFeature feature)
        {
            return false;
        }
    }
}

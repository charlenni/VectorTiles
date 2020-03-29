namespace VectorTiles.Filter
{
    public class NullFilter : Filter
    {
        public override bool Evaluate(VectorElement feature)
        {
            return true;
        }
    }
}

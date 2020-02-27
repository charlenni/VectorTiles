using System.Collections.Generic;

namespace VectorTiles.Filter
{
    public class AnyFilter : CompoundFilter
    {
        public AnyFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            foreach (var filter in Filters)
            {
                if (filter.Evaluate(feature))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

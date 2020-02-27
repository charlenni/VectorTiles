﻿using System.Collections.Generic;

namespace VectorTiles.Filter
{
    public class NoneFilter : CompoundFilter
    {
        public NoneFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            foreach (var filter in Filters)
            {
                if (filter.Evaluate(feature))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

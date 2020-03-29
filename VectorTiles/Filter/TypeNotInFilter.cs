using System.Collections.Generic;

namespace VectorTiles.Filter
{
    public class TypeNotInFilter : Filter
    {
        public IList<GeometryType> Types { get; }

        public TypeNotInFilter(IEnumerable<GeometryType> types)
        {
            Types = new List<GeometryType>();

            foreach (var type in types)
                Types.Add(type);
        }

        public override bool Evaluate(VectorElement feature)
        {
            if (feature == null)
                return true;

            foreach (var type in Types)
            {
                if (feature.Type.Equals(type))
                    return false;
            }

            return true;
        }
    }
}

using System.Collections.Generic;

namespace VectorTiles.Filter
{
    public class IdentifierNotInFilter : Filter
    {
        public IList<string> Identifiers { get; }

        public IdentifierNotInFilter(IEnumerable<string> identifiers)
        {
            Identifiers = new List<string>();

            foreach (var identifier in identifiers)
                Identifiers.Add(identifier);
        }

        public override bool Evaluate(VectorTileFeature feature)
        {
            if (feature == null)
                return true;

            foreach (var identifier in Identifiers)
            {
                if (feature.Id == identifier)
                    return false;
            }

            return true;
        }
    }
}

using VectorTiles.Filter;
using System.Collections.Generic;

namespace VectorTiles
{
    public interface IVectorStyleLayer
    {
        /// <summary>
        /// Minimal zoom from which this style is used
        /// </summary>
        float MinZoom { get; }

        /// <summary>
        /// Maximal zoom up to which this style is used
        /// </summary>
        float MaxZoom { get; }

        /// <summary>
        /// Name of source layer this style belongs to 
        /// </summary>
        string SourceLayer { get; }

        /// <summary>
        /// Type of this style
        /// </summary>
        StyleType Type { get; }

        /// <summary>
        /// Filter used for this style
        /// </summary>
        IFilter Filter { get; }

        /// <summary>
        /// Paint to use to draw the features
        /// </summary>
        IEnumerable<IVectorPaint> Paints { get; }
    }
}

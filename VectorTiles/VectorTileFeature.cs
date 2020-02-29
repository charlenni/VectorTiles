using NetTopologySuite.Geometries;
using System;

namespace VectorTiles
{
    public class VectorTileFeature
    {
        private string v;

        public VectorTileFeature(string layerName, string v)
        {
            this.Layer = layerName;
            this.v = v;
        }

        /// <summary>
        /// ID of this feature
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Layer to which this feature belongs
        /// </summary>
        public string Layer { get; }

        /// <summary>
        /// Geometry of this feature
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Type from geometry of this feature
        /// </summary>
        public GeometryType Type => (GeometryType) Enum.Parse(typeof(GeometryType), Geometry.GeometryType);

        /// <summary>
        /// Tags of this feature
        /// </summary>
        public TagsCollection Tags { get; set; } = new TagsCollection();

        public uint Extent { get; set; }
    }
}


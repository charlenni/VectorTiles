using System.Collections.Generic;

namespace VectorTiles.MapboxGL
{
    /// <summary>
    /// Class holding all relevant data from the Mapbox GL Json Style File
    /// </summary>
    public class MGLStyleFile
    {
        public MGLStyleFile(string name, int version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Name of this style file
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Version of this style file
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Center of map as provided in the style file
        /// </summary>
        public GeoPoint Center { get; internal set; }

        /// <summary>
        /// Sources is a list of all IDrawableTileSources, that this style file provides
        /// </summary>
        public List<IDrawableTileSource> TileSources { get; } = new List<IDrawableTileSource>();

        /// <summary>
        /// SpriteAtlas containing all sprites of this style file
        /// </summary>
        public MGLSpriteAtlas SpriteAtlas { get; } = new MGLSpriteAtlas();

        /// <summary>
        /// GlyphAtlas containing all glyphs for this style file
        /// </summary>
        public object GlyphAtlas { get; internal set; }
    }
}

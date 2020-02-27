using BruTile;
using SkiaSharp;
using System.Collections.Generic;

namespace VectorTiles
{
    /// <summary>
    /// TileSource, which could deliver drawables
    /// </summary>
    /// <remarks>
    /// A normal TileSource delivers for raster images byte arrays with image data and for
    /// vector images the data with the features. This data has than to be rendered, so that
    /// it could be displayed on screen. This is done by classes that implement this interface.
    /// So a IDrawableTileSource could provide byte arrays with images and drawables for the 
    /// same tile.
    /// </remarks>
    public interface IDrawableTileSource : ITileSource
    {
        /// <summary>
        /// Creates a SKDrawable for a given tile
        /// </summary>
        /// <param name="tileInfo">Tile to use</param>
        /// <returns>SKDrawable for given tile and a list of symbols</returns>
        (Drawable, List<object>) GetDrawable(TileInfo tileInfo);
    }
}

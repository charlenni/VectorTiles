using BruTile;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using VectorTiles.MapboxGL.Extensions;

namespace VectorTiles.MapboxGL
{
    public class MGLRasterTileSource : IDrawableTileSource
    {
        public string Name { get; }

        public double MinVisible {get; set;}

        public double MaxVisible { get; set; }

        public int TileSize { get; set; }

        /// <summary>
        /// Tile source that provides the content of the tiles. In this case, it provides byte[] with images.
        /// </summary>
        public ITileSource Source { get; }

        /// <summary>
        /// Style to use for drawing images
        /// </summary>
        public IVectorStyle Style { get; set; }

        public ITileSchema Schema => Source.Schema;

        public Attribution Attribution => Source.Attribution;

        public MGLRasterTileSource(string name, ITileSource source)
        {
            Name = name;
            Source = source;
        }

        (Drawable, List<object>) IDrawableTileSource.GetDrawable(TileInfo ti)
        {
            // Check Schema for TileInfo
            var tileInfo = Schema.YAxis == YAxis.OSM ? ti.ToTMS() : ti;

            try
            {
                var bytes = Source.GetTile(tileInfo);
                var image = SKImage.FromEncodedData(bytes);

                var result = new RasterTile(TileSize, image, Style.Paints.FirstOrDefault<IVectorPaint>());

                return (result, null);
            }
            catch (Exception e)
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Provides the image as byte array
        /// </summary>
        /// <param name="tileInfo">TileInfo for tile to get</param>
        /// <returns>Image as byte array</returns>
        public byte[] GetTile(TileInfo tileInfo)
        {
            return Source.GetTile(tileInfo);
        }
    }
}

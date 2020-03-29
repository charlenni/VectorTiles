using BruTile;
using SkiaSharp;
using System.Collections.Generic;
using VectorTiles.MapboxGL.Extensions;

namespace VectorTiles.MapboxGL
{
    /// <summary>
    /// TileSource for background
    /// </summary>
    /// <remarks>
    /// Background tiles have always size 256 and are, except the fill with the background paint, empty.
    /// It implements a SKDrawable, because it doesn't make sense to create an extra class for this.
    /// </remarks>
    public class MGLBackgroundTileSource : Drawable, IDrawableTileSource
    {
        public ITileSchema Schema { get; }

        public string Name => "Background";

        public Attribution Attribution { get; } = new Attribution();

        public int TileSize { get; set; } = 256;

        /// <summary>
        /// MGLPaint to use when drawing background
        /// </summary>
        public MGLPaint BackgroundPaint { get; internal set; }

        public MGLBackgroundTileSource()
        {
            var schema = new TileSchema();
            schema.Extent = new Extent(-20037508, -34662080, 20037508, 34662080);
            Schema = schema;

            for (var i = 0; i <= 30; i++)
                Schema.Resolutions.Add(i.ToString(), new BruTile.Resolution(i.ToString(), i.ToResolution()));
        }

        public Drawable GetVectorTile(TileInfo tileInfo)
        {
            return this;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            // All we have to do here is to clear canvas and, if there is one, draw the fill image
            var paint = BackgroundPaint.CreatePaint(Context);

            canvas.Clear(paint.Color);

            if (paint.Shader != null)
                canvas.DrawRect(OnGetBounds(), paint);
        }

        protected override SKRect OnGetBounds()
        {
            return new SKRect(0, 0, TileSize, TileSize);
        }
    }
}

using SkiaSharp;

namespace VectorTiles
{
    /// <summary>
    /// Class holding all tile informations, so that it could be drawn to a canvas
    /// </summary>
    public class RasterTile : Drawable
    {
        /// <summary>
        ///  Tile size of this vector tile
        /// </summary>
        public int TileSize { get; }

        public SKImage Image { get; }

        public IVectorPaint Paint { get; }

        public RasterTile(int tileSize, SKImage image, IVectorPaint paint)
        {
            TileSize = tileSize;
            Image = image;
            Paint = paint;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            canvas.Save();
            //canvas.ClipRect(canvas.LocalClipBounds);

            var paint = Paint.CreatePaint(Context);
            canvas.DrawImage(Image, new SKPoint(0, 0), Paint.CreatePaint(Context));

            canvas.Restore();
        }

        /// <summary>
        /// Return the bounds of the tile
        /// </summary>
        /// <returns></returns>
        protected override SKRect OnGetBounds()
        {
            return new SKRect(0, 0, (float)TileSize, (float)TileSize);
        }
    }
}

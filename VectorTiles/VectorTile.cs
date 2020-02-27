using SkiaSharp;
using System.Collections.Generic;

namespace VectorTiles
{
    /// <summary>
    /// Class holding all tile informations, so that it could be drawn to a canvas
    /// </summary>
    public class VectorTile : Drawable
    {
        /// <summary>
        ///  Tile size of this vector tile
        /// </summary>
        public double TileSize { get; }

        /// <summary>
        /// Bucket holding predefined path-paint-pairs 
        /// </summary>
        public List<PathPaintPair> PathPaintBucket { get; } = new List<PathPaintPair>();

        /// <summary>
        /// Bucket holding paints for all layers
        /// </summary>
        List<IVectorPaint> PaintBucket { get; } = new List<IVectorPaint>();

        /// <summary>
        /// Bucket holding all drawables
        /// </summary>
        List<SKDrawable> DrawableBucket { get; } = new List<SKDrawable>();

        /// <summary>
        /// Bucket for symbols
        /// </summary>
        // TODO
        //List<Symbol> SymbolBucket { get; } = new List<Symbol>();

        /// <summary>
        /// Bucket holding all path texts
        /// </summary>
        // TODO
        //List<PathText> PathTextBucket { get; } = new List<PathText>();

        public VectorTile(double tileSize, int overzoom)
        {
            TileSize = tileSize;
            Overzoom = overzoom;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            // Do this, because of the different sizes of tiles (512 instead of 256) and overzoom
            var context = new EvaluationContext(Context.Zoom + Overzoom)
            {
                Tags = Context.Tags,
            };

            canvas.Save();

            foreach (var pair in PathPaintBucket)
            {
                var paint = pair.Paint.CreatePaint(context);

                //var test = true;
                //if (paint.Style == SKPaintStyle.Fill)
                //{
                //    if (paint.Color.Red == 158 && paint.Color.Blue == 255)
                //        paint.Color = SKColors.Red;
                //}

                canvas.DrawPath(pair.Path, paint);
            }

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

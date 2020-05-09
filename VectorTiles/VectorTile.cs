using BruTile;
using SkiaSharp;
using System.Collections.Generic;
using VectorTiles.Enums;

using Rect = SkiaSharp.SKRect;

namespace VectorTiles
{
    /// <summary>
    /// Class holding all tile informations, so that it could be drawn to a canvas
    /// </summary>
    public class VectorTile : Drawable, ITileDataSink
    {
        Rect clipRect;
        EvaluationContext context;

        /// <summary>
        /// TileInfo for this tile
        /// </summary>
        public TileInfo TileInfo { get; }

        /// <summary>
        ///  Tile size of this vector tile
        /// </summary>
        public int TileSize { get; }

        public List<IVectorStyleLayer> StyleLayers { get; }

        public IBucket[] Buckets;

        /// <summary>
        /// Bucket for symbols
        /// </summary>
        // TODO
        public List<List<ISymbol>> SymbolBucket { get; } = new List<List<ISymbol>>();

        /// <summary>
        /// Bucket holding all path texts
        /// </summary>
        // TODO
        //List<PathText> PathTextBucket { get; } = new List<PathText>();

        public VectorTile(TileInfo tileInfo, int tileSize, List<IVectorStyleLayer> styles, int zoom, int overzoom)
        {
            TileInfo = tileInfo;
            TileSize = tileSize;
            StyleLayers = styles;
            Buckets = new IBucket[StyleLayers.Count];
            Zoom = zoom;
            Overzoom = overzoom;

            clipRect = new Rect(-1, -1, TileSize + 1, TileSize + 1);
            context = new EvaluationContext(Zoom);
        }

        public void Process(VectorElement element)
        {
            IVectorStyleLayer style;

            element.Scale(TileSize / 4096.0f);

            // Now process this element and check, for which style layers it is ok
            for (int i = 0; i < StyleLayers.Count; i++)
            {
                style = StyleLayers[i];

                // Is this style relevant or is it outside the zoom range
                if (!style.IsVisible || style.MinZoom > Zoom || style.MaxZoom < Zoom)
                    continue;

                // Is this style layer relevant for this feature?
                if (style.SourceLayer != element.Layer)
                    continue;

                // TODO: Remove, only for testing
                if (style.Type == StyleType.Symbol && style.SourceLayer == "poi")
                {
                    var name = style.SourceLayer;
                }

                // Fullfill element filter for this style layer
                if (!style.Filter.Evaluate(element))
                    continue;

                // Check for different types
                switch (style.Type)
                {
                    case StyleType.Symbol:
                        // Feature is a symbol
                        if (Buckets[i] == null)
                            Buckets[i] = new SymbolBucket(style);
                        ((SymbolBucket)Buckets[i]).AddElement(element, context);
                        break;
                    case StyleType.Line:
                    case StyleType.Fill:
                        // Element is a line or fill
                        if (Buckets[i] == null)
                            Buckets[i] = new PathBucket(style.Paints);
                        if (element.IsLine || element.IsPolygon)
                            ((PathBucket)Buckets[i]).AddElement(element);
                        else
                        {
                            // This are things like height of a building
                            // We don't use this up to now
                            //System.Diagnostics.Debug.WriteLine(element.Tags.ToString());
                        }
                        break;
                    default:
                        // throw new Exception("Unknown style type");
                        break;
                }
            }
        }

        public void Completed(QueryResult result)
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            canvas.Save();

            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            foreach (var bucket in Buckets)
            {
                if (bucket != null)
                    bucket.OnDraw(canvas, Context);
            }

            //watch.Stop();
            //System.Diagnostics.Debug.WriteLine($"Draw VectorTile ({TileInfo.Index.Level}/{TileInfo.Index.Col}/{TileInfo.Index.Row}): {watch.ElapsedMilliseconds}");

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

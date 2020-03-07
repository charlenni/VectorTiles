using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using System.Threading.Tasks;
using Mapsui.Logging;
using Mapsui.Providers;
using VectorTiles;

namespace Mapsui.Samples.Forms
{
    public class DrawableTileProvider : IProvider
    {
        readonly IDrawableTileSource source;
        readonly MemoryCache<Drawable> drawables = new MemoryCache<Drawable>(100, 200);
        readonly List<TileIndex> queue = new List<TileIndex>();

        public BoundingBox GetExtents()
        {
            return source.Schema.Extent.ToBoundingBox();
        }

        public string CRS { get; set; }

        public DrawableTileProvider(IDrawableTileSource tileSource)
        {
            source = tileSource;
        }

        public IEnumerable<IFeature> FetchTiles(BoundingBox boundingBox, double resolution)
        {
            var extent = new Extent(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.X, boundingBox.Max.Y);
            var levelId = BruTile.Utilities.GetNearestLevel(source.Schema.Resolutions, resolution);
            var infos = source.Schema.GetTileInfos(extent, levelId).ToList();

            ICollection<WaitHandle> waitHandles = new List<WaitHandle>();

            foreach (TileInfo info in infos)
            {
                if (drawables.Find(info.Index) != null) continue;
                if (queue.Contains(info.Index)) continue;
                var waitHandle = new AutoResetEvent(false);
                waitHandles.Add(waitHandle);
                queue.Add(info.Index);
                Task.Run(() => GetTileOnThread(new object[] { source, info, drawables, waitHandle }));
            }

            try
            {
                WaitHandle.WaitAll(waitHandles.ToArray());
            }
            catch (Exception ex)
            { }

            IFeatures features = new Features();
            foreach (TileInfo info in infos)
            {
                Drawable drawable = drawables.Find(info.Index);
                if (drawable == null) continue;
                IGeometry drawableTile = new DrawableTile(drawable, new BoundingBox(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY));
                IFeature feature = features.New();
                feature.Geometry = drawableTile;
                features.Add(feature);
            }
            return features;
        }

        private void GetTileOnThread(object parameter) // This could accept normal parameters now we use PCL Profile111
        {
            var parameters = (object[])parameter;
            if (parameters.Length != 4) throw new ArgumentException("Four parameters expected");
            var source = (IDrawableTileSource)parameters[0];
            var tileInfo = (TileInfo)parameters[1];
            var drawables = (MemoryCache<Drawable>)parameters[2];
            var autoResetEvent = (AutoResetEvent)parameters[3];

            if (tileInfo == null)
                System.Diagnostics.Debug.WriteLine($"TileInfo was null");

            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetch: {tileInfo.Index.Level}/{tileInfo.Index.Col}/{tileInfo.Index.Row}");
                drawables.Add(tileInfo.Index, source.GetDrawable(tileInfo));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                // todo: report back through callback
                System.Diagnostics.Debug.WriteLine($"GetTileOnThread: {ex.Message}");
            }
            finally
            {
                queue.Remove(tileInfo.Index);
                autoResetEvent.Set();
            }
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return FetchTiles(box, resolution);
        }
    }
}

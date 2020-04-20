using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Rendering;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using VectorTiles;
using VectorTiles.MapboxGL;
using VectorTiles.MapboxGL.Extensions;
using VectorTiles.RBush;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var filename = "monaco.mbtiles";
            string dir = "";
#if WINDOWS_UWP
            dir = Directory.GetCurrentDirectory();
#endif
#if __ANDROID__
            dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
#if __IOS__
            dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // Save for use of style loader for mbtiles files
            MGLStyleLoader.DirectoryForFiles = dir;
            filename = Path.Combine(dir, filename);
            if (!File.Exists(filename))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var resourceName = resourceNames.FirstOrDefault(s => s.ToLower().EndsWith("monaco.mbtiles") == true);
                if (resourceName != null)
                {
                    var stream  = assembly.GetManifestResourceStream(resourceName);
                    using (var file = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(file);
                    }
                }
            }

            // Create map
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation(),
                BackColor = new Mapsui.Styles.Color(239, 239, 239),
                RotationLock = true,
                Home = n => n.NavigateTo(new Geometries.Point(825650.0, 5423050.0).BoundingBox),
            };
            //map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom, MarginY = 20 });
            ((ViewportLimiter)map.Limiter).ZoomMode = ZoomMode.Unlimited; //.ZoomLimits = new MinMax(0, 24); //

            mapView.Map = map;
            mapView.MyLocationLayer.Enabled = false;
            //mapView.Navigator = new AnimatedNavigator(map, (IViewport)mapView.Viewport);
            mapView.Viewport.ViewportChanged += (s, args) => { Device.BeginInvokeOnMainThread(() => { lblInfo.Text = $"Zoom Level: {mapView.Viewport.Resolution.ToZoomLevel():0.0}"; }); };
            mapView.UseDoubleTap = false;


            // Get Mapbox GL Style File
            var mglStyleFile = CreateMGLStyleFile();

            if (mglStyleFile == null)
            {
                map.Layers.Add(OpenStreetMap.CreateTileLayer());
                return;
            }

            mapView.Renderer.StyleRenderers.Add(typeof(DrawableTileStyle), new DrawableTileStyleRenderer());

            // Ok, we have a valid style file, so get the tile sources, which contain the style file
            foreach (var tileSource in mglStyleFile.TileSources)
            {
                if (tileSource is MGLBackgroundTileSource)
                {
                    var layer = new TileLayer(tileSource, fetchStrategy: new FetchStrategy(0), fetchToFeature: DrawableTile.DrawableTileToFeature, fetchGetTile: tileSource.GetVectorTile);
                    layer.MinVisible = 30.ToResolution();
                    layer.MaxVisible = 0.ToResolution();
                    layer.Style = new DrawableTileStyle();
                    map.Layers.Add(layer);
                }

                if (tileSource is MGLVectorTileSource)
                {
                    var layer = new TileLayer(tileSource, fetchStrategy: new FetchStrategy(30), fetchToFeature: DrawableTile.DrawableTileToFeature, fetchGetTile: tileSource.GetVectorTile);
                    layer.MinVisible = 30.ToResolution();
                    layer.MaxVisible = 0.ToResolution();
                    layer.Style = new DrawableTileStyle();
                    layer.DataChanged += (s, args) =>
                    {
                        if (layer.Busy)
                            return;

                        // TODO: All tiles are loaded, so create buckets for symbols
                        var features = layer.GetFeaturesInView(mapView.Viewport.Extent, mapView.Viewport.Resolution);
                        var tiles = features.Select(f => (VectorTile)((DrawableTile)f.Geometry).Data).ToArray();

                        int numBuckets = 0;
                        int numSymbols = 0;

                        var symbols = new List<Symbol>();
                        var rbush = new RBush<Symbol>();

                        // Check symbols, if they are visible or not. For this,
                        // we use the same layer for all tiles and set as much as
                        // possible symbols to visible. Than we take the next
                        // layer and begin the next check.
                        for (int i = 0; i < tiles[0].Buckets.Length; i++)
                        {
                            if (tiles[0].StyleLayers[i].Type != StyleType.Symbol)
                                continue;

                            numBuckets++;

                            symbols.Clear();

                            foreach (var tile in tiles)
                            {
                                if (tile.Buckets[i] == null)
                                    continue;

                                symbols.AddRange(((SymbolBucket)tile.Buckets[i]).Symbols);

                                numSymbols += ((SymbolBucket)tile.Buckets[i]).Symbols.Count;
                            }

                            if (symbols.Count == 0)
                                continue;

                            // Now we have all symbols of one style layer, that should be 
                            // visible. So we could check, which symbol should be visible 
                            // and which not.
                            foreach (var symbol in symbols)
                            {
                                //if (symbol.Envelope != SKRect.Empty && rbush.Search(symbol.Envelope).Count == 0)
                                {
                                    symbol.IsVisible = true;
                                    rbush.Insert(symbol);
                                }
                            }

                            System.Diagnostics.Debug.WriteLine($"Found {numSymbols} symbols in {numBuckets} buckets of {tiles.Length} tiles");
                        };

                    };

                    map.Layers.Add(layer);
                }

                //if (tileSource is MGLRasterTileSource)
                //{
                //    var layer = new TileLayer(tileSource, fetchStrategy: new FetchStrategy(3), fetchToFeature: DrawableTile.DrawableTileToFeature, fetchGetTile: tileSource.GetVectorTile);
                //    layer.MinVisible = tileSource.Schema.Resolutions.Last().Value.UnitsPerPixel;
                //    layer.MaxVisible = tileSource.Schema.Resolutions.First().Value.UnitsPerPixel;
                //    layer.Style = new DrawableTileStyle();
                //    map.Layers.Add(layer);
                //}
            }

            mapView.Navigator.NavigateTo(new Geometries.Point(825650.0, 5423050.0), 12.ToResolution());

            lblInfo.Text = "";

            btnZoomIn.Clicked += (s, e) =>
            {
                if (mapView.Navigator is AnimatedNavigator animatedNavi)
                    animatedNavi.ZoomIn();
                else
                    mapView.Navigator.ZoomIn();
            };

            btnZoomOut.Clicked += (s, e) =>
            {
                if (mapView.Navigator is AnimatedNavigator animatedNavi)
                    animatedNavi.ZoomOut();
                else
                    mapView.Navigator.ZoomOut();
            };

            btnFlight.Clicked += (s, e) =>
            {
                if (mapView.Navigator is AnimatedNavigator animatedNavi)
                    animatedNavi.FlyTo(new Geometries.Point(825650.0, 5423050.0), 2.ToResolution(), 10000);
            };
        }

        public MGLStyleFile CreateMGLStyleFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(s => s.ToLower().EndsWith("styles.osm-liberty.json") == true);

            MGLStyleFile result;

            if (string.IsNullOrEmpty(resourceName))
                return null;

            // Open JSON style files and read contents
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                // Open JSON style files and read contents
                result = MGLStyleLoader.Load(stream);
            }

            return result;
        }
    }
}

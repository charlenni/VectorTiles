using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VectorTiles.MapboxGL;
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

            // Create map
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };
            map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom, MarginY = 20 });

            mapView.Map = map;
            mapView.Navigator = new AnimatedNavigator(map, (IViewport)mapView.Viewport);
            mapView.BackgroundColor = new Color(239, 239, 239);

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
                if (tileSource is MGLVectorTileSource)
                {
                    var provider = new DrawableTileProvider(tileSource);
                    var layer = new MemoryLayer(tileSource.Name) { DataSource = provider };
                    layer.Style = new DrawableTileStyle();
                    map.Layers.Add(layer);
                }
            }

            mapView.Navigator.NavigateTo(new Geometries.Point(825650.0, 5423050.0), 1.0);

            lblInfo.Text = "";

            btnZoomIn.Clicked += (s, e) =>
            {
                ((AnimatedNavigator)mapView.Navigator).ZoomIn();
            };

            btnZoomOut.Clicked += (s, e) =>
            {
                ((AnimatedNavigator)mapView.Navigator).ZoomOut();
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

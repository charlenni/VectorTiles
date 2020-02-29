using BruTile;
using BruTile.MbTiles;
using BruTile.Predefined;
using BruTile.Web;
using VectorTiles.Filter;
using VectorTiles.MapboxGL.Converter;
using VectorTiles.MapboxGL.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using SkiaSharp;

namespace VectorTiles.MapboxGL
{
    /// <summary>
    /// Class for loading Mapbox GL Json style files
    /// </summary>
    public static class MGLStyleLoader
    {
        /// <summary>
        /// Load a Mapbox GL Json style file
        /// </summary>
        /// <param name="input">Stream with Mapbox GL Json style file</param>
        /// <returns>Mapbox GL Style File</returns>
        public static MGLStyleFile Load(Stream input)
        {
            JsonStyleFile jsonStyle;

            using (var reader = new StreamReader(input))
                jsonStyle = JsonConvert.DeserializeObject<JsonStyleFile>(reader.ReadToEnd());

            var mglStyleFile = new MGLStyleFile(jsonStyle.Name, jsonStyle.Version)
            {

                // Extract the main part from the JSON file
                Center = jsonStyle.Center != null ? new Coordinate(jsonStyle.Center[0], jsonStyle.Center[1]) : new Coordinate()
            };

            // Extract sprite atlas
            ExtractSprites(jsonStyle, mglStyleFile.SpriteAtlas);

            // Extract glyphs
            ExtractGlyphs(jsonStyle, mglStyleFile.GlyphAtlas);

            // Extract background
            var backgroundSource = CreateBackgroundTileSource(jsonStyle.StyleLayers, mglStyleFile.SpriteAtlas);
            if (backgroundSource != null)
                mglStyleFile.TileSources.Add(backgroundSource);

            // Extract all sources in this JSON file
            mglStyleFile.TileSources.AddRange(ExtractSources(jsonStyle, jsonStyle.Sources, mglStyleFile.SpriteAtlas));

            return mglStyleFile;
        }

        private static void ExtractSprites(JsonStyleFile jsonStyle, MGLSpriteAtlas spriteAtlas)
        {
            // Save urls for sprite and glyphs
            var spriteUrl = jsonStyle.Sprite;

            if (!string.IsNullOrEmpty(spriteUrl))
            {
                spriteAtlas.AddSpriteSource(spriteUrl);
            }
        }

        private static void ExtractGlyphs(JsonStyleFile jsonStyle, object glyphAtlas)
        {
            // Save urls for glyphs
            var glyphsUrl = jsonStyle.Glyphs;

            if (!string.IsNullOrEmpty(glyphsUrl))
            {
                // Not implemented yet
            }
        }

        /// <summary>
        /// Extract all given sources (background, raster, vector and so on) given by this JSON file
        /// </summary>
        private static List<IDrawableTileSource> ExtractSources(JsonStyleFile jsonStyle, Dictionary<string, JsonSource> sources, MGLSpriteAtlas spriteAtlas)
        {
            List<IDrawableTileSource> tileSources = new List<IDrawableTileSource>();

            // Read all tile sources
            foreach (var source in sources)
            {
                var jsonSource = source.Value;

                switch (source.Value.Type)
                {
                    case "raster":
                        var rasterSource = CreateRasterTileSource(jsonStyle, source.Key, source.Value, spriteAtlas);
                        if (rasterSource != null)
                            tileSources.Add(rasterSource);
                        break;
                    case "vector":
                        var vectorSource = CreateVectorTileSource(jsonStyle, source.Key, source.Value, spriteAtlas);
                        if (vectorSource != null)
                            tileSources.Add(vectorSource);
                        break;
                    case "geoJson":
                        break;
                    case "image":
                        break;
                    case "video":
                        break;
                    default:
                        throw new ArgumentException($"{source.Value.Type} isn't a valid source");
                }
            }

            return tileSources;
        }

        private static IDrawableTileSource CreateBackgroundTileSource(IEnumerable<JsonStyleLayer> styleLayers, MGLSpriteAtlas spriteAtlas)
        {
            foreach (var styleLayer in styleLayers)
            {
                // styleLayer for background is independent from any source
                if (styleLayer.Type.ToLower().Equals("background"))
                {
                    // visibility
                    //   Optional enum. One of visible, none. Defaults to visible.
                    //   The display of this layer. none hides this layer.
                    if (styleLayer.Layout?.Visibility != "none" && styleLayer.Paint.BackgroundColor != null)
                    {
                        // Background should be visible, so create a MGLBackgroundSource
                        var backgroundSource = new MGLBackgroundTileSource();
                        backgroundSource.BackgroundPaint = StyleLayerConverter.ConvertBackgroundLayer(styleLayer, spriteAtlas).FirstOrDefault<MGLPaint>();
                        
                        return backgroundSource;
                    }
                }
            }

            // No background style found
            return null;
        }
        
        private static MGLRasterTileSource CreateRasterTileSource(JsonStyleFile jsonStyle, string name, JsonSource source, MGLSpriteAtlas spriteAtlas)
        {
            //if (!string.IsNullOrEmpty(source.Url))
            //{
            //    // Get TileJSON from http/https url
            //    jsonSource = new Source();
            //}

            // Create new raster layer
            var rasterSource = new MGLRasterTileSource(name, CreateTileSource(source))
            {
                TileSize = source?.TileSize ?? 256,
                MinVisible = source?.ZoomMin ?? 0,
                MaxVisible = source?.ZoomMax ?? 30,
            };

            // If we have a new raster tile source, than get style for this
            if (rasterSource != null)
            {
                // Add all ThemeStyles for this layer
                var style = ExtractStyles(rasterSource.Name, jsonStyle.StyleLayers, spriteAtlas).FirstOrDefault<IVectorStyleLayer>();
                if (style != null)
                {
                    // Replace color with white for opacity
                    ((MGLPaint)style.Paints.First<IVectorPaint>()).SetFixColor(SKColors.White);
                }
                rasterSource.Style = style;
            }

            return rasterSource;
        }

        private static IDrawableTileSource CreateVectorTileSource(JsonStyleFile jsonStyle, string name, JsonSource source, MGLSpriteAtlas spriteAtlas)
        {
            JsonSource jsonSource = null;

            if (!string.IsNullOrEmpty(source.Url))
            {
                if (source.Url.StartsWith("http"))
                {
                    // TODO: Get TileJSON from http/https url
                    jsonSource = JsonConvert.DeserializeObject<JsonSource>(@"{'tiles':['https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=tXiQqN3lIgskyDErJCeY'],'name':'OpenMapTiles','format':'pbf','basename':'v3.7.mbtiles','id':'openmaptiles','attribution':'<a href=\'http://www.openmaptiles.org/\' target=\'_blank\'>&copy; OpenMapTiles</a> <a href=\'http://www.openstreetmap.org/about/\' target=\'_blank\'>&copy; OpenStreetMap contributors</a>','description':'A tileset showcasing all layers in OpenMapTiles. http://openmaptiles.org','maxzoom':14,'minzoom':0,'pixel_scale':'256','vector_layers':[{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'water','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'waterway','description':''},{'maxzoom':14,'fields':{'class':'String','subclass':'String'},'minzoom':0,'id':'landcover','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'landuse','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:nl':'String','name:pl':'String','ele':'Number','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:ru':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ele_ft':'Number','name:ja_kana':'String','name:is':'String','osm_id':'Number','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'mountain_peak','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'park','description':''},{'maxzoom':14,'fields':{'admin_level':'Number','disputed':'Number','maritime':'Number'},'minzoom':0,'id':'boundary','description':''},{'maxzoom':14,'fields':{'ref':'String','class':'String'},'minzoom':0,'id':'aeroway','description':''},{'maxzoom':14,'fields':{'layer':'Number','service':'String','level':'Number','brunnel':'String','indoor':'Number','ramp':'Number','subclass':'String','oneway':'Number','class':'String'},'minzoom':0,'id':'transportation','description':''},{'maxzoom':14,'fields':{'render_min_height':'Number','render_height':'Number'},'minzoom':0,'id':'building','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'water_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','layer':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','subclass':'String','name:de':'String','indoor':'Number','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','network':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ref':'String','name:ja_kana':'String','level':'Number','ref_length':'Number','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'transportation_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','capital':'Number','name:sl':'String','name:lv':'String','name:ja':'String','name:ko_rm':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:th':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','iso_a2':'String','name:ja_kana':'String','name:is':'String','name:lt':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'place','description':''},{'maxzoom':14,'fields':{'housenumber':'String'},'minzoom':0,'id':'housenumber','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','subclass':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','agg_stop':'Number','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'poi','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:nl':'String','name:pl':'String','ele':'Number','name:lt':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:latin':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:ko_rm':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:ru':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:th':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ele_ft':'Number','name:ja_kana':'String','name:is':'String','osm_id':'Number','iata':'String','icao':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'aerodrome_label','description':''}],'center':[-12.2168,28.6135,4],'bounds':[-180,-85.0511,180,85.0511],'maskLevel':'8','planettime':'1523232000000','version':'3.7','tilejson':'2.0.0'}");
                    jsonSource.Name = name;
                }
                else if (source.Url.StartsWith("mbtiles"))
                {
                    var filename = source.Url.Substring(10);
                    var connection = new SQLiteConnectionString(filename, false);
                    var dataSource = new MbTilesTileSource(connection);
                    jsonSource = JsonConvert.DeserializeObject<JsonSource>(dataSource.Json);
                    jsonSource.Name = name;
                    jsonSource.Tiles = new List<string> { source.Url };
                    jsonSource.Scheme = dataSource.Schema.YAxis == YAxis.TMS ? "tms" : "osm";
                    jsonSource.Bounds = new JValue[]
                    {
                                    new JValue(dataSource.Schema.Extent.MinX),
                                    new JValue(dataSource.Schema.Extent.MinY),
                                    new JValue(dataSource.Schema.Extent.MaxX),
                                    new JValue(dataSource.Schema.Extent.MinX)
                    };
                }
            }

            if (jsonSource == null)
                return null;

            // Create new vector layer
            var tileSource = CreateTileSource(jsonSource);

            if (jsonSource.Bounds != null)
            {
                var left = jsonSource.Bounds[0].Type == JTokenType.Float ? (float)jsonSource.Bounds[0] : -180.0f;
                var bottom = jsonSource.Bounds[1].Type == JTokenType.Float ? (float)jsonSource.Bounds[1] : -85.0511f;
                var right = jsonSource.Bounds[2].Type == JTokenType.Float ? (float)jsonSource.Bounds[2] : 180.0f;
                var top = jsonSource.Bounds[3].Type == JTokenType.Float ? (float)jsonSource.Bounds[3] : 85.0511f;
            }

            var vectorTileSource = new MGLVectorTileSource(name, tileSource)
            {
                MinVisible = jsonSource?.ZoomMax ?? 30,
                MaxVisible = jsonSource?.ZoomMin ?? 0,
                TileSize = (int)jsonSource?.TileSize,
            };

            // If we have a new vector tile provider, than get styles for this
            if (vectorTileSource != null)
            {
                vectorTileSource.Styles.AddRange(ExtractStyles(vectorTileSource.Name, jsonStyle.StyleLayers, spriteAtlas));
            }

            return vectorTileSource;
        }

        /// <summary>
        /// Create the TileSource, that provides the data for the IDrawableTileSource
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ITileSource CreateTileSource(JsonSource source)
        {
            ITileSource tileSource = null;

            if (source.Tiles == null || source.Tiles.Count == 0)
                return null;

            if (source.Tiles[0].StartsWith("http"))
            {
                tileSource = new HttpTileSource(new GlobalSphericalMercator(
                        source.Scheme == "tms" ? YAxis.TMS : YAxis.OSM,
                        minZoomLevel: source.ZoomMin ?? 0,
                        maxZoomLevel: source.ZoomMax ?? 30                       
                    ),
                    source.Tiles[0], //"{s}",
                    source.Tiles,
                    name: source.Name,
                    attribution: new Attribution(source.Attribution)
                );
            }
            else if (source.Tiles[0].StartsWith("mbtiles://"))
            {
                // We should get the tile source from someone else
                tileSource = new MbTilesTileSource(new SQLiteConnectionString(source.Tiles[0].Substring(10), false),
                    new GlobalSphericalMercator(
                        source.Scheme == "tms" ? YAxis.TMS : YAxis.OSM,
                        minZoomLevel: source.ZoomMin ?? 0,
                        maxZoomLevel: source.ZoomMax ?? 14
                    ));
            }

            return tileSource;
        }

        private static List<MGLStyleLayer> ExtractStyles(string sourceName, IEnumerable<JsonStyleLayer> styleLayers, MGLSpriteAtlas spriteAtlas)
        {
            var styles = new List<MGLStyleLayer>();

            foreach (var styleLayer in styleLayers)
            {
                // styleLayer for background is independent from any source
                if (styleLayer.Type.ToLower().Equals("background"))
                {
                    // We handled this already in CreateBackgroundTileSource
                    continue;
                }

                if (!sourceName.Equals(styleLayer.Source, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                IFilter filter = new AllFilter(null);

                // Create filters for each style layer
                if (styleLayer.NativeFilter != null)
                    filter = FilterConverter.ConvertFilter(styleLayer.NativeFilter);

                MGLStyleLayer style = new MGLStyleLayer
                {
                    Id = styleLayer.Id,
                    MinZoom = styleLayer.MinZoom ?? 0,
                    MaxZoom = styleLayer.MaxZoom ?? 30,
                    Filter = filter,
                    SourceLayer = styleLayer.SourceLayer,
                };

                // We have a raster style or one of the vector styles "fill", "line", "symbol", "circle", "heatmap", "fill-extrusion"
                switch (styleLayer.Type.ToLower())
                {
                    case "raster":
                        style.Type = StyleType.Raster;
                        var rasterPaints = StyleLayerConverter.ConvertRasterLayer(styleLayer);
                        if (rasterPaints != null)
                            ((List<MGLPaint>)style.Paints).AddRange(rasterPaints);
                        break;
                    case "fill":
                        style.Type = StyleType.Fill;
                        var fillPaints = StyleLayerConverter.ConvertFillLayer(styleLayer, spriteAtlas);
                        if (fillPaints != null)
                            ((List<MGLPaint>)style.Paints).AddRange(fillPaints);
                        break;
                    case "line":
                        style.Type = StyleType.Line;
                        var linePaints = StyleLayerConverter.ConvertLineLayer(styleLayer, spriteAtlas);
                        if (linePaints != null)
                            ((List<MGLPaint>)style.Paints).AddRange(linePaints);
                        break;
                    case "symbol":
                        style.Type = StyleType.Symbol;
                        break;
                    case "circle":
                        break;
                    case "heatmap":
                        break;
                    case "fill-extrusion":
                        break;
                    default:
                        throw new ArgumentException($"Unknown layer type ${styleLayer.Type}");
                        break;
                }

                styles.Add(style);
            }

            return styles;
        }
    }
}
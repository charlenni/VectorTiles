using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BruTile;
using NetTopologySuite.Geometries;
using SkiaSharp;
using VectorTiles.MapboxGL.Extensions;
using VectorTiles.MapboxGL.Parser;

namespace VectorTiles.MapboxGL
{
    public class MGLVectorTileSource : IDrawableTileSource
    {
        public string Name { get; }

        public double MinVisible { get; set; }

        public double MaxVisible { get; set; }

        public int TileSize { get; set; } = 512;

        /// <summary>
        /// Tile source that provides the content of the tiles. In this case, it provides byte[] with features.
        /// </summary>
        public ITileSource Source { get; }

        public List<IVectorStyleLayer> Styles { get; } = new List<IVectorStyleLayer>();

        public ITileSchema Schema => Source.Schema;

        public Attribution Attribution => Source.Attribution;

        public MGLVectorTileSource(string name, ITileSource source)
        {
            Name = name;
            Source = source;
        }

        /// <summary>
        /// A MapboxGL tile has always coordinates of 4096 x 4096, but a TileSize of 512 x 512
        /// </summary>
        /// <param name="tileInfo">Info of tile to draw</param>
        /// <returns>Drawable VectorTile and List of symbols</returns>
        public Drawable GetDrawable(TileInfo ti)
        {
            // Check Schema for TileInfo
            var tileInfo = Schema.YAxis == YAxis.OSM ? ti.ToTMS() : ti;

            var tileSizeOfMGLVectorData = 4096f;
            var zoom = float.Parse(tileInfo.Index.Level);

            Polygon geometry;

            // Get data for this tile
            var (tileData, overzoom) = GetTileData(tileInfo);

            if (tileData == null)
                // We don't find any data for this tile, even if we check lower zoom levels
                return null;

            // For calculation of feature coordinates:
            // Coordinates between 0 and 4096. This is now multiplacated with the overzoom factor.
            // Than the offset is substracted. With this, the searched data is between 0 and 4096.
            // Than all coordinates scaled by TileSize/tileSizeOfMGLVectorData, so that all coordinates
            // are between 0 and TileSize.
            var features = GetFeatures(tileInfo, tileData, overzoom, TileSize / tileSizeOfMGLVectorData);

            if (features.Count == 0)
                return null;

            var result = new VectorTile(TileSize, (int)Math.Log(overzoom.Scale, 2));

            // Vector tiles have always a size of 4096 x 4096, but there could be overzoom (use of lower zoom level)
            // Drawing rect is only the part, that should later visible. With overzoom, only a part of the tile is used.
            var drawingRect = new SKRect(0, 0, TileSize, TileSize);

            // Now convert this features into drawables or add symbols and labels into buckets
            foreach (var style in Styles)
            {
                // Is this style relevant or is it outside the zoom range
                if (style.MinZoom > zoom || style.MaxZoom < zoom)
                    continue;

                SKPath path = new SKPath() { FillType = SKPathFillType.Winding };
                List<ISymbol> symbols = new List<ISymbol>();

                // Check all features
                foreach (var feature in features)
                {
                    // Is this feature on the actuell style layer?
                    if (style.SourceLayer != feature.Layer)
                        continue;

                    // Fullfill feature the filter for this style layer
                    if (!style.Filter.Evaluate(feature))
                        continue;

                    // Check for different types
                    switch (style.Type)
                    {
                        case StyleType.Symbol:
                            // Feature is a symbol
                            var symbol = CreateSymbol(feature, style);
                            if (symbol != null)
                                symbols.Add(symbol);
                            break;
                        case StyleType.Line:
                        case StyleType.Fill:
                            // Feature is a line or fill
                            CreatePath(path, feature, style);
                            break;
                        default:
                            throw new Exception("Unknown style type");
                    }
                }

                // Now we have all features on same layer with matching filters
                if (path.PointCount > 0)
                {
                    // We only want path, that are inside of the drawing rect
                    if (!path.Bounds.IntersectsWith(drawingRect))
                        continue;

                    foreach (var paint in style.Paints)
                        result.PathPaintBucket.Add(new PathPaintPair(path, paint));
                }

                if (symbols.Count > 0)
                {
                    result.SymbolBucket.Add(symbols);
                }
            }

            return result;
        }

        public MGLSymbol CreateSymbol(VectorTileFeature feature, IVectorStyleLayer style)
        {
            MGLSymbol symbol = null;

            switch (feature.Type)
            {
                case GeometryType.Point:
                    if (feature.Geometry.Coordinates[0].X < 0 || feature.Geometry.Coordinates[0].X > TileSize
                        || feature.Geometry.Coordinates[0].Y < 0 || feature.Geometry.Coordinates[0].Y > TileSize)
                        return null;
                    symbol = new MGLSymbol(feature, style);
                    break;
                case GeometryType.LineString:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"There are other geometries: ${feature.Type.ToString()}");
                    break;
            }

            // Create correct name

            return symbol;
        }

        /// <summary>
        /// Create path for feature
        /// </summary>
        /// <param name="path">SKPath to which this path should be added</param>
        /// <param name="feature">Feature to add</param>
        /// <param name="style">Style to use</param>
        private void CreatePath(SKPath path, VectorTileFeature feature, IVectorStyleLayer style)
        {
            switch (feature.Type)
            {
                case GeometryType.Point:
                    if (style.Type == StyleType.Line || style.Type == StyleType.Fill)
                    {
                        // This are things like height of a building
                        // We don't use this up to now
                        System.Diagnostics.Debug.WriteLine(feature.Tags.ToString());
                        return;
                    }
                    break;
                case GeometryType.LineString:
                    path.MoveTo((float)feature.Geometry.Coordinates[0].X, (float)feature.Geometry.Coordinates[0].Y);
                    for (var pos = 1; pos < feature.Geometry.Coordinates.Length; pos++)
                    {
                        path.LineTo((float)feature.Geometry.Coordinates[pos].X, (float)feature.Geometry.Coordinates[pos].Y);
                    }
                    break;
                case GeometryType.MultiLineString:
                    for (var n = 0; n < feature.Geometry.NumGeometries; n++)
                    {
                        path.MoveTo((float)feature.Geometry.GetGeometryN(n).Coordinates[0].X, (float)feature.Geometry.GetGeometryN(n).Coordinates[0].Y);
                        for (var pos = 1; pos < feature.Geometry.GetGeometryN(n).Coordinates.Length; pos++)
                        {
                            path.LineTo((float)feature.Geometry.GetGeometryN(n).Coordinates[pos].X, (float)feature.Geometry.GetGeometryN(n).Coordinates[pos].Y);
                        }
                    }
                    break;
                case GeometryType.Polygon:
                    var polygon = (Polygon)feature.Geometry;
                    path.MoveTo((float)polygon.Coordinates[0].X, (float)polygon.Coordinates[0].Y);
                    for (var pos = 1; pos < polygon.Coordinates.Length; pos++)
                    {
                        path.LineTo((float)polygon.Coordinates[pos].X, (float)polygon.Coordinates[pos].Y);
                    }
                    path.Close();
                    for (var hole = 0; hole < polygon.Holes.Length; hole++)
                    {
                        path.MoveTo((float)polygon.Holes[hole].Coordinates[0].X, (float)polygon.Holes[hole].Coordinates[0].Y);
                        for (var pos = 1; pos < polygon.Holes[hole].Coordinates.Length; pos++)
                        {
                            path.LineTo((float)polygon.Holes[hole].Coordinates[pos].X, (float)polygon.Holes[hole].Coordinates[pos].Y);
                        }
                        path.Close();
                    }
                    break;
                case GeometryType.MultiPolygon:
                    for (var n = 0; n < feature.Geometry.NumGeometries; n++)
                    {
                        var geometry = (Polygon)feature.Geometry.GetGeometryN(n);
                        path.MoveTo((float)geometry.ExteriorRing.Coordinates[0].X, (float)geometry.ExteriorRing.Coordinates[0].Y);
                        for (var pos = 1; pos < geometry.ExteriorRing.Coordinates.Length; pos++)
                        {
                            path.LineTo((float)geometry.ExteriorRing.Coordinates[pos].X, (float)geometry.ExteriorRing.Coordinates[pos].Y);
                        }
                        path.Close();
                        for (var hole = 0; hole < geometry.Holes.Length; hole++)
                        {
                            path.MoveTo((float)geometry.Holes[hole].Coordinates[0].X, (float)geometry.Holes[hole].Coordinates[0].Y);
                            for (var pos = 1; pos < geometry.Holes[hole].Coordinates.Length; pos++)
                            {
                                path.LineTo((float)geometry.Holes[hole].Coordinates[pos].X, (float)geometry.Holes[hole].Coordinates[pos].Y);
                            }
                            path.Close();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Get data for tile
        /// </summary>
        /// <remarks>
        /// If this tile couldn't be found, than we try to get tile data for a tile with lower zoom level
        /// </remarks>
        /// <param name="tileInfo">Tile info for tile to get data for</param>
        /// <returns>Raw tile data, factor for enlargement for this data and offsets for parts of this data, which to use</returns>
        private (byte[], Overzoom) GetTileData(TileInfo tileInfo)
        {
            var zoom = (int)float.Parse(tileInfo.Index.Level);
            var scale = 1;
            var offsetX = 0f;
            var offsetY = 0f;  //(Source.Schema.YAxis == YAxis.TMS ? -4096f : 0f);
            var offsetFactor = 4096;

            // Check MinZoom of source. MaxZoom isn't checked, because of overzoom
            if (zoom < 0)
                return (null, Overzoom.None);

            // Get byte data for this tile
            var tileData = Source.GetTile(tileInfo);

            if (tileData != null)
                return (tileData, Overzoom.None);

            var info = tileInfo;
            var row = info.Index.Row;
            var col = info.Index.Col;

            while (tileData == null && zoom >= 0)
            {
                scale <<= 1;
                offsetX = offsetX + (col % 2) * offsetFactor;
                offsetY = offsetY + (row % 2) * offsetFactor * (Source.Schema.YAxis == YAxis.TMS ? +1f : -1f);
                zoom--;
                row >>= 1;
                col >>= 1;
                offsetFactor <<= 1;
                info.Index = new TileIndex(col, row, zoom.ToString());
                tileData = Source.GetTile(info);
            }

            if (zoom < 0)
                return (null, Overzoom.None);

            offsetY = offsetFactor - offsetY + (Source.Schema.YAxis == YAxis.TMS ? -4096f : 0f);

            var overzoom = new Overzoom(scale, offsetX, offsetY);

            return (tileData, overzoom);
        }

        private IList<VectorTileFeature> GetFeatures(TileInfo tileInfo, byte[] tileData, Overzoom overzoom, float scale)
        {
            // Parse tile and convert it to a feature list
            Stream stream = new MemoryStream(tileData);

            if (IsGZipped(stream))
                stream = new GZipStream(stream, CompressionMode.Decompress);

            var features = VectorTileParser.Parse(tileInfo, stream, overzoom, scale);

            System.Diagnostics.Debug.WriteLine($"Cached Tile Level={tileInfo.Index.Level}, Col={tileInfo.Index.Col}, Row={tileInfo.Index.Row}");

            return features;
        }

        /// <summary>
        /// Check, if stream contains gzipped data 
        /// </summary>
        /// <param name="stream">Stream to check</param>
        /// <returns>True, if the stream is gzipped</returns>
        bool IsGZipped(Stream stream)
        {
            return IsZipped(stream, 3, "1F-8B-08");
        }

        /// <summary>
        /// Check, if stream contains zipped data
        /// </summary>
        /// <param name="stream">Stream to check</param>
        /// <param name="signatureSize">Length of bytes to check for signature</param>
        /// <param name="expectedSignature">Signature to check</param>
        /// <returns>True, if the stream is zipped</returns>
        bool IsZipped(Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;
            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, SeekOrigin.Begin);
            string actualSignature = BitConverter.ToString(signature);
            if (actualSignature == expectedSignature) return true;
            return false;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            throw new NotImplementedException();
        }
    }
}

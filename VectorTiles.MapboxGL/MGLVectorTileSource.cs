using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BruTile;
using BruTile.Predefined;
using VectorTiles.MapboxGL.Extensions;
using VectorTiles.MapboxGL.Parser;

using Rect = SkiaSharp.SKRect;

namespace VectorTiles.MapboxGL
{
    public class MGLVectorTileSource : IDrawableTileSource
    {
        private double minVisible = 14.ToResolution();
        private double maxVisible = 0.ToResolution();
        private double minZoomLevelProvider;
        private double maxZoomLevelProvider;
        private ITileDataParser tileDataParser;

        public string Name { get; }

        public double MinVisible
        {
            get => minVisible;
            set
            {
                if (minVisible != value)
                {
                    minVisible = value;
                    UpdateResolutions();
                }
            }
        }

        public double MaxVisible
        {
            get => maxVisible;
            set
            {
                if (maxVisible != value)
                {
                    maxVisible = value;
                    UpdateResolutions();
                }
            }
        }

        public int TileSize { get; set; } = 4096;

        /// <summary>
        /// Tile source that provides the content of the tiles. In this case, it provides byte[] with features.
        /// </summary>
        public ITileSource Source { get; }

        public List<IVectorStyleLayer> StyleLayers { get; } = new List<IVectorStyleLayer>();

        public ITileSchema Schema { get; }

        public Attribution Attribution => Source.Attribution;

        public MGLVectorTileSource(string name, ITileSource source, ITileDataParser tdp = null)
        {
            minZoomLevelProvider = source.Schema.Resolutions.First<KeyValuePair<string, Resolution>>().Value.UnitsPerPixel.ToZoomLevel();
            maxZoomLevelProvider = source.Schema.Resolutions.Last<KeyValuePair<string, Resolution>>().Value.UnitsPerPixel.ToZoomLevel();

            Name = name;
            Source = source;
            Schema = new GlobalSphericalMercator(source.Schema.YAxis, minZoomLevel: (int)minZoomLevelProvider, maxZoomLevel: (int)maxZoomLevelProvider);

            MinVisible = ((int)maxZoomLevelProvider).ToResolution();
            MaxVisible = ((int)minZoomLevelProvider).ToResolution();

            tileDataParser = tdp ?? new MGLTileParser();

            UpdateResolutions();
        }

        /// <summary>
        /// A MapboxGL tile has always coordinates of 4096 x 4096, but a TileSize of 512 x 512
        /// </summary>
        /// <param name="tileInfo">Info of tile to draw</param>
        /// <returns>Drawable VectorTile and List of symbols</returns>
        public Drawable GetVectorTile(TileInfo ti)
        {
            // Check Schema for TileInfo
            var tileInfo = Schema.YAxis == YAxis.OSM ? ti.ToTMS() : ti.Copy();

            var zoom = float.Parse(tileInfo.Index.Level);

            // If zoom level higher
            if (zoom > MinVisible.ToZoomLevel()) //maxZoomLevelProvider)
                return null;

            // Get data for this tile
            var (tileData, overzoom) = GetTileData(tileInfo);

            if (tileData == null)
                // We don't find any data for this tile, even if we check lower zoom levels
                return null;

            var sink = new VectorTile(tileInfo, TileSize, StyleLayers, (int)zoom, (int)Math.Log(overzoom.Scale, 2));

            // For calculation of feature coordinates:
            // Coordinates are between 0 and 4096. This is now multiplacated with the overzoom factor.
            // Than the offset is substracted. With this, the searched data is between 0 and 4096.
            // Than all coordinates scaled by TileSize/tileSizeOfMGLVectorData, so that all coordinates
            // are between 0 and TileSize.
            ParseTileData(ti, tileData, sink, overzoom);

            return sink;
        }

        private void UpdateResolutions()
        {
            Schema.Resolutions.Clear();
            for (int i = (int)MaxVisible.ToZoomLevel(); i <= (int)MinVisible.ToZoomLevel(); i++)
                Schema.Resolutions.Add(i.ToString(), new Resolution(i.ToString(), i.ToResolution()));
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

            // We only create overzoom tiles when zoom is between min and max zoom
            if (zoom <= MaxVisible.ToZoomLevel() || zoom > MinVisible.ToZoomLevel()) //Source.Schema.Resolutions.First().Value.UnitsPerPixel.ToZoomLevel() || zoom > Source.Schema.Resolutions.Last().Value.UnitsPerPixel.ToZoomLevel())
                return (null, Overzoom.None);

            var info = tileInfo;
            var row = info.Index.Row;
            var col = info.Index.Col;

            while (tileData == null && zoom >= 0)
            {
                scale <<= 1;
                offsetX = offsetX + (col % 2) * offsetFactor;
                offsetY = offsetY + (row % 2) * offsetFactor * (Source.Schema.YAxis == YAxis.TMS ? +1f : -1f);
                var doubleWidth = info.Extent.Width * 2.0;
                var doubleHeight = info.Extent.Height * 2.0;
                //var minX = info.Extent.MinX  ((col % 2) * halfWidth);
                //var minY = info.Extent.MinY + ((row % 2) * halfHeight);
                zoom--;
                row >>= 1;
                col >>= 1;
                offsetFactor <<= 1;
                //info.Extent = new Extent(minX, minY, minX + halfWidth, minY + halfHeight);
                info.Index = new TileIndex(col, row, zoom.ToString());
                tileData = Source.GetTile(info);
            }

            if (zoom < 0)
                return (null, Overzoom.None);

            offsetY = offsetFactor - offsetY + (Source.Schema.YAxis == YAxis.TMS ? -4096f : 0f);

            var overzoom = new Overzoom(scale, offsetX, offsetY);

            return (tileData, overzoom);
        }

        private void ParseTileData(TileInfo tileInfo, byte[] tileData, ITileDataSink sink, Overzoom overzoom)
        {
            // Parse tile and convert it to a feature list
            Stream stream = new MemoryStream(tileData);

            if (IsGZipped(stream))
                stream = new GZipStream(stream, CompressionMode.Decompress);

            try
            {
                tileDataParser.Parse(tileInfo, stream, sink, overzoom, new TileClipper(new Rect(-8, -8, TileSize + 8, TileSize + 8)));
            }
            catch (Exception e)
            {
                var test = e.Message;
            }
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
            throw new NotImplementedException("IVectorTileSource doesn't implement byte[] GetTile(TileInfo)");
        }
    }
}

using BruTile;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using VectorTiles.MapboxGL.Pbf;

namespace VectorTiles.MapboxGL.Parser
{
    public static class VectorTileParser
    {
        /// <summary>
        /// Parses a unzipped tile in Mapbox format
        /// </summary>
        /// <param name="tileInfo">TileInfo of this tile</param>
        /// <param name="stream">Stream containing tile data in Pbf format</param>
        /// <param name="scale">Factor for scaling of coordinates because of overzooming</param>
        /// <param name="offsetX">Offset in X direction because of overzooming</param>
        /// <param name="offsetY">Offset in Y direction because of overzooming</param>
        /// <returns>List of VectorTileLayers, which contain Name and VectorTilesFeatures of each layer, this tile containing</returns>
        public static IList<VectorTileFeature> Parse(TileInfo tileInfo, Stream stream, int overzoom, float offsetX, float offsetY, float scale)
        {
            // Get tile information from Pbf format
            var tile = Serializer.Deserialize<Tile>(stream);

            // Create new vector tile layer
            var features = new List<VectorTileFeature>();

            foreach (var layer in tile.Layers)
            {
                // Convert all features from Mapbox format into Mapsui format
                foreach (var feature in layer.Features)
                {
                    var vectorTileFeature = FeatureParser.Parse(tileInfo, layer.Name, feature, layer.Keys, layer.Values, layer.Extent, overzoom, offsetX, offsetY, scale);

                    // Add to layer
                    features.Add(vectorTileFeature);
                }
            }

            return features;
        }
    }
}
using BruTile;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using VectorTiles.MapboxGL.Pbf;

namespace VectorTiles.MapboxGL.Parser
{
    public static class FeatureParser
    {
        /// <summary>
        /// Converts a Mapbox feature in Mapbox coordinates into a VectorTileFeature
        /// </summary>
        /// <param name="tileInfo">TileInfo for tile informations like left top coordinates</param>
        /// <param name="layerName">Name of vector tile layer to which this vector tile feature belongs</param>
        /// <param name="feature">Mapbox feature to convert</param>
        /// <param name="keys">List of known keys for this tile</param>
        /// <param name="values">List of known values for this tile</param>
        /// <param name="extent">Extent/width of this Mapbox formated tile (normally 4096)</param>
        /// <param name="scale">Factor for scaling of coordinates because of overzooming</param>
        /// <param name="offsetX">Offset in X direction because of overzooming</param>
        /// <param name="offsetY">Offset in Y direction because of overzooming</param>
        /// <returns></returns>
        public static VectorTileFeature Parse(TileInfo tileInfo, string layerName, Feature feature, List<string> keys, List<Value> values, uint extent, int overzoom, float offsetX, float offsetY, float scale)
        {
            var vtf = new VectorTileFeature(layerName, feature.Id.ToString());

            var geometries =  GeometryParser.ParseGeometry(feature.Geometry, feature.Type, overzoom, offsetX, offsetY, scale);

            int i;

            // Add the geometry
            switch (feature.Type)
            {
                case GeomType.Point:
                    // Convert all Points
                    if (geometries.Count == 1)
                    {
                        // Single point
                        vtf.Geometry = new Point(geometries[0][0]);
                    }
                    else
                    {
                        // Multi point
                        var multiPoints = new List<Point>();
                        foreach (var points in geometries)
                        {
                            foreach (var point in points)
                            {
                                multiPoints.Add(new Point(point));
                            }
                        }
                        vtf.Geometry = new MultiPoint(multiPoints.ToArray());
                    }
                    break;
                case GeomType.LineString:
                    // Convert all LineStrings
                    if (geometries.Count == 1)
                    {
                        // Single line
                        vtf.Geometry = new LineString(geometries[0].ToArray());
                    }
                    else
                    {
                        // Multi line
                        var multiLines = new LineString[geometries.Count];
                        for (i = 0; i < geometries.Count; i++)
                        {
                            multiLines[i] = new LineString(geometries[i].ToArray());
                        }
                        vtf.Geometry = new MultiLineString(multiLines);
                    }
                    break;
                case GeomType.Polygon:
                    // Convert all Polygons
                    var polygons = new List<Polygon>();

                    LinearRing polygon = null;
                    List<LinearRing> holes = new List<LinearRing>();

                    i = 0;
                    do
                    {
                        // Check, if first and last are the same points
                        if (!geometries[i].First().Equals(geometries[i].Last()))
                            geometries[i].Add(geometries[i].First());

                        // Convert all points of this ring
                        var ring = new LinearRing(geometries[i].ToArray());

                        if (ring.IsCCW && polygon != null)
                        {
                            holes.Add(ring);
                        }
                        else
                        {
                            if (polygon != null)
                            {
                                polygons.Add(new Polygon(polygon, holes.ToArray()));
                            }
                            polygon = ring;
                            holes.Clear();
                        }

                        i++;
                    } while (i < geometries.Count);

                    // Save last one
                    polygons.Add(new Polygon(polygon, holes.ToArray()));

                    // Now save correct geometry
                    if (polygons.Count == 1)
                    {
                        vtf.Geometry = polygons[0];
                    }
                    else
                    {
                        vtf.Geometry = new MultiPolygon(polygons.ToArray());
                    }
                    break;
            }

            // now add the tags
            vtf.Tags.Add(TagsParser.Parse(keys, values, feature.Tags));

            if (vtf.Tags.TryGetValue("rank", out var rank) && rank.Type == Newtonsoft.Json.Linq.JTokenType.Integer)
                vtf.Rank = rank.ToObject<int>();

            return vtf;
        }
    }
}
﻿using BruTile;
using System;
using System.Collections.Generic;
using VectorTiles.MapboxGL.Pbf;

using Point = SkiaSharp.SKPoint;

namespace VectorTiles.MapboxGL.Parser
{
    public static class FeatureParser
    {
        //public static VectorElement element = new VectorElement();

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
        public static VectorElement Parse(VectorElement element, TileInfo tileInfo, string layerName, Feature feature, List<string> keys, List<Value> values, uint extent, Overzoom overzoom)
        {
            element.Clear();

            element.Layer = layerName;
            element.Id = feature.Id.ToString();

            var geometries =  GeometryParser.ParseGeometry(feature.Geometry, feature.Type, overzoom);

            int i;

            // Add the geometry
            switch (feature.Type)
            {
                case GeomType.Point:
                    // Convert all Points
                    element.StartPoint();
                    element.Add(geometries[0]);
                    break;
                case GeomType.LineString:
                    // Convert all LineStrings
                    foreach (var linePoints in geometries)
                    {
                        element.StartLine();
                        element.Add(linePoints);
                    }
                    break;
                case GeomType.Polygon:
                    // Convert all Polygons
                    for (i = 0; i < geometries.Count; i++)
                    {
                        if (ShoelaceArea(geometries[i]) >= 0)
                        {
                            element.StartPolygon();
                            element.Add(geometries[i]);
                        }
                        else
                        {
                            element.StartHole();
                            element.Add(geometries[i]);
                        }
                    }
                    break;
            }

            // now add the tags
            TagsParser.Parse(element, keys, values, feature.Tags);

            return element;
        }

        /// <summary>
        /// Function to calculate the area of a polygon. If it is CW then area is positive, if CCW then negative
        /// Found at: https://rosettacode.org/wiki/Shoelace_formula_for_polygonal_area#C.23
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        static double ShoelaceArea(List<Point> v)
        {
            int len = v.Count;
            double a = 0.0;

            for (int i = 0; i < len - 1; i++)
            {
                a += v[i].X * v[i + 1].Y - v[i + 1].X * v[i].Y;
            }

            return Math.Abs(a + v[len - 1].X * v[0].Y - v[0].X * v[len - 1].Y) / 2.0;
        }
    }
}
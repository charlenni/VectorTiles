using GeoAPI.Geometries;
using System.Collections.Generic;
using VectorTiles.MapboxGL.Pbf;

namespace VectorTiles.MapboxGL.Parser
{
    public static class GeometryParser
    {
        /// <summary>
        /// Convert Mapbox tile format (see https://www.mapbox.com/vector-tiles/specification/)
        /// </summary>
        /// <param name="geom">Geometry information in Mapbox format</param>
        /// <param name="geomType">GeometryType of this geometry</param>
        /// <param name="scale">Factor for scaling of coordinates because of overzooming</param>
        /// <param name="offsetX">Offset in X direction because of overzooming</param>
        /// <param name="offsetY">Offset in Y direction because of overzooming</param>
        /// <returns>List of list of points in world coordinates</returns>
        public static List<List<Coordinate>> ParseGeometry(List<uint> geom, GeomType geomType, int overzoom, float offsetX, float offsetY, float scale)
        {
            const uint cmdMoveTo = 1;
            //const uint cmdLineTo = 2;
            const uint cmdSegEnd = 7;
            //const uint cmdBits = 3;

            long x = 0;
            long y = 0;
            var coordsList = new List<List<Coordinate>>();
            List<Coordinate> coords = null;
            var geometryCount = geom.Count;
            uint length = 0;
            uint command = 0;
            var i = 0;
            while (i < geometryCount)
            {
                if (length <= 0)
                {
                    length = geom[i++];
                    command = length & ((1 << 3) - 1);
                    length = length >> 3;
                }

                if (length > 0)
                {
                    if (command == cmdMoveTo)
                    {
                        coords = new List<Coordinate>();
                        coordsList.Add(coords);
                    }
                }

                if (command == cmdSegEnd)
                {
                    if (geomType != GeomType.Point && coords?.Count != 0)
                    {
                        coords?.Add(coords[0]);
                    }
                    length--;
                    continue;
                }

                var dx = geom[i++];
                var dy = geom[i++];

                length--;

                var ldx = ZigZag.Decode(dx);
                var ldy = ZigZag.Decode(dy);

                x = x + ldx;
                y = y + ldy;

                // Correct coordinates for overzoom
                var coord = new Coordinate((x * overzoom - offsetX) * scale, (y * overzoom - offsetY) * scale);

                coords?.Add(coord);
            }
            return coordsList;
        }
    }
}
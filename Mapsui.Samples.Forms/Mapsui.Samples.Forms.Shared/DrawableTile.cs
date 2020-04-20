using System;
using BruTile;
using Mapsui.Geometries;
using Mapsui.Geometries.WellKnownBinary;
using Mapsui.Geometries.WellKnownText;
using Mapsui.Providers;
using VectorTiles;

namespace Mapsui.Samples.Forms
{
    public class DrawableTile : Geometry
    {
        public static Feature DrawableTileToFeature(TileInfo tileInfo, object data)
        {
            Drawable drawable = (Drawable)data;
            if (drawable == null)
                return null;
            IGeometry drawableTile = new DrawableTile(drawable, new BoundingBox(tileInfo.Extent.MinX, tileInfo.Extent.MinY, tileInfo.Extent.MaxX, tileInfo.Extent.MaxY));
            var feature = new Feature();
            feature.Geometry = drawableTile;
            return feature;
        }

        private readonly BoundingBox boundingBox;

        public DrawableTile(Drawable data, BoundingBox box)
        {
            Data = data;
            boundingBox = box;
            TickFetched = DateTime.Now.Ticks;
        }

        public Drawable Data { get; }

        public long TickFetched { get; }

        public override BoundingBox BoundingBox => boundingBox;

        public new string AsText()
        {
            return GeometryToWKT.Write(Envelope);
        }

        public new byte[] AsBinary()
        {
            return GeometryToWKB.Write(Envelope);
        }

        public override bool IsEmpty()
        {
            return boundingBox.Width * boundingBox.Height <= 0;
        }

        public new Geometry Clone()
        {
            return new DrawableTile(Data, boundingBox.Clone());
        }

        public override double Distance(Point point)
        {
            var geometry = Envelope;
            return geometry.Distance(point);
        }

        public override bool Contains(Point point)
        {
            return Envelope.Contains(point);
        }

        public override int GetHashCode()
        {
            // todo: check performance of MemoryStream.GetHashCode
            return Envelope.GetHashCode() * Data.GetHashCode();
        }

        public override bool Equals(Geometry geom)
        {
            var vectorTile = geom as DrawableTile;
            if (vectorTile == null) return false;
            return Equals(vectorTile);
        }
    }
}
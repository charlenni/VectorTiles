using NetTopologySuite.Geometries;

namespace VectorTiles
{
    public class Offset : Point
    {
        public new static Offset Empty = new Offset(0, 0);

        public Offset() : base(0, 0)
        { }

        public Offset(double x, double y) : base(x, y)
        { }
    }
}

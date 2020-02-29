using NetTopologySuite.Geometries;

namespace VectorTiles
{
    public class Rect
    {
        public static Rect Empty = new Rect(0, 0, 0, 0);

        public Rect(float left, float top, float right, float bottom)
        {
            TopLeft = new Point(left, top);
            BottomRight = new Point(right, bottom);
        }

        public Rect(Point topLeft, Point bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public Point TopLeft { get; }

        public Point BottomRight { get; }
    }
}

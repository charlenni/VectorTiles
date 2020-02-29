using VectorTiles.Enums;

namespace VectorTiles.MapboxGL.Extensions
{
    public static class OrientationExtensions
    {
        public static Orientation ToOrientation(this string orientation)
        {
            switch (orientation.ToLower())
            {
                case "horizontal":
                    return Orientation.Horizontal;
                case "vertical":
                    return Orientation.Vertical;
                default:
                    return Orientation.Horizontal;
            }
        }
    }
}

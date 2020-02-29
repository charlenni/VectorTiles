﻿using VectorTiles.Enums;

namespace VectorTiles.MapboxGL.Extensions
{
    public static class PlacementExtensions
    {
        public static Placement ToPlacement(this string placement)
        {
            switch (placement.ToLower())
            {
                case "point":
                    return Placement.Point;
                case "line":
                    return Placement.Line;
                case "line-center":
                    return Placement.LineCenter;
                default:
                    return Placement.Point;
            }
        }
    }
}

﻿using VectorTiles.Enums;

namespace VectorTiles.MapboxGL.Extensions
{
    public static class TextJustifyExtensions
    {
        public static TextJustify ToTextJustify(this string justify)
        {
            switch (justify.ToLower())
            {
                case "auto":
                    return TextJustify.Auto;
                case "left":
                    return TextJustify.Left;
                case "center":
                    return TextJustify.Center;
                case "right":
                    return TextJustify.Right;
                default:
                    return TextJustify.Center;

            }
        }
    }
}

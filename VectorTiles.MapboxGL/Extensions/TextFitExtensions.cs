﻿using VectorTiles.Enums;

namespace VectorTiles.MapboxGL.Extensions
{
    public static class TextFitExtensions
    {
        public static TextFit ToTextFit(this string text)
        {
            switch (text.ToLower())
            {
                case "none":
                    return TextFit.None;
                case "width":
                    return TextFit.Width;
                case "height":
                    return TextFit.Height;
                case "both":
                    return TextFit.Both;
                default:
                    return TextFit.None;
            }
        }
    }
}

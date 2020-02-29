using SkiaSharp;
using System.Collections.Generic;

namespace VectorTiles.MapboxGL
{
    public class MGLSprite : ISprite
    {
        public MGLSprite(KeyValuePair<string, Json.JsonSprite> sprite, SKImage image)
        {
            Name = sprite.Key;
            Image = image;
            Width = sprite.Value.Width;
            Height = sprite.Value.Height;
            PixelRatio = sprite.Value.PixelRatio;
            if (sprite.Value.Content != null && sprite.Value.Content.Count == 4)
                Content = new SKRect(sprite.Value.Content[0], sprite.Value.Content[1], sprite.Value.Content[1], sprite.Value.Content[3]);
            var strech = new SKRect(0, 0, 0, 0);
            if (sprite.Value.StrechX != null && sprite.Value.StrechX.Count == 2)
            {
                strech.Left = sprite.Value.StrechX[0];
                strech.Right = sprite.Value.StrechX[1];
            }
            if (sprite.Value.StrechY != null && sprite.Value.StrechY.Count == 2)
            {
                strech.Top = sprite.Value.StrechY[0];
                strech.Bottom = sprite.Value.StrechY[1];
            }
            Strech = strech;
        }

        public string Name { get; }

        public float Width { get; }

        public float Height { get; }

        public float PixelRatio { get; }

        public SKRect Content { get; }

        public SKRect Strech { get; }

        public SKImage Image { get; }
    }
}

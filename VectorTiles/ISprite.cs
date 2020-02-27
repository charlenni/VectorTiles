using SkiaSharp;

namespace VectorTiles
{
    /// <summary>
    /// Interface for a sprite. This sprite could come from a single image or a sprite atlas.
    /// </summary>
    public interface ISprite
    {
        /// <summary>
        /// Name of this sprite
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Width of sprite in pixel
        /// </summary>
        float Width { get; }

        /// <summary>
        /// Height of sprite in pixel
        /// </summary>
        float Height { get; }

        /// <summary>
        /// Image of this sprite
        /// </summary>
        SKImage Image { get; }
    }
}

using SkiaSharp;

namespace VectorTiles
{
    public abstract class Symbol : Drawable, ISymbol
    {
        private SKRect envelope;

        /// <summary>
        /// Id of this symbol
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Class of this symbol
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Subclass of this symbol
        /// </summary>
        public string Subclass { get; set; }

        /// <summary>
        /// Name of this symbol with correct culture
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rank of this symbol lower numbers are importanter
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Flag to show, if this symbol is visible or not
        /// </summary>
        public bool IsVisible { get; set; } = false;

        /// <summary>
        /// Envelope of this symbol with padding in tile relative coordinates
        /// </summary>
        public virtual ref readonly SKRect Envelope => ref envelope;

        public abstract void OnCalcBoundings();

        public abstract void OnDraw(SKCanvas canvas, EvaluationContext context);
    }
}

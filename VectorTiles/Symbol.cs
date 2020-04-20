using SkiaSharp;

namespace VectorTiles
{
    public abstract class Symbol : Drawable, ISymbol
    {
        private SKRect envelope;

        /// <summary>
        /// Class of this symbol
        /// </summary>
        public string Class { get; protected set; }

        /// <summary>
        /// Subclass of this symbol
        /// </summary>
        public string Subclass { get; protected set; }

        /// <summary>
        /// Name of this symbol with correct culture
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Rank of this symbol lower numbers are importanter
        /// </summary>
        public int Rank { get; protected set; }

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

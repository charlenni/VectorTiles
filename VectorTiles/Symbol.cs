using SkiaSharp;

namespace VectorTiles
{
    public abstract class Symbol : Drawable, ISymbol
    {
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

        public abstract void OnDraw(SKCanvas canvas, EvaluationContext context);
    }
}

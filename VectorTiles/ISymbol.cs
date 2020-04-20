using VectorTiles.RBush;

namespace VectorTiles
{
    public interface ISymbol : ISpatialData
    {
        /// <summary>
        /// Class of this symbol
        /// </summary>
        string Class { get; }

        /// <summary>
        /// Subclass of this symbol
        /// </summary>
        string Subclass { get; }

        /// <summary>
        /// Name of this symbol with correct culture
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Rank of this symbol lower numbers are importanter
        /// </summary>
        int Rank { get; }

        /// <summary>
        /// Flag to show, if this symbol is visible or not
        /// </summary>
        bool IsVisible { get; }
    }
}

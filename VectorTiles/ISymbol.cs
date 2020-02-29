namespace VectorTiles
{
    public interface ISymbol
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
    }
}

using System.Collections.Generic;

using Point = SkiaSharp.SKPoint;

namespace VectorTiles
{
    public interface IVectorSymbolStyler
    {
        bool HasIcon { get; }

        bool HasText { get; }

        Symbol CreateIconSymbol(Point point, TagsCollection tags, EvaluationContext context);

        Symbol CreateTextSymbol(Point point, TagsCollection tags, EvaluationContext context);

        Symbol CreateIconTextSymbol(Point point, TagsCollection tags, EvaluationContext context);

        IEnumerable<Symbol> CreatePathSymbols(VectorElement element, EvaluationContext context);
    }
}

using SkiaSharp;
using System.Collections.Generic;

namespace VectorTiles
{
    public class SymbolBucket : IBucket
    {
        IVectorStyleLayer styleLayer;
        IVectorSymbolStyler styler;

        public List<Symbol> Symbols = new List<Symbol>();

        public SymbolBucket(IVectorStyleLayer style)
        {
            styleLayer = style;
            styler = style.SymbolStyler;
        }

        public void AddElement(VectorElement element, EvaluationContext context)
        {
            // TODO: Remove, is only for tests
            if (styleLayer.SourceLayer == "poi")
            {
                var t10 = 10;
            }

            switch (element.Type)
            {
                case GeometryType.Point:
                    foreach (var point in element.Points)
                    {
                        if (styler.HasIcon && styler.HasText)
                        {
                            var iconTextSymbol = styler.CreateIconTextSymbol(point, element.Tags, context);
                            if (iconTextSymbol != null)
                                Symbols.Add(iconTextSymbol);
                        }
                        else if (styler.HasIcon)
                        {
                            var iconSymbol = styler.CreateIconSymbol(point, element.Tags, context);
                            if (iconSymbol != null)
                                Symbols.Add(iconSymbol);
                        }
                        else if (styler.HasText)
                        {
                            var textSymbol = styler.CreateTextSymbol(point, element.Tags, context);
                            if (textSymbol != null)
                                Symbols.Add(textSymbol);
                        }
                    }
                    break;
                case GeometryType.LineString:
                    if (styler == null)
                        return;
                    var pathSymbol = styler.CreatePathSymbols(element, context);
                    if (pathSymbol != null)
                        Symbols.Add(pathSymbol);
                    break;
                case GeometryType.Polygon:
                    var t3 = styleLayer.SourceLayer;
                    break;
                default:
                    var t4 = styleLayer.SourceLayer;
                    break;
            }
        }

        public void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            foreach (var symbol in Symbols)
            {
                symbol.OnDraw(canvas, context);
            }
        }
    }
}

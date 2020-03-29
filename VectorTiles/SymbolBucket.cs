using SkiaSharp;
using System;
using System.Collections.Generic;

namespace VectorTiles
{
    public class SymbolBucket : IBucket
    {
        IVectorStyleLayer styleLayer;
        IVectorSymbolStyler styler;
        List<Symbol> symbols = new List<Symbol>();

        public SymbolBucket(IVectorStyleLayer style)
        {
            styleLayer = style;
            styler = style.SymbolStyler;
        }

        public void AddElement(VectorElement element, EvaluationContext context)
        {
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
                                symbols.Add(iconTextSymbol);
                        }
                        else if (styler.HasIcon)
                        {
                            var iconSymbol = styler.CreateIconSymbol(point, element.Tags, context);
                            if (iconSymbol != null)
                                symbols.Add(iconSymbol);
                        }
                        else if (styler.HasText)
                        {
                            var textSymbol = styler.CreateTextSymbol(point, element.Tags, context);
                            if (textSymbol != null)
                                symbols.Add(textSymbol);
                        }
                    }
                    break;
                case GeometryType.LineString:
                    if (styler == null)
                        return;
                    var list = styler.CreatePathSymbols(element, context);
                    if (list != null)
                        symbols.AddRange(list);
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
            foreach (var symbol in symbols)
            {
                symbol.OnDraw(canvas, context);
            }
        }
    }
}

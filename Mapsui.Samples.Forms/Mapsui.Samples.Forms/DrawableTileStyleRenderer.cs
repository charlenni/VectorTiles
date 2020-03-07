using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Samples.Forms
{
    public class DrawableTileStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache)
        {
            canvas.DrawDrawable(((DrawableTile)feature).Data, new SKPoint(0, 0));
            return true;
        }
    }
}

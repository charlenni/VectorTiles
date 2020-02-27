using SkiaSharp;

namespace VectorTiles
{
    public interface IVectorPaint
    {
        /// <summary>
        /// Creates a SKPaint to 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        SKPaint CreatePaint(EvaluationContext context);
    }
}

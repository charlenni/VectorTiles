using SkiaSharp;

namespace VectorTiles.RBush
{
	public interface ISpatialData
	{
		ref readonly SKRect Envelope { get; }
	}
}

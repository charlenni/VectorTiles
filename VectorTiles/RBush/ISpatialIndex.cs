using SkiaSharp;
using System.Collections.Generic;

namespace VectorTiles.RBush
{
	public interface ISpatialIndex<out T>
	{
		IReadOnlyList<T> Search();

		IReadOnlyList<T> Search(in SKRect boundingBox);
	}
}

using BruTile;
using System;
using System.Collections.Generic;
using System.Text;

namespace VectorTiles
{
    public interface ITileDataSource
    {
        /// <summary>
        /// Query for tile. Return the result to sink.
        /// </summary>
        /// <param name="tile">Tile to get</param>
        /// <param name="sink">Tile sink to handle elemnts</param>
        void Query(TileInfo tile, ITileDataSink sink);

        /// <summary>
        /// Implementations should cancel their IO work and return
        /// </summary>
        void Cancel();
    }
}

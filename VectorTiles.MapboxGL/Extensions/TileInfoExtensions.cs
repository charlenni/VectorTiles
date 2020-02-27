using BruTile;
using System;

namespace VectorTiles.MapboxGL.Extensions
{
    public static class TileInfoExtensions
    {
        public static TileInfo ToTMS(this TileInfo tileInfo)
        {
            var result = new TileInfo();
            var zoom = float.Parse(tileInfo.Index.Level);
            var newRow = (int)Math.Pow(2, zoom) - tileInfo.Index.Row - 1;

            result.Index = new TileIndex(tileInfo.Index.Col, newRow, tileInfo.Index.Level);

            return result;
        }

        public static TileInfo ToOSM(this TileInfo tileInfo)
        {
            var result = new TileInfo();
            var zoom = float.Parse(tileInfo.Index.Level);
            var newRow = (int)Math.Pow(2, zoom) - tileInfo.Index.Row - 1;

            result.Index = new TileIndex(tileInfo.Index.Col, newRow, tileInfo.Index.Level);

            return result;
        }
    }
}

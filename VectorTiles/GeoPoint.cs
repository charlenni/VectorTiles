namespace VectorTiles
{
    public struct GeoPoint
    {
        public GeoPoint(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }

        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}

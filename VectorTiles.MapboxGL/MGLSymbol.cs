namespace VectorTiles.MapboxGL
{
    public class MGLSymbol : Symbol
    {
        public MGLSymbol(VectorTileFeature feature, IVectorStyleLayer style)
        {
            Feature = feature;
            Style = style;

            Class = feature.Tags.ContainsKey("class") ? feature.Tags["class"].ToString() : string.Empty;
            Subclass = feature.Tags.ContainsKey("subclass") ? feature.Tags["subclass"].ToString() : string.Empty;
            Rank = feature.Tags.ContainsKey("rank") ? int.Parse(feature.Tags["rank"].ToString()) : 0;
        }

        public VectorTileFeature Feature { get; }

        public IVectorStyleLayer Style;
    }
}

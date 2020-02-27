using VectorTiles.MapboxGL.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VectorTiles.MapboxGL.Json
{
    public class StyleLayer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("paint")]
        public Paint Paint { get; set; }

        [JsonProperty("interactive")]
        public bool Interactive { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        private string _sourceLayer;

        [JsonProperty("source-layer")]
        public string SourceLayer
        {
            get => _sourceLayer;
            set
            {
                _sourceLayer = value;
                SourceLayerHash = value.GetHashCode();
            }
        }

        [JsonProperty("filter")]
        public JArray NativeFilter { get; set; }

        public Filter.IFilter Filter { get; set; }

        //[JsonProperty("metadata")]
        //public Metadata { get; set; }

        [JsonProperty("layout")]
        public Layout Layout { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        private float? _maxZoom;

        [JsonProperty("maxzoom")]
        public float? MaxZoom
        {
            get => _maxZoom;
            set
            {
                _maxZoom = value;
                if (_maxZoom != null)
                    MinVisible = (_maxZoom ?? 30).ToResolution();
            }
        }

        private float? _minZoom;

        [JsonProperty("minzoom")]
        public float? MinZoom
        {
            get => _minZoom;
            set
            {
                _minZoom = value;
                if (_minZoom != null)
                    MaxVisible = (_minZoom ?? 0f).ToResolution();
            }
        }

        public int SourceLayerHash { get; set; }

        public double MinVisible { get; set; } = 0;

        public double MaxVisible { get; set; } = double.PositiveInfinity;

        public int ZIndex { get; set; }

        public override string ToString()
        {
            return Id + " " + Type;
        }
    }
}

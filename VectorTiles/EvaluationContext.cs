namespace VectorTiles
{
    /// <summary>
    /// Context for which the style should be evaluated
    /// </summary>
    public class EvaluationContext
    {
        public float? Zoom { get; set; }

        public int Scale { get; set; }

        public TagsCollection Tags { get; set; }

        public EvaluationContext(float? zoom, int scale = 1, TagsCollection tags = null)
        {
            Zoom = zoom;
            Scale = scale;
            Tags = tags;
        }
    }
}

using VectorTiles.Enums;

namespace VectorTiles
{
    public interface ITileDataSink
    {
        void Process(VectorElement element);

        /// <summary>
        /// Notify loader that tile loading is completed.
        /// </summary>
        /// <param name="result"></param>
        void Completed(QueryResult result);
    }
}

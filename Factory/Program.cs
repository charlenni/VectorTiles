using BruTile;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VectorTiles;
using VectorTiles.MapboxGL;

namespace Factory
{
    class Program
    {
        static List<string> stopwatchResults = new List<string>();

        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(s => string.Compare(s, "Factory.Styles.osm-liberty.json", true) == 0);

            if (string.IsNullOrEmpty(resourceName))
                return;

            var stopwatch = new System.Diagnostics.Stopwatch();

            MGLStyleFile mglStyleFile;

            stopwatch.Start();

            // Open JSON style files and read contents
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                mglStyleFile = MGLStyleLoader.Load(stream);
            };

            stopwatch.Stop();
            stopwatchResults.Add($"Elapsed time for loading style file: {stopwatch.ElapsedMilliseconds} ms");

            // Now create picture for tile
            var tile = new TileInfo
            {
                // Calc for Google link: Y = 2^zoom - Y - 1
                //Index = new TileIndex(68238, 83276, "17") // Google: https://a.tile.openstreetmap.org/17/68238/47795.png
                //Index = new TileIndex(34118, 41636, "16") // Google: https://a.tile.openstreetmap.org/16/34119/23897.png
                //Index = new TileIndex(17059, 20819, "15") // Google: https://a.tile.openstreetmap.org/15/17059/11948.png
                Index = new TileIndex(8529, 10409, "14") //  Google: https://a.tile.openstreetmap.org/14/8529/5974.png
                //Index = new TileIndex(4264, 5204, "13")
                //Index = new TileIndex(2132, 2602, "12")
                //Index = new TileIndex(1066, 1301, "11")
                //Index = new TileIndex(533, 650, "10")
                //Index = new TileIndex(266, 325, "9")
                //Index = new TileIndex(133, 163, "8")
                //Index = new TileIndex(66, 81, "7")
                //Index = new TileIndex(33, 40, "6")
                //Index = new TileIndex(16, 20, "5")
                //Index = new TileIndex(8, 10, "4")
                //Index = new TileIndex(4, 5, "3")
                //Index = new TileIndex(2, 2, "2")
                //Index = new TileIndex(1, 1, "1")
                //Index = new TileIndex(0, 0, "0")
            };

            stopwatch.Start();

            var bytes = CreateTile(mglStyleFile, tile, 512);

            stopwatch.Stop();
            stopwatchResults.Add($"Elapsed time for creating tile: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Reset();
            stopwatch.Start();

            // Now save picture
            using (var stream = File.OpenWrite("image.png"))
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            stopwatch.Stop();
            stopwatchResults.Add($"Elapsed time for writing tile: {stopwatch.ElapsedMilliseconds} ms");

            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine($"Created tile: ${resourceName}");
            foreach (var s in stopwatchResults)
                Console.WriteLine(s);
            Console.ReadKey();
        }

        /// <summary>
        /// Create image tile from all sources that are contained in the MGLStyleFile
        /// </summary>
        /// <param name="styleFile">StyleFile to use</param>
        /// <param name="tile">TileInfo of tile to draw</param>
        /// <param name="tileSize">Tile size of the image tile</param>
        /// <returns>Byte array of image data</returns>
        private static byte[] CreateTile(MGLStyleFile styleFile, TileInfo tile, int tileSize)
        {
            var recorder = new SKPictureRecorder();
            var stopwatch = new System.Diagnostics.Stopwatch();

            recorder.BeginRecording(new SKRect(0, 0, tileSize, tileSize));

            var canvas = recorder.RecordingCanvas;

            canvas.ClipRect(new SKRect(0, 0, tileSize, tileSize));

            if (styleFile.TileSources != null && styleFile.TileSources.Count > 0)
            {
                // We have one or more sources, so draw each source after the other
                foreach (var source in styleFile.TileSources)
                {
                    stopwatch.Start();

                    var vectorDrawable = source.GetVectorTile(tile);

                    stopwatch.Stop();
                    stopwatchResults.Add($"Ellapsed time for GetDrawable of ${source.Name}: {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Reset();

                    stopwatch.Start();

                    if (vectorDrawable is SKDrawable drawable)
                    {
                        vectorDrawable.Context.Zoom = float.Parse(tile.Index.Level);
                        vectorDrawable.Context.Tags = null;

                        canvas.Save();
                        canvas.Scale(tileSize / drawable.Bounds.Width);
                        canvas.DrawDrawable(drawable, 0, 0);
                        canvas.Restore();
                    }

                    stopwatch.Stop();
                    stopwatchResults.Add($"Ellapsed time for DrawDrawable of ${source.Name}: {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Reset();
                }
            }

            stopwatch.Start();

            var byteArray = SKImage.FromPicture(recorder.EndRecording(), new SKSizeI(tileSize, tileSize), new SKPaint() { IsAntialias = true }).Encode().ToArray();

            stopwatch.Stop();
            stopwatchResults.Add($"Ellapsed time for drawing picture: {stopwatch.ElapsedMilliseconds} ms");

            return byteArray;
        }
    }
}

using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using SkiaSharp;
using System;

namespace Mapsui.Samples.Forms
{
    public class DrawableTileStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache)
        {
            var vectorTile = ((DrawableTile)feature.Geometry).Data;
            vectorTile.Context.Zoom = (float)viewport.Resolution.ToZoomLevel();

			var boundingBox = feature.Geometry.BoundingBox;

			if (viewport.IsRotated)
			{
				var priorMatrix = canvas.TotalMatrix;

				var matrix = CreateRotationMatrix(viewport, boundingBox, priorMatrix);

				canvas.SetMatrix(matrix);

				var destination = new BoundingBox(0.0, 0.0, boundingBox.Width, boundingBox.Height).ToSkia();

				canvas.DrawDrawable(vectorTile, destination.Left, destination.Top);
			}
			else
			{
                var destination = RoundToPixel(WorldToScreen(viewport, feature.Geometry.BoundingBox)).ToSkia();
                var clipRect = vectorTile.Bounds;

                var scale = Math.Max(destination.Width, destination.Height) / vectorTile.Bounds.Width;
                vectorTile.Context.Scale = 1f / scale;

                canvas.Translate(new SKPoint(destination.Left, destination.Top));
                canvas.Scale(scale);
                canvas.ClipRect(clipRect);
                canvas.DrawDrawable(vectorTile, 0, 0);
                
                var frame = vectorTile.Bounds;
                canvas.DrawRect(frame, new SKPaint() { Style = SKPaintStyle.Stroke, Color = SKColors.Red });
			}

            return true;
        }

        private static SKMatrix CreateRotationMatrix(IReadOnlyViewport viewport, BoundingBox boundingBox, SKMatrix priorMatrix)
        {
            SKMatrix matrix = SKMatrix.MakeIdentity();

            // The front-end sets up the canvas with a matrix based on screen scaling (e.g. retina).
            // We need to retain that effect by combining our matrix with the incoming matrix.

            // We'll create four matrices in addition to the incoming matrix. They perform the
            // zoom scale, focal point offset, user rotation and finally, centering in the screen.

            var userRotation = SKMatrix.MakeRotationDegrees((float)viewport.Rotation);
            var focalPointOffset = SKMatrix.MakeTranslation(
                (float)(boundingBox.Left - viewport.Center.X),
                (float)(viewport.Center.Y - boundingBox.Top));
            var zoomScale = SKMatrix.MakeScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
            var centerInScreen = SKMatrix.MakeTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

            // We'll concatenate them like so: incomingMatrix * centerInScreen * userRotation * zoomScale * focalPointOffset

            SKMatrix.Concat(ref matrix, zoomScale, focalPointOffset);
            SKMatrix.Concat(ref matrix, userRotation, matrix);
            SKMatrix.Concat(ref matrix, centerInScreen, matrix);
            SKMatrix.Concat(ref matrix, priorMatrix, matrix);

            return matrix;
        }

        private static BoundingBox WorldToScreen(IReadOnlyViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
            (
                Math.Min(first.X, second.X),
                Math.Min(first.Y, second.Y),
                Math.Max(first.X, second.X),
                Math.Max(first.Y, second.Y)
            );
        }

        private static BoundingBox RoundToPixel(BoundingBox boundingBox)
        {
            return new BoundingBox(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}

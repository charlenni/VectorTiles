using System;
using System.Collections.Generic;
using System.Linq;
using Point = SkiaSharp.SKPoint;
using Rect = SkiaSharp.SKRect;

namespace VectorTiles
{
    public class TileClipper
    {
        /// <summary>
        /// Type of intersection
        /// </summary>
        public enum Intersection
        {
            CompleteInside,
            CompleteOutside,
            Both,
            FirstInside,
            SecondInside,
            Unknown
        }

        Rect clipRect;

        public TileClipper(Rect rect)
        {
            clipRect = rect;
        }

        /// <summary>
        /// Converts a LineString (list of Mapsui points) in world coordinates to a Skia path
        /// </summary>
        /// <param name="lineString">List of points in Mapsui world coordinates</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All lines outside aren't drawn.</param>
        /// <returns></returns>
        public List<List<Point>> ReduceLinePointsToClipRect(List<Point> points)
        {
            Point lastPoint = Point.Empty;
            List<Point> output = null;
            List<List<Point>> result = new List<List<Point>>();

            for (var i = 1; i < points.Count; i++)
            {
                // Check each part of LineString, if it is inside or intersects the clipping rectangle
                var intersect = LiangBarskyClip(points[i - 1], points[i], clipRect, out var intersectionPoint1, out var intersectionPoint2);

                if (intersect != Intersection.CompleteOutside)
                {
                    // If the last point isn't the same as actuall starting point ...
                    if (lastPoint.IsEmpty || !lastPoint.Equals(intersectionPoint1))
                    {
                        // ... than move to this point
                        output = new List<Point>();
                        output.Add(intersectionPoint1);
                        result.Add(output);
                    }
                    // Draw line
                    output?.Add(intersectionPoint2);

                    // Save last end point for later use
                    lastPoint = intersectionPoint2;
                }
            }

            return result;
        }

        /// <summary>
        /// A Liang-Barsky implementation to detect the intersection between a line and a rect.
        /// With this, all lines, that aren't visible on screen could be sorted out.
        /// Found at https://gist.github.com/ChickenProp/3194723
        /// </summary>
        /// <param name="point1">First point of line</param>
        /// <param name="point2">Second point of line</param>
        /// <param name="clipRect"></param>
        /// <param name="intersectionPoint1">First intersection point </param>
        /// <param name="intersectionPoint2">Second intersection point</param>
        /// <returns></returns>
        private static Intersection LiangBarskyClip(Point point1, Point point2, Rect clipRect, out Point intersectionPoint1, out Point intersectionPoint2)
        {
            var vx = point2.X - point1.X;
            var vy = point2.Y - point1.Y;
            var p = new[] { -vx, vx, -vy, vy };
            var q = new[] { point1.X - clipRect.Left, clipRect.Right - point1.X, point1.Y - clipRect.Top, clipRect.Bottom - point1.Y };
            var u1 = float.NegativeInfinity;
            var u2 = float.PositiveInfinity;

            // Up to now both points are inside the clipping rectangle
            intersectionPoint1 = point1;
            intersectionPoint2 = point2;

            // Check, if points are complete outside
            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    // Line is parallel to one side
                    if (q[i] < 0)
                        return Intersection.CompleteOutside;
                }
                else
                {
                    // Calculate intersection points
                    var t = q[i] / p[i];
                    if (p[i] < 0 && u1 < t)
                        u1 = t;
                    else if (p[i] > 0 && u2 > t)
                        u2 = t;
                }
            }

            // Are both points outside and don't intersect?
            if (u1 > u2)
                return Intersection.CompleteOutside;

            // Are both points inside and don't intersect?
            if (u1 < 0 && u2 > 1)
            {
                return Intersection.CompleteInside;
            }

            // Are both points outside, but intersect on both sides?
            if (u1 > 0 && u2 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.Both;
            }

            // Is the first point outside and the second point inside?
            if (u1 > 0 && u1 < 1)
            {
                intersectionPoint1.X = point1.X + u1 * vx;
                intersectionPoint1.Y = point1.Y + u1 * vy;

                return Intersection.SecondInside;
            }

            // Is the first point inside and the second point outside?
            if (u2 > 0 && u2 < 1)
            {
                intersectionPoint2.X = point1.X + u2 * vx;
                intersectionPoint2.Y = point1.Y + u2 * vy;

                return Intersection.FirstInside;
            }

            return Intersection.Unknown;
        }

        /// <summary>
        /// Comparer for each side of the clipping rectangle to check, if a point 
        /// is inside or outside of this edge.
        /// There are 4 edges (left, top, right, bottom).
        /// </summary>
        private static readonly Func<Point, Rect, bool>[] Comparer = new Func<Point, Rect, bool>[]
            {
            (point, rect) => point.X > rect.Left, // Left edge of rect
            (point, rect) => point.Y > rect.Top, // Top edge of rect
            (point, rect) => point.X < rect.Right, // Right edge of rect
            (point, rect) => point.Y < rect.Bottom, // Bottom edge of rect
            };

        /// <summary>
        /// Calculates the intersection point of line between pointStart and pointEnd 
        /// and the edge.
        /// There are 4 edges (left, top, right, bottom).
        /// </summary>
        private static readonly Func<Point, Point, Rect, Point>[] Intersecter = new Func<Point, Point, Rect, Point>[]
        {
            (pointStart, pointEnd, rect) => new Point(rect.Left, pointStart.Y + (rect.Left-pointStart.X)/(pointEnd.X-pointStart.X)*(pointEnd.Y-pointStart.Y)), // Left edge of rect
            (pointStart, pointEnd, rect) => new Point(pointStart.X + (rect.Top-pointStart.Y)/(pointEnd.Y-pointStart.Y)*(pointEnd.X-pointStart.X), rect.Top),   // Top edge of rect
            (pointStart, pointEnd, rect) => new Point(rect.Right, pointEnd.Y + (rect.Right-pointEnd.X)/(pointStart.X-pointEnd.X)*(pointStart.Y-pointEnd.Y)),   // Right edge of rect
            (pointStart, pointEnd, rect) => new Point(pointEnd.X + (rect.Bottom-pointEnd.Y)/(pointStart.Y-pointEnd.Y)*(pointStart.X-pointEnd.X), rect.Bottom), // Bottom edge of rect
        };

        /// <summary>
        /// Reduce list of points, so that all are inside of clipRect
        /// See https://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorithm
        /// </summary>
        /// <param name="points">List of points to reduce</param>
        /// <param name="viewport">Viewport implementation</param>
        /// <param name="clipRect">Rectangle to clip to. All points outside aren't drawn.</param>
        /// <returns></returns>
        public List<Point> ReducePolygonPointsToClipRect(List<Point> points)
        {
            // New input list is the last output list of points
            var input = points;
            var output = new List<Point>();

            // Do this for the 4 edges (left, top, right, bottom) of clipping rectangle
            for (var j = 0; j < 4; j++)
            {
                // If there aren't any points to reduce
                if (input == null || input.Count == 0)
                    return new List<Point>();

                output.Clear();

                var pointStart = input.Last();

                foreach (var pointEnd in input)
                {
                    // Is pointEnd inside of clipping rectangle regarding this edge
                    if (Comparer[j](pointEnd, clipRect))
                    {
                        // Is pointStart outside of clipping rectangle regarding this edge
                        if (!Comparer[j](pointStart, clipRect))
                        {
                            // Yes, than line is coming from outside to inside, so calculate intersection
                            output.Add(Intersecter[j](pointStart, pointEnd, clipRect));
                        }
                        // pointEnd is inside, so add it to points list
                        output.Add(pointEnd);
                    }
                    // Is pointStart inside of clipping rectangle regarding this edge
                    else if (Comparer[j](pointStart, clipRect))
                    {
                        // Yes, than line is coming from inside to outside, so calculate intersection
                        output.Add(Intersecter[j](pointStart, pointEnd, clipRect));
                    }

                    // Set next pointStart
                    pointStart = pointEnd;
                }

                // Now swap input and output, so we don't have to create a new list
                var swap = output;
                output = input;
                input = swap;
            }

            return output;
        }
    }
}

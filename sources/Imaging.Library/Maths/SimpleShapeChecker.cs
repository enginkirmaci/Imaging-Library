using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Extensions;
using System;
using System.Collections.Generic;

namespace Imaging.Library.Maths
{
    public class SimpleShapeChecker
    {
        private float angleError = 7;
        private float lengthError = 0.1f;

        private float minAcceptableDistortion = 0.5f;
        private float relativeDistortionLimit = 0.03f;
        private readonly FlatAnglesOptimizer shapeOptimizer = new FlatAnglesOptimizer(160);

        /// <summary>
        ///     Minimum value of allowed shapes' distortion.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The property sets minimum value for allowed shapes'
        ///         distortion (in pixels). See documentation to <see cref="SimpleShapeChecker" />
        ///         class for more details about this property.
        ///     </para>
        ///     <para>Default value is set to <b>0.5</b>.</para>
        /// </remarks>
        public float MinAcceptableDistortion
        {
            get { return minAcceptableDistortion; }
            set { minAcceptableDistortion = Math.Max(0, value); }
        }

        /// <summary>
        ///     Maximum value of allowed shapes' distortion, [0, 1].
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The property sets maximum value for allowed shapes'
        ///         distortion. The value is measured in [0, 1] range, which corresponds
        ///         to [0%, 100%] range, which means that maximum allowed shapes'
        ///         distortion is calculated relatively to shape's size. This results to
        ///         higher allowed distortion level for bigger shapes and smaller allowed
        ///         distortion for smaller shapers. See documentation to <see cref="SimpleShapeChecker" />
        ///         class for more details about this property.
        ///     </para>
        ///     <para>Default value is set to <b>0.03</b> (3%).</para>
        /// </remarks>
        public float RelativeDistortionLimit
        {
            get { return relativeDistortionLimit; }
            set { relativeDistortionLimit = Math.Max(0, Math.Min(1, value)); }
        }

        /// <summary>
        ///     Maximum allowed angle error in degrees, [0, 20].
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The value sets maximum allowed difference between two angles to
        ///         treat them as equal. It is used by <see cref="CheckPolygonSubType" /> method to
        ///         check for parallel lines and angles of triangles and quadrilaterals.
        ///         For example, if angle between two lines equals 5 degrees and this properties value
        ///         is set to 7, then two compared lines are treated as parallel.
        ///     </para>
        ///     <para>Default value is set to <b>7</b>.</para>
        /// </remarks>
        public float AngleError
        {
            get { return angleError; }
            set { angleError = Math.Max(0, Math.Min(20, value)); }
        }

        /// <summary>
        ///     Maximum allowed difference in sides' length (relative to shapes' size), [0, 1].
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The values sets maximum allowed difference between two sides' length
        ///         to treat them as equal. The error value is set relative to shapes size and measured
        ///         in [0, 1] range, which corresponds to [0%, 100%] range. Absolute length error in pixels
        ///         is calculated as:
        ///         <code lang="none">
        ///  LengthError * ( width + height ) / 2
        ///  </code>
        ///         , where <b>width</b> and <b>height</b> is the size of bounding rectangle for the
        ///         specified shape.
        ///     </para>
        ///     <para>Default value is set to <b>0.1</b> (10%).</para>
        /// </remarks>
        public float LengthError
        {
            get { return lengthError; }
            set { lengthError = Math.Max(0, Math.Min(1, value)); }
        }

        /// <summary>
        ///     Check type of the shape formed by specified points.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <returns>Returns type of the detected shape.</returns>
        public ShapeType CheckShapeType(List<Point> edgePoints)
        {
            if (IsCircle(edgePoints))
                return ShapeType.Circle;

            // check for convex polygon
            List<Point> corners;

            if (IsConvexPolygon(edgePoints, out corners))
                return corners.Count == 4 ? ShapeType.Quadrilateral : ShapeType.Triangle;

            return ShapeType.Unknown;
        }

        /// <summary>
        ///     Check if the specified set of points form a circle shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     circle shape or <see langword="false" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <note>
        ///             Circle shape must contain at least 8 points to be recognized.
        ///             The method returns <see langword="false" /> always, of number of points in the specified
        ///             shape is less than 8.
        ///         </note>
        ///     </para>
        /// </remarks>
        public bool IsCircle(List<Point> edgePoints)
        {
            Point center;
            float radius;

            return IsCircle(edgePoints, out center, out radius);
        }

        /// <summary>
        ///     Check if the specified set of points form a circle shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="center">Receives circle's center on successful return.</param>
        /// <param name="radius">Receives circle's radius on successful return.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     circle shape or <see langword="false" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <note>
        ///             Circle shape must contain at least 8 points to be recognized.
        ///             The method returns <see langword="false" /> always, of number of points in the specified
        ///             shape is less than 8.
        ///         </note>
        ///     </para>
        /// </remarks>
        public bool IsCircle(List<Point> edgePoints, out Point center, out float radius)
        {
            // make sure we have at least 8 points for curcle shape
            if (edgePoints.Count < 8)
            {
                center = new Point(0, 0);
                radius = 0;
                return false;
            }

            // get bounding rectangle of the points list
            Point minXY, maxXY;
            PointsCloud.GetBoundingRectangle(edgePoints, out minXY, out maxXY);
            // get cloud's size
            var cloudSize = maxXY.Subtract(minXY);
            // calculate center point
            center = minXY.Add(cloudSize.Divide(2));

            radius = (float)((cloudSize.X + cloudSize.Y) / 4);

            // calculate mean distance between provided edge points and estimated circle’s edge
            float meanDistance = 0;

            for (int i = 0, n = edgePoints.Count; i < n; i++)
                meanDistance += (float)Math.Abs(center.DistanceTo(edgePoints[i]) - radius);
            meanDistance /= edgePoints.Count;

            var maxDitance = Math.Max(minAcceptableDistortion,
                (float)(cloudSize.X + cloudSize.Y) / 2 * relativeDistortionLimit);

            return meanDistance <= maxDitance;
        }

        /// <summary>
        ///     Check if the specified set of points form a quadrilateral shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     quadrilateral shape or <see langword="false" /> otherwise.
        /// </returns>
        public bool IsQuadrilateral(List<Point> edgePoints)
        {
            List<Point> corners;
            return IsQuadrilateral(edgePoints, out corners);
        }

        /// <summary>
        ///     Check if the specified set of points form a quadrilateral shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of quadrilateral corners on successful return.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     quadrilateral shape or <see langword="false" /> otherwise.
        /// </returns>
        public bool IsQuadrilateral(List<Point> edgePoints, out List<Point> corners)
        {
            corners = GetShapeCorners(edgePoints);

            if (corners.Count != 4)
                return false;

            return CheckIfPointsFitShape(edgePoints, corners);
        }

        /// <summary>
        ///     Check if the specified set of points form a triangle shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     triangle shape or <see langword="false" /> otherwise.
        /// </returns>
        public bool IsTriangle(List<Point> edgePoints)
        {
            List<Point> corners;
            return IsTriangle(edgePoints, out corners);
        }

        /// <summary>
        ///     Check if the specified set of points form a triangle shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of triangle corners on successful return.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     triangle shape or <see langword="false" /> otherwise.
        /// </returns>
        public bool IsTriangle(List<Point> edgePoints, out List<Point> corners)
        {
            corners = GetShapeCorners(edgePoints);

            if (corners.Count != 3)
                return false;

            return CheckIfPointsFitShape(edgePoints, corners);
        }

        /// <summary>
        ///     Check if the specified set of points form a convex polygon shape.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">List of polygon corners on successful return.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified set of points form a
        ///     convex polygon shape or <see langword="false" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <note>
        ///             The method is able to detect only triangles and quadrilaterals
        ///             for now. Check number of detected corners to resolve type of the detected polygon.
        ///         </note>
        ///     </para>
        /// </remarks>
        public bool IsConvexPolygon(List<Point> edgePoints, out List<Point> corners)
        {
            corners = GetShapeCorners(edgePoints);
            return CheckIfPointsFitShape(edgePoints, corners);
        }

        /// <summary>
        ///     Check if a shape specified by the set of points fits a convex polygon
        ///     specified by the set of corners.
        /// </summary>
        /// <param name="edgePoints">Shape's points to check.</param>
        /// <param name="corners">Corners of convex polygon to check fitting into.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the specified shape fits
        ///     the specified convex polygon or <see langword="false" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         The method checks if the set of specified points form the same shape
        ///         as the set of provided corners.
        ///     </para>
        /// </remarks>
        public bool CheckIfPointsFitShape(List<Point> edgePoints, List<Point> corners)
        {
            var cornersCount = corners.Count;

            // lines coefficients (for representation as y(x)=k*x+b)
            var k = new float[cornersCount];
            var b = new float[cornersCount];
            var div = new float[cornersCount]; // precalculated divisor
            var isVert = new bool[cornersCount];

            for (var i = 0; i < cornersCount; i++)
            {
                var currentPoint = corners[i];
                var nextPoint = i + 1 == cornersCount ? corners[0] : corners[i + 1];

                if (!(isVert[i] = nextPoint.X == currentPoint.X))
                {
                    k[i] = (float)(nextPoint.Y - currentPoint.Y) / (float)(nextPoint.X - currentPoint.X);
                    b[i] = (float)currentPoint.Y - k[i] * (float)currentPoint.X;
                    div[i] = (float)Math.Sqrt(k[i] * k[i] + 1);
                }
            }

            // calculate distances between edge points and polygon sides
            float meanDistance = 0;

            for (int i = 0, n = edgePoints.Count; i < n; i++)
            {
                var minDistance = float.MaxValue;

                for (var j = 0; j < cornersCount; j++)
                {
                    float distance = 0;

                    if (!isVert[j])
                        distance = (float)Math.Abs((k[j] * edgePoints[i].X + b[j] - edgePoints[i].Y) / div[j]);
                    else
                        distance = Math.Abs((float)edgePoints[i].X - (float)corners[j].X);

                    if (distance < minDistance)
                        minDistance = distance;
                }

                meanDistance += minDistance;
            }
            meanDistance /= edgePoints.Count;

            // get bounding rectangle of the corners list
            Point minXY, maxXY;
            PointsCloud.GetBoundingRectangle(corners, out minXY, out maxXY);
            var rectSize = maxXY.Subtract(minXY);

            var maxDitance = Math.Max(minAcceptableDistortion,
                (float)(rectSize.X + rectSize.Y) / 2 * relativeDistortionLimit);

            return meanDistance <= maxDitance;
        }

        // Get optimized quadrilateral area
        private List<Point> GetShapeCorners(List<Point> edgePoints)
        {
            return shapeOptimizer.OptimizeShape(PointsCloud.FindQuadrilateralCorners(edgePoints));
        }
    }
}
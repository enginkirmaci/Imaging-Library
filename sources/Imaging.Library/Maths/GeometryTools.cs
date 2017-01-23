using Imaging.Library.Entities;
using Imaging.Library.Extensions;
using System;

namespace Imaging.Library.Maths
{
    public static class GeometryTools
    {
        public static float GetAngleBetweenVectors(Point startPoint, Point vector1end, Point vector2end)
        {
            var x1 = vector1end.X - startPoint.X;
            var y1 = vector1end.Y - startPoint.Y;

            var x2 = vector2end.X - startPoint.X;
            var y2 = vector2end.Y - startPoint.Y;

            return
                (float)
                (Math.Acos((x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2))) * 180.0 /
                 Math.PI);
        }

        public static float GetAngleBetweenLines(Point a1, Point a2, Point b1, Point b2)
        {
            var line1 = FromPoints(a1, a2);
            return line1.GetAngleBetweenLines(FromPoints(b1, b2));
        }

        private static Line FromPoints(Point point1, Point point2)
        {
            return new Line
            {
                X1 = point1.X,
                X2 = point2.X,
                Y1 = point1.Y,
                Y2 = point2.Y
            };
        }
    }
}
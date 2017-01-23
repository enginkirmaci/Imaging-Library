using Imaging.Library.Entities;
using System;

namespace Imaging.Library.Extensions
{
    public static class PointExtension
    {
        public static Point Add(this Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static Point Add(this Point point, double valueToAdd)
        {
            return new Point(point.X + valueToAdd, point.Y + valueToAdd);
        }

        public static Point Subtract(this Point point1, Point point2)
        {
            return new Point(point1.X - point2.X, point1.Y - point2.Y);
        }

        public static Point Divide(this Point point, int factor)
        {
            return new Point(point.X / factor, point.Y / factor);
        }

        public static float DistanceTo(this Point point, Point anotherPoint)
        {
            var dx = (int)(point.X - anotherPoint.X);
            var dy = (int)(point.Y - anotherPoint.Y);

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static Point Mirror(this Point point, Point[] line)
        {
            double dx, dy, a, b;
            double x2, y2;

            dx = line[1].X - line[0].X;
            dy = line[1].Y - line[0].Y;

            a = (dx * dx - dy * dy) / (dx * dx + dy * dy);
            b = 2 * dx * dy / (dx * dx + dy * dy);

            x2 = a * (point.X - line[0].X) + b * (point.Y - line[0].Y) + line[0].X;
            y2 = b * (point.X - line[0].X) - a * (point.Y - line[0].Y) + line[0].Y;

            point.X = x2;
            point.Y = y2;

            return point;
        }
    }
}
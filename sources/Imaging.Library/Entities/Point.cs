using System;

namespace Imaging.Library.Entities
{
    public struct Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public static bool operator ==(Point c1, Point c2)
        {
            return c1.X.Equals(c2.X) && c1.Y.Equals(c2.Y);
        }

        public static bool operator !=(Point c1, Point c2)
        {
            return !(c1.X.Equals(c2.X) && c1.Y.Equals(c2.Y));
        }

        public double DistanceTo(Point destination)
        {
            return Math.Sqrt(Math.Pow(destination.X - X, 2) + Math.Pow(destination.Y - Y, 2));
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
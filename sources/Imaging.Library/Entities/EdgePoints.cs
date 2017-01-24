using Imaging.Library.Maths;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imaging.Library.Entities
{
    public struct EdgePoints
    {
        //public EdgePoints()
        //{
        //    TopLeft = new Point();
        //    TopRight = new Point();
        //    BottomLeft = new Point();
        //    BottomRight = new Point();

        //    Bounds = Rectangle.Empty;
        //}

        public EdgePoints(EdgePoints edgePoints)
        {
            TopLeft = edgePoints.TopLeft;
            TopRight = edgePoints.TopRight;
            BottomLeft = edgePoints.BottomLeft;
            BottomRight = edgePoints.BottomRight;

            Bounds = edgePoints.Bounds;
        }

        public EdgePoints(Rectangle bounds)
        {
            TopLeft = new Point();
            TopRight = new Point();
            BottomLeft = new Point();
            BottomRight = new Point();

            Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
        public Rectangle Bounds { get; set; }

        public EdgePoints ZoomIn(double scale)
        {
            var result = new EdgePoints(Bounds)
            {
                TopLeft = new Point(TopLeft.X / scale, TopLeft.Y / scale),
                TopRight = new Point(TopRight.X / scale, TopRight.Y / scale),
                BottomLeft = new Point(BottomLeft.X / scale, BottomLeft.Y / scale),
                BottomRight = new Point(BottomRight.X / scale, BottomRight.Y / scale)
            };

            return result;
        }

        public bool SetPoints(Point[] points)
        {
            TopLeft = new Point(points[0].X, points[0].Y);
            TopRight = new Point(points[1].X, points[1].Y);
            BottomLeft = new Point(points[2].X, points[2].Y);
            BottomRight = new Point(points[3].X, points[3].Y);

            return CorrectPoints();
        }

        public void SetMaximum()
        {
            TopLeft = new Point(Bounds.X, Bounds.Y);
            TopRight = new Point(Bounds.X + Bounds.Width, Bounds.Y);
            BottomLeft = new Point(Bounds.X, Bounds.Y + Bounds.Height);
            BottomRight = new Point(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height);
        }

        public bool CorrectPoints()
        {
            var ch = ConvexHull.CH2(new List<Point> { TopLeft, TopRight, BottomLeft, BottomRight }, true);
            var emptyPoint = new Point();

            if (ch.Count != 4 || ch.FindAll(i => i == emptyPoint).Count > 1)
            {
                SetMaximum();

                return false;
            }

            var pointList = new List<Point> { TopLeft, TopRight, BottomLeft, BottomRight }.OrderBy(i => i.Y);
            var pointTopList = new List<Point> { pointList.ElementAt(0), pointList.ElementAt(1) }.OrderBy(i => i.X);

            var topLeftPoint = pointTopList.First();

            var current = ch.IndexOf(topLeftPoint);

            TopLeft = ch.ElementAt(current);
            TopRight = ch.ElementAt((current + 1) % 4);
            BottomLeft = ch.ElementAt((current + 3) % 4);
            BottomRight = ch.ElementAt((current + 2) % 4);

            return true;
        }

        public Size EstimatedRectangleSize()
        {
            //Method 1
            var heightLeft = Math.Abs(BottomLeft.Y - TopLeft.Y);
            var heightRight = Math.Abs(BottomRight.Y - TopRight.Y);

            var widthTop = Math.Abs(TopRight.X - TopLeft.X);
            var widthBottom = Math.Abs(BottomRight.X - BottomLeft.X);

            return new Size()
            {
                Width = (int)Math.Min(widthTop, widthBottom),
                Height = (int)Math.Min(heightRight, heightLeft)
            };

            //return new Size()
            //{
            //    Width = (int)((widthTop + widthBottom) / 2),
            //    Height = (int)((heightLeft + heightRight) / 2)
            //};

            ////Method 2
            //var newWidth = (int)Math.Max(TopLeft.DistanceTo(TopRight), BottomLeft.DistanceTo(BottomRight));
            //var newHeight = (int)Math.Max(TopLeft.DistanceTo(BottomLeft), TopRight.DistanceTo(BottomRight));

            //return new Size
            //{
            //    Width = newWidth,
            //    Height = newHeight
            //};
        }
    }
}
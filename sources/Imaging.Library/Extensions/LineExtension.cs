using Imaging.Library.Entities;
using System;

namespace Imaging.Library.Extensions
{
    public static class LineExtension
    {
        public static float GetAngleBetweenLines(this Line line, Line secondLine)
        {
            var k = (line.Y2 - line.Y1) / (line.X2 - line.X1);
            var k2 = (secondLine.Y2 - secondLine.Y1) / (secondLine.X2 - secondLine.X1);

            var isVertical1 = double.IsInfinity(k);
            var isVertical2 = double.IsInfinity(k2);

            // check if lines are parallel
            if (k == k2 || isVertical1 && isVertical2)
                return 0;

            float angle = 0;

            if (!isVertical1 && !isVertical2)
            {
                var tanPhi = (k2 > k ? k2 - k : k - k2) / (1 + k * k2);
                angle = (float)Math.Atan(tanPhi);
            }
            else
            {
                // one of the lines is parallel to Y axis

                if (isVertical1)
                    angle = (float)(Math.PI / 2 - Math.Atan(k2) * Math.Sign(k2));
                else
                    angle = (float)(Math.PI / 2 - Math.Atan(k) * Math.Sign(k));
            }

            // convert radians to degrees
            angle *= (float)(180.0 / Math.PI);

            if (angle < 0)
                angle = -angle;

            return angle;
        }
    }
}
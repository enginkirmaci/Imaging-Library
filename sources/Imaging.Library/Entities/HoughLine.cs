using System;

namespace Imaging.Library.Entities
{
    public class HoughLine : IComparable
    {
        public readonly double Theta;

        public readonly short Radius;

        public readonly short Intensity;

        public readonly double RelativeIntensity;

        public HoughLine(double theta, short radius, short intensity, double relativeIntensity)
        {
            Theta = theta;
            Radius = radius;
            Intensity = intensity;
            RelativeIntensity = relativeIntensity;
        }

        public int CompareTo(object value)
        {
            return (-Intensity.CompareTo(((HoughLine)value).Intensity));
        }
    }
}
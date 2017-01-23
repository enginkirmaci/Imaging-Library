using Imaging.Library.Entities;
using System;
using System.Collections.Generic;

namespace Imaging.Library.Filters.ComplexFilters
{
    public class HoughLineTransformation : FilterBase
    {
        // Hough transformation quality settings

        private int stepsPerDegree;
        private int houghHeight;
        private double thetaStep;

        // precalculated Sine and Cosine values

        private double[] sinMap;
        private double[] cosMap;

        // Hough map

        private short[,] houghMap;
        private short maxMapIntensity = 0;

        private int localPeakRadius = 4;
        private short minLineIntensity = 10;
        public List<HoughLine> lines = new List<HoughLine>();

        public int StepsPerDegree
        {
            get { return stepsPerDegree; }
            set
            {
                stepsPerDegree = Math.Max(1, Math.Min(10, value));
                houghHeight = 180 * stepsPerDegree;
                thetaStep = Math.PI / houghHeight;

                // precalculate Sine and Cosine values
                sinMap = new double[houghHeight];
                cosMap = new double[houghHeight];

                for (int i = 0; i < houghHeight; i++)
                {
                    sinMap[i] = Math.Sin(i * thetaStep);
                    cosMap[i] = Math.Cos(i * thetaStep);
                }
            }
        }

        public short MinLineIntensity
        {
            get { return minLineIntensity; }
            set { minLineIntensity = value; }
        }

        public int LocalPeakRadius
        {
            get { return localPeakRadius; }
            set { localPeakRadius = Math.Max(1, Math.Min(10, value)); }
        }

        public short MaxIntensity
        {
            get { return maxMapIntensity; }
        }

        public int LinesCount
        {
            get { return lines.Count; }
        }

        public HoughLineTransformation()
        {
            StepsPerDegree = 1;
        }

        public override void OnProcess()
        {
            // get source image size
            int width = Source.Width;
            int height = Source.Height;
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            // make sure the specified rectangle recides with the source image
            var rect = new Rectangle(0, 0, width, height);

            int startX = -halfWidth + (int)rect.Left;
            int startY = -halfHeight + (int)rect.Top;
            int stopX = width - halfWidth - (width - (int)rect.Right);
            int stopY = height - halfHeight - (height - (int)rect.Bottom);

            //int offset = image.Stride - rect.Width;

            // calculate Hough map's width
            int halfHoughWidth = (int)Math.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
            int houghWidth = halfHoughWidth * 2;

            houghMap = new short[houghHeight, houghWidth];

            // do the job

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    // for each Theta value
                    for (int theta = 0; theta < houghHeight; theta++)
                    {
                        int radius = (int)Math.Round(cosMap[theta] * x - sinMap[theta] * y) + halfHoughWidth;

                        if ((radius < 0) || (radius >= houghWidth))
                            continue;

                        houghMap[theta, radius]++;
                    }

            // find max value in Hough map
            maxMapIntensity = 0;
            for (int i = 0; i < houghHeight; i++)
            {
                for (int j = 0; j < houghWidth; j++)
                {
                    if (houghMap[i, j] > maxMapIntensity)
                    {
                        maxMapIntensity = houghMap[i, j];
                    }
                }
            }

            CollectLines();

            //width = houghMap.GetLength(1);
            //height = houghMap.GetLength(0);

            //float scale = 255.0f / maxMapIntensity;
            //Color c = new Color()
            //{
            //    A = 0,
            //    R = 0,
            //    G = 0,
            //    B = 0
            //};
            //var color = FromColor(c);

            //// do the job
            //for (int y = 0; y < height; y++)
            //    for (int x = 0; x < width; x++)
            //        targetPixelRegion.ImagePixels[DimensionConverter.To1D(x, y, width)] = color; // (byte)System.Math.Min(255, (int)(scale * houghMap[y, x]));
        }

        public HoughLine[] GetMostIntensiveLines(int count)
        {
            // lines count
            int n = Math.Min(count, lines.Count);

            // result array
            HoughLine[] dst = new HoughLine[n];
            lines.CopyTo(0, dst, 0, n);

            return dst;
        }

        public HoughLine[] GetLinesByRelativeIntensity(double minRelativeIntensity)
        {
            int count = 0, n = lines.Count;

            while ((count < n) && (((HoughLine)lines[count]).RelativeIntensity >= minRelativeIntensity))
                count++;

            return GetMostIntensiveLines(count);
        }

        private void CollectLines()
        {
            int maxTheta = houghMap.GetLength(0);
            int maxRadius = houghMap.GetLength(1);

            short intensity;
            bool foundGreater;

            int halfHoughWidth = maxRadius >> 1;

            // clean lines collection
            lines.Clear();

            // for each Theta value
            for (int theta = 0; theta < maxTheta; theta++)
            {
                // for each Radius value
                for (int radius = 0; radius < maxRadius; radius++)
                {
                    // get current value
                    intensity = houghMap[theta, radius];

                    if (intensity < minLineIntensity)
                        continue;

                    foundGreater = false;

                    // check neighboors
                    for (int tt = theta - localPeakRadius, ttMax = theta + localPeakRadius; tt < ttMax; tt++)
                    {
                        // break if it is not local maximum
                        if (foundGreater == true)
                            break;

                        int cycledTheta = tt;
                        int cycledRadius = radius;

                        // check limits
                        if (cycledTheta < 0)
                        {
                            cycledTheta = maxTheta + cycledTheta;
                            cycledRadius = maxRadius - cycledRadius;
                        }
                        if (cycledTheta >= maxTheta)
                        {
                            cycledTheta -= maxTheta;
                            cycledRadius = maxRadius - cycledRadius;
                        }

                        for (int tr = cycledRadius - localPeakRadius, trMax = cycledRadius + localPeakRadius; tr < trMax; tr++)
                        {
                            // skip out of map values
                            if (tr < 0)
                                continue;
                            if (tr >= maxRadius)
                                break;

                            // compare the neighboor with current value
                            if (houghMap[cycledTheta, tr] > intensity)
                            {
                                foundGreater = true;
                                break;
                            }
                        }
                    }

                    // was it local maximum ?
                    if (!foundGreater)
                    {
                        // we have local maximum
                        lines.Add(new HoughLine((double)theta / stepsPerDegree, (short)(radius - halfHoughWidth), intensity, (double)intensity / maxMapIntensity));
                    }
                }
            }

            lines.Sort();
        }
    }
}
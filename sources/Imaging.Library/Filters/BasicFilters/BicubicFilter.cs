using Imaging.Library.Entities;
using Imaging.Library.Maths;
using System;

namespace Imaging.Library.Filters.BasicFilters
{
    public class BicubicFilter : FilterBase
    {
        public BicubicFilter(double ratio)
        {
            this.Ratio = ratio;
        }

        public double Ratio { get; set; }
        private Size _outputSize { get; set; }

        public override void OnProcess()
        {
            _outputSize = new Size()
            {
                Width = (int)(Source.Width * Ratio),
                Height = (int)(Source.Height * Ratio),
            };

            // get source image size
            int width = Source.Width;
            int height = Source.Height;

            double xFactor = (double)width / _outputSize.Width;
            double yFactor = (double)height / _outputSize.Height;

            // coordinates of source points and cooefficiens
            double ox, oy, dx, dy, k1, k2;
            int ox1, oy1, ox2, oy2;
            // destination pixel values
            double r, g, b;
            // width and height decreased by 1
            int ymax = height - 1;
            int xmax = width - 1;

            for (int y = 0; y < _outputSize.Height; y++)
            {
                // Y coordinates
                oy = (double)y * yFactor - 0.5f;
                oy1 = (int)oy;
                dy = oy - (double)oy1;

                for (int x = 0; x < _outputSize.Width; x++)
                {
                    // X coordinates
                    ox = (double)x * xFactor - 0.5f;
                    ox1 = (int)ox;
                    dx = ox - (double)ox1;

                    // initial pixel value
                    r = g = b = 0;

                    for (int n = -1; n < 3; n++)
                    {
                        // get Y cooefficient
                        k1 = Interpolation.BiCubicKernel(dy - (double)n);

                        oy2 = oy1 + n;
                        if (oy2 < 0)
                            oy2 = 0;
                        if (oy2 > ymax)
                            oy2 = ymax;

                        for (int m = -1; m < 3; m++)
                        {
                            // get X cooefficient
                            k2 = k1 * Interpolation.BiCubicKernel((double)m - dx);

                            ox2 = ox1 + m;
                            if (ox2 < 0)
                                ox2 = 0;
                            if (ox2 > xmax)
                                ox2 = xmax;

                            r += k2 * Source.Map[oy2][ox2].R;
                            g += k2 * Source.Map[oy2][ox2].G;
                            b += k2 * Source.Map[oy2][ox2].B;
                        }
                    }

                    Source.Map[y][x] = new Pixel(255, (byte)Math.Max(0, Math.Min(255, r)), (byte)Math.Max(0, Math.Min(255, g)), (byte)Math.Max(0, Math.Min(255, b)));
                }
            }

            Source.Width = _outputSize.Width;
            Source.Height = _outputSize.Height;
        }
    }
}
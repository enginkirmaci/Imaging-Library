using Imaging.Library.Entities;
using System;

namespace Imaging.Library.Filters.ComplexFilters
{
    public class ConvolutionFilter : FilterBase
    {
        private readonly int _bias = 0;
        private readonly ConvolutionMatrix _matrix;

        public ConvolutionFilter(ConvolutionMatrix matrix)

        {
            _matrix = matrix;
        }

        public override void OnProcess()
        {
            var width = Source.Width;
            var height = Source.Height;

            if (_matrix.Factor == 0)
                return;

            int x, y, filterx, filtery;
            int s = _matrix.Size / 2;
            int r, g, b;

            for (y = s; y < height - s; y++)
            {
                for (x = s; x < width - s; x++)
                {
                    r = g = b = 0;

                    // Convolution
                    for (filtery = 0; filtery < _matrix.Size; filtery++)
                    {
                        for (filterx = 0; filterx < _matrix.Size; filterx++)
                        {
                            var tempPix = Source.Map[y + filtery - s][x + filterx - s];

                            r += _matrix.Matrix[filtery, filterx] * tempPix.R;
                            g += _matrix.Matrix[filtery, filterx] * tempPix.G;
                            b += _matrix.Matrix[filtery, filterx] * tempPix.B;
                        }
                    }

                    r = Math.Min(Math.Max((r / _matrix.Factor) + _matrix.Offset, 0), 255);
                    g = Math.Min(Math.Max((g / _matrix.Factor) + _matrix.Offset, 0), 255);
                    b = Math.Min(Math.Max((b / _matrix.Factor) + _matrix.Offset, 0), 255);

                    Source.Map[y][x] = new Pixel(255, (byte)r, (byte)g, (byte)b);
                }
            }
        }
    }
}
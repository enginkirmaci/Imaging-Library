using Imaging.Library.Entities;
using System;

namespace Imaging.Library.Filters.BasicFilters
{
    public class ColorPaletteFilter : FilterBase
    {
        private Pixel[] ColorList { get; }

        public ColorPaletteFilter(Pixel[] colorList)
        {
            ColorList = colorList;
        }

        public override void OnProcess()
        {
            for (var x = 0; x < Source.Width; x++)
            {
                for (var y = 0; y < Source.Height; y++)
                {
                    var c = Source.Map[y][x];

                    double dblInputRed = c.R;
                    double dblInputGreen = c.G;
                    double dblInputBlue = c.B;
                    // the Euclidean distance to be computed
                    // set this to an arbitrary number
                    // must be greater than the largest possible distance (appr. 441.7)
                    double distance = 500.0;
                    // store the interim result
                    double temp;
                    // RGB-Values of test colors
                    double dblTestRed;
                    double dblTestGreen;
                    double dblTestBlue;

                    foreach (var o in ColorList)
                    {
                        // compute the Euclidean distance between the two colors
                        // note, that the alpha-component is not used in this example
                        dblTestRed = Math.Pow(Convert.ToDouble(o.R) - dblInputRed, 2.0);
                        dblTestGreen = Math.Pow(Convert.ToDouble(o.G) - dblInputGreen, 2.0);
                        dblTestBlue = Math.Pow(Convert.ToDouble(o.B) - dblInputBlue, 2.0);
                        temp = Math.Sqrt(dblTestBlue + dblTestGreen + dblTestRed);
                        // explore the result and store the nearest color
                        if (temp < distance)
                        {
                            distance = temp;
                            c = o;
                        }
                    }

                    Source.Map[y][x] = c;
                }
            }
        }
    }
}
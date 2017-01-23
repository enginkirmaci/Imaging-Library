using Imaging.Library.Entities;
using System;

namespace Imaging.Library.Filters.BasicFilters
{
    public class LineDrawFilter : FilterBase
    {
        public Pixel Pixel { get; set; }
        public Line Line { get; set; }

        public LineDrawFilter(Line line, Pixel pixel)
        {
            Line = line;
            Pixel = pixel;
        }

        public override void OnProcess()
        {
            int xInitial = (int)Line.X1, yInitial = (int)Line.Y1, xFinal = (int)Line.X2, yFinal = (int)Line.Y2;

            int dx = xFinal - xInitial, dy = yFinal - yInitial, steps, k, xf, yf;

            float xIncrement, yIncrement, x = xInitial, y = yInitial;

            if (Math.Abs(dx) > Math.Abs(dy)) steps = Math.Abs(dx);
            else steps = Math.Abs(dy);
            xIncrement = dx / (float)steps;
            yIncrement = dy / (float)steps;

            for (k = 0; k < steps; k++)
            {
                x += xIncrement;
                xf = (int)x;
                y += yIncrement;
                yf = (int)y;
                try
                {
                    Source.Map[xf, yf] = Pixel;
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }
    }
}
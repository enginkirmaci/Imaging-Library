namespace Imaging.Library.Filters.BasicFilters
{
    public class HistogramFilter : FilterBase
    {
        public int[] Red { get; set; }

        public int[] Green { get; set; }

        public int[] Blue { get; set; }

        public int[] Luminance { get; set; }

        public HistogramFilter(out int[] red, out int[] green, out int[] blue, out int[] luminance)
        {
            Red = new int[byte.MaxValue + 1];
            Green = new int[byte.MaxValue + 1];
            Blue = new int[byte.MaxValue + 1];
            Luminance = new int[byte.MaxValue + 1];

            red = Red;
            green = Green;
            blue = Blue;
            luminance = Luminance;
        }

        public override void OnProcess()
        {
            for (var y = 0; y < Source.Height; y++)
            {
                for (var x = 0; x < Source.Width; x++)
                {
                    var c = Source.Map[y][x];

                    Red[c.R]++;
                    Green[c.G]++;
                    Blue[c.B]++;
                    Luminance[c.Luminance]++;
                }
            }
        }
    }
}
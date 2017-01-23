namespace Imaging.Library.Maths
{
    public class DimensionConverter
    {
        public static int To1D(int x, int y, int width)
        {
            return y * width + x;
        }

        public static int X(int pos, int width)
        {
            return pos - ((int)(pos / width) * width);
        }

        public static int Y(int pos, int width)
        {
            return pos / width;
        }
    }
}
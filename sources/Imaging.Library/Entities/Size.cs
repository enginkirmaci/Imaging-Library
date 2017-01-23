namespace Imaging.Library.Entities
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public static Size Empty { get; internal set; }

        public static bool operator ==(Size c1, Size c2)
        {
            return c1.Width.Equals(c2.Width) && c1.Height.Equals(c2.Height);
        }

        public static bool operator !=(Size c1, Size c2)
        {
            return !(c1.Width.Equals(c2.Width) && c1.Height.Equals(c2.Height));
        }

        public override string ToString()
        {
            return $"{Width}, {Height}";
        }
    }
}
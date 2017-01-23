namespace Imaging.Library.Entities
{
    public struct Rectangle
    {
        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public static Rectangle Empty { get; internal set; }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Width;
    }
}
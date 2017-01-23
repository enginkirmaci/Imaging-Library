namespace Imaging.Library.Entities
{
    public struct Blob
    {
        public Rectangle Rectangle { get; }

        public int ID { get; set; }

        public int Area { get; internal set; }

        public double Fullness { get; internal set; }

        public Point CenterOfGravity { get; internal set; }

        public Pixel ColorMean { get; internal set; }

        public Pixel ColorStdDev { get; internal set; }

        public Blob(int id, Rectangle rect)
        {
            ID = id;
            Rectangle = rect;
            CenterOfGravity = new Point(0, 0);
            Area = 0;
            Fullness = 0;

            ColorMean = new Pixel
            {
                A = 255,
                B = 255,
                G = 255,
                R = 255
            };

            ColorStdDev = new Pixel
            {
                A = 255,
                B = 255,
                G = 255,
                R = 255
            };
        }

        public Blob(Blob source)
        {
            ID = source.ID;
            Rectangle = source.Rectangle;
            CenterOfGravity = source.CenterOfGravity;
            Area = source.Area;
            Fullness = source.Fullness;
            ColorMean = source.ColorMean;
            ColorStdDev = source.ColorStdDev;
        }

        public Blob(int id, Blob source)
        {
            ID = id;
            Rectangle = source.Rectangle;
            CenterOfGravity = source.CenterOfGravity;
            Area = source.Area;
            Fullness = source.Fullness;
            ColorMean = source.ColorMean;
            ColorStdDev = source.ColorStdDev;
        }
    }
}
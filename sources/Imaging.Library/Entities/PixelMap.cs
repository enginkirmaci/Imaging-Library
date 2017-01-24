using System;

namespace Imaging.Library.Entities
{
    public class PixelMap
    {
        public readonly int Bpp;

        public readonly double DpiX;
        public readonly double DpiY;

        /// <summary>
        ///     The height of the PixelMap in pixels.
        /// </summary>
        public int Height;

        public Pixel[][] Map;

        /// <summary>
        ///     The width of the PixelMap in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        ///     Creates a blank PixelMap of desired width and height.
        /// </summary>
        public PixelMap(int width, int height, double dpiX, double dpiY, int bpp)
        {
            Width = width;
            Height = height;
            DpiX = dpiX;
            DpiY = dpiY;
            Map = new Pixel[Height][];

            for (int i = 0; i < Height; i++)
                Map[i] = new Pixel[Width];

            Bpp = bpp;
        }

        /// <summary>
        ///     Clones a PixelMap.
        /// </summary>
        public PixelMap(PixelMap original)
        {
            Width = original.Width;
            Height = original.Height;
            DpiX = original.DpiX;
            DpiY = original.DpiY;
            Map = new Pixel[Height][];

            for (int i = 0; i < Height; i++)
                Map[i] = new Pixel[Width];

            Bpp = original.Bpp;

            //Array.Copy(original.Map, Map, original.Map.Length);

            Map = CopyArrayBuiltIn(original.Map);

            //for (var x = 0; x < Width; x++)
            //    for (var y = 0; y < Height; y++)
            //        this[x, y] = original[x, y];
        }

        private static Pixel[][] CopyArrayBuiltIn(Pixel[][] source)
        {
            var len = source.Length;
            var dest = new Pixel[len][];

            for (var x = 0; x < len; x++)
            {
                var inner = source[x];
                var ilen = inner.Length;
                var newer = new Pixel[ilen];
                Array.Copy(inner, newer, ilen);
                dest[x] = newer;
            }

            return dest;
        }

        /// <summary>
        ///     Access a Pixel of the PixelMap from its X and Y coordinates.
        /// </summary>
        public Pixel this[int x, int y]
        {
            get
            {
                return Map[y][x];
            }
            set
            {
                Map[y][x] = value;
            }
        }

        /// <summary>
        ///     Access a Pixel of the PixelMap from its X and Y coordinates contained within a Point.
        /// </summary>
        public Pixel this[Point p]
        {
            get { return this[(int)p.X, (int)p.Y]; }
            set { this[(int)p.X, (int)p.Y] = value; }
        }

        /// <summary>
        ///     Access a Pixel of the PixelMap from its flattened index.
        /// </summary>
        public Pixel this[int i]
        {
            get { return this[i / Height, i % Height]; }
            set { this[i / Height, i % Height] = value; }
        }

        /// <summary>
        ///     Determine if a point is within this PixelMap.
        /// </summary>
        public bool Inside(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
        }
    }
}
using System.IO;
using Android.Graphics;
using Imaging.Library.Entities;
using Java.Nio;
using Xamarin.Forms;

namespace EdgeDetection.Forms.Helpers
{
    public class AndroidHelper
    {
        public static PixelMap GetPixelMap(Stream stream)
        {
            var decoder = BitmapRegionDecoder.NewInstance(stream, false);
            var bitmap = decoder.DecodeRegion(new Rect(0, 0, decoder.Width, decoder.Height), null);

            var width = bitmap.Width;
            var height = bitmap.Height;

            var source = new PixelMap(width, height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    source[x, y] = new Pixel
                    {
                        B = (byte)(pixel & 0x000000FF),
                        G = (byte)((pixel & 0x0000FF00) >> 8),
                        R = (byte)((pixel & 0x00FF0000) >> 16),
                        A = (byte)((pixel & 0xFF000000) >> 24)
                    };
                }
            }

            decoder.Recycle();

            return source;
        }

        public static ImageSource LoadImageFromPixelMap(PixelMap pixelMap)
        {
            var buffer = ByteBuffer.Wrap(pixelMap.ToByteArray());
            buffer.Rewind();

            var bitmap = Bitmap.CreateBitmap(pixelMap.Width, pixelMap.Height, Bitmap.Config.Argb8888);
            bitmap.CopyPixelsFromBuffer(buffer);

            var ms = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ImageSource.FromStream(() => ms);
        }
    }
}
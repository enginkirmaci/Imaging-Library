using CoreGraphics;
using Imaging.Library.Entities;
using UIKit;
using Xamarin.Forms;

namespace EdgeDetection.Forms.Helpers
{
    public class iOSHelper
    {
        public const int ByteToBit = 8;

        public static PixelMap GetPixelMap(string filePath)
        {
            var image = UIImage.FromFile(filePath);
            var cgImage = image.CGImage;

            var width = (int)cgImage.Width;
            var height = (int)cgImage.Height;
            var bytesPerPixel = (int)cgImage.BitsPerPixel / ByteToBit;
            var bytesPerRow = (int)cgImage.BytesPerRow;
            var bitsPerComponent = (int)cgImage.BitsPerComponent;

            var rawData = new byte[bytesPerRow * height];

            using (var colorSpace = CGColorSpace.CreateDeviceRGB())
            {
                //Crashes on the next line with an invalid handle exception
                using (var context = new CGBitmapContext(
                    rawData,
                    width,
                    height,
                    bitsPerComponent,
                    bytesPerRow,
                    colorSpace,
                    CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
                {
                    context.DrawImage(new CGRect(0, 0, width, height), cgImage);
                }
            }

            var source = new PixelMap(width, height, bytesPerPixel: bytesPerPixel, bitsPerComponent: bitsPerComponent);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var index = bytesPerRow * y + bytesPerPixel * x;

                    source[x, y] = new Pixel
                    {
                        R = rawData[index],
                        G = rawData[index + 1],
                        B = rawData[index + 2],
                        A = rawData[index + 3]
                    };
                }
            }

            return source;
        }

        public static ImageSource LoadImageFromPixelMap(PixelMap pixelMap)
        {
            var rawData = pixelMap.ToByteArray();

            using (var provider = new CGDataProvider(rawData, 0, rawData.Length))
            {
                using (var colorSpace = CGColorSpace.CreateDeviceRGB())
                {
                    var cgImage = new CGImage(
                        pixelMap.Width,
                        pixelMap.Height,
                        pixelMap.BitsPerComponent,
                        pixelMap.BytesPerPixel * ByteToBit,
                        pixelMap.BytesPerPixel * pixelMap.Width,
                        colorSpace,
                        CGBitmapFlags.ByteOrderDefault,
                        provider,
                        null,
                        true,
                        CGColorRenderingIntent.Default
                    );

                    return ImageSource.FromStream(() => UIImage.FromImage(cgImage).AsPNG().AsStream());
                }
            }
        }
    }
}
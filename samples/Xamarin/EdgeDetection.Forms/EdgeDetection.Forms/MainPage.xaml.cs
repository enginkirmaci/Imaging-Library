using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics;
using CoreGraphics;
using Imaging.Library;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Filters.BasicFilters;
using Imaging.Library.Filters.ComplexFilters;
using Imaging.Library.Maths;
using Java.Nio;
using Plugin.Media;
using Plugin.Media.Abstractions;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Point = Imaging.Library.Entities.Point;

namespace EdgeDetection.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public PixelMap Source { get; set; }

        private void LoadImage_Clicked(object sender, EventArgs e)
        {
            GetPixelMapFromGallery();
        }

        private void ScanDocument_Clicked(object sender, EventArgs e)
        {
            var imaging = new ImagingManager(Source);

            var scale = 0.4;

            imaging.AddFilter(new BicubicFilter(scale)); //Downscaling
            imaging.Render();

            imaging.AddFilter(new CannyEdgeDetector());
            imaging.Render();

            var blobCounter = new BlobCounter
            {
                ObjectsOrder = ObjectsOrder.Size
            };
            imaging.AddFilter(blobCounter);

            imaging.Render();

            List<Point> corners = null;
            var blobs = blobCounter.GetObjectsInformation();
            foreach (var blob in blobs)
            {
                var points = blobCounter.GetBlobsEdgePoints(blob);

                var shapeChecker = new SimpleShapeChecker();

                if (shapeChecker.IsQuadrilateral(points, out corners))
                    break;
            }

            var edgePoints = new EdgePoints();
            edgePoints.SetPoints(corners.ToArray());

            imaging.Render();
            imaging.UndoAll();

            edgePoints = edgePoints.ZoomIn(scale);
            imaging.AddFilter(new QuadrilateralTransformation(edgePoints, true));

            imaging.Render();

            MyImage.Source = LoadFromPixel(imaging.Output);
        }

        private void GetPixelMapAndroid(MediaFile file)
        {
            var decoder = BitmapRegionDecoder.NewInstance(file.GetStream(), false);
            var bitmap = decoder.DecodeRegion(new Rect(0, 0, decoder.Width, decoder.Height), null);

            var width = bitmap.Width;
            var height = bitmap.Height;
            var bpp = bitmap.RowBytes / bitmap.Width;
            var dpiX = 1;
            var dpiY = 1;

            Source = new PixelMap(width, height, dpiX, dpiY, bpp);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    Source[x, y] = new Pixel
                    {
                        R = (byte)(pixel & 0x000000FF),
                        G = (byte)((pixel & 0x0000FF00) >> 8),
                        B = (byte)((pixel & 0x00FF0000) >> 16),
                        A = (byte)((pixel & 0xFF000000) >> 24)
                    };
                }
            }

            decoder.Recycle();
        }

        private ImageSource LoadFromPixelMapAndroid(PixelMap pixelMap)
        {
            var width = pixelMap.Width;
            var height = pixelMap.Height;
            var stride = width * 4;

            var data = new byte[pixelMap.Height * stride];

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var pixel = pixelMap[j, i];

                    var idx = i * stride + j * 4;

                    data[idx] = pixel.B;
                    data[idx + 1] = pixel.G;
                    data[idx + 2] = pixel.R;
                    data[idx + 3] = pixel.A;
                }
            }
            var buffer = ByteBuffer.Wrap(data);
            buffer.Rewind();

            var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            bitmap.CopyPixelsFromBuffer(buffer);

            var ms = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ImageSource.FromStream(() => ms);
        }

        private void GetPixelMapiOS(MediaFile file)
        {
            var image = UIImage.FromFile(file.Path);

            var width = (int)image.Size.Width;
            var height = (int)image.Size.Height;
            var bytesPerPixel = 4;
            var bytesPerRow = bytesPerPixel * width;
            var bitsPerComponent = 8;
            var dpiX = 1;
            var dpiY = 1;

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
                    context.DrawImage(new CGRect(0, 0, width, height), image.CGImage);
                }
            }

            Source = new PixelMap(width, height, dpiX, dpiY, bytesPerPixel);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var index = bytesPerRow * y + bytesPerPixel * x;

                    Source[x, y] = new Pixel
                    {
                        B = rawData[index],
                        G = rawData[index + 1],
                        R = rawData[index + 2],
                        A = rawData[index + 3]
                    };
                }
            }
        }

        private ImageSource LoadFromPixelMapiOS(PixelMap pixelMap)
        {
            var width = pixelMap.Width;
            var height = pixelMap.Height;
            var stride = width * 4;

            var rawData = new byte[pixelMap.Height * stride];

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var pixel = pixelMap[j, i];

                    var idx = i * stride + j * 4;

                    rawData[idx] = pixel.B;
                    rawData[idx + 1] = pixel.G;
                    rawData[idx + 2] = pixel.R;
                    rawData[idx + 3] = pixel.A;
                }
            }

            var bitsPerComponant = 8;
            var bitsPerPixel = 32;
            var bytesPerRow = 4 * pixelMap.Width;

            using (var _provider = new CGDataProvider(rawData, 0, rawData.Length))
            {
                using (var colorSpace = CGColorSpace.CreateDeviceRGB())
                {
                    var cg = new CGImage(
                        pixelMap.Width,
                        pixelMap.Height,
                        bitsPerComponant,
                        bitsPerPixel,
                        bytesPerRow,
                        colorSpace,
                        CGBitmapFlags.ByteOrderDefault,
                        _provider,
                        null,
                        true,
                        CGColorRenderingIntent.Default
                    );

                    return ImageSource.FromStream(() => UIImage.FromImage(cg).AsPNG().AsStream());
                }
            }
        }

        private async void GetPixelMapFromGallery()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    GetPixelMapAndroid(file);
                    break;

                case Device.iOS:
                    GetPixelMapiOS(file);
                    break;
            }

            MyImage.Source = LoadFromPixel(Source);
        }

        private ImageSource LoadFromPixel(PixelMap pixelMap)
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    return LoadFromPixelMapAndroid(Source);

                case Device.iOS:
                    return LoadFromPixelMapiOS(Source);

                default:
                    return null;
            }
        }
    }
}
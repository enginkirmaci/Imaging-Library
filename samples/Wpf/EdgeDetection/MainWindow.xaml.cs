using Imaging.Library;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Filters.BasicFilters;
using Imaging.Library.Filters.ComplexFilters;
using Imaging.Library.Maths;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EdgeDetection
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = @"D:\OneDrive\Pictures\Misc",
                Filter = "Bitmap|*.bmp;*.jpg;*.jpeg;*.png;"
            };

            EdgeDetection();
            return;

            //if (dialog.ShowDialog() == true)
            {
                var path = @"D:\OneDrive\Pictures\Misc\WP_20150807_08_45_41_Pro.jpg";
                var savepath = @"D:\Desktop\test.jpg";

                var sources = new PixelMap[2];

                GetPixelMap(path, sources);
                var source = sources[0];

                image.Source = LoadFromPixelMap(source);

                var imaging = new ImagingManager(source);

                //imaging.AddFilter(new PixelInterpolation(50));

                //imaging.AddFilter(new ThresholdFilter(150, 150, 150));

                //imaging.AddFilter(new OtsuThresholdFilter());

                imaging.AddFilter(new BicubicFilter(0.5));

                imaging.Render();

                var bitmapImage = LoadFromPixelMap(imaging.Output);
                image2.Source = bitmapImage;

                SaveImageToFile(bitmapImage, savepath);
            }
        }

        private void EdgeDetection()
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = @"D:\OneDrive",
                Filter = "Bitmap|*.bmp;*.jpg;*.jpeg;*.png;"
            };

            if (dialog.ShowDialog() == true)
            {
                var scale = 0.4;
                var path = dialog.FileName;

                var sources = new PixelMap[2];

                GetPixelMap(path, sources);

                var source = sources[0];

                var imaging = new ImagingManager(source);
                imaging.AddFilter(new BicubicFilter(scale)); //Downscaling
                imaging.Render();

                imaging.AddFilter(new CannyEdgeDetector());
                imaging.Render();

                var blobCounter = new BlobCounter()
                {
                    ObjectsOrder = ObjectsOrder.Size
                };
                imaging.AddFilter(blobCounter);

                imaging.Render();

                List<Imaging.Library.Entities.Point> corners = null;
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

                var line1 = new Line
                {
                    X1 = edgePoints.TopLeft.X,
                    Y1 = edgePoints.TopLeft.Y,
                    X2 = edgePoints.BottomLeft.X,
                    Y2 = edgePoints.BottomLeft.Y
                };
                var line2 = new Line
                {
                    X1 = edgePoints.BottomLeft.X,
                    Y1 = edgePoints.BottomLeft.Y,
                    X2 = edgePoints.BottomRight.X,
                    Y2 = edgePoints.BottomRight.Y
                };
                var line3 = new Line
                {
                    X1 = edgePoints.BottomRight.X,
                    Y1 = edgePoints.BottomRight.Y,
                    X2 = edgePoints.TopRight.X,
                    Y2 = edgePoints.TopRight.Y
                };
                var line4 = new Line
                {
                    X1 = edgePoints.TopRight.X,
                    Y1 = edgePoints.TopRight.Y,
                    X2 = edgePoints.TopLeft.X,
                    Y2 = edgePoints.TopLeft.Y
                };
                var pixel = new Pixel(255, 255, 0, 0);
                imaging.AddFilter(new LineDrawFilter(line1, pixel));
                imaging.AddFilter(new LineDrawFilter(line2, pixel));
                imaging.AddFilter(new LineDrawFilter(line3, pixel));
                imaging.AddFilter(new LineDrawFilter(line4, pixel));

                imaging.Render();
                image.Source = LoadFromPixelMap(imaging.Output);

                imaging.UndoAll();

                edgePoints = edgePoints.ZoomIn(scale);
                imaging.AddFilter(new QuadrilateralTransformation(edgePoints, true));

                imaging.Render();
                image2.Source = LoadFromPixelMap(imaging.Output);

                SaveImageToFile(LoadFromPixelMap(imaging.Output), @"D:\Desktop\2.jpg");
            }
        }

        private BitmapSource LoadFromPixelMap(PixelMap pixelMap)
        {
            var width = pixelMap.Width;
            var height = pixelMap.Height;
            var stride = width * 4;

            var data = new byte[pixelMap.Height * stride];

            for (var i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                {
                    var pixel = pixelMap[j, i];

                    var idx = i * stride + j * 4;

                    data[idx] = pixel.B;
                    data[idx + 1] = pixel.G;
                    data[idx + 2] = pixel.R;
                    data[idx + 3] = pixel.A;
                }

            var bitmap = BitmapSource.Create(width, height, pixelMap.DpiX, pixelMap.DpiY, PixelFormats.Bgra32, null,
                data, stride);

            return bitmap;
        }

        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        private void GetPixelMap(string path, PixelMap[] sources)
        {
            var bitmap = new BitmapImage(new Uri(path, UriKind.Relative));
            var wb = new WriteableBitmap(bitmap);

            var width = wb.PixelWidth;
            var height = wb.PixelHeight;
            var bpp = wb.Format.BitsPerPixel;
            var dpiX = wb.DpiX;
            var dpiY = wb.DpiY;

            for (var i = 0; i < sources.Length; i++)
                sources[i] = new PixelMap(width, height, dpiX, dpiY, bpp);

            var stride = wb.PixelWidth * ((bpp + 7) / 8);

            var data = new byte[width * height * 4];
            wb.CopyPixels(data, stride, 0);

            var y0 = 0 / width;
            var x0 = 0 - width * y0;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    foreach (var source in sources)
                        source[x + x0, y + y0] = new Pixel
                        {
                            B = data[(y * width + x) * 4 + 0],
                            G = data[(y * width + x) * 4 + 1],
                            R = data[(y * width + x) * 4 + 2],
                            A = data[(y * width + x) * 4 + 3]
                        };
        }
    }
}
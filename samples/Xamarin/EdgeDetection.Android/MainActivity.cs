using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Imaging.Library;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Filters.BasicFilters;
using Imaging.Library.Filters.ComplexFilters;
using Imaging.Library.Maths;
using Java.Nio;
using Point = Imaging.Library.Entities.Point;
using Uri = Android.Net.Uri;

namespace EdgeDetection.Android
{
    [Activity(Label = "EdgeDetection", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public PixelMap Source { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var loadImageBtn = FindViewById<Button>(Resource.Id.loadImage);

            loadImageBtn.Click += delegate
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };

            var scanDocumentBtn = FindViewById<Button>(Resource.Id.scanDocument);

            scanDocumentBtn.Click += ScanDocumentBtn_Click;
        }

        private void ScanDocumentBtn_Click(object sender, EventArgs e)
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

            var image = LoadFromPixelMap(imaging.Output);

            var imageView = FindViewById<ImageView>(Resource.Id.myImageView);
            imageView.SetImageBitmap(image);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var imageView = FindViewById<ImageView>(Resource.Id.myImageView);
                imageView.SetImageURI(data.Data);

                GetPixelMap(data.Data);
            }
        }

        private Bitmap LoadFromPixelMap(PixelMap pixelMap)
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

            var buffer = ByteBuffer.Wrap(data);
            buffer.Rewind();

            var image = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            image.CopyPixelsFromBuffer(buffer);

            return image;
        }

        private void GetPixelMap(Uri imageUri)
        {
            var bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, imageUri);

            var width = bitmap.Width;
            var height = bitmap.Height;
            var bpp = bitmap.RowBytes / bitmap.Width;
            var dpiX = 1;
            var dpiY = 1;

            Source = new PixelMap(width, height, dpiX, dpiY, bpp);

            var y0 = 0 / width;
            var x0 = 0 - width * y0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var values = BitConverter.GetBytes(pixel);

                    Source[x + x0, y + y0] = new Pixel
                    {
                        B = values[0],
                        G = values[1],
                        R = values[2],
                        A = values[3]
                    };
                }
            }
        }
    }
}
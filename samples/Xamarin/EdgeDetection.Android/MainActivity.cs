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
            var buffer = ByteBuffer.Wrap(pixelMap.ToByteArray());
            buffer.Rewind();

            var bitmap = Bitmap.CreateBitmap(pixelMap.Width, pixelMap.Height, Bitmap.Config.Argb8888);
            bitmap.CopyPixelsFromBuffer(buffer);

            return bitmap;
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

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    Source[x, y] = new Pixel
                    {
                        B = (byte)(pixel & 0x000000FF),
                        G = (byte)((pixel & 0x0000FF00) >> 8),
                        R = (byte)((pixel & 0x00FF0000) >> 16),
                        A = (byte)((pixel & 0xFF000000) >> 24)
                    };
                }
            }
        }
    }
}
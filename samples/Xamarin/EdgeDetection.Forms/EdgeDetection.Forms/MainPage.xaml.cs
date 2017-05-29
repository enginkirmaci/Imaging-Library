using System;
using System.Collections.Generic;
using EdgeDetection.Forms.Helpers;
using Imaging.Library;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Filters.BasicFilters;
using Imaging.Library.Filters.ComplexFilters;
using Imaging.Library.Maths;
using Plugin.Media;
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
                    Source = AndroidHelper.GetPixelMap(file.GetStream());
                    break;

                case Device.iOS:
                    Source = iOSHelper.GetPixelMap(file.Path);
                    break;
            }

            MyImage.Source = LoadFromPixel(Source);
        }

        private ImageSource LoadFromPixel(PixelMap pixelMap)
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    return AndroidHelper.LoadImageFromPixelMap(pixelMap);

                case Device.iOS:
                    return iOSHelper.LoadImageFromPixelMap(pixelMap);

                default:
                    return null;
            }
        }
    }
}
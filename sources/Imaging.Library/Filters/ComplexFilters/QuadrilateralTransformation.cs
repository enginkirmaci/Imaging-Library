using Imaging.Library.Entities;

namespace Imaging.Library.Filters.ComplexFilters
{
    public class QuadrilateralTransformation : FilterBase
    {
        private Size _estimatedSize;

        public QuadrilateralTransformation(EdgePoints edgePoints, bool useInterpolation)
        {
            EdgePoints = edgePoints;
            _estimatedSize = edgePoints.EstimatedRectangleSize();
            UseInterpolation = useInterpolation;
        }

        public bool UseInterpolation { get; set; }

        public EdgePoints EdgePoints { get; set; }

        public override void OnProcess()
        {
            var srcW = Source.Width;
            var srcH = Source.Height;

            // get source and destination images size
            int srcWidth = Source.Width;
            int srcHeight = Source.Height;
            int dstWidth = _estimatedSize.Width;
            int dstHeight = _estimatedSize.Height;

            //int pixelSize = Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
            //int srcStride = sourceData.Stride;
            //int dstStride = destinationData.Stride;

            // find equations of four quadrilateral's edges ( f(x) = k*x + b )
            double kTop, bTop;
            double kBottom, bBottom;
            double kLeft, bLeft;
            double kRight, bRight;

            // top edge
            if (EdgePoints.TopRight.X == EdgePoints.TopLeft.X)
            {
                kTop = 0;
                bTop = EdgePoints.TopRight.X;
            }
            else
            {
                kTop = (double)(EdgePoints.TopRight.Y - EdgePoints.TopLeft.Y) /
                                (EdgePoints.TopRight.X - EdgePoints.TopLeft.X);
                bTop = (double)EdgePoints.TopLeft.Y - kTop * EdgePoints.TopLeft.X;
            }

            // bottom edge
            if (EdgePoints.BottomRight.X == EdgePoints.BottomLeft.X)
            {
                kBottom = 0;
                bBottom = EdgePoints.BottomRight.X;
            }
            else
            {
                kBottom = (double)(EdgePoints.BottomRight.Y - EdgePoints.BottomLeft.Y) /
                                   (EdgePoints.BottomRight.X - EdgePoints.BottomLeft.X);
                bBottom = (double)EdgePoints.BottomLeft.Y - kBottom * EdgePoints.BottomLeft.X;
            }

            // left edge
            if (EdgePoints.BottomLeft.X == EdgePoints.TopLeft.X)
            {
                kLeft = 0;
                bLeft = EdgePoints.BottomLeft.X;
            }
            else
            {
                kLeft = (double)(EdgePoints.BottomLeft.Y - EdgePoints.TopLeft.Y) /
                                 (EdgePoints.BottomLeft.X - EdgePoints.TopLeft.X);
                bLeft = (double)EdgePoints.TopLeft.Y - kLeft * EdgePoints.TopLeft.X;
            }

            // right edge
            if (EdgePoints.BottomRight.X == EdgePoints.TopRight.X)
            {
                kRight = 0;
                bRight = EdgePoints.BottomRight.X;
            }
            else
            {
                kRight = (double)(EdgePoints.BottomRight.Y - EdgePoints.TopRight.Y) /
                                  (EdgePoints.BottomRight.X - EdgePoints.TopRight.X);
                bRight = (double)EdgePoints.TopRight.Y - kRight * EdgePoints.TopRight.X;
            }

            // some precalculated values
            double leftFactor = (double)(EdgePoints.BottomLeft.Y - EdgePoints.TopLeft.Y) / dstHeight;
            double rightFactor = (double)(EdgePoints.BottomRight.Y - EdgePoints.TopRight.Y) / dstHeight;

            int srcY0 = (int)EdgePoints.TopLeft.Y;
            int srcY1 = (int)EdgePoints.TopRight.Y;

            // do the job
            //byte* baseSrc = (byte*)sourceData.ImageData.ToPointer();
            //byte* baseDst = (byte*)destinationData.ImageData.ToPointer();

            // source width and height decreased by 1
            int ymax = srcHeight - 1;
            int xmax = srcWidth - 1;

            // coordinates of source points
            double dx1, dy1, dx2, dy2;
            int sx1, sy1, sx2, sy2;

            byte A, R, G, B;
            Pixel p1, p2, p3, p4;

            // for each line
            for (int y = 0; y < dstHeight; y++)
            {
                //byte* dst = baseDst + dstStride * y;

                // find corresponding Y on the left edge of the quadrilateral
                double yHorizLeft = leftFactor * y + srcY0;
                // find corresponding X on the left edge of the quadrilateral
                double xHorizLeft = (kLeft == 0) ? bLeft : (yHorizLeft - bLeft) / kLeft;

                // find corresponding Y on the right edge of the quadrilateral
                double yHorizRight = rightFactor * y + srcY1;
                // find corresponding X on the left edge of the quadrilateral
                double xHorizRight = (kRight == 0) ? bRight : (yHorizRight - bRight) / kRight;

                // find equation of the line joining points on the left and right edges
                double kHoriz, bHoriz;

                if (xHorizLeft == xHorizRight)
                {
                    kHoriz = 0;
                    bHoriz = xHorizRight;
                }
                else
                {
                    kHoriz = (yHorizRight - yHorizLeft) / (xHorizRight - xHorizLeft);
                    bHoriz = yHorizLeft - kHoriz * xHorizLeft;
                }

                double horizFactor = (xHorizRight - xHorizLeft) / dstWidth;

                if (!UseInterpolation)
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        double xs = horizFactor * x + xHorizLeft;
                        double ys = kHoriz * xs + bHoriz;

                        if ((xs >= 0) && (ys >= 0) && (xs < srcWidth) && (ys < srcHeight))
                        {
                            Source.Map[x, y] = Source.Map[(int)xs, (int)ys];
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        double xs = horizFactor * x + xHorizLeft;
                        double ys = kHoriz * xs + bHoriz;

                        if ((xs >= 0) && (ys >= 0) && (xs < srcWidth) && (ys < srcHeight))
                        {
                            sx1 = (int)xs;
                            sx2 = (sx1 == xmax) ? sx1 : sx1 + 1;
                            dx1 = xs - sx1;
                            dx2 = 1.0 - dx1;

                            sy1 = (int)ys;
                            sy2 = (sy1 == ymax) ? sy1 : sy1 + 1;
                            dy1 = ys - sy1;
                            dy2 = 1.0 - dy1;

                            p1 = Source.Map[(int)sx1, (int)sy1];
                            p2 = Source.Map[(int)sx2, (int)sy1];
                            p3 = Source.Map[(int)sx1, (int)sy2];
                            p4 = Source.Map[(int)sx2, (int)sy2];

                            A = (byte)(dy2 * (dx2 * (p1.A) + dx1 * (p2.A)) + (dy1 * (dx2 * (p3.A) + dx1 * (p4.A))));
                            R = (byte)(dy2 * (dx2 * (p1.R) + dx1 * (p2.R)) + (dy1 * (dx2 * (p3.R) + dx1 * (p4.R))));
                            G = (byte)(dy2 * (dx2 * (p1.G) + dx1 * (p2.G)) + (dy1 * (dx2 * (p3.G) + dx1 * (p4.G))));
                            B = (byte)(dy2 * (dx2 * (p1.B) + dx1 * (p2.B)) + (dy1 * (dx2 * (p3.B) + dx1 * (p4.B))));

                            Source.Map[x, y] = new Pixel(A, R, G, B);
                        }
                    }
                }
            }

            Source.Width = _estimatedSize.Width;
            Source.Height = _estimatedSize.Height;
        }
    }
}
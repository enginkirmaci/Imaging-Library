using Imaging.Library.Comparers;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Interfaces;
using Imaging.Library.Maths;
using System;
using System.Collections.Generic;

namespace Imaging.Library.Filters.ComplexFilters
{
    public class BlobCounter : FilterBase
    {
        // found blobs
        private List<Blob> blobs = new List<Blob>();

        // objects' sort order
        private ObjectsOrder objectsOrder = ObjectsOrder.None;

        // filtering by size is required or not
        private bool filterBlobs = false;

        private IBlobsFilter filter = null;

        // coupled size filtering or not
        private bool coupledSizeFiltering = false;

        // blobs' minimal and maximal size
        private int minWidth = 1;

        private int minHeight = 1;
        private int maxWidth = int.MaxValue;
        private int maxHeight = int.MaxValue;

        private const byte backgroundThresholdR = 0;
        private const byte backgroundThresholdG = 0;
        private const byte backgroundThresholdB = 0;

        /// <summary>
        /// Objects count.
        /// </summary>
        protected int objectsCount;

        /// <summary>
        /// Objects' labels.
        /// </summary>
        protected int[] objectLabels;

        /// <summary>
        /// Width of processed image.
        /// </summary>
        protected int imageWidth;

        /// <summary>
        /// Height of processed image.
        /// </summary>
        protected int imageHeight;

        /// <summary>
        /// Objects count.
        /// </summary>
        ///
        /// <remarks><para>Number of objects (blobs) found by <see cref="ProcessImage(Bitmap)"/> method.
        /// </para></remarks>
        ///
        public int ObjectsCount
        {
            get { return objectsCount; }
        }

        /// <summary>
        /// Objects' labels.
        /// </summary>
        ///
        /// <remarks>The array of <b>width</b> * <b>height</b> size, which holds
        /// labels for all objects. Background is represented with <b>0</b> value,
        /// but objects are represented with labels starting from <b>1</b>.</remarks>
        ///
        public int[] ObjectLabels
        {
            get { return objectLabels; }
        }

        /// <summary>
        /// Objects sort order.
        /// </summary>
        ///
        /// <remarks><para>The property specifies objects' sort order, which are provided
        /// by <see cref="GetObjectsRectangles"/>, <see cref="GetObjectsInformation"/>, etc.
        /// </para></remarks>
        ///
        public ObjectsOrder ObjectsOrder
        {
            get { return objectsOrder; }
            set { objectsOrder = value; }
        }

        /// <summary>
        /// Specifies if blobs should be filtered.
        /// </summary>
        ///
        /// <remarks><para>If the property is equal to <b>false</b>, then there is no any additional
        /// post processing after image was processed. If the property is set to <b>true</b>, then
        /// blobs filtering is done right after image processing routine. If <see cref="BlobsFilter"/>
        /// is set, then custom blobs' filtering is done, which is implemented by user. Otherwise
        /// blobs are filtered according to dimensions specified in <see cref="MinWidth"/>,
        /// <see cref="MinHeight"/>, <see cref="MaxWidth"/> and <see cref="MaxHeight"/> properties.</para>
        ///
        /// <para>Default value is set to <see langword="false"/>.</para></remarks>
        ///
        public bool FilterBlobs
        {
            get { return filterBlobs; }
            set { filterBlobs = value; }
        }

        /// <summary>
        /// Specifies if size filetering should be coupled or not.
        /// </summary>
        ///
        /// <remarks><para>In uncoupled filtering mode, objects are filtered out in the case if
        /// their width is smaller than <see cref="MinWidth"/> <b>or</b> height is smaller than
        /// <see cref="MinHeight"/>. But in coupled filtering mode, objects are filtered out in
        /// the case if their width is smaller than <see cref="MinWidth"/> <b>and</b> height is
        /// smaller than <see cref="MinHeight"/>. In both modes the idea with filtering by objects'
        /// maximum size is the same as filtering by objects' minimum size.</para>
        ///
        /// <para>Default value is set to <see langword="false"/>, what means uncoupled filtering by size.</para>
        /// </remarks>
        ///
        public bool CoupledSizeFiltering
        {
            get { return coupledSizeFiltering; }
            set { coupledSizeFiltering = value; }
        }

        /// <summary>
        /// Minimum allowed width of blob.
        /// </summary>
        ///
        /// <remarks><para>The property specifies minimum object's width acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        ///
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        ///
        public int MinWidth
        {
            get { return minWidth; }
            set { minWidth = value; }
        }

        /// <summary>
        /// Minimum allowed height of blob.
        /// </summary>
        ///
        /// <remarks><para>The property specifies minimum object's height acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        ///
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        ///
        public int MinHeight
        {
            get { return minHeight; }
            set { minHeight = value; }
        }

        /// <summary>
        /// Maximum allowed width of blob.
        /// </summary>
        ///
        /// <remarks><para>The property specifies maximum object's width acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        ///
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        ///
        public int MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }

        /// <summary>
        /// Maximum allowed height of blob.
        /// </summary>
        ///
        /// <remarks><para>The property specifies maximum object's height acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        ///
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        ///
        public int MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; }
        }

        /// <summary>
        /// Custom blobs' filter to use.
        /// </summary>
        ///
        /// <remarks><para>The property specifies custom blobs' filtering routine to use. It has
        /// effect only in the case if <see cref="FilterBlobs"/> property is set to <see langword="true"/>.</para>
        ///
        /// <para><note>When custom blobs' filtering routine is set, it has priority over default filtering done
        /// with <see cref="MinWidth"/>, <see cref="MinHeight"/>, <see cref="MaxWidth"/> and <see cref="MaxHeight"/>.</note></para>
        /// </remarks>
        ///
        public IBlobsFilter BlobsFilter
        {
            get { return filter; }
            set { filter = value; }
        }

        public override void OnProcess()
        {
            imageWidth = Source.Width;
            imageHeight = Source.Height;

            // do actual objects map building
            BuildObjectsMap(Source);

            // collect information about blobs
            CollectObjectsInfo(Source);

            // filter blobs by size if required
            if (filterBlobs)
            {
                // labels remapping array
                int[] labelsMap = new int[objectsCount + 1];
                for (int i = 1; i <= objectsCount; i++)
                {
                    labelsMap[i] = i;
                }

                // check dimension of all objects and filter them
                int objectsToRemove = 0;

                if (filter == null)
                {
                    for (int i = objectsCount - 1; i >= 0; i--)
                    {
                        int blobWidth = (int)blobs[i].Rectangle.Width;
                        int blobHeight = (int)blobs[i].Rectangle.Height;

                        if (coupledSizeFiltering == false)
                        {
                            // uncoupled filtering
                            if (
                                (blobWidth < minWidth) || (blobHeight < minHeight) ||
                                (blobWidth > maxWidth) || (blobHeight > maxHeight))
                            {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        }
                        else
                        {
                            // coupled filtering
                            if (
                                ((blobWidth < minWidth) && (blobHeight < minHeight)) ||
                                ((blobWidth > maxWidth) && (blobHeight > maxHeight)))
                            {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = objectsCount - 1; i >= 0; i--)
                    {
                        if (!filter.Check(blobs[i]))
                        {
                            labelsMap[i + 1] = 0;
                            objectsToRemove++;
                            blobs.RemoveAt(i);
                        }
                    }
                }

                // update labels remapping array
                int label = 0;
                for (int i = 1; i <= objectsCount; i++)
                {
                    if (labelsMap[i] != 0)
                    {
                        label++;
                        // update remapping array
                        labelsMap[i] = label;
                    }
                }

                // repair object labels
                for (int i = 0, n = objectLabels.Length; i < n; i++)
                {
                    objectLabels[i] = labelsMap[objectLabels[i]];
                }

                objectsCount -= objectsToRemove;

                // repair IDs
                for (int i = 0, n = blobs.Count; i < n; i++)
                {
                    blobs[i] = new Blob(i + 1, blobs[i]);
                }
            }

            // do we need to sort the list?
            if (objectsOrder != ObjectsOrder.None)
            {
                blobs.Sort(new BlobsSorter(objectsOrder));
            }
        }

        public Rectangle[] GetObjectsRectangles()
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new Exception("Image should be processed before to collect objects map.");

            Rectangle[] rects = new Rectangle[objectsCount];

            for (int i = 0; i < objectsCount; i++)
            {
                rects[i] = blobs[i].Rectangle;
            }

            return rects;
        }

        public Blob[] GetObjectsInformation()
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new Exception("Image should be processed before to collect objects map.");

            Blob[] blobsToReturn = new Blob[objectsCount];

            // create each blob
            for (int k = 0; k < objectsCount; k++)
            {
                blobsToReturn[k] = new Blob(blobs[k]);
            }

            return blobsToReturn;
        }

        public void GetBlobsLeftAndRightEdges(Blob blob, out List<Point> leftEdge, out List<Point> rightEdge)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new Exception("Image should be processed before to collect objects map.");

            leftEdge = new List<Point>();
            rightEdge = new List<Point>();

            int xmin = (int)blob.Rectangle.Left;
            int xmax = (int)(xmin + blob.Rectangle.Width - 1);
            int ymin = (int)blob.Rectangle.Top;
            int ymax = (int)(ymin + blob.Rectangle.Height - 1);

            int label = blob.ID;

            // for each line
            for (int y = ymin; y <= ymax; y++)
            {
                // scan from left to right
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++)
                {
                    if (objectLabels[p] == label)
                    {
                        leftEdge.Add(new Point(x, y));
                        break;
                    }
                }

                // scan from right to left
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--)
                {
                    if (objectLabels[p] == label)
                    {
                        rightEdge.Add(new Point(x, y));
                        break;
                    }
                }
            }
        }

        public void GetBlobsTopAndBottomEdges(Blob blob, out List<Point> topEdge, out List<Point> bottomEdge)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new Exception("Image should be processed before to collect objects map.");

            topEdge = new List<Point>();
            bottomEdge = new List<Point>();

            int xmin = (int)blob.Rectangle.Left;
            int xmax = (int)(xmin + blob.Rectangle.Width - 1);
            int ymin = (int)blob.Rectangle.Top;
            int ymax = (int)(ymin + blob.Rectangle.Height - 1);

            int label = blob.ID;

            // for each column
            for (int x = xmin; x <= xmax; x++)
            {
                // scan from top to bottom
                int p = ymin * imageWidth + x;
                for (int y = ymin; y <= ymax; y++, p += imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        topEdge.Add(new Point(x, y));
                        break;
                    }
                }

                // scan from bottom to top
                p = ymax * imageWidth + x;
                for (int y = ymax; y >= ymin; y--, p -= imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        bottomEdge.Add(new Point(x, y));
                        break;
                    }
                }
            }
        }

        public List<Point> GetBlobsEdgePoints(Blob blob)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new Exception("Image should be processed before to collect objects map.");

            List<Point> edgePoints = new List<Point>();

            int xmin = (int)blob.Rectangle.Left;
            int xmax = (int)(xmin + blob.Rectangle.Width - 1);
            int ymin = (int)blob.Rectangle.Top;
            int ymax = (int)(ymin + blob.Rectangle.Height - 1);

            int label = blob.ID;

            // array of already processed points on left/right edges
            // (index in these arrays represent Y coordinate, but value - X coordinate)
            int[] leftProcessedPoints = new int[(int)blob.Rectangle.Height];
            int[] rightProcessedPoints = new int[(int)blob.Rectangle.Height];

            // for each line
            for (int y = ymin; y <= ymax; y++)
            {
                // scan from left to right
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++)
                {
                    if (objectLabels[p] == label)
                    {
                        edgePoints.Add(new Point(x, y));
                        leftProcessedPoints[y - ymin] = x;
                        break;
                    }
                }

                // scan from right to left
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if (leftProcessedPoints[y - ymin] != x)
                        {
                            edgePoints.Add(new Point(x, y));
                        }
                        rightProcessedPoints[y - ymin] = x;
                        break;
                    }
                }
            }

            // for each column
            for (int x = xmin; x <= xmax; x++)
            {
                // scan from top to bottom
                int p = ymin * imageWidth + x;
                for (int y = ymin, y0 = 0; y <= ymax; y++, y0++, p += imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if ((leftProcessedPoints[y0] != x) &&
                             (rightProcessedPoints[y0] != x))
                        {
                            edgePoints.Add(new Point(x, y));
                        }
                        break;
                    }
                }

                // scan from bottom to top
                p = ymax * imageWidth + x;
                for (int y = ymax, y0 = ymax - ymin; y >= ymin; y--, y0--, p -= imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if ((leftProcessedPoints[y0] != x) &&
                             (rightProcessedPoints[y0] != x))
                        {
                            edgePoints.Add(new Point(x, y));
                        }
                        break;
                    }
                }
            }

            return edgePoints;
        }

        private void BuildObjectsMap(PixelMap sourcePixelRegion)
        {
            // we don't want one pixel width images
            if (imageWidth == 1)
            {
                throw new Exception("BlobCounter cannot process images that are one pixel wide. Rotate the image or use RecursiveBlobCounter.");
            }

            int imageWidthM1 = imageWidth - 1;

            // allocate labels array
            objectLabels = new int[imageWidth * imageHeight];
            // initial labels count
            int labelsCount = 0;

            // create map
            int maxObjects = ((imageWidth / 2) + 1) * ((imageHeight / 2) + 1) + 1;
            int[] map = new int[maxObjects];

            // initially map all labels to themself
            for (int i = 0; i < maxObjects; i++)
            {
                map[i] = i;
            }

            // do the job
            int pos = 0;
            int p = 0;

            // color images
            int pixelSize = 1;
            int offset = imageWidth * pixelSize;

            int stride = imageWidth;
            int strideM1 = stride - pixelSize;
            int strideP1 = stride + pixelSize;

            // 1 - for pixels of the first row
            //Color firstColor = ToColor(sourcePixelRegion.ImagePixels[pos]);

            var firstPixel = Source.Map[DimensionConverter.Y(pos, imageWidth)][DimensionConverter.X(pos, imageWidth)];

            if ((firstPixel.R | firstPixel.G | firstPixel.B) != 0)
            {
                objectLabels[p] = ++labelsCount;
            }
            pos += pixelSize;
            ++p;

            for (int x = 1; x < imageWidth; x++, pos += pixelSize, p++)
            {
                var pixel = Source.Map[DimensionConverter.Y(pos, imageWidth)][DimensionConverter.X(pos, imageWidth)];

                // check if we need to label current pixel
                if ((pixel.R > backgroundThresholdR) ||
                     (pixel.G > backgroundThresholdG) ||
                     (pixel.B > backgroundThresholdB))
                {
                    var prevPixel = Source.Map[DimensionConverter.Y(pos - 1, imageWidth)][DimensionConverter.X(pos - 1, imageWidth)];
                    // check if the previous pixel already was labeled
                    if ((prevPixel.R > backgroundThresholdR) ||
                         (prevPixel.G > backgroundThresholdG) ||
                         (prevPixel.B > backgroundThresholdB))
                    {
                        // label current pixel, as the previous
                        objectLabels[p] = objectLabels[p - 1];
                    }
                    else
                    {
                        // create new label
                        objectLabels[p] = ++labelsCount;
                    }
                }
            }

            // 2 - for other rows
            // for each row
            for (int y = 1; y < imageHeight; y++)
            {
                var rowFirstPixel = Source.Map[DimensionConverter.Y(pos, imageWidth)][DimensionConverter.X(pos, imageWidth)];
                // for the first pixel of the row, we need to check
                // only upper and upper-right pixels
                if ((rowFirstPixel.R > backgroundThresholdR) ||
                        (rowFirstPixel.G > backgroundThresholdG) ||
                        (rowFirstPixel.B > backgroundThresholdB))
                {
                    var abovePixel = Source.Map[DimensionConverter.Y(pos - stride, imageWidth)][DimensionConverter.X(pos - stride, imageWidth)];
                    var aboveRightPixel = Source.Map[DimensionConverter.Y(pos - strideM1, imageWidth)][DimensionConverter.X(pos - strideM1, imageWidth)];
                    // check surrounding pixels
                    if ((abovePixel.R > backgroundThresholdR) ||
                         (abovePixel.G > backgroundThresholdG) ||
                         (abovePixel.B > backgroundThresholdB))
                    {
                        // label current pixel, as the above
                        objectLabels[p] = objectLabels[p - imageWidth];
                    }
                    else if ((aboveRightPixel.R > backgroundThresholdR) ||
                              (aboveRightPixel.G > backgroundThresholdG) ||
                              (aboveRightPixel.B > backgroundThresholdB))
                    {
                        // label current pixel, as the above right
                        objectLabels[p] = objectLabels[p + 1 - imageWidth];
                    }
                    else
                    {
                        // create new label
                        objectLabels[p] = ++labelsCount;
                    }
                }
                pos += pixelSize;
                ++p;

                // check left pixel and three upper pixels for the rest of pixels
                for (int x = 1; x < imageWidth - 1; x++, pos += pixelSize, p++)
                {
                    var pixel = Source.Map[DimensionConverter.Y(pos, imageWidth)][DimensionConverter.X(pos, imageWidth)];
                    if ((pixel.R > backgroundThresholdR) ||
                         (pixel.G > backgroundThresholdG) ||
                         (pixel.B > backgroundThresholdB))
                    {
                        var leftColor = Source.Map[DimensionConverter.Y(pos - pixelSize, imageWidth)][DimensionConverter.X(pos - pixelSize, imageWidth)];
                        var aboveLeftColor = Source.Map[DimensionConverter.Y(pos - strideP1, imageWidth)][DimensionConverter.X(pos - strideP1, imageWidth)];
                        var aboveColor = Source.Map[DimensionConverter.Y(pos - stride, imageWidth)][DimensionConverter.X(pos - stride, imageWidth)];
                        var aboveRightColor = Source.Map[DimensionConverter.Y(pos - strideM1, imageWidth)][DimensionConverter.X(pos - strideM1, imageWidth)];

                        // check surrounding pixels
                        if ((leftColor.R > backgroundThresholdR) ||
                             (leftColor.G > backgroundThresholdG) ||
                             (leftColor.B > backgroundThresholdB))
                        {
                            // label current pixel, as the left
                            objectLabels[p] = objectLabels[p - 1];
                        }
                        else if ((aboveLeftColor.R > backgroundThresholdR) ||
                                  (aboveLeftColor.G > backgroundThresholdG) ||
                                  (aboveLeftColor.B > backgroundThresholdB))
                        {
                            // label current pixel, as the above left
                            objectLabels[p] = objectLabels[p - 1 - imageWidth];
                        }
                        else if ((aboveColor.R > backgroundThresholdR) ||
                                  (aboveColor.G > backgroundThresholdG) ||
                                  (aboveColor.B > backgroundThresholdB))
                        {
                            // label current pixel, as the above
                            objectLabels[p] = objectLabels[p - imageWidth];
                        }

                        if ((aboveRightColor.R > backgroundThresholdR) ||
                             (aboveRightColor.G > backgroundThresholdG) ||
                             (aboveRightColor.B > backgroundThresholdB))
                        {
                            if (objectLabels[p] == 0)
                            {
                                // label current pixel, as the above right
                                objectLabels[p] = objectLabels[p + 1 - imageWidth];
                            }
                            else
                            {
                                int l1 = objectLabels[p];
                                int l2 = objectLabels[p + 1 - imageWidth];

                                if ((l1 != l2) && (map[l1] != map[l2]))
                                {
                                    // merge
                                    if (map[l1] == l1)
                                    {
                                        // map left value to the right
                                        map[l1] = map[l2];
                                    }
                                    else if (map[l2] == l2)
                                    {
                                        // map right value to the left
                                        map[l2] = map[l1];
                                    }
                                    else
                                    {
                                        // both values already mapped
                                        map[map[l1]] = map[l2];
                                        map[l1] = map[l2];
                                    }

                                    // reindex
                                    for (int i = 1; i <= labelsCount; i++)
                                    {
                                        if (map[i] != i)
                                        {
                                            // reindex
                                            int j = map[i];
                                            while (j != map[j])
                                            {
                                                j = map[j];
                                            }
                                            map[i] = j;
                                        }
                                    }
                                }
                            }
                        }

                        // label the object if it is not yet
                        if (objectLabels[p] == 0)
                        {
                            // create new label
                            objectLabels[p] = ++labelsCount;
                        }
                    }
                }

                // for the last pixel of the row, we need to check
                // only upper and upper-left pixels
                var upperColor = Source.Map[DimensionConverter.Y(pos, imageWidth)][DimensionConverter.X(pos, imageWidth)];
                if ((upperColor.R > backgroundThresholdR) ||
                     (upperColor.G > backgroundThresholdG) ||
                     (upperColor.B > backgroundThresholdB))
                {
                    var upperLeftColor = Source.Map[DimensionConverter.Y(pos - pixelSize, imageWidth)][DimensionConverter.X(pos - pixelSize, imageWidth)];
                    var aboveLeftColor = Source.Map[DimensionConverter.Y(pos - strideP1, imageWidth)][DimensionConverter.X(pos - strideP1, imageWidth)];
                    var aboveColor = Source.Map[DimensionConverter.Y(pos - stride, imageWidth)][DimensionConverter.X(pos - stride, imageWidth)];
                    // check surrounding pixels
                    if ((upperLeftColor.R > backgroundThresholdR) ||
                         (upperLeftColor.G > backgroundThresholdG) ||
                         (upperLeftColor.B > backgroundThresholdB))
                    {
                        // label current pixel, as the left
                        objectLabels[p] = objectLabels[p - 1];
                    }
                    else if ((aboveLeftColor.R > backgroundThresholdR) ||
                              (aboveLeftColor.G > backgroundThresholdG) ||
                              (aboveLeftColor.B > backgroundThresholdB))
                    {
                        // label current pixel, as the above left
                        objectLabels[p] = objectLabels[p - 1 - imageWidth];
                    }
                    else if ((aboveColor.R > backgroundThresholdR) ||
                              (aboveColor.G > backgroundThresholdG) ||
                              (aboveColor.B > backgroundThresholdB))
                    {
                        // label current pixel, as the above
                        objectLabels[p] = objectLabels[p - imageWidth];
                    }
                    else
                    {
                        // create new label
                        objectLabels[p] = ++labelsCount;
                    }
                }
                pos += pixelSize;
                ++p;
            }

            // allocate remapping array
            int[] reMap = new int[map.Length];

            // count objects and prepare remapping array
            objectsCount = 0;
            for (int i = 1; i <= labelsCount; i++)
            {
                if (map[i] == i)
                {
                    // increase objects count
                    reMap[i] = ++objectsCount;
                }
            }
            // second pass to complete remapping
            for (int i = 1; i <= labelsCount; i++)
            {
                if (map[i] != i)
                {
                    reMap[i] = reMap[map[i]];
                }
            }

            // repair object labels
            for (int i = 0, n = objectLabels.Length; i < n; i++)
            {
                objectLabels[i] = reMap[objectLabels[i]];
            }
        }

        // Collect objects' rectangles
        private void CollectObjectsInfo(PixelMap sourcePixelRegion)
        {
            int i = 0, label;

            // create object coordinates arrays
            int[] x1 = new int[objectsCount + 1];
            int[] y1 = new int[objectsCount + 1];
            int[] x2 = new int[objectsCount + 1];
            int[] y2 = new int[objectsCount + 1];

            int[] area = new int[objectsCount + 1];
            long[] xc = new long[objectsCount + 1];
            long[] yc = new long[objectsCount + 1];

            long[] meanR = new long[objectsCount + 1];
            long[] meanG = new long[objectsCount + 1];
            long[] meanB = new long[objectsCount + 1];

            long[] stdDevR = new long[objectsCount + 1];
            long[] stdDevG = new long[objectsCount + 1];
            long[] stdDevB = new long[objectsCount + 1];

            for (int j = 1; j <= objectsCount; j++)
            {
                x1[j] = imageWidth;
                y1[j] = imageHeight;
            }

            // color images
            int pixelSize = 1;
            byte r, g, b; // RGB value

            // walk through labels array
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++, i++)
                {
                    // get current label
                    label = objectLabels[i];

                    // skip unlabeled pixels
                    if (label == 0)
                        continue;

                    // check and update all coordinates

                    if (x < x1[label])
                    {
                        x1[label] = x;
                    }
                    if (x > x2[label])
                    {
                        x2[label] = x;
                    }
                    if (y < y1[label])
                    {
                        y1[label] = y;
                    }
                    if (y > y2[label])
                    {
                        y2[label] = y;
                    }

                    area[label]++;
                    xc[label] += x;
                    yc[label] += y;

                    var c = Source.Map[y][x];
                    r = c.R;
                    g = c.G;
                    b = c.B;

                    meanR[label] += r;
                    meanG[label] += g;
                    meanB[label] += b;

                    stdDevR[label] += r * r;
                    stdDevG[label] += g * g;
                    stdDevB[label] += b * b;
                }
            }

            // create blobs
            blobs.Clear();

            for (int j = 1; j <= objectsCount; j++)
            {
                int blobArea = area[j];

                Blob blob = new Blob(j, new Rectangle(x1[j], y1[j], x2[j] - x1[j] + 1, y2[j] - y1[j] + 1));
                blob.Area = blobArea;
                blob.Fullness = (double)blobArea / ((x2[j] - x1[j] + 1) * (y2[j] - y1[j] + 1));
                blob.CenterOfGravity = new Point((float)xc[j] / blobArea, (float)yc[j] / blobArea);
                blob.ColorMean = new Pixel(255, (byte)(meanR[j] / blobArea), (byte)(meanG[j] / blobArea), (byte)(meanB[j] / blobArea));
                blob.ColorStdDev = new Pixel(
                    255,
                    (byte)(Math.Sqrt(stdDevR[j] / blobArea - blob.ColorMean.R * blob.ColorMean.R)),
                    (byte)(Math.Sqrt(stdDevG[j] / blobArea - blob.ColorMean.G * blob.ColorMean.G)),
                    (byte)(Math.Sqrt(stdDevB[j] / blobArea - blob.ColorMean.B * blob.ColorMean.B)));

                blobs.Add(blob);
            }
        }
    }
}
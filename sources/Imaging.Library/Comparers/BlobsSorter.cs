using Imaging.Library.Entities;
using Imaging.Library.Enums;
using System.Collections.Generic;

namespace Imaging.Library.Comparers
{
    public class BlobsSorter : IComparer<Blob>
    {
        private readonly ObjectsOrder order;

        public BlobsSorter(ObjectsOrder order)
        {
            this.order = order;
        }

        public int Compare(Blob a, Blob b)
        {
            var aRect = a.Rectangle;
            var bRect = b.Rectangle;

            switch (order)
            {
                case ObjectsOrder.Size: // sort by size

                    // the order is changed to descending
                    return (int)(bRect.Width * bRect.Height - aRect.Width * aRect.Height);

                case ObjectsOrder.Area: // sort by area
                    return b.Area - a.Area;

                case ObjectsOrder.YX: // YX order

                    return (int)(aRect.Y * 100000 + aRect.X - (bRect.Y * 100000 + bRect.X));

                case ObjectsOrder.XY: // XY order

                    return (int)(aRect.X * 100000 + aRect.Y - (bRect.X * 100000 + bRect.Y));
            }
            return 0;
        }
    }
}
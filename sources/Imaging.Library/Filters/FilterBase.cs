using Imaging.Library.Entities;

namespace Imaging.Library.Filters
{
    public abstract class FilterBase
    {
        public bool IsApplied { get; set; }
        public PixelMap Source { get; set; }

        public abstract void OnProcess();
    }
}
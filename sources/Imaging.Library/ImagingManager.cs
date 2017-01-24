using Imaging.Library.Entities;
using Imaging.Library.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Imaging.Library
{
    public class ImagingManager
    {
        public ImagingManager(PixelMap source)
        {
            Filters = new List<FilterBase>();
            Original = source;
        }

        public List<FilterBase> Filters { get; set; }

        internal PixelMap Input
        {
            get
            {
                var first = Filters.FirstOrDefault();

                return first == null ? new PixelMap(Original) : first.Source;
            }
        }

        public PixelMap Output => Filters.Count == 0 ? Original : Filters.Last().Source;

        public PixelMap Original { get; }

        public void Dispose()
        {
            if (Filters != null)
            {
                UndoAll();
                Filters = null;
            }
        }

        public void AddFilter(FilterBase filter)
        {
            filter.Source = Input;
            Filters.Add(filter);
        }

        public void Render()
        {
            foreach (var filter in Filters)
                if (!filter.IsApplied)
                {
                    filter.IsApplied = true;
                    filter.OnProcess();
                }
        }

        public bool CanUndo()
        {
            return Filters.Count > 0;
        }

        public void Undo()
        {
            if (Filters.Count != 0)
            {
                Filters.RemoveAt(Filters.Count - 1);
                var source = new PixelMap(Original);

                foreach (var filter in Filters)
                {
                    filter.IsApplied = false;
                    filter.Source = source;
                }
            }
        }

        public void UndoAll()
        {
            Filters.Clear();
        }
    }
}
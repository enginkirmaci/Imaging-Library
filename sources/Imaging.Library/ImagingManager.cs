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
            Filters.Add(filter);
        }

        public void Render()
        {
            if (Filters.Count > 0)
            {
                var input = new PixelMap(Original);

                foreach (var filter in Filters)
                {
                    filter.Source = input;
                    filter.OnProcess();
                }
            }
        }

        public bool CanUndo()
        {
            return Filters.Count > 0;
        }

        public void Undo()
        {
            if (Filters.Count != 0)
                Filters.RemoveAt(Filters.Count - 1);
        }

        public void UndoAll()
        {
            Filters.Clear();
        }
    }
}
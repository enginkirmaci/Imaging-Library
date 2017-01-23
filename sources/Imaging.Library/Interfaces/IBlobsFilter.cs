using Imaging.Library.Entities;

namespace Imaging.Library.Interfaces
{
    public interface IBlobsFilter
    {
        /// <summary>
        /// Check specified blob and decide if should be kept or not.
        /// </summary>
        ///
        /// <param name="blob">Blob to check.</param>
        ///
        /// <returns>Return <see langword="true"/> if the blob should be kept or
        /// <see langword="false"/> if it should be removed.</returns>
        ///
        bool Check(Blob blob);
    }
}
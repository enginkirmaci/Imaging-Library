namespace Imaging.Library.Filters.BasicFilters
{
    public class PixelInterpolation : FilterBase
    {
        private readonly int _pixelSize = 0;

        public PixelInterpolation(int pixelSize)
        {
            _pixelSize = pixelSize;
        }

        public override void OnProcess()
        {
            int offsetX = _pixelSize / 2;
            int offsetY = _pixelSize / 2;

            int width = Source.Width;
            int height = Source.Height;

            for (int x = 0; x < width; x += _pixelSize)
            {
                for (int y = 0; y < height; y += _pixelSize)
                {
                    // make sure that the offset is within the boundry of the image
                    while (x + offsetX >= width) offsetX--;
                    while (y + offsetY >= height) offsetY--;

                    // get the pixel color in the center of the soon to be pixelated area
                    var pixel = Source.Map[x + offsetX, y + offsetY];

                    // for each pixel in the pixelate size, set it to the center color
                    for (int i = x; i < x + _pixelSize && i < width; i++)
                        for (int j = y; j < y + _pixelSize && j < height; j++)
                            Source.Map[i, j] = pixel;
                }
            }
        }
    }
}
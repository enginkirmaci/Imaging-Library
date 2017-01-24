namespace Imaging.Library.Filters.BasicFilters
{
    public class OtsuThresholdFilter : FilterBase
    {
        public byte Threshold { get; set; }

        public override void OnProcess()
        {
            float[] vet = new float[256];
            int[] hist = new int[256];

            for (int y = 0; y < Source.Height; y++)
            {
                for (int x = 0; x < Source.Width; x += 3)
                {
                    hist[Source.Map[y][x].Luminance]++;
                }
            }

            float p1, p2, p12;
            int k;

            for (k = 1; k != 255; k++)
            {
                p1 = Px(0, k, hist);
                p2 = Px(k + 1, 255, hist);
                p12 = p1 * p2;
                if (p12 == 0)
                    p12 = 1;
                float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                vet[k] = (float)diff * diff / p12;
                //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
            }

            Threshold = (byte)findMax(vet, 256);

            for (int y = 0; y < Source.Height; y++)
            {
                for (int x = 0; x < Source.Width; x++)
                {
                    var pixel = Source.Map[y][x];

                    if (pixel.Luminance < Threshold)
                        pixel.B = pixel.G = pixel.R = 0;
                    else
                        pixel.B = pixel.G = pixel.R = 255;

                    Source.Map[y][x] = pixel;
                }
            }
        }

        private float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        private float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        private int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }
            return idx;
        }

        //public FactoryBase PostProcessFactory()
        //{
        //    CustomFilterFactory preProcess = new CustomFilterFactory(null);
        //    var filter = new ThresholdFilter(ImagingManager.DummySource, Threshold, Threshold, Threshold);
        //    preProcess.ApplyFilter(filter);

        //    return preProcess;
        //}
    }
}
using Imaging.Library.Entities;

namespace Imaging.Library.Constants
{
    public static class ConvolutionMatrices
    {
        public static ConvolutionMatrix Laplacian3x3 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {-1, -1, -1},
                {-1, 8, -1},
                {-1, -1, -1}
            }
        };

        public static ConvolutionMatrix Laplacian5x5 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {-1, -1, -1, -1, -1},
                {-1, -1, -1, -1, -1},
                {-1, -1, 24, -1, -1},
                {-1, -1, -1, -1, -1},
                {-1, -1, -1, -1, -1}
            }
        };

        public static ConvolutionMatrix LaplacianOfGaussian = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {0, 0, -1, 0, 0},
                {0, -1, -2, -1, 0},
                {-1, -2, 16, -2, -1},
                {0, -1, -2, -1, 0},
                {0, 0, -1, 0, 0}
            }
        };

        public static ConvolutionMatrix Gaussian3x3 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {1, 2, 1},
                {2, 4, 2},
                {1, 2, 1}
            }
        };

        public static ConvolutionMatrix Gaussian5x5Type1 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {2, 04, 05, 04, 2},
                {4, 09, 12, 09, 4},
                {5, 12, 15, 12, 5},
                {4, 09, 12, 09, 4},
                {2, 04, 05, 04, 2}
            }
        };

        public static ConvolutionMatrix Gaussian5x5Type2 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {1, 4, 7, 4, 1},
                {4, 16, 26, 16, 4},
                {7, 24, 41, 24, 7},
                {4, 16, 26, 16, 4},
                {1, 4, 7, 4, 1}
            }
        };

        public static ConvolutionMatrix Gaussian7x7 = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {0, 0, 0, 5, 0, 0, 0},
                {0, 5, 18, 32, 18, 5, 0},
                {0, 18, 64, 100, 64, 18, 0},
                {5, 32, 100, 100, 100, 32, 5},
                {0, 18, 64, 100, 64, 18, 0},
                {0, 5, 18, 32, 18, 5, 0},
                {0, 0, 0, 5, 0, 0, 0}
            }
        };

        public static ConvolutionMatrix Sobel3x3Horizontal = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            }
        };

        public static ConvolutionMatrix Sobel3x3Vertical = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {1, 2, 1},
                {0, 0, 0},
                {-1, -2, -1}
            }
        };

        public static ConvolutionMatrix Prewitt3x3Horizontal = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {-1, 0, 1},
                {-1, 0, 1},
                {-1, 0, 1}
            }
        };

        public static ConvolutionMatrix Prewitt3x3Vertical = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {1, 1, 1},
                {0, 0, 0},
                {-1, -1, -1}
            }
        };

        public static ConvolutionMatrix Kirsch3x3Horizontal = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {5, 5, 5},
                {-3, 0, -3},
                {-3, -3, -3}
            }
        };

        public static ConvolutionMatrix Kirsch3x3Vertical = new ConvolutionMatrix()
        {
            Matrix = new[,]
            {
                {5, -3, -3},
                {5, 0, -3},
                {5, -3, -3}
            }
        };
    }
}
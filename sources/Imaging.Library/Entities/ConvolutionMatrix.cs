namespace Imaging.Library.Entities
{
    public class ConvolutionMatrix
    {
        private int[,] _matrix =
        {
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
        };

        private int _size = 5;

        public ConvolutionMatrix()
        {
            Offset = 0;
            Factor = 1;
        }

        public int Factor { get; set; }
        public int Offset { get; set; }

        public int[,] Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;

                _size = _matrix.GetLength(0);

                Factor = 0;
                for (var i = 0; i < Size; i++)
                    for (var j = 0; j < Size; j++)
                        Factor += _matrix[i, j];

                if (Factor == 0)
                    Factor = 1;
            }
        }

        public int Size
        {
            get { return _size; }
            set
            {
                if (value != 1 && value != 3 && value != 5 && value != 7)
                    _size = 5;
                else
                    _size = value;
            }
        }
    }
}
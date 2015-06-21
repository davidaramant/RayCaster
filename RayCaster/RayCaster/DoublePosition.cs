namespace RayCasterGame
{
    sealed class DoublePosition
    {
        public double X;
        public double Y;

        public Position TruncateToPosition()
        {
            return new Position { X = (int)X, Y = (int)Y };
        }

        public DoublePosition Clone()
        {
            return new DoublePosition { X = X, Y = Y };
        }
    }
}

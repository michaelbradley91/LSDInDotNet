namespace LSDInDotNet.Models
{
    public struct CoordinateList
    {
        public int X;
        public int Y;
        private object _nextCoordinate;

        public CoordinateList? Next
        {
            get => _nextCoordinate as CoordinateList?;
            set => _nextCoordinate = value;
        }
    }
}

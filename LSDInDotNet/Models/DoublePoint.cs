using System;

namespace LSDInDotNet.Models
{
    public class DoublePoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public DoublePoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public DoublePoint Clone()
        {
            return new DoublePoint(X, Y);
        }

        public double GetDistanceFrom(DoublePoint otherPoint)
        {
            return GetDistanceBetween(X, Y, otherPoint.X, otherPoint.Y);
        }

        private static double GetDistanceBetween(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }
    }
}

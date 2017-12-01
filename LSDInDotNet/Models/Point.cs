using System;

namespace LSDInDotNet.Models
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public double GetDistanceFrom(Point otherPoint)
        {
            return GetDistanceBetween(X, Y, otherPoint.X, otherPoint.Y);
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

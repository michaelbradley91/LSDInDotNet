using System;

namespace LSDInDotNet.Models
{
    public struct Point
    {
        public int X;
        public int Y;

        public double GetDistanceFrom(Point otherPoint)
        {
            return GetDistanceBetween(X, Y, otherPoint.X, otherPoint.Y);
        }

        private static double GetDistanceBetween(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }
    }
}

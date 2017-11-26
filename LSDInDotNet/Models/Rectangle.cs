﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LSDInDotNet.Models
{
    public struct Rectangle : IEnumerable<Point>
    {
        public Point FirstPoint { get; private set; }
        public Point SecondPoint { get; private set; }
        public double Width { get; private set; }
        public Point Centre { get; private set; }
        public double Angle { get; private set; }
        public double DeltaX { get; private set; }
        public double DeltaY { get; private set; }
        public double Precision { get; private set; }
        public double ProbabilityOfPointWithAngleWithinPrecision { get; private set }

        public Rectangle Clone()
        {
            return new Rectangle
            {
                FirstPoint = FirstPoint,
                SecondPoint = SecondPoint,
                Width = Width,
                Centre = Centre,
                Angle = Angle,
                DeltaX = DeltaX,
                DeltaY = DeltaY,
                Precision = Precision,
                ProbabilityOfPointWithAngleWithinPrecision = ProbabilityOfPointWithAngleWithinPrecision
            };
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return new RectangleExplorer(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

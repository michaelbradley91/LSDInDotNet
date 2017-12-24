using System;
using System.Collections;
using System.Collections.Generic;
using LSDInDotNet.Helpers;

namespace LSDInDotNet.Models
{
    public class RectangleExplorer<T> : IEnumerator<Point>
    {
        // Left most first, then bottom (largest y value)
        private readonly DoublePoint[] _fourCornersInCircularOrder;
        private double _yStart;
        private double _yEnd;
        private Point _exploredPixel;

        // If we are exploring a pixel right of any pixel in the rectangle
        private bool HasFinished => _exploredPixel.X > _fourCornersInCircularOrder[2].X;

        public RectangleExplorer(Rectangle<T> rectangle)
        {
            var x1 = rectangle.FirstPoint.X;
            var x2 = rectangle.SecondPoint.X;
            var y1 = rectangle.FirstPoint.Y;
            var y2 = rectangle.SecondPoint.Y;
            var dx = rectangle.DeltaX;
            var dy = rectangle.DeltaY;
            var width = rectangle.Width;

            var corners = new[]
            {
                new DoublePoint(x1 - dy * width / 2.0, y1 + dx * width / 2.0),
                new DoublePoint(x2 - dy * width / 2.0, y2 + dx * width / 2.0),
                new DoublePoint(x2 + dy * width / 2.0, y2 - dx * width / 2.0),
                new DoublePoint(x1 + dy * width / 2.0, y1 - dx * width / 2.0)
            };

            int offset;
            if (x1 < x2 && y1 <= y2) offset = 0;
            else if (x1 >= x2 && y1 < y2) offset = 1;
            else if (x1 > x2 && y1 >= y2) offset = 2;
            else offset = 3;

            _fourCornersInCircularOrder = new DoublePoint[4];
            for (var n = 0; n < 4; n++)
            {
                _fourCornersInCircularOrder[n] = corners[(offset + n) % 4];
                _fourCornersInCircularOrder[n] = corners[(offset + n) % 4];
            }

            InitialiseExploration();
        }

        private void MoveToNextPixelToExplore()
        {
            if (HasFinished) return;

            // Move down the column
            _exploredPixel.Y++;
            while (_exploredPixel.Y > _yEnd && !HasFinished)
            {
                // Next column
                _exploredPixel.X++;
                if (HasFinished) return;

                // Interpolate across the relevant edges to find the start and end y coordinates
                if (_exploredPixel.X < _fourCornersInCircularOrder[3].X)
                {
                    _yStart = MathHelpers.InterpolateLow(_exploredPixel.X,
                        _fourCornersInCircularOrder[0],
                        _fourCornersInCircularOrder[3]);
                }
                else
                {
                    _yStart = MathHelpers.InterpolateLow(_exploredPixel.X,
                        _fourCornersInCircularOrder[3],
                        _fourCornersInCircularOrder[2]);
                }

                if (_exploredPixel.X < _fourCornersInCircularOrder[1].X)
                {
                    _yEnd = MathHelpers.InterpolateHigh(_exploredPixel.X,
                        _fourCornersInCircularOrder[0],
                        _fourCornersInCircularOrder[1]);
                }
                else
                {
                    _yEnd = MathHelpers.InterpolateHigh(_exploredPixel.X,
                        _fourCornersInCircularOrder[1],
                        _fourCornersInCircularOrder[2]);
                }

                _exploredPixel.Y = (int)Math.Ceiling(_yStart);
            }
        }

        public bool MoveNext()
        {
            MoveToNextPixelToExplore();
            return !HasFinished;
        }

        public void Reset()
        {
            InitialiseExploration();
        }

        private void InitialiseExploration()
        {
            _exploredPixel = new Point((int)Math.Ceiling(_fourCornersInCircularOrder[0].X) - 1,
                (int)Math.Ceiling(_fourCornersInCircularOrder[0].Y));
            _yStart = _yEnd = double.MinValue;

            MoveToNextPixelToExplore();
        }

        public Point Current => new Point(_exploredPixel.X, _exploredPixel.Y);
        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}

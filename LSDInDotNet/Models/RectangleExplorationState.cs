using System;
using LSDInDotNet.Helpers;

namespace LSDInDotNet.Models
{
    public struct RectangleExplorationState
    {
        public RectangleExplorationState(Rectangle rectangle)
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

            FourCornersInCircularOrder = new DoublePoint[4];
            for (var n = 0; n < 4; n++)
            {
                FourCornersInCircularOrder[n] = corners[(offset + n) % 4];
                FourCornersInCircularOrder[n] = corners[(offset + n) % 4];
            }

            ExploredPixel = new Point((int) Math.Ceiling(FourCornersInCircularOrder[0].X) - 1,
                (int) Math.Ceiling(FourCornersInCircularOrder[0].Y));
            YStart = YEnd = double.MinValue;

            MoveToNextPixelToExplore();
        }

        // Left most first, then bottom (largest y value)
        public DoublePoint[] FourCornersInCircularOrder { get; set; }

        public double YStart { get; set; }
        public double YEnd { get; set; }
        public Point ExploredPixel;

        // If we are exploring a pixel right of any pixel in the rectangle
        public bool HasFinished => ExploredPixel.X > FourCornersInCircularOrder[2].X;

        public void MoveToNextPixelToExplore()
        {
            if (HasFinished) return;

            // Move down the column
            ExploredPixel.Y++;
            while (ExploredPixel.Y > YEnd && !HasFinished)
            {
                // Next column
                ExploredPixel.X++;
                if (HasFinished) return;

                // Interpolate across the relevant edges to find the start and end y coordinates
                if (ExploredPixel.X < FourCornersInCircularOrder[3].X)
                {
                    YStart = MathHelpers.InterpolateLow(ExploredPixel.X,
                        FourCornersInCircularOrder[0],
                        FourCornersInCircularOrder[3]);
                }
                else
                {
                    YStart = MathHelpers.InterpolateLow(ExploredPixel.X,
                        FourCornersInCircularOrder[3],
                        FourCornersInCircularOrder[2]);
                }

                if (ExploredPixel.X < FourCornersInCircularOrder[1].X)
                {
                    YStart = MathHelpers.InterpolateLow(ExploredPixel.X,
                        FourCornersInCircularOrder[0],
                        FourCornersInCircularOrder[1]);
                }
                else
                {
                    YStart = MathHelpers.InterpolateLow(ExploredPixel.X,
                        FourCornersInCircularOrder[1],
                        FourCornersInCircularOrder[2]);
                }

                ExploredPixel.Y = (int)Math.Ceiling(YStart);
            }
        }
    }
}

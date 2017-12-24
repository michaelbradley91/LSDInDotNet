using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using LSDInDotNet.Helpers;

namespace LSDInDotNet.Models
{
    public struct Region<T>
    {
        public Image<double, T> ModGradientImage;
        public Image<double, T> Angles;
        public Image<bool, T> Used;
        public LinkedList<Point> Points;
        public double RegionAngle;
        public double Precision;
        public int Size => Points.Count;

        private Region(Image<double,T> modGradientImage, Image<double,T> angles, Image<bool, T> used, double precision)
        {
            if (precision < 0) throw new ArgumentOutOfRangeException(nameof(precision), "Must be non-negative");
            
            ModGradientImage = modGradientImage;
            Angles = angles;
            Used = used;
            Points = new LinkedList<Point>();
            RegionAngle = 0.0;
            Precision = precision;
        }

        public static Region<T> Create(Point start, Image<double, T> angles, Image<double, T> modGradientImage, Image<bool, T> used, double precision)
        {
            if (start.X < 0 || start.X >= angles.Width) throw new ArgumentOutOfRangeException(nameof(start), "x coordinate not within the image");
            if (start.Y < 0 || start.Y >= angles.Height) throw new ArgumentOutOfRangeException(nameof(start), "y coordinate not within the image");
            
            var region = new Region<T>(modGradientImage, angles, used, precision);
            region.UpdatePointsAndAngle(start);
            return region;
        }

        private void UpdatePointsAndAngle(Point start)
        {
            Points.Clear();
            Points.AddLast(start);

            RegionAngle = Angles[start];
            var sumDx = Math.Cos(RegionAngle);
            var sumDy = Math.Sin(RegionAngle);
            Used[start] = true;

            var node = Points.First;
            while (node != null)
            {
                var p = node.Value;
                for (var xx = p.X - 1; xx <= p.X + 1; xx++)
                {
                    for (var yy = p.Y - 1; yy <= p.Y + 1; yy++)
                    {
                        if (xx < 0 || yy < 0 || xx >= Used.Width || yy >= Used.Height ||
                            Used[xx, yy] || !Angles[xx, yy].IsAlignedUpToPrecision(RegionAngle, Precision))
                        {
                            continue;
                        }

                        Used[xx, yy] = true;
                        Points.AddLast(new Point(xx, yy));

                        sumDx += Math.Cos(Angles[xx, yy]);
                        sumDy += Math.Sin(Angles[xx, yy]);
                        RegionAngle = Math.Atan2(sumDy, sumDx);
                    }
                }
                node = node.Next;
            }
        }

        public Rectangle<T> ToRectangle(double probabilityOfPointWithAngleWithinPrecision)
        {
            var centre = GetCentre();
            var angle = GetAngle(centre);

            var dx = Math.Cos(angle);
            var dy = Math.Sin(angle);
            var minLengthFromCentre = 0.0;
            var maxLengthFromCentre = 0.0;
            var minWidthFromCentre = 0.0;
            var maxWidthFromCentre = 0.0;

            foreach (var p in Points)
            {
                var lengthFromCentre = (p.X - centre.X) * dx + (p.Y - centre.Y) * dy;
                var widthFromCentre = (p.X - centre.X) * dy + (p.Y - centre.Y) * dx;

                if (lengthFromCentre > maxLengthFromCentre) maxLengthFromCentre = lengthFromCentre;
                if (lengthFromCentre < minLengthFromCentre) minLengthFromCentre = lengthFromCentre;
                if (widthFromCentre > maxWidthFromCentre) maxWidthFromCentre = widthFromCentre;
                if (widthFromCentre < minWidthFromCentre) minWidthFromCentre = widthFromCentre;
            }

            var rectangle = new Rectangle<T>
            {
                FirstPoint = new DoublePoint(centre.X + minLengthFromCentre * dx, centre.Y + minLengthFromCentre * dy),
                SecondPoint = new DoublePoint(centre.X + maxLengthFromCentre * dx, centre.Y + maxLengthFromCentre * dy),
                Width = maxWidthFromCentre - minWidthFromCentre,
                Centre = centre.Clone(),
                Angle = angle,
                DeltaX = dx,
                DeltaY = dy,
                Precision = Precision,
                ProbabilityOfPointWithAngleWithinPrecision = probabilityOfPointWithAngleWithinPrecision,
                Angles = Angles
            };

            // A sharp horizontal or vertical step would give a width of zero.
            // We correct this to one as it corresponds to a one pixel transition in the image
            if (rectangle.Width < 1.0) rectangle.Width = 1.0;

            return rectangle;
        }

        private DoublePoint GetCentre()
        {
            var cx = 0.0;
            var cy = 0.0;
            var totalWeight = 0.0;

            foreach (var p in Points)
            {
                var weight = ModGradientImage[p];
                cx += p.X * weight;
                cy += p.Y * weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0) throw new InvalidOperationException("Weights in region summed to zero");
            cx /= totalWeight;
            cy /= totalWeight;
            
            return new DoublePoint(cx, cy);
        }

        /// <summary>
        /// Computes the region's angle as the principal interia axis of the region
        /// </summary>
        /// <returns></returns>
        private double GetAngle(DoublePoint centre)
        {
            var ixx = 0.0;
            var iyy = 0.0;
            var ixy = 0.0;
            foreach (var p in Points)
            {
                var weight = ModGradientImage[p];
                ixx += (p.Y - centre.Y) * (p.Y - centre.Y) * weight;
                iyy += (p.X - centre.X) * (p.X - centre.X) * weight;
                ixy -= (p.X - centre.X) * (p.Y - centre.Y) * weight;
            }
            if (ixx.IsRoughlyEqualTo(0.0) && iyy.IsRoughlyEqualTo(0.0) && ixy.IsRoughlyEqualTo(0.0))
            {
                throw new InvalidOperationException("Null inertia matrix associated to region");
            }

            var smallestEigenValue = 0.5 * (ixx + iyy - Math.Sqrt((ixx - iyy) * (ixx - iyy) + 4.0 * ixy * ixy));

            var angle = Math.Abs(ixx) > Math.Abs(iyy)
                ? Math.Atan2(smallestEigenValue - ixx, ixy)
                : Math.Atan2(ixy, smallestEigenValue - iyy);

            if (angle.AbsoluteAngleDifferenceTo(RegionAngle) > Precision) angle += MathHelpers.Pi;

            return angle;
        }

        /// <summary>
        /// Reduce the region size by eliminating points far from the starting point until that leads
        /// to a rectangle with the right density of region points. Returns true if the region
        /// was shrunk successfully, and false otherwise.
        /// </summary>
        public bool ReduceRadius(double targetDensity, ref Rectangle<T> rectangle)
        {
            var currentDensity = GetDensity(rectangle);
            if (currentDensity >= targetDensity) return true;

            var centre = Points.First.Value;
            var radius = Math.Max(centre.GetDistanceFrom(rectangle.FirstPoint), centre.GetDistanceFrom(rectangle.SecondPoint));

            while (currentDensity < targetDensity)
            {
                radius *= 0.75;
                var currentNode = Points.First;
                while (currentNode != null)
                {
                    var nextNode = currentNode.Next;
                    var currentPoint = currentNode.Value;
                    if (centre.GetDistanceFrom(currentPoint) > radius)
                    {
                        Used[currentPoint] = false;
                        Points.Remove(currentNode);
                    }
                    currentNode = nextNode;
                }

                // If the region is now insignificant, discard it
                if (Size < 2) return false;

                // We need to modify the rectangle passed in
                // TODO update this method to return the rectangle.
                rectangle = ToRectangle(rectangle.ProbabilityOfPointWithAngleWithinPrecision);

                targetDensity = GetDensity(rectangle);
            }

            return true;
        }

        /// <summary>
        /// Refine a region by computing the angle tolerance based on the standard
        /// deviation of the angle at points near the region's starting point.
        /// A new region is then grown from the same point.
        /// </summary>
        public bool Refine(double targetDensity, ref Rectangle<T> rectangle)
        {
            var currentDensity = GetDensity(rectangle);
            if (currentDensity >= targetDensity) return true;

            var centre = Points.First.Value;
            var angleCentre = Angles[centre];
            var sum = 0.0;
            var squareSum = 0.0;
            var numberOfPoints = 0;

            foreach (var point in Points)
            {
                Used[point] = false;

                if (!(centre.GetDistanceFrom(point) < rectangle.Width)) continue;

                var angle = Angles[point];
                var angleDifference = angle.SignedAngleDifferenceTo(angleCentre);
                sum += angleDifference;
                squareSum += angleDifference * angleDifference;
                numberOfPoints++;
            }

            var meanAngle = sum / numberOfPoints;
            // two times the standard deviation
            Precision = 2.0 * Math.Sqrt((squareSum - 2.0 * meanAngle * sum) / numberOfPoints + meanAngle * meanAngle);
            UpdatePointsAndAngle(centre);

            // If the region is now insignificant, discard it
            if (Size < 2) return false;

            rectangle = ToRectangle(rectangle.ProbabilityOfPointWithAngleWithinPrecision);
            currentDensity = GetDensity(rectangle);

            return !(currentDensity < targetDensity) || ReduceRadius(targetDensity, ref rectangle);
        }

        private double GetDensity(Rectangle<T> rectangle)
        {
            return Size / (rectangle.FirstPoint.GetDistanceFrom(rectangle.SecondPoint) * rectangle.Width);
        }
    }
}

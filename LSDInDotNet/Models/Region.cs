using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LSDInDotNet.Helpers;

namespace LSDInDotNet.Models
{
    public struct Region<T>
    {
        public Image<double, T> ModGradientImage;
        public Image<double, T> Angles;
        public LinkedList<Point> Points;
        public double RegionAngle;
        public double Precision;
        public int Size => Points.Count;

        private Region(Image<double,T> modGradientImage, Image<double,T> angles, LinkedList<Point> points, double regionAngle, double precision)
        {
            if (points == null || points.Count <= 1) throw new ArgumentException("The region must have more than one point", nameof(points));
            if (precision < 0) throw new ArgumentOutOfRangeException(nameof(precision), "Must be non-negative");
            
            ModGradientImage = modGradientImage;
            Angles = angles;
            Points = points;
            RegionAngle = regionAngle;
            Precision = precision;
        }

        public static Region<T> Create(Point start, Image<double, T> angles, Image<double, T> modGradientImage, Image<bool, T> used, double precision)
        {
            if (start.X < 0 || start.X >= angles.Width) throw new ArgumentOutOfRangeException(nameof(start), "x coordinate not within the image");
            if (start.Y < 0 || start.Y >= angles.Height) throw new ArgumentOutOfRangeException(nameof(start), "y coordinate not within the image");
            
            var regionPoints = new LinkedList<Point>();
            regionPoints.AddLast(start);

            var regionAngle = angles[start];
            var sumDx = Math.Cos(regionAngle);
            var sumDy = Math.Sin(regionAngle);
            used[start] = true;

            var node = regionPoints.First;
            while (node != null)
            {
                var p = node.Value;
                for (var xx = p.X - 1; xx < p.X + 1; xx++)
                {
                    for (var yy = p.Y - 1; yy < p.Y + 1; yy++)
                    {
                        if (xx < 0 || yy < 0 || xx >= used.Width || yy >= used.Height ||
                            used[xx, yy] || !angles[xx, yy].IsAlignedUpToPrecision(regionAngle, precision))
                        {
                            continue;
                        }

                        used[xx, yy] = true;
                        regionPoints.AddLast(new Point(xx, yy));

                        sumDx += Math.Cos(angles[xx, yy]);
                        sumDy += Math.Sin(angles[xx, yy]);
                        regionAngle = Math.Atan2(sumDy, sumDx);
                    }
                }
                node = node.Next;
            }

            return new Region<T>(modGradientImage, angles, regionPoints, regionAngle, precision);
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
    }
}

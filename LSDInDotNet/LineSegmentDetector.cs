using System;
using LSDInDotNet.Helpers;
using LSDInDotNet.Models;
using LSDInDotNet.Services;

namespace LSDInDotNet
{
    public interface ILineSegmentDetector
    {
        /// <summary>
        /// TODO: write explanation!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image"></param>
        /// <param name="scale"></param>
        /// <param name="sigmaScale"></param>
        /// <param name="quantizationErrorBound"></param>
        /// <param name="angleThreshold"></param>
        /// <param name="detectionThreshold"></param>
        /// <param name="densityThreshold"></param>
        /// <param name="numberOfBins"></param>
        Tuple<TupleList, Image<int, T>> Run<T>(
            Image<double, T> image,
            double scale = 0.8,
            double sigmaScale = 0.6,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024);

        /// <summary>
        /// TODO: write explanation!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scaledImage"></param>
        /// <param name="scale"></param>
        /// <param name="quantizationErrorBound"></param>
        /// <param name="angleThreshold"></param>
        /// <param name="detectionThreshold"></param>
        /// <param name="densityThreshold"></param>
        /// <param name="numberOfBins"></param>
        /// <returns></returns>
        Tuple<TupleList, Image<int,T>> Run<T>(
            Image<double, T> scaledImage,
            double scale,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024);
    }

    public class LineSegmentDetector : ILineSegmentDetector
    {
        private readonly IImageScaler _imageScaler;
        private readonly ILevelLineCalculator _levelLineCalculator;

        public static ILineSegmentDetector Create()
        {
            return DependencyResolver.Resolve<ILineSegmentDetector>();
        }

        public LineSegmentDetector(IImageScaler imageScaler, ILevelLineCalculator levelLineCalculator)
        {
            _imageScaler = imageScaler;
            _levelLineCalculator = levelLineCalculator;
        }
        
        public Tuple<TupleList, Image<int, T>> Run<T>(
            Image<double, T> image,
            double scale = 0.8,
            double sigmaScale = 0.6,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (scale <= 0.0) throw new ArgumentOutOfRangeException(nameof(scale), "Must be positive");
            if (sigmaScale <= 0.0) throw new ArgumentOutOfRangeException(nameof(sigmaScale), "Must be positive");
            if (quantizationErrorBound < 0.0) throw new ArgumentOutOfRangeException(nameof(quantizationErrorBound), "Must be non-negative");
            if (angleThreshold <= 0.0 || angleThreshold >= 180.0) throw new ArgumentOutOfRangeException(nameof(angleThreshold), "Must be in the range (0,180)");
            if (densityThreshold < 0.0 || densityThreshold > 1.0) throw new ArgumentOutOfRangeException(nameof(detectionThreshold), "Must be in the range [0,1]");
            if (numberOfBins <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfBins), "Must be positive");

            

            if (!scale.IsRoughlyEqualTo(1))
            {
                image = _imageScaler.ScaleWithGuassianSampler(image, scale, sigmaScale);
            }
            return Run(image, scale, quantizationErrorBound, angleThreshold, densityThreshold, densityThreshold, numberOfBins);
        }

        public Tuple<TupleList, Image<int, T>> Run<T>(
            Image<double, T> scaledImage,
            double scale,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024)
        {
            var precision = Math.PI * angleThreshold / 180.0;
            var probabilityOfPointWithAngleWithinPrecision = angleThreshold / 180.0;
            var gradientMagnitudeThreshold = quantizationErrorBound / Math.Sin(precision);

            var levelLineResult = _levelLineCalculator.CreateLevelLineImage(scaledImage, gradientMagnitudeThreshold, numberOfBins);

            var angles = levelLineResult.GradientImage;
            var modGradientImage = levelLineResult.ModGradientImage;
            var coordinates = levelLineResult.CoordinatesOrderedByDecreasingGradientMagnitude;

            var width = angles.Width;
            var height = angles.Height;

            var logNumberOfTests = 5.0 * (Math.Log10(width) + Math.Log10(height)) / 2.0 + Math.Log10(11.0);
            var minimumRegionSize = (int) (-logNumberOfTests / Math.Log10(probabilityOfPointWithAngleWithinPrecision));
            var used = new Image<bool, T>(width, height, scaledImage.Metadata);

            var lineSegments = new TupleList(7);
            var regionOutput = new Image<int, T>(angles.Width, angles.Height, angles.Metadata);
            foreach (var coordinate in coordinates)
            {
                try
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (used[coordinate] || angles[coordinate] == MathHelpers.NoAngle) continue;

                    var region = Region<T>.Create(coordinate, angles, modGradientImage, used, precision);
                    if (region.Size < minimumRegionSize) continue;

                    var rectangle = region.ToRectangle(probabilityOfPointWithAngleWithinPrecision);

                    // Rectangle must be updated by the refine call
                    if (!region.Refine(densityThreshold, ref rectangle)) continue;

                    var logNumberOfFalseAlarms = rectangle.Improve(logNumberOfTests, detectionThreshold);

                    if (logNumberOfFalseAlarms <= detectionThreshold) continue;

                    rectangle.FirstPoint.X += 0.5;
                    rectangle.SecondPoint.X += 0.5;
                    rectangle.FirstPoint.Y += 0.5;
                    rectangle.SecondPoint.Y += 0.5;

                    if (!scale.IsRoughlyEqualTo(1))
                    {
                        rectangle.FirstPoint.X /= scale;
                        rectangle.FirstPoint.Y /= scale;
                        rectangle.SecondPoint.X /= scale;
                        rectangle.SecondPoint.Y /= scale;
                        rectangle.Width /= scale;
                    }

                    lineSegments.AddTuple(
                        rectangle.FirstPoint.X, rectangle.FirstPoint.Y,
                        rectangle.SecondPoint.X, rectangle.SecondPoint.Y,
                        rectangle.Width,
                        rectangle.ProbabilityOfPointWithAngleWithinPrecision,
                        logNumberOfFalseAlarms);

                    // Add the region to the image
                    foreach (var point in region.Points)
                    {
                        // Use the number of line segments so far to identify this
                        // particular line segment.
                        regionOutput[point] = lineSegments.Size;
                    }
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw e;
                }
            }

            return Tuple.Create(lineSegments, regionOutput);
        }
    }
}

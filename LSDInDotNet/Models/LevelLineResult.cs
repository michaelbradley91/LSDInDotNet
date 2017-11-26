using System;

namespace LSDInDotNet.Models
{
    public struct LevelLineResult<T>
    {
        public Image<double, T> GradientImage { get; }
        public Image<double, T> ModGradientImage { get; }
        public Point[] CoordinatesOrderedByDecreasingGradientMagnitude { get; }

        public LevelLineResult(Image<double, T> gradientImage, Image<double, T> modGradientImage, Point[] coordinatesOrderedByDecreasingGradientMagnitude)
        {
            if (gradientImage.Width != modGradientImage.Width || gradientImage.Height != modGradientImage.Height)
            {
                throw new ArgumentException("The images in the level line result do not have the same size");
            }

            GradientImage = gradientImage;
            ModGradientImage = modGradientImage;
            CoordinatesOrderedByDecreasingGradientMagnitude = coordinatesOrderedByDecreasingGradientMagnitude ??
                throw new ArgumentNullException(nameof(coordinatesOrderedByDecreasingGradientMagnitude));
        }
    }
}

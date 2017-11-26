using System;
using System.Collections.Generic;
using LSDInDotNet.Helpers;
using LSDInDotNet.Models;

namespace LSDInDotNet.Services
{
    public interface ILevelLineGenerator
    {
        LevelLineResult<T> CreateLevelLineImage<T>(Image<double, T> angleImage, double threshold, CoordinateList coordinates, int numberOfBins);
    }

    public class LevelLineGenerator : ILevelLineGenerator
    {
        public LevelLineResult<T> CreateLevelLineImage<T>(Image<double, T> angleImage, double threshold, CoordinateList coordinates, int numberOfBins)
        {
            if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold), "Must be positive");
            if (numberOfBins <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfBins), "The number of bins must be positive");

            var maxGradient = 0.0;

            var width = angleImage.Width;
            var height = angleImage.Height;

            var gradientImage = new Image<double, T>(width, height, angleImage.Metadata);
            var modGradientImage = new Image<double, T>(width, height, angleImage.Metadata);
            
            // Gradients are null on the bottom and right border of the image
            for (var x = 0; x < width; x++) gradientImage[x, height - 1] = MathHelpers.NoAngle;
            for (var y = 0; y < height; y++) gradientImage[width - 1, y] = MathHelpers.NoAngle;

            // Compute the gradient for the remaining pixels
            for (var x = 0; x < width - 1; x++)
            {
                for (var y = 0; y < height - 1; y++)
                {
                    /*
                     * Norm 2 computation using 2x2 pixel window:
                         A B
                         C D
                       and
                         com1 = D-A,  com2 = B-C.
                       Then
                         gx = B+D - (A+C)   horizontal difference
                         gy = C+D - (A+B)   vertical difference
                       com1 and com2 are just to avoid 2 additions.
                     */
                    var com1 = angleImage[x + 1, y + 1] - angleImage[x, y];
                    var com2 = angleImage[x + 1, y] - angleImage[x, y + 1];
                    var gx = com1 + com2;
                    var gy = com1 - com2;
                    var normSortOfSquared = gx * gx + gy * gy;
                    var norm = Math.Sqrt(normSortOfSquared / 4.0);

                    modGradientImage[x, y] = norm;

                    // If the normal is too small, then assume no gradient (likely noise)
                    if (norm <= threshold)
                    {
                        gradientImage[x, y] = MathHelpers.NoAngle;
                    }
                    else
                    {
                        gradientImage[x, y] = Math.Atan2(gx, -gy);

                        if (norm > maxGradient) maxGradient = norm;
                    }
                }
            }

            // Compute the histogram of gradient values

            var bins = new LinkedList<Point>[numberOfBins];
            for (var i = 0; i < numberOfBins; i++) bins[i] = new LinkedList<Point>();

            for (var x = 0; x < width - 1; x++)
            {
                for (var y = 0; y < height - 1; y++)
                {
                    var norm = modGradientImage[x, y];
                    var index = (int)(norm * numberOfBins / maxGradient);
                    if (index >= numberOfBins) index = numberOfBins - 1;

                    bins[index].AddLast(new Point(x, y));
                }
            }

            var orderedCoordinates = new Point[(width - 1) * (height - 1)];
            var currentIndex = 0;
            for (var i = numberOfBins - 1; i > 0; i--)
            {
                bins[i].CopyTo(orderedCoordinates, currentIndex);
                currentIndex += bins[i].Count;
            }

            return new LevelLineResult<T>(gradientImage, modGradientImage, orderedCoordinates);
        }
    }
}

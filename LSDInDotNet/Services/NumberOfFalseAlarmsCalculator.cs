using System;
using LSDInDotNet.Helpers;
using LSDInDotNet.Models;

namespace LSDInDotNet.Services
{
    public interface INumberOfFalseAlarmsCalculator
    {
        double CalculateLog10ExpectedNumberOfFalseAlarmsInRectangle<T>(Rectangle rectangle,
            Image<double, T> angles, double logNumberOfTests);
    }

    public class NumberOfFalseAlarmsCalculator : INumberOfFalseAlarmsCalculator
    {
        private const double MaximumErrorAllowed = 0.1; // an error of 10% is accepted

        public double CalculateLog10ExpectedNumberOfFalseAlarmsInRectangle<T>(Rectangle rectangle, Image<double, T> angles,
            double logNumberOfTests)
        {
            var numberOfPoints = 0;
            var numberOfAlignedPoints = 0;

            var rectangleExplorationState = new RectangleExplorationState(rectangle);

            while (!rectangleExplorationState.HasFinished)
            {
                var p = rectangleExplorationState.ExploredPixel;
                if (p.X >= 0 && p.Y >= 0 && p.X < angles.Width && p.Y < angles.Height)
                {
                    numberOfPoints++;
                    if (angles[p.X, p.Y].IsAlignedUpToPrecision(rectangle.Angle, rectangle.Precision))
                    {
                        numberOfAlignedPoints++;
                    }
                }

                rectangleExplorationState.MoveToNextPixelToExplore();
            }

            return CalculateLog10ExpectedNumberOfFalseAlarmsInRectangle(numberOfPoints,
                numberOfAlignedPoints, rectangle.Precision, logNumberOfTests);
        }

        private double CalculateLog10ExpectedNumberOfFalseAlarmsInRectangle(
            int numberOfPixels, int numberOfAlignedPoints, double precision, double logNumberOfTests)
        {
            // Binomial parameters
            var n = numberOfPixels;
            var k = numberOfAlignedPoints;
            var p = precision;

            if (n < 0) throw new ArgumentOutOfRangeException(nameof(numberOfPixels), "Must be positive");
            if (k < 0) throw new ArgumentOutOfRangeException(nameof(numberOfAlignedPoints), "Must be positive");
            if (k > n) throw new ArgumentException("The number of aligned points cannot be greater than the total number of points/pixels");
            if (p <= 0.0 || p >= 1.1) throw new ArgumentOutOfRangeException(nameof(precision), "The precision should be in the range [0, 1]");


            if (n == 0 || k == 0) return -logNumberOfTests;
            if (n == k) return -logNumberOfTests - n * Math.Log10(p);

            var probabilityTerm = p / (1.0 - p);

            /*
             * compute the first term of the series
             * 
             * binomial_tail(n,k,p) = sum_{i=k}^n bincoef(n,i) * p^i * (1-p)^{n-i}
             * where bincoef(n,i) are the binomial coefficients.
             * But
             *   bincoef(n,k) = gamma(n+1) / ( gamma(k+1) * gamma(n-k+1) ).
             * We use this to compute the log of the first term.
             * 
             * Below k is just i in the formulae above.
             */
            var logBinomialCoefficient = MathHelpers.LogAbsoluteGamma(n + 1.0) - MathHelpers.LogAbsoluteGamma(k + 1.0)
                                         - MathHelpers.LogAbsoluteGamma(n - k + 1.0);

            var logFirstTerm = logBinomialCoefficient + k * Math.Log(p) + (n - k) * Math.Log(1.0 - p);
            var firstTerm = Math.Exp(logFirstTerm);

            if (firstTerm.IsRoughlyEqualTo(0.0))
            {
                if (k > n * p)
                {
                    return -logFirstTerm / MathHelpers.NaturalLog10 - logNumberOfTests;
                }
                return -logNumberOfTests;
            }

            var term = firstTerm;
            var binomialTail = firstTerm;
            for (var i = k + 1; i <= n; i++)
            {
                /*
                    As
                      term_i = bincoef(n,i) * p^i * (1-p)^(n-i)
                    and 
                      bincoef(n,i)/bincoef(n,i-1) = n-1+1 / i,
                    then,
                      term_i / term_i-1 = (n-i+1)/i * p/(1-p)
                    and
                      term_i = term_i-1 * (n-i+1)/i * p/(1-p).
                    p/(1-p) is computed only once and stored in 'p_term'.
                */
                var binomialTerm = (n - i + 1) * (1.0 / i);
                var multipliedTerm = binomialTerm * probabilityTerm;
                term *= multipliedTerm;
                binomialTail += term;

                if (!(binomialTerm < 1.0)) continue;

                /*
                     * When bin_term<1 then mult_term_j<mult_term_i for j>i
                     * (remaining terms will be smaller).
                     * 
                     * Then, the error on the binomial tail when truncated at
                     * the i term can be bounded by a geometric series of form
                     * term_i * sum mult_term_i^j.
                    */
                var maximumPossibleError =
                    term * ((1.0 - Math.Pow(multipliedTerm, n - i + 1)) / (1.0 - multipliedTerm) - 1.0);

                /* 
                     * One wants an error at most of tolerance*final_result, or:
                     * tolerance * abs(-log10(bin_tail)-logNT).
                     * Now, the error that can be accepted on bin_tail is
                     * given by tolerance*final_result divided by the derivative
                     * of -log10(x) when x=bin_tail. that is:
                     * tolerance * abs(-log10(bin_tail)-logNT) / (1/bin_tail)
                     * Finally, we truncate the tail if the error is less than:
                     * tolerance * abs(-log10(bin_tail)-logNT) * bin_tail
                     */
                if (maximumPossibleError < MaximumErrorAllowed *
                    Math.Abs(-Math.Log10(binomialTail) - logNumberOfTests) * binomialTail)
                {
                    break;
                }
            }
            return -Math.Log10(binomialTail) - logNumberOfTests;
        }
    }
}

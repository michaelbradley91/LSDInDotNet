using System;
using System.Collections;
using System.Collections.Generic;
using LSDInDotNet.Helpers;

namespace LSDInDotNet.Models
{
    public struct Rectangle<T> : IEnumerable<Point>
    {
        public DoublePoint FirstPoint;
        public DoublePoint SecondPoint;
        public double Width { get; set; }
        public DoublePoint Centre { get; set; }
        public double Angle { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double Precision { get; set; }
        public double ProbabilityOfPointWithAngleWithinPrecision { get; set; }
        public Image<double, T> Angles { get; set; }

        /// <summary>
        /// Improves the rectangle with a number of heuristics.
        /// Note that these may shift the rectangle off of its points,
        /// which could lead to some slight inaccuracy.It may be possible to improve this.
        /// </summary>
        public double Improve(double logNumberOfTests, double logEpsilon)
        {
            const double delta = 0.5;
            const double halfDelta = delta / 2.0;

            var logNfa = CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
            if (logNfa > logEpsilon) return logNfa;

            var testRectangle = new Rectangle<T>();

            // Try finer precisions
            CopyTo(testRectangle);
            for (var n = 0; n < 5; n++)
            {
                testRectangle.ProbabilityOfPointWithAngleWithinPrecision /= 2.0;
                testRectangle.Precision = testRectangle.ProbabilityOfPointWithAngleWithinPrecision * MathHelpers.Pi;

                var newLogNfa = testRectangle.CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
                if (newLogNfa > logNfa)
                {
                    logNfa = newLogNfa;
                    testRectangle.CopyTo(this);
                }
            }
            if (logNfa > logEpsilon) return logNfa;

            // Try to reduce width
            CopyTo(testRectangle);
            for (var n = 0; n < 5; n++)
            {
                if (!(testRectangle.Width - delta >= 0.5)) break;

                testRectangle.Width -= delta;

                var newLogNfa = testRectangle.CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
                if (newLogNfa > logNfa)
                {
                    logNfa = newLogNfa;
                    testRectangle.CopyTo(this);
                }
            }
            if (logNfa > logEpsilon) return logNfa;

            // Try to reduce one side of the rectangle
            CopyTo(testRectangle);
            for (var n = 0; n < 5; n++)
            {
                if (!(testRectangle.Width - delta >= 0.5)) break;

                testRectangle.FirstPoint.X += -testRectangle.DeltaY * halfDelta;
                testRectangle.FirstPoint.Y += testRectangle.DeltaX * halfDelta;
                testRectangle.SecondPoint.X += -testRectangle.DeltaY * halfDelta;
                testRectangle.SecondPoint.Y += testRectangle.DeltaX * halfDelta;
                testRectangle.Width -= delta;

                var newLogNfa = testRectangle.CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
                if (newLogNfa > logNfa)
                {
                    logNfa = newLogNfa;
                    testRectangle.CopyTo(this);
                }
            }
            if (logNfa > logEpsilon) return logNfa;

            // Try to reduce the other side of the rectangle
            CopyTo(testRectangle);
            for (var n = 0; n < 5; n++)
            {
                if (!(testRectangle.Width - delta >= 0.5)) break;
                
                testRectangle.FirstPoint.X -= -testRectangle.DeltaY * halfDelta;
                testRectangle.FirstPoint.Y -= testRectangle.DeltaX * halfDelta;
                testRectangle.SecondPoint.X -= -testRectangle.DeltaY * halfDelta;
                testRectangle.SecondPoint.Y -= testRectangle.DeltaX * halfDelta;
                testRectangle.Width -= delta;

                var newLogNfa = testRectangle.CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
                if (newLogNfa > logNfa)
                {
                    logNfa = newLogNfa;
                    testRectangle.CopyTo(this);
                }
            }
            if (logNfa > logEpsilon) return logNfa;

            // Try even finer precisions
            CopyTo(testRectangle);
            for (var n = 0; n < 5; n++)
            {
                testRectangle.ProbabilityOfPointWithAngleWithinPrecision /= 2.0;
                testRectangle.Precision = testRectangle.ProbabilityOfPointWithAngleWithinPrecision * MathHelpers.Pi;

                var newLogNfa = testRectangle.CalculateExpectedNumberOfFalseAlarmsLog10(logNumberOfTests);
                if (newLogNfa > logNfa)
                {
                    logNfa = newLogNfa;
                    testRectangle.CopyTo(this);
                }
            }

            return logNfa;
        }

        private const double MaximumErrorAllowed = 0.1; // an error of 10% is accepted

        public double CalculateExpectedNumberOfFalseAlarmsLog10(double logNumberOfTests)
        {
            var numberOfPoints = 0;
            var numberOfAlignedPoints = 0;

            var angles = Angles;

            foreach (var p in this)
            {
                if (p.X < 0 || p.Y < 0 || p.X >= angles.Width || p.Y >= angles.Height) continue;

                numberOfPoints++;
                if (angles[p].IsAlignedUpToPrecision(Angle, Precision))
                {
                    numberOfAlignedPoints++;
                }
            }

            return CalculateExpectedNumberOfFalseAlarmsLog10(numberOfPoints,
                numberOfAlignedPoints, Precision, logNumberOfTests);
        }

        private static double CalculateExpectedNumberOfFalseAlarmsLog10(
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

        public IEnumerator<Point> GetEnumerator()
        {
            return new RectangleExplorer<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Rectangle<T> rectangle)
        {
            rectangle.FirstPoint = FirstPoint;
            rectangle.SecondPoint = SecondPoint;
            rectangle.Width = Width;
            rectangle.Centre = Centre;
            rectangle.Angle = Angle;
            rectangle.DeltaX = DeltaX;
            rectangle.DeltaY = DeltaY;
            rectangle.Precision = Precision;
            rectangle.ProbabilityOfPointWithAngleWithinPrecision = ProbabilityOfPointWithAngleWithinPrecision;
            rectangle.Angles = Angles;
        }
    }
}

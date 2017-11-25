using System;
using System.Linq;

namespace LSDInDotNet.Extensions
{
    public static class DoubleExtensions
    {
        // DBL_MIN in C++
        public const double MinimumNormalisedDouble = 2.2250738585072014e-308;

        // DBL_EPSILON in C++
        public const double DifferenceBetweenOneAndMinimumDoubleGreaterThanOne = 2.2204460492503131e-016;

        public const double RelativeErrorFactor = 100;

        public static bool IsRoughlyEqualTo(this double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (left == right) return true;

            var absoluteDifference = Math.Abs(left - right);
            var absoluteMaximum = new[] {Math.Abs(left), Math.Abs(right), MinimumNormalisedDouble}.Max();

            return absoluteDifference / absoluteMaximum <=
                   RelativeErrorFactor * DifferenceBetweenOneAndMinimumDoubleGreaterThanOne;
        }
    }
}

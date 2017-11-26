using System;
using System.Linq;

namespace LSDInDotNet.Helpers
{
    public static class MathHelpers
    {
        // DBL_MIN in C++
        public const double MinimumNormalisedDouble = 2.2250738585072014e-308;

        // DBL_EPSILON in C++
        public const double DifferenceBetweenOneAndMinimumDoubleGreaterThanOne = 2.2204460492503131e-016;

        // M_PI
        public const double Pi = Math.PI;

        // M_2__PI
        public const double TwoPi = Pi * 2.0;

        // M_3_2_PI
        public const double ThreeOverTwoPi = Pi * 3.0 / 2.0;

        // M_LN10
        public static readonly double NaturalLog10 = Math.Log(10);

        // NOTDEF - unfortunately this value is passed into the calculation
        // later, and so cannot be trivially swapped for null (see level line generator).
        public const double NoAngle = -1024;

        public static bool IsRoughlyEqualTo(this double left, double right)
        {
            const double relativeErrorFactor = 100;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (left == right) return true;

            var absoluteDifference = Math.Abs(left - right);
            var absoluteMaximum = new[] { Math.Abs(left), Math.Abs(right), MinimumNormalisedDouble }.Max();

            return absoluteDifference / absoluteMaximum <=
                   relativeErrorFactor * DifferenceBetweenOneAndMinimumDoubleGreaterThanOne;
        }

        /// <summary>
        /// Returns the absolute difference between angles in the range [0, Pi]
        /// </summary>
        public static double AbsoluteAngleDifferenceTo(this double angle, double otherAngle)
        {
            return Math.Abs(angle.SignedAngleDifferenceTo(otherAngle));
        }

        /// <summary>
        /// Returns the signed difference between the angles in the range [-Pi, Pi)
        /// </summary>
        public static double SignedAngleDifferenceTo(this double angle, double otherAngle)
        {
            angle -= otherAngle;
            while (angle <= -Pi) angle += TwoPi;
            while (angle > Pi) angle -= TwoPi;
            return angle;
        }

        /// <summary>
        /// This assumes the angle and otherAngle are in the range [-Pi, Pi]
        /// </summary>
        public static bool IsAlignedUpToPrecision(this double angle, double otherAngle, double precision)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (angle == NoAngle) return false;

            otherAngle = Math.Abs(otherAngle - angle);
            if (otherAngle > ThreeOverTwoPi)
            {
                otherAngle = Math.Abs(otherAngle - TwoPi);
            }

            return otherAngle <= precision;
        }

        /*----------------------------------------------------------------------------*/
        /** Computes the natural logarithm of the absolute value of
            the gamma function of x. When x>15 use log_gamma_windschitl(),
            otherwise use log_gamma_lanczos().
         */
        public static double LogAbsoluteGamma(double x)
        {
            return x > 15.0
                ? LogAbsoluteGammaWithWindschitlApproximation(x)
                : LogAbsoluteGammaWithLanczosApproximation(x);
        }
        
        /** Computes the natural logarithm of the absolute value of
            the gamma function of x using the Lanczos approximation.
            See http://www.rskey.org/gamma.htm

            The formula used is
            @f[
              \Gamma(x) = \frac{ \sum_{n=0}^{N} q_n x^n }{ \Pi_{n=0}^{N} (x+n) }
                          (x+5.5)^{x+0.5} e^{-(x+5.5)}
            @f]
            so
            @f[
              \log\Gamma(x) = \log\left( \sum_{n=0}^{N} q_n x^n \right)
                              + (x+0.5) \log(x+5.5) - (x+5.5) - \sum_{n=0}^{N} \log(x+n)
            @f]
            and
              q0 = 75122.6331530,
              q1 = 80916.6278952,
              q2 = 36308.2951477,
              q3 = 8687.24529705,
              q4 = 1168.92649479,
              q5 = 83.8676043424,
              q6 = 2.50662827511.
        */
        public static double LogAbsoluteGammaWithLanczosApproximation(double x)
        {
            var a = (x + 0.5) * Math.Log(x + 5.5) - (x + 5.5);
            var b = 0.0;

            for (var n = 0; n < 7; n++)
            {
                a -= Math.Log(x + n);
                b += LanczosGammaApproximationCoefficients[n] * Math.Pow(x, n);
            }

            return a + Math.Log(b);
        }

        public static readonly double[] LanczosGammaApproximationCoefficients =
        {
            75122.6331530,
            80916.6278952,
            36308.2951477,
            8687.24529705,
            1168.92649479,
            83.8676043424,
            2.50662827511
        };

        /** Computes the natural logarithm of the absolute value of
            the gamma function of x using Windschitl method.
            See http://www.rskey.org/gamma.htm

            The formula used is
            @f[
                \Gamma(x) = \sqrt{\frac{2\pi}{x}} \left( \frac{x}{e}
                            \sqrt{ x\sinh(1/x) + \frac{1}{810x^6} } \right)^x
            @f]
            so
            @f[
                \log\Gamma(x) = 0.5\log(2\pi) + (x-0.5)\log(x) - x
                              + 0.5x\log\left( x\sinh(1/x) + \frac{1}{810x^6} \right).
            @f]
            This formula is a good approximation when x > 15.
        */
        public static double LogAbsoluteGammaWithWindschitlApproximation(double x)
        {
            return 0.918938533204673 + (x - 0.5) * Math.Log(x) - x
                   + 0.5 * x * Math.Log(x * Math.Sinh(1 / x) + 1 / (810.0 * Math.Pow(x, 6.0)));
        }
    }
}

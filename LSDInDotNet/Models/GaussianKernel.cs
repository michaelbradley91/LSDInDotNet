using System;
using System.Linq;

namespace LSDInDotNet.Models
{
    public static class GaussianKernel
    {
        public static TupleList CreateKernel(int dimension, double sigma = 1, double mean = 1)
        {
            var kernel = new TupleList(dimension)
            {
                Size = 1,
                Values = new double[dimension]
            };

            UpdateKernel(kernel, sigma, mean);
            return kernel;
        }

        public static void UpdateKernel(TupleList kernel, double sigma, double mean)
        {
            for (var i = 0; i < kernel.Dimension; i++)
            {
                var value = (i - mean) / sigma;
                kernel.Values[i] = Math.Exp(-0.5 * value * value);
            }

            var sum = kernel.Values.Sum();
            if (sum < 0) return;

            for (var i = 0; i < kernel.Dimension; i++)
            {
                kernel.Values[i] /= sum;
            }
        }
    }
}

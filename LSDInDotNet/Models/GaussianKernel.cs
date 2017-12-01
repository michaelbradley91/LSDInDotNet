using System;

namespace LSDInDotNet.Models
{
    public static class GaussianKernel
    {
        public static TupleList CreateKernel(int dimension, double sigma = 1, double mean = 1)
        {
            var kernel = new TupleList(dimension);
            kernel.AddTuple(new double[dimension]);
            UpdateKernel(kernel, sigma, mean);
            return kernel;
        }

        public static void UpdateKernel(TupleList kernel, double sigma, double mean)
        {
            double sum = 0;
            for (var i = 0; i < kernel.Dimension; i++)
            {
                var value = (i - mean) / sigma;
                kernel[i] = Math.Exp(-0.5 * value * value);
                sum += kernel[i];
            }

            if (sum < 0) return;

            for (var i = 0; i < kernel.Dimension; i++)
            {
                kernel[i] /= sum;
            }
        }
    }
}

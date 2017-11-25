using System;
using LSDInDotNet.Models;

namespace LSDInDotNet.Services
{
    public class ImageScaler
    {
        public Image<double> ApplyGuassianSampler(Image<double> image, double scale, double sigmaScale)
        {
            if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), "Must be positive");
            if (sigmaScale <= 0) throw new ArgumentOutOfRangeException(nameof(sigmaScale), "Must be positive");

            if (image.Width * scale > int.MaxValue ||
                image.Height * scale > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Cannot scale the image to such a large size");
            }

            var newWidth = (int)Math.Ceiling(image.Width * scale);
            var newHeight = (int)Math.Ceiling(image.Height * scale);

            var sigma = scale < 1 ? sigmaScale / scale : sigmaScale;

            const double precision = 3.0;
            var h = (int) Math.Ceiling(sigma * Math.Sqrt(2 * precision * Math.Log(10.0)));
            var kernelSize = 1 + 2 * h;
            var kernel = GaussianKernel.CreateKernel(kernelSize);

            var doubleImageWidth = 2 * image.Width;
            var doubleImageHeight = 2 * image.Height;

            var auxiliaryImage = new Image<double>(newWidth, image.Height);
            var scaledImage = new Image<double>(newWidth, newHeight);

            // First subsampling: x axis
            for (var x = 0; x < auxiliaryImage.Width; x++)
            {
                /*
                 x   is the coordinate in the new image.
                 xx  is the corresponding x-value in the original size image.
                 xc  is the integer value, the pixel coordinate of xx.
                 */
                var xx = x / scale;
                var xc = (int) Math.Floor(xx + 0.5);

                GaussianKernel.UpdateKernel(kernel, sigma, h + xx - xc);

                for (var y = 0; y < auxiliaryImage.Height; y++)
                {
                    var sum = 0.0;
                    for (var i = 0; i < kernel.Dimension; i++)
                    {
                        var j = xc - h + i;

                        // symmetry boundary condition
                        while (j < 0) j += doubleImageWidth;
                        while (j >= doubleImageWidth) j -= doubleImageWidth;
                        if (j >= image.Width) j = doubleImageWidth - 1 - j;

                        sum += image.Data[j + y * image.Width] * kernel.Values[i];
                    }
                    auxiliaryImage.Data[x + y * auxiliaryImage.Width] = sum;
                }
            }
            // Auxiliary image now has the scaling applied in the x direction
            // but not the y direction

            // Second subsampling: y axis
            for (int y = 0; y < scaledImage.Height; y++)
            {
                /*
                  y   is the coordinate in the new image.
                  yy  is the corresponding x-value in the original size image.
                  yc  is the integer value, the pixel coordinate of xx.
                */
                var yy = y / scale;
                var yc = (int) Math.Floor(yy + 0.5);

                GaussianKernel.UpdateKernel(kernel, sigma, h + yy - yc);

                for (var x = 0; x < scaledImage.Width; x++)
                {
                    var sum = 0.0;
                    for (var i = 0; i < kernel.Dimension; i++)
                    {
                        var j = yc - h + i;

                        /* symmetry boundary condition */
                        while (j < 0) j += doubleImageHeight;
                        while (j >= doubleImageHeight) j -= doubleImageHeight;
                        if (j >= image.Height ) j = doubleImageHeight - 1 - j;

                        sum += auxiliaryImage.Data[x + j * auxiliaryImage.Width] * kernel.Values[i];
                    }
                    scaledImage.Data[x + y * scaledImage.Width] = sum;
                }
            }

            return scaledImage;
        }
    }
}

using System;
using System.Linq;
using System.Text;
using LSDInDotNet.Models;

namespace LSDInDotNet.TestHelpers.Services
{
    public static class PgmService
    {
        /// <summary>
        /// Only currently supports the more unusual P2 format
        /// </summary>
        public static Image<double, int> Read(byte[] bytes)
        {
            var lines = Encoding.UTF8.GetString(bytes).Split('\n').Where(l => !l.Contains("#")).ToList();

            var format = lines[0].Trim();
            if (format != "P2") throw new ArgumentException("The image is not in the P2 format", nameof(bytes));

            var dimensions = lines[1].Split(null).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var width =  int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var maxValue = int.Parse(lines[2].Trim());

            var values = lines.Skip(3).SelectMany(l => l.Split(null))
                .Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse).ToList();

            var image = new Image<double, int>(width, height, maxValue);
            for (var i = 0; i < values.Count; i++)
            {
                var x = i % width;
                var y = i / width;
                image[x, y] = values[i];
            }
            return image;
        }
        
        /// <summary>
        /// Writes an image in the P2 format
        /// </summary>
        public static byte[] Write(Image<double, int> image)
        {
            const string format = "P2";
            var dimensions = $"{image.Width} {image.Height}";
            var maxValue = $"{image.Metadata}";
            var file = new StringBuilder(string.Join(Environment.NewLine, format, dimensions, maxValue));

            for (var y = 0; y < image.Height; y++)
            {
                file.Append(Environment.NewLine);
                for (var x = 0; x < image.Width; x++)
                {
                    file.Append((int)image[x, y]);
                    if (y != image.Height) file.Append(" ");
                }
            }

            return Encoding.UTF8.GetBytes(file.ToString());
        }
    }
}

using LSDInDotNet.Models;
using LSDInDotNet.Services;
using LSDInDotNet.TestHelpers.Services;
using LSDInDotNet.Tests.Properties;
using NUnit.Framework;
using System;
using System.IO;

namespace LSDInDotNet.Tests.Services
{
    [TestFixture]
    public class LineSegmentDetectorTests
    {
        private ILineSegmentDetector _lineSegmentDetector;

        [SetUp]
        public void SetUp()
        {
            _lineSegmentDetector = DependencyResolver.Resolve<ILineSegmentDetector>();
        }

        [Test]
        public void Resolve_CanResolveWrappedLineSegmentDetector()
        {
            var pgmImage = PgmService.Read(Resources.chairs);

            var result = _lineSegmentDetector.Run(pgmImage);
            var lineImage = result.Item2;
            
            // Any non-zero value in the image is a line. Convert this to black and white
            // by simply setting the maximum value for all lines.
            var outputImage = new Image<double, int>(lineImage.Width, lineImage.Height, 1);
            for (var x = 0; x < outputImage.Width; x++)
            {
                for (var y = 0; y < outputImage.Height; y++)
                {
                    outputImage[x, y] = lineImage[x, y] > 0 ? 1 : 0;
                }
            }

            var newPgmImage = PgmService.Write(outputImage);
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"LineSegmentDetectionTest-{Guid.NewGuid()}.pgm");
            File.WriteAllBytes(path, newPgmImage);
        }
    }
}

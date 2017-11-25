using System;
using System.Diagnostics;
using System.IO;
using LSDInDotNet.Services;
using LSDInDotNet.TestHelpers.Services;
using LSDInDotNet.Tests.Properties;
using NUnit.Framework;

namespace LSDInDotNet.Tests.Services
{
    [TestFixture]
    public class ImageScalerTests
    {
        private ImageScaler _imageScaler;
        private const double ScaleFactor = 0.8;
        private const double SigmaScaleFactor = 0.6;

        [SetUp]
        public void SetUp()
        {
            _imageScaler = DependencyResolver.Resolve<ImageScaler>();
        }

        [Test]
        public void ScaleWithGuassianSampler_CanScaleAnImageCorrectly()
        {
            var pgmImage = PgmService.Read(Resources.chairs);

            var scaledImage = _imageScaler.ScaleWithGuassianSampler(pgmImage, ScaleFactor, SigmaScaleFactor);

            var newPgmImage = PgmService.Write(scaledImage);
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"ImageScalerTest-{Guid.NewGuid()}.pgm");
            File.WriteAllBytes(path, newPgmImage);
        }

        [Test]
        public void LoadTest()
        {
            var pgmImage = PgmService.Read(Resources.chairs);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 0; i < 200; i++)
            {
                _imageScaler.ScaleWithGuassianSampler(pgmImage, ScaleFactor, SigmaScaleFactor);
            }
            stopWatch.Stop();
            var perSecond = 1000 / (stopWatch.ElapsedMilliseconds / 1000);
            Console.WriteLine($"Took {stopWatch.ElapsedMilliseconds}ms, running at {perSecond} scalings per second");
        }
    }
}

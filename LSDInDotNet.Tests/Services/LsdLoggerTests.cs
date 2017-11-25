using LSDInDotNet.Services;
using NUnit.Framework;

namespace LSDInDotNet.Tests.Services
{
    [TestFixture]
    public class LsdLoggerTests
    {
        private LsdLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new LsdLogger();
        }

        [Test]
        public void Error_DoesNotThrowAnException()
        {
            Assert.DoesNotThrow(() => _logger.Error("Hello world"));
        }
    }
}

using FluentAssertions;
using LSDInDotNet.Services;
using NUnit.Framework;

namespace LSDInDotNet.Tests.Services
{
    [TestFixture]
    public class DependencyResolverTests
    {
        [Test]
        public void Resolve_CanResolveWrappedLineSegmentDetector()
        {
            var lineSegmentDetector = DependencyResolver.Resolve<ILineSegmentDetector>();

            lineSegmentDetector.Should().NotBeNull();
            lineSegmentDetector.Should().BeOfType<LineSegmentDetectorWrapper>();
        }
    }
}

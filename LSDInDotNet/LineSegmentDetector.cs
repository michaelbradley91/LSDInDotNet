using LSDInDotNet.Services;

namespace LSDInDotNet
{
    public interface ILineSegmentDetector
    {
        void Run();
    }

    public class LineSegmentDetector : ILineSegmentDetector
    {
        public static ILineSegmentDetector Create()
        {
            return DependencyResolver.Resolve<ILineSegmentDetector>();
        }

        public LineSegmentDetector() { }

        public void Run()
        {
        }
    }
}

using LSDInDotNet.Services;

namespace LSDInDotNet
{
    public class LineSegmentDetectorWrapper : ILineSegmentDetector
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ILineSegmentDetector _lineSegmentDetectorWrapper;

        public LineSegmentDetectorWrapper(IErrorHandler errorHandler, ILineSegmentDetector lineSegmentDetectorWrapper)
        {
            _errorHandler = errorHandler;
            _lineSegmentDetectorWrapper = lineSegmentDetectorWrapper;
        }

        public void Run()
        {
            _errorHandler.Wrap(() =>
            {
                _lineSegmentDetectorWrapper.Run();
            });
        }
    }
}
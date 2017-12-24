using System;
using LSDInDotNet.Models;
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

        public Tuple<TupleList, Image<int, T>> Run<T>(Image<double, T> image,
            double scale = 0.8,
            double sigmaScale = 0.6,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024)
        {
            return _errorHandler.Wrap(() =>
            {
                return _lineSegmentDetectorWrapper.Run(image, scale, sigmaScale, quantizationErrorBound, angleThreshold,
                    detectionThreshold, densityThreshold, numberOfBins);
            });
        }

        public Tuple<TupleList, Image<int, T>> Run<T>(Image<double, T> scaledImage,
            double scale = 0.8,
            double quantizationErrorBound = 2.0,
            double angleThreshold = 22.5,
            double detectionThreshold = 0.0,
            double densityThreshold = 0.7,
            int numberOfBins = 1024)
        {
            return _errorHandler.Wrap(() =>
            {
                return _lineSegmentDetectorWrapper.Run(scaledImage, scale, quantizationErrorBound, angleThreshold,
                    detectionThreshold, densityThreshold, numberOfBins);
            });
        }
    }
}
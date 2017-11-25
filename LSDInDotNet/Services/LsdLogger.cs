using System;
using NLog;

namespace LSDInDotNet.Services
{
    public interface ILsdLogger
    {
        void Error(Exception exception);
        void Error(string error);
    }

    public class LsdLogger : ILsdLogger
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Error(string error)
        {
            _logger.Error(error);
        }

        public void Error(Exception exception)
        {
            _logger.Error(exception);
        }
    }
}

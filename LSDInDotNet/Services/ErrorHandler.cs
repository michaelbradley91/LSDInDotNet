using System;

namespace LSDInDotNet.Services
{
    public interface IErrorHandler
    {
        void Wrap(Action action);
        T Wrap<T>(Func<T> func);
    }

    public class ErrorHandler : IErrorHandler
    {
        private readonly ILsdLogger _logger;

        public ErrorHandler(ILsdLogger logger)
        {
            _logger = logger;
        }

        public void Wrap(Action action)
        {
            Wrap(() =>
            {
                action();
                return 0;
            });
        }

        public T Wrap<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }
    }
}

using System;

namespace LinkUs.Core.Commands
{
    public class ExecuteCommandTimeoutException : Exception
    {
        private readonly TimeSpan _timeout;

        public ExecuteCommandTimeoutException(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public override string Message => $"Timeout waiting for method after {_timeout}";
    }
}
using System;

namespace LinkUs.Core.Commands
{
    public class ErrorOccuredOnRemoteClientException : Exception
    {
        public string FullMessage { get; }

        public ErrorOccuredOnRemoteClientException(string message, string fullMessage) : base(message)
        {
            FullMessage = fullMessage;
        }
    }
}
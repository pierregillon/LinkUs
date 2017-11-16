using System;

namespace LinkUs.CommandLine
{
    public class CommandLineProcessingFailed : Exception
    {
        public CommandLineProcessingFailed(string message) : base(message) { }
    }
}
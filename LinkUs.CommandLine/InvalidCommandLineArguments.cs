using System;

namespace LinkUs.CommandLine
{
    public class InvalidCommandLineArguments : Exception
    {
        public InvalidCommandLineArguments(string message) : base(message) { }
    }
}
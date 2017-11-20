using System;

namespace LinkUs.CommandLine
{
    public class ArgumentParseException : Exception
    {
        public ArgumentParseException(string message) : base(message) { }
    }
}
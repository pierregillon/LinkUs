using System;

namespace LinkUs.Core.Modules.Exceptions
{
    public class UnknownCommandException : Exception
    {
        private readonly string _commandName;
        private readonly string _moduleName;

        public UnknownCommandException(string commandName, string moduleName)
        {
            _commandName = commandName;
            _moduleName = moduleName;
        }

        public override string Message => $"The command '{_commandName}' is unknown in module '{_moduleName}'.";
    }
}
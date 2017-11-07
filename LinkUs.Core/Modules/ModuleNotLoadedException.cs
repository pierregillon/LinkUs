using System;

namespace LinkUs.Core.Modules
{
    public class ModuleNotLoadedException : Exception
    {
        private readonly string _moduleName;

        public ModuleNotLoadedException(string moduleName)
        {
            _moduleName = moduleName;
        }

        public override string Message => $"The module '{_moduleName}' was not loaded.";
    }
}
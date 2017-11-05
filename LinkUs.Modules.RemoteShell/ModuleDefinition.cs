using System;

namespace LinkUs.Modules.RemoteShell
{
    public class ModuleDefinition
    {
        public Type[] GetHandlers()
        {
            return new[] {typeof(ShellCommandHandler)};
        }
    }
}

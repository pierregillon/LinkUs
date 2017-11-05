using System;

namespace LinkUs.Modules.RemoteShell
{
    public class Module
    {
        public Type[] GetHandlers()
        {
            return new[] {typeof(ShellCommandHandler)};
        }
    }
}

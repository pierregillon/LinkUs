using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core.Modules
{
    public class ModuleManager : IModule
    {
        private readonly List<IModule> _modules = new List<IModule>();

        public IEnumerable<Type> AvailableHandlers
        {
            get { return _modules.SelectMany(x => x.AvailableHandlers); }
        }

        public void Register(IModule module)
        {
            _modules.Add(module);
        }
    }
}
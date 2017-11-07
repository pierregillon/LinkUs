using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core.Modules
{
    public class ModuleManager
    {
        private readonly List<IModule> _modules = new List<IModule>();

        public IEnumerable<IModule> Modules => _modules;

        public void Register(IModule module)
        {
            _modules.Add(module);
        }
        public void Unregister(IModule module)
        {
            _modules.Remove(module);
        }
        public IModule FindModule(string moduleName)
        {
            var module = GetModule(moduleName);
            if (module == null) {
                throw new ModuleNotLoadedException(moduleName);
            }
            return module;
        }
        public IModule GetModule(string moduleName)
        {
            return _modules.SingleOrDefault(x => x.Name == moduleName);
        }
    }
}
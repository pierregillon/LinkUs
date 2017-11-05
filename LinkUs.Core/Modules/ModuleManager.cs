using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinkUs.Core.Modules
{
    public class ModuleManager
    {
        private readonly List<IModule> _modules = new List<IModule>();
        private const string MODULE_DIRECTORY = ".";

        public IEnumerable<Type> AvailableHandlers
        {
            get { return _modules.SelectMany(x => x.AvailableHandlers); }
        }

        public IEnumerable<IModule> Modules => _modules;

        public void Register(IModule module)
        {
            _modules.Add(module);
        }
        public void ScanAssemblies()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY, "LinkUs.Modules.*.dll")) {
                Console.Write($"* Loading module {Path.GetFileName(filePath)} \t ");
                try {
                    var module = new LoadableModule(filePath);
                    module.Load();
                    Register(module);
                    Console.WriteLine("[OK]");
                }
                catch (Exception ex) {
                    Console.WriteLine("[FAILED] " + ex.Message);
                }
            }
        }
        public Type FindCommand(string commandName)
        {
            return _modules
                .SelectMany(x => x.AvailableCommands)
                .SingleOrDefault(x => x.Name == commandName);
        }
        public Type FindCommandHandler(Type commandType)
        {
            return _modules
                .SelectMany(x => x.AvailableHandlers)
                .SingleOrDefault(v => v.GetMethods().Any(method => method.Name == "Handle" && method.GetParameters()[0].ParameterType == commandType));
        }
    }
}
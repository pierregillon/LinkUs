using LinkUs.Core.Modules.Commands;

namespace LinkUs.Core.Modules
{
    public class ModuleCommandHandler : IHandler<ListModules, ModuleInformationResponse>
    {
        private readonly ModuleManager _moduleManager;

        public ModuleCommandHandler(ModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public ModuleInformationResponse Handle(ListModules command)
        {
            var response = new ModuleInformationResponse();
            foreach (var module in _moduleManager.Modules) {
                response.ModuleInformations.Add(module.GetStatus());
            }
            return response;
        }
    }
}
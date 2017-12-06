using LinkUs.Client.Install;
using LinkUs.Core.Commands;

namespace LinkUs.Client.ClientInformation
{
    public class GetStatusCommandHandler : ICommandHandler<GetStatus, ClientStatus>
    {
        private readonly IInstaller _installer;
        private readonly IEnvironment _environment;
        private readonly IRegistry _registry;

        public GetStatusCommandHandler(IInstaller installer, IEnvironment environment, IRegistry registry)
        {
            _installer = installer;
            _environment = environment;
            _registry = registry;
        }

        public ClientStatus Handle(GetStatus command)
        {
            return new ClientStatus {
                ClientExeLocation = _environment.ApplicationPath,
                IsInstalled = _installer.IsInstalled(_environment.ApplicationPath),
                IsRegisteredAtStartup = _registry.IsRegisteredAtStartup(_environment.ApplicationPath)
            };
        }
    }
}
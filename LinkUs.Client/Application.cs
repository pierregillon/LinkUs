using LinkUs.Client.Install;

namespace LinkUs.Client
{
    public class Application
    {
        private readonly InstallationProcessSupervisor _installationProcessSupervisor;
        private readonly IEnvironment _environment;
        private readonly HostRequestServer _requestServer;

        public Application(
            InstallationProcessSupervisor installationProcessSupervisor,
            IEnvironment environment,
            HostRequestServer requestServer)
        {
            _installationProcessSupervisor = installationProcessSupervisor;
            _environment = environment;
            _requestServer = requestServer;
        }

        // ----- Public methods
        public void Run(bool debug)
        {
            if (debug) {
                _requestServer.Serve();
                return;
            }

            if (_installationProcessSupervisor.IsNewInstallationRequired()) {
                try {
                    _installationProcessSupervisor.SuperviseNewInstallation(_environment.ApplicationPath);
                }
                catch (InstallationFailed) {
                    _requestServer.Serve();
                }
            }
            else {
                _installationProcessSupervisor.AssureInstallationComplete();
                _requestServer.Serve();
            }
        }
    }
}
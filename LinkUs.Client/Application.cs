using LinkUs.Client.Install;

namespace LinkUs.Client
{
    public class Application
    {
        private readonly InstallationProcessSupervisor _installationProcessSupervisor;
        private readonly HostRequestServer _requestServer;

        public Application(
            InstallationProcessSupervisor installationProcessSupervisor,
            HostRequestServer requestServer)
        {
            _installationProcessSupervisor = installationProcessSupervisor;
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
                    _installationProcessSupervisor.SuperviseNewInstallation();
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
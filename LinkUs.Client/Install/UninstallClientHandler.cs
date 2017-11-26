using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Client.Modules;
using LinkUs.Core.Commands;

namespace LinkUs.Client.Install
{
    public class UninstallClientHandler : ICommandHandler<UninstallClient, bool>
    {
        private readonly Installer _installer;
        private readonly IEnvironment _environment;
        private readonly ExternalAssemblyModuleLocator _moduleLocator;

        public UninstallClientHandler(
            Installer installer,
            IEnvironment environment,
            ExternalAssemblyModuleLocator moduleLocator)
        {
            _installer = installer;
            _environment = environment;
            _moduleLocator = moduleLocator;
        }

        public bool Handle(UninstallClient command)
        {
            _installer.Uninstall();

            DeleteModules();
            StartDeleteProcess();

            Task.Run(() => {
                Environment.Exit(0);
            });

            return true;
        }

        // ----- Internal logic
        private void DeleteModules()
        {
            try {
                Directory.Delete(_moduleLocator.GetModulesLocation(), true);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
        private void StartDeleteProcess()
        {
            var deleteProcess = new Process {
                StartInfo = {
                    FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                    Arguments = $"/C del \"{_environment.ApplicationPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false
                },
                EnableRaisingEvents = true
            };

            deleteProcess.Start();
        }
    }
}
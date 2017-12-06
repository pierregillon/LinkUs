using CommandLine;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine
{
    public class Options
    {
        // ----- Modules

        [VerbOption("list-modules", HelpText = "Display the list of the module installed on a client.")]
        public ListModulesCommandLine ListModules { get; set; }

        [VerbOption("install-module", HelpText = "Install a specific module to a remote client.")]
        public InstallModuleCommandLine InstallModule { get; set; }

        [VerbOption("uninstall-module", HelpText = "Uninstall a specific module to a remote client.")]
        public UninstallModuleCommandLine UninstallModule { get; set; }

        // ----- Others
        [VerbOption("ping", HelpText = "Send a ping request to a client.")]
        public PingCommandLine Ping { get; set; }

        [VerbOption("list-clients", HelpText = "Display the list of connected clients.")]
        public ListConnectedClientsCommandLine ListConnectedClient { get; set; }

        [VerbOption("shell", HelpText = "Start a remote shell on client.")]
        public ShellCommandLine Shell { get; set; }

        [VerbOption("upload", HelpText = "Upload a file to a remote client.")]
        public UploadFileCommandLine UploadFile { get; set; }

        [VerbOption("download", HelpText = "Download a file from remote client.")]
        public DownloadFileCommandLine DownloadFile { get; set; }

        [VerbOption("config", HelpText = "Configure options for the lkus command line.")]
        public ConfigCommandLine Config { get; set; }

        [VerbOption("uninstall", HelpText = "Completely uninstall a remote client.")]
        public UninstallCommandLine UninstallCommandLine { get; set; }

        [VerbOption("status", HelpText = "Get the remote client status.")]
        public StatusCommandLine StatusCommandLine { get; set; }
    }
}
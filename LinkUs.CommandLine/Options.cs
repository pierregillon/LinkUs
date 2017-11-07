using CommandLine;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    public class Options
    {
        [VerbOption("ping", HelpText = "Send a ping request to a client.")]
        public PingCommandLine Ping { get; set; }

        [VerbOption("load-module", HelpText = "Load a specific module of a client.")]
        public LoadModuleCommandLine LoadModule { get; set; }

        [VerbOption("unload-module", HelpText = "Unload a specific module of a client.")]
        public UnloadModuleCommandLine UnloadModule { get; set; }

        [VerbOption("list-modules", HelpText = "Display the list of the module installed on a client.")]
        public ListModulesCommandLine ListModules { get; set; }

        [VerbOption("list-clients", HelpText = "Display the list of connected clients.")]
        public ListRemoteClients ListRemoteClients { get; set; }

        [VerbOption("shell", HelpText = "Start a remote shell on client.")]
        public ShellCommandLine Shell { get; set; }
    }
}
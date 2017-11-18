using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class ConfigCommandLine
    {
        [Option('s', "server", Required = false, HelpText = "Define the server to connect.")]
        public string Server { get; set; }

        [Option('p', "port", Required = false, HelpText = "Define the port to connect.")]
        public int? Port { get; set; }

        public bool Any()
        {
            return string.IsNullOrEmpty(Server) == false || Port.HasValue;
        }
    }
}

using System.Collections.Generic;

namespace LinkUs.Core
{
    public class ExecuteShellCommand : Command
    {
        public ExecuteShellCommand() : base("ExecuteShellCommand") { }

        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }

    public class ShellStartedResponse : Command
    {
        public ShellStartedResponse() : base("ShellStartedResponse") { }
        public ShellStartedResponse(double processId)
        {
            ProcessId = processId;
        }
        public double ProcessId { get; set; }
    }

    public class ShellOuputReceivedResponse : Command
    {
        public ShellOuputReceivedResponse() : base("ShellOuputReceivedResponse") { }
        public ShellOuputReceivedResponse(string output, double processId) : this()
        {
            Output = output;
            ProcessId = processId;
        }

        public string Output { get; set; }
        public double ProcessId { get; set; }
    }

    public class SendInputToShellCommand : Command
    {
        public SendInputToShellCommand() : base("SendInputToShellCommand") { }
        public SendInputToShellCommand(string input, double processId) : this()
        {
            Input = input;
            ProcessId = processId;
        }

        public string Input { get; set; }
        public double ProcessId { get; set; }
    }

    public class ShellEndedResponse : Command
    {
        public ShellEndedResponse() : base("ShellEndedResponse") { }
        public ShellEndedResponse(double exitCode, double processId) : this()
        {
            ExitCode = exitCode;
            ProcessId = processId;
        }
        public double ExitCode { get; set; }
        public double ProcessId { get; set; }
    }

    public class KillShellCommand : Command
    {
        public KillShellCommand() : base("KillShellCommand") { }
        public KillShellCommand(double processId) : this()
        {
            ProcessId = processId;
        }
        public double ProcessId { get; set; }
    }
}
using System;
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
        public ShellStartedResponse() : base("ShellStartedResponse")
        {
            
        }
        public double ProcessId { get; set; }
    }

    public class ShellOuputReceivedResponse : Command
    {
        public ShellOuputReceivedResponse() : base("ShellOuputReceivedResponse") { }
        public ShellOuputReceivedResponse(string output) : this()
        {
            Output = output;
        }

        public string Output { get; set; }
    }

    public class SendInputToShellCommand : Command
    {
        public SendInputToShellCommand() : base("SendInputToShellCommand") { }
        public SendInputToShellCommand(string input) :this()
        {
            Input = input;
        }

        public string Input { get; set; }
    }

    public class ShellEndedResponse : Command
    {
        public ShellEndedResponse() : base("ShellEndedResponse") { }
        public ShellEndedResponse(int exitCode) : this()
        {
            ExitCode = exitCode;
        }
        public int ExitCode { get; set; }
    }

    public class KillShellCommand : Command
    {
        public KillShellCommand() : base("KillShellCommand")
        {
            
        }
    }
}
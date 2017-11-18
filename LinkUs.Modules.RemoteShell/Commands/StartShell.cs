using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Modules.RemoteShell.Commands
{
    public class StartShell 
    {
        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }

        public static StartShell Parse(string input)
        {
            var arguments = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commandLine = arguments.First();
            return new StartShell
            {
                CommandLine = commandLine,
                Arguments = arguments.Skip(1).OfType<object>().ToList()
            };
        }
    }
}
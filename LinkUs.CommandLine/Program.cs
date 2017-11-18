using System;
using System.Linq;
using CommandLine;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Handlers;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.ClientInformation;
using StructureMap;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] arguments)
        {
            var container = BuildContainer();

            try {
                var commandReader = container.GetInstance<ConsoleCommandReader>();
                if (arguments != null && arguments.Any()) {
                    commandReader.ExecuteSingleCommand(arguments);
                }
                else {
                    commandReader.ExecuteMultipleCommands();
                }
            }
            finally {
                //connection.Close();
            }
        }


        private static Container BuildContainer()
        {
            return new Container(configuration => {
                configuration.For<IConnection>().Singleton();
                configuration.For<PackageTransmitter>();
                configuration.For<ICommandSender>().Use<CommandSender>();
                configuration.For<ICommandLineProcessor>().Use<CommandLineProcessor>();
                configuration.For<IConsole>().Use<WindowsConsole>();
                configuration.For<ICommandSerializer>().Use<JsonCommandSerializer>();
                configuration.For<Parser>().Use(Parser.Default);
                configuration.For<GlobalParameters>().Singleton();
                configuration
                    .For<SocketAsyncOperationPool>()
                    .Use(new SocketAsyncOperationPool(10))
                    .Singleton();

                configuration.Scan(y => {
                    y.TheCallingAssembly();
                    y.AddAllTypesOf(typeof(ICommandLineHandler<>));
                    y.ConnectImplementationsToTypesClosing(typeof(ICommandLineHandler<>));
                    y.WithDefaultConventions();
                });
            });
        }
    }
}
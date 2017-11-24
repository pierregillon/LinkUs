using System;
using System.IO;
using System.Linq;
using LinkUs.CommandLine.ModuleIntegration;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class CommandLineModuleLocatorShould
    {
        private readonly ModuleLocator _moduleLocator;

        public CommandLineModuleLocatorShould()
        {
            _moduleLocator = new ModuleLocator("Resources\\Modules\\");
        }

        [Fact]
        public void default_module_is_not_available()
        {
            // Arranges
            var defaultModuleFileName = "LinkUs.Modules.Default";

            // Acts
            var availableModules = _moduleLocator.GetAvailableModules();

            // Asserts
            Check.That(availableModules.Select(x => x.Name)).Not.Contains(defaultModuleFileName);
            Check.That(File.Exists($"Resources\\Modules\\{defaultModuleFileName}.dll")).IsTrue();
        }

        [Fact]
        public void modules_with_correct_name_convention_are_available()
        {
            // Arranges
            var validModule = "LinkUs.Modules.RemoteShell";

            // Acts
            var availableModules = _moduleLocator.GetAvailableModules();

            // Asserts
            Check.That(availableModules.Select(x => x.Name)).Contains(validModule);
            Check.That(File.Exists($"Resources\\Modules\\{validModule}.dll")).IsTrue();
        }

        [Fact]
        public void available_module_contains_assembly_description_and_location()
        {
            // Arranges
            var validModule = "LinkUs.Modules.RemoteShell";

            // Acts
            var availableModules = _moduleLocator.GetAvailableModules();

            // Asserts
            var module = availableModules.Single(x => x.Name == validModule);
            Check.That(module)
                 .HasFieldsWithSameValues(new {
                     Name = "LinkUs.Modules.RemoteShell",
                     Description = "RemoteShell module allows to start a remote shell session with a client.",
                     FileLocation = $"Resources\\Modules\\{validModule}.dll"
                 });
        }

        [Fact]
        public void get_available_modules_multiple_times()
        {
            // Acts
            Action action = () => {
                for (int i = 0; i < 10; i++) {
                    _moduleLocator.GetAvailableModules().ToArray();
                }
            };

            // Asserts
            Check.ThatCode(action).DoesNotThrow();
        }
    }
}
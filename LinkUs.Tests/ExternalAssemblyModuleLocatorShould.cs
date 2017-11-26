using System.IO;
using LinkUs.Client.Modules;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class ExternalAssemblyModuleLocatorShould
    {
        private readonly ExternalAssemblyModuleLocator _moduleLocator;

        public ExternalAssemblyModuleLocatorShould()
        {
            _moduleLocator = new ExternalAssemblyModuleLocator();
        }

        [Fact]
        public void not_display_invalid_module()
        {
            try {
                // Arranges
                File.Copy("Resources\\some_file.txt", "ef3cA\\some_file.txt");

                // Acts
                var modules = _moduleLocator.GetModuleInformations();

                // Arranges
                Check.That(modules).HasSize(0);
            }
            finally {
                File.Delete("ef3cA\\some_file.txt");
            }
        }
    }
}

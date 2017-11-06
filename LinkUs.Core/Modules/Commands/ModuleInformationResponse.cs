using System.Collections.Generic;

namespace LinkUs.Core.Modules.Commands
{
    public class ModuleInformationResponse
    {
        public List<ModuleInformation> ModuleInformations { get; set; } = new List<ModuleInformation>();
    }
}
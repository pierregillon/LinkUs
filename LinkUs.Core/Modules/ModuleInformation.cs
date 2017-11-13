namespace LinkUs.Core.Modules
{
    public class ModuleInformation
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsLoaded { get; set; }

        public override string ToString()
        {
            return $"{Name}\t{Version}\t{IsLoaded}";
        }
    }
}
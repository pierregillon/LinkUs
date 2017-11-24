namespace LinkUs.Modules.Default.Modules
{
    public interface IModuleFactory<T> where T : IModule
    {
        T Build(string fileLocation);
    }
}
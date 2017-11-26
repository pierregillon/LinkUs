namespace LinkUs.Client.Modules
{
    public interface IModuleFactory<T> where T : IModule
    {
        T Build(string fileLocation);
    }
}
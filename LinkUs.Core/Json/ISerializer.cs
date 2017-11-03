namespace LinkUs.Core.Json
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] bytes);
        byte[] Serialize<T>(T command);
    }
}
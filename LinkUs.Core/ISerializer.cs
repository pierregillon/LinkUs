namespace LinkUs.Core
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] bytes);
        byte[] Serialize<T>(T command);
    }
}
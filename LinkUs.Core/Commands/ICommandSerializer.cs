using System;

namespace LinkUs.Core.Commands
{
    public interface ICommandSerializer
    {
        T Deserialize<T>(byte[] bytes);
        byte[] Serialize<T>(T command);
        object Deserialize(byte[] content, Type type);
        bool IsPrimitifMessage(byte[] content);
    }
}
using System;
using System.Linq;
using System.Text;

namespace LinkUs.Core.Json
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T command)
        {
            var json = SimpleJson.SerializeObject(command);
            if (json.First() == '{') {
                var commandName = $"\"CommandName\":\"{command.GetType().Name}\"";
                var assemblyName = $"\"ModuleName\":\"{command.GetType().Assembly.GetName().Name}\"";
                json = json.Insert(1, commandName + "," + assemblyName + ",");
            }
            return Encoding.UTF8.GetBytes(json);
        }
        public object Deserialize(byte[] content, Type type)
        {
            var json = Encoding.UTF8.GetString(content);
            return SimpleJson.DeserializeObject(json, type);
        }
        public T Deserialize<T>(byte[] result)
        {
            return (T) Deserialize(result, typeof(T));
        }
    }
}
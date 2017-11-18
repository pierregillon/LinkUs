using System;
using System.Linq;
using System.Text;
using LinkUs.Core.Json;

namespace LinkUs.Core.Commands
{
    public class JsonCommandSerializer : ICommandSerializer
    {
        public byte[] Serialize<T>(T command)
        {
            var json = SimpleJson.SerializeObject(command);
            if (json.First() == '{') {
                var commandName = $"\"CommandName\":\"{command.GetType().Name}\"";
                var assemblyName = $"\"AssemblyName\":\"{command.GetType().Assembly.GetName().Name}\"";
                json = json.Insert(1, commandName + "," + assemblyName + ",");
            }
            return Encoding.UTF8.GetBytes(json);
        }
        public object Deserialize(byte[] content, Type type)
        {
            var json = Encoding.UTF8.GetString(content);
            if (json.StartsWith("{") == false) {
                return SimpleJson.DeserializeObject(json, type);
            }
            else {
                var info = SimpleJson.DeserializeObject<CommandDescriptor>(json);
                if (info.CommandName == typeof(ErrorMessage).Name) {
                    var errorMessage = SimpleJson.DeserializeObject<ErrorMessage>(json);
                    throw new ErrorOccuredOnRemoteClientException(errorMessage.Message, errorMessage.FullError);
                }
                return SimpleJson.DeserializeObject(json, type);
            }
        }
        public bool IsPrimitifMessage(byte[] content)
        {
            var json = Encoding.UTF8.GetString(content);
            return json.First() != '{';
        }
        public T Deserialize<T>(byte[] result)
        {
            return (T) Deserialize(result, typeof(T));
        }
    }
}
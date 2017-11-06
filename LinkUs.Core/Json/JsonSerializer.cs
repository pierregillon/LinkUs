using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkUs.Core.Json
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T command)
        {
            var json = Json.Stringify(command);
            if (json.First() == '{') {
                var commandName = $"\"Name\":\"{command.GetType().Name}\"";
                var assemblyName = $"\"AssemblyName\":\"{command.GetType().Assembly.GetName().Name}\"";
                json = json.Insert(1, commandName + "," + assemblyName + ",");
            }
            return Encoding.UTF8.GetBytes(json);
        }
        public object Deserialize(byte[] content, Type type)
        {
            var json = Encoding.UTF8.GetString(content);
            var obj = Json.Parse(json);
            var isPrimitiveType = type.IsPrimitive || type.IsValueType || (type == typeof(string));
            if (isPrimitiveType) {
                return obj;
            }
            var properties = (IDictionary<string, object>) obj;
            var instance = Activator.CreateInstance(type);
            foreach (var propertyInfo in type.GetProperties().Where(x => x.CanRead && x.CanWrite)) {
                object value;
                if (properties.TryGetValue(propertyInfo.Name, out value)) {
                    propertyInfo.SetValue(instance, value);
                }
            }
            return instance;
        }
        public T Deserialize<T>(byte[] result)
        {
            return (T) Deserialize(result, typeof(T));
        }
    }
}
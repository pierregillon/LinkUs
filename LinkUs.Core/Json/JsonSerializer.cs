using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinkUs.Core.Json
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T command)
        {
            var json = Core.Json.Json.Stringify(command);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }
        public T Deserialize<T>(byte[] result)
        {
            var json = Encoding.UTF8.GetString(result);
            var obj = Core.Json.Json.Parse(json);
            var type = typeof(T);
            var isPrimitiveType = type.IsPrimitive || type.IsValueType || (type == typeof(string));
            if (isPrimitiveType) {
                return (T) obj;
            }
            var properties = (IDictionary<string, object>) obj;
            var instance = Activator.CreateInstance<T>();
            foreach (var propertyInfo in typeof(T).GetProperties().Where(x=>x.CanRead && x.CanWrite)) {
                object value;
                if (properties.TryGetValue(propertyInfo.Name, out value)) {
                    propertyInfo.SetValue(instance, value);
                }
            }
            return instance;
        }
    }
}
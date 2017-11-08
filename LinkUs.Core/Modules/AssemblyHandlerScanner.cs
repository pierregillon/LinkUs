using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinkUs.Core.Modules
{
    public class AssemblyHandlerScanner
    {
        public IDictionary<string, MaterializationInfo> Scan(Assembly assembly)
        {
            var materializationInfos = new Dictionary<string, MaterializationInfo>();
            var handlers = GetHandlerTypes(assembly);
            foreach (var handlerType in handlers) {
                var handleMethods = handlerType
                    .GetMethods()
                    .Where(x => x.Name == "Handle")
                    .Where(x=> x.ReturnType != typeof(void));

                foreach (var methodInfo in handleMethods) {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == 0) {
                        continue;
                    }
                    var parameterType = parameters[0].ParameterType;
                    materializationInfos.Add(parameterType.Name, new MaterializationInfo(parameterType, handlerType, methodInfo));
                }
            }
            return materializationInfos;
        }

        private static IEnumerable<Type> GetHandlerTypes(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(x => x.Name.EndsWith("Handler"))
                .Where(x => x.GetMethods().Any(method => method.Name == "Handle"))
                .ToArray();
        }
    }
}
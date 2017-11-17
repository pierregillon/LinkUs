using System;
using System.Reflection;

namespace LinkUs.Modules.Default.Modules
{
    public class MaterializationInfo
    {
        public Type CommandType { get; }
        public Type HandlerType { get;  }
        public MethodInfo HandleMethod { get; }

        public MaterializationInfo(Type commandType, Type handlerType, MethodInfo handleMethod)
        {
            CommandType = commandType;
            HandlerType = handlerType;
            HandleMethod = handleMethod;
        }
    }
}
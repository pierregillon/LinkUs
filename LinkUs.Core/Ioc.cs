using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core
{
    public class Ioc
    {
        // ----- Fields
        private readonly Dictionary<Type, Type> _registration = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        // ----- Public methods
        public void Register<TContract, TImplementation>()
            where TContract : class
            where TImplementation : class, TContract
        {
            _registration.Add(typeof(TContract), typeof(TImplementation));
        }
        public void Register<TImplementation>()
            where TImplementation : class
        {
            _registration.Add(typeof(TImplementation), typeof(TImplementation));
        }
        public void RegisterSingle<TContract, TImplementation>()
            where TContract : class
            where TImplementation : class, TContract
        {
            _singletons.Add(typeof(TContract), null);
            Register<TContract, TImplementation>();
        }
        public void RegisterSingle<TContract>(TContract instance)
            where TContract : class
        {
            if (_singletons.ContainsKey(typeof(TContract))) {
                _singletons[typeof(TContract)] = instance;
            }
            else {
                _singletons.Add(typeof(TContract), instance);
            }
        }
        public void RegisterSingle<TImplementation>()
        {
            _singletons.Add(typeof(TImplementation), null);
            _registration.Add(typeof(TImplementation), typeof(TImplementation));
        }

        public TContract GetInstance<TContract>()
        {
            return (TContract) GetInstance(typeof(TContract));
        }
        public object GetInstance(Type type)
        {
            if (_singletons.ContainsKey(type)) {
                if (_singletons[type] == null) {
                    _singletons[type] = CreateNewInstance(_registration[type]);
                }
                return _singletons[type];
            }
            else if (_registration.ContainsKey(type)) {
                return CreateNewInstance(_registration[type]);
            }
            else {
                throw new Exception($"The type '{type.Name}' is unknown.");
            }
        }
        public void UnregisterSingle<TContract>()
        {
            _singletons.Remove(typeof(TContract));
            _registration.Remove(typeof(TContract));
        }

        // ----- Utils
        private object CreateNewInstance(Type type)
        {
            var constructor = type.GetConstructors().Single();
            var parameters = constructor.GetParameters().Select(x => GetInstance(x.ParameterType)).ToArray();
            return constructor.Invoke(parameters);
        }
    }
}
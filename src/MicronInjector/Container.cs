using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicronInjector
{
    public class Container
    {
        private readonly IDictionary<Type, IDictionary<Type, object>> instances;
        public Container()
        {
            instances = new Dictionary<Type, IDictionary<Type, object>>();
        }
        public void Register<TInterface, TConcrete>()
        {
            instances.Add(typeof(TInterface), new Dictionary<Type, object> { { typeof(TConcrete), null } });
        }
        private object GetInstance(Type type)
        {
            if (instances.ContainsKey(type))
            {
                var interfaceType = instances[type].First();
                if (interfaceType.Value == null)
                {
                    var constructors = interfaceType.Key.GetConstructors();
                    if (constructors != null && constructors.Any())
                    {
                        var publicConstructor = constructors.FirstOrDefault(x => x.IsPublic);
                        if (publicConstructor != null)
                        {
                            var parameters = new List<object>();
                            foreach (var parameter in publicConstructor.GetParameters())
                                parameters.Add(GetInstance(parameter.ParameterType));

                            if (parameters.Any())
                                return instances[type][interfaceType.Key] = Activator.CreateInstance(interfaceType.Key, parameters.ToArray());
                        }
                    }

                    return instances[type][interfaceType.Key] = Activator.CreateInstance(interfaceType.Key);
                }
                return interfaceType.Value;
            }
            return null;
        }
        public TConcrete GetInstance<TConcrete>() where TConcrete : class => GetInstance(typeof(TConcrete)) as TConcrete;
    }
}

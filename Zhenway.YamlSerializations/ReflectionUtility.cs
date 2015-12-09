using System;
using System.Collections.Generic;

namespace Zhenway.YamlSerializations
{
    internal static class ReflectionUtility
	{
		public static Type GetImplementedGenericInterface(Type type, Type genericInterfaceType)
		{
			foreach (var interfacetype in GetImplementedInterfaces(type))
			{
				if (interfacetype.IsGenericType() && interfacetype.GetGenericTypeDefinition() == genericInterfaceType)
				{
					return interfacetype;
				}
			}
			return null;
		}

		public static IEnumerable<Type> GetImplementedInterfaces(Type type)
		{
			if (type.IsInterface())
			{
				yield return type;
			}

			foreach (var implementedInterface in type.GetInterfaces())
			{
				yield return implementedInterface;
			}
		}
	}
}

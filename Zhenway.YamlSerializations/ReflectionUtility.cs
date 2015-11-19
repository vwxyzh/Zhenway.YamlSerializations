using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

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

        public static MethodInfo GetMethod(Expression<Action> methodAccess)
		{
			var method = ((MethodCallExpression)methodAccess.Body).Method;
			if (method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition();
			}
			return method;
		}

		public static MethodInfo GetMethod<T>(Expression<Action<T>> methodAccess)
		{
			var method = ((MethodCallExpression)methodAccess.Body).Method;
			if (method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition();
			}
			return method;
		}
	}

	public sealed class GenericStaticMethod
	{
		private readonly MethodInfo methodToCall;

		public GenericStaticMethod(Expression<Action> methodCall)
		{
			var callExpression = (MethodCallExpression)methodCall.Body;
			methodToCall = callExpression.Method.GetGenericMethodDefinition();
		}

		public object Invoke(Type[] genericArguments, params object[] arguments)
		{
			try
			{
				return methodToCall
					.MakeGenericMethod(genericArguments)
					.Invoke(null, arguments);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.Unwrap();
			}
		}
	}

	public sealed class GenericInstanceMethod<TInstance>
	{
		private readonly MethodInfo methodToCall;

		public GenericInstanceMethod(Expression<Action<TInstance>> methodCall)
		{
			var callExpression = (MethodCallExpression)methodCall.Body;
			methodToCall = callExpression.Method.GetGenericMethodDefinition();
		}

		public object Invoke(Type[] genericArguments, TInstance instance, params  object[] arguments)
		{
			try
			{
				return methodToCall
					.MakeGenericMethod(genericArguments)
					.Invoke(instance, arguments);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.Unwrap();
			}
		}
	}
}

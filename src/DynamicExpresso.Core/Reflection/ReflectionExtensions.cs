﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso.Reflection
{
	internal static class ReflectionExtensions
	{
		public static DelegateInfo GetDelegateInfo(Type delegateType, params string[] parametersNames)
		{
#if NET_COREAPP
			MethodInfo method = delegateType.GetTypeInfo().GetMethod("Invoke");
#else
			MethodInfo method = delegateType.GetMethod("Invoke");
#endif
            if (method == null)
				throw new ArgumentException("The specified type is not a delegate");

			var delegateParameters = method.GetParameters();
			var parameters = new Parameter[delegateParameters.Length];

			bool useCustomNames = parametersNames != null && parametersNames.Length > 0;

			if (useCustomNames && parametersNames.Length != parameters.Length)
				throw new ArgumentException(string.Format("Provided parameters names doesn't match delegate parameters, {0} parameters expected.", parameters.Length));

			for (int i = 0; i < parameters.Length; i++)
			{
				var paramName = useCustomNames ? parametersNames[i] : delegateParameters[i].Name;
				var paramType = delegateParameters[i].ParameterType;

				parameters[i] = new Parameter(paramName, paramType);
			}

			return new DelegateInfo(method.ReturnType, parameters);
		}

		public static IEnumerable<MethodInfo> GetExtensionMethods(Type type)
		{
            var typeInfo = type;
#if NET_COREAPP
            if (type.GetTypeInfo().IsSealed && type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().IsNested)
#else
            if (typeInfo.IsSealed && typeInfo.IsAbstract && !typeInfo.IsGenericType && !typeInfo.IsNested)
#endif
            {
#if NET_COREAPP
                var query = from method in type.GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
#else
                var query = from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
#endif
                            where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
										select method;
				return query;
			}

			return Enumerable.Empty<MethodInfo>();
		}

		public class DelegateInfo
		{
			public DelegateInfo(Type returnType, Parameter[] parameters)
			{
				ReturnType = returnType;
				Parameters = parameters;
			}

			public Type ReturnType { get; private set; }
			public Parameter[] Parameters { get; private set; }
		}
	}
}

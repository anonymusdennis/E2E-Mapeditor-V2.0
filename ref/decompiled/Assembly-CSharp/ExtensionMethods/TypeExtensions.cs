using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtensionMethods;

public static class TypeExtensions
{
	public static Type GetEnumeratedType(this Type type)
	{
		Type elementType = type.GetElementType();
		if (elementType != null)
		{
			return elementType;
		}
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments.Length > 0)
		{
			return genericArguments[0];
		}
		return null;
	}

	public static bool IsIEnumerable(this Type type)
	{
		return type.GetInterfaces().Any((Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
	}

	public static bool IsDefault<T>(this T t)
	{
		return EqualityComparer<T>.Default.Equals(t, default(T));
	}
}

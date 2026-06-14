using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slate;

public static class ReflectionTools
{
	private const BindingFlags flagsEverything = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static List<Assembly> _loadedAssemblies;

	private static Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

	private static List<Assembly> loadedAssemblies
	{
		get
		{
			if (_loadedAssemblies == null)
			{
				_loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
			}
			return _loadedAssemblies;
		}
	}

	public static Type GetType(string typeName)
	{
		Type value = null;
		if (typeMap.TryGetValue(typeName, out value))
		{
			return value;
		}
		value = Type.GetType(typeName);
		if (value != null)
		{
			Type type = value;
			typeMap[typeName] = type;
			return type;
		}
		foreach (Assembly loadedAssembly in loadedAssemblies)
		{
			try
			{
				value = loadedAssembly.GetType(typeName);
			}
			catch
			{
				continue;
			}
			if (value != null)
			{
				Type type = value;
				typeMap[typeName] = type;
				return type;
			}
		}
		Type[] allTypes = GetAllTypes();
		foreach (Type type2 in allTypes)
		{
			if (type2.Name == typeName)
			{
				Type type = type2;
				typeMap[typeName] = type;
				return type;
			}
		}
		Debug.LogError($"Requested Type with name '{typeName}', could not be loaded");
		return null;
	}

	public static Type[] GetAllTypes()
	{
		List<Type> list = new List<Type>();
		foreach (Assembly loadedAssembly in loadedAssemblies)
		{
			try
			{
				list.AddRange(loadedAssembly.RTGetExportedTypes());
			}
			catch
			{
			}
		}
		return list.ToArray();
	}

	public static Type[] GetDerivedTypesOf(Type baseType)
	{
		List<Type> list = new List<Type>();
		foreach (Assembly loadedAssembly in loadedAssemblies)
		{
			try
			{
				list.AddRange(from t in loadedAssembly.RTGetExportedTypes()
					where t.RTIsSubclassOf(baseType) && !t.RTIsAbstract()
					select t);
			}
			catch
			{
			}
		}
		return list.ToArray();
	}

	private static Type[] RTGetExportedTypes(this Assembly asm)
	{
		return asm.GetExportedTypes();
	}

	public static bool RTIsStatic(this PropertyInfo propertyInfo)
	{
		return (propertyInfo.CanRead && propertyInfo.RTGetGetMethod().IsStatic) || (propertyInfo.CanWrite && propertyInfo.RTGetSetMethod().IsStatic);
	}

	public static bool RTIsAbstract(this Type type)
	{
		return type.IsAbstract;
	}

	public static bool RTIsSubclassOf(this Type type, Type other)
	{
		return type.IsSubclassOf(other);
	}

	public static bool RTIsAssignableFrom(this Type type, Type second)
	{
		return type.IsAssignableFrom(second);
	}

	public static FieldInfo RTGetField(this Type type, string name)
	{
		return type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static PropertyInfo RTGetProperty(this Type type, string name)
	{
		return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodInfo RTGetMethod(this Type type, string name)
	{
		return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static FieldInfo[] RTGetFields(this Type type)
	{
		return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static PropertyInfo[] RTGetProperties(this Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MemberInfo[] RTGetPropsAndFields(this Type type)
	{
		List<MemberInfo> list = new List<MemberInfo>();
		list.AddRange(type.RTGetFields());
		list.AddRange(type.RTGetProperties());
		return list.ToArray();
	}

	public static MethodInfo RTGetGetMethod(this PropertyInfo prop)
	{
		return prop.GetGetMethod();
	}

	public static MethodInfo RTGetSetMethod(this PropertyInfo prop)
	{
		return prop.GetSetMethod();
	}

	public static Type RTReflectedType(this Type type)
	{
		return type.ReflectedType;
	}

	public static Type RTReflectedType(this MemberInfo member)
	{
		return member.ReflectedType;
	}

	public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute
	{
		return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
	}

	public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute
	{
		return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
	}

	public static T RTCreateDelegate<T>(this MethodInfo method, object instance)
	{
		return (T)(object)Delegate.CreateDelegate(typeof(T), instance, method);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace ParadoxNotion;

public static class ReflectionTools
{
	private const BindingFlags flagsEverything = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static Assembly[] _loadedAssemblies;

	private static Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

	private static Type[] _allTypes;

	private static Dictionary<Type, FieldInfo[]> _typeFields = new Dictionary<Type, FieldInfo[]>();

	private static Assembly[] loadedAssemblies
	{
		get
		{
			if (_loadedAssemblies == null)
			{
				_loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			}
			return _loadedAssemblies;
		}
	}

	public static Type GetType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null)
	{
		if (string.IsNullOrEmpty(typeFullName))
		{
			return null;
		}
		Type value = null;
		if (typeMap.TryGetValue(typeFullName, out value))
		{
			return value;
		}
		value = GetTypeDirect(typeFullName);
		Type type;
		if (value != null)
		{
			type = value;
			typeMap[typeFullName] = type;
			return type;
		}
		LateLog($"<b>(Type Request)</b> Trying Fallback Type match for type '{typeFullName}'...\n<i>(This happens if the type can't be resolved by it's full assembly/namespace name)</i>", LogType.Warning);
		value = TryResolveGenericType(typeFullName, fallbackNoNamespace, fallbackAssignable);
		if (value != null)
		{
			LateLog($"<b>(Type Request)</b> Fallback Type Resolved to '{value.FullName}'");
			type = value;
			typeMap[typeFullName] = type;
			return type;
		}
		value = TryResolveDeserializeFromAttribute(typeFullName);
		if (value != null)
		{
			LateLog($"<b>(Type Request)</b> Fallback Type Resolved to '{value.FullName}'");
			type = value;
			typeMap[typeFullName] = type;
			return type;
		}
		if (fallbackNoNamespace)
		{
			value = TryResolveWithoutNamespace(typeFullName, fallbackAssignable);
			if (value != null)
			{
				LateLog($"<b>(Type Request)</b> Fallback Type Resolved to '{value.FullName}'");
				type = value;
				typeMap[value.FullName] = type;
				return type;
			}
		}
		LateLog($"<b>(Type Request)</b> Type with name '{typeFullName}' could not be resolved.", LogType.Error);
		type = null;
		typeMap[typeFullName] = type;
		return type;
	}

	private static void LateLog(object logMessage, LogType logType = LogType.Log)
	{
	}

	private static Type GetTypeDirect(string typeFullName)
	{
		Type type = Type.GetType(typeFullName);
		if (type != null)
		{
			return type;
		}
		for (int i = 0; i < loadedAssemblies.Length; i++)
		{
			Assembly assembly = loadedAssemblies[i];
			try
			{
				type = assembly.GetType(typeFullName);
			}
			catch
			{
				continue;
			}
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	private static Type TryResolveGenericType(string typeFullName, bool fallbackNoNamespace = false, Type fallbackAssignable = null)
	{
		if (!typeFullName.Contains('`') || !typeFullName.Contains('['))
		{
			return null;
		}
		try
		{
			int num = typeFullName.IndexOf('`');
			string typeFullName2 = typeFullName.Substring(0, num + 2);
			Type type = GetType(typeFullName2, fallbackNoNamespace, fallbackAssignable);
			if (type == null)
			{
				return null;
			}
			int num2 = Convert.ToInt32(typeFullName.Substring(num + 1, 1));
			string text = typeFullName.Substring(num + 2, typeFullName.Length - num - 2);
			string[] array = null;
			if (text.StartsWith("[["))
			{
				int num3 = typeFullName.IndexOf("[[") + 2;
				int num4 = typeFullName.LastIndexOf("]]");
				text = typeFullName.Substring(num3, num4 - num3);
				array = text.Split(new string[1] { "],[" }, num2, StringSplitOptions.RemoveEmptyEntries).ToArray();
			}
			else
			{
				int num5 = typeFullName.IndexOf('[') + 1;
				int num6 = typeFullName.LastIndexOf(']');
				text = typeFullName.Substring(num5, num6 - num5);
				array = text.Split(new char[1] { ',' }, num2, StringSplitOptions.RemoveEmptyEntries).ToArray();
			}
			Type[] array2 = new Type[num2];
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				if (!text2.Contains('`') && text2.Contains(','))
				{
					text2 = text2.Substring(0, text2.IndexOf(','));
				}
				Type fallbackAssignable2 = null;
				if (fallbackNoNamespace)
				{
					Type type2 = type.RTGetGenericArguments()[i];
					Type[] genericParameterConstraints = type2.GetGenericParameterConstraints();
					fallbackAssignable2 = ((genericParameterConstraints.Length != 0) ? genericParameterConstraints[0] : typeof(object));
				}
				Type type3 = GetType(text2, fallbackNoNamespace, fallbackAssignable2);
				if (type3 == null)
				{
					return null;
				}
				array2[i] = type3;
			}
			return type.RTMakeGenericType(array2);
		}
		catch (Exception ex)
		{
			LateLog("<b>(Type Request)</b> BUG (Please report this): " + ex.Message, LogType.Error);
			return null;
		}
	}

	private static Type TryResolveDeserializeFromAttribute(string typeName)
	{
		Type[] allTypes = GetAllTypes();
		foreach (Type type in allTypes)
		{
			DeserializeFromAttribute deserializeFromAttribute = type.RTGetAttribute<DeserializeFromAttribute>(inherited: false);
			if (deserializeFromAttribute != null && deserializeFromAttribute.previousTypeNames.Any((string n) => n == typeName))
			{
				return type;
			}
		}
		return null;
	}

	private static Type TryResolveWithoutNamespace(string typeName, Type fallbackAssignable = null)
	{
		if (typeName.Contains('`') && typeName.Contains('['))
		{
			return null;
		}
		if (typeName.Contains(','))
		{
			typeName = typeName.Substring(0, typeName.IndexOf(','));
		}
		if (typeName.Contains('.'))
		{
			int num = typeName.LastIndexOf('.') + 1;
			typeName = typeName.Substring(num, typeName.Length - num);
		}
		Type[] allTypes = GetAllTypes();
		foreach (Type type in allTypes)
		{
			if (type.Name == typeName && (fallbackAssignable == null || fallbackAssignable.RTIsAssignableFrom(type)))
			{
				return type;
			}
		}
		return null;
	}

	public static Type[] GetAllTypes()
	{
		if (_allTypes != null)
		{
			return _allTypes;
		}
		List<Type> list = new List<Type>();
		for (int i = 0; i < loadedAssemblies.Length; i++)
		{
			Assembly asm = loadedAssemblies[i];
			try
			{
				list.AddRange(asm.RTGetExportedTypes());
			}
			catch
			{
			}
		}
		return _allTypes = list.ToArray();
	}

	private static Type[] RTGetExportedTypes(this Assembly asm)
	{
		return asm.GetExportedTypes();
	}

	public static string FriendlyName(this Type t, bool trueSignature = false)
	{
		if (t == null)
		{
			return null;
		}
		if (!trueSignature && t == typeof(UnityEngine.Object))
		{
			return "UnityObject";
		}
		string text = ((!trueSignature) ? t.Name : t.FullName);
		if (!trueSignature)
		{
			if (text == "Single")
			{
				text = "Float";
			}
			if (text == "Int32")
			{
				text = "Integer";
			}
		}
		if (t.RTIsGenericParameter())
		{
			text = "T";
		}
		if (t.RTIsGenericType())
		{
			text = ((!trueSignature) ? t.Name : t.FullName);
			Type[] array = t.RTGetGenericArguments();
			if (array.Length != 0)
			{
				text = text.Replace("`" + array.Length, string.Empty);
				text += "<";
				for (int i = 0; i < array.Length; i++)
				{
					text = text + ((i != 0) ? ", " : string.Empty) + array[i].FriendlyName(trueSignature);
				}
				text += ">";
			}
		}
		return text;
	}

	public static string SignatureName(this MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters();
		string text = ((!method.IsStatic) ? string.Empty : "static ") + method.Name + " (";
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			text = text + ((!parameterInfo.IsOut) ? string.Empty : "out ") + parameterInfo.ParameterType.FriendlyName() + ((i >= parameters.Length - 1) ? string.Empty : ", ");
		}
		return text + ") : " + method.ReturnType.FriendlyName();
	}

	public static Type RTReflectedType(this Type type)
	{
		return type.ReflectedType;
	}

	public static Type RTReflectedType(this MemberInfo member)
	{
		return member.ReflectedType;
	}

	public static bool RTIsAssignableFrom(this Type type, Type second)
	{
		return type.IsAssignableFrom(second);
	}

	public static bool RTIsAbstract(this Type type)
	{
		return type.IsAbstract;
	}

	public static bool RTIsValueType(this Type type)
	{
		return type.IsValueType;
	}

	public static bool RTIsArray(this Type type)
	{
		return type.IsArray;
	}

	public static bool RTIsInterface(this Type type)
	{
		return type.IsInterface;
	}

	public static bool RTIsSubclassOf(this Type type, Type other)
	{
		return type.IsSubclassOf(other);
	}

	public static bool RTIsGenericParameter(this Type type)
	{
		return type.IsGenericParameter;
	}

	public static bool RTIsGenericType(this Type type)
	{
		return type.IsGenericType;
	}

	public static MethodInfo RTGetGetMethod(this PropertyInfo prop)
	{
		return prop.GetGetMethod();
	}

	public static MethodInfo RTGetSetMethod(this PropertyInfo prop)
	{
		return prop.GetSetMethod();
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

	public static MethodInfo RTGetMethod(this Type type, string name, Type[] paramTypes)
	{
		return type.GetMethod(name, paramTypes);
	}

	public static EventInfo RTGetEvent(this Type type, string name)
	{
		return type.GetEvent(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodInfo RTGetDelegateMethodInfo(this Delegate del)
	{
		return del.Method;
	}

	public static FieldInfo[] RTGetFields(this Type type)
	{
		if (!_typeFields.TryGetValue(type, out var value))
		{
			value = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			_typeFields[type] = value;
		}
		return value;
	}

	public static PropertyInfo[] RTGetProperties(this Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodInfo[] RTGetMethods(this Type type)
	{
		return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute
	{
		return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
	}

	public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute
	{
		return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
	}

	public static Type RTMakeGenericType(this Type type, Type[] typeArgs)
	{
		return type.MakeGenericType(typeArgs);
	}

	public static Type[] RTGetGenericArguments(this Type type)
	{
		return type.GetGenericArguments();
	}

	public static Type[] RTGetEmptyTypes()
	{
		return Type.EmptyTypes;
	}

	public static T RTCreateDelegate<T>(this MethodInfo method, object instance)
	{
		return (T)(object)method.RTCreateDelegate(typeof(T), instance);
	}

	public static Delegate RTCreateDelegate(this MethodInfo method, Type type, object instance)
	{
		return Delegate.CreateDelegate(type, instance, method);
	}

	public static bool IsObsolete(this MemberInfo member)
	{
		if (member is MethodInfo)
		{
			MethodInfo methodInfo = (MethodInfo)member;
			if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
			{
				member = methodInfo.DeclaringType.RTGetProperty(methodInfo.Name.Replace("get_", string.Empty).Replace("set_", string.Empty));
			}
		}
		return member.RTGetAttribute<ObsoleteAttribute>(inherited: true) != null;
	}

	public static bool IsReadOnly(this FieldInfo field)
	{
		return field.IsInitOnly || field.IsLiteral;
	}

	public static PropertyInfo GetBaseDefinition(this PropertyInfo propertyInfo)
	{
		MethodInfo methodInfo = propertyInfo.GetAccessors(nonPublic: true)[0];
		if (methodInfo == null)
		{
			return null;
		}
		MethodInfo baseDefinition = methodInfo.GetBaseDefinition();
		if (baseDefinition == methodInfo)
		{
			return propertyInfo;
		}
		Type[] types = (from p in propertyInfo.GetIndexParameters()
			select p.ParameterType).ToArray();
		return baseDefinition.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, propertyInfo.PropertyType, types, null);
	}

	public static FieldInfo GetBaseDefinition(this FieldInfo fieldInfo)
	{
		return fieldInfo.DeclaringType.RTGetField(fieldInfo.Name);
	}
}

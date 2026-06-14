using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ParadoxNotion.Serialization.FullSerializer.Internal;

public class fsIEnumerableConverter : fsConverter
{
	public override bool CanProcess(Type type)
	{
		if (!typeof(IEnumerable).IsAssignableFrom(type))
		{
			return false;
		}
		return GetAddMethod(type) != null;
	}

	public override object CreateInstance(fsData data, Type storageType)
	{
		return fsMetaType.Get(Serializer.Config, storageType).CreateInstance();
	}

	public override fsResult TrySerialize(object instance_, out fsData serialized, Type storageType)
	{
		IEnumerable enumerable = (IEnumerable)instance_;
		fsResult success = fsResult.Success;
		Type elementType = GetElementType(storageType);
		serialized = fsData.CreateList(HintSize(enumerable));
		List<fsData> asList = serialized.AsList;
		foreach (object item in enumerable)
		{
			fsData data;
			fsResult result = Serializer.TrySerialize(elementType, item, out data);
			success.AddMessages(result);
			if (!result.Failed)
			{
				asList.Add(data);
			}
		}
		if (IsStack(enumerable.GetType()))
		{
			asList.Reverse();
		}
		return success;
	}

	private bool IsStack(Type type)
	{
		return type.Resolve().IsGenericType && type.Resolve().GetGenericTypeDefinition() == typeof(Stack<>);
	}

	public override fsResult TryDeserialize(fsData data, ref object instance_, Type storageType)
	{
		IEnumerable enumerable = (IEnumerable)instance_;
		fsResult success = fsResult.Success;
		fsResult fsResult = (success += CheckType(data, fsDataType.Array));
		if (fsResult.Failed)
		{
			return success;
		}
		if (data.AsList.Count == 0)
		{
			return fsResult.Success;
		}
		if (enumerable is IList)
		{
			Type[] genericArguments = storageType.GetGenericArguments();
			if (genericArguments.Length == 1)
			{
				IList list = (IList)enumerable;
				Type storageType2 = genericArguments[0];
				for (int i = 0; i < data.AsList.Count; i++)
				{
					object result = null;
					Serializer.TryDeserialize(data.AsList[i], storageType2, ref result);
					list.Add(result);
				}
				return fsResult.Success;
			}
		}
		Type elementType = GetElementType(storageType);
		MethodInfo addMethod = GetAddMethod(storageType);
		MethodInfo flattenedMethod = storageType.GetFlattenedMethod("get_Item");
		MethodInfo flattenedMethod2 = storageType.GetFlattenedMethod("set_Item");
		if (flattenedMethod2 == null)
		{
			TryClear(storageType, enumerable);
		}
		int num = TryGetExistingSize(storageType, enumerable);
		List<fsData> asList = data.AsList;
		for (int j = 0; j < asList.Count; j++)
		{
			fsData data2 = asList[j];
			object result2 = null;
			if (flattenedMethod != null && j < num)
			{
				result2 = flattenedMethod.Invoke(enumerable, new object[1] { j });
			}
			fsResult result3 = Serializer.TryDeserialize(data2, elementType, ref result2);
			success.AddMessages(result3);
			if (!result3.Failed)
			{
				if (flattenedMethod2 != null && j < num)
				{
					flattenedMethod2.Invoke(enumerable, new object[2] { j, result2 });
				}
				else
				{
					addMethod.Invoke(enumerable, new object[1] { result2 });
				}
			}
		}
		return success;
	}

	private static int HintSize(IEnumerable collection)
	{
		if (collection is ICollection)
		{
			return ((ICollection)collection).Count;
		}
		return 0;
	}

	private static Type GetElementType(Type objectType)
	{
		if (objectType.HasElementType)
		{
			return objectType.GetElementType();
		}
		Type @interface = fsReflectionUtility.GetInterface(objectType, typeof(IEnumerable<>));
		if (@interface != null)
		{
			return @interface.GetGenericArguments()[0];
		}
		return typeof(object);
	}

	private static void TryClear(Type type, object instance)
	{
		type.GetFlattenedMethod("Clear")?.Invoke(instance, null);
	}

	private static int TryGetExistingSize(Type type, object instance)
	{
		PropertyInfo flattenedProperty = type.GetFlattenedProperty("Count");
		if (flattenedProperty != null)
		{
			return (int)flattenedProperty.GetGetMethod().Invoke(instance, null);
		}
		return 0;
	}

	private static MethodInfo GetAddMethod(Type type)
	{
		Type @interface = fsReflectionUtility.GetInterface(type, typeof(ICollection<>));
		if (@interface != null)
		{
			MethodInfo declaredMethod = @interface.GetDeclaredMethod("Add");
			if (declaredMethod != null)
			{
				return declaredMethod;
			}
		}
		return type.GetFlattenedMethod("Add") ?? type.GetFlattenedMethod("Push") ?? type.GetFlattenedMethod("Enqueue");
	}
}

using System;
using System.Collections.Generic;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Serialization.FullSerializer.Internal;
using UnityEngine;

namespace ParadoxNotion.Serialization;

public static class JSONSerializer
{
	private static Dictionary<string, fsData> cache = new Dictionary<string, fsData>();

	private static object serializerLock = new object();

	private static fsSerializer serializer = new fsSerializer();

	private static bool init = false;

	public static bool applicationPlaying = true;

	public static string Serialize(Type type, object value, bool pretyJson = false, List<UnityEngine.Object> objectReferences = null)
	{
		lock (serializerLock)
		{
			if (!init)
			{
				serializer.AddConverter(new fsUnityObjectConverter());
				init = true;
			}
			if (objectReferences != null)
			{
				objectReferences.Clear();
				serializer.Context.Set(objectReferences);
			}
			Type overrideConverterType = ((!typeof(UnityEngine.Object).RTIsAssignableFrom(type)) ? null : typeof(fsReflectedConverter));
			serializer.TrySerialize(type, overrideConverterType, value, out var data).AssertSuccess();
			if (pretyJson)
			{
				return fsJsonPrinter.PrettyJson(data);
			}
			return fsJsonPrinter.CompressedJson(data);
		}
	}

	public static T Deserialize<T>(string serializedState, List<UnityEngine.Object> objectReferences = null, T deserialized = default(T))
	{
		return (T)Deserialize(typeof(T), serializedState, objectReferences, deserialized);
	}

	public static object Deserialize(Type type, string serializedState, List<UnityEngine.Object> objectReferences = null, object deserialized = null)
	{
		lock (serializerLock)
		{
			if (!init)
			{
				serializer.AddConverter(new fsUnityObjectConverter());
				init = true;
			}
			if (objectReferences != null)
			{
				serializer.Context.Set(objectReferences);
			}
			fsData value = null;
			cache.TryGetValue(serializedState, out value);
			if (value == null)
			{
				value = fsJsonParser.Parse(serializedState);
				cache[serializedState] = value;
			}
			Type overrideConverterType = ((!typeof(UnityEngine.Object).RTIsAssignableFrom(type)) ? null : typeof(fsReflectedConverter));
			serializer.TryDeserialize(value, type, overrideConverterType, ref deserialized).AssertSuccess();
			return deserialized;
		}
	}

	public static void ForceCleanup()
	{
		cache.Clear();
	}
}

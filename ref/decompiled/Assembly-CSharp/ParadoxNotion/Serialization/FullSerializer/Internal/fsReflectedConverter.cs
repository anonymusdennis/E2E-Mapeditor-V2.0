using System;
using System.Collections;
using UnityEngine;

namespace ParadoxNotion.Serialization.FullSerializer.Internal;

public class fsReflectedConverter : fsConverter
{
	public override bool CanProcess(Type type)
	{
		if (type.Resolve().IsArray || typeof(ICollection).IsAssignableFrom(type))
		{
			return false;
		}
		return true;
	}

	public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
	{
		serialized = fsData.CreateDictionary();
		fsResult success = fsResult.Success;
		fsMetaType fsMetaType = fsMetaType.Get(Serializer.Config, instance.GetType());
		fsMetaType.EmitAotData();
		object obj = null;
		if (!fsGlobalConfig.SerializeDefaultValues && !(instance is UnityEngine.Object))
		{
			obj = fsMetaType.CreateInstance();
		}
		for (int i = 0; i < fsMetaType.Properties.Length; i++)
		{
			fsMetaProperty fsMetaProperty2 = fsMetaType.Properties[i];
			if (fsMetaProperty2.CanRead && (fsGlobalConfig.SerializeDefaultValues || obj == null || !object.Equals(fsMetaProperty2.Read(instance), fsMetaProperty2.Read(obj))))
			{
				fsData data;
				fsResult result = Serializer.TrySerialize(fsMetaProperty2.StorageType, fsMetaProperty2.OverrideConverterType, fsMetaProperty2.Read(instance), out data);
				success.AddMessages(result);
				if (!result.Failed)
				{
					serialized.AsDictionary[fsMetaProperty2.JsonName] = data;
				}
			}
		}
		return success;
	}

	public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
	{
		fsResult success = fsResult.Success;
		fsResult fsResult = (success += CheckType(data, fsDataType.Object));
		if (fsResult.Failed)
		{
			return success;
		}
		if (data.AsDictionary.Count == 0)
		{
			return fsResult.Success;
		}
		fsMetaType fsMetaType = fsMetaType.Get(Serializer.Config, storageType);
		fsMetaType.EmitAotData();
		for (int i = 0; i < fsMetaType.Properties.Length; i++)
		{
			fsMetaProperty fsMetaProperty2 = fsMetaType.Properties[i];
			if (fsMetaProperty2.CanWrite && data.AsDictionary.TryGetValue(fsMetaProperty2.JsonName, out var value))
			{
				object result = null;
				fsResult result2 = Serializer.TryDeserialize(value, fsMetaProperty2.StorageType, fsMetaProperty2.OverrideConverterType, ref result);
				success.AddMessages(result2);
				if (!result2.Failed)
				{
					fsMetaProperty2.Write(instance, result);
				}
			}
		}
		return success;
	}

	public override object CreateInstance(fsData data, Type storageType)
	{
		fsMetaType fsMetaType = fsMetaType.Get(Serializer.Config, storageType);
		return fsMetaType.CreateInstance();
	}
}

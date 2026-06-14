using System;
using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Framework.Internal;

public class fsReflectedActionWrapperProcessor : fsObjectProcessor
{
	public override bool CanProcess(Type type)
	{
		return typeof(ReflectedActionWrapper).RTIsAssignableFrom(type);
	}

	public override void OnBeforeSerialize(Type storageType, object instance)
	{
	}

	public override void OnAfterSerialize(Type storageType, object instance, ref fsData data)
	{
	}

	public override void OnBeforeDeserialize(Type storageType, ref fsData data)
	{
		if (data.IsNull)
		{
			return;
		}
		Dictionary<string, fsData> asDictionary = data.AsDictionary;
		if (asDictionary.ContainsKey("$type"))
		{
			Type type = ReflectionTools.GetType(asDictionary["$type"].AsString);
			if (type == null)
			{
				asDictionary["$type"] = new fsData(typeof(ReflectedAction).FullName);
			}
		}
	}

	public override void OnAfterDeserialize(Type storageType, object instance)
	{
	}
}

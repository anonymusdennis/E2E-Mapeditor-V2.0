using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParadoxNotion.Serialization.FullSerializer.Internal.DirectConverters;

public class Keyframe_DirectConverter : fsDirectConverter<Keyframe>
{
	protected override fsResult DoSerialize(Keyframe model, Dictionary<string, fsData> serialized)
	{
		fsResult success = fsResult.Success;
		success += SerializeMember(serialized, null, "time", model.time);
		success += SerializeMember(serialized, null, "value", model.value);
		success += SerializeMember(serialized, null, "tangentMode", model.tangentMode);
		success += SerializeMember(serialized, null, "inTangent", model.inTangent);
		return success + SerializeMember(serialized, null, "outTangent", model.outTangent);
	}

	protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Keyframe model)
	{
		fsResult success = fsResult.Success;
		float value = model.time;
		success += DeserializeMember<float>(data, null, "time", out value);
		model.time = value;
		float value2 = model.value;
		success += DeserializeMember<float>(data, null, "value", out value2);
		model.value = value2;
		int value3 = model.tangentMode;
		success += DeserializeMember<int>(data, null, "tangentMode", out value3);
		model.tangentMode = value3;
		float value4 = model.inTangent;
		success += DeserializeMember<float>(data, null, "inTangent", out value4);
		model.inTangent = value4;
		float value5 = model.outTangent;
		success += DeserializeMember<float>(data, null, "outTangent", out value5);
		model.outTangent = value5;
		return success;
	}

	public override object CreateInstance(fsData data, Type storageType)
	{
		return default(Keyframe);
	}
}

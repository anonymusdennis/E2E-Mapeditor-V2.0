using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Framework.Internal;

public class fsNodeProcessor : fsObjectProcessor
{
	public override bool CanProcess(Type type)
	{
		return typeof(Node).RTIsAssignableFrom(type);
	}

	public override void OnBeforeDeserialize(Type storageType, ref fsData data)
	{
		if (data.IsNull)
		{
			return;
		}
		Dictionary<string, fsData> json = data.AsDictionary;
		if (!json.TryGetValue("$type", out var value))
		{
			return;
		}
		Type type = ReflectionTools.GetType(value.AsString);
		if (type == null)
		{
			type = TryGetReplacement(value.AsString);
			if (type != null)
			{
				json["$type"] = new fsData(type.FullName);
				return;
			}
			json["recoveryState"] = new fsData(data.ToString());
			json["missingType"] = new fsData(value.AsString);
			json["$type"] = new fsData(typeof(MissingNode).FullName);
		}
		if (type != typeof(MissingNode))
		{
			return;
		}
		Type type2 = ReflectionTools.GetType(json["missingType"].AsString);
		if (type2 == null)
		{
			type2 = TryGetReplacement(json["missingType"].AsString);
		}
		if (type2 != null)
		{
			string asString = json["recoveryState"].AsString;
			Dictionary<string, fsData> asDictionary = fsJsonParser.Parse(asString).AsDictionary;
			json = json.Concat(asDictionary.Where((KeyValuePair<string, fsData> kvp) => !json.ContainsKey(kvp.Key))).ToDictionary((KeyValuePair<string, fsData> c) => c.Key, (KeyValuePair<string, fsData> c) => c.Value);
			json["$type"] = new fsData(type2.FullName);
			data = new fsData(json);
		}
	}

	private Type TryGetReplacement(string targetFullTypeName)
	{
		Type[] allTypes = ReflectionTools.GetAllTypes();
		Type[] array = allTypes;
		foreach (Type type in array)
		{
			DeserializeFromAttribute deserializeFromAttribute = type.RTGetAttribute<DeserializeFromAttribute>(inherited: false);
			if (deserializeFromAttribute != null && deserializeFromAttribute.previousTypeNames.Any((string n) => n == targetFullTypeName))
			{
				return type;
			}
		}
		string text = targetFullTypeName.Split('.').LastOrDefault();
		Type[] array2 = allTypes;
		foreach (Type type2 in array2)
		{
			if (type2.Name == text && type2.RTIsSubclassOf(typeof(Node)))
			{
				return type2;
			}
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Framework.Internal;

public class fsConnectionProcessor : fsObjectProcessor
{
	public override bool CanProcess(Type type)
	{
		return typeof(Connection).RTIsAssignableFrom(type);
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
			json["$type"] = new fsData(typeof(MissingConnection).FullName);
		}
		if (type != typeof(MissingConnection))
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
		string text = targetFullTypeName.Split('.').LastOrDefault();
		Type[] allTypes = ReflectionTools.GetAllTypes();
		foreach (Type type in allTypes)
		{
			if (type.Name == text && type.RTIsSubclassOf(typeof(Connection)))
			{
				return type;
			}
		}
		return null;
	}
}

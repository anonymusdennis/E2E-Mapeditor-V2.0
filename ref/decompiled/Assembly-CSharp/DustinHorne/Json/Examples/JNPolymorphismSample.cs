using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DustinHorne.Json.Examples;

public class JNPolymorphismSample
{
	private System.Random _rnd = new System.Random();

	public void Sample()
	{
		List<JNSimpleObjectModel> list = new List<JNSimpleObjectModel>();
		for (int i = 0; i < 3; i++)
		{
			list.Add(GetBaseModel());
		}
		for (int j = 0; j < 2; j++)
		{
			list.Add(GetSubClassModel());
		}
		for (int k = 0; k < 3; k++)
		{
			list.Add(GetBaseModel());
		}
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
		JsonSerializerSettings settings = jsonSerializerSettings;
		string value = JsonConvert.SerializeObject(list, Formatting.None, settings);
		List<JNSimpleObjectModel> list2 = JsonConvert.DeserializeObject<List<JNSimpleObjectModel>>(value, settings);
		for (int l = 0; l < list2.Count; l++)
		{
			JNSimpleObjectModel jNSimpleObjectModel = list2[l];
			if (jNSimpleObjectModel.ObjectType == JNObjectType.SubClass)
			{
				Debug.Log((jNSimpleObjectModel as JNSubClassModel).SubClassStringValue);
			}
			else
			{
				Debug.Log(jNSimpleObjectModel.StringValue);
			}
		}
	}

	private JNSimpleObjectModel GetBaseModel()
	{
		JNSimpleObjectModel jNSimpleObjectModel = new JNSimpleObjectModel();
		jNSimpleObjectModel.IntValue = _rnd.Next();
		jNSimpleObjectModel.FloatValue = (float)_rnd.NextDouble();
		jNSimpleObjectModel.StringValue = Guid.NewGuid().ToString();
		jNSimpleObjectModel.IntList = new List<int>
		{
			_rnd.Next(),
			_rnd.Next(),
			_rnd.Next()
		};
		jNSimpleObjectModel.ObjectType = JNObjectType.BaseClass;
		return jNSimpleObjectModel;
	}

	private JNSubClassModel GetSubClassModel()
	{
		JNSubClassModel jNSubClassModel = new JNSubClassModel();
		jNSubClassModel.IntValue = _rnd.Next();
		jNSubClassModel.FloatValue = (float)_rnd.NextDouble();
		jNSubClassModel.StringValue = Guid.NewGuid().ToString();
		jNSubClassModel.IntList = new List<int>
		{
			_rnd.Next(),
			_rnd.Next(),
			_rnd.Next()
		};
		jNSubClassModel.ObjectType = JNObjectType.SubClass;
		jNSubClassModel.SubClassStringValue = "This is the subclass value.";
		return jNSubClassModel;
	}
}

using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DustinHorne.Json.Examples;

public class JNSimpleObjectSample
{
	public void Sample()
	{
		JNSimpleObjectModel jNSimpleObjectModel = new JNSimpleObjectModel();
		jNSimpleObjectModel.IntValue = 5;
		jNSimpleObjectModel.FloatValue = 4.98f;
		jNSimpleObjectModel.StringValue = "Simple Object";
		jNSimpleObjectModel.IntList = new List<int> { 4, 7, 25, 34 };
		jNSimpleObjectModel.ObjectType = JNObjectType.BaseClass;
		JNSimpleObjectModel value = jNSimpleObjectModel;
		string value2 = JsonConvert.SerializeObject(value);
		JNSimpleObjectModel jNSimpleObjectModel2 = JsonConvert.DeserializeObject<JNSimpleObjectModel>(value2);
		Debug.Log(jNSimpleObjectModel2.IntList.Count);
	}
}

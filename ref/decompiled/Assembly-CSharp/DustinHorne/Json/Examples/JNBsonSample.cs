using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using UnityEngine;

namespace DustinHorne.Json.Examples;

public class JNBsonSample
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
		byte[] array = new byte[0];
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BsonWriter jsonWriter = new BsonWriter(memoryStream))
			{
				JsonSerializer jsonSerializer = new JsonSerializer();
				jsonSerializer.Serialize(jsonWriter, value);
			}
			array = memoryStream.ToArray();
			string message = Convert.ToBase64String(array);
			Debug.Log(message);
		}
		JNSimpleObjectModel jNSimpleObjectModel2;
		using (MemoryStream stream = new MemoryStream(array))
		{
			using BsonReader reader = new BsonReader(stream);
			JsonSerializer jsonSerializer2 = new JsonSerializer();
			jNSimpleObjectModel2 = jsonSerializer2.Deserialize<JNSimpleObjectModel>(reader);
		}
		if (jNSimpleObjectModel2 != null)
		{
			Debug.Log(jNSimpleObjectModel2.StringValue);
		}
	}
}

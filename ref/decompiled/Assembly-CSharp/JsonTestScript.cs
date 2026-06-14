using System;
using System.Collections.Generic;
using System.Text;
using Assets.DustinHorne.JsonDotNetUnity.TestCases;
using Assets.DustinHorne.JsonDotNetUnity.TestCases.TestModels;
using Newtonsoft.Json;
using SampleClassLibrary;
using UnityEngine;

public class JsonTestScript
{
	private TextMesh _text;

	private const string BAD_RESULT_MESSAGE = "Incorrect Deserialized Result";

	public JsonTestScript(TextMesh text)
	{
		_text = text;
	}

	public void SerializeVector3()
	{
		LogStart("Vector3 Serialization");
		try
		{
			Vector3 vector = new Vector3(2f, 4f, 6f);
			string text = JsonConvert.SerializeObject(vector);
			LogSerialized(text);
			Vector3 vector2 = JsonConvert.DeserializeObject<Vector3>(text);
			LogResult("4", vector2.y);
			if (vector2.y != vector.y)
			{
				DisplayFail("Vector3 Serialization", "Incorrect Deserialized Result");
			}
			DisplaySuccess("Vector3 Serialization");
		}
		catch (Exception ex)
		{
			DisplayFail("Vector3 Serialization", ex.Message);
		}
		LogEnd(1);
	}

	public void GenericListSerialization()
	{
		LogStart("List<T> Serialization");
		try
		{
			List<SimpleClassObject> list = new List<SimpleClassObject>();
			for (int i = 0; i < 4; i++)
			{
				list.Add(TestCaseUtils.GetSimpleClassObject());
			}
			string text = JsonConvert.SerializeObject(list);
			LogSerialized(text);
			List<SimpleClassObject> list2 = JsonConvert.DeserializeObject<List<SimpleClassObject>>(text);
			LogResult(list.Count.ToString(), list2.Count);
			LogResult(list[2].TextValue, list2[2].TextValue);
			if (list.Count != list2.Count || list[3].TextValue != list2[3].TextValue)
			{
				DisplayFail("List<T> Serialization", "Incorrect Deserialized Result");
				Debug.LogError("Deserialized List<T> has incorrect count or wrong item value");
			}
			else
			{
				DisplaySuccess("List<T> Serialization");
			}
		}
		catch (Exception ex)
		{
			DisplayFail("List<T> Serialization", ex.Message);
			throw;
		}
		LogEnd(2);
	}

	public void PolymorphicSerialization()
	{
		LogStart("Polymorphic Serialization");
		try
		{
			List<SampleBase> list = new List<SampleBase>();
			for (int i = 0; i < 4; i++)
			{
				list.Add(TestCaseUtils.GetSampleChid());
			}
			string text = JsonConvert.SerializeObject(list, Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
			LogSerialized(text);
			List<SampleBase> list2 = JsonConvert.DeserializeObject<List<SampleBase>>(text, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
			if (!(list2[2] is SampleChild))
			{
				DisplayFail("Polymorphic Serialization", "Incorrect Deserialized Result");
			}
			else
			{
				LogResult(list[2].TextValue, list2[2].TextValue);
				if (list[2].TextValue != list2[2].TextValue)
				{
					DisplayFail("Polymorphic Serialization", "Incorrect Deserialized Result");
				}
				else
				{
					DisplaySuccess("Polymorphic Serialization");
				}
			}
		}
		catch (Exception ex)
		{
			DisplayFail("Polymorphic Serialization", ex.Message);
			throw;
		}
		LogEnd(3);
	}

	public void DictionarySerialization()
	{
		LogStart("Dictionary & Other DLL");
		try
		{
			SampleExternalClass sampleExternalClass = new SampleExternalClass();
			sampleExternalClass.SampleString = Guid.NewGuid().ToString();
			SampleExternalClass sampleExternalClass2 = sampleExternalClass;
			sampleExternalClass2.SampleDictionary.Add(1, "A");
			sampleExternalClass2.SampleDictionary.Add(2, "B");
			sampleExternalClass2.SampleDictionary.Add(3, "C");
			sampleExternalClass2.SampleDictionary.Add(4, "D");
			string text = JsonConvert.SerializeObject(sampleExternalClass2);
			LogSerialized(text);
			SampleExternalClass sampleExternalClass3 = JsonConvert.DeserializeObject<SampleExternalClass>(text);
			LogResult(sampleExternalClass2.SampleString, sampleExternalClass3.SampleString);
			LogResult(sampleExternalClass2.SampleDictionary.Count.ToString(), sampleExternalClass3.SampleDictionary.Count);
			StringBuilder stringBuilder = new StringBuilder(4);
			StringBuilder stringBuilder2 = new StringBuilder(4);
			foreach (KeyValuePair<int, string> item in sampleExternalClass2.SampleDictionary)
			{
				stringBuilder.Append(item.Key.ToString());
				stringBuilder2.Append(item.Value);
			}
			LogResult("1234", stringBuilder.ToString());
			LogResult("ABCD", stringBuilder2.ToString());
			if (sampleExternalClass2.SampleString != sampleExternalClass3.SampleString || sampleExternalClass2.SampleDictionary.Count != sampleExternalClass3.SampleDictionary.Count || stringBuilder.ToString() != "1234" || stringBuilder2.ToString() != "ABCD")
			{
				DisplayFail("Dictionary & Other DLL", "Incorrect Deserialized Result");
			}
			else
			{
				DisplaySuccess("Dictionary & Other DLL");
			}
		}
		catch (Exception ex)
		{
			DisplayFail("Dictionary & Other DLL", ex.Message);
			throw;
		}
	}

	public void DictionaryObjectValueSerialization()
	{
		LogStart("Dictionary (Object Value)");
		try
		{
			Dictionary<int, SampleBase> dictionary = new Dictionary<int, SampleBase>();
			for (int i = 0; i < 4; i++)
			{
				dictionary.Add(i, TestCaseUtils.GetSampleBase());
			}
			string text = JsonConvert.SerializeObject(dictionary);
			LogSerialized(text);
			Dictionary<int, SampleBase> dictionary2 = JsonConvert.DeserializeObject<Dictionary<int, SampleBase>>(text);
			LogResult(dictionary[1].TextValue, dictionary2[1].TextValue);
			if (dictionary[1].TextValue != dictionary2[1].TextValue)
			{
				DisplayFail("Dictionary (Object Value)", "Incorrect Deserialized Result");
			}
			else
			{
				DisplaySuccess("Dictionary (Object Value)");
			}
		}
		catch (Exception ex)
		{
			DisplayFail("Dictionary (Object Value)", ex.Message);
			throw;
		}
	}

	public void DictionaryObjectKeySerialization()
	{
		LogStart("Dictionary (Object As Key)");
		try
		{
			Dictionary<SampleBase, int> dictionary = new Dictionary<SampleBase, int>();
			for (int i = 0; i < 4; i++)
			{
				dictionary.Add(TestCaseUtils.GetSampleBase(), i);
			}
			string text = JsonConvert.SerializeObject(dictionary);
			LogSerialized(text);
			_text.text = text;
			Dictionary<SampleBase, int> dictionary2 = JsonConvert.DeserializeObject<Dictionary<SampleBase, int>>(text);
			List<SampleBase> list = new List<SampleBase>();
			List<SampleBase> list2 = new List<SampleBase>();
			foreach (SampleBase key in dictionary.Keys)
			{
				list.Add(key);
			}
			foreach (SampleBase key2 in dictionary2.Keys)
			{
				list2.Add(key2);
			}
			LogResult(list[1].TextValue, list2[1].TextValue);
			if (list[1].TextValue != list2[1].TextValue)
			{
				DisplayFail("Dictionary (Object As Key)", "Incorrect Deserialized Result");
			}
			else
			{
				DisplaySuccess("Dictionary (Object As Key)");
			}
		}
		catch (Exception ex)
		{
			DisplayFail("Dictionary (Object As Key)", ex.Message);
			throw;
		}
	}

	private void DisplaySuccess(string testName)
	{
		_text.text = testName + "\r\nSuccessful";
	}

	private void DisplayFail(string testName, string reason)
	{
		try
		{
			_text.text = (testName + "\r\nFailed :( \r\n" + reason) ?? string.Empty;
		}
		catch
		{
			Debug.Log("%%%%%%%%%%%" + testName);
		}
	}

	private void LogStart(string testName)
	{
		Log(string.Empty);
		Log($"======= SERIALIZATION TEST: {testName} ==========");
	}

	private void LogEnd(int testNum)
	{
	}

	private void Log(object message)
	{
		Debug.Log(message);
	}

	private void LogSerialized(string message)
	{
		Debug.Log($"#### Serialized Object: {message}");
	}

	private void LogResult(string shouldEqual, object actual)
	{
		Log("--------------------");
		Log($"*** Original Test value: {shouldEqual}");
		Log($"*** Deserialized Test Value: {actual}");
		Log("--------------------");
	}
}

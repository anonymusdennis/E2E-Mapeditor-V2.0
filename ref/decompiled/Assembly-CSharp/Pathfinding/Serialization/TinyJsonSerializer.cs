using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Pathfinding.Util;
using Pathfinding.WindowsStore;
using UnityEngine;

namespace Pathfinding.Serialization;

public class TinyJsonSerializer
{
	private StringBuilder output = new StringBuilder();

	private Dictionary<Type, Action<object>> serializers = new Dictionary<Type, Action<object>>();

	private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

	private TinyJsonSerializer()
	{
		serializers[typeof(float)] = delegate(object v)
		{
			output.Append(((float)v).ToString("R", invariantCulture));
		};
		Dictionary<Type, Action<object>> dictionary = serializers;
		Type typeFromHandle = typeof(Version);
		Action<object> action = delegate(object v)
		{
			output.Append(v.ToString());
		};
		serializers[typeof(int)] = action;
		action = action;
		serializers[typeof(uint)] = action;
		action = action;
		serializers[typeof(bool)] = action;
		dictionary[typeFromHandle] = action;
		serializers[typeof(string)] = delegate(object v)
		{
			output.AppendFormat("\"{0}\"", v);
		};
		serializers[typeof(Vector2)] = delegate(object v)
		{
			output.AppendFormat("{{ \"x\": {0}, \"y\": {1} }}", ((Vector2)v).x.ToString("R", invariantCulture), ((Vector2)v).y.ToString("R", invariantCulture));
		};
		serializers[typeof(Vector3)] = delegate(object v)
		{
			output.AppendFormat("{{ \"x\": {0}, \"y\": {1}, \"z\": {2} }}", ((Vector3)v).x.ToString("R", invariantCulture), ((Vector3)v).y.ToString("R", invariantCulture), ((Vector3)v).z.ToString("R", invariantCulture));
		};
		serializers[typeof(Pathfinding.Util.Guid)] = delegate(object v)
		{
			output.AppendFormat("{{ \"value\": \"{0}\" }}", v.ToString());
		};
		serializers[typeof(LayerMask)] = delegate(object v)
		{
			output.AppendFormat("{{ \"value\": {0} }}", ((int)(LayerMask)v).ToString());
		};
	}

	public static void Serialize(object obj, StringBuilder output)
	{
		TinyJsonSerializer tinyJsonSerializer = new TinyJsonSerializer();
		tinyJsonSerializer.output = output;
		tinyJsonSerializer.Serialize(obj);
	}

	private void Serialize(object obj)
	{
		if (obj == null)
		{
			output.Append("null");
			return;
		}
		Type typeInfo = WindowsStoreCompatibility.GetTypeInfo(obj.GetType());
		if (serializers.ContainsKey(typeInfo))
		{
			serializers[typeInfo](obj);
			return;
		}
		if (typeInfo.IsEnum)
		{
			output.Append('"' + obj.ToString() + '"');
			return;
		}
		if (obj is IList)
		{
			output.Append("[");
			IList list = obj as IList;
			for (int i = 0; i < list.Count; i++)
			{
				if (i != 0)
				{
					output.Append(", ");
				}
				Serialize(list[i]);
			}
			output.Append("]");
			return;
		}
		if (obj is UnityEngine.Object)
		{
			SerializeUnityObject(obj as UnityEngine.Object);
			return;
		}
		bool flag = typeInfo.GetCustomAttributes(typeof(JsonOptInAttribute), inherit: true).Length > 0;
		output.Append("{");
		FieldInfo[] fields = typeInfo.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		bool flag2 = false;
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if ((!flag && fieldInfo.IsPublic) || fieldInfo.GetCustomAttributes(typeof(JsonMemberAttribute), inherit: true).Length > 0)
			{
				if (flag2)
				{
					output.Append(", ");
				}
				flag2 = true;
				output.AppendFormat("\"{0}\": ", fieldInfo.Name);
				Serialize(fieldInfo.GetValue(obj));
			}
		}
		output.Append("}");
	}

	private void QuotedField(string name, string contents)
	{
		output.AppendFormat("\"{0}\": \"{1}\"", name, contents);
	}

	private void SerializeUnityObject(UnityEngine.Object obj)
	{
		if (obj == null)
		{
			Serialize(null);
			return;
		}
		output.Append("{");
		QuotedField("Name", obj.name);
		output.Append(", ");
		QuotedField("Type", obj.GetType().FullName);
		Component component = obj as Component;
		GameObject gameObject = obj as GameObject;
		if (component != null || gameObject != null)
		{
			if (component != null && gameObject == null)
			{
				gameObject = component.gameObject;
			}
			UnityReferenceHelper unityReferenceHelper = gameObject.GetComponent<UnityReferenceHelper>();
			if (unityReferenceHelper == null)
			{
				Debug.Log("Adding UnityReferenceHelper to Unity Reference '" + obj.name + "'");
				unityReferenceHelper = gameObject.AddComponent<UnityReferenceHelper>();
			}
			unityReferenceHelper.Reset();
			output.Append(", ");
			QuotedField("GUID", unityReferenceHelper.GetGUID().ToString());
		}
		output.Append("}");
	}
}

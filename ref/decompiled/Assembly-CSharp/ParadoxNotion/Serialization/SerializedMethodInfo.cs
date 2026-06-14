using System;
using System.Linq;
using System.Reflection;
using ParadoxNotion.Serialization.FullSerializer.Internal;
using UnityEngine;

namespace ParadoxNotion.Serialization;

[Serializable]
public class SerializedMethodInfo : ISerializationCallbackReceiver
{
	[SerializeField]
	private string _returnInfo;

	[SerializeField]
	private string _baseInfo;

	[SerializeField]
	private string _paramsInfo;

	[NonSerialized]
	private MethodInfo _method;

	[NonSerialized]
	private bool _hasChanged;

	public SerializedMethodInfo()
	{
	}

	public SerializedMethodInfo(MethodInfo method)
	{
		_hasChanged = false;
		_method = method;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_hasChanged = false;
		if (_method != null)
		{
			_returnInfo = _method.ReturnType.FullName;
			_baseInfo = $"{_method.RTReflectedType().FullName}|{_method.Name}";
			_paramsInfo = string.Join("|", (from p in _method.GetParameters()
				select p.ParameterType.FullName).ToArray());
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_hasChanged = false;
		string[] array = _baseInfo.Split('|');
		Type type = fsTypeCache.GetType(array[0], null);
		if (type == null)
		{
			_method = null;
			return;
		}
		string name = array[1];
		string[] array2 = ((!string.IsNullOrEmpty(_paramsInfo)) ? _paramsInfo.Split('|') : null);
		Type[] array3 = ((array2 != null) ? array2.Select((string n) => fsTypeCache.GetType(n, null)).ToArray() : new Type[0]);
		if (array3.All((Type t) => t != null))
		{
			_method = type.RTGetMethod(name, array3);
			if (!string.IsNullOrEmpty(_returnInfo))
			{
				Type type2 = fsTypeCache.GetType(_returnInfo, null);
				if (_method != null && type2 != _method.ReturnType)
				{
					_method = null;
				}
			}
		}
		if (_method == null)
		{
			_hasChanged = true;
			_method = type.RTGetMethods().FirstOrDefault((MethodInfo m) => m.Name == name);
		}
	}

	public MethodInfo Get()
	{
		return _method;
	}

	public bool HasChanged()
	{
		return _hasChanged;
	}

	public string GetMethodString()
	{
		return string.Format("{0} ({1})", _baseInfo.Replace("|", "."), _paramsInfo.Replace("|", ", "));
	}
}

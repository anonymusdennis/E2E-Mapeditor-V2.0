using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Framework;

public class Blackboard : MonoBehaviour, ISerializationCallbackReceiver, IBlackboard
{
	[SerializeField]
	private string _serializedBlackboard;

	[SerializeField]
	private List<UnityEngine.Object> _objectReferences;

	[NonSerialized]
	private BlackboardSource _blackboard = new BlackboardSource();

	[NonSerialized]
	private bool hasDeserialized;

	public new string name
	{
		get
		{
			return (!string.IsNullOrEmpty(_blackboard.name)) ? _blackboard.name : (base.gameObject.name + "_BB");
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = base.gameObject.name + "_BB";
			}
			_blackboard.name = value;
		}
	}

	public object this[string varName]
	{
		get
		{
			return _blackboard[varName];
		}
		set
		{
			SetValue(varName, value);
		}
	}

	public Dictionary<string, Variable> variables
	{
		get
		{
			return _blackboard.variables;
		}
		set
		{
			_blackboard.variables = value;
		}
	}

	public GameObject propertiesBindTarget => base.gameObject;

	public event Action<Variable> onVariableAdded;

	public event Action<Variable> onVariableRemoved;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (_objectReferences != null && _objectReferences.Any((UnityEngine.Object o) => o != null))
		{
			hasDeserialized = false;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (!hasDeserialized || !JSONSerializer.applicationPlaying)
		{
			hasDeserialized = true;
			_blackboard = JSONSerializer.Deserialize<BlackboardSource>(_serializedBlackboard, _objectReferences);
			if (_blackboard == null)
			{
				_blackboard = new BlackboardSource();
			}
		}
	}

	private void Awake()
	{
		_blackboard.InitializePropertiesBinding(propertiesBindTarget, callSetter: false);
	}

	public Variable AddVariable(string name, Type type)
	{
		Variable variable = _blackboard.AddVariable(name, type);
		if (this.onVariableAdded != null)
		{
			this.onVariableAdded(variable);
		}
		return variable;
	}

	public Variable AddVariable(string name, object value)
	{
		Variable variable = _blackboard.AddVariable(name, value);
		if (this.onVariableAdded != null)
		{
			this.onVariableAdded(variable);
		}
		return variable;
	}

	public Variable RemoveVariable(string name)
	{
		Variable variable = _blackboard.RemoveVariable(name);
		if (this.onVariableRemoved != null)
		{
			this.onVariableRemoved(variable);
		}
		return variable;
	}

	public Variable GetVariable(string name, Type ofType = null)
	{
		return _blackboard.GetVariable(name, ofType);
	}

	public Variable GetVariableByID(string ID)
	{
		return _blackboard.GetVariableByID(ID);
	}

	public Variable<T> GetVariable<T>(string name)
	{
		return _blackboard.GetVariable<T>(name);
	}

	public T GetValue<T>(string name)
	{
		return _blackboard.GetValue<T>(name);
	}

	public Variable SetValue(string name, object value)
	{
		return _blackboard.SetValue(name, value);
	}

	public string[] GetVariableNames()
	{
		return _blackboard.GetVariableNames();
	}

	public string[] GetVariableNames(Type ofType)
	{
		return _blackboard.GetVariableNames(ofType);
	}

	public string Save()
	{
		return Save(name);
	}

	public string Save(string saveKey)
	{
		string text = Serialize();
		PlayerPrefs.SetString(saveKey, text);
		return text;
	}

	public bool Load()
	{
		return Load(name);
	}

	public bool Load(string saveKey)
	{
		string @string = PlayerPrefs.GetString(saveKey);
		if (string.IsNullOrEmpty(@string))
		{
			Debug.Log("No data to load blackboard variables from key " + saveKey);
			return false;
		}
		return Deserialize(@string);
	}

	public string Serialize()
	{
		return JSONSerializer.Serialize(typeof(BlackboardSource), _blackboard, pretyJson: false, _objectReferences);
	}

	public bool Deserialize(string json)
	{
		BlackboardSource blackboardSource = JSONSerializer.Deserialize<BlackboardSource>(json, _objectReferences);
		if (blackboardSource == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, Variable> variable in blackboardSource.variables)
		{
			if (_blackboard.variables.ContainsKey(variable.Key))
			{
				_blackboard.SetValue(variable.Key, variable.Value.value);
			}
			else
			{
				_blackboard.variables[variable.Key] = variable.Value;
			}
		}
		List<string> list = new List<string>(_blackboard.variables.Keys);
		foreach (string item in list)
		{
			if (!blackboardSource.variables.ContainsKey(item))
			{
				_blackboard.variables.Remove(item);
			}
		}
		_blackboard.InitializePropertiesBinding(propertiesBindTarget, callSetter: true);
		return true;
	}
}

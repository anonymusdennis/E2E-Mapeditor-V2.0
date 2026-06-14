using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.Framework.Internal;

[Serializable]
public sealed class BlackboardSource : IBlackboard
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>(StringComparer.Ordinal);

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public Dictionary<string, Variable> variables
	{
		get
		{
			return _variables;
		}
		set
		{
			_variables = value;
		}
	}

	public GameObject propertiesBindTarget => null;

	public object this[string varName]
	{
		get
		{
			try
			{
				return variables[varName].value;
			}
			catch
			{
				return null;
			}
		}
		set
		{
			SetValue(varName, value);
		}
	}

	public event Action<Variable> onVariableAdded;

	public event Action<Variable> onVariableRemoved;

	public void InitializePropertiesBinding(GameObject targetGO, bool callSetter)
	{
		foreach (Variable value in variables.Values)
		{
			value.InitializePropertyBinding(targetGO, callSetter);
		}
	}

	public Variable AddVariable(string varName, object value)
	{
		if (value == null)
		{
			return null;
		}
		Variable variable = AddVariable(varName, value.GetType());
		if (variable != null)
		{
			variable.value = value;
		}
		return variable;
	}

	public Variable AddVariable(string varName, Type type)
	{
		if (variables.ContainsKey(varName))
		{
			return GetVariable(varName, type);
		}
		Type type2 = typeof(Variable<>).RTMakeGenericType(new Type[1] { type });
		Variable variable = (Variable)Activator.CreateInstance(type2);
		variable.name = varName;
		variables[varName] = variable;
		if (this.onVariableAdded != null)
		{
			this.onVariableAdded(variable);
		}
		return variable;
	}

	public Variable RemoveVariable(string varName)
	{
		Variable value = null;
		if (variables.TryGetValue(varName, out value))
		{
			variables.Remove(varName);
			if (this.onVariableRemoved != null)
			{
				this.onVariableRemoved(value);
			}
		}
		return value;
	}

	public T GetValue<T>(string varName)
	{
		try
		{
			return (variables[varName] as Variable<T>).value;
		}
		catch
		{
			try
			{
				return (T)variables[varName].value;
			}
			catch
			{
				if (!variables.ContainsKey(varName))
				{
					return default(T);
				}
			}
		}
		return default(T);
	}

	public Variable SetValue(string varName, object value)
	{
		try
		{
			Variable variable = variables[varName];
			variable.value = value;
			return variable;
		}
		catch
		{
			if (!variables.ContainsKey(varName))
			{
				Variable variable2 = AddVariable(varName, value);
				variable2.isProtected = true;
				return variable2;
			}
		}
		return null;
	}

	public Variable GetVariable(string varName, Type ofType = null)
	{
		if (variables != null && varName != null && variables.TryGetValue(varName, out var value) && (ofType == null || value.CanConvertTo(ofType)))
		{
			return value;
		}
		return null;
	}

	public Variable GetVariableByID(string ID)
	{
		if (variables != null && ID != null)
		{
			foreach (KeyValuePair<string, Variable> variable in variables)
			{
				if (variable.Value.ID == ID)
				{
					return variable.Value;
				}
			}
		}
		return null;
	}

	public Variable<T> GetVariable<T>(string varName)
	{
		return (Variable<T>)GetVariable(varName, typeof(T));
	}

	public string[] GetVariableNames()
	{
		return variables.Keys.ToArray();
	}

	public string[] GetVariableNames(Type ofType)
	{
		return (from v in variables.Values
			where v.CanConvertTo(ofType)
			select v.name).ToArray();
	}

	public Variable<T> AddVariable<T>(string varName, T value)
	{
		Variable<T> variable = AddVariable<T>(varName);
		variable.value = value;
		return variable;
	}

	public Variable<T> AddVariable<T>(string varName)
	{
		return (Variable<T>)AddVariable(varName, typeof(T));
	}
}

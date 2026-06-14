using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
[SpoofAOT]
public abstract class BBParameter
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private string _targetVariableID;

	[NonSerialized]
	private IBlackboard _bb;

	[NonSerialized]
	private Variable _varRef;

	private string targetVariableID
	{
		get
		{
			return _targetVariableID;
		}
		set
		{
			_targetVariableID = value;
		}
	}

	public Variable varRef
	{
		get
		{
			return _varRef;
		}
		set
		{
			if (_varRef != value)
			{
				_varRef = value;
				Bind(value);
			}
		}
	}

	public IBlackboard bb
	{
		get
		{
			return _bb;
		}
		set
		{
			if (_bb != value)
			{
				_bb = value;
				varRef = ((value == null) ? null : ResolveReference(_bb, useID: true));
			}
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			if (_name != value)
			{
				_name = value;
				varRef = ((value == null) ? null : ResolveReference(bb, useID: false));
			}
		}
	}

	public bool useBlackboard
	{
		get
		{
			return name != null;
		}
		set
		{
			if (!value)
			{
				name = null;
			}
			if (value && name == null)
			{
				name = string.Empty;
			}
		}
	}

	public bool isNone => name == string.Empty;

	public bool isNull => object.Equals(objectValue, null);

	public Type refType => (varRef == null) ? null : varRef.varType;

	public object value
	{
		get
		{
			return objectValue;
		}
		set
		{
			objectValue = value;
		}
	}

	protected abstract object objectValue { get; set; }

	public abstract Type varType { get; }

	public BBParameter()
	{
	}

	public static BBParameter CreateInstance(Type t, IBlackboard bb)
	{
		if (t == null)
		{
			return null;
		}
		BBParameter bBParameter = (BBParameter)Activator.CreateInstance(typeof(BBParameter<>).RTMakeGenericType(new Type[1] { t }));
		bBParameter.bb = bb;
		return bBParameter;
	}

	public static void SetBBFields(object o, IBlackboard bb)
	{
		List<BBParameter> objectBBParameters = GetObjectBBParameters(o);
		for (int i = 0; i < objectBBParameters.Count; i++)
		{
			objectBBParameters[i].bb = bb;
		}
	}

	public static List<BBParameter> GetObjectBBParameters(object o)
	{
		List<BBParameter> list = new List<BBParameter>();
		if (o == null)
		{
			return list;
		}
		FieldInfo[] array = o.GetType().RTGetFields();
		foreach (FieldInfo fieldInfo in array)
		{
			if (typeof(BBParameter).RTIsAssignableFrom(fieldInfo.FieldType))
			{
				object obj = fieldInfo.GetValue(o);
				if (obj == null && fieldInfo.FieldType != typeof(BBParameter))
				{
					obj = Activator.CreateInstance(fieldInfo.FieldType);
					fieldInfo.SetValue(o, obj);
				}
				if (obj != null)
				{
					list.Add((BBParameter)obj);
				}
			}
			else if (typeof(IList).RTIsAssignableFrom(fieldInfo.FieldType) && !fieldInfo.FieldType.IsArray && typeof(BBParameter).RTIsAssignableFrom(fieldInfo.FieldType.RTGetGenericArguments()[0]))
			{
				if (!(fieldInfo.GetValue(o) is IList list2))
				{
					continue;
				}
				for (int j = 0; j < list2.Count; j++)
				{
					object obj2 = list2[j];
					if (obj2 == null && fieldInfo.FieldType != typeof(BBParameter))
					{
						obj2 = (list2[j] = Activator.CreateInstance(fieldInfo.FieldType.RTGetGenericArguments()[0]));
					}
					if (obj2 != null)
					{
						list.Add((BBParameter)obj2);
					}
				}
			}
			else if (o is ISubParametersContainer)
			{
				BBParameter[] includeParseParameters = (o as ISubParametersContainer).GetIncludeParseParameters();
				if (includeParseParameters != null && includeParseParameters.Length > 0)
				{
					list.AddRange(includeParseParameters);
				}
			}
		}
		return list;
	}

	protected abstract void Bind(Variable data);

	private Variable ResolveReference(IBlackboard targetBlackboard, bool useID)
	{
		string text = name;
		if (text != null && text.Contains("/"))
		{
			string[] array = text.Split('/');
			targetBlackboard = GlobalBlackboard.Find(array[0]);
			text = array[1];
		}
		Variable variable = null;
		if (targetBlackboard == null)
		{
			return null;
		}
		if (useID && targetVariableID != null)
		{
			variable = targetBlackboard.GetVariableByID(targetVariableID);
		}
		if (variable == null && !string.IsNullOrEmpty(text))
		{
			variable = targetBlackboard.GetVariable(text, varType);
		}
		return variable;
	}

	public Variable PromoteToVariable(IBlackboard targetBB)
	{
		if (string.IsNullOrEmpty(name))
		{
			varRef = null;
			return null;
		}
		string varName = name;
		string text = ((targetBB == null) ? string.Empty : targetBB.name);
		if (name.Contains("/"))
		{
			string[] array = name.Split('/');
			text = array[0];
			varName = array[1];
			targetBB = GlobalBlackboard.Find(text);
		}
		if (targetBB == null)
		{
			varRef = null;
			return null;
		}
		varRef = targetBB.AddVariable(varName, varType);
		return varRef;
	}

	public override string ToString()
	{
		if (isNone)
		{
			return "<b>NONE</b>";
		}
		if (useBlackboard)
		{
			return $"<b>${name}</b>";
		}
		if (isNull)
		{
			return "<b>NULL</b>";
		}
		if (objectValue is IList)
		{
			return $"<b>{varType.FriendlyName()}</b>";
		}
		if (objectValue is IDictionary)
		{
			return $"<b>{varType.FriendlyName()}</b>";
		}
		return $"<b>{objectValue.ToStringAdvanced()}</b>";
	}
}
[Serializable]
public class BBParameter<T> : BBParameter
{
	private Func<T> getter;

	private Action<T> setter;

	[SerializeField]
	protected T _value;

	public new T value
	{
		get
		{
			if (getter != null)
			{
				return getter();
			}
			if (Application.isPlaying && base.bb != null && !string.IsNullOrEmpty(base.name))
			{
				base.varRef = base.bb.GetVariable(base.name, typeof(T));
				return (getter == null) ? default(T) : getter();
			}
			return _value;
		}
		set
		{
			if (setter != null)
			{
				setter(value);
			}
			else
			{
				if (base.isNone)
				{
					return;
				}
				if (base.bb != null && !string.IsNullOrEmpty(base.name))
				{
					base.varRef = PromoteToVariable(base.bb);
					if (setter != null)
					{
						setter(value);
					}
				}
				else
				{
					_value = value;
				}
			}
		}
	}

	protected override object objectValue
	{
		get
		{
			return value;
		}
		set
		{
			this.value = (T)value;
		}
	}

	public override Type varType => typeof(T);

	public BBParameter()
	{
	}

	public BBParameter(T value)
	{
		_value = value;
	}

	protected override void Bind(Variable variable)
	{
		if (variable == null)
		{
			getter = null;
			setter = null;
			_value = default(T);
		}
		else
		{
			BindGetter(variable);
			BindSetter(variable);
		}
	}

	private bool BindGetter(Variable variable)
	{
		if (variable is Variable<T>)
		{
			getter = (variable as Variable<T>).GetValue;
			return true;
		}
		if (variable.CanConvertTo(varType))
		{
			Func<object> func = variable.GetGetConverter(varType);
			getter = () => (T)func();
			return true;
		}
		return false;
	}

	private bool BindSetter(Variable variable)
	{
		if (variable is Variable<T>)
		{
			setter = (variable as Variable<T>).SetValue;
			return true;
		}
		if (variable.CanConvertFrom(varType))
		{
			Action<object> func = variable.GetSetConverter(varType);
			setter = delegate(T value)
			{
				func(value);
			};
			return true;
		}
		return false;
	}

	public static implicit operator BBParameter<T>(T value)
	{
		BBParameter<T> bBParameter = new BBParameter<T>();
		bBParameter.value = value;
		return bBParameter;
	}
}

using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
[SpoofAOT]
public abstract class Variable
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private string _id;

	[SerializeField]
	private bool _protected;

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
				if (this.onNameChanged != null)
				{
					this.onNameChanged(value);
				}
			}
		}
	}

	public string ID
	{
		get
		{
			if (string.IsNullOrEmpty(_id))
			{
				_id = Guid.NewGuid().ToString();
			}
			return _id;
		}
	}

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

	public bool isProtected
	{
		get
		{
			return _protected;
		}
		set
		{
			_protected = value;
		}
	}

	protected abstract object objectValue { get; set; }

	public abstract Type varType { get; }

	public abstract bool hasBinding { get; }

	public abstract string propertyPath { get; set; }

	public event Action<string> onNameChanged;

	public event Action<string, object> onValueChanged;

	public Variable()
	{
	}

	protected bool HasValueChangeEvent()
	{
		return this.onValueChanged != null;
	}

	protected void OnValueChanged(string name, object value)
	{
		this.onValueChanged(name, value);
	}

	public abstract void BindProperty(MemberInfo prop, GameObject target = null);

	public abstract void UnBindProperty();

	public abstract void InitializePropertyBinding(GameObject go, bool callSetter = false);

	public bool CanConvertTo(Type toType)
	{
		return GetGetConverter(toType) != null;
	}

	public Func<object> GetGetConverter(Type toType)
	{
		if (toType.RTIsAssignableFrom(varType))
		{
			return () => value;
		}
		if (typeof(IConvertible).RTIsAssignableFrom(toType) && typeof(IConvertible).RTIsAssignableFrom(varType))
		{
			return delegate
			{
				try
				{
					return Convert.ChangeType(value, toType);
				}
				catch
				{
					return Activator.CreateInstance(toType);
				}
			};
		}
		if (toType == typeof(Transform) && varType == typeof(GameObject))
		{
			return () => (value == null) ? null : (value as GameObject).transform;
		}
		if (toType == typeof(GameObject) && typeof(Component).RTIsAssignableFrom(varType))
		{
			return () => (value == null) ? null : (value as Component).gameObject;
		}
		if (toType == typeof(Vector3) && varType == typeof(GameObject))
		{
			return () => (value == null) ? Vector3.zero : (value as GameObject).transform.position;
		}
		if (toType == typeof(Vector3) && varType == typeof(Transform))
		{
			return () => (value == null) ? Vector3.zero : (value as Transform).position;
		}
		if (toType == typeof(Vector3) && varType == typeof(Quaternion))
		{
			return () => ((Quaternion)value).eulerAngles;
		}
		if (toType == typeof(Quaternion) && varType == typeof(Vector3))
		{
			return () => Quaternion.Euler((Vector3)value);
		}
		return null;
	}

	public bool CanConvertFrom(Type fromType)
	{
		return GetSetConverter(fromType) != null;
	}

	public Action<object> GetSetConverter(Type fromType)
	{
		if (varType.RTIsAssignableFrom(fromType))
		{
			return delegate(object o)
			{
				value = o;
			};
		}
		if (typeof(IConvertible).RTIsAssignableFrom(varType) && typeof(IConvertible).RTIsAssignableFrom(fromType))
		{
			return delegate(object o)
			{
				try
				{
					value = Convert.ChangeType(o, varType);
				}
				catch
				{
					value = Activator.CreateInstance(varType);
				}
			};
		}
		if (varType == typeof(Transform) && fromType == typeof(GameObject))
		{
			return delegate(object o)
			{
				value = ((o == null) ? null : (o as GameObject).transform);
			};
		}
		if (varType == typeof(GameObject) && typeof(Component).RTIsAssignableFrom(fromType))
		{
			return delegate(object o)
			{
				value = ((o == null) ? null : (o as Component).gameObject);
			};
		}
		if (varType == typeof(GameObject) && fromType == typeof(Vector3))
		{
			return delegate(object o)
			{
				if (value != null)
				{
					(value as GameObject).transform.position = (Vector3)o;
				}
			};
		}
		if (varType == typeof(Transform) && fromType == typeof(Vector3))
		{
			return delegate(object o)
			{
				if (value != null)
				{
					(value as Transform).position = (Vector3)o;
				}
			};
		}
		if (varType == typeof(Vector3) && fromType == typeof(Quaternion))
		{
			return delegate(object o)
			{
				value = ((Quaternion)o).eulerAngles;
			};
		}
		if (varType == typeof(Quaternion) && fromType == typeof(Vector3))
		{
			return delegate(object o)
			{
				value = Quaternion.Euler((Vector3)o);
			};
		}
		return null;
	}

	public override string ToString()
	{
		return name;
	}
}
[Serializable]
public class Variable<T> : Variable
{
	[SerializeField]
	private T _value;

	[SerializeField]
	private string _propertyPath;

	private Func<T> getter;

	private Action<T> setter;

	public override string propertyPath
	{
		get
		{
			return _propertyPath;
		}
		set
		{
			_propertyPath = value;
		}
	}

	public override bool hasBinding => !string.IsNullOrEmpty(_propertyPath);

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

	public new T value
	{
		get
		{
			return (getter == null) ? _value : getter();
		}
		set
		{
			if (HasValueChangeEvent())
			{
				if (!object.Equals(_value, value))
				{
					_value = value;
					if (setter != null)
					{
						setter(value);
					}
					OnValueChanged(base.name, value);
				}
			}
			else if (setter != null)
			{
				setter(value);
			}
			else
			{
				_value = value;
			}
		}
	}

	public T GetValue()
	{
		return value;
	}

	public void SetValue(T newValue)
	{
		value = newValue;
	}

	public override void BindProperty(MemberInfo prop, GameObject target = null)
	{
		if (prop is PropertyInfo || prop is FieldInfo)
		{
			_propertyPath = $"{prop.RTReflectedType().FullName}.{prop.Name}";
			if (target != null)
			{
				InitializePropertyBinding(target);
			}
		}
	}

	public override void UnBindProperty()
	{
		_propertyPath = null;
		getter = null;
		setter = null;
	}

	public override void InitializePropertyBinding(GameObject go, bool callSetter = false)
	{
		if (!hasBinding || !Application.isPlaying)
		{
			return;
		}
		getter = null;
		setter = null;
		int num = _propertyPath.LastIndexOf('.');
		string text = _propertyPath.Substring(0, num);
		string arg = _propertyPath.Substring(num + 1);
		Type type = ReflectionTools.GetType(text);
		if (type == null)
		{
			Debug.LogError($"Type '{text}' not found for Blackboard Variable '{base.name}' Binding", go);
			return;
		}
		PropertyInfo propertyInfo = type.RTGetProperty(arg);
		if (propertyInfo != null)
		{
			MethodInfo getMethod = propertyInfo.RTGetGetMethod();
			MethodInfo setMethod = propertyInfo.RTGetSetMethod();
			bool flag = (getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic);
			Component instance2 = ((!flag) ? go.GetComponent(type) : null);
			if (instance2 == null && !flag)
			{
				Debug.LogError($"A Blackboard Variable '{base.name}' is due to bind to a Component type that is missing '{text}'. Binding ignored");
				return;
			}
			if (propertyInfo.CanRead)
			{
				try
				{
					getter = getMethod.RTCreateDelegate<Func<T>>(instance2);
				}
				catch
				{
					getter = () => (T)getMethod.Invoke(instance2, null);
				}
			}
			else
			{
				getter = delegate
				{
					Debug.LogError($"You tried to Get a Property Bound Variable '{base.name}', but the Bound Property '{_propertyPath}' is Write Only!");
					return default(T);
				};
			}
			if (propertyInfo.CanWrite)
			{
				try
				{
					setter = setMethod.RTCreateDelegate<Action<T>>(instance2);
				}
				catch
				{
					setter = delegate(T o)
					{
						setMethod.Invoke(instance2, new object[1] { o });
					};
				}
				if (callSetter)
				{
					setter(_value);
				}
			}
			else
			{
				setter = delegate
				{
					Debug.LogError($"You tried to Set a Property Bound Variable '{base.name}', but the Bound Property '{_propertyPath}' is Read Only!");
				};
			}
			return;
		}
		FieldInfo field = type.RTGetField(arg);
		if (field != null)
		{
			Component instance = ((!field.IsStatic) ? go.GetComponent(type) : null);
			if (instance == null && !field.IsStatic)
			{
				Debug.LogError($"A Blackboard Variable '{base.name}' is due to bind to a Component type that is missing '{text}'. Binding ignored");
				return;
			}
			if (field.IsReadOnly())
			{
				T value = (T)field.GetValue(instance);
				getter = () => value;
				return;
			}
			getter = () => (T)field.GetValue(instance);
			setter = delegate(T o)
			{
				field.SetValue(instance, o);
			};
		}
		else
		{
			Debug.LogError($"A Blackboard Variable '{base.name}' is due to bind to a property/field named '{arg}' that does not exist on type '{type.FullName}'. Binding ignored");
		}
	}
}

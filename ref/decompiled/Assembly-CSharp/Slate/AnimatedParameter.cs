using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slate;

[Serializable]
public class AnimatedParameter : IAnimatableData
{
	[Serializable]
	private class SerializationMetaData
	{
		public bool enabled = true;

		public string parameterName;

		public string declaringTypeName;

		public string transformHierarchyPath;

		public ParameterType parameterType;

		public Type declaringType { get; private set; }

		public PropertyInfo property { get; private set; }

		public FieldInfo field { get; private set; }

		public Type animatedType { get; private set; }

		public void Deserialize()
		{
			declaringType = ReflectionTools.GetType(declaringTypeName);
			if (declaringType != null)
			{
				property = declaringType.RTGetProperty(parameterName);
				field = declaringType.RTGetField(parameterName);
				animatedType = ((property != null) ? property.PropertyType : ((field == null) ? null : field.FieldType));
			}
		}
	}

	public enum ParameterType
	{
		NotSet,
		Property,
		Field
	}

	private const float COMPARE_THRESHOLD = 0.01f;

	private const float PROXIMITY_TOLERANCE = 1f / 30f;

	[SerializeField]
	private string _serializedData;

	[SerializeField]
	private AnimationCurve[] _curves;

	[NonSerialized]
	private SerializationMetaData _data;

	public static readonly Type[] supportedTypes = new Type[6]
	{
		typeof(bool),
		typeof(int),
		typeof(float),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Color)
	};

	[NonSerialized]
	private object _animatableAttribute;

	[NonSerialized]
	private object _resolved;

	[NonSerialized]
	private Action<bool> setterBool;

	[NonSerialized]
	private Action<float> setterFloat;

	[NonSerialized]
	private Action<int> setterInt;

	[NonSerialized]
	private Action<Vector2> setterVector2;

	[NonSerialized]
	private Action<Vector3> setterVector3;

	[NonSerialized]
	private Action<Color> setterColor;

	[NonSerialized]
	private object lastResolvedObject;

	public bool enabled => data.enabled;

	private string serializedData
	{
		get
		{
			return _serializedData;
		}
		set
		{
			_serializedData = value;
		}
	}

	private SerializationMetaData data
	{
		get
		{
			if (_data != null)
			{
				return _data;
			}
			_data = JsonUtility.FromJson<SerializationMetaData>(serializedData);
			_data.Deserialize();
			return _data;
		}
	}

	public AnimationCurve[] curves
	{
		get
		{
			return _curves;
		}
		private set
		{
			_curves = value;
		}
	}

	public string parameterName => data.parameterName;

	public Type animatedType => data.animatedType;

	public ParameterType parameterType => data.parameterType;

	public string transformHierarchyPath => data.transformHierarchyPath;

	public Type declaringType => data.declaringType;

	public PropertyInfo property => data.property;

	public FieldInfo field => data.field;

	public bool isProperty => parameterType == ParameterType.Property;

	private object snapshot { get; set; }

	private object currentEval { get; set; }

	private Transform context { get; set; }

	public AnimatableParameterAttribute animatableAttribute
	{
		get
		{
			if (_animatableAttribute == null)
			{
				MemberInfo memberInfo = GetMemberInfo();
				if (memberInfo == null)
				{
					return null;
				}
				AnimatableParameterAttribute animatableParameterAttribute = memberInfo.RTGetAttribute<AnimatableParameterAttribute>(inherited: true);
				_animatableAttribute = ((animatableParameterAttribute == null) ? new object() : animatableParameterAttribute);
			}
			return _animatableAttribute as AnimatableParameterAttribute;
		}
	}

	public bool isExternal => animatableAttribute == null;

	private AnimationCurve curve1
	{
		get
		{
			return curves[0];
		}
		set
		{
			curves[0] = value;
		}
	}

	private AnimationCurve curve2
	{
		get
		{
			return curves[1];
		}
		set
		{
			curves[1] = value;
		}
	}

	private AnimationCurve curve3
	{
		get
		{
			return curves[2];
		}
		set
		{
			curves[2] = value;
		}
	}

	private AnimationCurve curve4
	{
		get
		{
			return curves[3];
		}
		set
		{
			curves[3] = value;
		}
	}

	public bool isValid
	{
		get
		{
			if (string.IsNullOrEmpty(serializedData) || data == null)
			{
				return false;
			}
			return (!isProperty) ? (field != null) : (property != null);
		}
	}

	public static event Action<IAnimatableData> onParameterChanged;

	public AnimatedParameter(MemberInfo member, object obj, Transform root)
	{
		if (member is PropertyInfo)
		{
			InitWithProperty((PropertyInfo)member, obj, root);
		}
		else if (member is FieldInfo)
		{
			InitWithField((FieldInfo)member, obj, root);
		}
		else
		{
			Debug.LogError("MemberInfo provided is neither Property nor Field");
		}
	}

	public void SetEnabled(bool value, object obj, float time)
	{
		if (enabled != value)
		{
			data.enabled = value;
			serializedData = JsonUtility.ToJson(data);
			if (!value && snapshot != null)
			{
				SetCurrentValue(obj, snapshot);
			}
			if (value && snapshot != null)
			{
				SetEvaluatedValues(obj, time, 0f);
			}
			NotifyChange();
		}
	}

	public MemberInfo GetMemberInfo()
	{
		if (isValid)
		{
			return (!isProperty) ? ((MemberInfo)field) : ((MemberInfo)property);
		}
		return null;
	}

	public AnimationCurve[] GetCurves()
	{
		return curves;
	}

	private void InitWithProperty(PropertyInfo targetProperty, object obj, Transform root)
	{
		if (!supportedTypes.Contains(targetProperty.PropertyType))
		{
			Debug.LogError($"Type '{targetProperty.PropertyType}' is not supported for animation");
			return;
		}
		if (!targetProperty.CanRead || !targetProperty.CanWrite)
		{
			Debug.LogError("Animated Property must be able to both read and write");
			return;
		}
		if (targetProperty.RTGetGetMethod().IsStatic)
		{
			Debug.LogError("Static Properties are not supported");
			return;
		}
		SerializationMetaData serializationMetaData = new SerializationMetaData();
		serializationMetaData.parameterType = ParameterType.Property;
		serializationMetaData.parameterName = targetProperty.Name;
		serializationMetaData.declaringTypeName = targetProperty.RTReflectedType().FullName;
		if (obj is Component && root != null)
		{
			serializationMetaData.transformHierarchyPath = CalculateTransformPath(root, (obj as Component).transform);
		}
		serializedData = JsonUtility.ToJson(serializationMetaData);
		InitializeCurves();
	}

	private void InitWithField(FieldInfo targetField, object obj, Transform root)
	{
		if (!supportedTypes.Contains(targetField.FieldType))
		{
			Debug.LogError($"Type '{targetField.FieldType}' is not supported for animation");
			return;
		}
		if (targetField.IsStatic)
		{
			Debug.LogError("Static Fields are not supported");
			return;
		}
		SerializationMetaData serializationMetaData = new SerializationMetaData();
		serializationMetaData.parameterType = ParameterType.Field;
		serializationMetaData.parameterName = targetField.Name;
		serializationMetaData.declaringTypeName = targetField.RTReflectedType().FullName;
		if (obj is Component && root != null)
		{
			serializationMetaData.transformHierarchyPath = CalculateTransformPath(root, (obj as Component).transform);
		}
		serializedData = JsonUtility.ToJson(serializationMetaData);
		InitializeCurves();
	}

	public bool CompareTo(AnimatedParameter animParam)
	{
		if (parameterName == animParam.parameterName && declaringType == animParam.declaringType && transformHierarchyPath == animParam.transformHierarchyPath)
		{
			return true;
		}
		return false;
	}

	private string CalculateTransformPath(Transform root, Transform target)
	{
		List<string> list = new List<string>();
		Transform transform = target;
		while (transform != root && transform != null)
		{
			list.Add(transform.name);
			transform = transform.parent;
		}
		list.Reverse();
		return string.Join("/", list.ToArray());
	}

	private Transform ResolveTransformPath(Transform root)
	{
		Transform transform = root;
		string[] array = transformHierarchyPath.Split('/');
		foreach (string name in array)
		{
			transform = transform.Find(name);
			if (transform == null)
			{
				return null;
			}
		}
		return transform;
	}

	private void InitializeCurves()
	{
		if (animatedType == typeof(bool))
		{
			curves = new AnimationCurve[1];
			curve1 = new AnimationCurve();
		}
		else if (animatedType == typeof(int))
		{
			curves = new AnimationCurve[1];
			curve1 = new AnimationCurve();
		}
		else if (animatedType == typeof(float))
		{
			curves = new AnimationCurve[1];
			curve1 = new AnimationCurve();
		}
		else if (animatedType == typeof(Vector2))
		{
			curves = new AnimationCurve[2];
			curve1 = new AnimationCurve();
			curve2 = new AnimationCurve();
		}
		else if (animatedType == typeof(Vector3))
		{
			curves = new AnimationCurve[3];
			curve1 = new AnimationCurve();
			curve2 = new AnimationCurve();
			curve3 = new AnimationCurve();
		}
		else if (animatedType == typeof(Color))
		{
			curves = new AnimationCurve[4];
			curve1 = new AnimationCurve();
			curve2 = new AnimationCurve();
			curve3 = new AnimationCurve();
			curve4 = new AnimationCurve();
		}
		else
		{
			Debug.LogError($"Parameter Type '{animatedType.Name}' is not supported");
		}
	}

	public void SetTransformContext(Transform context)
	{
		this.context = context;
	}

	public void SetSnapshot(object obj)
	{
		if (isValid)
		{
			snapshot = GetCurrentValue(obj);
		}
	}

	public void RestoreSnapshot(object obj)
	{
		if (isValid && enabled)
		{
			if (snapshot != null && isExternal)
			{
				SetCurrentValue(obj, snapshot);
			}
			currentEval = null;
			snapshot = null;
		}
	}

	public object ResolvedObject(object obj)
	{
		if (obj == null || obj.Equals(null))
		{
			return null;
		}
		if (snapshot != null && _resolved != null && !_resolved.Equals(null))
		{
			return _resolved;
		}
		if (obj is GameObject)
		{
			if (!string.IsNullOrEmpty(transformHierarchyPath))
			{
				Transform transform = ResolveTransformPath((obj as GameObject).transform);
				return _resolved = ((!(transform != null)) ? null : transform.GetComponent(declaringType));
			}
			return _resolved = (obj as GameObject).GetComponent(declaringType);
		}
		return _resolved = obj;
	}

	public object GetCurrentValue(object obj)
	{
		if (!isValid)
		{
			return null;
		}
		obj = ResolvedObject(obj);
		if (obj == null || obj.Equals(null))
		{
			return null;
		}
		if (obj is Transform)
		{
			Transform transform = obj as Transform;
			Vector3 vector = default(Vector3);
			if (parameterName == "localPosition")
			{
				vector = transform.localPosition;
				if (context != null && transform.parent == null)
				{
					vector = context.InverseTransformPoint(vector);
				}
				return vector;
			}
			if (parameterName == "localEulerAngles")
			{
				vector = transform.GetLocalEulerAngles();
				if (context != null && transform.parent == null)
				{
					vector -= context.GetLocalEulerAngles();
				}
				return vector;
			}
			if (parameterName == "localScale")
			{
				return transform.localScale;
			}
		}
		return (!isProperty) ? field.GetValue(obj) : property.RTGetGetMethod().Invoke(obj, null);
	}

	public void SetCurrentValue(object obj, object value)
	{
		if (!isValid)
		{
			return;
		}
		obj = ResolvedObject(obj);
		if (obj == null || obj.Equals(null))
		{
			return;
		}
		if (obj is Transform)
		{
			Transform transform = obj as Transform;
			Vector3 vector = (Vector3)value;
			if (parameterName == "localPosition")
			{
				if (context != null && transform.parent == null)
				{
					vector = context.TransformPoint(vector);
				}
				transform.localPosition = vector;
				return;
			}
			if (parameterName == "localEulerAngles")
			{
				if (context != null && transform.parent == null)
				{
					vector += context.GetLocalEulerAngles();
				}
				transform.SetLocalEulerAngles(vector);
				return;
			}
			if (parameterName == "localScale")
			{
				transform.localScale = vector;
				return;
			}
		}
		if (isProperty)
		{
			property.RTGetSetMethod().Invoke(obj, new object[1] { value });
		}
		else
		{
			field.SetValue(obj, value);
		}
	}

	public void SetEvaluatedValues(object obj, float time, float previousTime)
	{
		Internal_SetEvaluatedValues_Runtime(obj, time, previousTime);
	}

	private void Internal_SetEvaluatedValues_Runtime(object obj, float time, float previousTime)
	{
		if (!enabled || !isValid || !HasAnyKey())
		{
			return;
		}
		obj = ResolvedObject(obj);
		bool flag = lastResolvedObject != obj;
		lastResolvedObject = obj;
		if (obj == null || obj.Equals(null))
		{
			return;
		}
		if (animatedType == typeof(bool))
		{
			bool flag2 = curve1.Evaluate(time) >= 1f;
			if (isProperty)
			{
				if (setterBool == null || flag)
				{
					setterBool = property.RTGetSetMethod().RTCreateDelegate<Action<bool>>(obj);
				}
				setterBool(flag2);
			}
			else
			{
				field.SetValue(obj, flag2);
			}
		}
		else if (animatedType == typeof(float))
		{
			float num = curve1.Evaluate(time);
			if (isProperty)
			{
				if (setterFloat == null || flag)
				{
					setterFloat = property.RTGetSetMethod().RTCreateDelegate<Action<float>>(obj);
				}
				setterFloat(num);
			}
			else
			{
				field.SetValue(obj, num);
			}
		}
		else if (animatedType == typeof(int))
		{
			int num2 = (int)curve1.Evaluate(time);
			if (isProperty)
			{
				if (setterInt == null || flag)
				{
					setterInt = property.RTGetSetMethod().RTCreateDelegate<Action<int>>(obj);
				}
				setterInt(num2);
			}
			else
			{
				field.SetValue(obj, num2);
			}
		}
		else if (animatedType == typeof(Vector2))
		{
			Vector2 vector = new Vector2(curve1.Evaluate(time), curve2.Evaluate(time));
			if (isProperty)
			{
				if (setterVector2 == null || flag)
				{
					setterVector2 = property.RTGetSetMethod().RTCreateDelegate<Action<Vector2>>(obj);
				}
				setterVector2(vector);
			}
			else
			{
				field.SetValue(obj, vector);
			}
		}
		else if (animatedType == typeof(Vector3))
		{
			Vector3 vector2 = new Vector3(curve1.Evaluate(time), curve2.Evaluate(time), curve3.Evaluate(time));
			if (obj is Transform)
			{
				Transform transform = (Transform)obj;
				if (parameterName == "localPosition")
				{
					if (context != null && transform.parent == null)
					{
						vector2 = context.TransformPoint(vector2);
					}
					transform.localPosition = vector2;
					return;
				}
				if (parameterName == "localEulerAngles")
				{
					if (context != null && transform.parent == null)
					{
						vector2 += context.GetLocalEulerAngles();
					}
					transform.SetLocalEulerAngles(vector2);
					return;
				}
				if (parameterName == "localScale")
				{
					transform.localScale = vector2;
					return;
				}
			}
			if (isProperty)
			{
				if (setterVector3 == null || flag)
				{
					setterVector3 = property.RTGetSetMethod().RTCreateDelegate<Action<Vector3>>(obj);
				}
				setterVector3(vector2);
			}
			else
			{
				field.SetValue(obj, vector2);
			}
		}
		else
		{
			if (animatedType != typeof(Color))
			{
				return;
			}
			Color color = new Color(curve1.Evaluate(time), curve2.Evaluate(time), curve3.Evaluate(time), curve4.Evaluate(time));
			if (isProperty)
			{
				if (setterColor == null || flag)
				{
					setterColor = property.RTGetSetMethod().RTCreateDelegate<Action<Color>>(obj);
				}
				setterColor(color);
			}
			else
			{
				field.SetValue(obj, color);
			}
		}
	}

	public bool TryAutoKey(object obj, float time)
	{
		if (!isValid || !enabled)
		{
			return false;
		}
		if (!HasAnyKey() && !isExternal)
		{
			return false;
		}
		if (HasChanged(obj))
		{
			SetKeyCurrent(obj, time);
			return true;
		}
		return false;
	}

	public bool TryKeyIdentity(object obj, float time)
	{
		if (!HasAnyKey())
		{
			SetKeyCurrent(obj, time);
			return true;
		}
		bool result = false;
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			if (AddKey(animationCurve, time, animationCurve.Evaluate(time)))
			{
				result = true;
			}
		}
		return result;
	}

	public bool HasChanged(object obj)
	{
		object obj2 = currentEval;
		if (obj2 == null)
		{
			return false;
		}
		object currentValue = GetCurrentValue(obj);
		if (currentValue == null)
		{
			return false;
		}
		if (animatedType == typeof(bool))
		{
			return (bool)obj2 != (bool)currentValue;
		}
		if (animatedType == typeof(int))
		{
			return (int)obj2 != (int)currentValue;
		}
		if (animatedType == typeof(float))
		{
			return Mathf.Abs((float)obj2 - (float)currentValue) > 0.01f;
		}
		if (animatedType == typeof(Vector2))
		{
			return Mathf.Abs(((Vector2)obj2 - (Vector2)currentValue).magnitude) > 0.01f;
		}
		if (animatedType == typeof(Vector3))
		{
			return Mathf.Abs(((Vector3)obj2 - (Vector3)currentValue).magnitude) > 0.01f;
		}
		if (animatedType == typeof(Color))
		{
			return Mathf.Abs(((Vector4)(Color)obj2 - (Vector4)(Color)currentValue).magnitude) > 0.01f;
		}
		return !object.Equals(obj2, currentValue);
	}

	public void SetKeyCurrent(object obj, float time)
	{
		object currentValue = GetCurrentValue(obj);
		SetKey(time, currentValue);
		currentEval = currentValue;
		NotifyChange();
	}

	public void SetKey(float time, object value)
	{
		if (enabled && value != null)
		{
			if (animatedType == typeof(bool))
			{
				AddKey(curve1, time, ((bool)value) ? 1 : 0);
			}
			else if (animatedType == typeof(int))
			{
				AddKey(curve1, time, (int)value);
			}
			else if (animatedType == typeof(float))
			{
				AddKey(curve1, time, (float)value);
			}
			else if (animatedType == typeof(Vector2))
			{
				AddKey(curve1, time, ((Vector2)value).x);
				AddKey(curve2, time, ((Vector2)value).y);
			}
			else if (animatedType == typeof(Vector3))
			{
				AddKey(curve1, time, ((Vector3)value).x);
				AddKey(curve2, time, ((Vector3)value).y);
				AddKey(curve3, time, ((Vector3)value).z);
			}
			else if (animatedType == typeof(Color))
			{
				AddKey(curve1, time, ((Color)value).r);
				AddKey(curve2, time, ((Color)value).g);
				AddKey(curve3, time, ((Color)value).b);
				AddKey(curve4, time, ((Color)value).a);
			}
		}
	}

	private bool AddKey(AnimationCurve curve, float time, float value)
	{
		curve.AddKey(time, value);
		NotifyChange();
		return true;
	}

	public void RemoveKey(float time)
	{
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			for (int j = 0; j < animationCurve.keys.Length; j++)
			{
				Keyframe keyframe = animationCurve.keys[j];
				if (Mathf.Abs(keyframe.time - time) < 1f / 30f)
				{
					animationCurve.RemoveKey(j);
					break;
				}
			}
		}
		NotifyChange();
	}

	public void SetPreWrapMode(WrapMode mode)
	{
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			animationCurve.preWrapMode = mode;
		}
		NotifyChange();
	}

	public void SetPostWrapMode(WrapMode mode)
	{
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			animationCurve.postWrapMode = mode;
		}
		NotifyChange();
	}

	public bool HasAnyKey()
	{
		for (int i = 0; i < curves.Length; i++)
		{
			if (curves[i].length > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasKey(float time)
	{
		if (time >= 0f)
		{
			for (int i = 0; i < curves.Length; i++)
			{
				if (curves[i].keys.Any((Keyframe k) => Mathf.Abs(k.time - time) < 1f / 30f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetKeyNext(float time)
	{
		List<Keyframe> list = new List<Keyframe>();
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			list.AddRange(animationCurve.keys);
		}
		list = list.OrderBy((Keyframe k) => k.time).ToList();
		Keyframe keyframe = list.FirstOrDefault((Keyframe k) => k.time > time + 1f / 30f);
		return (keyframe.time != 0f || HasKey(0f)) ? keyframe.time : list.FirstOrDefault().time;
	}

	public float GetKeyPrevious(float time)
	{
		List<Keyframe> list = new List<Keyframe>();
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			list.AddRange(animationCurve.keys);
		}
		list = list.OrderBy((Keyframe k) => k.time).ToList();
		if (time == 0f)
		{
			return list.LastOrDefault().time;
		}
		Keyframe keyframe = list.LastOrDefault((Keyframe k) => k.time < time - 1f / 30f);
		return (keyframe.time != 0f || HasKey(0f)) ? keyframe.time : list.LastOrDefault().time;
	}

	public void OffsetValue(object byValue)
	{
		if (animatedType == typeof(float))
		{
			OffsetCurveValues(curve1, (float)byValue);
		}
		if (animatedType == typeof(int))
		{
			OffsetCurveValues(curve1, (int)byValue);
		}
		if (animatedType == typeof(Vector2))
		{
			OffsetCurveValues(curve1, ((Vector2)byValue).x);
			OffsetCurveValues(curve2, ((Vector2)byValue).y);
		}
		if (animatedType == typeof(Vector3))
		{
			OffsetCurveValues(curve1, ((Vector3)byValue).x);
			OffsetCurveValues(curve2, ((Vector3)byValue).y);
			OffsetCurveValues(curve3, ((Vector3)byValue).z);
		}
		if (animatedType == typeof(Color))
		{
			OffsetCurveValues(curve1, ((Color)byValue).r);
			OffsetCurveValues(curve2, ((Color)byValue).g);
			OffsetCurveValues(curve3, ((Color)byValue).b);
			OffsetCurveValues(curve4, ((Color)byValue).a);
		}
		NotifyChange();
	}

	private void OffsetCurveValues(AnimationCurve curve, float byValue)
	{
		for (int i = 0; i < curve.keys.Length; i++)
		{
			Keyframe key = curve.keys[i];
			key.value += byValue;
			curve.MoveKey(i, key);
		}
	}

	public void Reset()
	{
		AnimationCurve[] array = curves;
		foreach (AnimationCurve animationCurve in array)
		{
			animationCurve.preWrapMode = WrapMode.Once;
			animationCurve.postWrapMode = WrapMode.Once;
			animationCurve.keys = new Keyframe[0];
		}
		NotifyChange();
	}

	public void ChangeMemberType(ParameterType newType)
	{
		if (newType != parameterType)
		{
			data.parameterType = newType;
			serializedData = JsonUtility.ToJson(data);
		}
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(serializedData))
		{
			return "NOT SET!";
		}
		if (!isValid)
		{
			return $"*{data.parameterName}*";
		}
		string text = parameterName;
		if (text == "localPosition")
		{
			text = "Position";
		}
		if (text == "localEulerAngles")
		{
			text = "Rotation";
		}
		if (text == "localScale")
		{
			text = "Scale";
		}
		if (isExternal)
		{
			text = $"{text} <i>({declaringType.Name})</i>";
		}
		if (!enabled)
		{
			text += " <i>(Disabled)</i>";
		}
		return (!string.IsNullOrEmpty(transformHierarchyPath)) ? (transformHierarchyPath + "/" + StringExtensions.SplitCamelCase(text)) : StringExtensions.SplitCamelCase(text);
	}

	public string GetKeyLabel(float time)
	{
		string text = string.Empty;
		if (animatedType == typeof(bool))
		{
			text = ((!(curve1.Evaluate(time) >= 1f)) ? "false" : "true");
		}
		if (animatedType == typeof(float))
		{
			text = curve1.Evaluate(time).ToString("0.0");
		}
		if (animatedType == typeof(int))
		{
			text = curve1.Evaluate(time).ToString("0");
		}
		if (animatedType == typeof(Vector2) || animatedType == typeof(Vector3))
		{
			for (int i = 0; i < curves.Length; i++)
			{
				text += curves[i].Evaluate(time).ToString("0");
				if (i < curves.Length - 1)
				{
					text += ",";
				}
			}
		}
		if (animatedType == typeof(Color))
		{
			Color color = new Color(curve1.Evaluate(time), curve2.Evaluate(time), curve3.Evaluate(time), curve4.Evaluate(time));
			return $"<color={ColorToHex(color)}><size=14>●</size></color>";
		}
		return $"({text})";
	}

	private string ColorToHex(Color32 color)
	{
		return ("#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2")).ToLower();
	}

	private void NotifyChange()
	{
		if (AnimatedParameter.onParameterChanged != null)
		{
			AnimatedParameter.onParameterChanged(this);
		}
	}
}

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CodingJar.MultiScene;

[Serializable]
public struct RuntimeCrossSceneReference
{
	[SerializeField]
	private UniqueObject _fromObject;

	[SerializeField]
	private string _fromField;

	[SerializeField]
	private UniqueObject _toObject;

	private UnityEngine.Object _fromObjectCached;

	private UnityEngine.Object _toObjectCached;

	public UnityEngine.Object fromObject
	{
		get
		{
			if (!_fromObjectCached)
			{
				_fromObjectCached = _fromObject.Resolve();
			}
			return _fromObjectCached;
		}
	}

	public UnityEngine.Object toObject
	{
		get
		{
			if (!_toObjectCached)
			{
				_toObjectCached = _toObject.Resolve();
			}
			return _toObjectCached;
		}
	}

	public AmsSceneReference fromScene => _fromObject.scene;

	public AmsSceneReference toScene => _toObject.scene;

	public RuntimeCrossSceneReference(UniqueObject from, string fromField, UniqueObject to)
	{
		_fromObject = from;
		_fromField = fromField;
		_toObject = to;
		_fromObjectCached = null;
		_toObjectCached = null;
	}

	public override string ToString()
	{
		return $"{_fromObject}.{_fromField} => {_toObject}";
	}

	public bool IsSameSource(RuntimeCrossSceneReference other)
	{
		return other._fromObject.Equals(_fromObject) && other._fromField == _fromField;
	}

	public void SetToNull()
	{
		UnityEngine.Object @object = fromObject;
		if (!@object)
		{
			throw new ResolveException($"Cross-Scene Ref: {this}. Could not Resolve fromObject {@object}");
		}
		ResolveInternal(@object, null, _fromField, this, -1, bIsBaked: false);
	}

	public void Resolve(int outElementIndex, bool bIsBaked)
	{
		UnityEngine.Object @object = fromObject;
		if (!@object)
		{
			Debug.Log("**PRISON_INFO**  Resolve 1");
			throw new ResolveException($"Cross-Scene Ref: {this}. Could not Resolve fromObject {@object}");
		}
		UnityEngine.Object object2 = toObject;
		if (!object2)
		{
			Debug.Log("**PRISON_INFO**  Resolve 2");
			throw new ResolveException($"Cross-Scene Ref: {this}. Could not Resolve toObject {object2}");
		}
		ResolveInternal(@object, object2, _fromField, this, outElementIndex, bIsBaked);
	}

	private static void ResolveInternal(object fromObject, UnityEngine.Object toObject, string fromFieldPath, RuntimeCrossSceneReference debugThis, int outElementIndex, bool bIsBaked)
	{
		string[] array = fromFieldPath.Split('.');
		for (int i = 0; i < array.Length - 1; i++)
		{
			try
			{
				fromObject = GetObjectFromField(fromObject, array[i]);
				if (fromObject == null)
				{
					Debug.Log("**PRISON_INFO**  ResolveInternal 1");
					throw new ResolveException($"Cross-Scene Ref: {debugThis}. Could not follow path {fromFieldPath} because {array[i]} was null");
				}
				if (!fromObject.GetType().IsClass)
				{
					Debug.Log("**PRISON_INFO**  ResolveInternal 2");
					throw new ResolveException($"Cross-Scene Ref: {debugThis}. Could not follow path {fromFieldPath} because {array[i]} was not a class (probably a struct). This is unsupported.");
				}
			}
			catch (Exception ex)
			{
				Debug.Log("**PRISON_INFO**  ResolveInternal 3");
				throw new ResolveException($"Cross-Scene Ref: {debugThis}. {ex.Message}");
			}
		}
		string text = array[array.Length - 1];
		if (!GetFieldFromObject(fromObject, text, out var field, out var arrayIndex))
		{
			Debug.Log("**PRISON_INFO**  ResolveInternal 4");
			throw new ResolveException($"Cross-Scene Ref: {debugThis}. Could not parse piece of path {text} from {fromFieldPath}");
		}
		if (fromObject is FloorManager.Floor && text.Contains("m_TileSystems"))
		{
			Debug.Log("**PRISON_INFO**    Doing a TileSystem " + text + "  " + arrayIndex + "     [" + outElementIndex + "]");
		}
		AssignField(fromObject, toObject, field, arrayIndex);
		if (fromObject is FloorManager.Floor && text.Contains("m_TileSystems"))
		{
			FloorManager.Floor floor = fromObject as FloorManager.Floor;
			Debug.Log("**PRISON_INFO**    DONE a TileSystem  -> " + floor.m_TileSystems[arrayIndex]);
		}
	}

	private static bool GetFieldFromObject(object fromObject, string fromField, out FieldInfo field, out int arrayIndex)
	{
		arrayIndex = -1;
		field = null;
		string[] array = fromField.Split(',');
		string name = array[0];
		if (array.Length > 1 && !int.TryParse(array[1], out arrayIndex))
		{
			return false;
		}
		Type type = fromObject.GetType();
		while (type != null && field == null)
		{
			field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			type = type.BaseType;
		}
		return field != null;
	}

	private static object GetObjectFromField(object fromObject, string fromField)
	{
		if (!GetFieldFromObject(fromObject, fromField, out var field, out var arrayIndex))
		{
			throw new ResolveException($"Could not find Field {fromField}");
		}
		if (arrayIndex >= 0)
		{
			if (!(field.GetValue(fromObject) is IList list))
			{
				throw new ResolveException($"Expected collection of elements for property {field.Name} but field type is {field.FieldType.Name}");
			}
			if (list.Count <= arrayIndex)
			{
				throw new ResolveException($"Expected collection of at least {arrayIndex + 1} elements from property {field.Name}");
			}
			return list[arrayIndex];
		}
		return field.GetValue(fromObject);
	}

	private static void AssignField(object fromObject, UnityEngine.Object toObject, FieldInfo field, int arrayIndex)
	{
		if (arrayIndex >= 0)
		{
			if (!(field.GetValue(fromObject) is IList list))
			{
				Debug.Log("**PRISON_INFO**  AssignField   list is NULL ");
				throw new ResolveException($"Expected collection of elements for property {field.Name} but field type is {field.FieldType.Name}");
			}
			if (list.Count > arrayIndex)
			{
				try
				{
					list[arrayIndex] = toObject;
					return;
				}
				catch (Exception exception)
				{
					Debug.Log("**PRISON_INFO**  AssignField  arrayIndex=" + arrayIndex + "  list.Count=" + list.Count);
					if (list[0] != null)
					{
						Debug.Log("**PRISON_INFO**  list " + list[0].ToString());
					}
					if (toObject != null)
					{
						Debug.Log("**PRISON_INFO**  to " + toObject.name);
					}
					Debug.LogException(exception);
					return;
				}
			}
			list.Insert(arrayIndex, toObject);
		}
		else
		{
			if (!field.FieldType.IsAssignableFrom(toObject.GetType()))
			{
				throw new ResolveException($"Field {field.Name} of type {field.FieldType} is not compatible with {toObject} of type {toObject.GetType().Name}");
			}
			field.SetValue(fromObject, toObject);
		}
	}
}

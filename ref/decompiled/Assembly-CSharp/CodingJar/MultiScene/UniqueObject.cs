using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodingJar.MultiScene;

[Serializable]
public struct UniqueObject
{
	public AmsSceneReference scene;

	public string fullPath;

	public string componentName;

	public int componentIndex;

	public int editorLocalId;

	public string editorPrefabRelativePath;

	[SerializeField]
	[HideInInspector]
	private int version;

	private static int CurrentSerializedVersion = 1;

	private static List<Component> _reusableComponentsList = new List<Component>();

	private UnityEngine.Object RuntimeResolve()
	{
		Scene scene = this.scene.scene;
		if (!scene.IsValid())
		{
			return null;
		}
		GameObject gameObject = GameObjectEx.FindBySceneAndPath(scene, fullPath);
		if (!gameObject)
		{
			return null;
		}
		if (string.IsNullOrEmpty(componentName))
		{
			return gameObject;
		}
		if (version < 1)
		{
			Component component = gameObject.GetComponent(componentName);
			if (componentIndex < 0 || (bool)component)
			{
				return component;
			}
		}
		Type type = Type.GetType(componentName, throwOnError: false, ignoreCase: true);
		if (type != null)
		{
			gameObject.GetComponents(type, _reusableComponentsList);
			if (componentIndex < _reusableComponentsList.Count)
			{
				return _reusableComponentsList[componentIndex];
			}
		}
		return null;
	}

	internal UnityEngine.Object Resolve()
	{
		return RuntimeResolve();
	}

	public override string ToString()
	{
		Type type = ((!string.IsNullOrEmpty(componentName)) ? Type.GetType(componentName, throwOnError: false, ignoreCase: true) : null);
		return string.Format("{0}'{1}' ({2} #{3})", scene.name, fullPath, (type == null) ? "GameObject" : type.FullName, componentIndex);
	}
}

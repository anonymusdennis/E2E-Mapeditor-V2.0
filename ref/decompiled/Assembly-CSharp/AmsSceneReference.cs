using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[Serializable]
public struct AmsSceneReference
{
	public string editorAssetGUID;

	public string name;

	[FormerlySerializedAs("path")]
	[SerializeField]
	public string _path;

	public Scene scene
	{
		get
		{
			Scene sceneByPath = SceneManager.GetSceneByPath(editorPath);
			if (sceneByPath.IsValid())
			{
				return sceneByPath;
			}
			return SceneManager.GetSceneByPath(runtimePath);
		}
	}

	public bool isLoaded => scene.isLoaded;

	public string editorPath
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
		}
	}

	public string runtimePath
	{
		get
		{
			int num = 0;
			int num2 = _path.Length;
			if (_path.StartsWith("Assets/"))
			{
				num = "Assets/".Length;
				num2 -= num;
			}
			if (_path.EndsWith(".unity"))
			{
				num2 -= ".unity".Length;
			}
			return _path.Substring(num, num2);
		}
	}

	public AmsSceneReference(Scene scene)
		: this(scene.path)
	{
	}

	public AmsSceneReference(string scenePath)
	{
		Scene sceneByPath = SceneManager.GetSceneByPath(scenePath);
		editorAssetGUID = string.Empty;
		name = sceneByPath.name;
		_path = sceneByPath.path;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AmsSceneReference amsSceneReference))
		{
			return false;
		}
		return editorAssetGUID == amsSceneReference.editorAssetGUID;
	}

	public override int GetHashCode()
	{
		return editorAssetGUID.GetHashCode();
	}

	public bool IsValid()
	{
		return scene.IsValid();
	}
}

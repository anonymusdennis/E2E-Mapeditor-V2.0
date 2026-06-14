using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodingJar;

public static class GameObjectEx
{
	private static List<GameObject> s_GameObjects = new List<GameObject>();

	public static void ForceCleanup()
	{
		s_GameObjects.Clear();
	}

	public static string GetFullName(this GameObject gameObj)
	{
		return gameObj.transform.FullPath();
	}

	public static GameObject CreateGameObjectInScene(string name, Scene scene)
	{
		Scene activeScene = SceneManager.GetActiveScene();
		SceneManager.SetActiveScene(scene);
		GameObject result = new GameObject(name);
		SceneManager.SetActiveScene(activeScene);
		return result;
	}

	public static T GetSceneSingleton<T>(this Scene scene, bool bCreate) where T : MonoBehaviour
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			T component = gameObject.GetComponent<T>();
			if ((bool)component)
			{
				return component;
			}
		}
		if (bCreate)
		{
			GameObject gameObject2 = CreateGameObjectInScene("! " + typeof(T).Name, scene);
			return gameObject2.AddComponent<T>();
		}
		return (T)null;
	}

	public static T GetRequiredComponent<T>(this GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if ((bool)component)
		{
			return component;
		}
		return gameObject.AddComponent<T>();
	}

	public static GameObject FindByAbsolutePath(string absolutePath)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject gameObject = FindBySceneAndPath(SceneManager.GetSceneAt(i), absolutePath);
			if ((bool)gameObject)
			{
				return gameObject;
			}
		}
		return null;
	}

	public static GameObject FindBySceneAndPath(Scene scene, string absolutePath)
	{
		string[] array = absolutePath.Split('/');
		int num = array.Length;
		scene.GetRootGameObjects(s_GameObjects);
		foreach (GameObject s_GameObject in s_GameObjects)
		{
			if (s_GameObject.name != array[1])
			{
				continue;
			}
			Transform transform = s_GameObject.transform;
			for (int i = 2; i < num; i++)
			{
				if (!transform)
				{
					break;
				}
				transform = transform.Find(array[i]);
			}
			if (!transform)
			{
				continue;
			}
			return transform.gameObject;
		}
		return null;
	}
}

using System;
using UnityEngine;

public static class T17NetHelpers
{
	public static bool NetViewIsMine(this GameObject go)
	{
		bool result = true;
		T17NetView component = go.GetComponent<T17NetView>();
		if (component != null)
		{
			result = component.isMine;
		}
		return result;
	}

	public static void SetNetViewSynchronization(this GameObject go, T17NetViewSynchronization netSynchronizationMode)
	{
		T17NetView component = go.GetComponent<T17NetView>();
		if (component != null)
		{
			component.NetViewSynchronization = netSynchronizationMode;
			return;
		}
		Debug.LogErrorFormat(go, "GameObject.SetNetViewID - Missing NetView component on {0}", go.name);
	}

	public static void SetNetViewID(this GameObject go, int netViewID)
	{
		T17NetView component = go.GetComponent<T17NetView>();
		if (component != null)
		{
			component.viewID = netViewID;
			return;
		}
		Debug.LogErrorFormat(go, "GameObject.SetNetViewID - Missing NetView component on {0}", go.name);
	}

	public static void SetNetViewIDs(this GameObject go, int[] viewsIDs)
	{
		T17NetView[] componentsInChildren = go.GetComponentsInChildren<T17NetView>(includeInactive: true);
		if (componentsInChildren.Length != viewsIDs.Length)
		{
			return;
		}
		for (int i = 0; i < viewsIDs.Length; i++)
		{
			bool flag = componentsInChildren[i].viewID == 0;
			componentsInChildren[i].viewID = viewsIDs[i];
			if (flag && viewsIDs[i] != 0)
			{
				componentsInChildren[i].EnsureRegistered();
			}
		}
	}

	private static void SendNetViewIDs(this GameObject go, bool allocateChildSceneViewIDs = true)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		T17NetView[] componentsInChildren = go.GetComponentsInChildren<T17NetView>(includeInactive: true);
		if (componentsInChildren.Length <= 1)
		{
			return;
		}
		int[] array = new int[componentsInChildren.Length];
		array[0] = componentsInChildren[0].viewID;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if (allocateChildSceneViewIDs)
			{
				componentsInChildren[i].viewID = T17NetManager.AllocateSceneViewID();
				componentsInChildren[i].EnsureRegistered();
			}
			array[i] = componentsInChildren[i].viewID;
		}
		T17NetworkManager.GetInstance().SendNetViewIDs(array[0], array);
	}

	public static GameObject Instantiate(int netViewID, UnityEngine.Object original, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject;
		try
		{
			gameObject = UnityEngine.Object.Instantiate(original, position, rotation) as GameObject;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			gameObject = null;
		}
		if (gameObject != null)
		{
			gameObject.SetNetViewID(netViewID);
			gameObject.SendNetViewIDs();
		}
		return gameObject;
	}

	internal static void RequestOwnership(this GameObject go)
	{
		T17NetView[] componentsInChildren = go.GetComponentsInChildren<T17NetView>(includeInactive: true);
		foreach (T17NetView t17NetView in componentsInChildren)
		{
			t17NetView.RequestOwnership();
		}
	}
}

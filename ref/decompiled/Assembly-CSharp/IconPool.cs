using System.Collections.Generic;
using UnityEngine;

public class IconPool : MonoBehaviour
{
	public GameObject m_PooledObject;

	public List<GameObject> m_ActiveObjects = new List<GameObject>();

	public List<GameObject> m_FreeObjects = new List<GameObject>();

	public int m_InitNumObjects = 30;

	private void Start()
	{
		if (m_FreeObjects.Count == 0 && m_ActiveObjects.Count == 0)
		{
			for (int i = 0; i < m_InitNumObjects; i++)
			{
				m_FreeObjects.Add(CreateObject());
			}
		}
	}

	public GameObject GetObject()
	{
		if (m_FreeObjects.Count > 0)
		{
			m_FreeObjects[0].SetActive(value: true);
			m_ActiveObjects.Add(m_FreeObjects[0]);
			m_FreeObjects.RemoveAt(0);
		}
		else
		{
			m_ActiveObjects.Add(CreateObject());
			m_ActiveObjects[m_ActiveObjects.Count - 1].SetActive(value: true);
		}
		return m_ActiveObjects[m_ActiveObjects.Count - 1];
	}

	public void FreeObject(GameObject obj)
	{
		obj.SetActive(value: false);
		if (!m_FreeObjects.Contains(obj))
		{
			m_FreeObjects.Add(obj);
			m_ActiveObjects.Remove(obj);
		}
	}

	public void FreeAllObjects()
	{
		for (int i = 0; i < m_ActiveObjects.Count; i++)
		{
			GameObject gameObject = m_ActiveObjects[i];
			gameObject.SetActive(value: false);
			if (!m_FreeObjects.Contains(gameObject))
			{
				m_FreeObjects.Add(gameObject);
			}
		}
		m_ActiveObjects.Clear();
	}

	private GameObject CreateObject()
	{
		GameObject gameObject = Object.Instantiate(m_PooledObject);
		gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		gameObject.SetActive(value: false);
		return gameObject;
	}
}

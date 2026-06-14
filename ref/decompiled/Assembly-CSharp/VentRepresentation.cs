using UnityEngine;

public class VentRepresentation : BaseLevelEditorKeepers
{
	public GameObject m_PrefabToCreate;

	private GameObject m_CreatedPrefab;

	public override AfterSetup Setup()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (base.transform.parent != null && instance != null && m_PrefabToCreate != null)
		{
			float z = base.transform.parent.transform.position.z;
			int num = (int)(instance.WhatLayerIsThisZIn(z) - 1);
			m_CreatedPrefab = Object.Instantiate(m_PrefabToCreate, instance.m_BuildingLayers[num].m_Walls.transform);
			if (m_CreatedPrefab != null)
			{
				Vector3 localPosition = base.transform.localPosition;
				localPosition.z += -0.01f;
				m_CreatedPrefab.transform.localPosition = localPosition;
			}
		}
		return AfterSetup.Disable;
	}

	public virtual void OnDestroy()
	{
		if (m_CreatedPrefab != null)
		{
			m_CreatedPrefab.SetActive(value: false);
			Object.Destroy(m_CreatedPrefab);
		}
	}
}

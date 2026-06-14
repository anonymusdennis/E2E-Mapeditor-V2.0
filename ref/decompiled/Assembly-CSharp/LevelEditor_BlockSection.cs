using System;
using UnityEngine;

public class LevelEditor_BlockSection : MonoBehaviour
{
	private LevelEditor_GridCellPopulator m_gridCellPopulator;

	private Transform m_gcpTransform;

	public LevelEditor_GridCellPopulator GridCellPopulator => m_gridCellPopulator;

	private void Awake()
	{
		m_gridCellPopulator = GetComponentInChildren<LevelEditor_GridCellPopulator>();
		if (m_gridCellPopulator != null)
		{
			LevelEditor_GridCellPopulator gridCellPopulator = m_gridCellPopulator;
			gridCellPopulator.OnCellsUpdated = (LevelEditor_GridCellPopulator.LevelEditor_GridCellPopulatorDelegate)Delegate.Combine(gridCellPopulator.OnCellsUpdated, new LevelEditor_GridCellPopulator.LevelEditor_GridCellPopulatorDelegate(UpdateActiveState));
		}
	}

	public void UpdateContent()
	{
		if (m_gridCellPopulator != null)
		{
			m_gridCellPopulator.UpdateCells();
		}
	}

	public void UpdateActiveState(LevelEditor_GridCellPopulator gridCellPopulator)
	{
		if (m_gridCellPopulator == null || !object.ReferenceEquals(gridCellPopulator, m_gridCellPopulator))
		{
			return;
		}
		if (m_gcpTransform == null)
		{
			m_gcpTransform = m_gridCellPopulator.transform;
		}
		bool active = false;
		int i = 0;
		for (int childCount = m_gcpTransform.childCount; i < childCount; i++)
		{
			if (m_gcpTransform.GetChild(i).gameObject.activeSelf)
			{
				active = true;
				break;
			}
		}
		base.gameObject.SetActive(active);
	}
}

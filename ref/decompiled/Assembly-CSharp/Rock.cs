using System;
using UnityEngine;

public class Rock : MonoBehaviour
{
	public Material[] m_RandomMaterials;

	private bool m_RandomlySpawned;

	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private int m_TileFloor = -1;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public int TileFloor => m_TileFloor;

	public bool RandomlySpawned
	{
		get
		{
			return m_RandomlySpawned;
		}
		set
		{
			m_RandomlySpawned = value;
		}
	}

	private void Start()
	{
		m_TileFloor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z).m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Wall, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			throw new Exception("Unable to find tile row/column");
		}
		FloorManager.GetInstance().AddRock(this);
		Vector3 position = base.transform.position;
		base.transform.position = new Vector3(position.x, position.y, position.z - 0.05f);
		FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(FloorManager.GetInstance().FindFloorbyIndex(m_TileFloor));
		if (floor.m_FloorIndex != m_TileFloor)
		{
			Hole hole = FloorManager.GetInstance().GetHole(m_TileRow + -1, m_TileColumn, floor.m_FloorIndex);
			if (hole != null)
			{
				hole.HasRockUnderneath = true;
			}
		}
		SetMaterial();
		CullingObjectCollector.GetInstance().Runtime_AddToBucket(GetComponent<MeshRenderer>(), bCheckForMaterialBlock: false, bAlsoFloorsAbove: true);
	}

	protected virtual void OnDestroy()
	{
		if (CullingObjectCollector.GetInstance() != null)
		{
			CullingObjectCollector.GetInstance().Runtime_RemoveFromBucket(GetComponent<MeshRenderer>());
		}
	}

	private void SetMaterial()
	{
		if (m_RandomMaterials != null && m_RandomMaterials.Length > 0)
		{
			MeshRenderer component = GetComponent<MeshRenderer>();
			Material material = m_RandomMaterials[UnityEngine.Random.Range(0, m_RandomMaterials.Length)];
			if (component != null && material != null)
			{
				component.sharedMaterial = material;
			}
		}
	}
}

using System;
using UnityEngine;

public class TunnelBrace : MonoBehaviour
{
	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private int m_TileFloor = -1;

	private const float FLOOR_OFFSET = -0.25f;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public int TileFloor => m_TileFloor;

	private void Start()
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
		if (!floor.IsUnderGround())
		{
			throw new Exception("Tunnel braces should only be within the underground layer");
		}
		m_TileFloor = floor.m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Wall, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			throw new Exception("Unable to find tile row/column");
		}
		FloorManager.GetInstance().AddTunnelBrace(this);
		if (LevelScript.GetInstance().m_Processed)
		{
			float zOffset = LayerHelper.GetZOffset(base.transform);
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z += zOffset;
			localPosition.z += -0.25f;
			base.transform.localPosition = localPosition;
		}
		else
		{
			Vector3 position = base.transform.position;
			base.transform.position = new Vector3(position.x, position.y, position.z + -0.25f);
		}
	}
}

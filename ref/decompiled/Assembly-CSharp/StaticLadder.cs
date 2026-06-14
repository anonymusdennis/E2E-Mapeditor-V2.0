using System;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(NodeLink))]
public class StaticLadder : MonoBehaviour
{
	public bool m_bOmniDirectionalEnter;

	public bool m_bTransitionsToFromVent;

	public Directionx4 m_ExitDirection = Directionx4.Down;

	private int m_TileRow = -1;

	private int m_TileColumn = -1;

	private int m_TileFloor = -1;

	private int m_EndTileFloor = -1;

	private bool m_DownwardTransition = true;

	private int m_NumFloorTransitions;

	public int TileRow => m_TileRow;

	public int TileColumn => m_TileColumn;

	public int TileFloor => m_TileFloor;

	public bool DownwardTransition => m_DownwardTransition;

	public int NumFloorTransitions => m_NumFloorTransitions;

	private void Start()
	{
		m_TileFloor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z).m_FloorIndex;
		if (!FloorManager.GetInstance().GetTileGridPoint(m_TileFloor, FloorManager.TileSystem_Type.TileSystem_Wall, base.transform.position, out m_TileRow, out m_TileColumn))
		{
			throw new Exception("Unable to find tile row/column");
		}
		NodeLink component = GetComponent<NodeLink>();
		if (component == null || component.End == null)
		{
			throw new Exception("No end point defined");
		}
		m_EndTileFloor = FloorManager.GetInstance().FindFloorAtZ(component.End.position.z).m_FloorIndex;
		m_DownwardTransition = m_TileFloor > m_EndTileFloor;
		m_NumFloorTransitions = Mathf.Abs(m_TileFloor - m_EndTileFloor);
		FloorManager.GetInstance().AddStaticLadder(this);
	}
}

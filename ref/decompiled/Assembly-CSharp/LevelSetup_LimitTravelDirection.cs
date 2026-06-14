using System;
using Pathfinding;
using UnityEngine;

public class LevelSetup_LimitTravelDirection : BaseComponentSetup
{
	public enum gridDirection
	{
		Down,
		Right,
		Up,
		Left,
		Down_Right,
		Up_Right,
		Up_Left,
		Down_Left,
		CENTER
	}

	public bool m_LimitHorizontal = true;

	private GridNode m_GridNode;

	private Vector3 m_Position = Vector3.zero;

	private static float[] m_gridDirectionOffset_X = new float[8] { 0f, 1f, 0f, -1f, 1f, 1f, -1f, -1f };

	private static float[] m_gridDirectionOffset_Y = new float[8] { -1f, 0f, 1f, 0f, -1f, 1f, 1f, -1f };

	private static gridDirection[] m_gridInverseDirections = new gridDirection[8]
	{
		gridDirection.Up,
		gridDirection.Left,
		gridDirection.Down,
		gridDirection.Right,
		gridDirection.Up_Left,
		gridDirection.Down_Left,
		gridDirection.Down_Right,
		gridDirection.Up_Right
	};

	protected virtual void OnDestroy()
	{
		AstarPath.OnLatePostScan = (OnScanDelegate)Delegate.Remove(AstarPath.OnLatePostScan, new OnScanDelegate(OnScanDelegate));
	}

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_3;
	}

	public override SetupReturnState Setup()
	{
		AstarPath.OnLatePostScan = (OnScanDelegate)Delegate.Combine(AstarPath.OnLatePostScan, new OnScanDelegate(OnScanDelegate));
		return Finished();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	public void OnScanDelegate(AstarPath script)
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			UpdateNodes();
		});
	}

	public void UpdateNodes()
	{
		m_Position = base.transform.position;
		m_GridNode = NavMeshUtil.GetNearestGraphNode(m_Position) as GridNode;
		if (m_GridNode != null)
		{
			if (m_LimitHorizontal)
			{
				AdjustDirection(gridDirection.CENTER, gridDirection.Down_Left, gridDirection.Down_Right, gridDirection.Up_Left, gridDirection.Up_Right, gridDirection.Left, gridDirection.Right);
			}
			else
			{
				AdjustDirection(gridDirection.CENTER, gridDirection.Down_Left, gridDirection.Down_Right, gridDirection.Up_Left, gridDirection.Up_Right, gridDirection.Up, gridDirection.Down);
			}
			AdjustDirection(gridDirection.Left, gridDirection.Down_Right, gridDirection.Up_Right);
			AdjustDirection(gridDirection.Right, gridDirection.Down_Left, gridDirection.Up_Left);
			AdjustDirection(gridDirection.Up, gridDirection.Down_Left, gridDirection.Down_Right);
			AdjustDirection(gridDirection.Down, gridDirection.Up_Left, gridDirection.Up_Right);
		}
	}

	private void AdjustDirection(gridDirection startpoint, params gridDirection[] directions)
	{
		GridNode gridNode = null;
		Vector3 position = m_Position;
		if (startpoint == gridDirection.CENTER)
		{
			gridNode = m_GridNode;
		}
		else
		{
			if (!m_GridNode.GetConnectionInternal((int)startpoint))
			{
				return;
			}
			position.x += m_gridDirectionOffset_X[(int)startpoint];
			position.y += m_gridDirectionOffset_Y[(int)startpoint];
			gridNode = NavMeshUtil.GetNearestGraphNode(position) as GridNode;
			if (gridNode == null)
			{
				return;
			}
		}
		for (int num = directions.Length - 1; num >= 0; num--)
		{
			int num2 = (int)directions[num];
			if (gridNode.GetConnectionInternal(num2))
			{
				gridNode.SetConnectionInternal(num2, value: false);
				if (NavMeshUtil.GetNearestGraphNode(position + new Vector3(m_gridDirectionOffset_X[num2], m_gridDirectionOffset_Y[num2], 0f)) is GridNode gridNode2)
				{
					gridNode2.SetConnectionInternal((int)m_gridInverseDirections[num2], value: false);
				}
			}
		}
	}
}

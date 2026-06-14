using Pathfinding;
using UnityEngine;

public class PlayerPathing : MonoBehaviour
{
	public Transform m_Transform;

	public Player m_Player;

	private int m_KeyAccess = 1;

	public void AddKeyColour(KeyFunctionality.KeyColour keyColor)
	{
		m_KeyAccess |= GetKeyTag(keyColor);
	}

	public static int GetKeyTag(KeyFunctionality.KeyColour keyColor)
	{
		return keyColor switch
		{
			KeyFunctionality.KeyColour.Black => 8, 
			KeyFunctionality.KeyColour.Cyan => 16, 
			KeyFunctionality.KeyColour.Red => 32, 
			KeyFunctionality.KeyColour.Green => 64, 
			KeyFunctionality.KeyColour.Yellow => 128, 
			KeyFunctionality.KeyColour.Purple => 256, 
			KeyFunctionality.KeyColour.Silver => 512, 
			KeyFunctionality.KeyColour.Solitary => 1024, 
			_ => 0, 
		};
	}

	public void ClearDoors()
	{
		m_KeyAccess = 1;
	}

	public bool PathToDestination(Vector3 targetPosition, T17_ABPath.ArrowPathCallback arrowPathFound, T17_ABPath.PathCallback arrowPathFailed = null, bool bCheckedLockedDoors = false)
	{
		T17_ABPath t17_ABPath = T17_ABPath.Construct(targetPosition, base.transform.position, OnPathComplete);
		t17_ABPath.path_OnArrowTargetFound = arrowPathFound;
		t17_ABPath.path_OnPathCancelled = arrowPathFailed;
		t17_ABPath.calculatePartial = true;
		if (bCheckedLockedDoors)
		{
			t17_ABPath.enabledTags = m_KeyAccess;
		}
		AstarPath.StartPath(t17_ABPath);
		return true;
	}

	public void OnPathComplete(Path p)
	{
		if (p == null)
		{
			return;
		}
		T17_ABPath t17_ABPath = (T17_ABPath)p;
		if (p.error)
		{
			if (t17_ABPath.path_OnPathCancelled != null)
			{
				t17_ABPath.path_OnPathCancelled();
			}
		}
		else
		{
			if (p.path == null || p.vectorPath == null || p.path.Count != p.vectorPath.Count)
			{
				return;
			}
			p.Claim(this);
			if (p.path.Count > 0)
			{
				bool pathChangesFloor = false;
				Vector3 targetPosition = p.vectorPath[p.vectorPath.Count - 1];
				GraphNode graphNode = p.path[p.path.Count - 1];
				if (graphNode != null)
				{
					uint graphIndex = graphNode.GraphIndex;
					for (int num = p.path.Count - 1; num >= 0; num--)
					{
						if (p.path[num] != null)
						{
							if (graphIndex != p.path[num].GraphIndex)
							{
								pathChangesFloor = true;
								break;
							}
							targetPosition = p.vectorPath[num];
						}
					}
				}
				if (t17_ABPath.path_OnArrowTargetFound != null)
				{
					t17_ABPath.path_OnArrowTargetFound(targetPosition, pathChangesFloor);
				}
			}
			p.Release(this);
		}
	}
}

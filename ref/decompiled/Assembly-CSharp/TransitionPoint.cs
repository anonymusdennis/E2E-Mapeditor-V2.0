using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
	private static List<TransitionPoint> s_TransitionPointList = new List<TransitionPoint>();

	public GameObject m_Partner;

	public NodeLink m_AINodeLink;

	[ReadOnly]
	public bool m_bHasMoved;

	[ReadOnly]
	public Vector3 m_TransitionPosition;

	private bool m_bExitNode;

	private Transform m_HighlightPosition;

	public static TransitionPoint FindClosest(Vector3 position, float maxRadius)
	{
		TransitionPoint result = null;
		float num = float.MaxValue;
		int i = 0;
		for (int count = s_TransitionPointList.Count; i < count; i++)
		{
			if (!(s_TransitionPointList[i].m_Partner == null) && !(Mathf.Abs(position.z - s_TransitionPointList[i].m_TransitionPosition.z) >= 0.1f))
			{
				float num2 = Mathf.Abs((position - s_TransitionPointList[i].m_TransitionPosition).sqrMagnitude);
				if (num2 <= maxRadius && num2 < num)
				{
					num = num2;
					result = s_TransitionPointList[i];
				}
			}
		}
		return result;
	}

	private void Start()
	{
		m_TransitionPosition = base.transform.position;
		m_bExitNode = m_Partner == null;
		if (!s_TransitionPointList.Find((TransitionPoint x) => x == this))
		{
			s_TransitionPointList.Add(this);
		}
		if (m_HighlightPosition == null)
		{
			m_HighlightPosition = base.transform.GetChild(0);
		}
	}

	public static void Cleanup()
	{
		if (s_TransitionPointList != null)
		{
			s_TransitionPointList.Clear();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (m_bExitNode)
		{
			return;
		}
		Character componentInParent = other.GetComponentInParent<Character>();
		if (componentInParent != null && !componentInParent.IsCarryingObject() && !componentInParent.m_bIsSearchingDesk && !componentInParent.IsInteracting() && componentInParent.m_CharacterStats.m_bIsPlayer && componentInParent.m_NetView.isMine)
		{
			Vector3 vector = m_TransitionPosition - componentInParent.transform.position;
			vector = m_Partner.transform.position - vector;
			if (componentInParent.Teleport(vector))
			{
			}
		}
	}

	public Vector3 GetHighlightPosition()
	{
		if (m_HighlightPosition != null)
		{
			return m_HighlightPosition.position;
		}
		return m_TransitionPosition;
	}

	public void OnDrawGizmos()
	{
		if (m_HighlightPosition == null)
		{
			m_HighlightPosition = base.transform.GetChild(0);
		}
		if (m_HighlightPosition != null)
		{
			Gizmos.DrawIcon(m_HighlightPosition.position - new Vector3(0f, 0f, 1.5f), "Gizmo_Stairs.png", allowScaling: true);
		}
		else
		{
			Gizmos.DrawIcon(base.transform.position - new Vector3(0f, 0f, 1.5f), "Gizmo_Stairs.png", allowScaling: true);
		}
	}
}

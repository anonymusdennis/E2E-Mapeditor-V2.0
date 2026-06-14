using Pathfinding;
using UnityEngine;

public class GhostNode : T17MonoBehaviour
{
	private BoxCollider[] m_Colliders;

	private void Start()
	{
		m_Colliders = GetComponents<BoxCollider>();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		uint keyTag = (uint)AIMovement.GetKeyTag(KeyFunctionality.KeyColour.Ghost);
		GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(base.transform.position);
		NavMeshUtil.SetNodeTag(nearestGraphNode, keyTag);
		return base.StartInit();
	}

	public void EnableColliders(bool bOn)
	{
		if (m_Colliders != null)
		{
			int num = m_Colliders.Length;
			for (int i = 0; i < num; i++)
			{
				m_Colliders[i].enabled = bOn;
			}
		}
	}
}

using UnityEngine;
using UnityEngine.Serialization;

public class LevelSetup_ObjectMover : BaseComponentSetup
{
	public BoxCollider m_Collider;

	public Vector3 m_ColliderChange = Vector3.zero;

	[FormerlySerializedAs("m_GameObject")]
	public GameObject m_TheGameObject;

	public Vector3 m_GameObjectChange = Vector3.zero;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_1;
	}

	public override SetupReturnState Setup()
	{
		if (m_TheGameObject != null)
		{
			Vector3 localPosition = m_TheGameObject.transform.localPosition;
			localPosition += m_GameObjectChange;
			m_TheGameObject.transform.localPosition = localPosition;
		}
		if (m_Collider != null)
		{
			Vector3 center = m_Collider.center;
			center += m_ColliderChange;
			m_Collider.center = center;
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

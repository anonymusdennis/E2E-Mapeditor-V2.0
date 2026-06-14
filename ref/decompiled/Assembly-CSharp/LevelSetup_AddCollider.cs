using UnityEngine;

public class LevelSetup_AddCollider : BaseComponentSetup
{
	public bool m_IsTrigger = true;

	public Vector3 m_Size = Vector2.one;

	public Vector3 m_Center = Vector2.zero;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_3;
	}

	public override SetupReturnState Setup()
	{
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = m_Center;
		boxCollider.size = m_Size;
		boxCollider.isTrigger = m_IsTrigger;
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

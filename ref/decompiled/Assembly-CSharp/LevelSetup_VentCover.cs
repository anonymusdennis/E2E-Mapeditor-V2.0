using UnityEngine;

public class LevelSetup_VentCover : BaseComponentSetup
{
	public GameObject m_VentShadowPrefab;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_2;
	}

	public override SetupReturnState Setup()
	{
		if (m_VentShadowPrefab == null)
		{
			Debug.LogError("LevelSetup_VentCover - Failed, no shadow prefab!");
			return FinishedAndRemove();
		}
		if (BaseComponentSetup.m_FloorManager == null)
		{
			BaseComponentSetup.m_FloorManager = GetClassInstance<FloorManager>();
		}
		if (BaseComponentSetup.m_FloorManager == null)
		{
			Debug.LogError("LevelSetup_VentCover - Failed to find floor manager!");
			return FinishedAndRemove();
		}
		FloorManager.Floor floor = BaseComponentSetup.m_FloorManager.FindFloorAtZ(base.transform.position.z);
		if (floor != null)
		{
			FloorManager.Floor floor2 = BaseComponentSetup.m_FloorManager.DownAFloor(floor);
			if (floor2 != null)
			{
				GameObject gameObject = Object.Instantiate(m_VentShadowPrefab, base.transform);
				if (gameObject != null)
				{
					gameObject.layer = LayerMask.NameToLayer("Floor");
					Vector3 position = base.transform.position;
					position.z = floor2.m_zPos;
					gameObject.transform.position = position;
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

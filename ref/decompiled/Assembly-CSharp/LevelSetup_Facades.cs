using UnityEngine;

public class LevelSetup_Facades : BaseComponentSetup
{
	public Material m_FacadeMaterial;

	public int m_BlockID = -1;

	public override SetupReturnState Setup()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		GameObject facadePrefab = BuildingBlockManager.GetInstance().m_FacadePrefab;
		if (facadePrefab == null)
		{
			return FinishedAndRemove();
		}
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.Roof;
		if (!GetLayerAndPosition(ref X, ref Y, ref layer))
		{
		}
		Y = 119 - Y;
		int num = Y * 120 + X;
		bool flag = false;
		if (!instance.m_VentLayers[(uint)layer] && layer != BaseLevelManager.LevelLayers.Roof && (instance.m_BuildingLayers[(uint)(layer + 2)].m_TileProperties[num] & BaseLevelManager.TileProperty.TileMask) == 0 && (instance.m_BuildingLayers[(uint)(layer + 2)].m_TileProperties[num + 120] & BaseLevelManager.TileProperty.TileMask) != 0)
		{
			flag = true;
		}
		if (flag)
		{
			GameObject walls = instance.m_BuildingLayers[(uint)(layer + 2)].m_Walls;
			if (!(walls == null))
			{
				GameObject gameObject = Object.Instantiate(facadePrefab, walls.transform);
				if (gameObject != null)
				{
					Vector3 localPosition = base.transform.localPosition;
					localPosition.z = 0f;
					gameObject.transform.localPosition = localPosition;
					gameObject.transform.localScale = Vector3.one;
					MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
					if (component != null)
					{
						component.material = m_FacadeMaterial;
					}
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}
}

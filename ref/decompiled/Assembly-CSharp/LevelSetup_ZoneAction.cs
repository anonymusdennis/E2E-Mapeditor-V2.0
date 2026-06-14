using System;
using UnityEngine;

public class LevelSetup_ZoneAction : BaseComponentSetup
{
	public enum SetupActionEnum
	{
		Nothing,
		DisableTag
	}

	[Serializable]
	public class SetupActions
	{
		public SetupActionEnum m_Action;
	}

	[Header("If this object is not in this zone")]
	public ZoneDetailsManager.ZoneTypes m_ZoneType;

	[Header("Perform these actions")]
	public SetupActions[] m_Actions = new SetupActions[0];

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		int iIndex = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL;
		if (GetLayerAndZoneMapIndex(ref iIndex, ref layer, FloorManager.TileSystem_Type.TileSystem_ObjectPlops))
		{
			bool flag = false;
			LevelEditor_ZoneManager.ZoneMap zoneMap = instance.GetZoneMap(layer);
			if (zoneMap != null)
			{
				int num = zoneMap.m_Map[iIndex];
				if (num == -1 && m_ZoneType != 0)
				{
					flag = true;
				}
				if (num != -1)
				{
					LevelEditor_ZoneManager.Zone zone = instance.GetZone(num);
					if (zone != null && zone.m_ZoneType != m_ZoneType)
					{
						flag = true;
					}
				}
				if (flag)
				{
					int num2 = m_Actions.Length;
					for (int i = 0; i < num2; i++)
					{
						if (m_Actions[i] == null)
						{
							continue;
						}
						SetupActionEnum action = m_Actions[i].m_Action;
						if (action != 0 && action == SetupActionEnum.DisableTag)
						{
							NetObjectLock component = GetComponent<NetObjectLock>();
							if (component != null)
							{
								component.SetProximityDetectorExternalOverride(bVisible: false);
							}
						}
					}
				}
			}
		}
		return FinishedAndRemove();
	}
}

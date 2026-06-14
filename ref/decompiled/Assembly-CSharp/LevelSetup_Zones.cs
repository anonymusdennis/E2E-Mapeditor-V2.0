using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_Zones : BaseComponentSetup
{
	public class ItemAdded
	{
		public int m_InstanceID;

		public int m_Count;

		public ItemAdded(int iID, int iCount)
		{
			m_InstanceID = iID;
			m_Count = iCount;
		}
	}

	public const int INVALID_ADDOBJECT = -1;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_0_First;
	}

	public override SetupReturnState Setup()
	{
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		if (!(instance == null))
		{
			instance.ValidateAllZones();
			LevelDetailsManager.GetInstance().UpdateReachableFlags();
			RunZonesSetupComponents();
		}
		return FinishedAndRemove();
	}

	private void RunZonesSetupComponents()
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		int totalZones = instance.GetTotalZones();
		for (int i = 0; i < totalZones; i++)
		{
			LevelEditor_ZoneManager.Zone zone = instance.GetZone(i, bSupressWarning: true);
			if (zone == null || !zone.m_bActive)
			{
				continue;
			}
			List<Object> tempZoneData = new List<Object>();
			int count = zone.m_BlocksInZone.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				LevelEditor_ZoneManager.Zone.ObjectsInZone objectsInZone = zone.m_BlocksInZone[num];
				if (objectsInZone != null && objectsInZone.m_GoodInteractPoint != -1 && objectsInZone.m_ComplexID == 0)
				{
					BaseZoneSetup component = objectsInZone.m_Object.GetComponent<BaseZoneSetup>();
					if (component != null)
					{
						component.SetupZone(zone, objectsInZone, ref tempZoneData);
					}
				}
			}
		}
	}
}

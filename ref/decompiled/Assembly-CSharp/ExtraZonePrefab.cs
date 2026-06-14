using System.Collections.Generic;
using UnityEngine;

public class ExtraZonePrefab : BaseZoneSetup
{
	public class ItemAdded : Object
	{
		public int m_InstanceID;

		public int m_Count;

		public ItemAdded(int iID, int iCount)
		{
			m_InstanceID = iID;
			m_Count = iCount;
		}
	}

	[Tooltip("What prefab should be created")]
	public GameObject m_PrefabToMake;

	[Tooltip("how many should be created by this component")]
	public int m_NumberToMake = 1;

	[Tooltip("The maximum that can be made for this zone. Zero = no limit")]
	public int m_MaxToMake;

	public const int INVALID_ADDOBJECT = -1;

	public override void SetupZone(LevelEditor_ZoneManager.Zone myZone, LevelEditor_ZoneManager.Zone.ObjectsInZone objInZone, ref List<Object> tempZoneData)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (myZone == null || !myZone.m_bActive)
		{
			return;
		}
		BaseLevelManager.LayerDataCollection layerDataCollection = instance.m_BuildingLayers[(uint)myZone.m_Layer];
		int instanceID = m_PrefabToMake.GetInstanceID();
		int addedObjectIndex = GetAddedObjectIndex(instanceID, ref tempZoneData);
		int num = m_NumberToMake;
		if (addedObjectIndex != -1 && m_MaxToMake != 0 && ((ItemAdded)tempZoneData[addedObjectIndex]).m_Count + num > m_MaxToMake)
		{
			num = m_MaxToMake - ((ItemAdded)tempZoneData[addedObjectIndex]).m_Count;
		}
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = Object.Instantiate(m_PrefabToMake, layerDataCollection.m_Objects.transform);
			float num2 = objInZone.m_GoodInteractPoint % 120;
			float num3 = objInZone.m_GoodInteractPoint / 120;
			float x = num2 + instance.m_fPositionOffsetsX[4];
			float y = num3 + instance.m_fPositionOffsetsY[4];
			gameObject.transform.localPosition = new Vector3(x, y, 0f);
			if (gameObject.GetComponentInChildren<PhotonView>() != null)
			{
				instance.ExternalAddPhotonViewObject(gameObject);
			}
			gameObject.AddComponent<OwnedByZone>().m_ZoneIndex = myZone.m_ID;
			LevelEditor_ZoneManager.Zone.ObjectsInZone objectsInZone = new LevelEditor_ZoneManager.Zone.ObjectsInZone();
			objectsInZone.m_Object = gameObject;
			objectsInZone.m_BeingBlocked = false;
			objectsInZone.m_X = objInZone.m_GoodInteractPoint % 120;
			objectsInZone.m_Y = objInZone.m_GoodInteractPoint / 120;
			myZone.m_BlocksInZone.Add(objectsInZone);
		}
		AddObject(instanceID, addedObjectIndex, num, ref tempZoneData);
	}

	private void AddObject(int iInstanceID, int iIndex, int iAmountToAdd, ref List<Object> tempZoneData)
	{
		if (iIndex == -1)
		{
			tempZoneData.Add(new ItemAdded(iInstanceID, iAmountToAdd));
		}
		else if (iIndex >= 0 && iIndex <= tempZoneData.Count && ((ItemAdded)tempZoneData[iIndex]).m_InstanceID == iInstanceID)
		{
			((ItemAdded)tempZoneData[iIndex]).m_Count = ((ItemAdded)tempZoneData[iIndex]).m_Count + iAmountToAdd;
		}
	}

	private int GetAddedObjectIndex(int iInstanceID, ref List<Object> tempZoneData)
	{
		int count = tempZoneData.Count;
		for (int i = 0; i < count; i++)
		{
			if (((ItemAdded)tempZoneData[i]).m_InstanceID == iInstanceID)
			{
				return i;
			}
		}
		return -1;
	}
}

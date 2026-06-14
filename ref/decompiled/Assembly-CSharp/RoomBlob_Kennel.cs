using System.Collections.Generic;
using UnityEngine;

public class RoomBlob_Kennel : RoomBlobData
{
	public InteractiveObject m_Bed;

	private bool m_bHandedOutPrimary;

	private List<InteractiveObject> m_SecondaryBeds = new List<InteractiveObject>();

	private int m_NextSecondaryIndex;

	protected virtual void OnDestroy()
	{
		if (m_RoomSpecificObjects != null)
		{
			m_RoomSpecificObjects.Clear();
			m_RoomSpecificObjects = null;
		}
		m_Bed = null;
	}

	public GameObject GetNextKennel()
	{
		if (!m_bHandedOutPrimary)
		{
			m_bHandedOutPrimary = true;
			return m_Bed.gameObject;
		}
		if (m_NextSecondaryIndex < m_SecondaryBeds.Count)
		{
			return m_SecondaryBeds[m_NextSecondaryIndex++].gameObject;
		}
		return null;
	}

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<KennelInteraction> kennelInteraction = new BaseLevelManager.RoomObjectCollectionType<KennelInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, kennelInteraction);
			SetupFromData(ref kennelInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<KennelInteraction> kennelInteraction = new BaseLevelManager.RoomObjectCollectionType<KennelInteraction>();
			BaseLevelManager.RoomObjectCollectionType<AICharacter_Dog> dogCharacters = new BaseLevelManager.RoomObjectCollectionType<AICharacter_Dog>();
			instance.GetObjectsInZone(ref zone, kennelInteraction, dogCharacters);
			SetupFromZone(ref kennelInteraction, ref dogCharacters);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<KennelInteraction> kennelInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (kennelInteraction.m_Contents.Count != 0)
		{
			m_Bed = kennelInteraction.m_Contents[0];
			for (int i = 1; i < kennelInteraction.m_Contents.Count; i++)
			{
				m_SecondaryBeds.Add(kennelInteraction.m_Contents[i]);
			}
		}
	}

	public void SetupFromZone(ref BaseLevelManager.RoomObjectCollectionType<KennelInteraction> kennelInteraction, ref BaseLevelManager.RoomObjectCollectionType<AICharacter_Dog> dogCharacters)
	{
		int count = kennelInteraction.m_Contents.Count;
		if (count != 0 && count == dogCharacters.m_Contents.Count)
		{
			m_Bed = kennelInteraction.m_Contents[0];
			for (int i = 1; i < count; i++)
			{
				m_SecondaryBeds.Add(kennelInteraction.m_Contents[i]);
			}
			for (int j = 0; j < count; j++)
			{
				dogCharacters.m_Contents[j].m_MyKennel = kennelInteraction.m_Contents[j].gameObject;
			}
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}
}

using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

public class RoomBlob_JobOffice : RoomBlobData
{
	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<BlockTagger> blockTags = new BaseLevelManager.RoomObjectCollectionType<BlockTagger>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, blockTags);
			SetupFromData(ref blockTags);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<BlockTagger> blockTags = new BaseLevelManager.RoomObjectCollectionType<BlockTagger>();
			instance.GetObjectsInZone(ref zone, blockTags);
			SetupFromData(ref blockTags);
		}
	}

	private void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<BlockTagger> blockTags)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = blockTags.m_Contents.Count;
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		List<GameObject> list3 = new List<GameObject>();
		for (int i = 0; i < count; i++)
		{
			switch (blockTags.m_Contents[i].m_Classification)
			{
			case BlockTagger.Classification.JobOffice_InmateChair:
				list.Add(blockTags.m_Contents[i].gameObject);
				break;
			case BlockTagger.Classification.JobOffice_OfficerChair:
				list2.Add(blockTags.m_Contents[i].gameObject);
				break;
			case BlockTagger.Classification.JobOffice_Officer:
				list3.Add(blockTags.m_Contents[i].gameObject);
				break;
			}
			Object.Destroy(blockTags.m_Contents[i]);
			blockTags.m_Contents[i] = null;
		}
		if (list.Count != list2.Count || list.Count != list3.Count || list.Count == 0)
		{
			return;
		}
		int count2 = list3.Count;
		float num = 10000f;
		int num2 = -1;
		for (int j = 0; j < count2; j++)
		{
			num = 10000f;
			num2 = -1;
			Vector3 position = list2[j].transform.position;
			for (int k = 0; k < count2; k++)
			{
				if (list[k] != null)
				{
					Vector3 position2 = list[k].transform.position;
					float num3 = Mathf.Min(Mathf.Abs(position.x - position2.x), Mathf.Abs(position.y - position2.y));
					if (num3 < num)
					{
						num = num3;
						num2 = k;
					}
				}
			}
			if (num2 != -1)
			{
				Blackboard component = list3[j].GetComponent<Blackboard>();
				if (component != null)
				{
					component.SetValue("m_InmateChair", list[num2].GetComponent<ChairInteraction>());
					component.SetValue("m_JobChair", list2[j]);
				}
			}
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<RoomMarker> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<RoomMarker>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType);
			int count = roomObjectCollectionType.m_Contents.Count;
			if (count == 1)
			{
				Vector3 position = roomObjectCollectionType.m_Contents[0].transform.position;
				blob.SetBuilderArrowPosition(position);
			}
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
	}
}

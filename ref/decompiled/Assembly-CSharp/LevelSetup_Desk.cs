using UnityEngine;

public class LevelSetup_Desk : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance == null)
		{
			Debug.LogError("LevelSetup_Desk - Failed to find details manager.");
			return FinishedAndRemove();
		}
		ItemContainer component = GetComponent<ItemContainer>();
		if (component == null)
		{
			Debug.LogError("LevelSetup_Desk - Failed to find item container on this desk.");
			return FinishedAndRemove();
		}
		LevelEditor_Settings editorSettings = GlobalStart.GetInstance().m_EditorSettings;
		if (editorSettings == null)
		{
			Debug.LogError("LevelSetup_Desk - Failed to find level editor settings!");
			return FinishedAndRemove();
		}
		if (!component.IsDesk())
		{
			Debug.LogError("LevelSetup_Desk - This item container is not a desk!");
			return FinishedAndRemove();
		}
		LevelDetailsManager.DiffecultyLevel levelDifficulty = instance.GetLevelDifficulty();
		switch (component.m_ContainerType)
		{
		case ItemContainer.ItemContainerType.Desk:
		{
			RoomManager classInstance = GetClassInstance<RoomManager>();
			if (classInstance == null)
			{
				Debug.LogError("LevelSetup_Desk - Failed to find room manager!");
				return FinishedAndRemove();
			}
			RoomBlob roomBlob = classInstance.LookUpRoom(base.gameObject.transform.position);
			if (roomBlob == null)
			{
				Debug.LogError("LevelSetup_Desk - Failed to look up room!");
				return FinishedAndRemove();
			}
			if (roomBlob.location == RoomBlob.eLocation.Infirmary || roomBlob.location == RoomBlob.eLocation.InfirmaryStockRoom)
			{
				component.ApplyContainerConfig(editorSettings.m_DifficultySettings[(int)levelDifficulty].MedicDeskConfig);
				Vector2 medicDeskMinMax = editorSettings.m_DifficultySettings[(int)levelDifficulty].MedicDeskMinMax;
				component.m_NumberFromGroups = Random.Range((int)medicDeskMinMax.x, (int)medicDeskMinMax.y);
			}
			else if (roomBlob.location == RoomBlob.eLocation.Maintenance)
			{
				component.ApplyContainerConfig(editorSettings.m_DifficultySettings[(int)levelDifficulty].MaintanenceDeskConfig);
				Vector2 maintanenceDeskMinMax = editorSettings.m_DifficultySettings[(int)levelDifficulty].MaintanenceDeskMinMax;
				component.m_NumberFromGroups = Random.Range((int)maintanenceDeskMinMax.x, (int)maintanenceDeskMinMax.y);
			}
			break;
		}
		case ItemContainer.ItemContainerType.DeskGuard:
		{
			Vector2 guardDeskMinMax = editorSettings.m_DifficultySettings[(int)levelDifficulty].GuardDeskMinMax;
			component.m_NumberFromGroups = Random.Range((int)guardDeskMinMax.x, (int)guardDeskMinMax.y);
			break;
		}
		case ItemContainer.ItemContainerType.DeskInmate:
		{
			Vector2 inmateDeskMinMax = editorSettings.m_DifficultySettings[(int)levelDifficulty].InmateDeskMinMax;
			component.m_NumberFromGroups = Random.Range((int)inmateDeskMinMax.x, (int)inmateDeskMinMax.y);
			break;
		}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

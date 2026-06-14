using System.Collections.Generic;
using Slate;
using UnityEngine;

public class ConstructEndgameInteraction : MultistageInputInteraction
{
	[Header("Escape Info")]
	public EscapeMethod m_EscapeMethod;

	public SpeechPODO m_TooManyPlayersDialog;

	public SpeechPODO m_TooFewPlayersDialog;

	public SpeechPODO m_InvalidMultiplayerProximityDialog;

	[Header("\"Hot\" Items Spawned Into Random Desks")]
	public ItemData[] m_TrackedItemsToSpawn;

	[Header("Escape Cutscene")]
	public Cutscene m_EscapeCutscene;

	[Header("Final Nameplate Text")]
	public string m_EscapeNameplateText = "Text.Interact.Escape";

	public string m_EscapeFarAwayNameplateText = string.Empty;

	private bool m_bEscapeTriggered;

	private static List<ItemData> m_SpawnList = new List<ItemData>();

	private static Dictionary<int, ItemContainer> m_SpawnItemMap = new Dictionary<int, ItemContainer>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		if (!(CutsceneManagerBase.GetInstance() != null) || CutsceneManagerBase.GetInstance().GetCutsceneIndex(m_EscapeCutscene) == -1)
		{
		}
		if (T17NetManager.IsMasterClient && m_TrackedItemsToSpawn != null && m_TrackedItemsToSpawn.Length > 0)
		{
			m_SpawnList.AddRange(m_TrackedItemsToSpawn);
		}
		return result;
	}

	[PunRPC]
	public override void RPC_SetStage(int stage, bool isSaveRestore)
	{
		base.RPC_SetStage(stage, isSaveRestore);
		if (m_Stages != null && stage == m_Stages.Count - 1 && m_NetObjectLock != null && null != m_NetObjectLock.m_TrackableElementReporter)
		{
			Localization.Get(m_EscapeNameplateText, out var localized);
			localized = ((!string.IsNullOrEmpty(localized)) ? localized : m_NetObjectLock.m_InteractActionNameTag);
			Localization.Get(m_EscapeFarAwayNameplateText, out var localized2);
			localized2 = ((!string.IsNullOrEmpty(localized2)) ? localized2 : m_NetObjectLock.m_FarAwayActionNameTag);
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localized);
			m_NetObjectLock.m_TrackableElementReporter.SetFarAwayDisplayName(localized2);
		}
	}

	public sealed override bool CanInteract(Character localCharacter)
	{
		if (!CheckCorrectAmountOfPlayers(localCharacter, out var characterDialog))
		{
			PlayDialogOnCharacter(localCharacter, characterDialog);
			return false;
		}
		if (!base.CanInteract(localCharacter))
		{
			return false;
		}
		if (!Child_CanInteract(localCharacter))
		{
			return false;
		}
		if (IsFinalStage() && !CanEscape(localCharacter, out characterDialog))
		{
			PlayDialogOnCharacter(localCharacter, characterDialog);
			return false;
		}
		return true;
	}

	protected override bool OnInteractedWithFinalStage(Character interactingCharacter)
	{
		if (m_bEscapeTriggered)
		{
			return false;
		}
		TriggerEscape(interactingCharacter);
		StartCoroutine(HideAnimatedObject());
		return true;
	}

	protected virtual bool Child_CanInteract(Character localCharacter)
	{
		return true;
	}

	private bool CanEscape(Character interactingCharacter, out SpeechPODO characterDialog)
	{
		return CheckCorrectAmountOfPlayers(interactingCharacter, out characterDialog) && CheckAllPlayersWithinProximityForCooperative(interactingCharacter, out characterDialog);
	}

	private bool CheckCorrectAmountOfPlayersSingleplayer(Character interactingCharacter, out SpeechPODO characterDialog)
	{
		if (Gamer.GetGamerCount() > 1 && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Cooperative)
		{
			characterDialog = m_TooManyPlayersDialog;
			return false;
		}
		characterDialog = null;
		return true;
	}

	private bool CheckCorrectAmountOfPlayersMultiplayer(Character interactingCharacter, out SpeechPODO characterDialog)
	{
		if (Gamer.GetGamerCount() < 2)
		{
			characterDialog = m_TooFewPlayersDialog;
			return false;
		}
		characterDialog = null;
		return true;
	}

	public bool CheckCorrectAmountOfPlayers(Character interactingCharacter, out SpeechPODO characterDialog)
	{
		ModeSupportedTypes modeSupportedTypes = m_SupportedMode;
		if (modeSupportedTypes == ModeSupportedTypes.Dynamic)
		{
			modeSupportedTypes = ((Gamer.GetGamerCount() > 1 && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Cooperative) ? ModeSupportedTypes.MultiPlayer : ModeSupportedTypes.SinglePlayer);
		}
		if (modeSupportedTypes == ModeSupportedTypes.SinglePlayer)
		{
			return CheckCorrectAmountOfPlayersSingleplayer(interactingCharacter, out characterDialog);
		}
		return CheckCorrectAmountOfPlayersMultiplayer(interactingCharacter, out characterDialog);
	}

	private bool CheckAllPlayersWithinProximityForCooperative(Character interactingCharacter, out SpeechPODO characterDialog)
	{
		if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Cooperative && !AreAllPlayersWithinProximity(interactingCharacter))
		{
			characterDialog = m_InvalidMultiplayerProximityDialog;
			return false;
		}
		characterDialog = null;
		return true;
	}

	private bool AreAllPlayersWithinProximity(Character interactingCharacter)
	{
		if (Gamer.GetGamerCount() == 1)
		{
			return true;
		}
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(interactingCharacter.transform.position, interactingCharacter.CurrentFloor);
		if (roomBlob != null)
		{
			bool result = true;
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int num = allGamers.Length - 1; num >= 0; num--)
			{
				if (allGamers[num] != null && !(allGamers[num].m_PlayerObject == interactingCharacter))
				{
					if (allGamers[num].m_PlayerObject == null)
					{
						result = false;
						break;
					}
					RoomBlob roomBlob2 = RoomManager.GetInstance().LookUpRoom(allGamers[num].m_PlayerObject.transform.position, allGamers[num].m_PlayerObject.CurrentFloor);
					if (roomBlob2 != roomBlob)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
		return false;
	}

	private void TriggerEscape(Character interactingCharacter)
	{
		EscapePrisonFunctionality.GetInstance().TriggerEscapeRPC(m_EscapeCutscene, m_EscapeMethod, interactingCharacter);
	}

	public static void SpawnAllNecessaryItems_Classic()
	{
		if (RoutineManager.GetInstance().m_RoutinesData.m_bIsTimedPrison)
		{
			return;
		}
		ItemManager instance = ItemManager.GetInstance();
		m_SpawnItemMap.Clear();
		if (!T17NetManager.IsMasterClient || !(instance != null))
		{
			return;
		}
		for (int i = 0; i < m_SpawnList.Count; i++)
		{
			if (m_SpawnList[i] != null)
			{
				ItemContainer anyQuestableDeskContainer = ItemContainerManager.GetInstance().GetAnyQuestableDeskContainer();
				if (anyQuestableDeskContainer != null)
				{
					int requestID = -1;
					m_SpawnItemMap.Add(ItemManager.GetInstance().GetNextRequestID(), anyQuestableDeskContainer);
					instance.AssignItemRPC(anyQuestableDeskContainer.NetView.ownerId, m_SpawnList[i].m_ItemDataID, OnItemMgrResponse, ref requestID);
				}
			}
		}
	}

	public static void SpawnAllNecessaryItems_Transport()
	{
		if (!RoutineManager.GetInstance().m_RoutinesData.m_bIsTimedPrison)
		{
			return;
		}
		ItemManager instance = ItemManager.GetInstance();
		if (T17NetManager.IsMasterClient && !PrisonSnapshotIO.IsThereSaveData() && instance != null)
		{
			for (int i = 0; i < m_SpawnList.Count; i++)
			{
				if (m_SpawnList[i] != null)
				{
					ItemContainer anyQuestableDeskContainer = ItemContainerManager.GetInstance().GetAnyQuestableDeskContainer();
					if (anyQuestableDeskContainer != null)
					{
						int requestID = -1;
						m_SpawnItemMap.Add(ItemManager.GetInstance().GetNextRequestID(), anyQuestableDeskContainer);
						instance.AssignItemRPC(anyQuestableDeskContainer.NetView.ownerId, m_SpawnList[i].m_ItemDataID, OnItemMgrResponse, ref requestID, anyQuestableDeskContainer.NetView.viewID);
					}
				}
			}
		}
		m_SpawnList.Clear();
	}

	private static void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (m_SpawnItemMap.ContainsKey(eventID))
		{
			ItemContainer itemContainer = m_SpawnItemMap[eventID];
			if (itemContainer != null && newItem != null && !itemContainer.AddItemRPC(newItem))
			{
				newItem.DropItemInLevel(null, itemContainer.transform.position);
			}
			m_SpawnItemMap.Remove(eventID);
		}
	}

	public static void CleanUp()
	{
		if (m_SpawnList != null)
		{
			m_SpawnList.Clear();
		}
		if (m_SpawnItemMap != null)
		{
			m_SpawnItemMap.Clear();
		}
	}

	public override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		if (item.IsQuestItem())
		{
			m_Container.RemoveItemRPC(item, releaseToManager: true);
		}
	}
}

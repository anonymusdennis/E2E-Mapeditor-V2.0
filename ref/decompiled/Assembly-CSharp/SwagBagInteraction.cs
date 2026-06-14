using System;
using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;

public class SwagBagInteraction : AnimatedInteraction, IControlledUpdate, INetworkLoadable
{
	public ItemContainer m_LinkedItemContainer;

	private static Vector3 HidePosition = new Vector3(-500f, -500f, 0f);

	private bool m_bIsHidden;

	public string m_OwnerName;

	public SwagBagEventManager m_EventManager;

	public string m_CharacterLocalizationToken = "$CharacterName";

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public bool IsHidden => m_bIsHidden;

	public void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return true;
	}

	public override bool InteractionVisibility()
	{
		return true;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected override void Init()
	{
		base.Init();
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		if (m_LinkedItemContainer == null)
		{
			m_LinkedItemContainer = GetComponent<ItemContainer>();
		}
		ItemContainer linkedItemContainer = m_LinkedItemContainer;
		linkedItemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(linkedItemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(BagItemsChanged));
		if (T17NetManager.IsMasterClient)
		{
			HideBagRPC();
		}
		HandleSwagBagLabel();
	}

	private void HandleSwagBagLabel()
	{
		if (m_NetObjectLock.m_TrackableElementReporter != null)
		{
			Localization.Get("Text.Name.Nobody", out var localized);
			string value = ((!string.IsNullOrEmpty(m_OwnerName)) ? m_OwnerName : localized);
			Localization.GetWithKeySwap(m_NetObjectLock.m_InteractActionNameTag, out var localised, m_CharacterLocalizationToken, value);
			localised = ((!string.IsNullOrEmpty(localised)) ? localised : m_NetObjectLock.m_InteractActionNameTag);
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localised);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_LinkedItemContainer != null)
		{
			m_interactingCharacter.m_OpenContainer = m_LinkedItemContainer;
		}
		TryOpenSwagBag();
	}

	public override bool LeaveCharacterPositionUnAltered()
	{
		return true;
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
	}

	private void TryOpenSwagBag()
	{
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer && (m_LinkedItemContainer.GetItemCount() > 0 || !m_bIsHidden) && m_interactingCharacter.m_OpenContainer != null)
		{
			((Player)m_interactingCharacter).ViewContainer(m_interactingCharacter.m_OpenContainer, InGameRootMenu.InGameMenuTypeToOpen.SwagBag);
			InGameMenuFlow.Instance.GetCorrectIGMData(((Player)m_interactingCharacter).m_PlayerCameraManagerBindingID, out var _);
		}
	}

	public void HideBagRPC()
	{
		if (m_NetObjectLock != null)
		{
			m_NetObjectLock.m_NetView.RPC("RPC_HideSwagBag", NetTargets.All);
		}
	}

	[PunRPC]
	private void RPC_HideSwagBag()
	{
		base.transform.position = HidePosition;
		m_bIsHidden = true;
		m_OwnerName = string.Empty;
	}

	[PunRPC]
	public void RPC_SetBagPosition(float x, float y, float z, string playerName)
	{
		Vector3 vector = default(Vector3);
		vector.x = x;
		vector.y = y;
		vector.z = z;
		base.transform.position = vector;
		if (T17NetManager.IsMasterClient && m_EventManager != null)
		{
			m_EventManager.StartSwagBagTimer();
		}
		m_OwnerName = playerName;
		HandleSwagBagLabel();
		CarryObjectInteraction component = GetComponent<CarryObjectInteraction>();
		FloorManager instance = FloorManager.GetInstance();
		if (component != null && instance != null)
		{
			instance.GetTileGridPointAndFloorIndex(vector, FloorManager.TileSystem_Type.TileSystem_Ground, out var row, out var column, out var floor);
			CarryObjectInteraction.UpdateMovedObjects(m_NetViewID.viewID, component.PackIntoULong(floor, row, column));
			CarryObjectInteraction.SerializeAll();
		}
	}

	public void SetSwagBag(Vector3 newPosition, ItemContainer playerInventory, Item outfit, Item equippedItem, string playerName)
	{
		m_bIsHidden = false;
		FloorManager instance = FloorManager.GetInstance();
		FloorManager.Floor floor = instance.FindFloorAtZ(newPosition.z);
		if (instance.GetTileGridPoint(floor, FloorManager.TileSystem_Type.TileSystem_Ground, newPosition, out var row, out var column))
		{
			Item itemAtFloorTile = instance.GetItemAtFloorTile(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row, column);
			if (itemAtFloorTile != null)
			{
				LevelScript.GetInstance().m_LevelItemContainer.RemoveItemRPC(itemAtFloorTile, releaseToManager: true);
			}
		}
		m_NetObjectLock.m_NetView.RPC("RPC_SetBagPosition", NetTargets.All, newPosition.x, newPosition.y, newPosition.z, playerName);
	}

	public bool TryMoveItemsIntoSwagBag(ItemContainer playerInventory, Item outfit, Item equippedItem)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < playerInventory.m_StartingItems.Count; i++)
		{
			if (playerInventory.m_StartingItems[i] != null)
			{
				if (dictionary.ContainsKey(playerInventory.m_StartingItems[i].m_ItemDataID))
				{
					dictionary[playerInventory.m_StartingItems[i].m_ItemDataID]++;
				}
				else
				{
					dictionary.Add(playerInventory.m_StartingItems[i].m_ItemDataID, 1);
				}
			}
		}
		int num = 0;
		if (outfit != null && (!dictionary.ContainsKey(outfit.ItemDataID) || dictionary[outfit.ItemDataID] == 0))
		{
			num++;
		}
		if (equippedItem != null && (!dictionary.ContainsKey(equippedItem.ItemDataID) || dictionary[equippedItem.ItemDataID] == 0))
		{
			num++;
		}
		int itemCount = playerInventory.GetItemCount();
		int freeSpaceCount = m_LinkedItemContainer.GetFreeSpaceCount();
		if (itemCount + num > freeSpaceCount)
		{
			int num2 = itemCount + num - freeSpaceCount;
			int num3 = 0;
			while (num2 > 0 && num3 < m_LinkedItemContainer.m_MaxSize)
			{
				m_LinkedItemContainer.RemoveItemRPC(m_LinkedItemContainer.GetItem(0), releaseToManager: true);
				num2--;
				num3++;
			}
		}
		int itemCount2 = m_LinkedItemContainer.GetItemCount();
		playerInventory.MoveItemsToAnotherContainer(m_LinkedItemContainer, includeHidden: false);
		int itemCount3 = m_LinkedItemContainer.GetItemCount();
		for (int num4 = itemCount3 - 1; num4 >= itemCount2; num4--)
		{
			Item item = m_LinkedItemContainer.GetItem(num4);
			if (item != null && dictionary.ContainsKey(item.ItemDataID) && dictionary[item.ItemDataID] > 0)
			{
				m_LinkedItemContainer.RemoveItemRPC(item, releaseToManager: true);
			}
		}
		if (outfit != null && (!dictionary.ContainsKey(outfit.ItemDataID) || dictionary[outfit.ItemDataID] == 0))
		{
			m_LinkedItemContainer.AddItemRPC(outfit);
		}
		if (equippedItem != null && (!dictionary.ContainsKey(equippedItem.ItemDataID) || dictionary[equippedItem.ItemDataID] == 0))
		{
			m_LinkedItemContainer.AddItemRPC(equippedItem);
		}
		if (itemCount2 < m_LinkedItemContainer.GetItemCount())
		{
			return true;
		}
		return false;
	}

	public void PlaceSwagBagInCell(Character character, RoomBlob targetCell, ItemContainer playerInventory, Item outfit, Item equippedItem, string playerName)
	{
		if (!(targetCell != null) || !(playerInventory != null) || (playerInventory.GetItemCount() <= 0 && !(outfit != null) && !(equippedItem != null)) || !TryMoveItemsIntoSwagBag(playerInventory, outfit, equippedItem))
		{
			return;
		}
		FloorManager instance = FloorManager.GetInstance();
		SwagBagManager instance2 = SwagBagManager.GetInstance();
		if (!(instance != null) || !(instance2 != null))
		{
			return;
		}
		RoomBlob_Cell roomBlobData = targetCell.GetRoomBlobData<RoomBlob_Cell>();
		if (!(roomBlobData != null))
		{
			return;
		}
		SpawnPoint spawnPointForCharacter = roomBlobData.GetSpawnPointForCharacter(character);
		if (spawnPointForCharacter != null)
		{
			Vector3 position = spawnPointForCharacter.transform.position;
			FloorManager.Floor floor = null;
			floor = instance.FindFloorAtZ(position.z);
			if (floor != null)
			{
				Vector3 newPosition = instance2.FindCleanestPositionInRoom(floor, position, targetCell);
				SetSwagBag(newPosition, playerInventory, outfit, equippedItem, playerName);
			}
		}
	}

	public void BagItemsChanged()
	{
		if (T17NetManager.IsMasterClient && m_LinkedItemContainer.GetItemCount() <= 0)
		{
			HideBagRPC();
			if (T17NetManager.IsMasterClient && m_EventManager != null)
			{
				m_EventManager.ClearSwagBagEvent();
			}
		}
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			if (m_LoadState == LOADSTATE.Finished_OK)
			{
				m_NetObjectLock.m_NetView.RPC("RPC_RequestStateResponce_Yes_SwagBag", player, base.transform.position.x, base.transform.position.y, base.transform.position.z, m_bIsHidden);
			}
			else
			{
				m_NetObjectLock.m_NetView.RPC("RPC_RequestStateResponce_No_SwagBag", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_SwagBag(PhotonMessageInfo info)
	{
		m_LoadError = "Generator RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_SwagBag(float posX, float posY, float posZ, bool bIsHidden, PhotonMessageInfo info)
	{
		base.transform.position = new Vector3(posX, posY, posZ);
		m_bIsHidden = bIsHidden;
		m_LoadState = LOADSTATE.Finished_OK;
	}

	protected override void OnDestroy()
	{
		if (m_LinkedItemContainer != null)
		{
			ItemContainer linkedItemContainer = m_LinkedItemContainer;
			linkedItemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(linkedItemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(BagItemsChanged));
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		base.OnDestroy();
	}
}

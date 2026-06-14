using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class Vendor : T17MonoBehaviour
{
	private class PendingAssignment
	{
		public Character character;

		public int duration;

		public PendingAssignment(Character _character, int _duration)
		{
			character = _character;
			duration = _duration;
		}
	}

	private class PendingItemRefresh
	{
		public ItemData[] items;

		public PendingItemRefresh(ItemData[] _items)
		{
			items = _items;
		}
	}

	private PendingAssignment m_PendingAssignment;

	private PendingItemRefresh m_PendingItemRefresh;

	private bool m_BlockUnassign;

	private T17NetView m_NetView;

	private ItemContainer m_ItemContainer;

	private Character m_Character;

	private RoutineManager.CallbackInGameTimer m_ExpireTimer;

	public Sprite m_MapIcon;

	public string m_MapToolTipTag = string.Empty;

	private int m_PinID = -1;

	private List<int> m_ItemMgrResponseIDs = new List<int>();

	private int m_ImmediateItemMgrResponseID = -1;

	private bool m_bIsObjective;

	protected override void Awake()
	{
		base.Awake();
		m_ItemContainer = GetComponent<ItemContainer>();
		m_NetView = GetComponent<T17NetView>();
	}

	protected virtual void OnDestroy()
	{
		if (m_PinID != -1)
		{
			m_Character.ResetPinImage(PinManager.Pin.PinFilterType.Shops);
			if (m_bIsObjective)
			{
				m_Character.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
			}
		}
		m_PinID = -1;
		m_ItemContainer = null;
		m_NetView = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		return base.StartInit();
	}

	private void Update()
	{
		if (m_PendingAssignment != null && !m_BlockUnassign)
		{
			if (m_PendingAssignment.character != null)
			{
				AssignCharacterRPC(m_PendingAssignment.character, m_PendingAssignment.duration);
			}
			else
			{
				UnassignCharacterRPC();
				VendorManager.GetInstance().OnVendorExpired(this);
			}
			m_PendingAssignment = null;
		}
		if (m_PendingItemRefresh != null && !m_BlockUnassign)
		{
			RefreshItems(m_PendingItemRefresh.items);
			m_PendingItemRefresh = null;
		}
	}

	public void SetBlockExternalChangesRPC(bool blocked)
	{
		m_NetView.RPC("RPC_SetBlockExternalChanges", NetTargets.All, blocked);
	}

	[PunRPC]
	private void RPC_SetBlockExternalChanges(bool blocked, PhotonMessageInfo info)
	{
		m_BlockUnassign = blocked;
	}

	public void RequestAssignCharacter(Character character, int duration)
	{
		if (m_BlockUnassign)
		{
			m_PendingAssignment = new PendingAssignment(character, duration);
		}
		else
		{
			AssignCharacterRPC(character, duration);
		}
	}

	public void RequestUnassignCharacter()
	{
		if (m_BlockUnassign)
		{
			m_PendingAssignment = new PendingAssignment(null, 0);
		}
		else
		{
			UnassignCharacterRPC();
		}
	}

	public void RequestRefreshItems(ItemData[] items)
	{
		if (m_BlockUnassign)
		{
			m_PendingItemRefresh = new PendingItemRefresh(items);
		}
		else
		{
			RefreshItems(items);
		}
	}

	public void RequestRefreshItems(RandomItemGroup possibleItems, int count)
	{
		int num = Mathf.Min(count);
		ItemData[] array = new ItemData[num];
		for (int i = 0; i < num; i++)
		{
			ItemData randomItem = possibleItems.GetRandomItem(bUniqueItems: true);
			if (randomItem != null)
			{
				array[i] = randomItem;
			}
		}
		RequestRefreshItems(array);
	}

	private void AssignCharacterRPC(Character character, int duration)
	{
		if (!(character == null) && duration >= 0)
		{
			m_NetView.PostLevelLoadRPC("RPC_AssignCharacter", NetTargets.All, character.m_NetView.viewID, duration);
		}
	}

	[PunRPC]
	private void RPC_AssignCharacter(int characterNetViewID, int duration, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterNetViewID);
		if (!(character == null))
		{
			if (m_Character != null)
			{
				UnassignCharacter();
			}
			AssignCharacter(character);
			if (m_ExpireTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_ExpireTimer);
			}
			if (duration > 0)
			{
				m_ExpireTimer = RoutineManager.GetInstance().CreateCallbackTimer(0, 0, duration, OnAlarm_VendorExpire, relativeToStart: false);
			}
			VendorManager.GetInstance().OnVendorUpdated();
		}
	}

	private void AssignCharacter(Character character)
	{
		m_Character = character;
		m_Character.SetIsVendor(value: true);
		m_Character.m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.Vendor);
		m_Character.SetPinImage(m_MapIcon, PinManager.Pin.PinFilterType.Shops);
	}

	private void UnassignCharacterRPC()
	{
		if (!(m_Character == null))
		{
			m_NetView.PostLevelLoadRPC("RPC_UnassignCharacter", NetTargets.All);
		}
	}

	[PunRPC]
	private void RPC_UnassignCharacter(PhotonMessageInfo info)
	{
		if (!(m_Character == null))
		{
			UnassignCharacter();
			if (null != m_ItemContainer && T17NetManager.IsMasterClient)
			{
				m_ItemContainer.RemoveAllItems(releaseToManager: true);
			}
			if (m_ExpireTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_ExpireTimer);
				m_ExpireTimer = null;
			}
			VendorManager.GetInstance().OnVendorUpdated();
		}
	}

	private void UnassignCharacter()
	{
		if (!(m_Character == null))
		{
			m_Character.m_IconHandler.RemoveIcon(CharacterIconHandler.IconType.Vendor);
			m_Character.SetIsVendor(value: false);
			m_Character.ResetPinImage(PinManager.Pin.PinFilterType.Shops);
			if (m_bIsObjective)
			{
				m_Character.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
			}
			m_Character = null;
		}
	}

	public Character GetCharacter()
	{
		return m_Character;
	}

	public ItemContainer GetItemContainer()
	{
		return m_ItemContainer;
	}

	public void PauseExpireTimer()
	{
		SetExpireTimerActive_RPC(active: false);
	}

	public void UnpauseExpireTimer()
	{
		SetExpireTimerActive_RPC(active: true);
	}

	private void SetExpireTimerActive_RPC(bool active)
	{
		if (m_NetView != null)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetExpireTimerActive", NetTargets.All, active);
		}
	}

	[PunRPC]
	private void RPC_SetExpireTimerActive(bool active)
	{
		if (m_ExpireTimer != null)
		{
			if (active)
			{
				m_ExpireTimer.Resume();
			}
			else
			{
				m_ExpireTimer.Pause();
			}
		}
	}

	private void RefreshItems(ItemData[] items)
	{
		if (!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient))
		{
			return;
		}
		m_ItemContainer.RemoveAllItems(releaseToManager: true);
		int num = Mathf.Min(items.Length, m_ItemContainer.GetFreeSpaceCount());
		m_ItemMgrResponseIDs.Clear();
		for (int i = 0; i < num; i++)
		{
			ItemData itemData = items[i];
			if (itemData != null)
			{
				m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(m_NetView.ownerId, itemData.m_ItemDataID, OnItemMgrResponse_AddToInventory, ref m_ImmediateItemMgrResponseID));
			}
		}
	}

	private void OnItemMgrResponse_AddToInventory(Item item, int eventID)
	{
		if (item != null && (eventID == m_ImmediateItemMgrResponseID || m_ItemMgrResponseIDs.Contains(eventID)) && !m_ItemContainer.AddItemRPC(item))
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	public void SetIsObjective(bool isObjective)
	{
		m_bIsObjective = isObjective;
		if (isObjective)
		{
			m_Character.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
		}
		else
		{
			m_Character.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
		}
	}

	public bool CanUseVendor(Player player)
	{
		int opinionOf = m_Character.GetOpinionOf(player);
		int requiredOpinion = VendorManager.GetInstance().GetRequiredOpinion();
		return opinionOf >= requiredOpinion;
	}

	public int GetModifiedItemCost(Item item)
	{
		return Mathf.FloorToInt(Mathf.Round((float)item.Value * VendorManager.GetInstance().GetItemCostModifier() / 5f) * 5f);
	}

	public bool PurchaseItemRPC(int itemIndex, Player player, ItemContainer targetContainer)
	{
		Item item = m_ItemContainer.GetItem(itemIndex);
		if (item == null || player == null || targetContainer == null)
		{
			return false;
		}
		bool result = false;
		if (targetContainer.GetFreeSpaceCount() > 0 || (player.GetEquippedItem() == null && player.CanEquipItem(item)))
		{
			int modifiedItemCost = GetModifiedItemCost(item);
			if (player.m_CharacterStats.Money >= (float)modifiedItemCost)
			{
				m_NetView.RPC("RPC_PurchaseItem", NetTargets.MasterClient, item.m_NetView.viewID, player.m_NetView.viewID, targetContainer.GetObjectNetID());
				player.m_CharacterStats.DecreaseMoney(modifiedItemCost);
				result = true;
			}
		}
		return result;
	}

	[PunRPC]
	private void RPC_PurchaseItem(int itemViewID, int playerViewID, int targetContainerViewID, PhotonMessageInfo info)
	{
		Item itemByViewID = m_ItemContainer.GetItemByViewID(itemViewID);
		Player player = T17NetView.Find<Player>(playerViewID);
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(targetContainerViewID);
		if (!(itemByViewID == null) && !(player == null) && !(itemContainer == null))
		{
			if (player.GetEquippedItem() == null && player.CanEquipItem(itemByViewID))
			{
				m_ItemContainer.MoveItemToCharacterEquipedSlot(itemByViewID.m_NetView.viewID, playerViewID);
			}
			else
			{
				m_ItemContainer.MoveItemToAnotherContainerRPC(itemByViewID.m_NetView.viewID, itemContainer.NetView.viewID);
			}
			if (m_Character != null && m_ItemContainer.GetItemCount() <= 0)
			{
				RequestUnassignCharacter();
				VendorManager.GetInstance().OnVendorExpired(this);
			}
		}
	}

	private void OnAlarm_VendorExpire()
	{
		m_ExpireTimer = null;
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			RequestUnassignCharacter();
			VendorManager.GetInstance().OnVendorExpired(this);
		}
	}

	public ulong Serialize()
	{
		BitField bitField = new BitField();
		int uValue = 0;
		if (m_ExpireTimer != null)
		{
			uValue = (int)(m_ExpireTimer.TimeLeft / 60f / 60f);
			uValue = Mathf.Clamp(uValue, 0, 127);
		}
		bitField.Set(12, (uint)m_NetView.viewID);
		bitField.Set(12, (uint)m_Character.m_NetView.viewID);
		bitField.Set(7, (uint)uValue);
		return (ulong)bitField;
	}

	public bool Deserialize(ulong data, ref string error)
	{
		BitField bitField = new BitField(data);
		int uInt = (int)bitField.GetUInt(12);
		int uInt2 = (int)bitField.GetUInt(12);
		uint uInt3 = bitField.GetUInt(7);
		bool flag = false;
		if (uInt == m_NetView.viewID)
		{
			Character character = T17NetView.Find<Character>(uInt2);
			if (character != null)
			{
				AssignCharacter(character);
				if (T17NetManager.IsMasterClient)
				{
					m_ExpireTimer = RoutineManager.GetInstance().CreateCallbackTimer(0, (int)uInt3, 0, OnAlarm_VendorExpire, relativeToStart: false);
				}
			}
			flag = true;
		}
		if (!flag)
		{
			string text = $"Failed to properly deserialize vendor '{m_NetView.viewID}'";
			error += text;
		}
		return flag;
	}

	public static int DeserializeVendorID(ulong data)
	{
		BitField bitField = new BitField(data);
		return (int)bitField.GetUInt(12);
	}
}

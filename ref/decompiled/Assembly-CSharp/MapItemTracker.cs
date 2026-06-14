using System;
using System.Collections.Generic;
using UnityEngine;

public class MapItemTracker : T17MonoBehaviour, IControlledUpdate, Saveable
{
	public class PinData
	{
		public int pinID = -1;

		public ItemContainer container;

		public List<Item> items;

		public int currIndex = -1;

		public float timer;
	}

	public class MapItemTrackerDeserializer : IDeserializable
	{
		public bool Deserialize(string data, ref string error)
		{
			return GlobalDeserialize(data, ref error);
		}

		public string GetSerializationData()
		{
			return NetPrisonViewDetails.Instance.PlayerItemTrackingData;
		}
	}

	[Serializable]
	public class NetSaveData
	{
		[Serializable]
		public class NetContainerData
		{
			public int viewID;

			public List<int> itemViewIDs = new List<int>();
		}

		public List<NetContainerData> m_SerializedData = new List<NetContainerData>();
	}

	[Serializable]
	protected class SaveData_MapItemTracker_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public string SDATA;

		public SaveData_MapItemTracker_V1()
		{
			m_Version = 1;
		}
	}

	private const float PIN_CYCLE_DURATION = 1f;

	private const int MAX_TRACKED_ITEMS_PER_CONTAINER = 8;

	public T17NetView m_NetView;

	public Sprite m_DefaultTrackingSprite;

	private List<PinData> m_PinData = new List<PinData>();

	private Dictionary<ItemContainer, List<Item>> m_TrackedItems = new Dictionary<ItemContainer, List<Item>>();

	private List<ItemContainer> m_TrackedContainers = new List<ItemContainer>();

	private Item[] m_TempFoundItems = new Item[8];

	private Player[] m_PinVisiblePlayers;

	private Player m_Player;

	private float m_LastUpdateTime;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private static bool m_IsSerializing;

	private static bool m_ShouldReserialize;

	private SaveDataRegister m_SaveData;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_Player = GetComponent<Player>();
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, 19091, bIsMajorManagerComponent: false, m_Player.m_PlayerNumber);
		m_LastUpdateTime = UpdateManager.time;
		Player component = GetComponent<Player>();
		if (component != null)
		{
			m_PinVisiblePlayers = new Player[1] { component };
		}
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.SlowPeriodic);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.SlowPeriodic);
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_NetView = null;
	}

	public void ControlledUpdate()
	{
		float num = UpdateManager.time - m_LastUpdateTime;
		for (int i = 0; i < m_PinData.Count; i++)
		{
			PinData pinData = m_PinData[i];
			pinData.timer -= num;
			if (pinData.timer <= 0f)
			{
				pinData.timer = 1f;
				if (++pinData.currIndex >= pinData.items.Count)
				{
					pinData.currIndex = 0;
				}
				SetPinIcon(pinData.pinID, pinData.items[pinData.currIndex]);
			}
		}
		if (m_ShouldReserialize)
		{
			if (T17NetManager.IsMasterClient)
			{
				UpdateNetPrisonViewData();
			}
			m_ShouldReserialize = false;
		}
		m_LastUpdateTime = UpdateManager.time;
	}

	public void ControlledFixedUpdate()
	{
	}

	public static bool IsTrackableItem(Item item)
	{
		return item.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null || item.HasFunctionality(BaseItemFunctionality.Functionality.Keycard) != null;
	}

	public bool UpdateTrackedItems(ItemContainer container)
	{
		bool flag = IsTracked(container);
		int num = FindTrackableItems(container, ref m_TempFoundItems);
		if (num <= 0)
		{
			if (flag)
			{
				StopTrackingContainer(container);
				return true;
			}
			return false;
		}
		if (!flag)
		{
			StartTrackingContainer(container);
		}
		bool flag2 = false;
		List<Item> value = null;
		if (m_TrackedItems.TryGetValue(container, out value))
		{
			for (int num2 = value.Count - 1; num2 >= 0; num2--)
			{
				Item item = value[num2];
				int num3 = Array.IndexOf(m_TempFoundItems, value[num2]);
				if (num3 < 0)
				{
					value.RemoveAt(num2);
					flag2 = true;
				}
			}
			for (int i = 0; i < num; i++)
			{
				if (!value.Contains(m_TempFoundItems[i]))
				{
					value.Add(m_TempFoundItems[i]);
					flag2 = true;
				}
			}
			if (flag2)
			{
				int pinDataIndex = GetPinDataIndex(container);
				if (pinDataIndex >= 0)
				{
					PinData pinData = m_PinData[pinDataIndex];
					if (pinData.currIndex >= pinData.items.Count)
					{
						pinData.currIndex = 0;
					}
					SetPinIcon(pinData.pinID, pinData.items[pinData.currIndex]);
				}
			}
		}
		for (int j = 0; j < m_TempFoundItems.Length; j++)
		{
			m_TempFoundItems[j] = null;
		}
		return flag2;
	}

	private bool StartTrackingContainer(ItemContainer container)
	{
		List<Item> value = new List<Item>();
		if (!m_TrackedItems.TryGetValue(container, out value))
		{
			value = new List<Item>(8);
			m_TrackedItems.Add(container, value);
			m_TrackedContainers.Add(container);
		}
		bool bUpdatePosition = container.GetCharacterOwner() != null;
		PinManager instance = PinManager.GetInstance();
		bool bForMainMap = true;
		bool bForMiniMap = true;
		GameObject target = container.gameObject;
		Sprite defaultTrackingSprite = m_DefaultTrackingSprite;
		Player[] pinVisiblePlayers = m_PinVisiblePlayers;
		int num = instance.CreatePin(bForMainMap, bForMiniMap, target, defaultTrackingSprite, bUpdatePosition, null, pinVisiblePlayers, PinManager.Pin.PinFilterType.Characters, edgable: false, floorTrackable: false, directional: false, string.Empty);
		if (num != -1)
		{
			PinData pinData = new PinData();
			pinData.pinID = num;
			pinData.container = container;
			pinData.items = value;
			pinData.currIndex = 0;
			pinData.timer = 1f;
			m_PinData.Add(pinData);
		}
		container.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(container.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(OnContainerItemRemoved));
		return true;
	}

	private void StopTrackingContainer(ItemContainer container)
	{
		List<Item> value = null;
		if (m_TrackedItems.TryGetValue(container, out value))
		{
			value.Clear();
			m_TrackedItems.Remove(container);
		}
		m_TrackedContainers.Remove(container);
		int pinDataIndex = GetPinDataIndex(container);
		if (pinDataIndex >= 0)
		{
			PinManager.GetInstance().RemovePin(m_PinData[pinDataIndex].pinID);
			m_PinData.RemoveAt(pinDataIndex);
		}
		container.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(container.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(OnContainerItemRemoved));
	}

	public bool IsTracked(ItemContainer container)
	{
		return m_TrackedItems.ContainsKey(container);
	}

	private int GetPinDataIndex(ItemContainer container)
	{
		int result = -1;
		for (int i = 0; i < m_PinData.Count; i++)
		{
			if (m_PinData[i].container == container)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void SetPinIcon(int pinID, Item item)
	{
		if (!(item != null) || !(item.m_ItemData != null))
		{
			return;
		}
		Sprite sprite = item.m_ItemData.m_ItemUIMapImage;
		if (sprite == null)
		{
			sprite = item.m_ItemData.m_ItemUIImage;
		}
		if (sprite != null)
		{
			PinManager instance = PinManager.GetInstance();
			if (instance != null)
			{
				instance.UpdatePinIconSprite(pinID, sprite, item.m_ItemData.m_UIMapWorldPositionOffset.x, item.m_ItemData.m_UIMapWorldPositionOffset.y);
			}
		}
	}

	private bool ContainsTrackableItems(ItemContainer container)
	{
		int itemCount = container.GetItemCount();
		for (int i = 0; i < itemCount; i++)
		{
			if (IsTrackableItem(container.GetItem(i)))
			{
				return true;
			}
		}
		return false;
	}

	private int FindTrackableItems(ItemContainer container, ref Item[] found)
	{
		int num = Mathf.Min(container.GetItemCount(), found.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Item item = container.GetItem(i);
			if (IsTrackableItem(item))
			{
				found[num2] = item;
				num2++;
			}
		}
		return num2;
	}

	public void BroadcastContainerInfo(ItemContainer container)
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (!(allPlayers[i] == m_Player))
			{
				player.m_MapItemTracker.UpdateTrackedItems(container);
			}
		}
		if (T17NetManager.NetOnlineMode && m_NetView != null)
		{
			m_NetView.RPC("RPC_BroadcastContainerInfo", NetTargets.Others, container.GetObjectNetID());
		}
		m_ShouldReserialize = true;
	}

	[PunRPC]
	private void RPC_BroadcastContainerInfo(int viewID, PhotonMessageInfo info)
	{
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(viewID);
		if (!(itemContainer == null))
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				allPlayers[i].m_MapItemTracker.UpdateTrackedItems(itemContainer);
			}
			m_ShouldReserialize = true;
		}
	}

	public void OnContainerViewed(ItemContainer container)
	{
		if (!(container == null))
		{
			container.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(container.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnContainerItemAdded));
			container.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Combine(container.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnContainerItemAdded));
			CheckBroadcastContainerInfo(container);
		}
	}

	public void OnContainerClosed(ItemContainer container)
	{
		if (!(container == null))
		{
			container.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(container.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnContainerItemAdded));
			CheckBroadcastContainerInfo(container);
		}
	}

	private void CheckBroadcastContainerInfo(ItemContainer container)
	{
		if ((IsTracked(container) || ContainsTrackableItems(container)) && UpdateTrackedItems(container))
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (instance != null && instance.gameType == PrisonConfig.ConfigType.Cooperative)
			{
				BroadcastContainerInfo(container);
			}
		}
	}

	public void OnContainerItemRemoved(ItemContainer container, Item item)
	{
		if (IsTracked(container))
		{
			OnContainerItemChanged(container);
		}
	}

	public void OnContainerItemAdded(ItemContainer container, Item item, bool hidden)
	{
		if (ContainsTrackableItems(container))
		{
			OnContainerItemChanged(container);
		}
	}

	private void OnContainerItemChanged(ItemContainer container)
	{
		UpdateTrackedItems(container);
	}

	public void OnGamerDisconnected()
	{
		if (m_TrackedContainers == null || m_TrackedItems == null)
		{
			return;
		}
		for (int i = 0; i < m_TrackedContainers.Count; i++)
		{
			if (m_TrackedContainers[i] != null)
			{
				StopTrackingContainer(m_TrackedContainers[i]);
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
		return true;
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

	private void UpdateNetPrisonViewData()
	{
		if (!m_IsSerializing)
		{
			string playerItemTrackingData = GlobalSerialize();
			if (NetPrisonViewDetails.Instance != null)
			{
				NetPrisonViewDetails.Instance.PlayerItemTrackingData = playerItemTrackingData;
			}
		}
	}

	public static string GlobalSerialize()
	{
		string result = string.Empty;
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_PlayerObject == null)
		{
			result = primaryGamer.m_PlayerObject.m_MapItemTracker.Serialize();
		}
		return result;
	}

	public static bool GlobalDeserialize(string data, ref string error)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
		{
			return true;
		}
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error = "GlobalDeserialize: JSON data is corrupt";
			return false;
		}
		m_IsSerializing = true;
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			allPlayers[i].m_MapItemTracker.Deserialize(netSaveData, ref error);
		}
		m_IsSerializing = false;
		return true;
	}

	private string Serialize()
	{
		m_NetSaveData.m_SerializedData.Clear();
		for (int i = 0; i < m_TrackedContainers.Count; i++)
		{
			ItemContainer itemContainer = m_TrackedContainers[i];
			if (itemContainer == null)
			{
				continue;
			}
			List<Item> value = null;
			if (m_TrackedItems.TryGetValue(itemContainer, out value))
			{
				NetSaveData.NetContainerData netContainerData = new NetSaveData.NetContainerData();
				netContainerData.viewID = itemContainer.GetObjectNetID();
				for (int j = 0; j < value.Count; j++)
				{
					netContainerData.itemViewIDs.Add(value[j].m_NetView.viewID);
				}
				m_NetSaveData.m_SerializedData.Add(netContainerData);
			}
		}
		return JsonUtility.ToJson(m_NetSaveData);
	}

	private bool Deserialize(NetSaveData data, ref string error)
	{
		for (int i = 0; i < m_TrackedContainers.Count; i++)
		{
			if (m_TrackedContainers[i] != null)
			{
				StopTrackingContainer(m_TrackedContainers[i]);
			}
		}
		bool result = true;
		if (data != null && data.m_SerializedData != null)
		{
			for (int j = 0; j < data.m_SerializedData.Count; j++)
			{
				NetSaveData.NetContainerData netContainerData = data.m_SerializedData[j];
				ItemContainer itemContainer = T17NetView.Find<ItemContainer>(netContainerData.viewID);
				if (itemContainer == null)
				{
					continue;
				}
				StartTrackingContainer(itemContainer);
				List<Item> value = null;
				if (m_TrackedItems.TryGetValue(itemContainer, out value))
				{
					for (int k = 0; k < netContainerData.itemViewIDs.Count; k++)
					{
						Item item = T17NetView.Find<Item>(netContainerData.itemViewIDs[k]);
						if (!(item == null))
						{
							value.Add(item);
						}
					}
				}
				int pinDataIndex = GetPinDataIndex(itemContainer);
				if (pinDataIndex >= 0)
				{
					PinData pinData = m_PinData[pinDataIndex];
					SetPinIcon(pinData.pinID, pinData.items[pinData.currIndex]);
				}
			}
		}
		return result;
	}

	public string CreateSnapshot()
	{
		if (m_Player == null || m_Player.m_Gamer == null || !m_Player.m_Gamer.IsLocal() || !m_Player.m_Gamer.m_bPrimaryLocal)
		{
			return string.Empty;
		}
		SaveData_MapItemTracker_V1 saveData_MapItemTracker_V = new SaveData_MapItemTracker_V1();
		saveData_MapItemTracker_V.SDATA = Serialize();
		return JsonUtility.ToJson(saveData_MapItemTracker_V);
	}

	public void StartedFromSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		SaveData_MapItemTracker_V1 saveData_MapItemTracker_V = null;
		try
		{
			saveData_MapItemTracker_V = JsonUtility.FromJson<SaveData_MapItemTracker_V1>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (saveData_MapItemTracker_V == null || saveData_MapItemTracker_V.m_Version != 1)
		{
			return;
		}
		string sDATA = saveData_MapItemTracker_V.SDATA;
		if (!string.IsNullOrEmpty(sDATA))
		{
			NetSaveData netSaveData = null;
			try
			{
				netSaveData = JsonUtility.FromJson<NetSaveData>(sDATA);
			}
			catch
			{
				return;
			}
			string error = null;
			if (Deserialize(netSaveData, ref error))
			{
			}
		}
	}
}

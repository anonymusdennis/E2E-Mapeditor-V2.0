using System;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : T17MonoBehaviour, IControlledUpdate, IDeserializable
{
	[Serializable]
	public class NetSaveData
	{
		public List<ulong> m_SerializedTags = new List<ulong>();
	}

	private const int MAX_TAGS_PER_PLAYER = 1;

	private Dictionary<int, Tag[]> m_PlayerTags = new Dictionary<int, Tag[]>();

	private T17NetView m_NetView;

	private Dictionary<int, int> m_PlayerCurrentTagIndexes = new Dictionary<int, int>();

	private bool m_bDictionarysInitialized;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private bool m_IsSerializing;

	private bool m_ShouldReserialize;

	private static TagManager m_Instance;

	public static TagManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_NetView = GetComponent<T17NetView>();
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.SlowPeriodic);
		}
		return base.StartInit();
	}

	public void Initialise()
	{
		if (m_bDictionarysInitialized)
		{
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Tag[] array = new Tag[1];
			for (int j = 0; j < array.Length; j++)
			{
				GameObject tagPrefabForPlayer = GetTagPrefabForPlayer(allPlayers[i]);
				GameObject gameObject = UnityEngine.Object.Instantiate(tagPrefabForPlayer, Vector3.one, Quaternion.identity);
				if (!(gameObject != null))
				{
					continue;
				}
				Tag component = gameObject.GetComponent<Tag>();
				if (!(component != null))
				{
					continue;
				}
				component.SetTagActive(active: false);
				if (T17NetManager.IsMasterClient)
				{
					int num = T17NetManager.AllocateSceneViewID();
					if (num != -1)
					{
						component.m_NetView.viewID = num;
					}
				}
				component.playerID = allPlayers[i].m_NetView.viewID;
				array[j] = component;
			}
			if (!m_PlayerTags.ContainsKey(allPlayers[i].m_NetView.viewID))
			{
				m_PlayerTags.Add(allPlayers[i].m_NetView.viewID, array);
				m_PlayerCurrentTagIndexes.Add(allPlayers[i].m_NetView.viewID, 0);
			}
		}
		m_bDictionarysInitialized = true;
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.SlowPeriodic);
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	public void ClearDestroyedPlayer(int viewID)
	{
		Tag[] value = null;
		if (!m_PlayerTags.TryGetValue(viewID, out value))
		{
			return;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] != null)
			{
				UnityEngine.Object.Destroy(value[i].gameObject);
				value[i] = null;
			}
		}
		m_PlayerTags.Remove(viewID);
		m_PlayerCurrentTagIndexes.Remove(viewID);
	}

	public void ControlledUpdate()
	{
		if (m_ShouldReserialize)
		{
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				UpdateNetPrisonViewData();
			}
			m_ShouldReserialize = false;
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	private Tag[] GetTagsForPlayer(int playerID)
	{
		Tag[] value = null;
		m_PlayerTags.TryGetValue(playerID, out value);
		return value;
	}

	private Tag[] GetTagsForPlayer(Player player, int viewID = -1)
	{
		if (player != null && player.m_NetView != null)
		{
			Tag[] value = null;
			m_PlayerTags.TryGetValue(player.m_NetView.viewID, out value);
			return value;
		}
		return null;
	}

	private int GetCurrentIndexForPlayer(Player player)
	{
		if (player != null && player.m_NetView != null)
		{
			return GetCurrentIndexForPlayer(player.m_NetView.viewID);
		}
		return -1;
	}

	private int GetCurrentIndexForPlayer(int playerID)
	{
		int value = -1;
		m_PlayerCurrentTagIndexes.TryGetValue(playerID, out value);
		value++;
		if (value >= 1 || value == -1)
		{
			value = 0;
		}
		if (!m_PlayerCurrentTagIndexes.ContainsKey(playerID))
		{
			m_PlayerCurrentTagIndexes.Add(playerID, value);
		}
		else
		{
			m_PlayerCurrentTagIndexes[playerID] = value;
		}
		return value;
	}

	public bool PlaceTagForPlayer(Player player, int targetRow, int targetColumn, int targetFloor)
	{
		m_NetView.RPC("RPC_Master_SetTag", NetTargets.MasterClient, player.m_NetView.viewID, targetRow, targetColumn, targetFloor);
		return true;
	}

	[PunRPC]
	private void RPC_Master_SetTag(int playerID, int row, int column, int floor, PhotonMessageInfo info)
	{
		Player player = T17NetView.Find<Player>(playerID);
		Tag[] tagsForPlayer = GetTagsForPlayer(player);
		if (tagsForPlayer.Length >= 1)
		{
			m_NetView.RPC("RPC_DeactivateTag", NetTargets.All, player.m_NetView.viewID, 0);
		}
		Tag tag = SetTag_Internal(playerID, row, column, floor);
		int num = -1;
		if (tag != null)
		{
			num = tag.m_NetView.viewID;
		}
		m_NetView.RPC("RPC_Others_SetTag", NetTargets.Others, playerID, row, column, floor, num);
		m_ShouldReserialize = true;
	}

	[PunRPC]
	private void RPC_Others_SetTag(int playerID, int row, int column, int floor, int viewID, PhotonMessageInfo info)
	{
		if (m_bDictionarysInitialized)
		{
			SetTag_Internal(playerID, row, column, floor, viewID);
			m_ShouldReserialize = true;
		}
	}

	private Tag SetTag_Internal(int playerID, int row, int column, int floor)
	{
		if (playerID <= 0)
		{
			return null;
		}
		Player player = T17NetView.Find<Player>(playerID);
		return PlaceTag(player, row, column, floor);
	}

	private Tag SetTag_Internal(int playerID, int row, int column, int floor, int viewID, bool active = true)
	{
		if (playerID <= 0)
		{
			return null;
		}
		Player player = T17NetView.Find<Player>(playerID);
		return PlaceTag(player, row, column, floor, viewID, active);
	}

	public bool RemoveTag(Player player, int row, int column, int floor)
	{
		Tag[] tagsForPlayer = GetTagsForPlayer(player);
		if (tagsForPlayer != null)
		{
			int num = -1;
			for (int i = 0; i < tagsForPlayer.Length; i++)
			{
				if (tagsForPlayer[i] != null && tagsForPlayer[i].tileRow == row && tagsForPlayer[i].tileColumn == column && tagsForPlayer[i].tileFloor == floor && tagsForPlayer[i].gameObject.activeSelf)
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				m_NetView.RPC("RPC_DeactivateTag", NetTargets.All, player.m_NetView.viewID, num);
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	private void RPC_DeactivateTag(int playerID, int index, PhotonMessageInfo info)
	{
		if (playerID > 0)
		{
			Tag[] tagsForPlayer = GetTagsForPlayer(playerID);
			if (tagsForPlayer != null && index >= 0 && index < tagsForPlayer.Length)
			{
				DeactivateTag(tagsForPlayer[index]);
				m_ShouldReserialize = true;
			}
		}
	}

	private void DeactivateTag(Tag tagToDelete)
	{
		if (tagToDelete != null)
		{
			tagToDelete.SetTagActive(active: false);
		}
	}

	public void OnGamerDisconnected(Player player)
	{
		int viewID = player.m_NetView.viewID;
		Tag[] tagsForPlayer = GetTagsForPlayer(viewID);
		if (tagsForPlayer != null)
		{
			for (int num = tagsForPlayer.Length - 1; num >= 0; num--)
			{
				DeactivateTag(tagsForPlayer[num]);
			}
		}
		m_ShouldReserialize = true;
	}

	private Tag PlaceTag(Player player, int row, int column, int floorIndex)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (instance.GetTileCentrePosition(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, row, column, out var worldPosition))
		{
			Tag[] tagsForPlayer = GetTagsForPlayer(player);
			int currentIndexForPlayer = GetCurrentIndexForPlayer(player);
			if (currentIndexForPlayer >= 0 && currentIndexForPlayer < tagsForPlayer.Length)
			{
				Tag tag = tagsForPlayer[currentIndexForPlayer];
				if (tag != null)
				{
					tag.SetPosition(worldPosition);
					FloorManager.Floor floor = instance.FindFloorbyIndex(floorIndex);
					if (floor != null && floor.m_FloorRootObject != null)
					{
						tag.transform.parent = floor.m_FloorRootObject;
					}
					tag.SetTagActive(active: true);
					return tag;
				}
			}
		}
		return null;
	}

	private Tag PlaceTag(Player player, int row, int column, int floorIndex, int viewID, bool active)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (instance.GetTileCentrePosition(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, row, column, out var worldPosition))
		{
			Tag[] tagsForPlayer = GetTagsForPlayer(player, viewID);
			int currentIndexForPlayer = GetCurrentIndexForPlayer(player);
			if (currentIndexForPlayer >= 0 && currentIndexForPlayer < tagsForPlayer.Length)
			{
				Tag tag = tagsForPlayer[currentIndexForPlayer];
				if (tag != null)
				{
					tag.playerID = player.m_NetView.viewID;
					tag.SetPosition(worldPosition);
					FloorManager.Floor floor = instance.FindFloorbyIndex(floorIndex);
					if (floor != null && floor.m_FloorRootObject != null)
					{
						tag.transform.parent = floor.m_FloorRootObject;
					}
					tag.SetTagActive(active);
					return tag;
				}
				GameObject tagPrefabForPlayer = GetTagPrefabForPlayer(player);
				GameObject gameObject = UnityEngine.Object.Instantiate(tagPrefabForPlayer, Vector3.one, Quaternion.identity);
				if (gameObject != null && viewID > -1)
				{
					tag = gameObject.GetComponent<Tag>();
					if (tag != null)
					{
						tag.SetTagActive(active);
						tag.m_NetView.viewID = viewID;
						tag.playerID = player.m_NetView.viewID;
					}
					tagsForPlayer[currentIndexForPlayer] = tag;
				}
			}
		}
		return null;
	}

	private GameObject GetTagPrefabForPlayer(Player player)
	{
		return PlayerDataManager.GetInstance().GetPlayerSpecificStuff(player.m_PlayerNumber)?.tag;
	}

	private void UpdateNetPrisonViewData()
	{
		if (!m_IsSerializing)
		{
			string playerTagData = Serialize();
			if (NetPrisonViewDetails.Instance != null)
			{
				NetPrisonViewDetails.Instance.PlayerTagData = playerTagData;
			}
		}
	}

	private string Serialize()
	{
		m_IsSerializing = true;
		m_NetSaveData.m_SerializedTags.Clear();
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			Tag[] tagsForPlayer = GetTagsForPlayer(player);
			if (tagsForPlayer != null)
			{
				for (int j = 0; j < tagsForPlayer.Length; j++)
				{
					ulong item = tagsForPlayer[j].Serialize();
					m_NetSaveData.m_SerializedTags.Add(item);
				}
			}
		}
		m_IsSerializing = false;
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public string GetSerializationData()
	{
		return NetPrisonViewDetails.Instance.PlayerTagData;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		m_IsSerializing = true;
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(data);
		}
		catch
		{
			error += "TagManager: JSON Data is currupt.";
			return false;
		}
		if (m_PlayerTags.Count > 0)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Tag[] tagsForPlayer = GetTagsForPlayer(allPlayers[i]);
				if (tagsForPlayer != null)
				{
					for (int j = 0; j < tagsForPlayer.Length; j++)
					{
						DeactivateTag(tagsForPlayer[j]);
					}
				}
			}
		}
		bool result = true;
		if (netSaveData != null && netSaveData.m_SerializedTags != null)
		{
			for (int k = 0; k < netSaveData.m_SerializedTags.Count; k++)
			{
				Tag.DeserializedTag deserializedTag = Tag.GlobalDeserialize(netSaveData.m_SerializedTags[k]);
				if (deserializedTag.viewID == -1 || deserializedTag.playerID == -1)
				{
					continue;
				}
				List<Player> allPlayers2 = Player.GetAllPlayers();
				Player player = null;
				for (int l = 0; l < allPlayers2.Count; l++)
				{
					if (allPlayers2[k] != null && allPlayers2[k].m_NetView != null && allPlayers2[k].m_NetView.viewID == deserializedTag.playerID)
					{
						player = allPlayers2[k];
						break;
					}
				}
				if (!(player != null))
				{
					continue;
				}
				Tag[] value = null;
				if (!m_PlayerTags.TryGetValue(deserializedTag.playerID, out value))
				{
					continue;
				}
				for (int m = 0; m < value.Length; m++)
				{
					if (!(value[m] != null) && value[m].m_NetView.viewID != -1 && value[m].m_NetView.viewID != deserializedTag.viewID)
					{
						continue;
					}
					value[m].m_NetView.viewID = deserializedTag.viewID;
					value[m].playerID = player.m_NetView.viewID;
					FloorManager instance = FloorManager.GetInstance();
					if (instance != null && instance.GetTileCentrePosition(deserializedTag.floor, FloorManager.TileSystem_Type.TileSystem_Ground, deserializedTag.row, deserializedTag.column, out var worldPosition))
					{
						value[m].SetPosition(worldPosition);
						FloorManager.Floor floor = instance.FindFloorbyIndex(deserializedTag.floor);
						if (floor != null && floor.m_FloorRootObject != null)
						{
							value[m].transform.parent = floor.m_FloorRootObject;
						}
						value[m].SetTagActive(deserializedTag.active);
						break;
					}
				}
				m_PlayerTags[player.m_NetView.viewID] = value;
			}
		}
		m_IsSerializing = false;
		return result;
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
}

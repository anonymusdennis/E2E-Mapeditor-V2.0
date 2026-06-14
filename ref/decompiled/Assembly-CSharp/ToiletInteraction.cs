using System;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using SaveHelpers;
using UnityEngine;

public class ToiletInteraction : AnimatedInteraction, IControlledUpdate
{
	[Serializable]
	private struct TPData
	{
		[SerializeField]
		public int Cost;

		[SerializeField]
		public int Parent;
	}

	[Serializable]
	public class NetToiletSaveData
	{
		public List<ulong> m_ToiletSerializedData = new List<ulong>();
	}

	public enum FloodingStatus
	{
		None,
		Flooding,
		Flooded,
		Draining
	}

	[Header("General")]
	public ItemContainer m_LinkedItemContainer;

	public float m_ToiletFlushTime = 10f;

	public string m_ToiletInteractionText = "Text.Interact.Toilet";

	public string m_ToiletPlumberInteractionText = "Text.Interact.Toilet.Plunger";

	public float m_InteractingZOffset = -0.1f;

	[Range(0f, 100f)]
	[Header("Flooding")]
	public float m_FloodingChance = 33f;

	public GameObject m_FloodWaterPrefab;

	public float m_FloodSpreadTime = 1f;

	public bool m_bUse8Directions = true;

	private MaterialPropertyBlock m_floodTilePropertyBlock;

	private int m_propId_FloodProgress;

	private int m_propId_1OMaxTileDistance;

	private int m_propId_FloodSourceWorldPosition;

	private float m_MaxTileDistance;

	[Header("Draining")]
	public float m_DrainTileTime = 1f;

	[Header("Debug")]
	[ReadOnly]
	public List<Vector3> m_PossibleFloodPoints;

	[ReadOnly]
	public int m_ToiletIndexInFloodPoints;

	[SerializeField]
	[ReadOnly]
	private TPData[] m_DistanceToToiletPoints;

	[ReadOnly]
	[SerializeField]
	private int m_HighestSpreadIndex;

	private Transform m_FloodWaterParent;

	private Dictionary<int, List<MeshRenderer>> m_FloodWater = new Dictionary<int, List<MeshRenderer>>();

	private float m_ElapsedFloodSpread;

	private float m_RandomizedToiletFloodTime;

	private float m_ElapsedDrainTime;

	private int m_CurrentDrainIndex;

	private int m_CurrentSpreadIndex;

	private bool m_bShouldUpdateProperties;

	private float m_LastKnowUpdateTime;

	private List<Player> m_PlayersWhoAlwaysFlood = new List<Player>();

	private static Dictionary<int, ulong> m_ToiletNetData = new Dictionary<int, ulong>();

	public ToiletEventManager m_ToiletEventManager;

	private bool m_FloodVisibilityPending;

	public STAT_IDS m_StatUponFlushing = STAT_IDS.NoneStat;

	private static NetToiletSaveData m_NetToiletSaveData = null;

	private FloodingStatus m_FloodingStatus;

	private float m_ElapsedTime;

	private bool m_bIsFlushing;

	private PlumberInteraction m_PlumberInteraction;

	private Animator m_ToiletAnimator;

	private const string FLUSH_PARAM = "Flush";

	private const string FLOOD_PARAM = "Flooded";

	private const int DIR_COUNT_8 = 8;

	private Vector3[] directions = new Vector3[8]
	{
		new Vector3(1f, 0f),
		new Vector3(1f, 1f),
		new Vector3(0f, 1f),
		new Vector3(-1f, 1f),
		new Vector3(-1f, 0f),
		new Vector3(-1f, -1f),
		new Vector3(0f, -1f),
		new Vector3(1f, -1f)
	};

	private const int DIR_COUNT_4 = 4;

	private Vector3[] directions4 = new Vector3[4]
	{
		new Vector3(1f, 0f),
		new Vector3(0f, 1f),
		new Vector3(-1f, 0f),
		new Vector3(0f, -1f)
	};

	public bool IsFlushing => m_bIsFlushing;

	public bool IsClogged => m_FloodingStatus != FloodingStatus.None;

	public float FlushPercentage
	{
		get
		{
			if (m_bIsFlushing)
			{
				return m_ElapsedTime / m_ToiletFlushTime;
			}
			return 0f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_ToiletAnimator = GetComponentInChildren<Animator>();
		m_PlumberInteraction = GetComponent<PlumberInteraction>();
		if (m_PlumberInteraction == null)
		{
			m_PlumberInteraction = base.gameObject.AddComponent<PlumberInteraction>();
			m_PlumberInteraction.CheckSelfForToiletInteraction();
		}
	}

	protected override void Init()
	{
		base.Init();
		if (m_LinkedItemContainer == null)
		{
			m_LinkedItemContainer = GetComponent<ItemContainer>();
		}
		if (m_floodTilePropertyBlock == null)
		{
			InitFloodTilePropertyBlock();
		}
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.TimeSlicedFastInteractions);
			m_LastKnowUpdateTime = Time.realtimeSinceStartup;
		}
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnRoutineChanged += RoutineChanged;
		}
	}

	private void InitFloodTilePropertyBlock()
	{
		m_floodTilePropertyBlock = new MaterialPropertyBlock();
		m_propId_FloodProgress = Shader.PropertyToID("_FloodProgress");
		m_propId_1OMaxTileDistance = Shader.PropertyToID("_1OMaxTileDistance");
		m_propId_FloodSourceWorldPosition = Shader.PropertyToID("_FloodSourceWorldPosition");
		m_floodTilePropertyBlock.SetVector(m_propId_FloodSourceWorldPosition, base.transform.position);
	}

	public static void Cleanup()
	{
		m_ToiletNetData.Clear();
		m_NetToiletSaveData = null;
	}

	protected override void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.TimeSlicedFastInteractions);
		}
		base.OnDestroy();
		m_PlumberInteraction = null;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			return localCharacter.m_CharacterRole == CharacterRole.Guard || m_FloodingStatus == FloodingStatus.None;
		}
		return false;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_LinkedItemContainer != null)
		{
			localCharacter.m_OpenContainer = m_LinkedItemContainer;
		}
		if (localCharacter.m_CharacterStats.m_bIsPlayer && localCharacter.m_OpenContainer != null)
		{
			((Player)localCharacter).ViewContainer(localCharacter.m_OpenContainer, InGameRootMenu.InGameMenuTypeToOpen.Toilet);
			InGameMenuFlow.Instance.GetCorrectIGMData(((Player)localCharacter).m_PlayerCameraManagerBindingID, out var data);
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)data.m_PlayerRootMenu.GetCurrentOpenMenu();
			if (gameMenuBehaviour != null && gameMenuBehaviour.m_MenuType == BaseMenuBehaviour.InGameMenuTypes.ToiletInventory)
			{
				ToiletMenu toiletMenu = (ToiletMenu)gameMenuBehaviour;
				toiletMenu.SetToiletInteraction(this);
			}
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (m_FloodingStatus == FloodingStatus.None && null != localCharacter && null != m_LinkedItemContainer)
		{
			m_LinkedItemContainer.MoveItemsToAnotherContainer(localCharacter.m_ItemContainer, includeHidden: false);
		}
	}

	public bool MustFloodForPlayer(Player player)
	{
		return m_PlayersWhoAlwaysFlood.Contains(player);
	}

	public void SetMustFloodForPlayer(Player player, bool flood)
	{
		bool flag = m_PlayersWhoAlwaysFlood.Contains(player);
		if (flood && !flag)
		{
			m_PlayersWhoAlwaysFlood.Add(player);
		}
		else if (!flood && flag)
		{
			m_PlayersWhoAlwaysFlood.Remove(player);
		}
	}

	public void FlushToilet(int[] itemNetViewIDs, int playerNetViewID)
	{
		m_NetObjectLock.m_NetView.RPC("RPC_SERVER_FlushToilet", NetTargets.MasterClient, itemNetViewIDs, playerNetViewID);
	}

	[PunRPC]
	private void RPC_SERVER_FlushToilet(int[] itemNetViewIDs, int playerNetViewID)
	{
		if (m_bIsFlushing)
		{
		}
		m_bIsFlushing = true;
		m_ElapsedTime = 0f;
		Player player = T17NetView.Find<Player>(playerNetViewID);
		int i = 0;
		for (int num = itemNetViewIDs.Length; i < num; i++)
		{
			if (player.m_ItemContainer.HasSpecificItem(itemNetViewIDs[i]))
			{
				Item item = T17NetView.Find<Item>(itemNetViewIDs[i]);
				player.m_ItemContainer.RemoveItemRPC(item, releaseToManager: true);
				continue;
			}
			Item equippedItem = player.GetEquippedItem();
			if (equippedItem != null && equippedItem.m_NetView != null && equippedItem.m_NetView.viewID == itemNetViewIDs[i])
			{
				player.RemoveItemRPC(equippedItem, RPC_CallContexts.Unknown, release: true);
			}
		}
		m_LinkedItemContainer.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Env_Toilet_Flush, base.gameObject);
		if (m_ToiletAnimator != null)
		{
			m_ToiletAnimator.SetTrigger("Flush");
		}
		m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FlushResponse", NetTargets.Others, m_bIsFlushing);
	}

	[PunRPC]
	private void RPC_CLIENT_FlushResponse(bool isFlushing)
	{
		m_bIsFlushing = isFlushing;
		if (m_bIsFlushing)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Env_Toilet_Flush, base.gameObject);
			m_ElapsedTime = 0f;
			if (m_ToiletAnimator != null)
			{
				m_ToiletAnimator.SetTrigger("Flush");
			}
		}
	}

	private void CreateFloodWaterParent()
	{
		if (!(m_FloodWaterParent != null))
		{
			m_FloodWaterParent = new GameObject(base.gameObject.name + "_Floodwater").transform;
			m_FloodWaterParent.SetParent(base.transform.parent);
			m_FloodWaterParent.localPosition = Vector3.zero;
			m_FloodWaterParent.localScale = Vector3.one;
		}
	}

	[ContextMenu("Flood Toilet")]
	public void FloodToilet()
	{
		m_NetObjectLock.m_NetView.RPC("RPC_SERVER_FloodToilet", NetTargets.MasterClient);
	}

	[PunRPC]
	private void RPC_SERVER_FloodToilet()
	{
		if (m_FloodingStatus != 0)
		{
			return;
		}
		m_CurrentSpreadIndex = 0;
		m_FloodingStatus = FloodingStatus.Flooding;
		SetInteractionTextForCurrentState();
		m_RandomizedToiletFloodTime = UnityEngine.Random.Range(m_FloodSpreadTime - 0.5f, m_FloodSpreadTime + 0.5f);
		m_FloodWater.Clear();
		if (m_FloodWaterParent == null)
		{
			CreateFloodWaterParent();
		}
		m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Env_Toilet_Flood, base.gameObject);
		if (m_ToiletAnimator != null)
		{
			m_ToiletAnimator.SetBool("Flooded", value: true);
		}
		m_bShouldUpdateProperties = true;
		CreateAllFloodLevelTiles();
		if (m_ToiletEventManager != null)
		{
			if (RoutineManager.GetInstance() != null && RoutineManager.GetInstance().GetCurrentRoutine() != null && RoutineManager.GetInstance().GetCurrentRoutine().m_SubRoutineType != RoutineSubTypes.JobTime)
			{
				m_ToiletEventManager.UpdateFloodEventVisibility(isFlooded: true);
			}
			else
			{
				m_FloodVisibilityPending = true;
			}
		}
	}

	[PunRPC]
	private void RPC_CLIENT_FloodResponse(FloodingStatus floodStatus, int currentSpreadIndex)
	{
		bool flag = false;
		if (m_FloodingStatus == FloodingStatus.None && floodStatus == FloodingStatus.Flooding)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Env_Toilet_Flood, base.gameObject);
		}
		if (m_ToiletAnimator != null)
		{
			m_ToiletAnimator.SetBool("Flooded", floodStatus == FloodingStatus.Flooding || floodStatus == FloodingStatus.Flooded);
		}
		m_FloodingStatus = floodStatus;
		SetInteractionTextForCurrentState();
		if (m_FloodWaterParent == null)
		{
			CreateFloodWaterParent();
			CreateAllFloodLevelTiles();
			flag = true;
		}
		Debug.Log(string.Concat("  RPC_CLIENT_FloodResponse   ", floodStatus, "    currentSpreadIndex = ", currentSpreadIndex, "   bInitialResponse ", flag));
		m_CurrentSpreadIndex = currentSpreadIndex;
		switch (m_FloodingStatus)
		{
		case FloodingStatus.None:
			DestroyFloodLevelTiles();
			break;
		case FloodingStatus.Flooded:
			if (flag)
			{
				m_CurrentSpreadIndex = m_HighestSpreadIndex;
				m_bShouldUpdateProperties = true;
			}
			break;
		}
		UpdateFloodTileEffects();
	}

	[ContextMenu("Unclog Toilet")]
	public void UnClogToilet()
	{
		m_NetObjectLock.m_NetView.RPC("RPC_SERVER_UnClogToilet", NetTargets.MasterClient);
	}

	[PunRPC]
	private void RPC_SERVER_UnClogToilet()
	{
		if (m_FloodingStatus == FloodingStatus.Flooded || m_FloodingStatus == FloodingStatus.Flooding)
		{
			m_LinkedItemContainer.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
			m_CurrentDrainIndex = m_FloodWater.Count - 1;
			m_FloodingStatus = FloodingStatus.Draining;
			SetInteractionTextForCurrentState();
			if (m_ToiletAnimator != null)
			{
				m_ToiletAnimator.SetBool("Flooded", value: false);
			}
			m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
			m_bShouldUpdateProperties = true;
		}
	}

	public override bool InteractionVisibility()
	{
		return m_FloodingStatus == FloodingStatus.None;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public void ControlledUpdate()
	{
		float num = Time.realtimeSinceStartup - m_LastKnowUpdateTime;
		m_LastKnowUpdateTime = Time.realtimeSinceStartup;
		if (m_bIsFlushing)
		{
			m_ElapsedTime += num;
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				if (m_ElapsedTime >= m_ToiletFlushTime)
				{
					m_ElapsedTime = 0f;
					m_bIsFlushing = false;
					m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FlushResponse", NetTargets.Others, m_bIsFlushing);
					m_bShouldUpdateProperties = true;
				}
			}
			else if (m_ElapsedTime >= m_ToiletFlushTime)
			{
				m_ElapsedTime = m_ToiletFlushTime;
			}
		}
		if ((T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient)) && m_DistanceToToiletPoints != null)
		{
			switch (m_FloodingStatus)
			{
			case FloodingStatus.Flooding:
				m_ElapsedFloodSpread += num;
				if (m_ElapsedFloodSpread >= m_RandomizedToiletFloodTime)
				{
					m_RandomizedToiletFloodTime = UnityEngine.Random.Range(m_FloodSpreadTime - 0.5f, m_FloodSpreadTime + 0.5f);
					m_ElapsedFloodSpread = 0f;
					m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
					m_CurrentSpreadIndex++;
					if (m_CurrentSpreadIndex > m_HighestSpreadIndex)
					{
						m_CurrentSpreadIndex = m_HighestSpreadIndex;
						m_FloodingStatus = FloodingStatus.Flooded;
						SetInteractionTextForCurrentState();
						m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
					}
					m_bShouldUpdateProperties = true;
					UpdateFloodTileEffects();
				}
				break;
			case FloodingStatus.Flooded:
				if (m_FloodWaterParent == null)
				{
					m_CurrentSpreadIndex = m_HighestSpreadIndex;
					CreateFloodWaterParent();
					CreateAllFloodLevelTiles();
					m_bShouldUpdateProperties = true;
					m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
				}
				break;
			case FloodingStatus.Draining:
				m_ElapsedDrainTime += num;
				if (!(m_ElapsedDrainTime >= m_DrainTileTime))
				{
					break;
				}
				m_ElapsedDrainTime = 0f;
				m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
				m_CurrentDrainIndex--;
				m_CurrentSpreadIndex--;
				if (m_CurrentDrainIndex < 0)
				{
					m_CurrentSpreadIndex = 0;
					m_FloodingStatus = FloodingStatus.None;
					SetInteractionTextForCurrentState();
					if (m_ToiletEventManager != null)
					{
						m_ToiletEventManager.UpdateFloodEventVisibility(isFlooded: false);
						m_FloodVisibilityPending = false;
					}
					DestroyFloodLevelTiles();
					if (m_FloodWater.Count > 0)
					{
					}
					m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_CLIENT_FloodResponse", NetTargets.Others, m_FloodingStatus, m_CurrentSpreadIndex);
				}
				m_bShouldUpdateProperties = true;
				UpdateFloodTileEffects();
				break;
			}
		}
		if (m_bShouldUpdateProperties)
		{
			m_bShouldUpdateProperties = false;
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				UpdateNetPrisonViewData(this);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	private void CreateAllFloodLevelTiles()
	{
		if (m_ToiletEventManager != null)
		{
			m_ToiletEventManager.ClearOffsets();
		}
		if (m_floodTilePropertyBlock == null)
		{
			InitFloodTilePropertyBlock();
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < m_DistanceToToiletPoints.Length; i++)
		{
			int cost = m_DistanceToToiletPoints[i].Cost;
			Vector3 vector = m_PossibleFloodPoints[i];
			if (cost != -1)
			{
				if (!m_FloodWater.ContainsKey(cost))
				{
					m_FloodWater.Add(cost, new List<MeshRenderer>());
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(m_FloodWaterPrefab);
				gameObject.transform.SetParent(m_FloodWaterParent);
				gameObject.transform.position = vector;
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0f);
				MeshRenderer componentInChildren = gameObject.GetComponentInChildren<MeshRenderer>();
				componentInChildren.SetPropertyBlock(m_floodTilePropertyBlock);
				m_FloodWater[cost].Add(componentInChildren);
				CullingObjectCollector.GetInstance().Runtime_AddToBucket(componentInChildren, bCheckForMaterialBlock: true, bAlsoFloorsAbove: true);
			}
			if (m_ToiletEventManager != null)
			{
				m_ToiletEventManager.AddOffet(vector - position);
			}
			float magnitude = (vector - base.transform.position).magnitude;
			if (magnitude > m_MaxTileDistance)
			{
				m_MaxTileDistance = magnitude + 0.5f;
			}
		}
		m_floodTilePropertyBlock.SetFloat(m_propId_1OMaxTileDistance, 1f / m_MaxTileDistance);
		UpdateFloodTileEffects();
	}

	private void DestroyFloodLevelTiles()
	{
		if (m_ToiletEventManager != null)
		{
			m_ToiletEventManager.ClearOffsets();
		}
		CullingObjectCollector instance = CullingObjectCollector.GetInstance();
		foreach (KeyValuePair<int, List<MeshRenderer>> item in m_FloodWater)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				if (item.Value[i] != null)
				{
					instance.Runtime_RemoveFromBucket(item.Value[i], bCheckForMaterialBlock: true);
					UnityEngine.Object.Destroy(item.Value[i].gameObject);
				}
			}
		}
		m_FloodWater.Clear();
		CameraManager.GetInstance().ForceAnUpdateForActiveCameras();
	}

	private void UpdateFloodTileEffects()
	{
		if (m_floodTilePropertyBlock == null)
		{
			InitFloodTilePropertyBlock();
		}
		float num = (float)m_CurrentSpreadIndex / (float)m_HighestSpreadIndex;
		m_floodTilePropertyBlock.SetFloat(m_propId_FloodProgress, num);
		Debug.Log(" *** UpdateFloodTileEffects  floodProgress=" + num + "   m_HighestSpreadIndex=" + m_HighestSpreadIndex + "    " + m_FloodWater.Count);
		for (int i = 0; i <= m_HighestSpreadIndex; i++)
		{
			if (!m_FloodWater.ContainsKey(i))
			{
				continue;
			}
			List<MeshRenderer> list = m_FloodWater[i];
			if (list == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null)
				{
					list[j].SetPropertyBlock(m_floodTilePropertyBlock);
				}
			}
		}
	}

	private static void UpdateNetPrisonViewData(ToiletInteraction toilet)
	{
		ulong value = toilet.Serialize();
		if (m_NetToiletSaveData == null)
		{
			m_NetToiletSaveData = new NetToiletSaveData();
		}
		int viewID = toilet.m_NetObjectLock.m_NetView.viewID;
		if (m_ToiletNetData.ContainsKey(viewID))
		{
			m_ToiletNetData[viewID] = value;
		}
		else
		{
			m_ToiletNetData.Add(viewID, value);
		}
		if (NetPrisonViewDetails.Instance != null)
		{
			m_NetToiletSaveData.m_ToiletSerializedData = m_ToiletNetData.Values.ToList();
			NetPrisonViewDetails.Instance.ToiletInteractionData = JsonUtility.ToJson(m_NetToiletSaveData);
		}
	}

	private void SetInteractionTextForCurrentState()
	{
		string text = ((m_FloodingStatus != 0 && m_FloodingStatus != FloodingStatus.Draining) ? m_ToiletPlumberInteractionText : m_ToiletInteractionText);
		Localization.Get(text, out var localized);
		m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localized);
	}

	private ulong Serialize()
	{
		Debug.Log(" ****** Serialize ***** ");
		BitField bitField = new BitField();
		bitField.Set(12, (uint)m_NetObjectLock.m_NetView.viewID);
		bitField.Set(m_bIsFlushing);
		bitField.Set(3, (uint)m_FloodingStatus);
		if (m_FloodingStatus == FloodingStatus.Flooding)
		{
			bitField.Set(6, (uint)m_CurrentSpreadIndex);
		}
		else if (m_FloodingStatus == FloodingStatus.Draining)
		{
			bitField.Set(6, (uint)m_CurrentDrainIndex);
		}
		return (ulong)bitField;
	}

	public static bool GlobalDeserialize(string data, ref string error)
	{
		Debug.Log(" ****** GlobalDeserialize ***** ");
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		NetToiletSaveData netToiletSaveData = null;
		try
		{
			netToiletSaveData = JsonUtility.FromJson<NetToiletSaveData>(data);
		}
		catch
		{
			error = "GlobalDeserialize: JSON data is corrupt";
			return false;
		}
		for (int i = 0; i < netToiletSaveData.m_ToiletSerializedData.Count; i++)
		{
			ulong num = netToiletSaveData.m_ToiletSerializedData[i];
			if (num == 0)
			{
				continue;
			}
			BitField bitField = new BitField(num);
			int uInt = (int)bitField.GetUInt(12);
			PhotonView photonView = PhotonView.Find(uInt);
			if (photonView != null)
			{
				ToiletInteraction component = photonView.GetComponent<ToiletInteraction>();
				if (component != null)
				{
					component.Deserialize(bitField);
				}
			}
		}
		return true;
	}

	private void Deserialize(BitField ourField)
	{
		Debug.Log(" ****** Deserialize ***** ");
		m_bIsFlushing = ourField.GetBool();
		m_FloodingStatus = (FloodingStatus)ourField.GetUInt(3);
		SetInteractionTextForCurrentState();
		switch (m_FloodingStatus)
		{
		case FloodingStatus.Flooding:
			m_CurrentSpreadIndex = (int)ourField.GetUInt(6);
			break;
		case FloodingStatus.Flooded:
			m_CurrentSpreadIndex = m_HighestSpreadIndex;
			break;
		case FloodingStatus.Draining:
			m_CurrentDrainIndex = (int)ourField.GetUInt(6);
			break;
		}
	}

	public FloodingStatus GetFloodingStatus()
	{
		return m_FloodingStatus;
	}

	private void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (oldRoutine != null && oldRoutine.m_SubRoutineType == RoutineSubTypes.JobTime && m_ToiletEventManager != null && m_FloodVisibilityPending)
		{
			m_ToiletEventManager.UpdateFloodEventVisibility(isFlooded: true);
			m_FloodVisibilityPending = false;
		}
	}

	public void SetToiletFloodPoints(ref List<Vector3> floodpoints, RoomFloor roomFloor)
	{
		m_PossibleFloodPoints = floodpoints.ToList();
		m_ToiletIndexInFloodPoints = 0;
		m_HighestSpreadIndex = -1;
		m_DistanceToToiletPoints = null;
		ConvertRoomToWorldPoints(roomFloor);
		FloodFillForAllowedPoints();
	}

	private void ConvertRoomToWorldPoints(RoomFloor roomfloor)
	{
		for (int i = 0; i < m_PossibleFloodPoints.Count; i++)
		{
			Vector3 pos = m_PossibleFloodPoints[i];
			pos.x += 1f;
			pos.y += 1f;
			m_PossibleFloodPoints[i] = RoomUtility.RoomGridToWorld(pos, roomfloor);
			if ((m_PossibleFloodPoints[i] - base.transform.position).sqrMagnitude < 1f)
			{
				m_ToiletIndexInFloodPoints = i;
			}
		}
	}

	[ContextMenu("Try FloodFil")]
	private void FloodFillForAllowedPoints()
	{
		HashSet<int> hashSet = new HashSet<int>();
		m_DistanceToToiletPoints = new TPData[m_PossibleFloodPoints.Count];
		TPData tPData = default(TPData);
		for (int i = 0; i < m_DistanceToToiletPoints.Length; i++)
		{
			tPData.Cost = -1;
			tPData.Parent = -1;
			m_DistanceToToiletPoints[i] = tPData;
		}
		Queue<int> queue = new Queue<int>();
		queue.Enqueue(m_ToiletIndexInFloodPoints);
		hashSet.Add(m_ToiletIndexInFloodPoints);
		TPData tPData2 = default(TPData);
		tPData2.Cost = 0;
		tPData2.Parent = m_ToiletIndexInFloodPoints;
		m_DistanceToToiletPoints[m_ToiletIndexInFloodPoints] = tPData2;
		TPData tPData3 = default(TPData);
		while (queue.Count != 0)
		{
			int num = queue.Dequeue();
			List<int> pointNeighbours = GetPointNeighbours(num);
			while (pointNeighbours.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, pointNeighbours.Count);
				int num2 = pointNeighbours[index];
				pointNeighbours.RemoveAt(index);
				if (hashSet.Contains(num2))
				{
					continue;
				}
				hashSet.Add(num2);
				Vector3 vector = m_PossibleFloodPoints[num2];
				int mask = LayerMask.GetMask("Wall", "Fence", "Door");
				if (!Physics.Raycast(vector - Vector3.forward * 2f, Vector3.forward, 1.8f, mask))
				{
					queue.Enqueue(num2);
					tPData3.Cost = m_DistanceToToiletPoints[num].Cost + 1;
					tPData3.Parent = num;
					m_DistanceToToiletPoints[num2] = tPData3;
					if (tPData3.Cost > m_HighestSpreadIndex)
					{
						m_HighestSpreadIndex = tPData3.Cost;
					}
				}
			}
		}
	}

	private List<int> GetPointNeighbours(int index)
	{
		List<int> list = new List<int>();
		if (m_bUse8Directions)
		{
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector = m_PossibleFloodPoints[index] + directions[i];
				for (int j = 0; j < m_PossibleFloodPoints.Count; j++)
				{
					if (index != j && (m_PossibleFloodPoints[j] - vector).sqrMagnitude < 1f)
					{
						list.Add(j);
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < 4; k++)
			{
				Vector3 vector2 = m_PossibleFloodPoints[index] + directions4[k];
				for (int l = 0; l < m_PossibleFloodPoints.Count; l++)
				{
					if (index != l && (m_PossibleFloodPoints[l] - vector2).sqrMagnitude < 1f)
					{
						list.Add(l);
					}
				}
			}
		}
		return list;
	}

	private Color MakeColorGradient(int index, float frequency1, float frequency2, float frequency3, float phase1, float phase2, float phase3, float center = float.NegativeInfinity, float width = float.NegativeInfinity)
	{
		if (center == float.NegativeInfinity)
		{
			center = 128f;
		}
		if (width == float.NegativeInfinity)
		{
			width = 127f;
		}
		float r = (Mathf.Sin(frequency1 * (float)index + phase1) * width + center) / 255f;
		float g = (Mathf.Sin(frequency2 * (float)index + phase2) * width + center) / 255f;
		float b = (Mathf.Sin(frequency3 * (float)index + phase3) * width + center) / 255f;
		return new Color(r, g, b);
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

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}

	protected override void UpdateInteractionZ_PreTransitionStart()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_Interacting()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_PostTransitionEnd()
	{
		SetInteractionZ();
	}

	private void SetInteractionZ()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_InteractingZOffset);
		}
	}
}

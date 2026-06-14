using System;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuFlow : BaseFlowBehaviour
{
	[Serializable]
	public class PlayerIGMData
	{
		public CameraManager.PlayerBindingID m_PlayerBindingID;

		public Canvas m_ParentObject;

		public Transform m_MaskContainer;

		public GameObject m_DialogBoxParentObject;

		public InGameRootMenu m_PlayerRootMenu;

		public SafeSpaceHandler m_PlayerSafeSpace;

		public bool m_MousePlayer;

		private PlayerInventoryMenu m_PlayerInventory;

		private int m_ShowMenuCount;

		[HideInInspector]
		public Camera m_PlayerCamera;

		public MainMapMenu m_Map;

		public CalendarMenu m_Calendar;

		public JobBoardMenu m_JobBoard;

		public PayphoneMenu m_Payphone;

		public PlayerSelectMenu m_PlayerSelect;

		public PlayerSelectMenu m_DemoPlayerSelect;

		public PauseMenu m_PauseMenu;

		public PauseMenu m_DemoPauseMenu;

		public JobTutorialMenu m_JobTutorialMenu;

		public GuardBoard m_GuardBoardMenu;

		public InformationBoard m_InformationBoardMenu;

		public SignPost m_SignPostMenu;

		public BedSaveMenu m_BedSaveMenu;

		public Transform m_ClickOffButton;

		public PlayerMapsSnapshot m_PreOverlayPlayerSnapshot;

		public PlayerInventoryMenu PlayerInventory
		{
			get
			{
				if (m_PlayerInventory == null && m_PlayerRootMenu != null)
				{
					m_PlayerInventory = m_PlayerRootMenu.GetComponentInChildren<PlayerInventoryMenu>(includeInactive: true);
				}
				return m_PlayerInventory;
			}
		}

		public bool AnyMenusOpen => m_ShowMenuCount > 0;

		public int OpenMenuCount
		{
			get
			{
				return m_ShowMenuCount;
			}
			set
			{
				if (value < 0)
				{
					m_ShowMenuCount = 0;
					return;
				}
				if (m_ClickOffButton != null)
				{
					m_ClickOffButton.gameObject.SetActive(value != 0 && m_MousePlayer);
				}
				m_ShowMenuCount = value;
			}
		}

		public void HideActiveMenus()
		{
			if (m_Calendar != null && m_Calendar.gameObject.activeSelf)
			{
				m_Calendar.Hide();
			}
			if (m_JobBoard != null && m_JobBoard.gameObject.activeSelf)
			{
				m_JobBoard.Hide();
			}
			if (m_JobTutorialMenu != null && m_JobTutorialMenu.gameObject.activeSelf)
			{
				m_JobTutorialMenu.Hide();
			}
			if (m_Map != null && m_Map.gameObject.activeSelf)
			{
				m_Map.HideMap();
			}
			if (m_Payphone != null && m_Payphone.gameObject.activeSelf)
			{
				m_Payphone.Hide();
			}
			if (m_GuardBoardMenu != null && m_GuardBoardMenu.gameObject.activeSelf)
			{
				m_GuardBoardMenu.Hide();
			}
			if (m_InformationBoardMenu != null && m_InformationBoardMenu.gameObject.activeSelf)
			{
				m_InformationBoardMenu.Hide();
			}
			if (m_SignPostMenu != null && m_SignPostMenu.gameObject.activeSelf)
			{
				m_SignPostMenu.Hide();
			}
			if (m_BedSaveMenu != null && m_BedSaveMenu.gameObject.activeSelf)
			{
				m_BedSaveMenu.Hide();
			}
		}

		public void ClickOffCloseContainer()
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (allPlayers[i] != null && allPlayers[i].m_PlayerCameraManagerBindingID == m_PlayerBindingID)
				{
					if (AnyMenusOpen && m_PlayerRootMenu != null)
					{
						allPlayers[i].RequestCloseContainer();
						allPlayers[i].RequestStopInteraction();
					}
					break;
				}
			}
		}
	}

	private static InGameMenuFlow m_Instance;

	public List<PlayerIGMData> m_PlayersIGMData = new List<PlayerIGMData>();

	public Image m_FSSplitScreenDivider;

	public GameObject m_PauseMenuCoopText;

	private Dictionary<Gamer, CameraManager.PlayerBindingID> m_GamerCameraIndex = new Dictionary<Gamer, CameraManager.PlayerBindingID>();

	private int m_PreviousCameraCount;

	private bool m_bMenuInstanceCheckRequested;

	private bool m_bMenuInstancesInitialised;

	public IGMTutorialArrowController m_TutorialController;

	public static InGameMenuFlow Instance => m_Instance;

	public CameraManager.PlayerBindingID GetCameraIndexForGamer(Gamer gamer)
	{
		if (gamer != null && m_GamerCameraIndex.ContainsKey(gamer))
		{
			return m_GamerCameraIndex[gamer];
		}
		return CameraManager.PlayerBindingID.CM_PBID_UNSET;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		if (m_FSSplitScreenDivider != null)
		{
			m_FSSplitScreenDivider.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i] != null && m_PlayersIGMData[i].m_ParentObject != null)
			{
				m_PlayersIGMData[i].m_ParentObject.enabled = false;
			}
		}
		NetLoadSync.RegisterOnReadyToPlayInterest(OnLoadComplete);
		Gamer.OnCreate += OnGamerCreated;
		T17NetManager.OnPhotonConnectionChangeEvent += OnNetworkConnectionChange;
		Platform instance = Platform.GetInstance();
		instance.m_PauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance.m_PauseCallBack, new Platform.PlatformPauseRequest(OnPlatformPauseRequest));
		Platform instance2 = Platform.GetInstance();
		instance2.m_PauseCallBack = (Platform.PlatformPauseRequest)Delegate.Combine(instance2.m_PauseCallBack, new Platform.PlatformPauseRequest(OnPlatformPauseRequest));
	}

	protected override void OnDestroy()
	{
		Platform instance = Platform.GetInstance();
		instance.m_PauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance.m_PauseCallBack, new Platform.PlatformPauseRequest(OnPlatformPauseRequest));
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		if (CameraManager.GetInstance() != null)
		{
			CameraManager instance2 = CameraManager.GetInstance();
			instance2.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance2.OnTargetsUpdated, new CameraManager.CameraManagerHandler(RequestMenuInstanceCheck));
		}
		NetLoadSync.RegisterOnReadyToPlayInterest(OnLoadComplete, bAdd: false);
		Gamer.OnCreate -= OnGamerCreated;
		T17NetManager.OnPhotonConnectionChangeEvent -= OnNetworkConnectionChange;
		base.OnDestroy();
	}

	protected override void Start()
	{
		base.Start();
		if (CameraManager.GetInstance() != null)
		{
			CameraManager instance = CameraManager.GetInstance();
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(RequestMenuInstanceCheck));
		}
		if ((LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial) && m_TutorialController != null)
		{
			m_TutorialController.enabled = false;
		}
	}

	private void RequestMenuInstanceCheck()
	{
		if (MenusEnabled() || !m_bMenuInstancesInitialised)
		{
			CheckMenuInstances();
			m_bMenuInstancesInitialised = true;
		}
		else
		{
			m_bMenuInstanceCheckRequested = true;
		}
	}

	private bool MenusEnabled()
	{
		if (m_PlayersIGMData == null || m_PlayersIGMData[0] == null || m_PlayersIGMData[0].m_ParentObject == null)
		{
			return false;
		}
		return m_PlayersIGMData[0].m_ParentObject.isActiveAndEnabled;
	}

	private void CheckMenuInstances()
	{
		int usedCameraCount = CameraManager.GetInstance().GetUsedCameraCount();
		int count = m_PlayersIGMData.Count;
		HUDMenuFlow instance = HUDMenuFlow.Instance;
		if (m_PlayersIGMData.Count < usedCameraCount && m_PlayersIGMData.Count > 0)
		{
			int num = usedCameraCount - m_PlayersIGMData.Count;
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_PlayersIGMData[0].m_ParentObject.gameObject);
				gameObject.transform.SetParent(m_PlayersIGMData[0].m_ParentObject.transform.parent);
				InGameRootMenu componentInChildren = gameObject.GetComponentInChildren<InGameRootMenu>(includeInactive: true);
				PlayerIGMData playerIGMData = new PlayerIGMData();
				SafeSpaceHandler componentInChildren2 = gameObject.GetComponentInChildren<SafeSpaceHandler>(includeInactive: true);
				T17ItemTooltip[] componentsInChildren = gameObject.GetComponentsInChildren<T17ItemTooltip>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[j].gameObject);
				}
				playerIGMData.m_ParentObject = gameObject.GetComponent<Canvas>();
				playerIGMData.m_PlayerRootMenu = componentInChildren;
				playerIGMData.m_PlayerSafeSpace = componentInChildren2;
				playerIGMData.m_Map = gameObject.GetComponentInChildren<MainMapMenu>(includeInactive: true);
				playerIGMData.m_Map.FirstTimeSetup();
				playerIGMData.m_Calendar = gameObject.GetComponentInChildren<CalendarMenu>(includeInactive: true);
				playerIGMData.m_JobBoard = gameObject.GetComponentInChildren<JobBoardMenu>(includeInactive: true);
				playerIGMData.m_Payphone = gameObject.GetComponentInChildren<PayphoneMenu>(includeInactive: true);
				playerIGMData.m_PlayerSelect = gameObject.GetComponentInChildren<PlayerSelectMenu>(includeInactive: true);
				playerIGMData.m_PauseMenu = GetComponentInChildren<PauseMenu>(includeInactive: true);
				playerIGMData.m_JobTutorialMenu = gameObject.GetComponentInChildren<JobTutorialMenu>(includeInactive: true);
				playerIGMData.m_GuardBoardMenu = gameObject.GetComponentInChildren<GuardBoard>(includeInactive: true);
				playerIGMData.m_InformationBoardMenu = gameObject.GetComponentInChildren<InformationBoard>(includeInactive: true);
				playerIGMData.m_SignPostMenu = gameObject.GetComponentInChildren<SignPost>(includeInactive: true);
				playerIGMData.m_BedSaveMenu = gameObject.GetComponentInChildren<BedSaveMenu>(includeInactive: true);
				playerIGMData.m_MaskContainer = gameObject.transform.FindChild("MaskContainer");
				playerIGMData.m_ClickOffButton = gameObject.transform.FindChild("ClickOffButton");
				List<Player> allPlayers = Player.GetAllPlayers();
				Player player = allPlayers.Find((Player x) => x.m_Gamer != null && x.m_Gamer.m_RewiredPlayer != null && x.m_Gamer.m_RewiredPlayer.controllers.hasMouse);
				if (player != null && playerIGMData.m_PlayerBindingID == player.m_PlayerCameraManagerBindingID)
				{
					playerIGMData.m_MousePlayer = true;
				}
				else
				{
					playerIGMData.m_MousePlayer = false;
				}
				if (playerIGMData.m_MaskContainer != null)
				{
					playerIGMData.m_DialogBoxParentObject = playerIGMData.m_MaskContainer.transform.FindChild("DialogBoxParent").gameObject;
				}
				playerIGMData.m_PlayerRootMenu.Hide();
				playerIGMData.HideActiveMenus();
				m_PlayersIGMData.Add(playerIGMData);
			}
		}
		for (int k = 0; k < m_PlayersIGMData.Count; k++)
		{
			if (m_PlayersIGMData[k] == null)
			{
				continue;
			}
			if (!m_PlayersIGMData[k].m_ParentObject.isActiveAndEnabled)
			{
				m_PlayersIGMData[k].m_ParentObject.gameObject.SetActive(value: true);
			}
			if (instance != null)
			{
				HUDItemsLayout posScale = instance.m_SplitscreenHUDHandler.GetPosScale(usedCameraCount, k);
				if (posScale == null)
				{
					continue;
				}
				if (m_PlayersIGMData[k].m_ClickOffButton != null)
				{
					T17Button component = m_PlayersIGMData[k].m_ClickOffButton.GetComponent<T17Button>();
					if (component != null)
					{
						component.onClick.RemoveAllListeners();
						component.onClick.AddListener(m_PlayersIGMData[k].ClickOffCloseContainer);
					}
				}
				if (m_PlayersIGMData[k].m_MaskContainer != null)
				{
					ResizeRect((RectTransform)m_PlayersIGMData[k].m_MaskContainer, posScale.m_MaskContainerSize, posScale.m_MaskContainerPosition);
				}
				RectTransform rectTransform = (RectTransform)m_PlayersIGMData[k].m_PlayerRootMenu.transform;
				if (rectTransform != null)
				{
					RectTransform rectTransform2 = rectTransform;
					RectTransform parent = (RectTransform)rectTransform.parent;
					rectTransform2.SetParent(parent, worldPositionStays: true);
					if (posScale != null)
					{
						Vector2 iGMScale = posScale.m_IGMScale;
						rectTransform2.transform.localScale = new Vector3(iGMScale.x, iGMScale.y, 1f);
						rectTransform2.transform.localPosition = posScale.m_IGMPosition;
						rectTransform = rectTransform2;
					}
				}
				RectTransform rectTransform3 = (RectTransform)m_PlayersIGMData[k].m_Map.transform;
				if (rectTransform3 != null)
				{
					RectTransform rectTransform4 = rectTransform3;
					RectTransform parent2 = (RectTransform)rectTransform3.parent;
					rectTransform4.SetParent(parent2, worldPositionStays: true);
					if (posScale != null)
					{
						rectTransform3 = rectTransform4;
						Vector2 scale = new Vector2(1f, 1f);
						m_PlayersIGMData[k].m_Map.SetScale(scale);
						RectTransform key = m_PlayersIGMData[k].m_Map.m_Key;
						if (key != null)
						{
							key.transform.localPosition = posScale.m_FullScreenMapKeyOffset;
							key.transform.localScale = new Vector3(posScale.m_FullScreenMapKeyScale.x, posScale.m_FullScreenMapKeyScale.y, 1f);
						}
						RectTransform floor = m_PlayersIGMData[k].m_Map.m_Floor;
						if (floor != null)
						{
							floor.transform.localPosition = posScale.m_FullScreenMapFloorsOffset;
							floor.transform.localScale = new Vector3(posScale.m_FullScreenMapFloorsScale.x, posScale.m_FullScreenMapFloorsScale.y, 1f);
						}
						RectTransform legend = m_PlayersIGMData[k].m_Map.m_Legend;
						if (legend != null)
						{
							legend.transform.localPosition = posScale.m_FullScreenMapLegendOffset;
							legend.transform.localScale = new Vector3(posScale.m_FullScreenMapLegendScale.x, posScale.m_FullScreenMapLegendScale.y, 1f);
						}
					}
				}
				RectTransform rectTransform5 = (RectTransform)m_PlayersIGMData[k].m_Calendar.transform;
				if (rectTransform5 != null)
				{
					RectTransform rectTransform6 = rectTransform5;
					RectTransform parent3 = (RectTransform)rectTransform.parent;
					rectTransform6.SetParent(parent3, worldPositionStays: true);
					if (posScale != null)
					{
						Vector2 iGMScale2 = posScale.m_IGMScale;
						rectTransform6.transform.localScale = new Vector3(iGMScale2.x, iGMScale2.y, 1f);
						rectTransform6.transform.localPosition = posScale.m_CalendarPosition;
						rectTransform5 = rectTransform6;
					}
				}
				RectTransform rectTransform7 = (RectTransform)m_PlayersIGMData[k].m_JobBoard.transform;
				if (rectTransform7 != null)
				{
					RectTransform rectTransform8 = rectTransform7;
					RectTransform parent4 = (RectTransform)rectTransform7.parent;
					rectTransform8.SetParent(parent4, worldPositionStays: true);
					if (posScale != null)
					{
						Vector2 jobsBoardScale = posScale.m_JobsBoardScale;
						rectTransform8.transform.localScale = new Vector3(jobsBoardScale.x, jobsBoardScale.y, 1f);
						rectTransform8.transform.localPosition = posScale.m_JobsBoardPosition;
						rectTransform7 = rectTransform8;
					}
				}
				RectTransform rectTransform9 = (RectTransform)m_PlayersIGMData[k].m_Payphone.transform;
				if (rectTransform9 != null)
				{
					RectTransform rectTransform10 = rectTransform9;
					RectTransform parent5 = (RectTransform)rectTransform9.parent;
					rectTransform10.SetParent(parent5, worldPositionStays: true);
					if (posScale != null)
					{
						Vector2 payphoneScale = posScale.m_PayphoneScale;
						rectTransform10.transform.localScale = new Vector3(payphoneScale.x, payphoneScale.y, 1f);
						rectTransform10.transform.localPosition = posScale.m_PayphonePosition;
						rectTransform9 = rectTransform10;
					}
				}
				RectTransform rectTransform11 = (RectTransform)m_PlayersIGMData[k].m_BedSaveMenu.transform;
				if (rectTransform11 != null)
				{
					RectTransform rectTransform12 = rectTransform11;
					RectTransform parent6 = (RectTransform)rectTransform11.parent;
					rectTransform12.SetParent(parent6, worldPositionStays: true);
					if (posScale != null)
					{
						Vector2 bedSaveMenuScale = posScale.m_BedSaveMenuScale;
						rectTransform12.transform.localScale = new Vector3(bedSaveMenuScale.x, bedSaveMenuScale.y, 1f);
						rectTransform12.transform.localPosition = posScale.m_BedSaveMenuPosition;
						rectTransform11 = rectTransform12;
					}
				}
				if (posScale != null)
				{
					SetLocalPositionAndScaleForHUDItem(m_PlayersIGMData[k].m_JobTutorialMenu.transform.parent, posScale.m_InformationMenuPosition, posScale.m_InformationMenuScale);
				}
			}
			m_PlayersIGMData[k].m_ParentObject.enabled = true;
			m_PlayersIGMData[k].m_PlayerBindingID = CameraManager.GetInstance().GetUsedBindingID(k);
			m_PlayersIGMData[k].m_PlayerCamera = CameraManager.GetInstance().GetCamera(m_PlayersIGMData[k].m_PlayerBindingID);
			m_PlayersIGMData[k].m_ParentObject.planeDistance = m_PlayersIGMData[k].m_PlayerCamera.nearClipPlane + 1f;
			m_PlayersIGMData[k].m_ParentObject.gameObject.name = "Player " + k;
			m_PlayersIGMData[k].m_PlayerRootMenu.InitializeData();
			if (m_PlayersIGMData[k].m_PlayerSelect.m_AddMorePlayersText != null)
			{
				m_PlayersIGMData[k].m_PlayerSelect.m_AddMorePlayersText.gameObject.SetActive(CanShowCoopMessages());
			}
			if ((bool)m_PlayersIGMData[k].m_ClickOffButton)
			{
				m_PlayersIGMData[k].m_ClickOffButton.gameObject.SetActive(m_PlayersIGMData[k].OpenMenuCount != 0 && m_PlayersIGMData[k].m_MousePlayer);
			}
			if (k >= m_PreviousCameraCount || k >= count)
			{
				m_PlayersIGMData[k].m_PlayerRootMenu.Hide();
			}
			m_PreviousCameraCount = usedCameraCount;
		}
	}

	private void ResizeRect(RectTransform panel, Vector2 rectSize, Vector3 localPosition)
	{
		if (panel != null)
		{
			RectTransform rectTransform = panel;
			RectTransform parent = (RectTransform)panel.parent;
			rectTransform.SetParent(parent, worldPositionStays: true);
			rectTransform.sizeDelta = rectSize;
			rectTransform.transform.localPosition = localPosition;
			panel = rectTransform;
		}
	}

	private void SetLocalPositionAndScaleForHUDItem(Transform targetUITransform, Vector2 targetPosition, Vector2 targetScale)
	{
		if (!(targetUITransform == null))
		{
			RectTransform rectTransform = (RectTransform)targetUITransform;
			if (rectTransform != null)
			{
				RectTransform rectTransform2 = rectTransform;
				RectTransform parent = (RectTransform)rectTransform.parent;
				rectTransform2.SetParent(parent, worldPositionStays: true);
				Vector2 vector = targetScale;
				rectTransform2.transform.localScale = new Vector3(vector.x, vector.y, 1f);
				rectTransform2.transform.localPosition = targetPosition;
				rectTransform = rectTransform2;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_PlayersIGMData != null)
		{
			if (m_bMenuInstanceCheckRequested && MenusEnabled())
			{
				m_bMenuInstanceCheckRequested = false;
				CheckMenuInstances();
			}
			for (int i = 0; i < m_PlayersIGMData.Count; i++)
			{
				if (m_PlayersIGMData[i] != null && m_PlayersIGMData[i].m_PlayerCamera != null)
				{
					m_PlayersIGMData[i].m_ParentObject.planeDistance = m_PlayersIGMData[i].m_PlayerCamera.nearClipPlane + 1f;
				}
			}
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			Gamer gamer = allGamers[num];
			if (gamer != null && gamer.IsLocal() && gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.CharacterOwned)
			{
				T17NetworkManager.TryOpeningPlayerSelect(gamer);
			}
		}
	}

	public bool AnyMenusOpen(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.AnyMenusOpen ?? false;
	}

	public bool HasMenusToOpen(InGameRootMenu.InGameMenuTypeToOpen typeToOpen, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.m_PlayerRootMenu.HasPanelsToShow(typeToOpen) ?? false;
	}

	public bool PrepareMenuSetToOpen(InGameRootMenu.InGameMenuTypeToOpen typeToOpen, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			data.m_PlayerRootMenu.SetInGameMenuTypeToOpen(typeToOpen);
			return true;
		}
		return false;
	}

	public bool OpenMenu(InGameRootMenu.InGameMenuTypeToOpen typeToOpen, Player player, CameraManager.PlayerBindingID bindingID)
	{
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.HideMenu(bindingID);
		}
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (player != null && data.m_PlayerRootMenu.Show(player.m_Gamer, null, null))
			{
				data.m_PlayerRootMenu.SetGamePlayerForMenus(player);
				player.DisableAllUITrackers();
				data.OpenMenuCount++;
				return true;
			}
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGame);
			}
			return false;
		}
		return false;
	}

	public bool HideMenu(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			data.OpenMenuCount--;
			if (player != null)
			{
				player.RestoreAllUITrackers();
			}
			bool flag = data.m_PlayerRootMenu.Hide();
			if (flag && player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			return flag;
		}
		if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
		}
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
		}
		return false;
	}

	public int RefreshInventory(ItemContainer sourceIC, ItemContainer descIC, CameraManager.PlayerBindingID bindingID)
	{
		int result = -1;
		GetCorrectIGMData(bindingID, out var _);
		return result;
	}

	public bool SetUpInventory(ItemContainer playersContainer, ItemContainer otherContainer, Player player, CameraManager.PlayerBindingID bindingID, InGameRootMenu.InGameMenuTypeToOpen typeToOpen)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null && data.PlayerInventory != null)
		{
			data.PlayerInventory.PopulateWithItemContainer(ref playersContainer, player, firstTimeInit: true);
			ItemContainer itemContainer = playersContainer;
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(data.PlayerInventory.RefreshAllSlotsWithCurrentContainer));
			ItemContainer itemContainer2 = playersContainer;
			itemContainer2.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer2.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(data.PlayerInventory.RefreshAllSlotsWithCurrentContainer));
			data.PlayerInventory.SetInventoryMoney(player.m_CharacterStats.Money);
			CharacterStats characterStats = player.m_CharacterStats;
			characterStats.OnMoneyStatChanged = (CharacterStats.CharacterStatsEvent)Delegate.Combine(characterStats.OnMoneyStatChanged, new CharacterStats.CharacterStatsEvent(data.PlayerInventory.SetInventoryMoney));
			List<GameMenuBehaviour> list = data.m_PlayerRootMenu.GetCurrentMenuSet().Cast<GameMenuBehaviour>().ToList();
			GameMenuBehaviour.GameMenuInformation gameMenuInformation = default(GameMenuBehaviour.GameMenuInformation);
			gameMenuInformation.m_MenuRepresentative = otherContainer.GetCharacterOwner();
			gameMenuInformation.m_MenuRepresentativeContainer = otherContainer;
			gameMenuInformation.m_Player = player;
			gameMenuInformation.m_PlayerItemContainer = playersContainer;
			gameMenuInformation.m_PlayerInventoryBehaviour = data.PlayerInventory.GetInventoryBehaviour();
			gameMenuInformation.m_PlayerInventoryMenu = data.PlayerInventory;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetGameMenuInformation(gameMenuInformation);
			}
			return true;
		}
		return false;
	}

	public void CleanupInventory(ItemContainer playersContainer, ItemContainer otherContainer, Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		PlayerInventoryMenu playerInventoryMenu = null;
		if (data != null)
		{
			playerInventoryMenu = data.PlayerInventory;
			if (null != playerInventoryMenu)
			{
				PlayerInventoryMenu playerInventory = data.PlayerInventory;
				if (null != playerInventory)
				{
					BaseInventoryBehaviour inventoryBehaviour = playerInventory.GetInventoryBehaviour();
					if (null != inventoryBehaviour)
					{
						inventoryBehaviour.SetItemContainerLinks(null, null, null);
						inventoryBehaviour.SetItemClickToCallback();
						inventoryBehaviour.OnItemClickedEvent = null;
					}
				}
			}
		}
		if (null != player && null != playerInventoryMenu && null != player.m_CharacterStats)
		{
			CharacterStats characterStats = player.m_CharacterStats;
			characterStats.OnMoneyStatChanged = (CharacterStats.CharacterStatsEvent)Delegate.Remove(characterStats.OnMoneyStatChanged, new CharacterStats.CharacterStatsEvent(playerInventoryMenu.SetInventoryMoney));
		}
		if (playersContainer != null && playerInventoryMenu != null)
		{
			playersContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(playersContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(playerInventoryMenu.RefreshAllSlotsWithCurrentContainer));
		}
		if (!(null != otherContainer) || data == null || !(null != data.m_PlayerRootMenu))
		{
			return;
		}
		List<BaseMenuBehaviour> currentMenuSet = data.m_PlayerRootMenu.GetCurrentMenuSet();
		if (currentMenuSet == null)
		{
			return;
		}
		IEnumerable<GameMenuBehaviour> enumerable = currentMenuSet.Cast<GameMenuBehaviour>();
		if (enumerable == null)
		{
			return;
		}
		List<GameMenuBehaviour> list = enumerable.ToList();
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].m_MenuType == BaseMenuBehaviour.InGameMenuTypes.InmateLooting)
			{
				LootingMenu lootingMenu = (LootingMenu)list[i];
				if (null != lootingMenu)
				{
					otherContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(otherContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(lootingMenu.RefreshAllSlotsWithCurrentContainer));
					otherContainer.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(otherContainer.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(lootingMenu.OnItemRemovedEvent));
				}
			}
			else if (list[i].m_MenuType == BaseMenuBehaviour.InGameMenuTypes.DeskInventory || list[i].m_MenuType == BaseMenuBehaviour.InGameMenuTypes.CutlreyInventory)
			{
				DeskMenu deskMenu = (DeskMenu)list[i];
				if (null != deskMenu)
				{
					otherContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(otherContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(deskMenu.RefreshAllSlotsWithCurrentContainer));
					otherContainer.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(otherContainer.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(deskMenu.OnItemRemovedEvent));
				}
			}
			else if (list[i].m_MenuType == BaseMenuBehaviour.InGameMenuTypes.ToiletInventory)
			{
				ToiletMenu toiletMenu = (ToiletMenu)list[i];
				if (null != toiletMenu)
				{
					otherContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(otherContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(toiletMenu.RefreshAllSlotsWithCurrentContainer));
				}
			}
		}
	}

	public void OnSelectItem(CameraManager.PlayerBindingID playerBindingID)
	{
	}

	public bool GetCorrectIGMData(CameraManager.PlayerBindingID playerBindingID, out PlayerIGMData data)
	{
		data = null;
		for (int i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i].m_PlayerBindingID == playerBindingID)
			{
				data = m_PlayersIGMData[i];
				return true;
			}
		}
		return false;
	}

	public void EnableSplitScreenEffects(bool bOn)
	{
		if (m_FSSplitScreenDivider != null)
		{
			m_FSSplitScreenDivider.gameObject.SetActive(bOn);
		}
	}

	public void OpenMap(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMainMap);
			}
			if (data.m_Map != null)
			{
				HUDMenuFlow.Instance.HideMenu(bindingID, showMinimap: false);
				data.m_Map.ShowMap(player);
				data.OpenMenuCount++;
			}
		}
	}

	public void HideMap(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_Map != null)
			{
				data.m_Map.HideMap();
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
				data.OpenMenuCount--;
			}
		}
	}

	public PlayerInventoryMenu GetInventoryMenu(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.PlayerInventory;
	}

	public MainMapMenu GetMapForPlayer(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.m_Map;
	}

	public void OpenCalendar(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_Calendar != null)
			{
				data.m_Calendar.ShowCalendar(player);
				data.OpenMenuCount++;
				player.IsBrowsingSmallMenu = true;
				HUDMenuFlow.Instance.HideMenu(bindingID);
			}
		}
	}

	public void HideCalendar(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_Calendar != null)
			{
				data.m_Calendar.Hide();
				data.OpenMenuCount--;
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenJobsBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_JobBoard != null)
			{
				data.m_JobBoard.ShowJobBoard(player, bindingID);
				data.OpenMenuCount++;
				player.IsBrowsingSmallMenu = true;
				HUDMenuFlow.Instance.HideMenu(bindingID);
			}
		}
	}

	public void HideJobsBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_JobBoard != null)
			{
				data.m_JobBoard.Hide();
				data.OpenMenuCount--;
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenGuardBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_GuardBoardMenu != null)
			{
				data.m_GuardBoardMenu.Show(player.m_Gamer, null, null);
				HUDMenuFlow.Instance.HideMenu(bindingID);
				player.IsBrowsingSmallMenu = true;
				data.OpenMenuCount++;
			}
		}
	}

	public void HideGuardBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_GuardBoardMenu != null)
			{
				data.m_GuardBoardMenu.Hide();
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
				data.OpenMenuCount--;
			}
		}
	}

	public void OpenInformationBoard(Player player, CameraManager.PlayerBindingID bindingID, string titleTag, string bodyTag)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_InformationBoardMenu != null)
			{
				data.m_InformationBoardMenu.Show(player.m_Gamer, null, null);
				data.m_InformationBoardMenu.SetBoardLocalisationTags(titleTag, bodyTag);
				data.OpenMenuCount++;
				player.IsBrowsingSmallMenu = true;
				HUDMenuFlow.Instance.HideMenu(bindingID);
			}
		}
	}

	public void HideInformationBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_InformationBoardMenu != null)
			{
				data.m_InformationBoardMenu.Hide();
				data.OpenMenuCount--;
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenPayphone(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_Payphone != null)
			{
				data.m_Payphone.ShowPayphone(player, bindingID);
				data.OpenMenuCount++;
				player.IsBrowsingSmallMenu = true;
				HUDMenuFlow.Instance.HideMenu(bindingID);
			}
		}
	}

	public void HidePayphone(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_Payphone != null)
			{
				data.m_Payphone.Hide();
				data.OpenMenuCount--;
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenJobTutorialBoard(Player player, CameraManager.PlayerBindingID bindingID, BaseJob job)
	{
		if (GetCorrectIGMData(bindingID, out var data) && !(data.m_JobTutorialMenu == null))
		{
			JobTutorialMenu jobTutorialMenu = data.m_JobTutorialMenu;
			jobTutorialMenu.SetupWithJob(job);
			ShowIngameMenu(player, bindingID, jobTutorialMenu);
			data.OpenMenuCount++;
			player.IsBrowsingSmallMenu = true;
		}
	}

	public void HideJobTutorialBoard(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null && !(data.m_JobTutorialMenu == null))
		{
			HideIngameMenu(player, bindingID, data.m_JobTutorialMenu);
			data.OpenMenuCount--;
			player.IsBrowsingSmallMenu = false;
		}
	}

	private bool ShowIngameMenu(Player player, CameraManager.PlayerBindingID bindingID, BaseIngameMenu menu)
	{
		if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
		}
		if (menu != null)
		{
			menu.Show(player);
			HUDMenuFlow.Instance.HideMenu(bindingID);
			return true;
		}
		return false;
	}

	private void HideIngameMenu(Player player, CameraManager.PlayerBindingID bindingID, BaseIngameMenu menu)
	{
		if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			GetCorrectIGMData(bindingID, out var data);
			ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
		}
		if (menu != null)
		{
			menu.Hide();
			HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
		}
	}

	public void OpenPlayerSelect(Gamer gamer, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null && data.m_PlayerSelect != null)
		{
			data.m_PlayerSelect.ShowPlayerSelect(gamer, bindingID);
			HUDMenuFlow.Instance.HideMenu(bindingID, showMinimap: false);
		}
	}

	public void HidePlayerSelect(Gamer gamer, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (gamer != null && gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_PlayerSelect != null)
			{
				data.m_PlayerSelect.Hide();
				Player player = T17NetView.Find<Player>(gamer.m_NetViewID);
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenSignPost(Player player, CameraManager.PlayerBindingID bindingID, string titleText, string bodyText, Sprite image)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_SignPostMenu != null)
			{
				data.m_SignPostMenu.SetUIElements(titleText, bodyText, image);
				data.m_SignPostMenu.Show(player.m_Gamer, null, null);
				HUDMenuFlow.Instance.HideMenu(bindingID);
				player.IsBrowsingSmallMenu = true;
				data.OpenMenuCount++;
			}
		}
	}

	public void HideSignPost(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_SignPostMenu != null)
			{
				data.m_SignPostMenu.Hide();
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
				data.OpenMenuCount--;
			}
		}
	}

	public void OpenBedSave(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
			if (data.m_BedSaveMenu != null)
			{
				data.m_BedSaveMenu.Show(player.m_Gamer, null, null);
				data.OpenMenuCount++;
				player.IsBrowsingSmallMenu = true;
				HUDMenuFlow.Instance.HideMenu(bindingID);
			}
		}
	}

	public void HideBedSave(Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			if (player != null && player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
			{
				ApplyGameplayCategoryPauseMenuSafe(player.m_Gamer.m_RewiredPlayer, data, T17EventSystem.InputCateogryStates.InGame);
			}
			if (data.m_BedSaveMenu != null)
			{
				data.m_BedSaveMenu.Hide();
				data.OpenMenuCount--;
				player.IsBrowsingSmallMenu = false;
				HUDMenuFlow.Instance.OpenPlayerHUD(player, bindingID);
			}
		}
	}

	public void OpenPauseMenu(Player player)
	{
		if (player != null && player.m_Gamer != null && AnyMenusOpen(player.m_PlayerCameraManagerBindingID))
		{
			Gamer gamer = player.m_Gamer;
			Rewired.Player rewiredPlayer = gamer.m_RewiredPlayer;
			if (T17RewiredStandaloneInputModule.UsedSharedKeyboardAction("Pause", "UI_Close", rewiredPlayer))
			{
				return;
			}
		}
		GetCorrectIGMData(player.m_PlayerCameraManagerBindingID, out var data);
		if (data != null && data.m_PauseMenu != null)
		{
			GameObject gameObject = data.m_PauseMenu.gameObject;
			if (gameObject.activeInHierarchy)
			{
				return;
			}
		}
		Debug.Log("  ******    OpenPauseMenu ");
		if (data == null || !(player != null))
		{
			return;
		}
		if (data.m_PreOverlayPlayerSnapshot == null && player.m_Gamer != null)
		{
			data.m_PreOverlayPlayerSnapshot = PlayerMapsSnapshot.CreateSnapshotForGamer(player.m_Gamer, disableAllMaps: false);
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int num = allPlayers.Count - 1; num >= 0; num--)
		{
			if (allPlayers[num] != null && allPlayers[num].m_Gamer != null && allPlayers[num].m_Gamer.IsLocal())
			{
				GetCorrectIGMData(allPlayers[num].m_PlayerCameraManagerBindingID, out var data2);
				if (data2 != null && data2.m_ParentObject != null)
				{
					data2.m_PreOverlayPlayerSnapshot = PlayerMapsSnapshot.CreateSnapshotForGamer(allPlayers[num].m_Gamer, disableAllMaps: false);
				}
			}
		}
		if (player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGamePaused);
		}
		if (data.m_PauseMenu != null)
		{
			AudioController.SetState(State_Group.Pause_Menu, Pause_Menu.Pause_Menu_In.ToString());
			data.m_PauseMenu.ShowPauseMenu(player);
			HUDMenuFlow.Instance.HideAllHUDs();
			HideAllMenus();
		}
		if (player != null)
		{
			player.SetBrowsingPauseMenu(browsing: true);
		}
		HUDMenuFlow.Instance.RemoveAllMouseHUDItems(player.m_PlayerCameraManagerBindingID);
		if (m_PauseMenuCoopText != null)
		{
			m_PauseMenuCoopText.SetActive(CanShowCoopMessages());
		}
	}

	public void HidePauseMenu(Player player, bool bPlayerIsExitting)
	{
		GetCorrectIGMData(player.m_PlayerCameraManagerBindingID, out var data);
		Debug.Log("  ******    HidePauseMenu ");
		if (bPlayerIsExitting)
		{
			HideMap(player, player.m_PlayerCameraManagerBindingID);
		}
		if (player.m_Gamer != null && player.m_Gamer.m_RewiredPlayer != null)
		{
			if (data.m_PreOverlayPlayerSnapshot != null)
			{
				data.m_PreOverlayPlayerSnapshot.RestoreControllerMaps();
			}
			if (player.m_Gamer.m_RewiredPlayer.GetButton("UI_Submit"))
			{
				player.SetBlockInteraction();
			}
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int num = allPlayers.Count - 1; num >= 0; num--)
		{
			if (allPlayers[num] != null && allPlayers[num].m_Gamer != null && allPlayers[num].m_Gamer.m_RewiredPlayer != null && allPlayers[num].m_Gamer.IsLocal() && allPlayers[num] != player)
			{
				PlayerIGMData data2 = null;
				Instance.GetCorrectIGMData(allPlayers[num].m_PlayerCameraManagerBindingID, out data2);
				if (data2 != null)
				{
					data2.m_PreOverlayPlayerSnapshot.RestoreControllerMaps();
					data2.m_PreOverlayPlayerSnapshot = null;
				}
			}
		}
		if (data != null && data.m_PauseMenu != null)
		{
			AudioController.SetState(State_Group.Pause_Menu, Pause_Menu.Pause_Menu_out.ToString());
			data.m_PauseMenu.Hide();
			HUDMenuFlow.Instance.ShowAllHUDs();
			ShowAllMenus();
		}
		if (player != null)
		{
			player.SetBrowsingPauseMenu(browsing: false);
		}
		if (data.m_PreOverlayPlayerSnapshot != null)
		{
			data.m_PreOverlayPlayerSnapshot.RestoreSelectedGameobject();
		}
		data.m_PreOverlayPlayerSnapshot = null;
		if (player.GetCloseInventoryOnPauseMenuHide())
		{
			player.CloseInventory();
		}
	}

	public void HideAllMenus()
	{
		for (int i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i] != null && m_PlayersIGMData[i].m_PlayerBindingID != 0)
			{
				GetCorrectIGMData(m_PlayersIGMData[i].m_PlayerBindingID, out var data);
				if (data != null && data.m_ParentObject != null)
				{
					data.m_ParentObject.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public void ShowAllMenus()
	{
		for (int i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i] != null && m_PlayersIGMData[i].m_PlayerBindingID != 0)
			{
				GetCorrectIGMData(m_PlayersIGMData[i].m_PlayerBindingID, out var data);
				if (data != null && data.m_ParentObject != null)
				{
					data.m_ParentObject.gameObject.SetActive(value: true);
				}
			}
		}
	}

	public void DestroyPlayerIGM(CameraManager.PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i] != null && m_PlayersIGMData[i].m_PlayerBindingID == bindingID)
			{
				PlayerIGMData playerIGMData = m_PlayersIGMData[i];
				UnityEngine.Object.Destroy(playerIGMData.m_ParentObject.gameObject);
				m_PlayersIGMData[i] = null;
				m_PlayersIGMData.RemoveAt(i);
				break;
			}
		}
	}

	public void StartTutorial(List<ItemData> items, IGMTutorialArrowController.IGMTutorial type)
	{
		if (m_TutorialController != null && m_TutorialController.enabled)
		{
			m_TutorialController.StartTutorial(items, type);
		}
	}

	public void StopTutorial(IGMTutorialArrowController.IGMTutorial type)
	{
		if (m_TutorialController != null && m_TutorialController.enabled)
		{
			m_TutorialController.RemoveActiveTutorial(type);
		}
	}

	public void OnGamerCreated(Gamer gamer)
	{
		SelectPlayerForGamer(gamer);
	}

	public void OnLoadComplete(int iPhotonId)
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num = allGamers.Length - 1; num >= 0; num--)
		{
			if (allGamers[num] != null && allGamers[num].m_PhotonID == iPhotonId)
			{
				SelectPlayerForGamer(allGamers[num]);
			}
		}
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (allPlayers[i] != null && allPlayers[i].m_Gamer == null)
			{
				allPlayers[i].GamerNotSetOnLoad();
			}
		}
	}

	public void SelectPlayerForGamer(Gamer gamer)
	{
		if (gamer == null || !gamer.IsLocal() || gamer.m_eCharacterSelectionStage != 0)
		{
			return;
		}
		if (!m_GamerCameraIndex.ContainsKey(gamer))
		{
			CameraManager.PlayerBindingID value = CameraManager.GetInstance().AssignABindingToGetUnUsedBindingID();
			m_GamerCameraIndex.Add(gamer, value);
		}
		bool flag = gamer.m_NetViewID != -1;
		if (gamer.m_bPrimaryLocal && T17NetManager.IsMasterClient && PrisonSnapshotIO.IsThereSaveData())
		{
			int primaryPlayerID = PlayerDataManager.GetInstance().GetPrimaryPlayerID();
			if (primaryPlayerID >= 0)
			{
				T17NetworkManager.GetInstance().RequestPlayerOwnershipRPC(primaryPlayerID, gamer);
				flag = true;
			}
		}
		if (!flag)
		{
			CameraManager.PlayerBindingID playerBindingID = CameraManager.PlayerBindingID.CM_PBID_UNSET;
			playerBindingID = m_GamerCameraIndex[gamer];
			CameraManager.GetInstance().SetTarget(CameraManager.GetInstance().m_PlayerSelectCameraPosition, playerBindingID);
			if (gamer.m_bPrimaryLocal)
			{
				string outValue = string.Empty;
				T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.HostKey, ref outValue);
				MatchingGames.Game game = MatchingGames.GetInstance().FindGame(outValue);
				if (game != null)
				{
					T17NetworkManager.GetInstance().RequestPlayerOwnershipRPC(game.m_SlotNetViewID, gamer);
				}
				else
				{
					T17NetworkManager.GetInstance().RequestPlayerOwnershipRPC();
				}
			}
			else
			{
				T17NetworkManager.GetInstance().RequestPlayerOwnershipRPC();
			}
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		int i;
		for (i = 0; i < m_PlayersIGMData.Count; i++)
		{
			if (m_PlayersIGMData[i] != null)
			{
				Player player = allPlayers.Find((Player x) => x.m_PlayerCameraManagerBindingID == m_PlayersIGMData[i].m_PlayerBindingID);
				if (player != null && player.m_Gamer != null && player.m_Gamer.IsLocal())
				{
					m_PlayersIGMData[i].m_MousePlayer = player.m_Gamer.m_RewiredPlayer != null && player.m_Gamer.m_RewiredPlayer.controllers.hasMouse;
				}
				else
				{
					m_PlayersIGMData[i].m_MousePlayer = false;
				}
			}
		}
	}

	public void SkipPlayerSelect(Gamer gamer)
	{
		if (gamer != null && gamer.m_PlayerObject != null && Instance.GetCameraIndexForGamer(gamer) != 0)
		{
			gamer.m_PlayerObject.SetCameraTargetToPlayer();
			gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.InGame;
			gamer.m_PlayerObject.SetIsDisabled(bDisabled: false);
			gamer.m_PlayerObject.EnableInLevel();
			HUDMenuFlow.Instance.HideMenu(gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
			HUDMenuFlow.Instance.OpenPlayerHUD(gamer.m_PlayerObject, gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
			T17EventSystem.ApplyCategories(gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGame);
		}
	}

	public void SetupPlayerForTutorial(Gamer gamer)
	{
		if (gamer == null || !(gamer.m_PlayerObject != null))
		{
			return;
		}
		Customisation details = null;
		if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null)
		{
			CustomisationConfig defaultPlayerCustomisations = LevelScript.GetInstance().m_LevelSetup.m_DefaultPlayerCustomisations;
			if (defaultPlayerCustomisations != null)
			{
				details = new Customisation();
				if (!CustomisationManager.RandomiseFromPool(ref details, defaultPlayerCustomisations))
				{
					details = CustomisationManager.GetInstance().DefaultCustomisation;
				}
			}
		}
		if (details == null)
		{
			details = CustomisationManager.GetInstance().DefaultCustomisation;
		}
		if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null)
		{
			CustomisationConstraint defaultPlayerConstraint = LevelScript.GetInstance().m_LevelSetup.m_DefaultPlayerConstraint;
			if (defaultPlayerConstraint != null)
			{
				CustomisationManager.ApplyConstraint(ref details, defaultPlayerConstraint);
			}
		}
		PlayerDataManager.GetInstance().RequestSetPlayerAppearanceRPC(gamer.m_PlayerObject, details);
		if (!gamer.m_bPrimaryLocal || !T17NetManager.IsMasterClient)
		{
			gamer.m_PlayerObject.m_CharacterStats.ResetToBaseline();
			OpinionManager instance = OpinionManager.GetInstance();
			if (instance != null)
			{
				instance.ResetOpinionsOfPlayerRPC(gamer.m_PlayerObject);
			}
		}
		CutsceneManagerBase instance2 = CutsceneManagerBase.GetInstance();
		if (instance2 != null)
		{
			Player playerObject = gamer.m_PlayerObject;
			instance2.ConsiderPlayingIntroCutscene(null, UIAnimatedEffectController.Effects.FadeToOpaque, null, playerObject);
		}
	}

	public void OnPlatformPauseRequest()
	{
		if (GlobalStart.GetInstance().GetMode() != GlobalStart.GLOBALSTART_MODE.IN_LEVEL)
		{
			return;
		}
		Player primaryPlayer = GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if (CutsceneManagerBase.IsACutscenePlaying())
		{
			if (T17NetManager.OfflineMode)
			{
				Platform instance = Platform.GetInstance();
				instance.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnpauseRequest));
				Platform instance2 = Platform.GetInstance();
				instance2.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Combine(instance2.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnpauseRequest));
				Time.timeScale = 0f;
			}
		}
		else
		{
			OpenPauseMenu(primaryPlayer);
		}
	}

	public void OnPlatformUnpauseRequest()
	{
		if (CutsceneManagerBase.IsACutscenePlaying() && T17NetManager.OfflineMode)
		{
			Platform instance = Platform.GetInstance();
			instance.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnpauseRequest));
			Time.timeScale = 1f;
		}
	}

	private Player GetPrimaryPlayer()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (allPlayers[i].m_Gamer != null && allPlayers[i].m_Gamer.m_bPrimaryLocal && allPlayers[i].m_Gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.EnabledInGame && !Platform.GetInstance().ReInput_IsAnyControllerDisconnected() && !T17DialogBoxManager.HasAnyOpenDialogs())
			{
				return allPlayers[i];
			}
		}
		return null;
	}

	private void OnNetworkConnectionChange(bool isConnected)
	{
		if (!isConnected)
		{
			T17NetManager instance = T17NetManager.Instance;
			if (instance != null && NetConnectAndJoinRoom.GetRequestedConnectionState() != 0 && !T17NetInvites.HasInvite())
			{
				ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected);
			}
		}
	}

	private void ApplyGameplayCategoryPauseMenuSafe(Rewired.Player rewiredPlayer, PlayerIGMData igmData, T17EventSystem.InputCateogryStates inputCategory)
	{
		if (igmData != null && igmData.m_PreOverlayPlayerSnapshot != null)
		{
			igmData.m_PreOverlayPlayerSnapshot.OverrideCapturedSelected(inputCategory);
		}
		else
		{
			T17EventSystem.ApplyCategories(rewiredPlayer, inputCategory);
		}
	}

	public void CloseAllMenusOnAllPlayers()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null)
		{
			return;
		}
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player != null)
			{
				HideMenu(player, player.m_PlayerCameraManagerBindingID);
			}
		}
	}

	private bool CanShowCoopMessages()
	{
		ConfigManager instance = ConfigManager.GetInstance();
		return (instance.gameType == PrisonConfig.ConfigType.Cooperative || (instance.gameType == PrisonConfig.ConfigType.Versus && !T17NetManager.IsConnectedOnline() && NetConnectAndJoinRoom.GetRequestedConnectionState() == NetConnectionState.OfflineMode)) && Gamer.GetGamerCount() < 4;
	}
}

using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class HUDMenuFlow : BaseFlowBehaviour
{
	[Serializable]
	public class PlayerHUDData
	{
		public CameraManager.PlayerBindingID m_PlayerBindingID;

		public Canvas m_ParentObject;

		public Canvas m_WorldSpace_ParentObject;

		public UIAnimatedEffectController m_FadeEffects;

		public HUDRootMenu m_PlayerRootMenu;

		public GameObject m_PlayerPersistantHud;

		public GameObject m_PlayerSafeSpace;

		public List<GameObject> m_MouseHUDObjects = new List<GameObject>();

		[HideInInspector]
		public bool m_bIsSplitHorizontally;

		public RoutineAndTimeTrackerHUD m_MiniMapParent;

		public FloorIndicatorHUD m_FloodIndicator;

		public Camera m_PlayerCamera;

		public T17RawImage m_PIPImage;

		public GameObject m_ArrowParent;

		private TutorialPopup m_ActivePopupInstance;

		private float m_PopupTimer;

		private PlayerInventoryHUD m_PlayerInventory;

		private PerPlayerTrackedUIElements m_PerPlayerTrackedItems;

		public ObjectiveTrackerHUD m_PlayerObjectiveTrackedHUD;

		private PlayerSolitaryHUD m_PlayerSolitaryHUD;

		public TutorialSpeechHUD m_TutorialSpeechHUD;

		public Vector2 m_CanvasReferenceResolution = default(Vector2);

		public ChatFeedHUD m_ChatFeedHUD;

		public RectTransform m_CentreCanvasRect;

		public RectTransform m_TutorialParent;

		public PerPlayerTrackedUIElements PerPlayerTrackedItems
		{
			get
			{
				if (m_PerPlayerTrackedItems == null && m_WorldSpace_ParentObject != null)
				{
					m_PerPlayerTrackedItems = m_WorldSpace_ParentObject.GetComponentInChildren<PerPlayerTrackedUIElements>(includeInactive: true);
				}
				return m_PerPlayerTrackedItems;
			}
		}

		public PlayerInventoryHUD PlayerInventory
		{
			get
			{
				if (m_PlayerInventory == null && m_PlayerRootMenu != null)
				{
					m_PlayerInventory = m_PlayerRootMenu.GetComponentInChildren<PlayerInventoryHUD>(includeInactive: true);
				}
				return m_PlayerInventory;
			}
		}

		public PlayerSolitaryHUD PlayerSolitaryHUD
		{
			get
			{
				if (m_PlayerSolitaryHUD == null && m_PlayerRootMenu != null)
				{
					m_PlayerSolitaryHUD = (PlayerSolitaryHUD)m_PlayerRootMenu.GetMenuOFType<PlayerSolitaryHUD>(RootMenu.RootMenuType.HUD);
				}
				return m_PlayerSolitaryHUD;
			}
		}

		public TutorialPopup ActivePopupInstance
		{
			get
			{
				return m_ActivePopupInstance;
			}
			set
			{
				m_ActivePopupInstance = value;
			}
		}

		public float PopupTimer
		{
			get
			{
				return m_PopupTimer;
			}
			set
			{
				m_PopupTimer = value;
			}
		}
	}

	public delegate void SplitscreenScaleChangedHandler(bool isNowScaled);

	private static HUDMenuFlow m_Instance;

	public List<PlayerHUDData> m_PlayersHUDData = new List<PlayerHUDData>();

	public GameObject m_WorldSpaceCanvasUIElementsPrefab;

	public SplitscreenHUDHandler m_SplitscreenHUDHandler;

	public UIAnimatedEffectController m_GlobalEfectController;

	private List<WorldCanvasTrackedUIElements> m_WorldSpaceCanvases = new List<WorldCanvasTrackedUIElements>();

	private bool m_bMenuInstanceCheckRequested;

	public HUDTutorialArrowController m_TutorialController;

	private Vector3[] m_CachedFourCornersArray = new Vector3[4];

	private bool m_bIsUsingSplitScreenScale;

	private GameObject m_WorldCanvasContainer;

	private Vector3 m_PlayerWorldDefaultScale;

	private Vector3 m_PlayerWorldHudScale = Vector3.one;

	public static HUDMenuFlow Instance => m_Instance;

	public static float WorldOffsetZ
	{
		get
		{
			if (FloorManager.GetInstance() == null)
			{
				return -4f;
			}
			return (float)FloorManager.GetInstance().m_FloorOffset * 0.9997f;
		}
	}

	public event SplitscreenScaleChangedHandler SplitscreenScaleChangedEvent;

	protected override void Awake()
	{
		if (m_Instance != null)
		{
			UnityEngine.Object.Destroy(m_Instance);
		}
		m_Instance = this;
		base.Awake();
	}

	protected override void Start()
	{
		Debug.LogError(" ****  HUDMenuFlow   Start  " + Time.realtimeSinceStartup);
		base.Start();
		if (CameraManager.GetInstance() != null)
		{
			CameraManager instance = CameraManager.GetInstance();
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(RequestMenuInstanceCheck));
		}
		if (!LevelScript.GetInstance().m_PreBuildHUDElements)
		{
			if (FloorManager.GetInstance() != null)
			{
				CreateWorldCanvas(base.transform);
			}
		}
		else
		{
			m_WorldCanvasContainer = new GameObject();
			m_WorldCanvasContainer.name = "WorldCanvas";
			m_WorldSpaceCanvases = LevelScript.GetInstance().m_WorldSpaceCanvases;
			int count = m_WorldSpaceCanvases.Count;
			for (int i = 0; i < count; i++)
			{
				m_WorldSpaceCanvases[i].PreGeneratePoolFixUp();
				m_WorldSpaceCanvases[i].transform.SetParent(m_WorldCanvasContainer.transform, worldPositionStays: true);
			}
		}
		if (m_PlayersHUDData[0] != null && CameraManager.GetInstance() != null)
		{
			m_PlayersHUDData[0].m_PlayerBindingID = CameraManager.GetInstance().GetUsedBindingID(0);
			if (m_PlayersHUDData[0].m_PlayerRootMenu != null)
			{
				Transform transform = m_PlayersHUDData[0].m_PlayerRootMenu.transform.Find("ReferenceIGMRect");
				if (transform != null)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			m_PlayerWorldDefaultScale = m_PlayersHUDData[0].m_WorldSpace_ParentObject.gameObject.transform.localScale;
		}
		if ((LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial) && m_TutorialController != null)
		{
			m_TutorialController.enabled = false;
		}
		Debug.LogError(" ****  HUDMenuFlow   Start  Bottom " + Time.realtimeSinceStartup);
	}

	protected override void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		this.SplitscreenScaleChangedEvent = null;
		if (CameraManager.GetInstance() != null)
		{
			CameraManager instance = CameraManager.GetInstance();
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(RequestMenuInstanceCheck));
		}
		CleanupEventCleanersUnderObject(base.gameObject);
		if (m_WorldCanvasContainer != null)
		{
			UnityEngine.Object.Destroy(m_WorldCanvasContainer);
		}
		m_WorldCanvasContainer = null;
		m_WorldSpaceCanvasUIElementsPrefab = null;
		m_SplitscreenHUDHandler = null;
		m_GlobalEfectController = null;
		m_TutorialController = null;
		m_PlayersHUDData.Clear();
		int count = m_WorldSpaceCanvases.Count;
		for (int i = 0; i < count; i++)
		{
			m_WorldSpaceCanvases[i] = null;
		}
		m_WorldSpaceCanvases = null;
		base.OnDestroy();
	}

	public void CreateWorldCanvas(Transform transform)
	{
		Debug.LogError(" ****  HUDMenuFlow   Floor Loop   " + Time.realtimeSinceStartup);
		List<FloorManager.Floor> validFloors = FloorManager.GetInstance().GetValidFloors();
		int layer = LayerMask.NameToLayer("Global_Inside_TrackedTags");
		m_WorldCanvasContainer = new GameObject();
		m_WorldCanvasContainer.name = "WorldCanvas";
		for (int i = 0; i < validFloors.Count; i++)
		{
			FloorManager.Floor floor = validFloors[i];
			if (m_WorldSpaceCanvasUIElementsPrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_WorldSpaceCanvasUIElementsPrefab);
				gameObject.transform.SetParent(m_WorldCanvasContainer.transform);
				gameObject.name = "WorldSpace_Canvas_Floor_" + floor.m_FloorType.ToString() + "_Index_" + i;
				gameObject.layer = layer;
				RectTransform rectTransform = gameObject.transform as RectTransform;
				rectTransform.localPosition = new Vector3(0f, 0f, (float)floor.m_zPos + WorldOffsetZ);
				FloorManager.GetInstance().GetTileSystemBounds(floor, FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
				rectTransform.sizeDelta = new Vector2(maxRows * 20, maxColumns * 20);
				rectTransform.localScale = Vector3.one / 20f;
				WorldCanvasTrackedUIElements componentInChildren = gameObject.GetComponentInChildren<WorldCanvasTrackedUIElements>(includeInactive: true);
				int num = 150;
				int num2 = 30;
				componentInChildren.m_PoolSize = ((floor.m_FloorType != FloorManager.FLOOR_TYPE.Floor_Prison) ? num2 : num);
				componentInChildren.GeneratePool();
				m_WorldSpaceCanvases.Add(componentInChildren);
			}
		}
		Debug.LogError(" ****  HUDMenuFlow   Floor Loop   " + Time.realtimeSinceStartup);
	}

	private void RequestMenuInstanceCheck()
	{
		if (HUDEnabled())
		{
			CheckMenuInstances();
		}
		else
		{
			m_bMenuInstanceCheckRequested = true;
		}
	}

	private bool HUDEnabled()
	{
		if (m_PlayersHUDData == null || m_PlayersHUDData[0] == null || m_PlayersHUDData[0].m_ParentObject == null)
		{
			return false;
		}
		return m_PlayersHUDData[0].m_ParentObject.isActiveAndEnabled;
	}

	private void CheckMenuInstances()
	{
		CameraManager instance = CameraManager.GetInstance();
		if (instance == null)
		{
			return;
		}
		int usedCameraCount = instance.GetUsedCameraCount();
		if (m_PlayersHUDData == null || m_PlayersHUDData[0] == null || m_PlayersHUDData[0].m_ParentObject == null || m_PlayersHUDData[0].m_ParentObject.gameObject == null)
		{
			return;
		}
		Gamer[] localGamers = Gamer.GetLocalGamers();
		int num = localGamers.Length;
		Rect[] cameraViewportRects = instance.GetCameraViewportRects(usedCameraCount);
		bool bIsUsingSplitScreenScale = m_bIsUsingSplitScreenScale;
		HUDItemLayoutGroup hUDItemLayoutGroup = m_SplitscreenHUDHandler.UpdateActiveLayoutGroup();
		Vector3 localScale = Vector3.one * hUDItemLayoutGroup.m_MasterWorldHUDScale;
		localScale.z = 1f;
		if (m_WorldCanvasContainer != null)
		{
			m_WorldCanvasContainer.transform.localScale = localScale;
		}
		m_PlayerWorldHudScale = m_PlayerWorldDefaultScale * hUDItemLayoutGroup.m_MasterWorldHUDScale;
		switch (usedCameraCount)
		{
		case 1:
		case 2:
			m_bIsUsingSplitScreenScale = false;
			break;
		case 3:
			m_PlayerWorldHudScale *= CameraManager.GetCorrectHudScaleForPlatform(hUDItemLayoutGroup.m_ThreePlayerConsoleScale, hUDItemLayoutGroup.m_ThreePlayerPcScale);
			m_bIsUsingSplitScreenScale = true;
			break;
		case 4:
			m_PlayerWorldHudScale *= CameraManager.GetCorrectHudScaleForPlatform(hUDItemLayoutGroup.m_FourPlayerConsoleScale, hUDItemLayoutGroup.m_FourPlayerPcScale);
			m_bIsUsingSplitScreenScale = true;
			break;
		}
		m_PlayerWorldHudScale.z = m_PlayerWorldDefaultScale.z;
		if (m_PlayersHUDData.Count < usedCameraCount && m_PlayersHUDData.Count > 0)
		{
			int num2 = usedCameraCount - m_PlayersHUDData.Count;
			for (int i = 0; i < num2; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_PlayersHUDData[0].m_ParentObject.gameObject);
				gameObject.transform.SetParent(m_PlayersHUDData[0].m_ParentObject.transform.parent);
				HUDRootMenu componentInChildren = gameObject.GetComponentInChildren<HUDRootMenu>(includeInactive: true);
				PlayerHUDData playerHUDData = new PlayerHUDData();
				GameObject gameObject2 = gameObject.transform.GetChild(0).GetChild(0).gameObject;
				T17RawImage[] componentsInChildren = gameObject.GetComponentsInChildren<T17RawImage>(includeInactive: true);
				T17RawImage pIPImage = null;
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j].name == "PIP Image")
					{
						pIPImage = componentsInChildren[j];
						break;
					}
				}
				T17ItemTooltip[] componentsInChildren2 = gameObject.GetComponentsInChildren<T17ItemTooltip>(includeInactive: true);
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[k].gameObject);
				}
				RoutineAndTimeTrackerHUD componentInChildren2 = gameObject.GetComponentInChildren<RoutineAndTimeTrackerHUD>(includeInactive: true);
				FloorIndicatorHUD componentInChildren3 = gameObject.GetComponentInChildren<FloorIndicatorHUD>(includeInactive: true);
				componentInChildren2.PostInit();
				GameObject arrowParent = gameObject2.transform.FindChild("ArrowParent").gameObject;
				ChatFeedHUD componentInChildren4 = gameObject.GetComponentInChildren<ChatFeedHUD>(includeInactive: true);
				RectTransform component = gameObject2.transform.FindChild("TutorialParent").GetComponent<RectTransform>();
				playerHUDData.m_ParentObject = gameObject.GetComponent<Canvas>();
				playerHUDData.m_PlayerRootMenu = componentInChildren;
				playerHUDData.m_PlayerPersistantHud = gameObject2.transform.FindChild("PersistantHUD").gameObject;
				playerHUDData.m_PlayerSafeSpace = gameObject2;
				playerHUDData.m_PIPImage = pIPImage;
				playerHUDData.m_MiniMapParent = componentInChildren2;
				playerHUDData.m_ArrowParent = arrowParent;
				playerHUDData.m_FloodIndicator = componentInChildren3;
				playerHUDData.m_ChatFeedHUD = componentInChildren4;
				playerHUDData.m_TutorialParent = component;
				DestroyUnwantedObjectsInClonedHUD(playerHUDData);
				CanvasScaler component2 = playerHUDData.m_ParentObject.GetComponent<CanvasScaler>();
				if (component2 != null)
				{
					playerHUDData.m_CanvasReferenceResolution = component2.referenceResolution;
				}
				GameObject gameObject3 = UnityEngine.Object.Instantiate(m_PlayersHUDData[0].m_WorldSpace_ParentObject.gameObject);
				gameObject3.transform.SetParent(m_PlayersHUDData[0].m_WorldSpace_ParentObject.transform.parent);
				playerHUDData.m_WorldSpace_ParentObject = gameObject3.GetComponent<Canvas>();
				GameObject gameObject4 = UnityEngine.Object.Instantiate(m_PlayersHUDData[0].m_FadeEffects.gameObject);
				gameObject4.transform.SetParent(m_PlayersHUDData[0].m_FadeEffects.transform.parent);
				playerHUDData.m_FadeEffects = gameObject4.GetComponent<UIAnimatedEffectController>();
				playerHUDData.m_PlayerRootMenu.Hide();
				m_PlayersHUDData.Add(playerHUDData);
				playerHUDData.m_ParentObject.GetComponent<CanvasAlphaChanger>().Copy(m_PlayersHUDData[0].m_ParentObject.GetComponent<CanvasAlphaChanger>());
				playerHUDData.m_WorldSpace_ParentObject.GetComponent<CanvasAlphaChanger>().Copy(m_PlayersHUDData[0].m_WorldSpace_ParentObject.GetComponent<CanvasAlphaChanger>());
				playerHUDData.m_WorldSpace_ParentObject.transform.localScale = m_PlayersHUDData[0].m_WorldSpace_ParentObject.transform.localScale;
				foreach (Transform item in playerHUDData.PerPlayerTrackedItems.transform)
				{
					if (!item.name.Contains("HealthBars") && !item.name.Contains("Ovens"))
					{
						item.gameObject.SetActive(value: false);
					}
				}
			}
		}
		for (int l = 0; l < m_PlayersHUDData.Count; l++)
		{
			if (m_PlayersHUDData[l] == null || Player.GetAllPlayers() == null || l >= Player.GetAllPlayers().Count)
			{
				continue;
			}
			if (!m_PlayersHUDData[l].m_ParentObject.isActiveAndEnabled)
			{
				m_PlayersHUDData[l].m_ParentObject.gameObject.SetActive(value: true);
			}
			m_PlayersHUDData[l].m_PlayerBindingID = CameraManager.GetInstance().GetUsedBindingID(l);
			m_PlayersHUDData[l].m_PlayerCamera = CameraManager.GetInstance().GetCamera(m_PlayersHUDData[l].m_PlayerBindingID);
			m_PlayersHUDData[l].m_ParentObject.planeDistance = m_PlayersHUDData[l].m_PlayerCamera.nearClipPlane + 1f;
			m_PlayersHUDData[l].m_ParentObject.gameObject.name = "Player " + l;
			m_PlayersHUDData[l].m_WorldSpace_ParentObject.gameObject.name = "Player " + l + "_WorldHUD";
			m_PlayersHUDData[l].m_WorldSpace_ParentObject.gameObject.transform.localScale = m_PlayerWorldHudScale;
			m_PlayersHUDData[l].m_FadeEffects.gameObject.name = "Player " + l + "_FadeEffectCanvas";
			m_PlayersHUDData[l].m_PlayerRootMenu.InitializeData();
			m_PlayersHUDData[l].PerPlayerTrackedItems.SetTrackingCamera(m_PlayersHUDData[l].m_PlayerCamera, m_PlayersHUDData[l].m_PlayerBindingID);
			m_PlayersHUDData[l].PlayerInventory.UpdateExpandParentVisibility();
			if (l < cameraViewportRects.Length)
			{
				m_PlayersHUDData[l].m_bIsSplitHorizontally = Mathf.Approximately(cameraViewportRects[l].height, 0.5f);
			}
			else
			{
				m_PlayersHUDData[l].m_bIsSplitHorizontally = false;
			}
			m_PlayersHUDData[l].PerPlayerTrackedItems.SetLayer(LayerMask.NameToLayer("Player" + l + "_TrackedTags"));
			Player player = null;
			for (int m = 0; m < num; m++)
			{
				if (!(null == player))
				{
					break;
				}
				Gamer gamer = localGamers[m];
				if (gamer != null && null != gamer.m_PlayerObject && m_PlayersHUDData[l].m_PlayerBindingID == gamer.m_PlayerObject.m_PlayerCameraManagerBindingID)
				{
					player = gamer.m_PlayerObject;
				}
			}
			m_PlayersHUDData[l].m_FloodIndicator.SetPlayer(player, l);
			if (m_PlayersHUDData[l].PerPlayerTrackedItems.m_OvenHudContainer == null)
			{
				m_PlayersHUDData[l].PerPlayerTrackedItems.m_OvenHudContainer = m_PlayersHUDData[l].PerPlayerTrackedItems.GetComponentInChildren<OvensHudContainer>(includeInactive: true);
			}
			m_PlayersHUDData[l].m_PlayerObjectiveTrackedHUD = m_PlayersHUDData[l].m_ParentObject.GetComponentInChildren<ObjectiveTrackerHUD>(includeInactive: true);
			BaseIngamePassiveUI[] componentsInChildren3 = m_PlayersHUDData[l].m_ParentObject.GetComponentsInChildren<BaseIngamePassiveUI>(includeInactive: true);
			for (int n = 0; n < componentsInChildren3.Length; n++)
			{
				componentsInChildren3[n].Init(player);
			}
			RectTransform rectTransform = (RectTransform)m_PlayersHUDData[l].m_PlayerRootMenu.transform.Find("HUD_BottomLeft");
			HUDItemsLayout posScale = m_SplitscreenHUDHandler.GetPosScale(usedCameraCount, l);
			if (rectTransform != null)
			{
				RectTransform rectTransform2 = rectTransform;
				RectTransform parent = (RectTransform)rectTransform.parent;
				rectTransform2.SetParent(parent, worldPositionStays: true);
				if (posScale != null)
				{
					Vector2 statsScale = posScale.m_StatsScale;
					rectTransform2.transform.localScale = new Vector3(statsScale.x, statsScale.y, 1f);
					rectTransform2.transform.localPosition = posScale.m_StatsPosition;
					rectTransform = rectTransform2;
				}
			}
			ObjectiveTrackerHUD componentInChildren5 = m_PlayersHUDData[l].m_PlayerRootMenu.GetComponentInChildren<ObjectiveTrackerHUD>();
			if (componentInChildren5 != null)
			{
				RectTransform rectTransform3 = (RectTransform)m_PlayersHUDData[l].m_PlayerRootMenu.GetComponentInChildren<ObjectiveTrackerHUD>().transform;
				if (rectTransform3 != null)
				{
					RectTransform rectTransform4 = rectTransform3;
					RectTransform parent2 = (RectTransform)rectTransform3.parent;
					rectTransform4.SetParent(parent2, worldPositionStays: true);
					if (posScale != null)
					{
						rectTransform4.transform.localScale = posScale.m_QuestScale;
						rectTransform4.transform.localPosition = posScale.m_QuestPosition;
						rectTransform3 = rectTransform4;
					}
				}
			}
			RectTransform rectTransform5 = (RectTransform)m_PlayersHUDData[l].m_PlayerRootMenu.transform.Find("HUD_Emotes");
			if (rectTransform5 != null)
			{
				RectTransform rectTransform6 = rectTransform5;
				RectTransform parent3 = (RectTransform)rectTransform5.parent;
				rectTransform6.SetParent(parent3, worldPositionStays: true);
				if (posScale != null)
				{
					rectTransform6.transform.localScale = posScale.m_EmoteScale;
					rectTransform6.transform.localPosition = posScale.m_EmotePosition;
					rectTransform5 = rectTransform6;
				}
			}
			RectTransform rectTransform7 = (RectTransform)m_PlayersHUDData[l].m_PlayerRootMenu.transform.Find("HUD_BottomRight");
			if (rectTransform7 != null)
			{
				RectTransform rectTransform8 = rectTransform7;
				RectTransform parent4 = (RectTransform)rectTransform7.parent;
				rectTransform8.SetParent(parent4, worldPositionStays: true);
				if (posScale != null)
				{
					rectTransform8.transform.localScale = posScale.m_BottomRightScale;
					rectTransform8.transform.localPosition = posScale.m_BottomRightPosition;
					rectTransform7 = rectTransform8;
				}
			}
			RectTransform rectTransform9 = (RectTransform)m_PlayersHUDData[l].m_PlayerPersistantHud.transform.Find("HUD_VoiceChatFeed");
			if (rectTransform9 != null)
			{
				RectTransform rectTransform10 = rectTransform9;
				RectTransform parent5 = (RectTransform)rectTransform9.parent;
				rectTransform10.SetParent(parent5, worldPositionStays: true);
				if (posScale != null)
				{
					rectTransform10.transform.localScale = posScale.m_VoiceChatFeedScale;
					rectTransform10.transform.localPosition = posScale.m_VoiceChatFeedPosition;
					rectTransform9 = rectTransform10;
				}
			}
			RectTransform rectTransform11 = (RectTransform)m_PlayersHUDData[l].m_PlayerRootMenu.transform.parent.Find("Player_01_MiniMap");
			if (rectTransform11 != null)
			{
				RectTransform rectTransform12 = rectTransform11;
				RectTransform parent6 = (RectTransform)rectTransform11.parent;
				rectTransform12.SetParent(parent6, worldPositionStays: true);
				if (posScale != null)
				{
					rectTransform12.transform.localScale = posScale.m_MiniMapScale;
					rectTransform12.transform.localPosition = posScale.m_MiniMapPosition;
					rectTransform11 = rectTransform12;
				}
			}
			if (posScale != null)
			{
				UIAnimatedEffect[] componentsInChildren4 = m_PlayersHUDData[l].m_FadeEffects.GetComponentsInChildren<UIAnimatedEffect>(includeInactive: true);
				for (int num3 = 0; num3 < componentsInChildren4.Length; num3++)
				{
					RectTransform rectTransform13 = componentsInChildren4[num3].transform as RectTransform;
					rectTransform13.transform.localScale = posScale.m_FadingCanvasScale;
					rectTransform13.transform.localPosition = posScale.m_FadingCanvasPosition;
				}
			}
			if (null != m_PlayersHUDData[l].m_ChatFeedHUD)
			{
				RectTransform rectTransform14 = (RectTransform)m_PlayersHUDData[l].m_ChatFeedHUD.transform;
				if (rectTransform14 != null)
				{
					RectTransform rectTransform15 = rectTransform14;
					RectTransform parent7 = (RectTransform)rectTransform14.parent;
					rectTransform15.SetParent(parent7, worldPositionStays: true);
					if (posScale != null)
					{
						rectTransform15.transform.localScale = posScale.m_ChatFeedScale;
						rectTransform15.transform.localPosition = posScale.m_ChatFeedPosition;
						rectTransform14 = rectTransform15;
					}
					if (!m_PlayersHUDData[l].m_ChatFeedHUD.IsRequiredForPlayer(l))
					{
						m_PlayersHUDData[l].m_ChatFeedHUD.Hide(restoreInvokerState: false);
					}
				}
			}
			if (null != m_PlayersHUDData[l].m_CentreCanvasRect)
			{
				RectTransform rectTransform16 = (RectTransform)m_PlayersHUDData[l].m_CentreCanvasRect.transform;
				if (rectTransform16 != null)
				{
					RectTransform rectTransform17 = rectTransform16;
					RectTransform parent8 = (RectTransform)rectTransform16.parent;
					rectTransform17.SetParent(parent8, worldPositionStays: true);
					if (posScale != null)
					{
						rectTransform17.transform.localScale = posScale.m_CentreCanvasScale;
						rectTransform17.transform.localPosition = posScale.m_CentreCanvasPosition;
						rectTransform16 = rectTransform17;
					}
				}
			}
			RectTransform tutorialParent = m_PlayersHUDData[l].m_TutorialParent;
			if (tutorialParent != null)
			{
				RectTransform rectTransform18 = tutorialParent;
				RectTransform parent9 = (RectTransform)tutorialParent.parent;
				rectTransform18.SetParent(parent9, worldPositionStays: true);
				if (posScale != null)
				{
					rectTransform18.transform.localPosition = posScale.m_MaskContainerPosition;
					rectTransform18.sizeDelta = posScale.m_MaskContainerSize;
					tutorialParent = rectTransform18;
				}
			}
			TutorialPopup[] componentsInChildren5 = m_PlayersHUDData[l].m_TutorialParent.GetComponentsInChildren<TutorialPopup>();
			for (int num4 = 0; num4 < componentsInChildren5.Length; num4++)
			{
				ResetTutorialRect(l, componentsInChildren5[num4].gameObject);
			}
			if (m_PlayersHUDData[l].PerPlayerTrackedItems != null)
			{
				m_PlayersHUDData[l].PerPlayerTrackedItems.SetScalesOfPerTrackedUI(m_PlayerWorldHudScale, m_PlayerWorldDefaultScale);
			}
			if (player != null && m_PlayersHUDData[l].m_MiniMapParent != null)
			{
				T17Button component3 = m_PlayersHUDData[l].m_MiniMapParent.GetComponent<T17Button>();
				if (component3 != null)
				{
					component3.SetGamerForEventSystem(player.m_Gamer, null);
				}
			}
			ObjectiveManager instance2 = ObjectiveManager.GetInstance();
			if (instance2 != null)
			{
				instance2.ShowCurrentTrackingObjective(player);
			}
		}
		if (bIsUsingSplitScreenScale != m_bIsUsingSplitScreenScale && this.SplitscreenScaleChangedEvent != null)
		{
			this.SplitscreenScaleChangedEvent(m_bIsUsingSplitScreenScale);
		}
		if (ArrowManager.GetInstance() != null)
		{
			ArrowManager.GetInstance().CheckArrowInstances();
		}
	}

	private void DestroyUnwantedObjectsInClonedHUD(PlayerHUDData cloneHUD)
	{
		List<MonoBehaviour> list = new List<MonoBehaviour>();
		TutorialPopup[] componentsInChildren = cloneHUD.m_TutorialParent.GetComponentsInChildren<TutorialPopup>();
		list.AddRange(componentsInChildren);
		GuideArrow[] componentsInChildren2 = cloneHUD.m_ParentObject.GetComponentsInChildren<GuideArrow>(includeInactive: true);
		list.AddRange(componentsInChildren2);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(list[num].gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_PlayersHUDData == null)
		{
			return;
		}
		if (m_bMenuInstanceCheckRequested && HUDEnabled())
		{
			m_bMenuInstanceCheckRequested = false;
			CheckMenuInstances();
		}
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			PlayerHUDData data = m_PlayersHUDData[i];
			if (data != null && data.m_PlayerCamera != null)
			{
				data.m_ParentObject.planeDistance = data.m_PlayerCamera.nearClipPlane + 1f;
			}
			if (data.ActivePopupInstance != null)
			{
				data.PopupTimer -= UpdateManager.deltaTime;
				if (data.PopupTimer < 0f)
				{
					HidePopupDialogue(i);
				}
			}
			Rewired.Player player = null;
			Player player2 = Player.GetAllPlayers().Find((Player x) => x != null && x.m_PlayerCameraManagerBindingID == data.m_PlayerBindingID);
			if (player2 != null && player2.m_Gamer != null)
			{
				player = player2.m_Gamer.m_RewiredPlayer;
			}
			if (player != null && !InGameMenuFlow.Instance.AnyMenusOpen(data.m_PlayerBindingID))
			{
				if (data.m_MouseHUDObjects.Count > 0 && T17EventSystem.GetStateForRewiredPlayer(player) == T17EventSystem.InputCateogryStates.InGame)
				{
					T17EventSystem.ApplyCategories(player, T17EventSystem.InputCateogryStates.InGameMouseOnHUD);
				}
				if (data.m_MouseHUDObjects.Count == 0 && T17EventSystem.GetStateForRewiredPlayer(player) == T17EventSystem.InputCateogryStates.InGameMouseOnHUD)
				{
					T17EventSystem.ApplyCategories(player, T17EventSystem.InputCateogryStates.InGame);
				}
			}
		}
	}

	public OvensHudContainer GetOvensHudContainer(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.PerPlayerTrackedItems.m_OvenHudContainer;
	}

	public PerPlayerTrackedUIElements GetPlayerTrackedUIElements(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.PerPlayerTrackedItems;
	}

	public PlayerInventoryHUD GetPlayerInventoryHUD(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.PlayerInventory;
	}

	public ObjectiveTrackerHUD GetPlayerObjectiveHUD(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.m_PlayerObjectiveTrackedHUD;
	}

	public TutorialSpeechHUD GetPlayerTutorialSpeechHUD(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.m_TutorialSpeechHUD;
	}

	public UIAnimatedEffectController GetPlayerEffectsController(CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		return data?.m_FadeEffects;
	}

	public void GetHudContainingObjects(CameraManager.PlayerBindingID bindingID, out Canvas hudParentObject, out Canvas hudWorldSpaceParent)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data == null)
		{
			hudParentObject = null;
			hudWorldSpaceParent = null;
		}
		else
		{
			hudParentObject = data.m_ParentObject;
			hudWorldSpaceParent = data.m_WorldSpace_ParentObject;
		}
	}

	public WorldCanvasTrackedUIElements GetUIElementsWorldCanvas(int floorIndex)
	{
		FloorManager.Floor floor = FloorManager.GetInstance().m_PrisonFloors[floorIndex];
		if (m_WorldSpaceCanvases.Count > 0 && m_WorldSpaceCanvases.Count > floorIndex)
		{
			if (m_WorldSpaceCanvases[floorIndex] == null)
			{
			}
			return m_WorldSpaceCanvases[floorIndex];
		}
		return null;
	}

	public void ShiftWorldElementsForAnyFacade()
	{
		bool isSplitScreen = CameraManager.GetInstance().m_ActiveCameraCount > 1;
		for (int num = m_WorldSpaceCanvases.Count - 1; num >= 0; num--)
		{
			WorldCanvasTrackedUIElements worldCanvasTrackedUIElements = m_WorldSpaceCanvases[num];
			if (worldCanvasTrackedUIElements != null)
			{
				List<T17TrackedUIElement> usedAlwaysVisibleElements = worldCanvasTrackedUIElements.GetUsedAlwaysVisibleElements();
				usedAlwaysVisibleElements.Sort((T17TrackedUIElement x, T17TrackedUIElement y) => x.m_SpeechBubble.GetLastShiftTestTimestamp().CompareTo(y.m_SpeechBubble.GetLastShiftTestTimestamp()));
				int num2 = Math.Min(usedAlwaysVisibleElements.Count, 2);
				int num3 = 0;
				int count = usedAlwaysVisibleElements.Count;
				for (int i = 0; i < count; i++)
				{
					bool flag = false;
					if (usedAlwaysVisibleElements[i].AttachedTo != null && usedAlwaysVisibleElements[i].AttachedTo.CharacterOwner != null)
					{
						RoomBlob currentLocation = usedAlwaysVisibleElements[i].AttachedTo.CharacterOwner.m_CurrentLocation;
						if (currentLocation == null || currentLocation.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors || usedAlwaysVisibleElements[i].AttachedTo.CharacterOwner.CurrentFloor.IsUnderGround())
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						usedAlwaysVisibleElements[i].m_SpeechBubble.ResetFacadeOffset();
						continue;
					}
					SpeechBubbleHUD speechBubble = usedAlwaysVisibleElements[i].m_SpeechBubble;
					if (!speechBubble.isActiveAndEnabled)
					{
						continue;
					}
					speechBubble.ResetFacadeOffset();
					if (DoShiftTestForSpeechBubble(speechBubble, isSplitScreen))
					{
						speechBubble.RecordShiftTestDone();
						num3++;
						if (num3 >= num2)
						{
							break;
						}
					}
				}
			}
		}
	}

	private bool DoShiftTestForSpeechBubble(SpeechBubbleHUD bubbleToTest, bool isSplitScreen)
	{
		CameraManager instance = CameraManager.GetInstance();
		RoomManager instance2 = RoomManager.GetInstance();
		FloorManager instance3 = FloorManager.GetInstance();
		bubbleToTest.m_Background.GetComponent<RectTransform>().GetWorldCorners(m_CachedFourCornersArray);
		float x = m_CachedFourCornersArray[2].x - m_CachedFourCornersArray[1].x;
		FloorManager.Floor floor = instance3.FindFloorAtZ(bubbleToTest.transform.position.z - WorldOffsetZ);
		CameraManager.CameraBinding cameraBinding = null;
		bool flag = false;
		bool flag2 = false;
		bool result = false;
		for (int i = 0; i < instance.m_CameraBindings.Length; i++)
		{
			cameraBinding = instance.m_CameraBindings[i];
			if (cameraBinding.m_Camera != null && (cameraBinding.m_Character != null || cameraBinding.m_TargetPosition != Vector3.zero))
			{
				Vector2 vector = cameraBinding.m_Camera.WorldToViewportPoint(bubbleToTest.transform.position);
				if (vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
				{
					continue;
				}
				result = true;
				Vector3 vector2 = m_CachedFourCornersArray[1];
				Vector3 vector3 = m_CachedFourCornersArray[2];
				Vector3 vector4 = vector2 + new Vector3(x, 0f, 0f);
				Vector3 vector5 = vector3 - new Vector3(x, 0f, 0f);
				Vector3 cameraCullTarget = instance.GetCameraCullTarget(cameraBinding.m_Camera);
				vector2.z = cameraCullTarget.z;
				vector3.z = cameraCullTarget.z;
				FloorManager.Floor floor2 = instance3.FindFloorAtZ(cameraCullTarget.z);
				if (floor2.m_FloorIndex > floor.m_FloorIndex)
				{
					Vector3 vector6 = default(Vector3);
					Vector3 vector7;
					Vector3 worldPosition2;
					Vector3 worldPosition;
					Vector3 vector8 = (vector7 = (worldPosition2 = (worldPosition = vector6)));
					for (int num = floor2.m_FloorIndex; num > floor.m_FloorIndex; num--)
					{
						FloorManager.Floor floor3 = instance3.FindFloorbyIndex(num);
						if (!floor3.IsVent())
						{
							vector8.Set(vector2.x, vector2.y, floor3.m_zPos);
							vector7.Set(vector3.x, vector3.y, floor3.m_zPos);
							worldPosition2.Set(vector4.x, vector4.y, floor3.m_zPos);
							worldPosition.Set(vector5.x, vector5.y, floor3.m_zPos);
							vector6.Set((vector2.x + vector3.x) / 2f, vector2.y, floor3.m_zPos);
							RoomBlob roomBlob = instance2.LookUpRoom(vector8, floor3);
							RoomBlob roomBlob2 = instance2.LookUpRoom(vector7, floor3);
							bool flag3 = roomBlob != null && roomBlob.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors;
							bool flag4 = roomBlob2 != null && roomBlob2.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors;
							bool flag5 = instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Wall, vector8);
							bool flag6 = instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Wall, vector7);
							bool flag7 = instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Ground, vector8);
							bool flag8 = instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Ground, vector7);
							if ((flag5 || flag7) && (flag6 || (!flag6 && !flag4)))
							{
								flag = true;
								if (isSplitScreen)
								{
									break;
								}
							}
							if ((flag6 || flag8) && (flag5 || (!flag5 && !flag3)))
							{
								flag2 = true;
								if (isSplitScreen)
								{
									break;
								}
							}
							if ((!flag || !flag2) && !flag3 && !flag4 && (instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Wall, worldPosition2) || instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Wall, worldPosition) || instance3.CheckTileExists(floor3, FloorManager.TileSystem_Type.TileSystem_Wall, vector6)))
							{
								flag = true;
								flag2 = true;
							}
							if (flag && flag2)
							{
								break;
							}
						}
					}
				}
			}
			if ((flag && flag2) || (isSplitScreen && (flag || flag2)))
			{
				break;
			}
		}
		bubbleToTest.SetFacadeOffset(flag, flag2, isSplitScreen);
		return result;
	}

	public void DisableAllButWorldCanvasAtIndex(int floorIndex)
	{
		for (int num = m_WorldSpaceCanvases.Count - 1; num >= 0; num--)
		{
			bool flag = num == floorIndex;
			if (m_WorldSpaceCanvases[num] != null)
			{
				m_WorldSpaceCanvases[num].m_ToggledWorldSpaceCanvas.enabled = flag;
				m_WorldSpaceCanvases[num].m_VisibleWorldSpaceCanvas.enabled = flag;
			}
		}
	}

	public bool OpenPlayerHUD(Player player, CameraManager.PlayerBindingID bindingID)
	{
		bool flag = false;
		if (LevelScript.GetInstance() != null && LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Tutorial)
		{
			return OpenMenu(HUDRootMenu.HUDMenuTypeToOpen.PlayerInfoTutorial, player, bindingID);
		}
		return OpenMenu(HUDRootMenu.HUDMenuTypeToOpen.PlayerInfo, player, bindingID);
	}

	public bool OpenMenu(HUDRootMenu.HUDMenuTypeToOpen typeToOpen, Player player, CameraManager.PlayerBindingID bindingID)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null && data.m_PlayerRootMenu != null && player != null)
		{
			data.m_PlayerRootMenu.SetHUDMenuTypeToOpen(typeToOpen);
			if (data.m_PlayerRootMenu.Show(player.m_Gamer, null, null))
			{
				data.m_PlayerRootMenu.SetGamePlayerForMenus(player);
				data.m_MiniMapParent.SetGamePlayer(player);
				data.m_MiniMapParent.transform.parent.gameObject.SetActive(value: true);
				data.m_ArrowParent.gameObject.SetActive(value: true);
				return true;
			}
			return false;
		}
		return false;
	}

	public bool HideMenu(CameraManager.PlayerBindingID bindingID, bool showMinimap = true)
	{
		GetCorrectIGMData(bindingID, out var data);
		if (data != null)
		{
			RemoveAllMouseHUDItems(bindingID);
			data.m_MiniMapParent.transform.parent.gameObject.SetActive(showMinimap);
			data.m_ArrowParent.gameObject.SetActive(value: false);
			return data.m_PlayerRootMenu.Hide();
		}
		return false;
	}

	public void HideAllHUDs()
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID != 0 && m_PlayersHUDData[i].m_ParentObject != null)
			{
				for (int j = 0; j < m_PlayersHUDData[i].m_ParentObject.transform.childCount; j++)
				{
					Transform child = m_PlayersHUDData[i].m_ParentObject.transform.GetChild(j);
					child.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public void ShowAllHUDs()
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID != 0 && m_PlayersHUDData[i].m_ParentObject != null)
			{
				for (int j = 0; j < m_PlayersHUDData[i].m_ParentObject.transform.childCount; j++)
				{
					Transform child = m_PlayersHUDData[i].m_ParentObject.transform.GetChild(j);
					child.gameObject.SetActive(value: true);
				}
			}
		}
	}

	private void GetCorrectIGMData(CameraManager.PlayerBindingID bindingID, out PlayerHUDData data)
	{
		data = null;
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID == bindingID)
			{
				data = m_PlayersHUDData[i];
			}
		}
	}

	public Rect GetHUDCanvasRect(CameraManager.PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID == bindingID)
			{
				return m_PlayersHUDData[i].m_ParentObject.pixelRect;
			}
		}
		return default(Rect);
	}

	public Vector2 GetHUDCanvasReferenceResolution(CameraManager.PlayerBindingID bindingID)
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID == bindingID)
			{
				return m_PlayersHUDData[i].m_CanvasReferenceResolution;
			}
		}
		return new Vector2(-1f, -1f);
	}

	public bool ShowPopupDialogue(GameObject popupPrefab, CameraManager.PlayerBindingID bindingID, float popupDuration)
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i] != null && m_PlayersHUDData[i].m_PlayerBindingID == bindingID)
			{
				if (m_PlayersHUDData[i].PopupTimer > 0f)
				{
					return false;
				}
				if (UpdateManager.AquireHeavyCpuLock())
				{
					GameObject popupObj = UnityEngine.Object.Instantiate(popupPrefab);
					return SetTutorialRect(i, popupObj, popupDuration);
				}
			}
		}
		return false;
	}

	public void HidePopupDialogue(int playerIndex)
	{
		if (m_PlayersHUDData[playerIndex].ActivePopupInstance != null)
		{
			m_PlayersHUDData[playerIndex].ActivePopupInstance = null;
			TutorialPopup[] componentsInChildren = m_PlayersHUDData[playerIndex].m_TutorialParent.GetComponentsInChildren<TutorialPopup>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(componentsInChildren[num].gameObject);
			}
		}
	}

	public void PlayGlobalEffect(UIAnimatedEffectController.Effects effect, float time)
	{
		m_GlobalEfectController.PlayEffect(effect, time);
	}

	public void DestoryPlayerHUD(CameraManager.PlayerBindingID playerBindingID)
	{
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData != null && m_PlayersHUDData[i].m_PlayerBindingID == playerBindingID)
			{
				PlayerHUDData playerHUDData = m_PlayersHUDData[i];
				CleanupEventCleanersUnderObject(playerHUDData.m_ParentObject.gameObject);
				UnityEngine.Object.Destroy(playerHUDData.m_ParentObject.gameObject);
				UnityEngine.Object.Destroy(playerHUDData.m_WorldSpace_ParentObject.gameObject);
				UnityEngine.Object.Destroy(playerHUDData.m_FadeEffects.gameObject);
				m_PlayersHUDData[i] = null;
				m_PlayersHUDData.RemoveAt(i);
				break;
			}
		}
	}

	private void CleanupEventCleanersUnderObject(GameObject targetGameObject)
	{
		if (!(targetGameObject != null))
		{
			return;
		}
		IEventCleaner[] componentsInChildren = targetGameObject.GetComponentsInChildren<IEventCleaner>(includeInactive: true);
		if (componentsInChildren != null)
		{
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				IEventCleaner eventCleaner = componentsInChildren[num];
				eventCleaner.CleanUpEvents();
			}
		}
	}

	public bool GetCorrectHUDData(CameraManager.PlayerBindingID playerBindingID, out PlayerHUDData data)
	{
		data = null;
		for (int i = 0; i < m_PlayersHUDData.Count; i++)
		{
			if (m_PlayersHUDData[i].m_PlayerBindingID == playerBindingID)
			{
				data = m_PlayersHUDData[i];
				return true;
			}
		}
		return false;
	}

	private bool SetTutorialRect(int playerIndex, GameObject popupObj, float popupDuration)
	{
		RectTransform rectTransform = (RectTransform)popupObj.transform;
		if (rectTransform != null)
		{
			PositionTutorialRect(playerIndex, rectTransform);
			m_PlayersHUDData[playerIndex].ActivePopupInstance = popupObj.GetComponent<TutorialPopup>();
			m_PlayersHUDData[playerIndex].PopupTimer = popupDuration;
			T17Text[] componentsInChildren = m_PlayersHUDData[playerIndex].ActivePopupInstance.GetComponentsInChildren<T17Text>(includeInactive: true);
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				List<Player> allPlayers = Player.GetAllPlayers();
				Player player = allPlayers.Find((Player x) => x.m_PlayerCameraManagerBindingID == m_PlayersHUDData[playerIndex].m_PlayerBindingID);
				if ((bool)player && player.m_Gamer != null)
				{
					componentsInChildren[num].SetGamerForEventSystem(player.m_Gamer, T17EventSystemsManager.Instance.GetEventSystemForGamer(player.m_Gamer));
				}
			}
			AnimationPlayer component = m_PlayersHUDData[playerIndex].m_TutorialParent.GetComponent<AnimationPlayer>();
			if (component != null)
			{
				component.Play();
			}
			return true;
		}
		return false;
	}

	private bool ResetTutorialRect(int playerIndex, GameObject popupObj)
	{
		RectTransform rectTransform = (RectTransform)popupObj.transform;
		if (rectTransform != null)
		{
			PositionTutorialRect(playerIndex, rectTransform);
			return true;
		}
		return false;
	}

	private void PositionTutorialRect(int playerIndex, RectTransform popupRect)
	{
		CameraManager instance = CameraManager.GetInstance();
		if (!(instance != null) || !(popupRect != null) || m_PlayersHUDData == null || playerIndex < 0 || playerIndex >= m_PlayersHUDData.Count)
		{
			return;
		}
		int usedCameraCount = instance.GetUsedCameraCount();
		if (m_PlayersHUDData[playerIndex].m_TutorialParent == null)
		{
			return;
		}
		RectTransform tutorialParent = m_PlayersHUDData[playerIndex].m_TutorialParent;
		if (tutorialParent == null)
		{
			return;
		}
		popupRect.SetParent(tutorialParent.parent, worldPositionStays: true);
		if (m_SplitscreenHUDHandler != null)
		{
			HUDItemsLayout posScale = m_SplitscreenHUDHandler.GetPosScale(usedCameraCount, playerIndex);
			if (posScale != null)
			{
				popupRect.transform.localScale = posScale.m_TutorialPopupScale;
				popupRect.transform.localPosition = posScale.m_TutorialPopupPosition;
			}
		}
		popupRect.SetParent(tutorialParent, worldPositionStays: true);
	}

	public void StartTutorial(List<ItemData> items, HUDTutorialArrowController.HUDTutorial type)
	{
		if (m_TutorialController != null && m_TutorialController.enabled)
		{
			m_TutorialController.StartTutorial(items, type);
		}
	}

	public void StopTutorial(HUDTutorialArrowController.HUDTutorial type)
	{
		if (m_TutorialController != null && m_TutorialController.enabled)
		{
			m_TutorialController.RemoveActiveTutorial(type);
		}
	}

	public List<WorldCanvasTrackedUIElements> GetWorldSpaveCanvases()
	{
		return m_WorldSpaceCanvases;
	}

	public bool HasHorizontallySplitscreen(CameraManager.PlayerBindingID bindingId)
	{
		if (m_PlayersHUDData != null)
		{
			for (int num = m_PlayersHUDData.Count - 1; num >= 0; num--)
			{
				if (m_PlayersHUDData[num] != null && m_PlayersHUDData[num].m_PlayerBindingID == bindingId)
				{
					return m_PlayersHUDData[num].m_bIsSplitHorizontally;
				}
			}
		}
		return false;
	}

	public bool IsUsingSplitscreenScale()
	{
		return m_bIsUsingSplitScreenScale;
	}

	public Vector3 GetPlayerWorldHudScale()
	{
		return m_PlayerWorldHudScale;
	}

	public Vector3 GetDefaultPlayerWorldHudScale()
	{
		return m_PlayerWorldDefaultScale;
	}

	public void AddMouseHUDItem(CameraManager.PlayerBindingID bindingID, GameObject HUDObject)
	{
		for (int num = m_PlayersHUDData.Count - 1; num >= 0; num--)
		{
			if (m_PlayersHUDData[num] != null && m_PlayersHUDData[num].m_PlayerBindingID == bindingID)
			{
				PlayerHUDData playerHUDData = m_PlayersHUDData[num];
				if (!playerHUDData.m_MouseHUDObjects.Contains(HUDObject))
				{
					playerHUDData.m_MouseHUDObjects.Add(HUDObject);
				}
				break;
			}
		}
	}

	public void RemoveMouseHUDItem(CameraManager.PlayerBindingID bindingID, GameObject HUDObject)
	{
		for (int num = m_PlayersHUDData.Count - 1; num >= 0; num--)
		{
			if (m_PlayersHUDData[num] != null && m_PlayersHUDData[num].m_PlayerBindingID == bindingID)
			{
				PlayerHUDData playerHUDData = m_PlayersHUDData[num];
				playerHUDData.m_MouseHUDObjects.Remove(HUDObject);
				break;
			}
		}
	}

	public void RemoveAllMouseHUDItems(CameraManager.PlayerBindingID bindingID)
	{
		for (int num = m_PlayersHUDData.Count - 1; num >= 0; num--)
		{
			if (m_PlayersHUDData[num] != null && m_PlayersHUDData[num].m_PlayerBindingID == bindingID)
			{
				m_PlayersHUDData[num].m_MouseHUDObjects.Clear();
				break;
			}
		}
	}
}

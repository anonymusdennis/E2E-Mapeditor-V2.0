using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerPlayerTrackedUIElements : T17MonoBehaviour
{
	public GameObject m_TrackedItemPrefab;

	public Transform m_TrackedItemsParent;

	public Canvas m_TrackedItemsCanvas;

	public Vector2 m_TrackedElementOffsetFromObject = new Vector2(0f, 0.8f);

	public TargetHUD m_TargetHUD;

	public TargetHUD m_CombatTargetHUD;

	public T17Image m_PressAndHoldImage;

	public T17Image m_SmashAttackChargeImage;

	public AlternateButtonMasher m_ButtonMasher;

	public GymMasher_WeightLifting m_WeightLiftingButtonMasher;

	public GymMasher_Pullup m_PullUpMasher;

	public GymMasher_KettleBelts m_KettlebellsMasher;

	public GymMasher_Threadmill_ExerciseBike m_ThreadmillAndExerciseBikeMasher;

	public GymMasher_Pommel_Footbag m_PommelAndFootbadMasher;

	public ReadingMasher m_ReadingMasher;

	public SolitaryPotatoMasher m_SolitaryPotatoMasher;

	public IconDisplayHUD m_ClimbIcon;

	public T17Text m_ClimbInputUp;

	public T17Text m_ClimbInputDown;

	[Header("Health bars")]
	public T17StatsSlider m_CombatTargetHealthSlider;

	public T17StatsSlider m_PlayerHealthSlider;

	public T17StatsSlider m_IncidentalHealthSliderPrefab;

	public int m_MaxIncidentalSliders = 6;

	[Header("Jobs and Other Minigames")]
	public ReadingMasher m_ElectricianMasher;

	public SolitaryPotatoMasher m_PaintingMasher;

	public GymMasher_Threadmill_ExerciseBike m_PlumberMasher;

	public OvensHudContainer m_OvenHudContainer;

	public SolitaryPotatoMasher m_PumpkinCarvingMasher;

	public SolitaryPotatoMasher m_TreeDecorationMasher;

	public ReadingMasher m_FacePaintingMasher;

	public SolitaryPotatoMasher m_LionTamingMasher;

	public SolitaryPotatoMasher m_HangingPosterMasher;

	public ReadingMasher m_HorseshoeAnvilMasher;

	public SolitaryPotatoMasher m_MinstrelMasher;

	public SolitaryPotatoMasher m_StonemasonCarvingMasher;

	public ReadingMasher m_RobotServicingMasher;

	public SolitaryPotatoMasher m_ST_JugglingPotatoMasher;

	public SolitaryPotatoMasher m_ST_HulaHoopPotatoMasher;

	public SolitaryPotatoMasher m_ST_FireBreathingPotatoMasher;

	public SolitaryPotatoMasher m_ST_UnicyclePotatoMasher;

	private const int m_PoolSize = 8;

	private Dictionary<Character, T17StatsSlider> m_ActiveIncidentalHealthSliders;

	private T17StatsSlider[] m_IncidentalHealthSliderPool;

	private T17TrackedUIElement m_MyTrackedItem;

	private T17TrackedUIElement[] m_TrackedItemPool;

	private List<T17TrackedUIElement> m_UsedItemPool = new List<T17TrackedUIElement>();

	private List<T17TrackedUIElement> m_GhostItemPool = new List<T17TrackedUIElement>();

	private Camera m_TrackedCamera;

	private RectTransform m_CanvasRect;

	private int m_OurLayer = -1;

	private CameraManager.PlayerBindingID m_BindingID;

	private bool m_bIsDisabled;

	private bool m_bNeedsFacadeWeighting;

	private static int m_CanvasScalar = 20;

	public Material m_WorldHUDMaterial;

	protected override void Awake()
	{
		m_TrackedItemPool = GetComponentsInChildren<T17TrackedUIElement>(includeInactive: true);
		if (m_TrackedItemPool == null || m_TrackedItemPool.Length == 0)
		{
			m_TrackedItemPool = new T17TrackedUIElement[8];
			for (int i = 0; i < 8; i++)
			{
				GameObject gameObject = Object.Instantiate(m_TrackedItemPrefab);
				gameObject.transform.SetParent(m_TrackedItemsParent, worldPositionStays: true);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.name = "PERPLAYER_TrIt_" + i;
				gameObject.SetActive(value: false);
				T17TrackedUIElement componentInChildren = gameObject.GetComponentInChildren<T17TrackedUIElement>(includeInactive: true);
				m_TrackedItemPool[i] = componentInChildren;
			}
			m_MyTrackedItem = m_TrackedItemPool[0];
			m_MyTrackedItem.name = "MY_TRACKED_UI_ITEM";
		}
		else
		{
			m_UsedItemPool.Clear();
			for (int j = 0; j < 8; j++)
			{
				m_TrackedItemPool[j].ResetAll();
				m_TrackedItemPool[j].gameObject.SetActive(value: false);
			}
			m_MyTrackedItem = m_TrackedItemPool[0];
		}
		if (m_TrackedItemsCanvas != null)
		{
			m_CanvasRect = m_TrackedItemsCanvas.GetComponent<RectTransform>();
			m_CanvasRect.localPosition = new Vector3(0f, 0f, -12f);
			if (FloorManager.GetInstance() != null)
			{
				FloorManager.GetInstance().GetTileSystemBounds(FloorManager.GetInstance().FindFloorAtZ(-12f), FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
				m_CanvasRect.sizeDelta = new Vector2(maxColumns * m_CanvasScalar, maxRows * m_CanvasScalar);
				m_CanvasRect.localScale = new Vector3(1f / (float)m_CanvasScalar, 1f / (float)m_CanvasScalar, 1f / (float)m_CanvasScalar);
			}
		}
		CameraManager.CameraViewChangedEvent += CameraManager_CameraViewChangedEvent;
		if (m_IncidentalHealthSliderPrefab != null)
		{
			m_ActiveIncidentalHealthSliders = new Dictionary<Character, T17StatsSlider>(Character.CharacterTComparer);
			m_IncidentalHealthSliderPool = new T17StatsSlider[m_MaxIncidentalSliders];
			m_IncidentalHealthSliderPool[0] = m_IncidentalHealthSliderPrefab;
			Transform parent = m_IncidentalHealthSliderPrefab.transform.parent;
			for (int k = 1; k < m_MaxIncidentalSliders; k++)
			{
				T17StatsSlider t17StatsSlider = Object.Instantiate(m_IncidentalHealthSliderPrefab);
				t17StatsSlider.name = m_IncidentalHealthSliderPrefab.name + string.Empty + k;
				t17StatsSlider.transform.SetParent(parent, worldPositionStays: false);
				t17StatsSlider.transform.localPosition = Vector3.zero;
				t17StatsSlider.gameObject.SetActive(value: false);
				m_IncidentalHealthSliderPool[k] = t17StatsSlider;
			}
		}
		if (m_WorldHUDMaterial != null)
		{
			T17Image[] componentsInChildren = GetComponentsInChildren<T17Image>(includeInactive: true);
			for (int l = 0; l < componentsInChildren.Length; l++)
			{
				componentsInChildren[l].material = m_WorldHUDMaterial;
			}
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>(includeInactive: true);
			for (int m = 0; m < componentsInChildren2.Length; m++)
			{
				componentsInChildren2[m].material = m_WorldHUDMaterial;
			}
		}
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		CameraManager.CameraViewChangedEvent -= CameraManager_CameraViewChangedEvent;
		for (int i = 0; i < 8; i++)
		{
			m_TrackedItemPool[i] = null;
		}
		m_TrackedItemPool = null;
		m_ActiveIncidentalHealthSliders.Clear();
		for (int j = 1; j < m_MaxIncidentalSliders; j++)
		{
			Object.Destroy(m_IncidentalHealthSliderPool[j]);
			m_IncidentalHealthSliderPool[j] = null;
		}
		m_IncidentalHealthSliderPool = null;
		m_WorldHUDMaterial = null;
		m_TrackedItemPrefab = null;
		m_TrackedItemsParent = null;
		m_TrackedItemsCanvas = null;
		m_TargetHUD = null;
		m_CombatTargetHUD = null;
		m_PressAndHoldImage = null;
		m_SmashAttackChargeImage = null;
		m_ButtonMasher = null;
		m_WeightLiftingButtonMasher = null;
		m_PullUpMasher = null;
		m_KettlebellsMasher = null;
		m_ThreadmillAndExerciseBikeMasher = null;
		m_PommelAndFootbadMasher = null;
		m_ReadingMasher = null;
		m_SolitaryPotatoMasher = null;
		m_ClimbIcon = null;
		m_ClimbInputUp = null;
		m_ClimbInputDown = null;
		m_CombatTargetHealthSlider = null;
		m_PlayerHealthSlider = null;
		m_IncidentalHealthSliderPrefab = null;
		m_ElectricianMasher = null;
		m_PaintingMasher = null;
		m_PlumberMasher = null;
		m_OvenHudContainer = null;
		m_PumpkinCarvingMasher = null;
		m_HangingPosterMasher = null;
		m_LionTamingMasher = null;
		m_FacePaintingMasher = null;
		m_HorseshoeAnvilMasher = null;
		m_MinstrelMasher = null;
		m_StonemasonCarvingMasher = null;
		m_RobotServicingMasher = null;
		m_ST_UnicyclePotatoMasher = null;
		m_ST_JugglingPotatoMasher = null;
		m_ST_HulaHoopPotatoMasher = null;
		m_ST_FireBreathingPotatoMasher = null;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_bIsDisabled && m_UsedItemPool.Count > 0)
		{
			for (int num = m_UsedItemPool.Count - 1; num >= 0; num--)
			{
				if (m_UsedItemPool[num].AttachedTo == null && m_UsedItemPool[num].GhostedTo == null)
				{
					ReleaseTrackedUIElement(m_UsedItemPool[num]);
				}
				else if (m_UsedItemPool[num].Flags != 0)
				{
					UpdatePosition(m_UsedItemPool[num]);
				}
			}
		}
		if (m_bIsDisabled || m_GhostItemPool.Count <= 0)
		{
			return;
		}
		for (int num2 = m_GhostItemPool.Count - 1; num2 >= 0; num2--)
		{
			if (m_GhostItemPool[num2].AttachedTo == null && m_GhostItemPool[num2].GhostedTo == null)
			{
				ReleaseGhostTrackedUIElement(m_GhostItemPool[num2]);
			}
			else if (m_GhostItemPool[num2].Flags != 0)
			{
				UpdatePosition(m_GhostItemPool[num2]);
			}
		}
	}

	private void CameraManager_CameraViewChangedEvent(CameraManager.CameraBinding binding)
	{
		if (!(binding.m_Camera == m_TrackedCamera))
		{
			return;
		}
		float num = CameraManager.GetInstance().GetZOffsetForFacade();
		if (binding.m_CameraView == CameraView.Facade)
		{
			if (!m_bNeedsFacadeWeighting)
			{
				SetElementDepth(m_TrackedItemsCanvas.transform.position.z + num);
				m_bNeedsFacadeWeighting = true;
			}
		}
		else if (m_bNeedsFacadeWeighting)
		{
			SetElementDepth(m_TrackedItemsCanvas.transform.position.z - num);
			m_bNeedsFacadeWeighting = false;
		}
	}

	public void SetBaseElementDepth(float depth)
	{
		if (m_bNeedsFacadeWeighting)
		{
			depth += (float)CameraManager.GetInstance().GetZOffsetForFacade();
		}
		SetElementDepth(depth);
	}

	private void SetElementDepth(float depth)
	{
		Vector3 position = m_TrackedCamera.transform.position;
		position.z = depth;
		m_TrackedItemsCanvas.transform.position = position;
	}

	private void UpdatePosition(T17TrackedUIElement element)
	{
		Vector3 attachedPosition = element.AttachedPosition;
		attachedPosition.z = element.transform.position.z;
		attachedPosition.y += m_TrackedElementOffsetFromObject.y;
		attachedPosition.x += m_TrackedElementOffsetFromObject.x;
		element.transform.position = attachedPosition;
	}

	public void DisableTrackers()
	{
		m_bIsDisabled = true;
		if (m_TrackedCamera != null)
		{
			TurnOffLayerInCullingMask(m_TrackedCamera, m_OurLayer);
		}
	}

	public void EnableTrackers()
	{
		m_bIsDisabled = false;
		if (m_TrackedCamera != null && m_UsedItemPool.Count > 0)
		{
			TurnOnLayerInCullingMask(m_TrackedCamera, m_OurLayer);
		}
	}

	public void SetTrackingCamera(Camera camera, CameraManager.PlayerBindingID bindingID)
	{
		m_TrackedCamera = camera;
		m_BindingID = bindingID;
		int layer = LayerMask.NameToLayer("Player0_TrackedTags");
		int layer2 = LayerMask.NameToLayer("Player1_TrackedTags");
		int layer3 = LayerMask.NameToLayer("Player2_TrackedTags");
		int layer4 = LayerMask.NameToLayer("Player3_TrackedTags");
		int layer5 = LayerMask.NameToLayer("Global_Inside_TrackedTags");
		int layer6 = LayerMask.NameToLayer("Global_Outside_TrackedTags");
		TurnOffLayerInCullingMask(m_TrackedCamera, layer);
		TurnOffLayerInCullingMask(m_TrackedCamera, layer2);
		TurnOffLayerInCullingMask(m_TrackedCamera, layer3);
		TurnOffLayerInCullingMask(m_TrackedCamera, layer4);
		TurnOffLayerInCullingMask(m_TrackedCamera, layer5);
		TurnOffLayerInCullingMask(m_TrackedCamera, layer6);
	}

	public void SetLayer(int layer)
	{
		m_OurLayer = layer;
		if (!m_bIsDisabled)
		{
			TurnOnLayerInCullingMask(m_TrackedCamera, m_OurLayer);
		}
		for (int i = 0; i < 8; i++)
		{
			Transform[] componentsInChildren = m_TrackedItemPool[i].GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				transform.gameObject.layer = layer;
			}
		}
		m_TrackedItemsCanvas.gameObject.layer = m_OurLayer;
		if (m_TargetHUD != null)
		{
			m_TargetHUD.gameObject.layer = m_OurLayer;
		}
		if (m_CombatTargetHUD != null)
		{
			m_CombatTargetHUD.gameObject.layer = m_OurLayer;
		}
		if (m_CombatTargetHealthSlider != null)
		{
			m_CombatTargetHealthSlider.gameObject.layer = m_OurLayer;
		}
		if (m_PlayerHealthSlider != null)
		{
			m_PlayerHealthSlider.gameObject.layer = m_OurLayer;
		}
		if (m_IncidentalHealthSliderPool != null)
		{
			for (uint num = 0u; num < m_IncidentalHealthSliderPool.Length; num++)
			{
				if (m_IncidentalHealthSliderPool[num] != null)
				{
					m_IncidentalHealthSliderPool[num].gameObject.layer = m_OurLayer;
				}
			}
		}
		if (m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.layer = m_OurLayer;
		}
		if (m_SolitaryPotatoMasher != null)
		{
			m_SolitaryPotatoMasher.gameObject.layer = m_OurLayer;
		}
		if (m_ReadingMasher != null)
		{
			m_ReadingMasher.gameObject.layer = m_OurLayer;
		}
		if (m_ThreadmillAndExerciseBikeMasher != null)
		{
			m_ThreadmillAndExerciseBikeMasher.gameObject.layer = m_OurLayer;
		}
		if (m_WeightLiftingButtonMasher != null)
		{
			m_WeightLiftingButtonMasher.gameObject.layer = m_OurLayer;
		}
		if (m_PullUpMasher != null)
		{
			m_PullUpMasher.gameObject.layer = m_OurLayer;
		}
		if (m_KettlebellsMasher != null)
		{
			m_KettlebellsMasher.gameObject.layer = m_OurLayer;
		}
		if (m_PommelAndFootbadMasher != null)
		{
			m_PommelAndFootbadMasher.gameObject.layer = m_OurLayer;
		}
		SetLayerRecursively(base.gameObject, layer);
	}

	private T17TrackedUIElement GetFirstUnusedElement()
	{
		for (int i = 1; i < 8; i++)
		{
			if (m_TrackedItemPool[i].AttachedTo == null && m_TrackedItemPool[i].GhostedTo == null)
			{
				return m_TrackedItemPool[i];
			}
		}
		return null;
	}

	private T17TrackedUIElement GetFirstUnusedElement(TrackableUIElementsReporter reporter, bool attemptToFindHistoricallyAssignedElement)
	{
		int num = -1;
		for (int i = 1; i < 8; i++)
		{
			if (num == -1 && m_TrackedItemPool[i].AttachedTo == null && m_TrackedItemPool[i].GhostedTo == null)
			{
				num = i;
			}
			if (attemptToFindHistoricallyAssignedElement && (m_TrackedItemPool[i].AttachedTo == reporter || m_TrackedItemPool[i].LastValidAttachedTo == reporter))
			{
				return m_TrackedItemPool[i];
			}
		}
		if (num != -1)
		{
			return m_TrackedItemPool[num];
		}
		return null;
	}

	public bool ChangeElementPriority(T17TrackedUIElement element, int priority)
	{
		if (element == null)
		{
			return false;
		}
		priority = Mathf.Clamp(priority, -1, 7);
		if (element.m_NamePlate != null)
		{
			switch (priority)
			{
			case -1:
				element.SetNameplateHighlight(highlight: false);
				element.transform.SetSiblingIndex(6);
				break;
			case 0:
				element.SetNameplateHighlight(highlight: true);
				element.transform.SetSiblingIndex(7);
				break;
			default:
				element.SetNameplateHighlight(highlight: false);
				element.transform.SetSiblingIndex(6 - priority);
				break;
			}
		}
		return true;
	}

	public void AddGhostElement(T17TrackedUIElement element)
	{
		m_GhostItemPool.Add(element);
	}

	public T17TrackedUIElement AttachFirstUnusedElementToReporer(TrackableUIElementsReporter reporter, int priority, bool isElementFarAway, bool attemptToFindHistoricallyAssignedElement)
	{
		if (reporter == null)
		{
			return null;
		}
		T17TrackedUIElement element = ((priority != -1) ? GetFirstUnusedElement(reporter, attemptToFindHistoricallyAssignedElement) : m_MyTrackedItem);
		if (element != null)
		{
			element.SetAttachedToReporter(reporter);
			element.SetIsPlateFarAway(isElementFarAway);
			AddElementToUsedPool(element);
		}
		if (element == null)
		{
			return null;
		}
		priority = Mathf.Clamp(priority, -1, 7);
		if (element.m_NamePlate != null)
		{
			ChangeElementPriority(element, priority);
		}
		reporter.AttachUITrackedElement(ref element, isElementFarAway);
		return element;
	}

	public void AddElementToUsedPool(T17TrackedUIElement element)
	{
		if (!element.gameObject.activeSelf)
		{
			element.gameObject.SetActive(value: true);
		}
		element.m_CameraBindingID = m_BindingID;
		UpdatePosition(element);
		if (!m_UsedItemPool.Contains(element))
		{
			m_UsedItemPool.Add(element);
		}
		if (!m_bIsDisabled)
		{
			TurnOnLayerInCullingMask(m_TrackedCamera, m_OurLayer);
		}
	}

	public bool ReleaseTrackedUIElement(T17TrackedUIElement element, bool doLayerMaskChanges = true)
	{
		if (element == null)
		{
			return false;
		}
		for (int num = m_UsedItemPool.Count - 1; num >= 0; num--)
		{
			if (m_UsedItemPool[num] == element)
			{
				if (m_UsedItemPool[num].Flags == 0)
				{
					m_UsedItemPool[num].ResetAll();
					m_UsedItemPool[num].gameObject.SetActive(value: false);
					m_UsedItemPool.RemoveAt(num);
				}
				if (doLayerMaskChanges && m_UsedItemPool.Count == 0 && m_GhostItemPool.Count == 0 && m_TrackedCamera != null)
				{
					TurnOffLayerInCullingMask(m_TrackedCamera, m_OurLayer);
				}
				return true;
			}
		}
		for (int num2 = m_GhostItemPool.Count - 1; num2 >= 0; num2--)
		{
			if (m_GhostItemPool[num2] == element)
			{
				m_GhostItemPool.RemoveAt(num2);
				if (doLayerMaskChanges && m_UsedItemPool.Count == 0 && m_GhostItemPool.Count == 0 && m_TrackedCamera != null)
				{
					TurnOffLayerInCullingMask(m_TrackedCamera, m_OurLayer);
				}
				return true;
			}
		}
		return false;
	}

	public bool ReleaseGhostTrackedUIElement(T17TrackedUIElement element)
	{
		if (element == null)
		{
			return false;
		}
		for (int num = m_GhostItemPool.Count - 1; num >= 0; num--)
		{
			if (m_GhostItemPool[num] == element)
			{
				m_GhostItemPool.RemoveAt(num);
				if (m_UsedItemPool.Count == 0 && m_GhostItemPool.Count == 0 && m_TrackedCamera != null)
				{
					m_TrackedCamera.cullingMask &= ~(1 << m_OurLayer);
				}
				return true;
			}
		}
		return false;
	}

	public bool ReleaseTrackedUIElementWithoutDisable(T17TrackedUIElement element)
	{
		if (element == null)
		{
			return false;
		}
		for (int num = m_UsedItemPool.Count - 1; num >= 0; num--)
		{
			if (m_UsedItemPool[num] == element)
			{
				m_UsedItemPool[num].SetAttachedToReporter(null);
				m_UsedItemPool.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	private Vector2 TileToHUD(int tileRow, int tileColumn)
	{
		Vector2 zero = Vector2.zero;
		if (m_CanvasRect != null)
		{
			float num = m_CanvasRect.sizeDelta.x / 2f / (float)m_CanvasScalar;
			float num2 = m_CanvasRect.sizeDelta.y / 2f / (float)m_CanvasScalar;
			zero.x = (float)tileColumn - num;
			zero.y = num2 - (float)tileRow;
			zero.x += 0.5f;
			zero.y -= 0.5f;
		}
		return zero;
	}

	private void TurnOnLayerInCullingMask(Camera camera, int layer)
	{
		camera.cullingMask |= 1 << layer;
	}

	private void TurnOffLayerInCullingMask(Camera camera, int layer)
	{
		camera.cullingMask &= ~(1 << layer);
	}

	public void ShowTarget(int tileRow, int tileColumn, bool bValidPosition, Sprite validSpriteOverride = null)
	{
		if (m_TargetHUD != null)
		{
			m_TargetHUD.SetValidSpriteOverride(validSpriteOverride);
			m_TargetHUD.SetPosition(TileToHUD(tileRow, tileColumn));
			m_TargetHUD.SetPositionIsValid(bValidPosition);
			m_TargetHUD.Show();
		}
	}

	public void HideTarget()
	{
		if (m_TargetHUD != null)
		{
			m_TargetHUD.Hide();
		}
	}

	public void ShowCombatTarget(Vector3 targetPosition, bool isFriendly)
	{
		if (m_CombatTargetHUD != null)
		{
			m_CombatTargetHUD.SetPosition(targetPosition);
			m_CombatTargetHUD.SetPositionIsValid(isFriendly);
			m_CombatTargetHUD.Show();
		}
	}

	public void HideCombatTarget()
	{
		if (m_CombatTargetHUD != null)
		{
			m_CombatTargetHUD.Hide();
		}
	}

	public void ShowCombatTargetHealth(Character character)
	{
		if (character != null && m_CombatTargetHealthSlider != null)
		{
			float z = base.transform.position.z;
			m_CombatTargetHealthSlider.m_MaxValue = (int)(float)character.m_CharacterStats.m_HealthBaseLine;
			m_CombatTargetHealthSlider.SetValue(Mathf.CeilToInt(character.m_CharacterStats.Health));
			m_CombatTargetHealthSlider.transform.position = new Vector3(character.m_Transform.position.x, character.m_Transform.position.y - 0.6f, z);
			m_CombatTargetHealthSlider.gameObject.SetActive(value: true);
		}
	}

	public void HideCombatTargetHealth()
	{
		if (m_CombatTargetHealthSlider != null)
		{
			m_CombatTargetHealthSlider.gameObject.SetActive(value: false);
		}
	}

	public void ShowCharacterHealth(Character character)
	{
		if (character == null || character.m_CharacterStats == null || m_ActiveIncidentalHealthSliders == null || m_IncidentalHealthSliderPool == null)
		{
			return;
		}
		T17StatsSlider value = null;
		if (!m_ActiveIncidentalHealthSliders.TryGetValue(character, out value))
		{
			for (uint num = 0u; num < m_IncidentalHealthSliderPool.Length; num++)
			{
				if (m_IncidentalHealthSliderPool[num] != null && !m_IncidentalHealthSliderPool[num].isActiveAndEnabled)
				{
					value = m_IncidentalHealthSliderPool[num];
					break;
				}
			}
			if (value == null)
			{
				FastList<Character> fastList = new FastList<Character>(m_ActiveIncidentalHealthSliders.Keys);
				Character character2 = null;
				float num2 = 0f;
				for (int i = 0; i < fastList.Count; i++)
				{
					if (character2 == null || fastList[i].GetTimeLastHit() < num2)
					{
						character2 = fastList[i];
						num2 = fastList[i].GetTimeLastHit();
					}
				}
				value = m_ActiveIncidentalHealthSliders[character2];
				m_ActiveIncidentalHealthSliders.Remove(character2);
			}
			value.m_MaxValue = (int)(float)character.m_CharacterStats.m_HealthBaseLine;
			value.gameObject.SetActive(value: true);
			m_ActiveIncidentalHealthSliders.Add(character, value);
		}
		float z = base.transform.position.z;
		value.transform.position = new Vector3(character.m_Transform.position.x, character.m_Transform.position.y - 0.6f, z);
		value.SetValue(Mathf.CeilToInt(character.m_CharacterStats.Health));
	}

	public void HideCharacterHealth(Character character)
	{
		if (!(character == null) && m_ActiveIncidentalHealthSliders != null)
		{
			T17StatsSlider value = null;
			m_ActiveIncidentalHealthSliders.TryGetValue(character, out value);
			if (value != null)
			{
				value.gameObject.SetActive(value: false);
				m_ActiveIncidentalHealthSliders.Remove(character);
			}
		}
	}

	public void ShowPlayerHealth(Player player)
	{
		if (player != null && m_PlayerHealthSlider != null)
		{
			float z = base.transform.position.z;
			m_PlayerHealthSlider.m_MaxValue = (int)(float)player.m_CharacterStats.m_HealthBaseLine;
			m_PlayerHealthSlider.SetValue(Mathf.CeilToInt(player.m_CharacterStats.Health));
			m_PlayerHealthSlider.transform.position = new Vector3(player.m_Transform.position.x, player.m_Transform.position.y - 0.6f, z);
			m_PlayerHealthSlider.gameObject.SetActive(value: true);
		}
	}

	public void HidePlayerHealth()
	{
		if (m_PlayerHealthSlider != null)
		{
			m_PlayerHealthSlider.gameObject.SetActive(value: false);
		}
	}

	public AlternateButtonMasher GetButtonMasher()
	{
		return m_ButtonMasher;
	}

	public void ShowButtonMasher(Player player)
	{
		if (player != null && m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.SetActive(value: true);
			m_ButtonMasher.SetPlayerToCheck(player);
		}
	}

	public void HideButtonMasher()
	{
		if (m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.SetActive(value: false);
			m_ButtonMasher.Reset();
		}
	}

	public void SetPressAndHoldPercentage(float percentage, Vector3 position)
	{
		if (m_PressAndHoldImage != null)
		{
			m_PressAndHoldImage.gameObject.SetActive(value: true);
			float z = base.transform.position.z;
			m_PressAndHoldImage.transform.position = new Vector3(position.x + m_TrackedElementOffsetFromObject.x, position.y + m_TrackedElementOffsetFromObject.y, z);
			m_PressAndHoldImage.fillAmount = percentage;
		}
	}

	public void HidePressAndHold()
	{
		if (m_PressAndHoldImage != null)
		{
			m_PressAndHoldImage.gameObject.SetActive(value: false);
		}
	}

	public void SetSmashAttackChargePercentage(float percentage, Vector3 position)
	{
		if (m_SmashAttackChargeImage != null)
		{
			m_SmashAttackChargeImage.gameObject.SetActive(value: true);
			float z = base.transform.position.z;
			m_SmashAttackChargeImage.transform.position = new Vector3(position.x + m_TrackedElementOffsetFromObject.x, position.y + m_TrackedElementOffsetFromObject.y, z);
			m_SmashAttackChargeImage.fillAmount = percentage;
		}
	}

	public void HideSmashAttackCharge()
	{
		if (m_SmashAttackChargeImage != null)
		{
			m_SmashAttackChargeImage.gameObject.SetActive(value: false);
		}
	}

	public void ShowClimbIcon(bool isDown, int tileRow, int tileColumn, Player thePlayer)
	{
		if (m_ClimbIcon != null)
		{
			m_ClimbIcon.gameObject.SetActive(value: true);
			if (isDown)
			{
				m_ClimbIcon.SetIconType(CharacterIconHandler.IconType.ClimbDown);
			}
			else
			{
				m_ClimbIcon.SetIconType(CharacterIconHandler.IconType.ClimbUp);
			}
			Vector3 position = TileToHUD(tileRow, tileColumn);
			position.z = base.transform.position.z;
			m_ClimbIcon.transform.position = position;
		}
		if (m_ClimbInputUp != null)
		{
			m_ClimbInputUp.gameObject.SetActive(!isDown);
			if (thePlayer != null && thePlayer.m_Gamer != null)
			{
				m_ClimbInputUp.SetGamerForEventSystem(thePlayer.m_Gamer, T17EventSystemsManager.Instance.GetEventSystemForGamer(thePlayer.m_Gamer));
			}
		}
		if (m_ClimbInputDown != null)
		{
			m_ClimbInputDown.gameObject.SetActive(isDown);
			if (thePlayer != null && thePlayer.m_Gamer != null)
			{
				m_ClimbInputDown.SetGamerForEventSystem(thePlayer.m_Gamer, T17EventSystemsManager.Instance.GetEventSystemForGamer(thePlayer.m_Gamer));
			}
		}
	}

	public void HideClimbIcon()
	{
		if (m_ClimbIcon != null)
		{
			m_ClimbIcon.gameObject.SetActive(value: false);
		}
		if (m_ClimbInputUp != null)
		{
			m_ClimbInputUp.gameObject.SetActive(value: false);
		}
		if (m_ClimbInputDown != null)
		{
			m_ClimbInputDown.gameObject.SetActive(value: false);
		}
	}

	public void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			SetLayerRecursively(item.gameObject, layer);
		}
	}

	public GymMasher_Threadmill_ExerciseBike GetPlumberMasher()
	{
		return m_PlumberMasher;
	}

	public ReadingMasher GetElectricianMasher()
	{
		return m_ElectricianMasher;
	}

	public SolitaryPotatoMasher GetPaintingMasher()
	{
		return m_PaintingMasher;
	}

	public SolitaryPotatoMasher GetPumpkinCarvingMasher()
	{
		return m_PumpkinCarvingMasher;
	}

	public SolitaryPotatoMasher GetTreeDecorationMasherMasher()
	{
		return m_TreeDecorationMasher;
	}

	public ReadingMasher GetFacePaintingMasher()
	{
		return m_FacePaintingMasher;
	}

	public SolitaryPotatoMasher GetLionTamingMasher()
	{
		return m_LionTamingMasher;
	}

	public SolitaryPotatoMasher GetHangingPostersMasher()
	{
		return m_HangingPosterMasher;
	}

	public ReadingMasher GetHorseshoeAnvilMasher()
	{
		return m_HorseshoeAnvilMasher;
	}

	public SolitaryPotatoMasher GetMinstrelMasher()
	{
		return m_MinstrelMasher;
	}

	public SolitaryPotatoMasher GetStonemasonCarvingMasher()
	{
		return m_StonemasonCarvingMasher;
	}

	public ReadingMasher GetRobotServicingMasher()
	{
		return m_RobotServicingMasher;
	}

	public GymMasher_WeightLifting GetWeightLiftingButtonMasher()
	{
		return m_WeightLiftingButtonMasher;
	}

	public GymMasher_Pullup GetPullupMasher()
	{
		return m_PullUpMasher;
	}

	public GymMasher_KettleBelts GetKettleBellsMasher()
	{
		return m_KettlebellsMasher;
	}

	public GymMasher_Threadmill_ExerciseBike GetThreadmillAndExerciseBikeMasher()
	{
		return m_ThreadmillAndExerciseBikeMasher;
	}

	public GymMasher_Pommel_Footbag GetPommelAndFootbagMasher()
	{
		return m_PommelAndFootbadMasher;
	}

	public CameraManager.PlayerBindingID GetBinding()
	{
		return m_BindingID;
	}

	public void SetScalesOfPerTrackedUI(Vector3 rootScale, Vector3 defaultRootScale)
	{
		if (m_TargetHUD != null)
		{
			m_TargetHUD.transform.localScale = new Vector3(defaultRootScale.x / rootScale.x, defaultRootScale.y / rootScale.y, defaultRootScale.z / rootScale.z);
		}
		Vector3 localScale = ((!HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_BindingID)) ? new Vector3(defaultRootScale.x / rootScale.x, defaultRootScale.y / rootScale.y, defaultRootScale.z / rootScale.z) : Vector3.one);
		for (int i = 0; i < m_TrackedItemPool.Length; i++)
		{
			if (m_TrackedItemPool[i] != null)
			{
				m_TrackedItemPool[i].transform.localScale = localScale;
			}
		}
	}

	public bool AnyMasherUIActive()
	{
		if (m_ButtonMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_KettlebellsMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_PaintingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_PlumberMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_PommelAndFootbadMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_PullUpMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ReadingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_SolitaryPotatoMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ThreadmillAndExerciseBikeMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_WeightLiftingButtonMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_PumpkinCarvingMasher != null && m_PumpkinCarvingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_TreeDecorationMasher != null && m_TreeDecorationMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_LionTamingMasher != null && m_LionTamingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_HangingPosterMasher != null && m_HangingPosterMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_HorseshoeAnvilMasher != null && m_HorseshoeAnvilMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_MinstrelMasher != null && m_MinstrelMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_StonemasonCarvingMasher != null && m_StonemasonCarvingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_RobotServicingMasher != null && m_RobotServicingMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ST_FireBreathingPotatoMasher != null && m_ST_FireBreathingPotatoMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ST_HulaHoopPotatoMasher != null && m_ST_HulaHoopPotatoMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ST_JugglingPotatoMasher != null && m_ST_JugglingPotatoMasher.isActiveAndEnabled)
		{
			return true;
		}
		if (m_ST_UnicyclePotatoMasher != null && m_ST_UnicyclePotatoMasher.isActiveAndEnabled)
		{
			return true;
		}
		return false;
	}

	public ReadingMasher GetReadingMasher()
	{
		return m_ReadingMasher;
	}

	public SolitaryPotatoMasher GetSolitaryPotatoMasher()
	{
		return m_SolitaryPotatoMasher;
	}

	public SolitaryPotatoMasher GetShowTimeJugglingMasher()
	{
		return m_ST_JugglingPotatoMasher;
	}

	public SolitaryPotatoMasher GetShowTimeHulaHoopsMasher()
	{
		return m_ST_HulaHoopPotatoMasher;
	}

	public SolitaryPotatoMasher GetShowTimeFireBreathingMasher()
	{
		return m_ST_FireBreathingPotatoMasher;
	}

	public SolitaryPotatoMasher GetShowTimeUnicycleMasher()
	{
		return m_ST_UnicyclePotatoMasher;
	}
}

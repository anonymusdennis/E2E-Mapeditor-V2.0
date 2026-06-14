using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class MainMap : MonoBehaviour
{
	public T17RawImage m_ActiveRawImage;

	public T17RawImage m_BGRawImage;

	public T17RawImage[] m_UnderImages;

	public float m_InitBGWidth = 50f;

	public float m_InitBGHeight = 50f;

	public float m_MaxMapZoom = 8f;

	public float m_MinMapZoom = 1f;

	private bool m_bZooming;

	private bool m_bZoomingSoundOn;

	public float m_MapZoom = 4f;

	public float m_StepScale = 120f;

	public float m_mapMovementSpeed = 20f;

	public float m_mapZoomSpeed = 2f;

	private FloorManager m_FloorMan;

	private FloorManager.Floor m_Floor;

	private float m_InitMapZoom = 3f;

	private Vector2 m_CentrePos;

	private Gamer m_Gamer;

	private PinManager m_PinMan;

	private IconPool m_IconPool;

	private PinManager.Pin.PinFilterType m_ActiveFilterType;

	private Vector2 m_MapViewScale;

	public Vector3 m_ScaleOfIcons = new Vector3(8f, 8f, 1f);

	public T17Text m_FilterHeader;

	private MainMapKeyFilter m_Filter;

	public GameObject m_KeyRoot;

	public T17Button[] FloorButtons;

	private MapToolTip m_ToolTip;

	private MapToolTip m_MouseToolTip;

	private Vector2 m_MouseToolTipOffset = new Vector2(0f, 18f);

	private bool m_bMouseMoveMap;

	private Vector2 m_PreviousMousePos = Vector2.zero;

	private int[] m_ButtonFloors = new int[3];

	public GameObject m_PlayerLayerObject;

	public GameObject m_CharacterLayerObject;

	public GameObject m_ShopLayerObject;

	public GameObject m_FavoursLayerObject;

	public GameObject m_ObjectiveLayerObject;

	public GameObject m_TagLayerObject;

	public Vector3[] m_FloorIconPos16x16;

	public Vector3[] m_FloorIconPos32x32;

	private void Awake()
	{
		if (m_FloorIconPos16x16 == null || m_FloorIconPos16x16.Length <= 0)
		{
			m_FloorIconPos16x16 = new Vector3[2]
			{
				new Vector3(5.41f, 5.5f, 9.09f),
				new Vector3(13.44f, 2.55f, 9.09f)
			};
		}
		if (m_FloorIconPos32x32 == null || m_FloorIconPos32x32.Length <= 0)
		{
			m_FloorIconPos32x32 = new Vector3[2]
			{
				new Vector3(9f, 10f, 9.09f),
				new Vector3(25f, 12f, 9.09f)
			};
		}
	}

	private void Start()
	{
		m_FloorMan = FloorManager.GetInstance();
		m_PinMan = PinManager.GetInstance();
		m_IconPool = base.transform.parent.GetComponentInChildren<IconPool>();
		m_Filter = base.transform.GetComponent<MainMapKeyFilter>();
		m_ToolTip = base.transform.parent.GetComponentInChildren<MapToolTip>(includeInactive: true);
		if (m_ToolTip != null)
		{
			m_MouseToolTip = Object.Instantiate(m_ToolTip, base.transform.parent);
		}
		for (int i = 0; i < FloorButtons.Length; i++)
		{
			FloorButtons[i].m_CanUIReselectDelegate = () => false;
		}
	}

	protected virtual void OnDestroy()
	{
		m_FloorMan = null;
		m_PinMan = null;
	}

	public void FirstTimeSetup()
	{
		if (m_IconPool == null)
		{
			m_IconPool = base.transform.parent.GetComponentInChildren<IconPool>();
			m_IconPool.FreeAllObjects();
		}
		IconData[] componentsInChildren = m_IconPool.GetComponentsInChildren<IconData>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
	}

	public void InitMap(Vector2 initWorldPos, FloorManager.Floor startFloor, Gamer gamer, Vector2 mapViewScale)
	{
		m_Gamer = gamer;
		m_CentrePos = initWorldPos;
		m_MapZoom = m_InitMapZoom;
		m_bZooming = false;
		m_bZoomingSoundOn = false;
		float num = m_ActiveRawImage.rectTransform.rect.width * mapViewScale.x;
		float num2 = m_ActiveRawImage.rectTransform.rect.height * mapViewScale.y;
		float num3 = num / num2;
		float num4 = 1f / m_MapZoom * num3;
		float num5 = 1f / m_MapZoom;
		m_MapViewScale = mapViewScale;
		m_ActiveRawImage.texture = startFloor.m_MapTexture;
		int maxRows = 120;
		int maxColumns = 120;
		if (m_FloorMan == null)
		{
			m_FloorMan = FloorManager.GetInstance();
		}
		m_FloorMan.GetTileSystemBounds(startFloor, FloorManager.TileSystem_Type.TileSystem_Ground, out maxRows, out maxColumns);
		m_Floor = startFloor;
		HandleFloors();
		m_CentrePos.x = (m_CentrePos.x + (float)(maxColumns / 2)) / (float)maxColumns;
		m_CentrePos.y = (m_CentrePos.y + (float)(maxRows / 2)) / (float)maxRows;
		float num6 = (float)maxColumns / (float)maxRows;
		m_ActiveRawImage.uvRect = new Rect(m_CentrePos.x - num4 / 2f, m_CentrePos.y - num5 / 2f, num4, num5 * num6);
		m_BGRawImage.uvRect = new Rect((m_CentrePos.x - num4 / 2f) * m_InitBGWidth, (m_CentrePos.y - num5 / 2f) * m_InitBGHeight, m_InitBGWidth * num3 * 1f / m_MapZoom, m_InitBGHeight * 1f / m_MapZoom);
		m_ActiveFilterType = PinManager.Pin.PinFilterType.All;
		if (m_FilterHeader != null)
		{
			m_FilterHeader.SetNewLocalizationTag("Text.MapFilter." + m_ActiveFilterType);
		}
		if (m_Filter == null)
		{
			m_Filter = base.transform.GetComponent<MainMapKeyFilter>();
		}
		SetKeyFilterElements();
		if (m_ToolTip != null && m_Gamer != null && m_Gamer.m_PlayerObject != null)
		{
			m_ToolTip.SetCameraBinding(m_Gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
			m_ToolTip.SetLocalScaleSplit();
			if (m_MouseToolTip != null)
			{
				m_MouseToolTip.SetCameraBinding(m_Gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
				m_MouseToolTip.SetLocalScaleSplit();
			}
		}
		if (CullingObjectCollector.GetInstance() != null)
		{
			CullingObjectCollector.GetInstance().HideAllMode(bHide: true, m_Gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
		}
	}

	public void Hiding()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_MiniMap_Zoom, base.gameObject);
		if (CullingObjectCollector.GetInstance() != null && m_Gamer != null)
		{
			CullingObjectCollector.GetInstance().HideAllMode(bHide: false, m_Gamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
		}
	}

	private void Update()
	{
		if (m_Floor == null || m_ActiveRawImage == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			flag = m_Gamer.m_RewiredPlayer.GetButton("ZoomIn");
			flag2 = m_Gamer.m_RewiredPlayer.GetButton("ZoomOut");
			flag3 = m_Gamer.m_RewiredPlayer.GetButtonDown("UpAFloor");
			flag4 = m_Gamer.m_RewiredPlayer.GetButtonDown("DownAFloor");
			flag5 = m_Gamer.m_RewiredPlayer.GetButtonDown("CycleFilter1");
			flag6 = m_Gamer.m_RewiredPlayer.GetButtonDown("CycleFilter2");
		}
		bool bZooming = m_bZooming;
		if (flag3)
		{
			FloorManager.Floor floor = m_Floor;
			m_Floor = m_FloorMan.UpAFloor(m_Floor);
			if (floor != m_Floor)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_FloorUp, base.gameObject);
			}
		}
		else if (flag4)
		{
			FloorManager.Floor floor2 = m_Floor;
			m_Floor = m_FloorMan.DownAFloor(m_Floor);
			if (floor2 != m_Floor)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_FloorDoor, base.gameObject);
			}
		}
		HandleFloors();
		m_ActiveRawImage.texture = m_Floor.m_MapTexture;
		FloorManager.Floor floor3 = m_FloorMan.DownAFloor(m_Floor);
		for (int i = 0; i < m_UnderImages.Length; i++)
		{
			if (!floor3.IsVent() && !floor3.IsUnderGround())
			{
				m_UnderImages[i].texture = floor3.m_MapTexture;
				m_UnderImages[i].gameObject.SetActive(value: true);
			}
			else
			{
				m_UnderImages[i].texture = null;
				m_UnderImages[i].gameObject.SetActive(value: false);
			}
			floor3 = m_FloorMan.DownAFloor(floor3);
		}
		float num = m_ActiveRawImage.rectTransform.rect.width * m_MapViewScale.x;
		float num2 = m_ActiveRawImage.rectTransform.rect.height * m_MapViewScale.y;
		float num3 = num / num2;
		m_bZooming = false;
		if (flag)
		{
			m_MapZoom += m_mapZoomSpeed * UpdateManager.deltaTime;
			if (m_MapZoom >= m_MaxMapZoom)
			{
				m_MapZoom = m_MaxMapZoom;
			}
			else
			{
				m_bZooming = true;
			}
		}
		else if (flag2)
		{
			m_MapZoom -= m_mapZoomSpeed * UpdateManager.deltaTime;
			if (m_MapZoom <= m_MinMapZoom)
			{
				m_MapZoom = m_MinMapZoom;
			}
			else
			{
				m_bZooming = true;
			}
		}
		if (!m_bZoomingSoundOn && !bZooming && m_bZooming)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Zoom, base.gameObject);
			m_bZoomingSoundOn = true;
		}
		else if (m_bZoomingSoundOn && !bZooming && !m_bZooming)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_MiniMap_Zoom, base.gameObject);
			m_bZoomingSoundOn = false;
		}
		if (m_bZoomingSoundOn && m_bZooming)
		{
			float value = (m_MapZoom - m_MinMapZoom) / (m_MaxMapZoom - m_MinMapZoom);
			AudioController.SetParameter(Game_Parameter.MiniMap_Zoom, value);
		}
		float num4 = 1f / m_MapZoom * num3;
		float num5 = 1f / m_MapZoom;
		Vector2 vector = default(Vector2);
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			vector.x = m_Gamer.m_RewiredPlayer.GetAxis("MoveMap_Horizontal");
			vector.y = m_Gamer.m_RewiredPlayer.GetAxis("MoveMap_Vertical");
		}
		if (vector.magnitude > 0.01f)
		{
			vector.Normalize();
			vector /= m_StepScale;
			m_CentrePos += vector * m_mapMovementSpeed * UpdateManager.deltaTime;
			m_CentrePos.x = Mathf.Clamp(m_CentrePos.x, 0f, 1f);
			m_CentrePos.y = Mathf.Clamp(m_CentrePos.y, 0f, 1f);
		}
		int maxRows = 1;
		int maxColumns = 1;
		m_FloorMan.GetTileSystemBounds(m_Floor, FloorManager.TileSystem_Type.TileSystem_Ground, out maxRows, out maxColumns);
		float num6 = (float)maxColumns / (float)maxRows;
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.controllers != null && m_Gamer.m_RewiredPlayer.controllers.Mouse != null)
		{
			if (!m_Gamer.m_RewiredPlayer.GetButton("MoveMap0"))
			{
				m_bMouseMoveMap = false;
			}
			else if (!m_bMouseMoveMap)
			{
				m_bMouseMoveMap = true;
				m_PreviousMousePos = m_Gamer.m_RewiredPlayer.controllers.Mouse.screenPosition;
			}
			else
			{
				Vector2 screenPosition = m_Gamer.m_RewiredPlayer.controllers.Mouse.screenPosition;
				Vector2 vector2 = screenPosition - m_PreviousMousePos;
				vector = -vector2;
				m_PreviousMousePos = screenPosition;
				Vector2 vector3 = new Vector2(num4 / (float)Screen.width, num5 / (float)Screen.height);
				vector.x *= vector3.x;
				vector.y *= vector3.y;
				vector.y *= num6;
				m_CentrePos += vector;
			}
		}
		Vector2 centrePos = m_CentrePos;
		centrePos.x -= num4 / 2f;
		centrePos.y -= num5 * num6 / 2f;
		m_ActiveRawImage.uvRect = new Rect(centrePos.x, centrePos.y, num4, num5 * num6);
		for (int j = 0; j < m_UnderImages.Length; j++)
		{
			if (m_UnderImages[j].texture != null)
			{
				m_UnderImages[j].uvRect = new Rect(centrePos.x, centrePos.y, num4, num5 * num6);
			}
		}
		m_BGRawImage.uvRect = new Rect(centrePos.x * m_InitBGWidth, centrePos.y * m_InitBGHeight, m_InitBGWidth * num3 * 1f / m_MapZoom, m_InitBGHeight * 1f / m_MapZoom);
		if (flag5)
		{
			m_ActiveFilterType++;
			if (m_ActiveFilterType == PinManager.Pin.PinFilterType.Count)
			{
				m_ActiveFilterType = PinManager.Pin.PinFilterType.All;
			}
			if (m_FilterHeader != null)
			{
				m_FilterHeader.SetNewLocalizationTag("Text.MapFilter." + m_ActiveFilterType);
			}
			SetKeyFilterElements();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Marker_In, base.gameObject);
		}
		else if (flag6)
		{
			if (m_ActiveFilterType > PinManager.Pin.PinFilterType.All)
			{
				m_ActiveFilterType--;
			}
			else
			{
				m_ActiveFilterType = PinManager.Pin.PinFilterType.Tags;
			}
			if (m_FilterHeader != null)
			{
				m_FilterHeader.SetNewLocalizationTag("Text.MapFilter." + m_ActiveFilterType);
			}
			SetKeyFilterElements();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Marker_In, base.gameObject);
		}
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.GetButtonDown("ToggleKey"))
		{
			m_KeyRoot.SetActive(!m_KeyRoot.GetActive());
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Toggle, base.gameObject);
		}
		UpdatePins(m_ActiveRawImage.uvRect);
	}

	private void UpdatePins(Rect mapRect)
	{
		if (m_ActiveRawImage == null || m_ActiveRawImage.rectTransform == null)
		{
			return;
		}
		Vector3 localPosition = new Vector3(0f, 0f, 0f);
		Vector3 eulerAngles = new Vector3(0f, 0f, 0f);
		Vector2 vector = new Vector2(m_ActiveRawImage.rectTransform.rect.width / mapRect.width, m_ActiveRawImage.rectTransform.rect.height / mapRect.height);
		Vector2 vector2 = new Vector2(m_ActiveRawImage.rectTransform.rect.width * 0.5f, m_ActiveRawImage.rectTransform.rect.height * 0.5f);
		bool flag = false;
		if (null != m_ToolTip && null != m_ToolTip.gameObject)
		{
			if (m_ToolTip.gameObject.activeInHierarchy)
			{
				flag = true;
			}
			m_ToolTip.gameObject.SetActive(value: false);
		}
		bool flag2 = false;
		if (m_MouseToolTip != null && m_MouseToolTip.gameObject != null)
		{
			if (m_MouseToolTip.gameObject.activeInHierarchy)
			{
				flag2 = true;
			}
			m_MouseToolTip.gameObject.SetActive(value: false);
		}
		if (m_PinMan == null || m_PinMan.m_MainMapPins == null || m_IconPool == null || m_ActiveRawImage == null || m_ToolTip == null)
		{
			return;
		}
		for (int i = 0; i < m_PinMan.m_MainMapPins.Count; i++)
		{
			PinManager.Pin pin = m_PinMan.m_MainMapPins[i];
			if (pin == null)
			{
				T17NetManager.LogGoogleException($"MainMap::UpdatePins - Pin at index '{i}' is null!");
				continue;
			}
			Dictionary<int, PinManager.Pin.PlayerIcons> playerIcons = pin.m_PlayerIcons;
			if (m_Gamer == null || playerIcons == null || playerIcons.Count <= 0 || !playerIcons.ContainsKey(m_Gamer.m_NetViewID))
			{
				continue;
			}
			PinManager.Pin.PlayerIcons playerIcons2 = playerIcons[m_Gamer.m_NetViewID];
			if (playerIcons2 == null)
			{
				continue;
			}
			T17Image t17Image = playerIcons2.m_MainMapIcon;
			if (pin.m_Floor != null && pin.m_Floor != m_Floor && !pin.m_FloorTrackable)
			{
				if (t17Image != null)
				{
					m_IconPool.FreeObject(t17Image.gameObject);
					t17Image = null;
					playerIcons2.m_MainMapIconPool = null;
					playerIcons2.m_MainMapIcon = null;
				}
				continue;
			}
			Vector2 mapPos = pin.m_MapPos;
			if (mapRect.Contains(mapPos) && (pin.m_FilterType == PinManager.Pin.PinFilterType.All || pin.m_FilterType == m_ActiveFilterType || m_ActiveFilterType == PinManager.Pin.PinFilterType.All))
			{
				if (t17Image == null)
				{
					GameObject iconObject = GetIconObject(pin.m_FilterType, pin.m_bIsPlayer);
					if (iconObject != null)
					{
						t17Image = iconObject.GetComponent<T17Image>();
					}
					playerIcons2.m_MainMapIconPool = m_IconPool;
					playerIcons2.m_MainMapIcon = t17Image;
					if (null == t17Image)
					{
						continue;
					}
					t17Image.transform.SetSiblingIndex(Mathf.RoundToInt((float)t17Image.transform.parent.childCount * m_PinMan.GetIconPriorityMultiplier(pin.m_FilterType)));
				}
				t17Image.sprite = pin.m_IconSprite;
				IconData component = t17Image.GetComponent<IconData>();
				if (pin.m_IconSprite != null)
				{
					Vector2 vector3 = new Vector2(pin.m_IconSprite.rect.width, pin.m_IconSprite.rect.height);
					if (t17Image.rectTransform.sizeDelta != vector3)
					{
						t17Image.rectTransform.sizeDelta = vector3;
						if (component != null)
						{
							if (vector3.x == 16f)
							{
								if (component.UpArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos16x16[0])
								{
									component.UpArrow.rectTransform.anchoredPosition = m_FloorIconPos16x16[0];
								}
								if (component.DownArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos16x16[1])
								{
									component.DownArrow.rectTransform.anchoredPosition = m_FloorIconPos16x16[1];
								}
							}
							else if (vector3.x == 32f)
							{
								if (component.UpArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos32x32[0])
								{
									component.UpArrow.rectTransform.anchoredPosition = m_FloorIconPos32x32[0];
								}
								if (component.DownArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos32x32[1])
								{
									component.DownArrow.rectTransform.anchoredPosition = m_FloorIconPos32x32[1];
								}
							}
						}
					}
				}
				if (!pin.m_bOverrideIconScale)
				{
					Vector3 localScale = new Vector3(m_MapZoom * m_ScaleOfIcons.x * (m_ActiveRawImage.rectTransform.rect.height / 1080f), m_MapZoom * m_ScaleOfIcons.y * (m_ActiveRawImage.rectTransform.rect.height / 1080f), 1f);
					t17Image.transform.localScale = localScale;
				}
				else
				{
					Vector3 localScale2 = new Vector3(m_MapZoom * pin.m_OverrideIconScale.x, m_MapZoom * pin.m_OverrideIconScale.y, 1f);
					t17Image.transform.localScale = localScale2;
				}
				if (component == null)
				{
					T17NetManager.LogGoogleException($"MainMap::UpdatePins - IconData was null for pin at index '{i}'!");
				}
				else if (pin.m_FloorTrackable)
				{
					if (pin.m_Floor == null)
					{
						T17NetManager.LogGoogleException($"MainMap::UpdatePins - pin.m_Floor was null for pin at index '{i}'!");
					}
					else if (pin.m_Floor.m_FloorIndex > m_Floor.m_FloorIndex)
					{
						component.SetActive(activeUp: true, activeDown: false);
					}
					else if (pin.m_Floor.m_FloorIndex < m_Floor.m_FloorIndex)
					{
						component.SetActive(activeUp: false, activeDown: true);
					}
					else
					{
						component.SetActive(activeUp: false, activeDown: false);
						component.DownArrow.transform.rotation = Quaternion.identity;
						component.UpArrow.transform.rotation = Quaternion.identity;
					}
				}
				else
				{
					component.SetActive(activeUp: false, activeDown: false);
					component.DownArrow.transform.rotation = Quaternion.identity;
					component.UpArrow.transform.rotation = Quaternion.identity;
				}
				mapPos.x -= mapRect.xMin;
				mapPos.y -= mapRect.yMin;
				localPosition.x = mapPos.x * vector.x - vector2.x;
				localPosition.y = mapPos.y * vector.y - vector2.y;
				localPosition.z = t17Image.transform.localPosition.z;
				t17Image.transform.localPosition = localPosition;
				if (pin.m_IconSprite != null)
				{
					float num = pin.m_IconSprite.rect.width * 3f;
					float num2 = pin.m_IconSprite.rect.height * 3f;
					Rect rect = new Rect(t17Image.transform.localPosition.x - num / 2f, t17Image.transform.localPosition.y - num2 / 2f, num, num2);
					if (rect.Contains(new Vector2(0f, 0f)) && !string.IsNullOrEmpty(pin.m_ToolTipTag) && null != m_ToolTip.m_Description)
					{
						m_ToolTip.gameObject.SetActive(value: true);
						if (pin.m_LocaliseToolTipTag)
						{
							m_ToolTip.m_Description.m_bNeedsLocalization = true;
							m_ToolTip.m_Description.SetLocalisedTextCatchAll(pin.m_ToolTipTag);
						}
						else
						{
							m_ToolTip.m_Description.m_bNeedsLocalization = false;
							m_ToolTip.m_Description.m_LocalizationTag = null;
							m_ToolTip.m_Description.text = pin.m_ToolTipTag;
						}
					}
				}
				if (m_MouseToolTip != null && m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.controllers != null && m_Gamer.m_RewiredPlayer.controllers.Mouse != null)
				{
					Vector2 localPoint = Vector2.zero;
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(t17Image.rectTransform, m_Gamer.m_RewiredPlayer.controllers.Mouse.screenPosition, null, out localPoint) && t17Image.rectTransform.rect.Contains(localPoint) && !string.IsNullOrEmpty(pin.m_ToolTipTag) && null != m_MouseToolTip.m_Description)
					{
						m_MouseToolTip.gameObject.SetActive(value: true);
						m_MouseToolTip.transform.position = m_Gamer.m_RewiredPlayer.controllers.Mouse.screenPosition + m_MouseToolTipOffset;
						if (pin.m_LocaliseToolTipTag)
						{
							m_MouseToolTip.m_Description.m_bNeedsLocalization = true;
							m_MouseToolTip.m_Description.SetLocalisedTextCatchAll(pin.m_ToolTipTag);
						}
						else
						{
							m_MouseToolTip.m_Description.m_bNeedsLocalization = false;
							m_MouseToolTip.m_Description.m_LocalizationTag = null;
							m_MouseToolTip.m_Description.text = pin.m_ToolTipTag;
						}
					}
				}
				if (pin.m_Directional && null != pin.m_Target)
				{
					Character component2 = pin.m_Target.GetComponent<Character>();
					if (component2 != null)
					{
						eulerAngles.z = (int)component2.m_x8FacingDirection * 45;
						if (pin.m_FloorTrackable)
						{
							Vector3 position = component.DownArrow.transform.position;
							Vector3 position2 = component.UpArrow.transform.position;
							t17Image.transform.eulerAngles = eulerAngles;
							component.DownArrow.transform.rotation = Quaternion.identity;
							component.UpArrow.transform.rotation = Quaternion.identity;
							component.DownArrow.transform.position = position;
							component.UpArrow.transform.position = position2;
						}
						else
						{
							t17Image.transform.eulerAngles = eulerAngles;
						}
					}
				}
				else
				{
					t17Image.transform.eulerAngles = Vector3.zero;
				}
			}
			else
			{
				if (!(t17Image != null))
				{
					continue;
				}
				if (pin.m_FloorTrackable)
				{
					IconData component3 = t17Image.GetComponent<IconData>();
					if (component3 != null)
					{
						component3.SetActive(activeUp: false, activeDown: false);
						component3.DownArrow.transform.rotation = Quaternion.identity;
						component3.UpArrow.transform.rotation = Quaternion.identity;
					}
				}
				m_IconPool.FreeObject(t17Image.gameObject);
				t17Image = null;
				playerIcons2.m_MainMapIconPool = null;
				playerIcons2.m_MainMapIcon = null;
			}
		}
		if (null != m_ToolTip && null != m_ToolTip.gameObject)
		{
			if (m_ToolTip.gameObject.activeInHierarchy)
			{
				if (!flag)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_On, base.gameObject);
				}
			}
			else if (flag)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_Off, base.gameObject);
			}
		}
		if (!(null != m_MouseToolTip) || !(null != m_MouseToolTip.gameObject))
		{
			return;
		}
		if (m_MouseToolTip.gameObject.activeInHierarchy)
		{
			if (!flag2)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_On, base.gameObject);
			}
		}
		else if (flag2)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_Off, base.gameObject);
		}
	}

	private void SetKeyFilterElements()
	{
		for (int i = 0; i < m_Filter.KeyFilterMasks.Count; i++)
		{
			if (m_Filter.KeyFilterMasks[i].KeyElement != null)
			{
				bool active = m_Filter.KeyFilterMasks[i].FilterMask[(int)m_ActiveFilterType];
				m_Filter.KeyFilterMasks[i].KeyElement.gameObject.SetActive(active);
			}
		}
	}

	private void HandleFloors()
	{
		if (m_FloorMan == null)
		{
			return;
		}
		List<FloorManager.Floor> validFloors = m_FloorMan.GetValidFloors();
		int num = m_Floor.m_FloorIndex - FloorButtons.Length / 2;
		if (num < 0)
		{
			num = 0;
		}
		if (FloorButtons == null)
		{
			return;
		}
		if (num + FloorButtons.Length > validFloors.Count)
		{
			num--;
		}
		for (int i = 0; i < FloorButtons.Length; i++)
		{
			if (!(FloorButtons[i] == null))
			{
				Localization.Get("Text.MapFloorname." + validFloors[num + i].m_FloorType.ToString() + validFloors[num + i].m_FloorUINumber, out var localized);
				FloorButtons[i].SetText(localized);
				FloorButtons[i].m_bPlaySound = false;
				m_ButtonFloors[i] = num + i;
			}
		}
		if (T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return;
		}
		T17EventSystemsManager instance = T17EventSystemsManager.Instance;
		if (instance != null)
		{
			T17EventSystem eventSystemForGamer = instance.GetEventSystemForGamer(m_Gamer);
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(null);
				eventSystemForGamer.SetSelectedGameObject(FloorButtons[m_Floor.m_FloorIndex - num].gameObject);
			}
		}
	}

	private GameObject GetIconObject(PinManager.Pin.PinFilterType pinLayer, bool bPlayerPin)
	{
		GameObject @object = m_IconPool.GetObject();
		if (@object != null)
		{
			GameObject gameObject = null;
			switch (pinLayer)
			{
			case PinManager.Pin.PinFilterType.All:
				gameObject = m_IconPool.gameObject;
				break;
			case PinManager.Pin.PinFilterType.Characters:
				gameObject = ((!bPlayerPin) ? m_CharacterLayerObject : m_PlayerLayerObject);
				break;
			case PinManager.Pin.PinFilterType.Shops:
				gameObject = m_ShopLayerObject;
				break;
			case PinManager.Pin.PinFilterType.Favours:
				gameObject = m_FavoursLayerObject;
				break;
			case PinManager.Pin.PinFilterType.Objectives:
				gameObject = m_ObjectiveLayerObject;
				break;
			case PinManager.Pin.PinFilterType.Tags:
				gameObject = m_TagLayerObject;
				break;
			}
			@object.name = "icon " + pinLayer;
			if (gameObject != null)
			{
				Debug.LogFormat("GetIconObject {0} {1} parent {2}", pinLayer, @object, gameObject.name);
				@object.transform.SetParent(gameObject.transform, worldPositionStays: true);
			}
			else
			{
				Debug.LogFormat("GetIconObject {0} {1} parent null", pinLayer, @object);
			}
		}
		return @object;
	}

	public void UpAFloor()
	{
		FloorManager.Floor floor = m_Floor;
		m_Floor = m_FloorMan.UpAFloor(m_Floor);
		if (floor != m_Floor)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_FloorUp, base.gameObject);
		}
	}

	public void DownAFloor()
	{
		FloorManager.Floor floor = m_Floor;
		m_Floor = m_FloorMan.DownAFloor(m_Floor);
		if (floor != m_Floor)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_FloorDoor, base.gameObject);
		}
	}

	public void MoveToFloor(int iButtonIndex)
	{
		FloorManager.Floor floor = m_FloorMan.FindFloorbyIndex(m_ButtonFloors[iButtonIndex]);
		if (floor != null && floor != m_Floor)
		{
			m_Floor = floor;
		}
	}
}

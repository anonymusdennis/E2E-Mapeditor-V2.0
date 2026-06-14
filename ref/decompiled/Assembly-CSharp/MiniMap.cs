using UnityEngine;

public class MiniMap : MonoBehaviour
{
	public Transform m_Target;

	public Player m_player;

	public T17RawImage mapTexture;

	[Range(1f, 10f)]
	public float m_MapZoom = 4f;

	private FloorManager m_FloorMan;

	private FloorManager.Floor m_LastFloor;

	private PinManager m_PinMan;

	private IconPool m_IconPool;

	public Vector3 m_ScaleOfIcons = new Vector3(2f, 2f, 1f);

	public Vector3[] m_FloorIconPos16x16 = new Vector3[2]
	{
		new Vector3(5.41f, 5.5f, 9.09f),
		new Vector3(13.44f, 2.55f, 9.09f)
	};

	public Vector3[] m_FloorIconPos32x32 = new Vector3[2]
	{
		new Vector3(9f, 10f, 9.09f),
		new Vector3(25f, 12f, 9.09f)
	};

	public GameObject m_PlayerLayerObject;

	public GameObject m_CharacterLayerObject;

	public GameObject m_ShopLayerObject;

	public GameObject m_FavoursLayerObject;

	public GameObject m_ObjectiveLayerObject;

	public GameObject m_TagLayerObject;

	private float m_ImageAspectRatio = 1.7777778f;

	private void Start()
	{
		m_FloorMan = FloorManager.GetInstance();
		m_PinMan = PinManager.GetInstance();
		m_IconPool = base.transform.parent.GetComponentInChildren<IconPool>();
		mapTexture = GetComponent<T17RawImage>();
		if (mapTexture != null)
		{
			float width = mapTexture.rectTransform.rect.width;
			float height = mapTexture.rectTransform.rect.height;
			m_ImageAspectRatio = width / height;
		}
		m_IconPool.FreeAllObjects();
		for (int i = 0; i < base.transform.parent.childCount; i++)
		{
			Transform child = base.transform.parent.GetChild(i);
			if (child != null)
			{
				if (child.name == "PlayerLayerObject")
				{
					m_PlayerLayerObject = child.gameObject;
				}
				else if (child.name == "CharactersLayerObject")
				{
					m_CharacterLayerObject = child.gameObject;
				}
				else if (child.name == "ShopLayerObject")
				{
					m_ShopLayerObject = child.gameObject;
				}
				else if (child.name == "FavoursLayerObject")
				{
					m_FavoursLayerObject = child.gameObject;
				}
				else if (child.name == "ObjectiveLayerObject")
				{
					m_ObjectiveLayerObject = child.gameObject;
				}
				else if (child.name == "TagLayerObject")
				{
					m_TagLayerObject = child.gameObject;
				}
			}
		}
	}

	protected virtual void OnDestroy()
	{
		m_FloorMan = null;
		m_PinMan = null;
		mapTexture.texture = null;
	}

	private void Update()
	{
		if (!(m_Target != null) || !(m_player != null) || m_player.m_Gamer == null || m_player.m_Gamer.m_NetViewID != m_PinMan.m_MapUpdate_NetViewID)
		{
			return;
		}
		int maxRows = 1;
		int maxColumns = 1;
		float num = 1f / m_MapZoom * m_ImageAspectRatio;
		float num2 = 1f / m_MapZoom;
		if (m_player != null)
		{
			if (m_LastFloor != m_player.CurrentFloor && m_player.CurrentFloor != null && m_player.CurrentFloor.m_MapTexture != null)
			{
				mapTexture.texture = m_player.CurrentFloor.m_MapTexture;
			}
			m_LastFloor = m_player.CurrentFloor;
			if (m_FloorMan != null)
			{
				m_FloorMan.GetTileSystemBounds(m_player.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, out maxRows, out maxColumns);
				if (mapTexture != null)
				{
					float num3 = (float)maxColumns / (float)maxRows;
					float x = (m_Target.position.x + (float)(maxColumns / 2)) / (float)maxColumns - num / 2f;
					float y = (m_Target.position.y + (float)(maxRows / 2)) / (float)maxRows - num2 * num3 / 2f;
					mapTexture.uvRect = new Rect(x, y, num, num2 * num3);
				}
			}
		}
		if (null != mapTexture)
		{
			UpdatePins(mapTexture.uvRect);
		}
	}

	private void UpdatePins(Rect mapRect)
	{
		Vector3 localPosition = new Vector3(0f, 0f, 0f);
		Vector3 eulerAngles = new Vector3(0f, 0f, 0f);
		Vector2 vector = new Vector2(mapTexture.rectTransform.rect.width / mapRect.width, mapTexture.rectTransform.rect.height / mapRect.height);
		Vector2 vector2 = new Vector2(mapTexture.rectTransform.rect.width * 0.5f, mapTexture.rectTransform.rect.height * 0.5f);
		for (int i = 0; i < m_PinMan.m_MiniMapPins.Count; i++)
		{
			PinManager.Pin pin = m_PinMan.m_MiniMapPins[i];
			if (pin.m_PlayerIcons.Count <= 0 || m_player.m_Gamer == null || !pin.m_PlayerIcons.ContainsKey(m_player.m_Gamer.m_NetViewID))
			{
				continue;
			}
			PinManager.Pin.PlayerIcons playerIcons = pin.m_PlayerIcons[m_player.m_Gamer.m_NetViewID];
			if (pin.m_Floor != m_LastFloor)
			{
				if (!pin.m_FloorTrackable)
				{
					if (playerIcons.m_MiniMapIcon != null)
					{
						m_IconPool.FreeObject(playerIcons.m_MiniMapIcon.gameObject);
						playerIcons.m_MiniMapIcon = null;
						playerIcons.m_MiniMapIconPool = null;
					}
					continue;
				}
			}
			else if (pin.m_FloorTrackable && playerIcons.m_MiniMapIcon != null)
			{
				IconData component = playerIcons.m_MiniMapIcon.GetComponent<IconData>();
				component.SetActive(activeUp: false, activeDown: false);
			}
			Vector2 mapPos = pin.m_MapPos;
			if (mapRect.Contains(mapPos) || pin.m_Edgable)
			{
				if (playerIcons.m_MiniMapIcon == null)
				{
					GameObject iconObject = GetIconObject(pin.m_FilterType, pin.m_bIsPlayer);
					if (iconObject != null)
					{
						playerIcons.m_MiniMapIcon = iconObject.GetComponent<T17Image>();
					}
					if (!pin.m_bOverrideIconScale)
					{
						playerIcons.m_MiniMapIcon.transform.localScale = m_ScaleOfIcons;
					}
					else
					{
						playerIcons.m_MiniMapIcon.transform.localScale = pin.m_OverrideIconScale;
					}
					playerIcons.m_MiniMapIconPool = m_IconPool;
					playerIcons.m_MiniMapIcon.transform.SetSiblingIndex(Mathf.RoundToInt((float)playerIcons.m_MiniMapIcon.transform.parent.childCount * m_PinMan.GetIconPriorityMultiplier(pin.m_FilterType)));
				}
				playerIcons.m_MiniMapIcon.sprite = pin.m_IconSprite;
				IconData component2 = playerIcons.m_MiniMapIcon.GetComponent<IconData>();
				if (pin.m_IconSprite != null)
				{
					Vector2 vector3 = new Vector2(pin.m_IconSprite.rect.width, pin.m_IconSprite.rect.height);
					if (playerIcons.m_MiniMapIcon.rectTransform.sizeDelta != vector3)
					{
						playerIcons.m_MiniMapIcon.rectTransform.sizeDelta = vector3;
						if (vector3.x == 16f)
						{
							if (component2.UpArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos16x16[0])
							{
								component2.UpArrow.rectTransform.anchoredPosition = m_FloorIconPos16x16[0];
							}
							if (component2.DownArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos16x16[1])
							{
								component2.DownArrow.rectTransform.anchoredPosition = m_FloorIconPos16x16[1];
							}
						}
						else if (vector3.x == 32f)
						{
							if (component2.UpArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos32x32[0])
							{
								component2.UpArrow.rectTransform.anchoredPosition = m_FloorIconPos32x32[0];
							}
							if (component2.DownArrow.rectTransform.anchoredPosition != (Vector2)m_FloorIconPos32x32[1])
							{
								component2.DownArrow.rectTransform.anchoredPosition = m_FloorIconPos32x32[1];
							}
						}
					}
				}
				if (pin.m_FloorTrackable)
				{
					if (pin.m_Floor.m_FloorIndex > m_LastFloor.m_FloorIndex)
					{
						component2.SetActive(activeUp: true, activeDown: false);
					}
					else if (pin.m_Floor.m_FloorIndex < m_LastFloor.m_FloorIndex)
					{
						component2.SetActive(activeUp: false, activeDown: true);
					}
				}
				else
				{
					component2.SetActive(activeUp: false, activeDown: false);
					component2.DownArrow.transform.rotation = Quaternion.identity;
					component2.UpArrow.transform.rotation = Quaternion.identity;
				}
				mapPos.x = Mathf.Clamp(mapPos.x, mapRect.xMin, mapRect.xMax);
				mapPos.y = Mathf.Clamp(mapPos.y, mapRect.yMin, mapRect.yMax);
				mapPos.x -= mapRect.xMin;
				mapPos.y -= mapRect.yMin;
				localPosition.x = mapPos.x * vector.x - vector2.x;
				localPosition.y = mapPos.y * vector.y - vector2.y;
				localPosition.z = playerIcons.m_MiniMapIcon.transform.localPosition.z;
				playerIcons.m_MiniMapIcon.transform.localPosition = localPosition;
				if (pin.m_Directional && null != pin.m_Target)
				{
					Character component3 = pin.m_Target.GetComponent<Character>();
					if (component3 != null)
					{
						eulerAngles.z = (int)component3.m_x8FacingDirection * 45;
						if (pin.m_FloorTrackable)
						{
							Vector3 position = component2.DownArrow.transform.position;
							Vector3 position2 = component2.UpArrow.transform.position;
							playerIcons.m_MiniMapIcon.transform.eulerAngles = eulerAngles;
							component2.DownArrow.transform.rotation = Quaternion.identity;
							component2.UpArrow.transform.rotation = Quaternion.identity;
							component2.DownArrow.transform.position = position;
							component2.UpArrow.transform.position = position2;
						}
						else
						{
							playerIcons.m_MiniMapIcon.transform.eulerAngles = eulerAngles;
						}
					}
				}
				else
				{
					playerIcons.m_MiniMapIcon.transform.eulerAngles = Vector3.zero;
				}
			}
			else if (playerIcons.m_MiniMapIcon != null)
			{
				if (pin.m_FloorTrackable)
				{
					IconData component4 = playerIcons.m_MiniMapIcon.GetComponent<IconData>();
					component4.SetActive(activeUp: false, activeDown: false);
					component4.DownArrow.transform.rotation = Quaternion.identity;
					component4.UpArrow.transform.rotation = Quaternion.identity;
				}
				m_IconPool.FreeObject(playerIcons.m_MiniMapIcon.gameObject);
				playerIcons.m_MiniMapIcon = null;
				playerIcons.m_MiniMapIconPool = null;
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
			if (gameObject != null)
			{
				@object.transform.SetParent(gameObject.transform, worldPositionStays: true);
			}
		}
		return @object;
	}

	public void OnMinimapButtonClicked(T17Button buttonClicked)
	{
		if (m_player != null)
		{
			m_player.RequestMapOpen();
			HUDMenuFlow.Instance.RemoveMouseHUDItem(m_player.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}

	public void OnPointerEnter(T17Button sender)
	{
		if (m_player != null)
		{
			HUDMenuFlow.Instance.AddMouseHUDItem(m_player.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}

	public void OnPointerExit(T17Button sender)
	{
		if (m_player != null)
		{
			HUDMenuFlow.Instance.RemoveMouseHUDItem(m_player.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}
}

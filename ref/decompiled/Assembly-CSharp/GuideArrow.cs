using UnityEngine;

public class GuideArrow : MonoBehaviour, IControlledUpdate
{
	private Transform m_TargetTransform;

	private Vector3 m_TargetPos = Vector3.zero;

	private RoomBlob m_TargetRoom;

	public ArrowManager.ArrowType m_Type;

	[HideInInspector]
	public int m_TargetFloorIndex = -1;

	[HideInInspector]
	public bool m_bShowOnscreenIndicator = true;

	public T17Image m_ArrowImage;

	public Sprite m_OffscreenArrowSprite;

	public Sprite m_OnscreenArrowSprite;

	public GameObject m_AnimatedLocationIndicator;

	public GameObject m_AnimatedTileIndicator;

	[HideInInspector]
	public bool m_bShowChangeFloorIndicator;

	public GameObject m_ChangeFloorIndicator;

	public float m_ChangeFloorIndicatorDistance = 0.1f;

	[HideInInspector]
	public bool m_bHideArrowChangeFloor;

	public float m_ChangeFloorHideArrowDistance = 0.1f;

	public Transform m_ArrowImageParent;

	public T17Image m_FloorIndicator;

	public Sprite[] m_FloorIndicatorSprites;

	public RectTransform m_SafeSpace;

	public RectTransform m_ArrowRect;

	public int m_TargetCameraIndex;

	private CameraManager.CameraBinding m_CameraBinding;

	public float m_ArrowMaxOnScreenDistance = 0.25f;

	public float m_ArrowDisableDistance = 0.3f;

	private bool m_ArrowOffscreen = true;

	private float m_ArrowMinViewportPos;

	private float m_ArrowMaxViewportPos;

	[Range(-1f, 1f)]
	public float m_arrowXOffset;

	[Range(-1f, 1f)]
	public float m_arrowYOffset = 0.04f;

	[HideInInspector]
	public int m_ID = -1;

	public Transform TargetTransform
	{
		get
		{
			return m_TargetTransform;
		}
		set
		{
			m_TargetTransform = value;
		}
	}

	public Vector3 TargetPos
	{
		get
		{
			return m_TargetPos;
		}
		set
		{
			m_TargetPos = value;
		}
	}

	public RoomBlob TargetRoom
	{
		get
		{
			return m_TargetRoom;
		}
		set
		{
			m_TargetRoom = value;
		}
	}

	private void Start()
	{
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Register(this, UpdateCategory.RapidPeriodic);
		}
		m_ArrowMinViewportPos = 0.5f - m_ArrowMaxOnScreenDistance;
		m_ArrowMaxViewportPos = 0.5f + m_ArrowMaxOnScreenDistance;
		if (CameraManager.GetInstance() != null)
		{
			m_CameraBinding = CameraManager.GetInstance().m_CameraBindings[m_TargetCameraIndex];
		}
	}

	protected virtual void OnDestroy()
	{
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
	}

	public void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
		if (m_CameraBinding == null)
		{
			CameraManager instance = CameraManager.GetInstance();
			if (!(instance != null) || m_TargetCameraIndex >= instance.m_CameraBindings.Length || m_TargetCameraIndex < 0)
			{
				return;
			}
			m_CameraBinding = CameraManager.GetInstance().m_CameraBindings[m_TargetCameraIndex];
		}
		if (CutsceneManagerBase.IsACutscenePlaying() || !(m_CameraBinding.m_Character != null))
		{
			return;
		}
		if (m_TargetTransform != null)
		{
			Vector3 vector = m_CameraBinding.m_Camera.WorldToViewportPoint(m_TargetTransform.position);
			float num = 1.2f;
			float num2 = 1f - 1f / num;
			float num3 = 1f / num;
			FloorManager.Floor floor = null;
			FloorManager instance2 = FloorManager.GetInstance();
			if (instance2 != null)
			{
				floor = instance2.FindFloorAtZ(m_TargetTransform.position.z);
			}
			if (floor == null)
			{
				return;
			}
			if (vector.x < num3 && vector.x > num2 && vector.y < num3 && vector.y > num2 && floor == m_CameraBinding.m_Character.CurrentFloor && m_bShowOnscreenIndicator)
			{
				if (m_FloorIndicator != null)
				{
					m_FloorIndicator.gameObject.SetActive(value: false);
				}
				if (m_ChangeFloorIndicator != null && m_ChangeFloorIndicator.activeSelf)
				{
					m_ChangeFloorIndicator.SetActive(value: false);
				}
				CalculateOnScreenPosition(vector, m_TargetTransform.position, floor, num);
			}
			else
			{
				Vector3 vector2 = m_CameraBinding.m_Camera.WorldToViewportPoint(m_CameraBinding.m_Character.transform.position);
				float num4 = Vector2.Distance(vector2, vector);
				if (num4 > m_ArrowDisableDistance)
				{
					CalculateOffScreenPosition(m_TargetTransform.position, floor);
				}
				else if (m_FloorIndicator != null)
				{
					m_FloorIndicator.gameObject.SetActive(value: false);
				}
			}
		}
		else if (m_TargetRoom != null)
		{
			if (m_CameraBinding.m_Character.m_CurrentLocation != null && m_CameraBinding.m_Character.m_CurrentLocation.location != m_TargetRoom.location)
			{
				FloorManager.Floor floor2 = m_TargetRoom.GetFloor().GetFloor();
				CalculateOffScreenPosition(m_TargetRoom.GetArrowPosition(), floor2);
				return;
			}
			if (m_ArrowImage != null)
			{
				m_ArrowImage.gameObject.SetActive(value: false);
			}
			if (m_FloorIndicator != null)
			{
				m_FloorIndicator.gameObject.SetActive(value: false);
			}
			if (m_ChangeFloorIndicator != null)
			{
				m_ChangeFloorIndicator.gameObject.SetActive(value: false);
			}
		}
		else if (m_TargetPos != Vector3.zero)
		{
			Vector3 targetViewportPoint = m_CameraBinding.m_Camera.WorldToViewportPoint(m_TargetPos);
			float num5 = 1.2f;
			float num6 = 1f - 1f / num5;
			float num7 = 1f / num5;
			FloorManager.Floor floor3 = null;
			FloorManager instance3 = FloorManager.GetInstance();
			if (instance3 != null)
			{
				floor3 = instance3.FindFloorAtZ(m_TargetPos.z);
			}
			if (floor3 != null)
			{
				bool flag = false;
				if (floor3 == m_CameraBinding.m_Character.CurrentFloor && m_bShowOnscreenIndicator)
				{
					if (targetViewportPoint.x < num7 && targetViewportPoint.x > num6 && targetViewportPoint.y < num7 && targetViewportPoint.y > num6)
					{
						flag = true;
					}
					else
					{
						Vector3 localPosition = m_CameraBinding.m_Character.transform.localPosition;
						if (Mathf.Abs(localPosition.x - m_TargetPos.x) < 4f && Mathf.Abs(localPosition.y - m_TargetPos.y) < 4f)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					if (m_FloorIndicator != null)
					{
						m_FloorIndicator.gameObject.SetActive(value: false);
					}
					if (m_ChangeFloorIndicator != null && m_ChangeFloorIndicator.activeSelf)
					{
						m_ChangeFloorIndicator.SetActive(value: false);
					}
					CalculateOnScreenPosition(targetViewportPoint, m_TargetPos, floor3, num5);
				}
				else
				{
					CalculateOffScreenPosition(m_TargetPos, floor3);
				}
			}
			else
			{
				if (m_ArrowImage != null)
				{
					m_ArrowImage.gameObject.SetActive(value: false);
				}
				if (m_FloorIndicator != null)
				{
					m_FloorIndicator.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			if (m_ArrowImage != null)
			{
				m_ArrowImage.gameObject.SetActive(value: false);
			}
			if (m_FloorIndicator != null)
			{
				m_FloorIndicator.gameObject.SetActive(value: false);
			}
		}
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
		return true;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	private void CalculateOffScreenPosition(Vector3 targetWorldSpacePoint, FloorManager.Floor targetFloor)
	{
		if (m_bShowChangeFloorIndicator || m_bHideArrowChangeFloor)
		{
			Vector2 b = m_CameraBinding.m_Camera.WorldToViewportPoint(targetWorldSpacePoint);
			Vector2 a = m_CameraBinding.m_Camera.WorldToViewportPoint(m_CameraBinding.m_Character.m_CachedCurrentPosition);
			float num = Vector2.Distance(a, b);
			if (m_bShowChangeFloorIndicator)
			{
				if (num < m_ChangeFloorIndicatorDistance)
				{
					ShowChangeFloorIndicator();
					return;
				}
			}
			else if (m_bHideArrowChangeFloor && m_CameraBinding.m_Character != null && m_CameraBinding.m_Character.m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)m_CameraBinding.m_Character;
				if (player != null && player.HasValidTransitionInteraction() && num < m_ChangeFloorHideArrowDistance)
				{
					HideEverything();
					return;
				}
			}
		}
		if (m_ChangeFloorIndicator != null && m_ChangeFloorIndicator.activeSelf)
		{
			m_ChangeFloorIndicator.SetActive(value: false);
		}
		if (!m_ArrowImage.gameObject.activeSelf)
		{
			m_ArrowImage.gameObject.SetActive(value: true);
		}
		if (!m_ArrowOffscreen && m_ArrowImage != null)
		{
			m_ArrowImage.sprite = m_OffscreenArrowSprite;
		}
		m_ArrowOffscreen = true;
		Vector3 value = m_CameraBinding.m_Camera.transform.position - targetWorldSpacePoint;
		value.z = 0f;
		value = Vector3.Normalize(value);
		m_ArrowImageParent.gameObject.transform.rotation = Quaternion.LookRotation(m_ArrowImageParent.gameObject.transform.forward, value);
		if (CameraManager.GetInstance() == null || CameraManager.GetInstance().GetCameraViewportRects() == null || m_TargetCameraIndex >= CameraManager.GetInstance().GetCameraViewportRects().Length)
		{
			return;
		}
		Rect rect = CameraManager.GetInstance().GetCameraViewportRects()[m_TargetCameraIndex];
		Vector2 vector = m_CameraBinding.m_Camera.WorldToViewportPoint(targetWorldSpacePoint);
		vector.x = Mathf.Clamp(vector.x, m_ArrowMinViewportPos, m_ArrowMaxViewportPos);
		vector.y = Mathf.Clamp(vector.y, m_ArrowMinViewportPos, m_ArrowMaxViewportPos);
		float num2 = vector.x * rect.width;
		float num3 = vector.y * rect.height;
		Vector2 vector2 = new Vector2(rect.x + num2, 1f - rect.y - (rect.height - num3));
		m_ArrowRect.anchoredPosition = new Vector2(vector2.x * m_SafeSpace.rect.width, vector2.y * m_SafeSpace.rect.height);
		if (m_FloorIndicator != null)
		{
			FloorManager.Floor currentFloor = m_CameraBinding.m_Character.CurrentFloor;
			if (currentFloor != null && targetFloor != null)
			{
				if (currentFloor.m_FloorIndex != targetFloor.m_FloorIndex)
				{
					int floorUINumber = targetFloor.m_FloorUINumber;
					if (m_FloorIndicatorSprites != null && floorUINumber < m_FloorIndicatorSprites.Length && floorUINumber >= 0)
					{
						m_FloorIndicator.gameObject.SetActive(value: true);
						m_FloorIndicator.sprite = m_FloorIndicatorSprites[floorUINumber];
					}
					else
					{
						m_FloorIndicator.gameObject.SetActive(value: false);
					}
				}
				else
				{
					m_FloorIndicator.gameObject.SetActive(value: false);
					if (m_TargetFloorIndex != -1 && m_TargetFloorIndex != currentFloor.m_FloorIndex)
					{
						FloorManager instance = FloorManager.GetInstance();
						if (instance != null)
						{
							FloorManager.Floor floor = instance.FindFloorbyIndex(m_TargetFloorIndex);
							if (floor != null)
							{
								int floorUINumber2 = floor.m_FloorUINumber;
								if (m_FloorIndicatorSprites != null && floorUINumber2 < m_FloorIndicatorSprites.Length && floorUINumber2 >= 0)
								{
									m_FloorIndicator.gameObject.SetActive(value: true);
									m_FloorIndicator.sprite = m_FloorIndicatorSprites[floorUINumber2];
								}
							}
						}
					}
				}
			}
		}
		if (m_AnimatedLocationIndicator != null)
		{
			m_AnimatedLocationIndicator.SetActive(value: false);
		}
		if (m_AnimatedTileIndicator != null)
		{
			m_AnimatedTileIndicator.SetActive(value: false);
		}
	}

	private void CalculateOnScreenPosition(Vector3 targetViewportPoint, Vector3 targetWorldSpacePoint, FloorManager.Floor targetFloor, float overscanFactor)
	{
		if (m_ArrowOffscreen || m_ArrowImage.sprite != m_OnscreenArrowSprite)
		{
			m_ArrowImageParent.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
			if (m_ArrowImage != null)
			{
				if (m_OnscreenArrowSprite != null)
				{
					m_ArrowImage.sprite = m_OnscreenArrowSprite;
					m_ArrowImage.gameObject.SetActive(value: true);
				}
				else
				{
					m_ArrowImage.gameObject.SetActive(value: false);
				}
			}
		}
		m_ArrowOffscreen = false;
		HUDMenuFlow instance = HUDMenuFlow.Instance;
		if (instance != null)
		{
			Vector2 vector = CalcScreenPos(targetViewportPoint, overscanFactor);
			m_ArrowRect.anchoredPosition = new Vector2(vector.x * m_SafeSpace.rect.width, vector.y * m_SafeSpace.rect.height);
			m_ArrowRect.anchoredPosition += new Vector2((float)Screen.width * m_arrowXOffset, (float)Screen.height * m_arrowYOffset);
		}
		if (!(m_OnscreenArrowSprite == null) || !m_bShowOnscreenIndicator)
		{
			return;
		}
		if (m_TargetTransform != null || m_TargetPos != Vector3.zero)
		{
			if (m_AnimatedTileIndicator != null)
			{
				m_AnimatedTileIndicator.SetActive(value: true);
			}
		}
		else if (m_AnimatedLocationIndicator != null)
		{
			m_AnimatedLocationIndicator.SetActive(value: true);
		}
	}

	private Vector3 CalcScreenPos(Vector3 targetViewportPoint, float overscanFactor)
	{
		HUDMenuFlow instance = HUDMenuFlow.Instance;
		if (instance != null)
		{
			Rect rect = CameraManager.GetInstance().GetCameraViewportRects()[m_TargetCameraIndex];
			float num = overscanFactor - 1f;
			float num2 = overscanFactor - 1f;
			Vector3 vector = targetViewportPoint - new Vector3(0.5f, 0.5f, 0f);
			targetViewportPoint.x += vector.x * num;
			targetViewportPoint.y += vector.y * num2;
			float num3 = targetViewportPoint.x * rect.width;
			float num4 = targetViewportPoint.y * rect.height;
			Vector2 vector2 = new Vector2(rect.x + num3, 1f - rect.y - (rect.height - num4));
			return vector2;
		}
		return Vector3.zero;
	}

	private void ShowChangeFloorIndicator()
	{
		if (m_ArrowImage.gameObject.activeSelf)
		{
			m_ArrowImage.gameObject.SetActive(value: false);
		}
		if (m_ChangeFloorIndicator != null && !m_ChangeFloorIndicator.activeSelf)
		{
			m_ChangeFloorIndicator.SetActive(value: true);
			m_ArrowImageParent.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
		}
		if (m_AnimatedLocationIndicator != null && m_AnimatedLocationIndicator.gameObject.activeSelf)
		{
			m_AnimatedLocationIndicator.SetActive(value: false);
		}
		if (m_AnimatedTileIndicator != null && m_AnimatedTileIndicator.gameObject.activeSelf)
		{
			m_AnimatedTileIndicator.SetActive(value: false);
		}
		if (m_FloorIndicator != null && m_FloorIndicator.gameObject.activeSelf)
		{
			m_FloorIndicator.gameObject.SetActive(value: false);
		}
		Vector3 targetViewportPoint = m_CameraBinding.m_Camera.WorldToViewportPoint(m_TargetPos);
		Vector3 vector = CalcScreenPos(targetViewportPoint, 1.2f);
		m_ArrowRect.anchoredPosition = new Vector2(vector.x * m_SafeSpace.rect.width, vector.y * m_SafeSpace.rect.height);
		m_ArrowOffscreen = false;
	}

	public void SetChangeFloorIndicator(Sprite theSprite)
	{
		if (m_ChangeFloorIndicator != null)
		{
			T17Image componentInChildren = m_ChangeFloorIndicator.GetComponentInChildren<T17Image>();
			if (componentInChildren != null)
			{
				componentInChildren.sprite = theSprite;
			}
		}
	}

	private void HideEverything()
	{
		if (m_ArrowImage.gameObject.activeSelf)
		{
			m_ArrowImage.gameObject.SetActive(value: false);
		}
		if (m_AnimatedLocationIndicator != null && m_AnimatedLocationIndicator.gameObject.activeSelf)
		{
			m_AnimatedLocationIndicator.SetActive(value: false);
		}
		if (m_AnimatedTileIndicator != null && m_AnimatedTileIndicator.gameObject.activeSelf)
		{
			m_AnimatedTileIndicator.SetActive(value: false);
		}
		if (m_FloorIndicator != null && m_FloorIndicator.gameObject.activeSelf)
		{
			m_FloorIndicator.gameObject.SetActive(value: false);
		}
		if (m_ChangeFloorIndicator != null && m_ChangeFloorIndicator.gameObject.activeSelf)
		{
			m_ChangeFloorIndicator.SetActive(value: false);
		}
	}
}

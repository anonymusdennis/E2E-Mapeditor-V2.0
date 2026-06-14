using System;
using UnityEngine;

public class LevelEditor_ZoneIconControl : MonoBehaviour
{
	[Serializable]
	public class ModeData
	{
		public bool m_FlashMenuIcon;

		public bool m_DisplayMenu;

		public bool m_OverButton;

		public string m_BorderType = "UNKNOWN";

		public Vector3 m_MinOffset = Vector3.zero;

		public Vector3 m_MaxOffset = Vector3.zero;

		public Vector3 m_PivotOffset = Vector3.zero;

		public AnimationCurve m_IconAlpha = new AnimationCurve();

		public AnimationCurve m_BackerAlpha = new AnimationCurve();

		public string m_BorderColorValid = "valid";

		public string m_BorderColorInvalid = "invalid";

		public bool m_ApplyZoomToScale = true;

		public float m_BorderZOffset;
	}

	public enum Mode
	{
		NotOverZone,
		MouseOverZone,
		MouseOverButton,
		ZoneSelected,
		ZoneSelectedOverButton,
		EditingTheZone,
		TOTAL,
		INVALID
	}

	public enum IconDisplayed
	{
		INVALID,
		NormalZone,
		InvalidZone,
		Menu,
		MenuInvalid
	}

	public enum OverIconEnum
	{
		NotNear,
		Near,
		Over
	}

	private LevelEditor_ZoneManager.Zone m_MyZone;

	private bool m_LastValidState = true;

	private float m_LastZoomLevel;

	private IconDisplayed m_IconDisplayed;

	private LevelEditor_Controller m_Controller;

	private LevelEditor_Cursor m_Cursor;

	private float m_TimePassed;

	private float m_TransitionTime;

	private bool m_CurrentlyDisplayingMenu;

	private bool m_CurrentlyOverMenu;

	private bool m_CurrentlyOverButton;

	private static float c_ScreenHeight;

	private static float c_ScreenScale;

	private static float c_ScreenOrth;

	private bool m_bUpdateBorder;

	private bool m_bUpdateMenu;

	private bool m_bUpdateZ;

	[NonSerialized]
	public GameObject m_MyGameObject;

	[NonSerialized]
	public Transform m_MyTransform;

	[Tooltip("The Meshrenderer we will be controlling")]
	public MeshRenderer m_ZoneImageMat;

	[Tooltip("The Meshrenderer we will be controlling")]
	public MeshRenderer m_ZoneBackerMat;

	[Tooltip("The graphic to show when showing the Menu Icon")]
	public Material m_MenuMaterial;

	[Tooltip("The graphic to show when showing the Menu Icon when the menu is invalid")]
	public Material m_MenuMaterialInvalid;

	public ZoneIconBackerData m_ZoneIconBackerData;

	[Tooltip("How fast it changes from one mode to another")]
	public float m_TransitionSpeed = 0.5f;

	[Tooltip("How long the Zone icon is displayed before changing to the menu icon")]
	public float m_ZoneIconTime = 1f;

	[Tooltip("How long the mein icon is displayed before changing to the zone icon")]
	public float m_MenuIconTime = 0.25f;

	public float m_fZoomedInHotSpot = 1.5f;

	public float m_fZoomedOutHotSpot = 1.05f;

	private Vector3 m_StandardPosition = Vector3.zero;

	private bool m_bInitialized;

	public float m_MinScale = 1f;

	public float m_MaxScale = 1f;

	private bool m_Show = true;

	[HideInInspector]
	public ModeData[] m_ModeData = new ModeData[6];

	private Mode m_CurrentMode;

	private Mode m_TargetMode = Mode.INVALID;

	private float m_fIconCurrentAlpha = 1f;

	private float m_fBackerCurrentAlpha = 1f;

	private void Start()
	{
		m_bInitialized = Initialize();
	}

	private void OnDisable()
	{
		if (m_CurrentlyOverButton)
		{
			if (m_Cursor != null)
			{
				m_Cursor.LeavingControlArea();
			}
			m_CurrentlyOverButton = false;
		}
	}

	private void Update()
	{
		if (m_bInitialized || (m_bInitialized = Initialize()))
		{
			UpdateMode();
		}
	}

	private bool Initialize()
	{
		bool flag = true;
		if (m_Controller == null && (m_Controller = LevelEditor_Controller.GetInstance()) == null)
		{
			flag = false;
		}
		if (m_Cursor == null && (m_Cursor = LevelEditor_Cursor.GetInstance()) == null)
		{
			flag = false;
		}
		if (m_ZoneImageMat == null)
		{
			flag = false;
		}
		m_ZoneImageMat.enabled = m_Show;
		if (m_ZoneBackerMat == null)
		{
			flag = false;
		}
		m_ZoneBackerMat.enabled = m_Show;
		if (m_MenuMaterial == null)
		{
			flag = false;
		}
		if (m_MenuMaterialInvalid == null)
		{
			flag = false;
		}
		if (flag)
		{
			SetUpIcon();
		}
		return flag;
	}

	public void SetMode(Mode newMode, bool bForce = false)
	{
		if (m_TargetMode != newMode || bForce)
		{
			if (m_TargetMode != Mode.INVALID)
			{
				m_CurrentMode = m_TargetMode;
				m_TargetMode = newMode;
				m_TransitionTime = 1f - m_TransitionTime;
				m_bUpdateBorder = true;
				m_bUpdateMenu = true;
			}
			else if (m_CurrentMode != newMode || bForce)
			{
				m_TargetMode = newMode;
				m_TransitionTime = 1f;
				m_bUpdateBorder = true;
				m_bUpdateMenu = true;
			}
		}
	}

	private void UpdateMode()
	{
		bool flag = m_LastZoomLevel != (m_LastZoomLevel = m_Controller.GetZoomPerc());
		bool flag2 = m_LastValidState != (m_LastValidState = m_MyZone.IsFullyValid());
		if (m_TargetMode != Mode.INVALID)
		{
			m_TransitionTime -= 1f / m_TransitionSpeed * Time.deltaTime;
			if (m_TransitionTime < 0f)
			{
				m_TransitionTime = 0f;
			}
			UpdatePosition();
			if (m_TransitionTime == 0f)
			{
				m_CurrentMode = m_TargetMode;
				m_TargetMode = Mode.INVALID;
				UpdateIcon();
				UpdateAlpha();
				UpdatePosition();
				m_bUpdateBorder = true;
				if (m_CurrentlyOverButton != m_ModeData[(int)m_CurrentMode].m_OverButton && m_Cursor != null)
				{
					if (m_ModeData[(int)m_CurrentMode].m_OverButton)
					{
						m_Cursor.EnteringControlArea();
					}
					else
					{
						m_Cursor.LeavingControlArea();
					}
					m_CurrentlyOverButton = m_ModeData[(int)m_CurrentMode].m_OverButton;
				}
			}
			else
			{
				if (UpdateIcon())
				{
					m_fIconCurrentAlpha = -1f;
					m_fBackerCurrentAlpha = -1f;
				}
				UpdateAlpha();
			}
		}
		else
		{
			if (flag)
			{
				UpdatePosition();
			}
			if ((flag2 || m_ModeData[(int)m_CurrentMode].m_FlashMenuIcon) && UpdateIcon())
			{
				m_fIconCurrentAlpha = -1f;
				m_fBackerCurrentAlpha = -1f;
				UpdateAlpha();
			}
		}
		if (flag)
		{
			UpdateAlpha();
		}
		if (m_bUpdateBorder)
		{
			m_bUpdateBorder = false;
			UpdateBorderType();
			UpdateBorderColor();
		}
		if (m_bUpdateMenu)
		{
			m_bUpdateMenu = false;
			UpdateMenu();
		}
		if (m_bUpdateZ)
		{
			m_bUpdateZ = !UpdateBorderZ();
		}
	}

	private void SetUpIcon()
	{
		m_LastValidState = m_MyZone.IsFullyValid();
		m_LastZoomLevel = m_Controller.GetZoomPerc();
		UpdateIcon();
		UpdateAlpha(bForceUpdate: true);
		UpdatePosition(bForceUpdate: true);
		UpdateBorderType();
		UpdateBorderColor();
		UpdateBorderZ();
	}

	private bool UpdateIcon()
	{
		m_TimePassed += Time.deltaTime;
		while (m_TimePassed > m_MenuIconTime + m_ZoneIconTime)
		{
			m_TimePassed -= m_MenuIconTime + m_ZoneIconTime;
		}
		IconDisplayed iconDisplayed = IconDisplayed.INVALID;
		if (m_ModeData[(int)m_CurrentMode].m_FlashMenuIcon && m_TimePassed > m_ZoneIconTime)
		{
			iconDisplayed = (m_LastValidState ? IconDisplayed.Menu : IconDisplayed.MenuInvalid);
		}
		if (iconDisplayed == IconDisplayed.INVALID)
		{
			iconDisplayed = (m_LastValidState ? IconDisplayed.NormalZone : IconDisplayed.InvalidZone);
		}
		if (iconDisplayed != m_IconDisplayed)
		{
			m_IconDisplayed = iconDisplayed;
			UpdateBorderColor();
			switch (m_IconDisplayed)
			{
			case IconDisplayed.InvalidZone:
				m_ZoneImageMat.material = m_MyZone.m_ZoneDetails.m_ZoneImageInvalid;
				m_ZoneBackerMat.material = m_ZoneIconBackerData.ZoneIconBacker_Invalid;
				break;
			case IconDisplayed.NormalZone:
				m_ZoneImageMat.material = m_MyZone.m_ZoneDetails.m_ZoneImage;
				m_ZoneBackerMat.material = m_ZoneIconBackerData.ZoneIconBacker_Valid;
				break;
			case IconDisplayed.Menu:
				m_ZoneImageMat.material = m_MenuMaterial;
				m_ZoneBackerMat.material = m_ZoneIconBackerData.ZoneIconBacker_Valid;
				break;
			case IconDisplayed.MenuInvalid:
				m_ZoneImageMat.material = m_MenuMaterialInvalid;
				m_ZoneBackerMat.material = m_ZoneIconBackerData.ZoneIconBacker_Invalid;
				break;
			}
			return true;
		}
		return false;
	}

	private void UpdateAlpha(bool bForceUpdate = false)
	{
		float zoomPerc = m_Controller.GetZoomPerc();
		float num = 1f;
		float num2 = 1f;
		if (m_TargetMode == Mode.INVALID)
		{
			num = m_ModeData[(int)m_CurrentMode].m_IconAlpha.Evaluate(zoomPerc);
			num2 = m_ModeData[(int)m_CurrentMode].m_BackerAlpha.Evaluate(zoomPerc);
		}
		else
		{
			float b = m_ModeData[(int)m_CurrentMode].m_IconAlpha.Evaluate(zoomPerc);
			float a = m_ModeData[(int)m_TargetMode].m_IconAlpha.Evaluate(zoomPerc);
			num = Mathf.Lerp(a, b, m_TransitionTime);
			b = m_ModeData[(int)m_CurrentMode].m_IconAlpha.Evaluate(zoomPerc);
			a = m_ModeData[(int)m_TargetMode].m_IconAlpha.Evaluate(zoomPerc);
			num = Mathf.Lerp(a, b, m_TransitionTime);
		}
		if (bForceUpdate || num != m_fIconCurrentAlpha || num2 != m_fBackerCurrentAlpha)
		{
			m_fIconCurrentAlpha = num;
			Color color = m_ZoneImageMat.material.color;
			color.a = num;
			m_ZoneImageMat.material.color = color;
			m_fBackerCurrentAlpha = num2;
			color = m_ZoneBackerMat.material.color;
			color.a = num2;
			m_ZoneBackerMat.material.color = color;
		}
	}

	private void UpdatePosition(bool bForceUpdate = false)
	{
		if (bForceUpdate || c_ScreenHeight != (float)Screen.height || c_ScreenOrth != m_Controller.GetCameraOrthographicSize())
		{
			c_ScreenHeight = Screen.height;
			c_ScreenOrth = m_Controller.GetCameraOrthographicSize();
			float num = c_ScreenHeight / 1080f;
			float num2 = c_ScreenOrth * 2f / c_ScreenHeight;
			c_ScreenScale = num2 * 64f * num;
		}
		Vector3 zero = Vector3.zero;
		if (m_TargetMode == Mode.INVALID)
		{
			float num3 = 1f;
			num3 = ((!m_ModeData[(int)m_CurrentMode].m_ApplyZoomToScale) ? Mathf.Lerp(m_MinScale, m_MaxScale, m_LastZoomLevel) : c_ScreenScale);
			m_MyTransform.localScale = new Vector3(num3, num3, 1f);
			num3 /= 2f;
			Vector3 vector = new Vector3(num3 * m_ModeData[(int)m_CurrentMode].m_PivotOffset.x, num3 * m_ModeData[(int)m_CurrentMode].m_PivotOffset.y, 0f);
			zero = m_StandardPosition + Vector3.Lerp(m_ModeData[(int)m_CurrentMode].m_MinOffset, m_ModeData[(int)m_CurrentMode].m_MaxOffset, m_LastZoomLevel) + vector;
		}
		else
		{
			float num4 = 1f;
			float num5 = num4;
			num5 = ((!m_ModeData[(int)m_CurrentMode].m_ApplyZoomToScale) ? Mathf.Lerp(m_MinScale, m_MaxScale, m_LastZoomLevel) : c_ScreenScale);
			float num6 = num4;
			num6 = ((!m_ModeData[(int)m_TargetMode].m_ApplyZoomToScale) ? Mathf.Lerp(m_MinScale, m_MaxScale, m_LastZoomLevel) : c_ScreenScale);
			num4 = Mathf.Lerp(num6, num5, m_TransitionTime);
			m_MyTransform.localScale = new Vector3(num4, num4, 1f);
			num4 /= 2f;
			Vector3 vector2 = new Vector3(num4 * m_ModeData[(int)m_CurrentMode].m_PivotOffset.x, num4 * m_ModeData[(int)m_CurrentMode].m_PivotOffset.y, 0f);
			Vector3 b = Vector3.Lerp(m_ModeData[(int)m_CurrentMode].m_MinOffset + vector2, m_ModeData[(int)m_CurrentMode].m_MaxOffset + vector2, m_LastZoomLevel);
			vector2 = new Vector3(num4 * m_ModeData[(int)m_TargetMode].m_PivotOffset.x, num4 * m_ModeData[(int)m_TargetMode].m_PivotOffset.y, 0f);
			Vector3 a = Vector3.Lerp(m_ModeData[(int)m_TargetMode].m_MinOffset + vector2, m_ModeData[(int)m_TargetMode].m_MaxOffset + vector2, m_LastZoomLevel);
			zero = m_StandardPosition + Vector3.Lerp(a, b, m_TransitionTime);
		}
		zero.z -= m_StandardPosition.y * -1f / 120f + m_StandardPosition.x / 240f;
		if ((m_TargetMode != Mode.INVALID && m_TargetMode != 0) || (m_CurrentMode != Mode.INVALID && m_CurrentMode != 0))
		{
			zero.z -= 1f;
		}
		m_MyTransform.localPosition = zero;
	}

	public bool GetMenuPosition(ref Vector3 vPos)
	{
		int num = -1;
		if (m_ModeData[(int)m_CurrentMode].m_DisplayMenu)
		{
			num = (int)m_CurrentMode;
		}
		else if (m_TargetMode != Mode.INVALID && m_ModeData[(int)m_TargetMode].m_DisplayMenu)
		{
			num = (int)m_TargetMode;
		}
		if (num == -1)
		{
			return false;
		}
		float num2 = c_ScreenScale;
		num2 /= 2f;
		Vector3 vector = new Vector3(num2 * m_ModeData[num].m_PivotOffset.x, num2 * m_ModeData[num].m_PivotOffset.y, 0f);
		Vector3 position = base.transform.parent.transform.position;
		vector.x += position.x;
		vector.y += position.y;
		vPos = m_StandardPosition + Vector3.Lerp(m_ModeData[num].m_MinOffset, m_ModeData[num].m_MaxOffset, m_LastZoomLevel) + vector;
		return true;
	}

	private void UpdateBorderType()
	{
		if (m_MyZone.m_ZoneGraphic != null)
		{
			int num = (int)m_TargetMode;
			if (m_TargetMode == Mode.INVALID)
			{
				num = (int)m_CurrentMode;
			}
			m_MyZone.m_ZoneGraphic.SetInteractionState(m_ModeData[num].m_BorderType);
			m_bUpdateZ = true;
		}
	}

	private bool UpdateBorderZ()
	{
		if (m_MyZone.m_ZoneGraphic != null)
		{
			Vector3 localPosition = m_MyZone.m_ZoneGraphic.transform.localPosition;
			localPosition.z = GetBorderZ();
			m_MyZone.m_ZoneGraphic.transform.localPosition = localPosition;
			return true;
		}
		return false;
	}

	private void UpdateBorderColor()
	{
		if (m_MyZone.m_ZoneGraphic != null)
		{
			int num = (int)m_TargetMode;
			if (m_TargetMode == Mode.INVALID)
			{
				num = (int)m_CurrentMode;
			}
			if (m_MyZone.IsFullyValid())
			{
				m_MyZone.m_ZoneGraphic.SetColourState(m_ModeData[num].m_BorderColorValid);
			}
			else
			{
				m_MyZone.m_ZoneGraphic.SetColourState(m_ModeData[num].m_BorderColorInvalid);
			}
		}
	}

	public static LevelEditor_ZoneIconControl CreateZoneIcon(UnityEngine.Object prefab, LevelEditor_ZoneManager.Zone zone, Vector3 vPos, Mode mode)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null && prefab != null && zone != null && mode != Mode.INVALID && mode >= Mode.NotOverZone && mode < Mode.TOTAL)
		{
			UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
			if (@object != null)
			{
				GameObject gameObject = @object as GameObject;
				if (gameObject != null)
				{
					LevelEditor_ZoneIconControl componentInChildren = gameObject.GetComponentInChildren<LevelEditor_ZoneIconControl>();
					if (componentInChildren != null)
					{
						gameObject.transform.parent = instance.m_BuildingLayers[(uint)zone.m_Layer].m_Tiles.transform;
						componentInChildren.m_MyGameObject = gameObject;
						componentInChildren.m_MyTransform = gameObject.transform;
						componentInChildren.m_StandardPosition = vPos;
						componentInChildren.m_MyZone = zone;
						componentInChildren.m_CurrentMode = mode;
						return componentInChildren;
					}
				}
			}
			if (@object != null)
			{
				UnityEngine.Object.Destroy(@object);
			}
		}
		return null;
	}

	public void DestroyZoneIcon()
	{
		if (m_MyGameObject != null)
		{
			UnityEngine.Object.Destroy(m_MyGameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void SetStandardPosition(Vector3 vPos)
	{
		if (vPos != m_StandardPosition)
		{
			m_StandardPosition = vPos;
			m_LastZoomLevel = -1f;
		}
	}

	public OverIconEnum IsOverIcon(float fX, float fY)
	{
		if (m_CurrentlyOverMenu)
		{
			return OverIconEnum.Over;
		}
		if (m_CurrentMode == Mode.NotOverZone)
		{
			return OverIconEnum.NotNear;
		}
		Vector3 position = m_MyTransform.position;
		Vector3 localScale = m_MyTransform.localScale;
		float num = position.x - localScale.x / 2f;
		float num2 = position.y + localScale.y / 2f;
		if (fX >= num && fY <= num2 && fX <= num + localScale.x && fY >= num2 - localScale.y)
		{
			return OverIconEnum.Over;
		}
		float num3 = Mathf.Lerp(m_fZoomedInHotSpot, m_fZoomedOutHotSpot, m_Controller.GetZoomPerc());
		if (fX >= num && fY <= num2 && fX <= num + localScale.x * num3 && fY >= num2 - localScale.y * num3 && (fX <= num + localScale.x || fY >= num2 - localScale.y))
		{
			return OverIconEnum.Near;
		}
		return OverIconEnum.NotNear;
	}

	public void OverMenu(bool bOver)
	{
		m_CurrentlyOverMenu = bOver;
	}

	private void UpdateMenu()
	{
		Mode mode = m_TargetMode;
		if (m_TargetMode == Mode.INVALID)
		{
			mode = m_CurrentMode;
		}
		if (m_ModeData[(int)mode].m_DisplayMenu != m_CurrentlyDisplayingMenu)
		{
			m_CurrentlyDisplayingMenu = m_ModeData[(int)mode].m_DisplayMenu;
			m_Controller.DisplayZoneMenu(m_CurrentlyDisplayingMenu, m_MyZone);
		}
	}

	public float GetBorderZ()
	{
		Mode mode = m_TargetMode;
		if (mode == Mode.INVALID)
		{
			mode = m_CurrentMode;
			if (mode == Mode.INVALID)
			{
				return 0f;
			}
		}
		return m_ModeData[(int)mode].m_BorderZOffset;
	}

	public void ShowIcon(bool bShow)
	{
		m_Show = bShow;
		m_ZoneImageMat.enabled = bShow;
		m_ZoneBackerMat.enabled = bShow;
	}

	public bool IsInMode(Mode mode, bool bCheckCurrent, bool bCheckTarget)
	{
		bool flag = false;
		flag |= bCheckCurrent && m_CurrentMode == mode;
		return flag | (bCheckTarget && m_TargetMode == mode);
	}

	public bool IsDisplayingMenu()
	{
		return m_CurrentlyDisplayingMenu;
	}
}

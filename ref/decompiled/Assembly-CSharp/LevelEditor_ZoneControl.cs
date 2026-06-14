using UnityEngine;
using UnityEngine.UI;

public class LevelEditor_ZoneControl : MonoBehaviour
{
	public enum State
	{
		NotInUse,
		FadingIn,
		FadingOut,
		Selected,
		SelectedInvalid
	}

	public enum MenuButtonTypes
	{
		Header,
		Edit,
		Delete,
		TOTAL
	}

	private LevelEditor_ZoneManager.Zone m_MyZone;

	private LevelEditor_Controller m_Controller;

	private LevelEditor_Cursor m_Cursor;

	public CanvasGroup m_CanvasGroup;

	public T17Text m_MessageText;

	public GameObject m_Master;

	public GameObject m_Bar;

	public T17Image m_BarImage;

	public T17Button m_BarButton;

	private RectTransform m_BarImageTransform;

	public GameObject m_Buttons;

	public T17RawImage m_ZoneTextureImage;

	public T17RawImage m_ZoneBorderImage;

	public ZoneIconBackerData m_ZoneIconBackerData;

	public Image m_EditImage;

	private RectTransform m_EditImageTransform;

	public Image m_DeleteImage;

	private RectTransform m_DeleteImageTransform;

	public float m_MovementSpeed = 1f;

	public float m_FadeSpeed = 1f;

	public string m_ValidSelectingText = string.Empty;

	private string m_ValidSelectingTextTranslated = string.Empty;

	public string m_InvalidSelectingText = string.Empty;

	private string m_InvalidSelectingTextTranslated = string.Empty;

	public string m_DeselectingText = string.Empty;

	private string m_DeselectingTextTranslated = string.Empty;

	public float m_DisabledAlpha = 0.4f;

	private bool m_CurrentlyNearButtons;

	public Sprite m_ValidBackground;

	public Sprite m_InvalidBackground;

	public Color m_ValidTextColour = Color.white;

	public Color m_InvalidTextColour = Color.white;

	private Vector3 m_Destination = Vector3.zero;

	private bool m_Moving;

	private bool m_bInitialized;

	private float m_fCurrentAlpha = 1f;

	private int m_OverCount;

	private int m_MyZoneUpdateCount = -1;

	private LevelEditor_ZoneManager.Zone m_CurrentSelectedZone;

	private State m_State;

	private void Start()
	{
		m_bInitialized = Initialize();
		m_Destination = base.transform.localPosition;
		if (!string.IsNullOrEmpty(m_ValidSelectingText))
		{
			if (!Localization.Get(m_ValidSelectingText, out m_ValidSelectingTextTranslated))
			{
				m_ValidSelectingTextTranslated = "[" + m_ValidSelectingText + "]";
			}
		}
		else
		{
			m_ValidSelectingTextTranslated = "[ NO TEXT RESOURCE SET IN LevelEditor_ZoneControl]";
		}
		if (!string.IsNullOrEmpty(m_InvalidSelectingText))
		{
			if (!Localization.Get(m_InvalidSelectingText, out m_InvalidSelectingTextTranslated))
			{
				m_InvalidSelectingTextTranslated = "[" + m_InvalidSelectingText + "]";
			}
		}
		else
		{
			m_InvalidSelectingTextTranslated = "[ NO TEXT RESOURCE SET IN LevelEditor_ZoneControl]";
		}
		if (!string.IsNullOrEmpty(m_DeselectingText))
		{
			if (!Localization.Get(m_DeselectingText, out m_DeselectingTextTranslated))
			{
				m_DeselectingTextTranslated = "[" + m_DeselectingText + "]";
			}
		}
		else
		{
			m_DeselectingTextTranslated = "[ NO TEXT RESOURCE SET IN LevelEditor_ZoneControl]";
		}
		if (m_BarImage != null)
		{
			m_BarImageTransform = m_BarImage.GetComponent<RectTransform>();
		}
		if (m_EditImage != null)
		{
			m_EditImageTransform = m_EditImage.GetComponent<RectTransform>();
		}
		if (m_DeleteImage != null)
		{
			m_DeleteImageTransform = m_DeleteImage.GetComponent<RectTransform>();
		}
	}

	private void OnDisable()
	{
		m_OverCount = 0;
		if (m_MyZone != null && m_MyZone.m_bActive && m_MyZone.m_ZoneIcon != null)
		{
			m_MyZone.m_ZoneIcon.OverMenu(bOver: false);
		}
	}

	private void Update()
	{
		if (m_bInitialized || (m_bInitialized = Initialize()))
		{
			UpdateState();
		}
	}

	private void LateUpdate()
	{
		PositionToZone();
		UpdateMenuValidity();
	}

	private void UpdateMenuValidity()
	{
		bool flag = m_Controller.IsMenuNearButtons();
		if (m_CurrentlyNearButtons != flag)
		{
			m_CurrentlyNearButtons = flag;
			if (m_CurrentlyNearButtons)
			{
				m_CanvasGroup.alpha = m_DisabledAlpha;
				m_DeleteImage.raycastTarget = false;
				m_EditImage.raycastTarget = false;
				m_BarImage.raycastTarget = false;
			}
			else
			{
				m_CanvasGroup.alpha = 1f;
				m_DeleteImage.raycastTarget = true;
				m_EditImage.raycastTarget = true;
				m_BarImage.raycastTarget = true;
			}
		}
	}

	public void EnteringControlArea()
	{
		m_OverCount++;
		if (m_bInitialized || (m_bInitialized = Initialize()))
		{
			if (m_MyZone != null && m_MyZone.m_bActive && m_MyZone.m_ZoneIcon != null)
			{
				m_MyZone.m_ZoneIcon.OverMenu(m_OverCount > 0);
			}
			m_Cursor.EnteringControlArea();
		}
	}

	public void LeavingControlArea()
	{
		m_OverCount--;
		if (m_bInitialized || (m_bInitialized = Initialize()))
		{
			if (m_MyZone != null && m_MyZone.m_bActive && m_MyZone.m_ZoneIcon != null)
			{
				m_MyZone.m_ZoneIcon.OverMenu(m_OverCount > 0);
			}
			m_Cursor.LeavingControlArea();
		}
	}

	private bool Initialize()
	{
		bool result = true;
		if (m_Cursor == null && (m_Cursor = LevelEditor_Cursor.GetInstance()) == null)
		{
			result = false;
		}
		if (m_Controller == null && (m_Controller = LevelEditor_Controller.GetInstance()) == null)
		{
			result = false;
		}
		if (m_CanvasGroup == null)
		{
			result = false;
		}
		else
		{
			m_CanvasGroup.alpha = m_fCurrentAlpha;
		}
		if (m_MessageText == null)
		{
			result = false;
		}
		if (m_Bar == null)
		{
			result = false;
		}
		if (m_Master == null)
		{
			result = false;
		}
		else
		{
			m_Master.SetActive(value: false);
		}
		if (m_BarImage == null)
		{
			result = false;
		}
		if (m_Buttons == null)
		{
			result = false;
		}
		if (m_ZoneTextureImage == null)
		{
			result = false;
		}
		if (m_ZoneBorderImage == null)
		{
			result = false;
		}
		return result;
	}

	private void UpdatePosition()
	{
		if (m_Moving)
		{
			Vector3 localPosition = base.transform.localPosition;
			float maxDistanceDelta = m_MovementSpeed * Time.deltaTime;
			localPosition = Vector3.MoveTowards(localPosition, m_Destination, maxDistanceDelta);
			base.transform.localPosition = localPosition;
			if (localPosition == m_Destination)
			{
				m_Moving = false;
			}
		}
	}

	private void SetState(State newState, bool bForce = false)
	{
		if (m_State != newState || bForce)
		{
			m_State = newState;
			switch (m_State)
			{
			case State.NotInUse:
				ShowMenu(bShow: false, bClearZone: true);
				break;
			case State.FadingIn:
				SetText();
				m_CanvasGroup.alpha = 1f;
				ShowMenu(bShow: true, bClearZone: false);
				SetState(State.Selected);
				break;
			case State.FadingOut:
				SetState(State.NotInUse);
				break;
			case State.Selected:
				break;
			}
		}
	}

	private void UpdateState()
	{
		switch (m_State)
		{
		case State.NotInUse:
			break;
		case State.FadingIn:
			break;
		case State.FadingOut:
			break;
		case State.Selected:
			if (m_MyZone == null || !m_MyZone.m_bActive)
			{
				SetState(State.FadingOut);
			}
			else if (m_MyZoneUpdateCount != m_MyZone.m_ZoneUpdateCount || m_CurrentSelectedZone != m_Controller.CurrentZone)
			{
				SetColors(m_MyZone.IsFullyValid());
				SetText();
				m_MyZoneUpdateCount = m_MyZone.m_ZoneUpdateCount;
				m_CurrentSelectedZone = m_Controller.CurrentZone;
			}
			break;
		}
	}

	private void SetColors(bool bValid)
	{
	}

	private void SetText()
	{
		if (m_MyZone == null)
		{
			return;
		}
		SetColors(m_MyZone.IsFullyValid());
		if (m_MyZone.IsFullyValid())
		{
			m_ZoneTextureImage.texture = m_MyZone.m_ZoneDetails.m_ZoneImage.mainTexture;
			Rect uvRect = new Rect(m_MyZone.m_ZoneDetails.m_ZoneImage.mainTextureOffset, m_MyZone.m_ZoneDetails.m_ZoneImage.mainTextureScale);
			m_ZoneTextureImage.uvRect = uvRect;
			m_ZoneBorderImage.texture = m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTexture;
			uvRect = new Rect(m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTextureOffset, m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTextureScale);
			m_ZoneBorderImage.uvRect = uvRect;
			if (m_Controller.CurrentZone == m_MyZone)
			{
				m_MessageText.SetNonLocalizedText(m_DeselectingTextTranslated);
			}
			else
			{
				m_MessageText.SetNonLocalizedText(m_ValidSelectingTextTranslated);
			}
			SpriteState spriteState = m_BarButton.spriteState;
			spriteState.pressedSprite = m_ValidBackground;
			m_BarButton.spriteState = spriteState;
			m_BarImage.sprite = m_ValidBackground;
			m_MessageText.color = m_ValidTextColour;
		}
		else
		{
			m_ZoneTextureImage.texture = m_MyZone.m_ZoneDetails.m_ZoneImageInvalid.mainTexture;
			Rect uvRect2 = new Rect(m_MyZone.m_ZoneDetails.m_ZoneImageInvalid.mainTextureOffset, m_MyZone.m_ZoneDetails.m_ZoneImageInvalid.mainTextureScale);
			m_ZoneTextureImage.uvRect = uvRect2;
			m_ZoneBorderImage.texture = m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTexture;
			uvRect2 = new Rect(m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTextureOffset, m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTextureScale);
			m_ZoneBorderImage.uvRect = uvRect2;
			if (m_Controller.CurrentZone == m_MyZone)
			{
				m_MessageText.SetNonLocalizedText(m_DeselectingTextTranslated);
			}
			else
			{
				m_MessageText.SetNonLocalizedText(m_InvalidSelectingTextTranslated);
			}
			SpriteState spriteState2 = m_BarButton.spriteState;
			spriteState2.pressedSprite = m_InvalidBackground;
			m_BarButton.spriteState = spriteState2;
			m_BarImage.sprite = m_InvalidBackground;
			m_MessageText.color = m_InvalidTextColour;
		}
	}

	public void OnEditClicked()
	{
		if ((m_bInitialized || (m_bInitialized = Initialize())) && m_MyZone != null && m_MyZone.m_bActive)
		{
			m_Controller.EnterEditZone(m_MyZone.m_ID);
		}
	}

	public void OnDeleteClicked()
	{
		if ((m_bInitialized || (m_bInitialized = Initialize())) && m_MyZone != null && m_MyZone.m_bActive)
		{
			m_Controller.DeleteZone(m_MyZone);
			m_MyZone = null;
		}
	}

	public void OnTitleClicked()
	{
		if ((m_bInitialized || (m_bInitialized = Initialize())) && m_MyZone != null && m_MyZone.m_bActive)
		{
			m_Controller.SetCurrentZone(m_MyZone, bClearIfSetSame: true);
		}
	}

	public State GetState()
	{
		return m_State;
	}

	public void SetZone(LevelEditor_ZoneManager.Zone zone)
	{
		if ((!m_bInitialized && !(m_bInitialized = Initialize())) || object.ReferenceEquals(zone, m_MyZone))
		{
			return;
		}
		string text = "NULL";
		if (m_MyZone != null)
		{
			text = m_MyZone.m_ZoneType.ToString();
		}
		string text2 = "NULL";
		if (zone != null)
		{
			text2 = zone.m_ZoneType.ToString();
		}
		if (zone == null)
		{
			SetState(State.FadingOut);
			return;
		}
		if (m_MyZone != null)
		{
			if (m_MyZone != null && m_MyZone.m_bActive && m_MyZone.m_ZoneIcon != null)
			{
				m_MyZone.m_ZoneIcon.OverMenu(bOver: false);
			}
			m_OverCount = 0;
		}
		m_MyZone = zone;
		PositionToZone();
		SetState(State.FadingIn, bForce: true);
	}

	private void PositionToZone()
	{
		if (m_MyZone != null && m_MyZone.m_bActive)
		{
			Vector3 vPos = Vector3.zero;
			if (m_MyZone.m_ZoneIcon.GetMenuPosition(ref vPos))
			{
				base.transform.position = m_Controller.ConvertLevelToUIPosition(vPos);
			}
		}
	}

	public void StartMoveTo(float fX, float fY, bool bImediadte)
	{
		if (m_bInitialized || (m_bInitialized = Initialize()))
		{
			m_Moving = false;
			m_Destination.x = fX;
			m_Destination.y = fY;
			if (bImediadte)
			{
				base.transform.localPosition = m_Destination;
			}
			else if (m_Destination != base.transform.localPosition)
			{
				m_Moving = true;
			}
		}
	}

	private void ShowMenu(bool bShow, bool bClearZone)
	{
		if (!(m_Master == null))
		{
			if (!bShow && m_MyZone != null && m_MyZone.m_ZoneIcon != null)
			{
				m_MyZone.m_ZoneIcon.OverMenu(bOver: false);
			}
			if (bClearZone)
			{
				m_MyZone = null;
			}
			m_Master.SetActive(bShow);
		}
	}

	public bool GetButtonArea(MenuButtonTypes buttonType, ref Rect area)
	{
		RectTransform rectTransform = null;
		switch (buttonType)
		{
		case MenuButtonTypes.Header:
			if (m_EditImageTransform != null)
			{
				rectTransform = m_EditImageTransform;
				break;
			}
			return false;
		case MenuButtonTypes.Edit:
			if (m_DeleteImageTransform != null)
			{
				rectTransform = m_DeleteImageTransform;
				break;
			}
			return false;
		default:
			if (m_BarImageTransform != null)
			{
				rectTransform = m_BarImageTransform;
				break;
			}
			return false;
		}
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		area.xMin = array[0].x;
		area.yMin = array[0].y;
		area.xMax = array[2].x;
		area.yMax = array[2].y;
		return true;
	}
}

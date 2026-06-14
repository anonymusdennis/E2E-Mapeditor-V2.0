using UnityEngine;

public class LevelEditor_ZoneCreateButton : MonoBehaviour
{
	public ZoneIconBackerData m_ZoneBackerData;

	public T17RawImage m_IconBacker;

	public T17RawImage m_BlockIcon;

	public GameObject m_ButtonHighlight;

	public T17Text m_Title;

	public T17Text m_Totals;

	public Color m_NormalColour = Color.white;

	public Color m_InvalidColor = new Color(47f / 51f, 0.7137255f, 4f / 85f);

	public Color m_MaxColor = new Color(47f / 51f, 72f / 85f, 0.6156863f);

	public ZoneDetailsManager.ZoneTypes m_ZoneType;

	public float m_SecondsBeforeToolTip;

	private float m_ToolTipTimer = -1f;

	public LevelEditor_FilterButton m_CreatZoneButton;

	private int m_iLimitationID = -1;

	private bool m_bInitialized;

	private string m_StrTitleText = "NOT SET";

	private string m_StrTitleTextNonTranslated = "NOT SET";

	private string m_StrToolTipTextNonTranslated = "NOT SET";

	private bool m_bIsHighlit;

	public static ZoneDetailsManager.ZoneTypes c_CurrentButtonOver;

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void OnDisable()
	{
		if (m_bIsHighlit)
		{
			m_bIsHighlit = false;
			m_ButtonHighlight.SetActive(value: false);
		}
		HideToolTip();
	}

	private bool Initialize()
	{
		if (m_bInitialized)
		{
			return true;
		}
		ZoneDetailsManager instance = ZoneDetailsManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (!(m_IconBacker == null))
		{
			m_IconBacker.enabled = false;
		}
		if (m_BlockIcon == null)
		{
			return false;
		}
		m_BlockIcon.enabled = false;
		if (m_ButtonHighlight == null)
		{
			return false;
		}
		m_ButtonHighlight.SetActive(value: false);
		if (m_Title == null)
		{
			return false;
		}
		if (m_Totals == null)
		{
			return false;
		}
		if (m_ZoneType == ZoneDetailsManager.ZoneTypes.INVALID)
		{
			return false;
		}
		ZoneDetailsManager.ZoneDetails zoneDetails = instance.GetZoneDetails(m_ZoneType);
		if (zoneDetails == null)
		{
			return false;
		}
		m_BlockIcon.texture = zoneDetails.m_ZoneImage.mainTexture;
		Rect uvRect = new Rect(zoneDetails.m_ZoneImage.mainTextureOffset, zoneDetails.m_ZoneImage.mainTextureScale);
		m_BlockIcon.uvRect = uvRect;
		m_IconBacker.texture = m_ZoneBackerData.ZoneIconBacker_Valid.mainTexture;
		uvRect = new Rect(m_ZoneBackerData.ZoneIconBacker_Valid.mainTextureOffset, m_ZoneBackerData.ZoneIconBacker_Valid.mainTextureScale);
		m_IconBacker.uvRect = uvRect;
		m_StrTitleText = zoneDetails.GetZoneNameText();
		m_StrTitleTextNonTranslated = zoneDetails.m_NameResource;
		m_StrToolTipTextNonTranslated = zoneDetails.m_ToolTipResource;
		m_iLimitationID = zoneDetails.m_LimitationGroup;
		m_BlockIcon.enabled = true;
		m_IconBacker.enabled = true;
		m_bInitialized = true;
		return true;
	}

	public void OnEnter()
	{
		if (!Initialize() || m_bIsHighlit)
		{
			return;
		}
		c_CurrentButtonOver = m_ZoneType;
		m_bIsHighlit = true;
		m_ButtonHighlight.SetActive(value: true);
		m_Title.SetNonLocalizedText(m_StrTitleText);
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		string text = string.Empty;
		Color color = m_NormalColour;
		if (m_iLimitationID != -1 && instance != null)
		{
			BuildingBlockManager.LimitationGroup theLimitationGroup = instance.GetTheLimitationGroup(m_iLimitationID);
			if (theLimitationGroup != null)
			{
				text = theLimitationGroup.m_CurrentTotal.ToString();
				if (theLimitationGroup.m_Max != 0)
				{
					text = text + " / " + theLimitationGroup.m_Max;
					if (theLimitationGroup.m_CurrentTotal > theLimitationGroup.m_Max)
					{
						color = m_InvalidColor;
					}
					else if (theLimitationGroup.m_CurrentTotal == theLimitationGroup.m_Max)
					{
						color = m_MaxColor;
					}
				}
			}
		}
		m_Totals.SetNonLocalizedText(text);
		m_Totals.color = color;
		m_ToolTipTimer = m_SecondsBeforeToolTip;
	}

	public void OnLeave()
	{
		if (Initialize() && m_bIsHighlit)
		{
			m_bIsHighlit = false;
			m_ButtonHighlight.SetActive(value: false);
			if (c_CurrentButtonOver == m_ZoneType)
			{
				m_Title.SetLocalisedTextCatchAll("Text.SelectZoneToCreate");
				m_Totals.SetNonLocalizedText(string.Empty);
				c_CurrentButtonOver = ZoneDetailsManager.ZoneTypes.INVALID;
			}
			HideToolTip();
		}
	}

	public void OnPressed()
	{
		if (Initialize())
		{
			LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
			if (instance != null)
			{
				LevelEditor_Controller.GetInstance().EnterCreateZone(m_ZoneType);
			}
			if (m_CreatZoneButton != null)
			{
				m_CreatZoneButton.Hide();
			}
		}
	}

	private void Update()
	{
		if (m_ToolTipTimer >= 0f)
		{
			m_ToolTipTimer -= Time.deltaTime;
			if (m_ToolTipTimer < 0f)
			{
				DisplayToolTip();
			}
		}
	}

	private void DisplayToolTip()
	{
		if (!string.IsNullOrEmpty(m_StrToolTipTextNonTranslated) && LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().DisplayToolTip(m_StrTitleTextNonTranslated, m_StrToolTipTextNonTranslated);
		}
	}

	private void HideToolTip()
	{
		m_ToolTipTimer = -1f;
		if (LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().HideToolTip();
		}
	}
}

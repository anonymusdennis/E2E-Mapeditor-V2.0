using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
	private static Credits s_Instance;

	public GameObject m_Container;

	public ScrollRect m_ScrollRect;

	public T17Text m_ServerLabel;

	public GameObject m_SwitchExtension;

	public Transform m_SwitchExtensionPosition;

	public float m_AutoScrollDelay = 2.7f;

	public float m_ScrollSpeed = 0.01f;

	public float m_AutoCloseDelay = 1.5f;

	public float m_PreventCloseDelay = 1.5f;

	public float m_ScrollSpeedFast = 0.08f;

	private float m_TimestampForAutoScrolling;

	private float m_TimestampForAutoClose;

	private float m_TimeStampForUserClose;

	private bool m_bIsShowing;

	private Gamer m_PrimaryGamer;

	private bool m_bIsAutoScrolling;

	private bool m_bHasReachedBottom;

	private bool m_bFastMode;

	public static Credits GetInstance()
	{
		return s_Instance;
	}

	private void Awake()
	{
		s_Instance = this;
		if (m_ServerLabel != null)
		{
			m_ServerLabel.m_bNeedsLocalization = false;
			switch (PhotonNetwork.ServerSettingsId)
			{
			case 1:
				m_ServerLabel.text = "Server: Live";
				break;
			case 2:
				m_ServerLabel.text = "Server: Dev";
				break;
			case 3:
				m_ServerLabel.text = "Server: QA";
				break;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
	}

	public void ShowAndReset()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Credits_Screen, AudioController.UI_Audio_GO);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Music_Frontend, AudioController.UI_Audio_GO);
		m_TimeStampForUserClose = m_PreventCloseDelay + Time.time;
		ResetScrollView();
		if (m_Container != null)
		{
			m_Container.SetActive(value: true);
		}
		m_bIsShowing = true;
		m_PrimaryGamer = Gamer.GetPrimaryGamer();
		m_TimestampForAutoScrolling = Time.time + m_AutoScrollDelay;
		m_bIsAutoScrolling = false;
	}

	private void ResetScrollView()
	{
		if (m_ScrollRect != null)
		{
			m_ScrollRect.normalizedPosition = new Vector2(0.5f, 1f);
		}
		m_bHasReachedBottom = false;
	}

	public void Hide()
	{
		if (m_Container != null)
		{
			m_Container.SetActive(value: false);
		}
		m_bIsShowing = false;
	}

	private void Update()
	{
		if (!m_bIsShowing)
		{
			return;
		}
		if (m_PrimaryGamer == null)
		{
			m_PrimaryGamer = Gamer.GetPrimaryGamer();
		}
		if (m_PrimaryGamer != null && m_PrimaryGamer.m_RewiredPlayer != null)
		{
			if (m_PrimaryGamer.m_RewiredPlayer.GetButtonDown("UI_Cancel") && ExitCreditsScreen())
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
			}
			if (m_PrimaryGamer.m_RewiredPlayer.GetButtonDown("UI_Submit"))
			{
				m_bFastMode = !m_bFastMode;
			}
		}
		if (!m_bHasReachedBottom)
		{
			if (m_bIsAutoScrolling)
			{
				if (m_bFastMode)
				{
					m_ScrollRect.normalizedPosition = new Vector2(0.5f, m_ScrollRect.verticalNormalizedPosition - m_ScrollSpeedFast * Time.deltaTime);
				}
				else
				{
					m_ScrollRect.normalizedPosition = new Vector2(0.5f, m_ScrollRect.verticalNormalizedPosition - m_ScrollSpeed * Time.deltaTime);
				}
			}
			else if (Time.time > m_TimestampForAutoScrolling)
			{
				m_bIsAutoScrolling = true;
			}
			if (m_ScrollRect.verticalNormalizedPosition <= 0f)
			{
				m_bHasReachedBottom = true;
				m_TimestampForAutoClose = Time.time + m_AutoCloseDelay;
			}
		}
		if (m_bHasReachedBottom && Time.time > m_TimestampForAutoClose && ExitCreditsScreen())
		{
			m_bIsShowing = false;
		}
	}

	public bool ExitCreditsScreen()
	{
		if (Time.time >= m_TimeStampForUserClose)
		{
			m_bFastMode = false;
			GlobalStart.GetInstance().HideCreditsScreen();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Credits_Screen, AudioController.UI_Audio_GO);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Music_Frontend, AudioController.UI_Audio_GO);
			return true;
		}
		return false;
	}
}

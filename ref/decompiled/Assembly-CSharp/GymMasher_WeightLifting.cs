using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class GymMasher_WeightLifting : GymMasherBase
{
	private const float MIN_GAIN_THRESHOLD = 0.001f;

	[Header("GymMasher_WeightLifting")]
	public T17Text m_Key1;

	public T17Text m_Key2;

	public T17Image m_Key1Image;

	public T17Image m_Key2Image;

	public T17Slider m_Slider;

	public T17Image m_ThresholdMarker1;

	public T17Image m_ThresholdMarker2;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	public Sprite m_LightOff;

	public Sprite m_LightOn;

	public T17Image m_Light1;

	public T17Image m_Light2;

	public T17Image m_Light3;

	private AlternateButtonMasher.AlternateMasherSettings m_MasherSettings;

	private T17Image m_FillImage;

	private float m_TimeToKeepInThreshold = 3f;

	private float m_CurrentGain;

	private bool m_bKeyOnePressed;

	private bool m_bKeyTwoPressed;

	private bool m_bIsPositive;

	private bool m_bBackToZero;

	private bool m_bInValidState;

	private bool m_bResettingGain;

	private Vector3 m_Key1OrigScale;

	private Vector3 m_Key2OrigScale;

	private Vector3 m_Key1ImageOrigScale;

	private Vector3 m_Key2ImageOrigScale;

	private float m_ElapsedTimeInThreshold;

	private float m_OneThridTime;

	private bool m_bHadSuccesfullRep;

	private bool m_bReportSucces;

	private bool m_bReportStaminaSpent;

	protected override void Awake()
	{
		base.Awake();
		ResetLights();
	}

	protected override void Update()
	{
		base.Update();
		UpdateKeyInput();
		m_bReportSucces = false;
		m_bReportStaminaSpent = false;
		UpdateVizualization();
	}

	private void UpdateKeyInput()
	{
		if (m_RewiredPlayer != null)
		{
			Controller lastActiveController = m_RewiredPlayer.controllers.GetLastActiveController();
			if (lastActiveController != null)
			{
				if (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Mouse)
				{
					if (m_Key1Image != null)
					{
						m_Key1Image.enabled = true;
					}
					if (m_Key2Image != null)
					{
						m_Key2Image.enabled = true;
					}
				}
				else
				{
					if (m_Key1Image != null)
					{
						m_Key1Image.enabled = false;
					}
					if (m_Key2Image != null)
					{
						m_Key2Image.enabled = false;
					}
				}
			}
		}
		if (m_bResettingGain)
		{
			m_CurrentGain = Mathf.Lerp(m_CurrentGain, 0f, UpdateManager.deltaTime * (5f * (1.1f - m_CurrentGain)));
			if (m_CurrentGain < 0.02f)
			{
				m_CurrentGain = 0f;
				m_bResettingGain = false;
			}
		}
		else
		{
			float num = m_RewiredPlayer.GetAxis("Alternate_Key1") + m_RewiredPlayer.GetAxis("Alternate_Key2");
			if (num > 0.8f)
			{
				m_bKeyOnePressed = true;
			}
			else if (num < -0.8f)
			{
				m_bKeyTwoPressed = true;
			}
			m_bIsPositive = num > 0f;
			if (num < 0.8f && num > -0.8f)
			{
				m_bBackToZero = true;
			}
			if (!m_bInValidState && m_bKeyOnePressed != m_bKeyTwoPressed)
			{
				m_bInValidState = true;
			}
			m_CurrentGain = Mathf.Clamp(m_CurrentGain - m_MasherSettings.m_DecayPerSecond * UpdateManager.deltaTime, 0f, 1f);
			if (m_bInValidState && m_bKeyOnePressed && m_bBackToZero && m_bKeyTwoPressed)
			{
				if (m_bIsPositive)
				{
					m_bKeyTwoPressed = false;
					m_Key1.rectTransform.localScale = m_Key1OrigScale;
					m_Key2.rectTransform.localScale = m_Key2OrigScale * 1.3f;
					if (m_Key1Image != null)
					{
						m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale;
					}
					if (m_Key2Image != null)
					{
						m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale * 1.3f;
					}
					OnRepACompleted();
				}
				else
				{
					m_bKeyOnePressed = false;
					m_Key1.rectTransform.localScale = m_Key1OrigScale * 1.3f;
					m_Key2.rectTransform.localScale = m_Key2OrigScale;
					if (m_Key1Image != null)
					{
						m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale * 1.3f;
					}
					if (m_Key2Image != null)
					{
						m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale;
					}
					OnRepBCompleted();
				}
				m_bBackToZero = false;
				ApplyGain();
			}
		}
		UpdateSlider();
		if (m_CurrentGain < 0.001f)
		{
			m_MasherState = AlternateButtonMasher.MasherState.Idle;
			return;
		}
		AlternateButtonMasher.MasherState masherState = ((!(m_CurrentGain >= m_MasherSettings.m_Threshold1) || !(m_CurrentGain <= m_MasherSettings.m_Threshold2)) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid);
		if (masherState != m_MasherState)
		{
			m_MasherState = masherState;
			if (m_FillImage != null)
			{
				m_FillImage.sprite = ((m_MasherState != AlternateButtonMasher.MasherState.Valid) ? m_InvalidSprite : m_ValidSprite);
			}
		}
	}

	private void UpdateVizualization()
	{
		if (m_MasherState == AlternateButtonMasher.MasherState.Valid)
		{
			m_ElapsedTimeInThreshold += UpdateManager.deltaTime;
			if (m_ElapsedTimeInThreshold >= m_OneThridTime && m_Light1 != null && m_Light1.sprite != m_LightOn)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
				m_bReportStaminaSpent = true;
				m_Light1.sprite = m_LightOn;
			}
			if (m_ElapsedTimeInThreshold >= m_OneThridTime * 2f && m_Light2 != null && m_Light2.sprite != m_LightOn)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
				m_bReportStaminaSpent = true;
				m_Light2.sprite = m_LightOn;
			}
			if (m_ElapsedTimeInThreshold >= m_TimeToKeepInThreshold)
			{
				if (m_Light3 != null && m_Light3.sprite != m_LightOn)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
					m_bReportStaminaSpent = true;
					m_Light3.sprite = m_LightOn;
				}
				if (!m_bHadSuccesfullRep)
				{
					m_bReportSucces = true;
					m_bHadSuccesfullRep = true;
				}
				if (m_ElapsedTimeInThreshold >= m_TimeToKeepInThreshold + 0.5f)
				{
					ResetLights();
					ResetGain();
				}
			}
		}
		else if (m_ElapsedTimeInThreshold > 0f)
		{
			ResetLights();
		}
	}

	private void ResetLights()
	{
		m_bHadSuccesfullRep = false;
		m_bReportSucces = false;
		m_ElapsedTimeInThreshold = 0f;
		if (m_Light1 != null)
		{
			m_Light1.sprite = m_LightOff;
		}
		if (m_Light2 != null)
		{
			m_Light2.sprite = m_LightOff;
		}
		if (m_Light3 != null)
		{
			m_Light3.sprite = m_LightOff;
		}
	}

	private void ApplyGain()
	{
		m_CurrentGain = Mathf.Clamp(m_CurrentGain + m_MasherSettings.m_GainPerAlternate, 0f, 1f);
	}

	private void ApplyPenalty()
	{
		m_CurrentGain = Mathf.Clamp(m_CurrentGain - m_MasherSettings.m_PenaltyWhenWrong, 0f, 1f);
		m_bKeyOnePressed = false;
		m_bKeyTwoPressed = false;
		m_bInValidState = false;
	}

	private void ResetGain(bool instant = false)
	{
		if (!instant)
		{
			if (m_bResettingGain)
			{
				return;
			}
			m_bResettingGain = true;
		}
		else
		{
			m_CurrentGain = 0f;
		}
		m_bInValidState = false;
		m_bKeyOnePressed = false;
		m_bKeyTwoPressed = false;
	}

	private void UpdateSlider()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentGain;
		}
	}

	public void SetMasherSettings(ref AlternateButtonMasher.AlternateMasherSettings settings, float timeToKeepInThreshold)
	{
		m_MasherSettings = settings;
		m_TimeToKeepInThreshold = timeToKeepInThreshold;
		m_OneThridTime = m_TimeToKeepInThreshold / 3f;
		Setup();
	}

	protected override void Setup()
	{
		base.Setup();
		SetThresholdSettings();
		m_FillImage = m_Slider.fillRect.GetComponent<T17Image>();
		if (m_FillImage != null)
		{
			m_FillImage.sprite = m_InvalidSprite;
		}
		m_Key1OrigScale = m_Key1.rectTransform.localScale;
		m_Key2OrigScale = m_Key2.rectTransform.localScale;
		if (m_Key1Image != null)
		{
			m_Key1ImageOrigScale = m_Key1Image.rectTransform.localScale;
		}
		if (m_Key2Image != null)
		{
			m_Key2ImageOrigScale = m_Key2Image.rectTransform.localScale;
		}
		if (m_Key1Image != null)
		{
			m_Key1Image.rectTransform.position = m_Key1.rectTransform.position;
		}
		if (m_Key2Image != null)
		{
			m_Key2Image.rectTransform.position = m_Key2.rectTransform.position;
		}
	}

	private void SetThresholdSettings()
	{
		float width = m_Slider.GetComponent<RectTransform>().rect.width;
		float x = width * m_MasherSettings.m_Threshold1;
		float x2 = width * m_MasherSettings.m_Threshold2;
		Vector2 anchoredPosition = m_ThresholdMarker1.rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		m_ThresholdMarker1.rectTransform.anchoredPosition = anchoredPosition;
		if (m_MasherSettings.m_Threshold2 < 1f)
		{
			m_ThresholdMarker2.gameObject.SetActive(value: true);
			anchoredPosition = m_ThresholdMarker2.rectTransform.anchoredPosition;
			anchoredPosition.x = x2;
			m_ThresholdMarker2.rectTransform.anchoredPosition = anchoredPosition;
		}
		else
		{
			m_ThresholdMarker2.gameObject.SetActive(value: false);
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_Key1.rectTransform.localScale = m_Key1OrigScale;
		m_Key2.rectTransform.localScale = m_Key2OrigScale;
		if (m_Key1Image != null)
		{
			m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale;
		}
		if (m_Key2Image != null)
		{
			m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale;
		}
		m_bKeyOnePressed = false;
		m_bKeyTwoPressed = false;
		m_bIsPositive = false;
		m_bBackToZero = false;
		m_bInValidState = false;
		ResetLights();
		ResetGain(instant: true);
		UpdateSlider();
	}

	public override void SetPlayerToCheck(Player player)
	{
		base.SetPlayerToCheck(player);
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
		if (m_Key1 != null)
		{
			m_Key1.SetGamerForEventSystem(player.m_Gamer, eventSystemForGamer);
		}
		if (m_Key2 != null)
		{
			m_Key2.SetGamerForEventSystem(player.m_Gamer, eventSystemForGamer);
		}
	}

	public override AlternateButtonMasher.MasherState GetMasherState()
	{
		return (!m_bReportSucces) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid;
	}

	public override bool StaminaSpent()
	{
		return m_bReportStaminaSpent;
	}
}

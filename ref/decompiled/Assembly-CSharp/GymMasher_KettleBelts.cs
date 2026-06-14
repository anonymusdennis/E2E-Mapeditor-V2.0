using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class GymMasher_KettleBelts : GymMasherBase
{
	[Serializable]
	public struct HoldingMasherSettings
	{
		[Range(0.1f, 0.9f)]
		public float m_SafezoneSize;

		public float m_DecayPerSecond;

		public float m_GainIncreasePerSecond;

		public float m_ThresholdChangeTime;

		public float m_MinSafezoneChange;

		public float m_MaxSafezoneChange;

		public float m_TimeToKeepInThreshold;
	}

	private const float MIN_GAIN_THRESHOLD = 0.001f;

	[Header("GymMasher_KettleBelts")]
	public T17Text m_Key;

	public T17Image m_KeyImage;

	public T17Slider m_Slider;

	public T17Image m_ThresholdMarker1;

	public T17Image m_ThresholdMarker2;

	public Sprite m_LightOff;

	public Sprite m_LightOn;

	public T17Image m_Light1;

	public T17Image m_Light2;

	public T17Image m_Light3;

	public Sprite m_SuccesfulSprite;

	public Sprite m_InvalidSprite;

	[HideInInspector]
	public HoldingMasherSettings m_MasherSettings;

	private T17Image m_FillImage;

	private float m_CurrentGain;

	private bool m_bResettingGain;

	private bool m_bChangeThreshold;

	private float m_ElapsedthresholdChangeTime;

	private float m_OldLowerThreshold;

	private float m_NewLowerThreshold;

	private float m_LowerThreshold;

	private float m_CurrentSafezoneSize;

	private float m_SliderWidth;

	private float m_ElapsedTimeInThreshold;

	private float m_OneThridTime;

	private bool m_bHadSuccesfullRep;

	private bool m_bReportSucces;

	private bool m_bReportStaminaSpent;

	private const float AXIS_REGISTER_THRESHOLD = -0.5f;

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
		if (m_RewiredPlayer != null && m_KeyImage != null)
		{
			Controller lastActiveController = m_RewiredPlayer.controllers.GetLastActiveController();
			if (lastActiveController != null)
			{
				if (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Mouse)
				{
					m_KeyImage.enabled = true;
				}
				else
				{
					m_KeyImage.enabled = false;
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
			float axis = m_RewiredPlayer.GetAxis("Alternate_Key2");
			if (axis < -0.5f)
			{
				float num = Mathf.Abs(-0.5f - axis) / 1.5f;
				m_CurrentGain = Mathf.Clamp(m_CurrentGain + m_MasherSettings.m_GainIncreasePerSecond * num * UpdateManager.deltaTime, 0f, 1f);
			}
			else
			{
				m_CurrentGain = Mathf.Clamp(m_CurrentGain - m_MasherSettings.m_DecayPerSecond * UpdateManager.deltaTime, 0f, 1f);
			}
		}
		UpdateSlider();
		if (m_CurrentGain < 0.001f)
		{
			m_MasherState = AlternateButtonMasher.MasherState.Idle;
		}
		else
		{
			AlternateButtonMasher.MasherState masherState = ((!(m_CurrentGain >= m_LowerThreshold) || !(m_CurrentGain <= m_LowerThreshold + m_CurrentSafezoneSize)) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid);
			if (masherState != m_MasherState)
			{
				m_MasherState = masherState;
				bool flag = m_MasherState == AlternateButtonMasher.MasherState.Valid;
				if (m_FillImage != null)
				{
					m_FillImage.sprite = ((!flag) ? m_InvalidSprite : m_SuccesfulSprite);
				}
				m_bChangeThreshold = flag;
			}
			m_LowerThreshold = Mathf.Lerp(m_LowerThreshold, m_LowerThreshold + UnityEngine.Random.Range(-0.2f, 0.2f), UpdateManager.deltaTime);
		}
		if (m_bChangeThreshold)
		{
			m_ElapsedthresholdChangeTime += UpdateManager.deltaTime;
			if (m_ElapsedthresholdChangeTime >= m_MasherSettings.m_ThresholdChangeTime)
			{
				m_ElapsedthresholdChangeTime = 0f;
				bool flag2 = UnityEngine.Random.Range(0, 2) == 0;
				float num2 = UnityEngine.Random.Range(m_MasherSettings.m_MinSafezoneChange, m_MasherSettings.m_MinSafezoneChange);
				m_OldLowerThreshold = m_LowerThreshold;
				m_NewLowerThreshold = Mathf.Clamp(m_LowerThreshold + ((!flag2) ? (0f - num2) : num2), 0f, 1f - m_MasherSettings.m_SafezoneSize);
			}
			m_LowerThreshold = Mathf.Lerp(m_OldLowerThreshold, m_NewLowerThreshold, m_ElapsedthresholdChangeTime / m_MasherSettings.m_ThresholdChangeTime);
			SetThresholdSettings();
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
			if (m_ElapsedTimeInThreshold >= m_MasherSettings.m_TimeToKeepInThreshold)
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
				if (m_ElapsedTimeInThreshold >= m_MasherSettings.m_TimeToKeepInThreshold + 0.5f)
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

	private void ResetGain(bool instant = false)
	{
		if (!instant)
		{
			if (!m_bResettingGain)
			{
				m_bResettingGain = true;
			}
		}
		else
		{
			m_CurrentGain = 0f;
		}
	}

	private void UpdateSlider()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentGain;
		}
	}

	public void SetMasherSettings(ref HoldingMasherSettings settings)
	{
		m_MasherSettings = settings;
		m_OneThridTime = settings.m_TimeToKeepInThreshold / 3f;
		m_LowerThreshold = UnityEngine.Random.Range(0.1f, 1f - m_MasherSettings.m_SafezoneSize);
		m_OldLowerThreshold = m_LowerThreshold;
		m_NewLowerThreshold = m_LowerThreshold;
		m_CurrentSafezoneSize = m_MasherSettings.m_SafezoneSize;
		m_CurrentGain = 0f;
		m_ElapsedTimeInThreshold = 0f;
		Setup();
	}

	protected override void Setup()
	{
		base.Setup();
		m_SliderWidth = m_Slider.GetComponent<RectTransform>().rect.width;
		SetThresholdSettings();
		m_FillImage = m_Slider.fillRect.GetComponent<T17Image>();
		if (m_FillImage != null)
		{
			m_FillImage.sprite = m_InvalidSprite;
		}
	}

	private void SetThresholdSettings()
	{
		float x = m_SliderWidth * m_LowerThreshold;
		float x2 = m_SliderWidth * (m_LowerThreshold + m_CurrentSafezoneSize);
		Vector2 anchoredPosition = m_ThresholdMarker1.rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		m_ThresholdMarker1.rectTransform.anchoredPosition = anchoredPosition;
		anchoredPosition = m_ThresholdMarker2.rectTransform.anchoredPosition;
		anchoredPosition.x = x2;
		m_ThresholdMarker2.rectTransform.anchoredPosition = anchoredPosition;
	}

	public override void Reset()
	{
		base.Reset();
		ResetLights();
		ResetGain(instant: true);
		UpdateSlider();
	}

	public override void SetPlayerToCheck(Player player)
	{
		base.SetPlayerToCheck(player);
		if (m_Key != null)
		{
			m_Key.SetGamerForEventSystem(player.m_Gamer);
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

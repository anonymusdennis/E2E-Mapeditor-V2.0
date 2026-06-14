using System;
using Rewired;
using UnityEngine;

public class GymMasher_Threadmill_ExerciseBike : GymMasherBase
{
	[Serializable]
	public struct ThreadMillMasherSettings
	{
		public float m_DecayPerSecond;

		public float m_GainPerAlternate;

		public float m_StaminaSpendTimeInterval;

		public bool IsValid()
		{
			return m_DecayPerSecond != 0f;
		}
	}

	private const float MIN_GAIN_THRESHOLD = 0.001f;

	[Header("GymMasher_Threadmill_ExerciseBike")]
	public T17Text m_Key1;

	public T17Text m_Key2;

	public T17Image m_Key1Image;

	public T17Image m_Key2Image;

	public T17Slider m_Slider;

	public T17Text m_DistanceValue;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	public bool m_bSingleButtonMode;

	public string m_SingleButtonId = "PrimaryKey";

	private ThreadMillMasherSettings m_MasherSettings;

	private T17Image m_FillImage;

	private float m_CurrentGain;

	private float m_ElapsedStaminaSpentTime;

	private bool m_bKeyOnePressed;

	private bool m_bKeyTwoPressed;

	private bool m_bIsPositive;

	private bool m_bBackToZero;

	private bool m_bInValidState;

	private Vector3 m_Key1OrigScale;

	private Vector3 m_Key2OrigScale;

	private Vector3 m_Key1ImageOrigScale;

	private Vector3 m_Key2ImageOrigScale;

	private bool m_bReportSucces;

	private bool m_bReportStaminaSpent;

	private int m_SuccesfullRuns;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		m_bReportSucces = false;
		m_bReportStaminaSpent = false;
		UpdateKeyInput();
	}

	private void UpdateKeyInput()
	{
		if (!m_bSingleButtonMode)
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
		}
		else
		{
			if (m_RewiredPlayer != null && m_Key1Image != null)
			{
				Controller lastActiveController2 = m_RewiredPlayer.controllers.GetLastActiveController();
				if (lastActiveController2 != null)
				{
					if (lastActiveController2.type == ControllerType.Keyboard || lastActiveController2.type == ControllerType.Mouse)
					{
						m_Key1Image.enabled = true;
					}
					else
					{
						m_Key1Image.enabled = false;
					}
				}
			}
			if (m_Key2Image != null)
			{
				m_Key2Image.enabled = false;
			}
			if (m_RewiredPlayer.GetButtonDown(m_SingleButtonId) && m_Key1 != null)
			{
				m_bInValidState = true;
				m_bIsPositive = !m_bIsPositive;
				m_Key1.rectTransform.localScale = m_Key1OrigScale * 1.3f;
				if (m_Key1Image != null)
				{
					m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale * 1.3f;
				}
			}
			if (m_RewiredPlayer.GetButtonUp(m_SingleButtonId) && m_Key1 != null)
			{
				m_Key1.rectTransform.localScale = m_Key1OrigScale;
				if (m_Key1Image != null)
				{
					m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale;
				}
			}
		}
		m_CurrentGain = Mathf.Clamp(m_CurrentGain - m_MasherSettings.m_DecayPerSecond * UpdateManager.deltaTime, 0f, 1f);
		if (m_bInValidState)
		{
			if (!m_bSingleButtonMode)
			{
				if (m_bKeyOnePressed && m_bBackToZero && m_bKeyTwoPressed)
				{
					if (m_bIsPositive)
					{
						m_bKeyTwoPressed = false;
						OnRepACompleted();
						if (m_Key1 != null)
						{
							m_Key1.rectTransform.localScale = m_Key1OrigScale;
							if (m_Key1Image != null)
							{
								m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale;
							}
						}
						if (m_Key2 != null)
						{
							m_Key2.rectTransform.localScale = m_Key2OrigScale * 1.3f;
							if (m_Key2Image != null)
							{
								m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale * 1.3f;
							}
						}
					}
					else
					{
						m_bKeyOnePressed = false;
						OnRepBCompleted();
						if (m_Key1 != null)
						{
							m_Key1.rectTransform.localScale = m_Key1OrigScale * 1.3f;
							if (m_Key1Image != null)
							{
								m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale * 1.3f;
							}
						}
						if (m_Key2 != null)
						{
							m_Key2.rectTransform.localScale = m_Key2OrigScale;
							if (m_Key2Image != null)
							{
								m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale;
							}
						}
					}
					m_bBackToZero = false;
					ApplyGain();
				}
			}
			else
			{
				ApplyGain();
				m_bInValidState = false;
			}
		}
		UpdateSlider();
		if (m_CurrentGain < 0.001f)
		{
			m_MasherState = AlternateButtonMasher.MasherState.Idle;
			return;
		}
		AlternateButtonMasher.MasherState masherState = ((!(m_CurrentGain >= 1f)) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid);
		if (masherState != m_MasherState)
		{
			m_MasherState = masherState;
			if (m_FillImage != null)
			{
				m_FillImage.sprite = ((m_MasherState != AlternateButtonMasher.MasherState.Valid) ? m_InvalidSprite : m_ValidSprite);
			}
			if (masherState == AlternateButtonMasher.MasherState.Valid)
			{
				ResetGain();
				m_SuccesfullRuns++;
				if (m_DistanceValue != null)
				{
					m_DistanceValue.text = m_SuccesfullRuns.ToString();
				}
				m_bReportSucces = true;
			}
		}
		m_ElapsedStaminaSpentTime += UpdateManager.deltaTime;
		if (m_ElapsedStaminaSpentTime >= m_MasherSettings.m_StaminaSpendTimeInterval)
		{
			m_ElapsedStaminaSpentTime = 0f;
			m_bReportStaminaSpent = true;
		}
	}

	private void ApplyGain()
	{
		m_CurrentGain = Mathf.Clamp(m_CurrentGain + m_MasherSettings.m_GainPerAlternate, 0f, 1f);
	}

	private void ResetGain(bool instant = false)
	{
		m_CurrentGain = 0f;
	}

	private void UpdateSlider()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentGain;
		}
	}

	public void SetMasherSettings(ref ThreadMillMasherSettings settings)
	{
		m_MasherSettings = settings;
		m_SuccesfullRuns = 0;
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_ElapsedStaminaSpentTime = 0f;
		Setup();
	}

	protected override void Setup()
	{
		base.Setup();
		m_FillImage = m_Slider.fillRect.GetComponent<T17Image>();
		if (m_FillImage != null)
		{
			m_FillImage.sprite = m_InvalidSprite;
		}
		if (m_Key1 != null)
		{
			m_Key1OrigScale = m_Key1.rectTransform.localScale;
			if (m_Key1Image != null)
			{
				m_Key1ImageOrigScale = m_Key1Image.rectTransform.localScale;
			}
		}
		if (m_Key2 != null)
		{
			m_Key2OrigScale = m_Key2.rectTransform.localScale;
			if (m_Key2Image != null)
			{
				m_Key2ImageOrigScale = m_Key2Image.rectTransform.localScale;
			}
		}
		m_bKeyOnePressed = false;
		m_bKeyTwoPressed = false;
		m_bIsPositive = false;
		m_bBackToZero = false;
		m_bInValidState = false;
	}

	public override void Reset()
	{
		base.Reset();
		if (m_Key1 != null)
		{
			m_Key1.rectTransform.localScale = m_Key1OrigScale;
			if (m_Key1Image != null)
			{
				m_Key1Image.rectTransform.localScale = m_Key1ImageOrigScale;
			}
		}
		if (m_Key2 != null)
		{
			m_Key2.rectTransform.localScale = m_Key2OrigScale;
			if (m_Key2Image != null)
			{
				m_Key2Image.rectTransform.localScale = m_Key2ImageOrigScale;
			}
		}
		m_bKeyOnePressed = false;
		m_bKeyTwoPressed = false;
		m_bIsPositive = false;
		m_bBackToZero = false;
		m_bInValidState = false;
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

using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class GymMasher_Pommel_Footbag : GymMasherBase
{
	[Serializable]
	public struct PommelMasherSettings
	{
		[Range(0.1f, 0.45f)]
		public float m_ThresholdStartSize;

		public float m_ThresholdMinSize;

		public float m_ThresholdDecreaseSize;

		public float m_MarkerStartSpeed;

		public float m_MarkerMaxSpeed;

		public float m_MarkerMinSpeed;

		public int m_RotationNeededForStatIncrease;

		public float m_StaminaSpendTimeInterval;

		public float m_ExpectedPlayerAnimationTime;
	}

	[Header("GymMasher_Pommel_Footbag")]
	public T17Text m_Key1;

	public T17Text m_Key2;

	public T17Image m_Key1Image;

	public T17Image m_Key2Image;

	public T17Image m_Safezone1;

	public T17Image m_Safezone2;

	public T17Slider m_Slider;

	public Sprite m_SafeZoneHitSprite;

	public Sprite m_SafeZoneIdleSprite;

	public T17Text m_DistanceValue;

	private PommelMasherSettings m_MasherSettings;

	private float m_ElapsedStaminaSpentTime;

	private float m_CurrentSliderSpeed = 1f;

	private float m_CurrentSliderValue;

	private bool m_bWasInLeft;

	private bool m_bWasInRight;

	private bool m_bDidLeft;

	private bool m_bDidRight;

	private float m_Threshold1;

	private float m_Threshold2;

	private float m_CurrentThresholdSize;

	private Vector3 m_Key1OrigScale;

	private Vector3 m_Key2OrigScale;

	private Vector3 m_Key1ImageOrigScale;

	private Vector3 m_Key2ImageOrigScale;

	private bool m_bReportSucces;

	private bool m_bReportStaminaSpent;

	private int m_SuccesfullRuns;

	private int m_Needed = 2;

	private bool m_bIgnoreLeftInput;

	private bool m_bIgnoreRightInput;

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
		m_CurrentSliderValue += m_CurrentSliderSpeed * UpdateManager.deltaTime;
		if (m_CurrentSliderValue >= 1f)
		{
			m_CurrentSliderValue = 1f;
			m_CurrentSliderSpeed *= -1f;
		}
		else if (m_CurrentSliderValue <= 0f)
		{
			m_CurrentSliderValue = 0f;
			m_CurrentSliderSpeed *= -1f;
		}
		UpdateSlider();
		if (m_CurrentSliderValue <= m_Threshold1)
		{
			m_bWasInLeft = true;
			m_bWasInRight = false;
			if ((double)m_RewiredPlayer.GetAxis("Alternate_Key1") > 0.5)
			{
				if (!m_bIgnoreLeftInput)
				{
					m_bIgnoreLeftInput = true;
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
					bool bDidLeft = m_bDidLeft;
					m_bDidLeft = true;
					if (m_Safezone1 != null)
					{
						m_Safezone1.sprite = m_SafeZoneHitSprite;
						if (!bDidLeft && m_bDidLeft)
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
						}
					}
				}
			}
			else
			{
				m_bIgnoreLeftInput = false;
			}
			if (m_bDidRight && m_bDidLeft)
			{
				m_Needed--;
				m_bDidRight = false;
				if (m_Safezone2 != null)
				{
					m_Safezone2.sprite = m_SafeZoneIdleSprite;
				}
				OnRepACompleted();
				if (m_Needed == 0)
				{
					ReportSucces(wasSafezone1Hit: true);
				}
			}
		}
		else if (m_CurrentSliderValue >= m_Threshold2)
		{
			m_bWasInRight = true;
			m_bWasInLeft = false;
			if ((double)m_RewiredPlayer.GetAxis("Alternate_Key2") < -0.5)
			{
				if (!m_bIgnoreRightInput)
				{
					m_bIgnoreRightInput = true;
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
					bool bDidRight = m_bDidRight;
					m_bDidRight = true;
					if (m_Safezone2 != null)
					{
						m_Safezone2.sprite = m_SafeZoneHitSprite;
						if (!bDidRight && m_bDidRight)
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
						}
					}
				}
			}
			else
			{
				m_bIgnoreRightInput = false;
			}
			if (m_bDidRight && m_bDidLeft)
			{
				m_Needed--;
				m_bDidLeft = false;
				if (m_Safezone1 != null)
				{
					m_Safezone1.sprite = m_SafeZoneIdleSprite;
				}
				OnRepBCompleted();
				if (m_Needed == 0)
				{
					ReportSucces(wasSafezone1Hit: false);
				}
			}
		}
		else
		{
			if (((double)m_RewiredPlayer.GetAxis("Alternate_Key2") < -0.5 && !m_bIgnoreRightInput) || ((double)m_RewiredPlayer.GetAxis("Alternate_Key1") > 0.5 && !m_bIgnoreLeftInput))
			{
				Fail();
			}
			if (m_bWasInLeft && m_bDidRight)
			{
				Fail();
			}
			if (m_bWasInRight && m_bDidLeft)
			{
				Fail();
			}
		}
		if (m_bDidLeft || m_bDidRight)
		{
			m_ElapsedStaminaSpentTime += UpdateManager.deltaTime;
			if (m_ElapsedStaminaSpentTime >= m_MasherSettings.m_StaminaSpendTimeInterval)
			{
				m_ElapsedStaminaSpentTime = 0f;
				m_bReportStaminaSpent = true;
			}
		}
	}

	private void ReportSucces(bool wasSafezone1Hit)
	{
		m_SuccesfullRuns++;
		m_Needed = 2;
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_bReportSucces = true;
		m_CurrentSliderSpeed = Mathf.Sign(m_CurrentSliderSpeed) * UnityEngine.Random.Range(m_MasherSettings.m_MarkerMinSpeed, m_MasherSettings.m_MarkerMaxSpeed);
		m_CurrentThresholdSize = Mathf.Clamp(m_CurrentThresholdSize - m_MasherSettings.m_ThresholdDecreaseSize, m_MasherSettings.m_ThresholdMinSize, m_MasherSettings.m_ThresholdStartSize);
		SetThresholds(wasSafezone1Hit, !wasSafezone1Hit);
	}

	private void Fail()
	{
		if (m_SuccesfullRuns != 0)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Fail, m_Player.gameObject);
		}
		m_SuccesfullRuns = 0;
		m_Needed = 2;
		m_bDidLeft = false;
		m_bDidRight = false;
		m_bWasInLeft = false;
		m_bWasInRight = false;
		if (m_Safezone1 != null)
		{
			m_Safezone1.sprite = m_SafeZoneIdleSprite;
		}
		if (m_Safezone2 != null)
		{
			m_Safezone2.sprite = m_SafeZoneIdleSprite;
		}
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_CurrentSliderSpeed = Mathf.Sign(m_CurrentSliderSpeed) * m_MasherSettings.m_MarkerStartSpeed;
		m_CurrentThresholdSize = m_MasherSettings.m_ThresholdStartSize;
		SetThresholds();
	}

	private void UpdateSlider()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentSliderValue;
		}
	}

	public void SetMasherSettings(ref PommelMasherSettings settings)
	{
		m_MasherSettings = settings;
		Setup();
	}

	protected override void Setup()
	{
		base.Setup();
		m_SuccesfullRuns = 0;
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_ElapsedStaminaSpentTime = 0f;
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
		m_bWasInLeft = false;
		m_bWasInRight = false;
		m_bDidLeft = false;
		m_bDidRight = false;
		m_Needed = 2;
		m_CurrentThresholdSize = m_MasherSettings.m_ThresholdStartSize;
		SetThresholds();
		m_CurrentSliderSpeed = m_MasherSettings.m_MarkerStartSpeed;
		m_bIgnoreLeftInput = false;
		m_bIgnoreRightInput = false;
	}

	private void SetThresholds(bool safezone1UseHitSprite = false, bool safezone2UseHitSprite = false)
	{
		float width = m_Slider.GetComponent<RectTransform>().rect.width;
		float num = width * m_CurrentThresholdSize;
		if (m_Safezone1 != null)
		{
			Vector2 sizeDelta = m_Safezone1.rectTransform.sizeDelta;
			sizeDelta.x = num;
			m_Threshold1 = num / width;
			m_Safezone1.rectTransform.sizeDelta = sizeDelta;
			m_Safezone1.sprite = ((!safezone1UseHitSprite) ? m_SafeZoneIdleSprite : m_SafeZoneHitSprite);
		}
		if (m_Safezone2 != null)
		{
			Vector2 sizeDelta2 = m_Safezone2.rectTransform.sizeDelta;
			sizeDelta2.x = num;
			m_Threshold2 = (width - num) / width;
			m_Safezone2.rectTransform.sizeDelta = sizeDelta2;
			m_Safezone2.sprite = ((!safezone2UseHitSprite) ? m_SafeZoneIdleSprite : m_SafeZoneHitSprite);
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

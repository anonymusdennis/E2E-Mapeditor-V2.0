using System;
using Rewired;
using UnityEngine;

public class AlternateButtonMasher : MasherBase
{
	public enum MasherState
	{
		Idle,
		Invalid,
		Valid
	}

	[Serializable]
	public struct AlternateMasherSettings
	{
		[Range(0f, 1f)]
		public float m_Threshold1;

		[Range(0f, 1f)]
		public float m_Threshold2;

		public float m_DecayPerSecond;

		public float m_GainPerAlternate;

		public float m_PenaltyWhenWrong;

		public float m_StaminaDrainRate;
	}

	private const float MIN_GAIN_THRESHOLD = 0.001f;

	public T17Text m_Key1;

	public T17Text m_Key2;

	public T17Image m_Key1Image;

	public T17Image m_Key2Image;

	public T17Slider m_Slider;

	public T17Image m_ThresholdMarker1;

	public T17Image m_ThresholdMarker2;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	protected AlternateMasherSettings m_MasherSettings;

	private T17Image m_FillImage;

	private float m_CurrentGain;

	private bool m_bKeyOnePressed;

	private bool m_bKeyTwoPressed;

	private bool m_bIsPositive;

	private bool m_bBackToZero;

	private bool m_bInValidState;

	private bool m_bIsSetup;

	private bool m_bResettingGain;

	private Vector3 m_Key1OrigScale;

	private Vector3 m_Key2OrigScale;

	private Vector3 m_Key1ImageOrigScale;

	private Vector3 m_Key2ImageOrigScale;

	private float m_StaminaDrainTimer;

	private bool m_bStaminaSpentThisFrame;

	protected MasherState m_MasherState;

	[SerializeField]
	public WorldSpaceHudScalePODO m_WorldSpacePositionInfo;

	protected virtual void Awake()
	{
	}

	protected virtual void Update()
	{
		if (m_RewiredPlayer == null || m_Player == null)
		{
			return;
		}
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
		PositionForPlayer();
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
				}
				m_bBackToZero = false;
				ApplyGain();
			}
		}
		UpdateSlider();
		m_bStaminaSpentThisFrame = false;
		if (m_CurrentGain < 0.001f)
		{
			m_MasherState = MasherState.Idle;
			return;
		}
		MasherState masherState = ((!(m_CurrentGain >= m_MasherSettings.m_Threshold1) || !(m_CurrentGain <= m_MasherSettings.m_Threshold2)) ? MasherState.Invalid : MasherState.Valid);
		if (masherState != m_MasherState)
		{
			m_MasherState = masherState;
			if (m_FillImage != null)
			{
				m_FillImage.sprite = ((m_MasherState != MasherState.Valid) ? m_InvalidSprite : m_ValidSprite);
			}
		}
		if (m_MasherSettings.m_StaminaDrainRate > 0f)
		{
			m_StaminaDrainTimer += UpdateManager.deltaTime;
			if (m_StaminaDrainTimer >= m_MasherSettings.m_StaminaDrainRate)
			{
				m_StaminaDrainTimer = 0f;
				m_bStaminaSpentThisFrame = true;
			}
		}
	}

	private void PositionForPlayer()
	{
		if (!(m_Player == null))
		{
			float z = base.transform.position.z;
			Vector3 position = new Vector3(m_Player.m_Transform.position.x, m_Player.m_Transform.position.y - 0.6f, z);
			m_WorldSpacePositionInfo.PositionTransform(base.transform, position, HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_Player.m_PlayerCameraManagerBindingID));
		}
	}

	public override void SetPlayerToCheck(Player player)
	{
		base.SetPlayerToCheck(player);
		if (player != null)
		{
			if (m_Key1 != null)
			{
				m_Key1.SetGamerForEventSystem(m_Player.m_Gamer);
			}
			if (m_Key2 != null)
			{
				m_Key2.SetGamerForEventSystem(m_Player.m_Gamer);
			}
			player.OnMinigameEntered();
			PositionForPlayer();
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (m_Player != null)
		{
			m_Player.OnMinigameExited();
		}
		m_Player = null;
		m_RewiredPlayer = null;
		m_StaminaDrainTimer = 0f;
		m_bStaminaSpentThisFrame = false;
		ResetGain(instant: true);
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

	protected void ResetGain(bool instant = false)
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

	public virtual MasherState GetMasherState()
	{
		return m_MasherState;
	}

	public float CurrentPercentageBeforeFirstThreshold()
	{
		return m_CurrentGain / m_MasherSettings.m_Threshold1;
	}

	public float CurrentPercentageBeforeSecondThreshold()
	{
		return (m_CurrentGain - m_MasherSettings.m_Threshold2) / (1f - m_MasherSettings.m_Threshold2);
	}

	public virtual void SetMasherSettings(ref AlternateMasherSettings settings)
	{
		m_MasherSettings = settings;
		if (!m_bIsSetup)
		{
			Setup();
		}
		SetThresholdSettings();
	}

	private void Setup()
	{
		m_bIsSetup = true;
		Reset();
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
		if (m_Key2 != null)
		{
			m_Key2ImageOrigScale = m_Key2Image.rectTransform.localScale;
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

	public virtual bool StaminaSpent()
	{
		return m_bStaminaSpentThisFrame;
	}
}

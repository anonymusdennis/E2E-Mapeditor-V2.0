using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

public class ReadingMasher : MasherBase, IMinigameMasher
{
	[Serializable]
	public class MasherSettings
	{
		[Range(0.1f, 0.9f)]
		public float m_SafezoneSize;

		public float m_GainIncreasePerSecond;

		public float m_GainDecayPerSecond;

		public float m_ProgressIncreasePerSecond;

		public float m_ProgressDecayPerSecond;

		public float m_ThresholdChangeTime;

		public float m_MinSafezoneChange;

		public float m_MaxSafezoneChange;

		public float m_TimeToReduceStamina;

		public int m_RepsToReachMaxChange;
	}

	private const float AXIS_REGISTER_THRESHOLD = 0.5f;

	private const float MIN_GAIN_THRESHOLD = 0.02f;

	private const float MIN_PROGRESS_THRESHOLD = 0.02f;

	public Vector2 m_CharacterOffset = new Vector2(0f, 0f);

	[SerializeField]
	public WorldSpaceHudScalePODO m_WorldSpacePositionInfo;

	public T17Text m_Key;

	public T17Image m_KeyImage;

	public T17Slider m_Slider;

	public T17Image m_ThresholdMarkerLeft;

	public T17Image m_ThresholdMarkerRight;

	public T17Slider m_ProgressBar;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	public bool m_bSingleButtonMode;

	public string m_SingleButtonId = "PrimaryKey";

	private bool m_bIsGainIncreasing;

	[FormerlySerializedAs("m_bPlayReadingSoundEffects")]
	public bool m_bPlayRepComplimentarySoundEffect;

	public string m_RepComplimentarySoundEffect = "Play_Player_Book_Rep";

	private MasherSettings m_MasherSettings;

	private float m_CurrentGain;

	private bool m_bResettingGain;

	private float m_CurrentProgress;

	private bool m_bResettingProgress;

	private float m_OldLowerThreshold;

	private float m_NewLowerThreshold;

	private float m_LowerThreshold;

	private float m_ElapsedThresholdChangeTime;

	private float m_ElapsedTimeInThreshold;

	private AlternateButtonMasher.MasherState m_MasherState;

	private bool m_bReportRepComplete;

	private bool m_bReportStaminaSpent;

	private int m_SuccessiveReps;

	private T17Image m_FillImage;

	private RectTransform m_SliderTransform;

	private Transform m_Transform;

	private void Awake()
	{
		m_Transform = GetComponent<Transform>();
		if (m_Slider != null)
		{
			m_SliderTransform = m_Slider.GetComponent<RectTransform>();
			m_FillImage = m_Slider.fillRect.GetComponent<T17Image>();
		}
		if (m_FillImage != null)
		{
			m_FillImage.sprite = m_InvalidSprite;
		}
	}

	public void SetupMasher(Player player, MasherSettings settings)
	{
		Reset();
		SetPlayerToCheck(player);
		m_MasherSettings = settings;
		if (m_Key != null)
		{
			m_Key.SetGamerForEventSystem(player.m_Gamer);
		}
		float min = 0f;
		float max = 1f - m_MasherSettings.m_SafezoneSize;
		m_LowerThreshold = UnityEngine.Random.Range(min, max);
		m_OldLowerThreshold = m_LowerThreshold;
		m_NewLowerThreshold = m_LowerThreshold;
		m_ElapsedThresholdChangeTime = m_MasherSettings.m_ThresholdChangeTime + 1f;
		UpdateVizualization();
		PositionForPlayer();
	}

	private void Update()
	{
		PositionForPlayer();
		m_bReportRepComplete = false;
		m_bReportStaminaSpent = false;
		UpdateKeyInput();
		UpdateMasherState();
		UpdateProgress();
		UpdateThreshold();
		UpdateVizualization();
	}

	private void PositionForPlayer()
	{
		if (m_Player != null && ((m_RewiredPlayer != null) & (m_Transform != null)))
		{
			Vector3 position = m_Transform.position;
			position.x = m_Player.m_Transform.position.x + m_CharacterOffset.x;
			position.y = m_Player.m_Transform.position.y + m_CharacterOffset.y;
			m_WorldSpacePositionInfo.PositionTransform(base.transform, position, HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_Player.m_PlayerCameraManagerBindingID));
		}
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
			return;
		}
		float f = ((!m_bSingleButtonMode) ? m_RewiredPlayer.GetAxis("Alternate_Key2") : ((!m_RewiredPlayer.GetButton(m_SingleButtonId)) ? 0f : 1f));
		float num = Mathf.Abs(f);
		if (num > 0.5f)
		{
			float num2 = (num - 0.5f) / 0.5f;
			m_CurrentGain = Mathf.Clamp(m_CurrentGain + m_MasherSettings.m_GainIncreasePerSecond * num2 * UpdateManager.deltaTime, 0f, 1f);
			m_bIsGainIncreasing = true;
		}
		else
		{
			float num3 = (0.5f - num) / 0.5f;
			m_CurrentGain = Mathf.Clamp(m_CurrentGain - m_MasherSettings.m_GainDecayPerSecond * num3 * UpdateManager.deltaTime, 0f, 1f);
		}
	}

	private void UpdateMasherState()
	{
		AlternateButtonMasher.MasherState masherState = AlternateButtonMasher.MasherState.Idle;
		if (m_CurrentGain > 0.02f)
		{
			masherState = ((!(m_CurrentGain >= m_LowerThreshold) || !(m_CurrentGain <= m_LowerThreshold + m_MasherSettings.m_SafezoneSize)) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid);
		}
		if (masherState != m_MasherState)
		{
			if (m_MasherState == AlternateButtonMasher.MasherState.Valid && masherState == AlternateButtonMasher.MasherState.Invalid)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Fail, m_Player.gameObject);
			}
			m_MasherState = masherState;
			if (m_FillImage != null)
			{
				m_FillImage.sprite = ((m_MasherState != AlternateButtonMasher.MasherState.Valid) ? m_InvalidSprite : m_ValidSprite);
			}
		}
		if (m_MasherState == AlternateButtonMasher.MasherState.Valid)
		{
			m_ElapsedTimeInThreshold += UpdateManager.deltaTime;
			if (m_ElapsedTimeInThreshold > m_MasherSettings.m_TimeToReduceStamina)
			{
				m_ElapsedTimeInThreshold = 0f;
				m_bReportStaminaSpent = true;
			}
		}
		else
		{
			m_SuccessiveReps = 0;
		}
	}

	private void UpdateProgress()
	{
		if (m_bResettingProgress)
		{
			m_CurrentProgress = Mathf.Lerp(m_CurrentProgress, 0f, UpdateManager.deltaTime * (5f * (1.1f - m_CurrentProgress)));
			if (m_CurrentProgress < 0.02f)
			{
				m_CurrentProgress = 0f;
				m_bResettingProgress = false;
			}
			return;
		}
		float currentProgress = m_CurrentProgress;
		currentProgress = ((m_MasherState != AlternateButtonMasher.MasherState.Valid) ? (m_CurrentProgress - m_MasherSettings.m_ProgressDecayPerSecond * UpdateManager.deltaTime) : (m_CurrentProgress + m_MasherSettings.m_ProgressIncreasePerSecond * UpdateManager.deltaTime));
		m_CurrentProgress = Mathf.Clamp(currentProgress, 0f, 1f);
		if (m_CurrentProgress >= 1f)
		{
			m_bReportRepComplete = true;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, m_Player.gameObject);
			if (m_bPlayRepComplimentarySoundEffect && !string.IsNullOrEmpty(m_RepComplimentarySoundEffect))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_RepComplimentarySoundEffect, m_Player.gameObject);
			}
			m_SuccessiveReps++;
			ResetProgress();
		}
	}

	private void UpdateThreshold()
	{
		m_ElapsedThresholdChangeTime += UpdateManager.deltaTime;
		if (m_ElapsedThresholdChangeTime >= m_MasherSettings.m_ThresholdChangeTime)
		{
			m_ElapsedThresholdChangeTime = 0f;
			float min = 0f;
			float max = 1f - m_MasherSettings.m_SafezoneSize;
			float max2 = Mathf.Clamp01((float)m_SuccessiveReps / (float)m_MasherSettings.m_RepsToReachMaxChange) * m_MasherSettings.m_MaxSafezoneChange;
			float num = m_MasherSettings.m_MinSafezoneChange + UnityEngine.Random.Range(0f, max2);
			bool flag = UnityEngine.Random.value > 0.5f;
			float value = m_LowerThreshold + ((!flag) ? (0f - num) : num);
			value = Mathf.Clamp(value, min, max);
			m_OldLowerThreshold = m_LowerThreshold;
			m_NewLowerThreshold = value;
		}
		m_LowerThreshold = Mathf.Lerp(m_OldLowerThreshold, m_NewLowerThreshold, m_ElapsedThresholdChangeTime / m_MasherSettings.m_ThresholdChangeTime);
	}

	private void UpdateVizualization()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentGain;
		}
		if (m_ProgressBar != null)
		{
			m_ProgressBar.value = m_CurrentProgress;
		}
		if (m_SliderTransform != null && m_ThresholdMarkerLeft != null && m_ThresholdMarkerRight != null)
		{
			float xPos = m_SliderTransform.rect.width * m_LowerThreshold;
			float xPos2 = m_SliderTransform.rect.width * (m_LowerThreshold + m_MasherSettings.m_SafezoneSize);
			SetThresholdMarkerPosition(m_ThresholdMarkerLeft, xPos);
			SetThresholdMarkerPosition(m_ThresholdMarkerRight, xPos2);
		}
	}

	private void SetThresholdMarkerPosition(T17Image marker, float xPos)
	{
		if (marker != null)
		{
			Vector2 anchoredPosition = marker.rectTransform.anchoredPosition;
			anchoredPosition.x = xPos;
			marker.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public override void Reset()
	{
		base.Reset();
		ResetGain(instant: true);
		ResetProgress(instant: true);
		m_bReportRepComplete = false;
		m_bReportStaminaSpent = false;
		m_SuccessiveReps = 0;
		m_LowerThreshold = 0f;
		m_OldLowerThreshold = 0f;
		m_NewLowerThreshold = 0f;
		m_ElapsedTimeInThreshold = 0f;
		m_ElapsedThresholdChangeTime = 0f;
		m_MasherSettings = null;
		m_RewiredPlayer = null;
		m_Player = null;
	}

	private void ResetGain(bool instant = false)
	{
		if (instant)
		{
			m_CurrentGain = 0f;
		}
		else
		{
			m_bResettingGain = true;
		}
	}

	private void ResetProgress(bool instant = false)
	{
		if (instant)
		{
			m_CurrentProgress = 0f;
		}
		else
		{
			m_bResettingProgress = true;
		}
	}

	public bool IsGainIncreasing()
	{
		return m_bIsGainIncreasing;
	}

	public AlternateButtonMasher.MasherState GetMasherState()
	{
		return m_MasherState;
	}

	public bool GetHasCompletedRep()
	{
		return m_bReportRepComplete;
	}

	public bool GetShouldExpendStamina()
	{
		return m_bReportStaminaSpent;
	}

	public bool HasCompletedRep()
	{
		return GetHasCompletedRep();
	}

	public void EnableForPlayer(Player thePlayer)
	{
		base.gameObject.SetActive(value: true);
		m_Player = thePlayer;
		m_RewiredPlayer = m_Player.m_Gamer.m_RewiredPlayer;
		if (m_Key != null)
		{
			m_Key.SetGamerForEventSystem(thePlayer.m_Gamer);
		}
		SetMasherSettings(m_MasherSettings);
		PositionForPlayer();
	}

	public bool IsEnabled()
	{
		return base.gameObject.activeSelf;
	}

	public void SetMasherSettings(MasherSettings settings)
	{
		m_MasherSettings = settings;
		float min = 0f;
		float max = 1f - m_MasherSettings.m_SafezoneSize;
		m_LowerThreshold = UnityEngine.Random.Range(min, max);
		m_OldLowerThreshold = m_LowerThreshold;
		m_NewLowerThreshold = m_LowerThreshold;
		m_ElapsedThresholdChangeTime = m_MasherSettings.m_ThresholdChangeTime + 1f;
		UpdateVizualization();
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public bool IsSignificantMomentInMinigame()
	{
		return m_bReportStaminaSpent;
	}
}

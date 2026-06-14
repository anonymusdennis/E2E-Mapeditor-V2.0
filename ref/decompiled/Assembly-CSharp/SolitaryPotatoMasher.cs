using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class SolitaryPotatoMasher : MonoBehaviour, IMinigameMasher
{
	[Serializable]
	public class MasherSettings
	{
		[Range(0.1f, 0.45f)]
		public float m_ThresholdStartSize;

		public float m_ThresholdMinSize;

		public float m_ThresholdSizeDecrease;

		public float m_MarkerStartSpeed;

		public float m_MarkerMaxSpeed;

		public float m_MarkerMinSpeed;

		public int m_PressesPerRep;

		public float m_TimeToReduceStamina;

		public bool IsValid()
		{
			return m_PressesPerRep != 0 && m_ThresholdStartSize != 0f && m_MarkerStartSpeed != 0f;
		}
	}

	public enum SliderState
	{
		Center,
		Left,
		Right
	}

	private const float AXIS_REGISTER_THRESHOLD = 0.5f;

	public Vector2 m_CharacterOffset = new Vector2(0f, 0f);

	[SerializeField]
	public WorldSpaceHudScalePODO m_WorldSpacePositionInfo;

	public T17Text m_LowerKey;

	public T17Text m_UpperKey;

	public T17Image m_LowerKeyImage;

	public T17Image m_UpperKeyImage;

	public T17Slider m_Slider;

	public T17Image m_LowerSafezone;

	public T17Image m_UpperSafezone;

	public T17Text m_SuccessiveRepsText;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	private Vector3 m_UpperKeyOriginalScale;

	private Vector3 m_LowerKeyOriginalScale;

	private Vector3 m_UpperKeyImageOriginalScale;

	private Vector3 m_LowerKeyImageOriginalScale;

	private const float kHighlighScale = 1.3f;

	public bool m_bSingleButtonMode;

	public string m_SingleButtonId = "PrimaryKey";

	public string m_FailRepSoundEffect = "Play_Player_Rep_Fail";

	public bool m_bPlayComplimentarySuccessSound;

	public string m_ComplimentarySuccessSound;

	private MasherSettings m_MasherSettings;

	private float m_CurrentSliderSpeed;

	private float m_CurrentSliderValue;

	private float m_LowerThreshold;

	private float m_UpperThreshold;

	private float m_CurrentThresholdSize;

	private SliderState m_SliderState;

	private SliderState m_PrevSliderState;

	private SliderState m_NextPressState;

	private int m_SuccessivePresses;

	private bool m_bPressedTooEarly;

	private float m_ElapsedStaminaSpentTime;

	private bool m_bReportRepComplete;

	private bool m_bReportStaminaSpent;

	private int m_SuccessiveReps;

	private Player m_Player;

	private Rewired.Player m_RewiredPlayer;

	private RectTransform m_SliderTransform;

	private Transform m_Transform;

	private void Awake()
	{
		m_Transform = GetComponent<Transform>();
		if (m_Slider != null)
		{
			m_SliderTransform = m_Slider.GetComponent<RectTransform>();
		}
		m_UpperKeyOriginalScale = Vector3.one;
		m_LowerKeyOriginalScale = Vector3.one;
		m_UpperKeyImageOriginalScale = Vector3.one;
		m_LowerKeyImageOriginalScale = Vector3.one;
	}

	public void SetupMasher(Player player, MasherSettings settings)
	{
		Reset();
		m_Player = player;
		m_RewiredPlayer = m_Player.m_Gamer.m_RewiredPlayer;
		m_MasherSettings = settings;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(player.m_Gamer);
		if (m_LowerKey != null)
		{
			m_LowerKey.SetGamerForEventSystem(player.m_Gamer, eventSystemForGamer);
		}
		if (m_UpperKey != null)
		{
			m_UpperKey.SetGamerForEventSystem(player.m_Gamer, eventSystemForGamer);
		}
		m_CurrentSliderValue = 0.5f;
		m_NextPressState = SliderState.Right;
		SetMasherSettings(m_MasherSettings);
		PositionForPlayer();
	}

	private void Update()
	{
		if (!(m_Player == null))
		{
			PositionForPlayer();
			m_bReportRepComplete = false;
			m_bReportStaminaSpent = false;
			UpdateKeyInput();
			UpdateMasherState();
			UpdateThresholds();
			UpdateVizualization();
		}
	}

	private void PositionForPlayer()
	{
		if (m_Player != null && m_RewiredPlayer != null)
		{
			Vector3 position = m_Transform.position;
			position.x = m_Player.m_Transform.position.x + m_CharacterOffset.x;
			position.y = m_Player.m_Transform.position.y + m_CharacterOffset.y;
			m_WorldSpacePositionInfo.PositionTransform(base.transform, position, HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_Player.m_PlayerCameraManagerBindingID));
		}
	}

	private void UpdateKeyInput()
	{
		if (m_RewiredPlayer == null)
		{
			return;
		}
		Controller lastActiveController = m_RewiredPlayer.controllers.GetLastActiveController();
		if (lastActiveController != null)
		{
			if (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Mouse)
			{
				if (m_LowerKeyImage != null)
				{
					m_LowerKeyImage.enabled = true;
				}
				if (m_UpperKeyImage != null)
				{
					m_UpperKeyImage.enabled = true;
				}
			}
			else
			{
				if (m_LowerKeyImage != null)
				{
					m_LowerKeyImage.enabled = false;
				}
				if (m_UpperKeyImage != null)
				{
					m_UpperKeyImage.enabled = false;
				}
			}
		}
		m_PrevSliderState = m_SliderState;
		m_CurrentSliderValue += m_CurrentSliderSpeed * UpdateManager.deltaTime;
		if (m_CurrentSliderValue < 0f || m_CurrentSliderValue > 1f)
		{
			m_CurrentSliderSpeed = 0f - m_CurrentSliderSpeed;
			m_CurrentSliderValue = Mathf.Clamp01(m_CurrentSliderValue);
		}
		if (m_CurrentSliderValue <= m_LowerThreshold)
		{
			m_SliderState = SliderState.Left;
		}
		else if (m_CurrentSliderValue >= m_UpperThreshold)
		{
			m_SliderState = SliderState.Right;
		}
		else
		{
			m_SliderState = SliderState.Center;
		}
		bool flag = m_RewiredPlayer.GetAxis("Alternate_Key1") > 0.5f;
		bool flag2 = m_RewiredPlayer.GetAxis("Alternate_Key2") < -0.5f;
		if (m_bSingleButtonMode)
		{
			bool buttonDown = m_RewiredPlayer.GetButtonDown(m_SingleButtonId);
			flag = buttonDown;
			flag2 = buttonDown;
		}
		if (m_SliderState == m_NextPressState)
		{
			if ((m_SliderState == SliderState.Left && flag) || (m_SliderState == SliderState.Right && flag2))
			{
				m_SuccessivePresses++;
				NextTargetState();
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
				if (m_bPlayComplimentarySuccessSound && !string.IsNullOrEmpty(m_ComplimentarySuccessSound))
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_ComplimentarySuccessSound, m_Player.gameObject);
				}
			}
		}
		else if (m_SliderState == SliderState.Center && ((m_NextPressState == SliderState.Left && flag) || (m_NextPressState == SliderState.Right && flag2)))
		{
			m_bPressedTooEarly = true;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_FailRepSoundEffect, m_Player.gameObject);
		}
	}

	private void UpdateMasherState()
	{
		if (m_bPressedTooEarly || (m_PrevSliderState != m_SliderState && m_PrevSliderState == m_NextPressState))
		{
			m_SuccessivePresses = 0;
			m_SuccessiveReps = 0;
			NextTargetState();
			m_CurrentSliderSpeed = Mathf.Sign(m_CurrentSliderSpeed) * m_MasherSettings.m_MarkerStartSpeed;
			m_CurrentThresholdSize = m_MasherSettings.m_ThresholdStartSize;
			SetButtonHighlightScale(SliderState.Right);
		}
		if (m_SuccessivePresses >= m_MasherSettings.m_PressesPerRep)
		{
			m_SuccessiveReps++;
			m_bReportRepComplete = true;
			m_SuccessivePresses = 0;
			m_CurrentSliderSpeed = Mathf.Sign(m_CurrentSliderSpeed) * UnityEngine.Random.Range(m_MasherSettings.m_MarkerMinSpeed, m_MasherSettings.m_MarkerMaxSpeed);
			m_CurrentThresholdSize = Mathf.Clamp(m_CurrentThresholdSize - m_MasherSettings.m_ThresholdSizeDecrease, m_MasherSettings.m_ThresholdMinSize, m_MasherSettings.m_ThresholdStartSize);
		}
		if (m_SuccessivePresses > 0)
		{
			m_ElapsedStaminaSpentTime += UpdateManager.deltaTime;
			if (m_ElapsedStaminaSpentTime >= m_MasherSettings.m_TimeToReduceStamina)
			{
				m_ElapsedStaminaSpentTime = 0f;
				m_bReportStaminaSpent = true;
			}
		}
	}

	private void NextTargetState()
	{
		if (m_NextPressState == SliderState.Right)
		{
			m_NextPressState = SliderState.Left;
		}
		else
		{
			m_NextPressState = SliderState.Right;
		}
		SetButtonHighlightScale(m_NextPressState);
		m_bPressedTooEarly = false;
	}

	private void UpdateThresholds()
	{
		if (m_SliderTransform != null)
		{
			float width = m_SliderTransform.rect.width;
			float num = width * m_CurrentThresholdSize;
			m_LowerThreshold = num / width;
			m_UpperThreshold = (width - num) / width;
		}
	}

	private void UpdateVizualization()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentSliderValue;
		}
		if (m_SuccessiveRepsText != null)
		{
			m_SuccessiveRepsText.text = m_SuccessiveReps.ToString();
		}
		if (m_SliderTransform != null && m_LowerSafezone != null && m_UpperSafezone != null)
		{
			float width = m_SliderTransform.rect.width * m_CurrentThresholdSize;
			SetThresholdMarkerWidth(m_LowerSafezone, width);
			SetThresholdMarkerWidth(m_UpperSafezone, width);
		}
		if (m_LowerSafezone != null)
		{
			m_LowerSafezone.sprite = ((m_NextPressState != SliderState.Left) ? m_ValidSprite : m_InvalidSprite);
		}
		if (m_UpperSafezone != null)
		{
			m_UpperSafezone.sprite = ((m_NextPressState != SliderState.Right) ? m_ValidSprite : m_InvalidSprite);
		}
	}

	private void SetThresholdMarkerWidth(T17Image marker, float width)
	{
		if (marker != null)
		{
			Vector2 sizeDelta = marker.rectTransform.sizeDelta;
			sizeDelta.x = width;
			marker.rectTransform.sizeDelta = sizeDelta;
		}
	}

	public void Reset()
	{
		m_CurrentSliderSpeed = 0f;
		m_CurrentSliderValue = 0f;
		m_CurrentThresholdSize = 0f;
		m_SliderState = SliderState.Center;
		m_PrevSliderState = SliderState.Center;
		m_NextPressState = SliderState.Center;
		m_bReportRepComplete = false;
		m_bReportStaminaSpent = false;
		m_SuccessivePresses = 0;
		m_SuccessiveReps = 0;
		m_ElapsedStaminaSpentTime = 0f;
		m_bPressedTooEarly = false;
		m_MasherSettings = null;
		m_RewiredPlayer = null;
		m_Player = null;
		SetButtonHighlightScale(SliderState.Right);
	}

	public SliderState GetSliderState()
	{
		return m_SliderState;
	}

	private void SetButtonHighlightScale(SliderState sliderState)
	{
		if (m_LowerKey != null)
		{
			bool flag = sliderState == SliderState.Center || sliderState == SliderState.Left;
			m_LowerKey.transform.localScale = ((!flag) ? m_UpperKeyOriginalScale : (m_UpperKeyOriginalScale * 1.3f));
			if (m_LowerKeyImage != null)
			{
				m_LowerKeyImage.transform.localScale = ((!flag) ? m_UpperKeyImageOriginalScale : (m_UpperKeyImageOriginalScale * 1.3f));
			}
		}
		if (m_UpperKey != null)
		{
			bool flag2 = sliderState == SliderState.Center || sliderState == SliderState.Right;
			m_UpperKey.transform.localScale = ((!flag2) ? m_LowerKeyOriginalScale : (m_LowerKeyOriginalScale * 1.3f));
			if (m_UpperKeyImage != null)
			{
				m_UpperKeyImage.transform.localScale = ((!flag2) ? m_LowerKeyImageOriginalScale : (m_LowerKeyImageOriginalScale * 1.3f));
			}
		}
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

	public bool IsEnabled()
	{
		return base.gameObject.activeSelf;
	}

	public void EnableForPlayer(Player thePlayer)
	{
		base.gameObject.SetActive(value: true);
		m_Player = thePlayer;
		m_RewiredPlayer = m_Player.m_Gamer.m_RewiredPlayer;
		if (m_LowerKey != null)
		{
			m_LowerKey.SetGamerForEventSystem(thePlayer.m_Gamer);
		}
		if (m_UpperKey != null)
		{
			m_UpperKey.SetGamerForEventSystem(thePlayer.m_Gamer);
		}
		m_CurrentSliderValue = 0.5f;
		m_NextPressState = SliderState.Right;
		PositionForPlayer();
	}

	public void SetMasherSettings(MasherSettings settings)
	{
		m_MasherSettings = settings;
		m_CurrentSliderSpeed = m_MasherSettings.m_MarkerStartSpeed;
		m_CurrentThresholdSize = m_MasherSettings.m_ThresholdStartSize;
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

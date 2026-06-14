using System;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class GymMasher_Pullup : GymMasherBase
{
	[Serializable]
	public struct PullupMasherSettings
	{
		[Range(0.1f, 0.45f)]
		public float m_ThresholdStartSize;

		public float m_ThresholdMinSize;

		public float m_ThresholdDecreaseSize;

		public float m_ThresholdMinMove;

		public float m_ThresholdMaxMove;

		public float m_MarkerSpeed;

		public float m_StaminaSpendTimeInterval;
	}

	private enum InteractionButton
	{
		Any,
		Key1,
		Key2
	}

	private const float AXIS_REGISTER_THRESHOLD = 0.5f;

	[Header("GymMasher_Pullup")]
	public T17Text m_Key1;

	public T17Text m_Key2;

	public T17Image m_ThresholdMarkerLeft;

	public T17Image m_ThresholdMarkerRight;

	public T17Slider m_Slider;

	public T17Text m_DistanceValue;

	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	public GameObject m_Key1GameObject;

	public GameObject m_Key2GameObject;

	public T17Image m_Key1Image;

	public T17Image m_Key2Image;

	private Vector3 m_Key1OriginalScale;

	private Vector3 m_Key2OriginalScale;

	private Vector3 m_Key1ImageOriginalScale;

	private Vector3 m_Key2ImageOriginalScale;

	private const float kHighlighScale = 1.3f;

	private PullupMasherSettings m_MasherSettings;

	private float m_CurrentSliderSpeed = 1f;

	private float m_CurrentSliderValue;

	private bool m_bButtonsPressed;

	private bool m_bWasInThreshold;

	private bool m_bDidThreshold;

	private float m_ThresholdLeft;

	private float m_ThresholdRight;

	private bool m_bReportSuccess;

	private bool m_bReportStaminaSpent;

	private int m_SuccesfullRuns;

	private int m_Needed = 1;

	private InteractionButton m_ExpectingButtonPress;

	private bool m_bKeyOnePressed;

	private bool m_bKeyTwoPressed;

	private RectTransform m_SliderTransform;

	private T17Image m_FillImage;

	protected override void Awake()
	{
		base.Awake();
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

	public void SetMasherSettings(ref PullupMasherSettings settings)
	{
		m_MasherSettings = settings;
		Setup();
	}

	protected override void Setup()
	{
		m_Key1OriginalScale = Vector3.one;
		m_Key2OriginalScale = Vector3.one;
		m_Key1ImageOriginalScale = Vector3.one;
		m_Key2ImageOriginalScale = Vector3.one;
		base.Setup();
		Reset();
		float thresholdStartSize = m_MasherSettings.m_ThresholdStartSize;
		float min = thresholdStartSize;
		float max = 1f;
		m_ThresholdRight = m_ThresholdLeft + thresholdStartSize;
		m_ThresholdRight = Mathf.Clamp(m_ThresholdRight, min, max);
		m_CurrentSliderSpeed = m_MasherSettings.m_MarkerSpeed;
	}

	protected override void Update()
	{
		base.Update();
		m_bReportSuccess = false;
		m_bReportStaminaSpent = false;
		UpdateKeyInput();
		UpdateVisualisation();
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
		float currentSliderValue = m_CurrentSliderValue;
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
		bool bButtonsPressed = m_bButtonsPressed;
		bool flag = false;
		bool flag2 = false;
		float num = m_RewiredPlayer.GetAxis("Alternate_Key1") + m_RewiredPlayer.GetAxis("Alternate_Key2");
		if (num > 0.5f)
		{
			if (!m_bKeyOnePressed)
			{
				flag = true;
			}
			m_bKeyOnePressed = true;
		}
		else
		{
			m_bKeyOnePressed = false;
		}
		if (num < -0.5f)
		{
			if (!m_bKeyTwoPressed)
			{
				flag2 = true;
			}
			m_bKeyTwoPressed = true;
		}
		else
		{
			m_bKeyTwoPressed = false;
		}
		switch (m_ExpectingButtonPress)
		{
		case InteractionButton.Any:
			m_bButtonsPressed = flag || flag2;
			break;
		case InteractionButton.Key1:
			m_bButtonsPressed = flag;
			break;
		case InteractionButton.Key2:
			m_bButtonsPressed = flag2;
			break;
		}
		if (m_CurrentSliderValue >= m_ThresholdLeft && m_CurrentSliderValue <= m_ThresholdRight)
		{
			if (currentSliderValue < m_ThresholdLeft || currentSliderValue > m_ThresholdRight)
			{
				m_bWasInThreshold = true;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Hit, m_Player.gameObject);
			}
			if (m_bWasInThreshold)
			{
				if (m_bButtonsPressed && !bButtonsPressed)
				{
					m_bDidThreshold = true;
				}
				if (m_bDidThreshold)
				{
					m_Needed--;
					if (m_Needed == 0)
					{
						ReportSucces();
					}
					SetExpectingButtonPress((!flag) ? InteractionButton.Key1 : InteractionButton.Key2);
				}
			}
		}
		else if (m_bWasInThreshold && !m_bDidThreshold)
		{
			Fail();
		}
		else if (flag || flag2)
		{
			Fail();
		}
		if (flag || flag2)
		{
			m_bReportStaminaSpent = true;
		}
	}

	private void ReportSucces()
	{
		m_SuccesfullRuns++;
		m_Needed = 1;
		m_bDidThreshold = false;
		m_bWasInThreshold = false;
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_bReportSuccess = true;
		float value = m_ThresholdRight - m_ThresholdLeft - m_MasherSettings.m_ThresholdDecreaseSize;
		value = Mathf.Clamp(value, m_MasherSettings.m_ThresholdMinSize, m_MasherSettings.m_ThresholdStartSize);
		float min = 0f;
		float max = 1f - value;
		float thresholdMaxMove = m_MasherSettings.m_ThresholdMaxMove;
		float num = m_MasherSettings.m_ThresholdMinMove + UnityEngine.Random.Range(0f, thresholdMaxMove);
		bool flag = UnityEngine.Random.value > 0.5f;
		m_ThresholdLeft += ((!flag) ? (0f - num) : num);
		m_ThresholdLeft = Mathf.Clamp(m_ThresholdLeft, min, max);
		min = value;
		max = 1f;
		m_ThresholdRight = m_ThresholdLeft + value;
		m_ThresholdRight = Mathf.Clamp(m_ThresholdRight, min, max);
	}

	private void Fail()
	{
		bool flag = m_SuccesfullRuns != 0;
		if (flag)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Fail, m_Player.gameObject);
		}
		m_SuccesfullRuns = 0;
		m_Needed = 1;
		m_bDidThreshold = false;
		m_bWasInThreshold = false;
		SetExpectingButtonPress(InteractionButton.Key2);
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		m_CurrentSliderSpeed = Mathf.Sign(m_CurrentSliderSpeed) * m_MasherSettings.m_MarkerSpeed;
		if (flag)
		{
			float thresholdStartSize = m_MasherSettings.m_ThresholdStartSize;
			thresholdStartSize = Mathf.Clamp(thresholdStartSize, m_MasherSettings.m_ThresholdMinSize, m_MasherSettings.m_ThresholdStartSize);
			float min = 0f;
			float max = 1f - thresholdStartSize;
			float thresholdMaxMove = m_MasherSettings.m_ThresholdMaxMove;
			float num = m_MasherSettings.m_ThresholdMinMove + UnityEngine.Random.Range(0f, thresholdMaxMove);
			bool flag2 = UnityEngine.Random.value > 0.5f;
			m_ThresholdLeft += ((!flag2) ? (0f - num) : num);
			m_ThresholdLeft = Mathf.Clamp(m_ThresholdLeft, min, max);
			min = thresholdStartSize;
			max = 1f;
			m_ThresholdRight = m_ThresholdLeft + thresholdStartSize;
			m_ThresholdRight = Mathf.Clamp(m_ThresholdRight, min, max);
		}
	}

	private void UpdateVisualisation()
	{
		if (m_Slider != null)
		{
			m_Slider.value = m_CurrentSliderValue;
		}
		if (m_FillImage != null)
		{
			m_FillImage.sprite = ((!(m_CurrentSliderValue >= m_ThresholdLeft) || !(m_CurrentSliderValue <= m_ThresholdRight)) ? m_InvalidSprite : m_ValidSprite);
		}
		if (m_DistanceValue != null)
		{
			m_DistanceValue.text = m_SuccesfullRuns.ToString();
		}
		if (m_SliderTransform != null && m_ThresholdMarkerLeft != null && m_ThresholdMarkerRight != null)
		{
			float xPos = m_SliderTransform.rect.width * m_ThresholdLeft;
			float xPos2 = m_SliderTransform.rect.width * m_ThresholdRight;
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
		m_SuccesfullRuns = 0;
		m_bButtonsPressed = false;
		m_bWasInThreshold = false;
		m_bDidThreshold = false;
		m_Needed = 1;
		m_ThresholdLeft = 0.5f;
		m_ThresholdRight = 0.5f;
		m_CurrentSliderSpeed = 0f;
		SetExpectingButtonPress(InteractionButton.Key2);
	}

	public override void SetPlayerToCheck(Player player)
	{
		base.SetPlayerToCheck(player);
		if (m_Key1 != null)
		{
			m_Key1.SetGamerForEventSystem(player.m_Gamer);
		}
		if (m_Key2 != null)
		{
			m_Key2.SetGamerForEventSystem(player.m_Gamer);
		}
	}

	public override AlternateButtonMasher.MasherState GetMasherState()
	{
		return (!m_bReportSuccess) ? AlternateButtonMasher.MasherState.Invalid : AlternateButtonMasher.MasherState.Valid;
	}

	public override bool StaminaSpent()
	{
		return m_bReportStaminaSpent;
	}

	private void SetExpectingButtonPress(InteractionButton buttonType)
	{
		m_ExpectingButtonPress = buttonType;
		if (m_Key1GameObject != null)
		{
			bool flag = m_ExpectingButtonPress == InteractionButton.Any || m_ExpectingButtonPress == InteractionButton.Key1;
			m_Key1GameObject.transform.localScale = ((!flag) ? m_Key1OriginalScale : (m_Key1OriginalScale * 1.3f));
			if (m_Key1Image != null)
			{
				m_Key1Image.rectTransform.localScale = ((!flag) ? m_Key1ImageOriginalScale : (m_Key1ImageOriginalScale * 1.3f));
			}
		}
		if (m_Key2GameObject != null)
		{
			bool flag2 = m_ExpectingButtonPress == InteractionButton.Any || m_ExpectingButtonPress == InteractionButton.Key2;
			m_Key2GameObject.transform.localScale = ((!flag2) ? m_Key2OriginalScale : (m_Key2OriginalScale * 1.3f));
			if (m_Key2Image != null)
			{
				m_Key2Image.rectTransform.localScale = ((!flag2) ? m_Key2ImageOriginalScale : (m_Key2ImageOriginalScale * 1.3f));
			}
		}
	}
}

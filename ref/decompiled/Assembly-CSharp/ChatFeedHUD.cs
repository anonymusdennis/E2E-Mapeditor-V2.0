using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ChatFeedHUD : BaseMenuBehaviour
{
	public ScrollRect m_ScrollRect;

	public T17InputField m_InputField;

	public GameObject m_InputFieldBacker;

	private List<T17Text> m_TextLinePool = new List<T17Text>();

	public T17Text m_TextLinePrefab;

	public int m_MaxTextLines = 16;

	public float m_TimeToBeginFadeDown = 5f;

	private float m_FadeDownStartTime;

	public float m_FadeDownDuration = 3f;

	private float m_FadingDownTime;

	public float m_FadeUpDuration = 1f;

	private float m_FadingUpTime;

	private bool m_bFadeDownTimer;

	private bool m_bFadeUpTimer;

	private bool m_bPointerOver;

	private bool m_bIsSelected;

	private bool m_bRemoveHUDItemOnPointerUp;

	private int m_FrameDeselected;

	private List<int> m_HeldMouseButtons = new List<int>();

	private float m_CurrentScrollBarPos;

	private Scrollbar m_VerticalScrollbar;

	[Range(0f, 1f)]
	public float m_FadeTransparency;

	private int m_NextTextLine;

	public CanvasGroup m_CanvasGroup;

	private ChatFeedManager m_ChatFeedManager;

	private FastList<string> m_FeedQueue = new FastList<string>();

	protected override void Awake()
	{
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		for (int i = 0; i < m_MaxTextLines; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_TextLinePrefab.gameObject);
			T17Text component = gameObject.GetComponent<T17Text>();
			if (component != null)
			{
				component.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
				component.name = "Chat Feed Entry " + i;
				component.gameObject.SetActive(value: false);
				m_TextLinePool.Add(component);
			}
		}
		if (m_InputField != null)
		{
			m_InputField.onConfirmValue.AddListener(InputSubmitted);
			m_InputField.onEndEdit.AddListener(InputEnded);
			T17InputField inputField = m_InputField;
			inputField.FieldSelectedEvent = (T17InputField.SelectedHandler)Delegate.Combine(inputField.FieldSelectedEvent, new T17InputField.SelectedHandler(InputFieldSelected));
			T17InputField inputField2 = m_InputField;
			inputField2.OnInputFieldPointerEnter = (T17InputField.PointerHandler)Delegate.Combine(inputField2.OnInputFieldPointerEnter, new T17InputField.PointerHandler(OnPointerEnter));
			T17InputField inputField3 = m_InputField;
			inputField3.OnInputFieldPointerExit = (T17InputField.PointerHandler)Delegate.Combine(inputField3.OnInputFieldPointerExit, new T17InputField.PointerHandler(OnPointerExit));
			T17InputField inputField4 = m_InputField;
			inputField4.OnInputFieldDeselect = (T17InputField.PointerHandler)Delegate.Combine(inputField4.OnInputFieldDeselect, new T17InputField.PointerHandler(OnInputFieldDeselect));
			m_InputField.m_bSelectAllOnFocus = false;
			m_InputField.m_bMoveCaretToEndOnFocus = true;
			T17Button t17Button = m_InputField.GetComponent<T17Button>();
			if (t17Button == null)
			{
				t17Button = m_InputField.GetComponentInParent<T17Button>();
			}
			if (t17Button != null)
			{
				t17Button.m_CanUIReselectDelegate = m_InputField.CanReselectOnMouseDisable;
				t17Button.m_ReleaseOnPointerClickDelegate = m_InputField.ReleaseSelectionOnPointerClickOrExit;
			}
			T17Text t17Text = m_InputField.GetComponent<T17Text>();
			if (t17Text == null)
			{
				t17Text = m_InputField.GetComponentInParent<T17Text>();
			}
			if (t17Text != null)
			{
				t17Text.m_ReleaseOnPointerClickDelegate = m_InputField.ReleaseSelectionOnPointerClickOrExit;
			}
		}
		if (m_CanvasGroup == null)
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
		}
		if (m_CanvasGroup != null)
		{
			m_CanvasGroup.alpha = m_FadeTransparency;
		}
		m_ChatFeedManager = ChatFeedManager.GetInstance();
		if (!(m_ChatFeedManager == null))
		{
		}
	}

	public void InputSubmitted(string message)
	{
		if (!string.IsNullOrEmpty(message) && m_ChatFeedManager != null)
		{
			m_ChatFeedManager.SendChatMessage_RPC(base.CurrentGamer, message, ChatFeedManager.MessageTag.PlayerMsg, bNeedslocalize: false);
		}
	}

	public void InputEnded(string message)
	{
		m_bFadeDownTimer = true;
		m_bFadeUpTimer = false;
		m_FadeDownStartTime = 0f;
		m_FadingDownTime = 0f;
	}

	public void AddFeedMessage(string message)
	{
		if (m_ScrollRect != null)
		{
			m_FeedQueue.Add(message);
		}
	}

	private void UpdateMessageFeedQueue()
	{
		if (m_FeedQueue.Count > 0 && UpdateManager.AquireHeavyCpuLock())
		{
			string text = m_FeedQueue[0];
			m_FeedQueue.RemoveAt(0);
			if (m_NextTextLine >= m_TextLinePool.Count || m_NextTextLine < 0)
			{
				m_NextTextLine = 0;
			}
			T17Text t17Text = m_TextLinePool[m_NextTextLine];
			if (t17Text != null)
			{
				t17Text.gameObject.SetActive(value: true);
				t17Text.text = text;
				Transform transform = t17Text.transform;
				transform.SetParent(m_ScrollRect.transform);
				transform.SetParent(m_ScrollRect.content.transform);
				transform.localScale = Vector3.one;
				transform.localPosition = Vector3.zero;
				m_bFadeDownTimer = true;
				m_bFadeUpTimer = true;
				m_FadingUpTime = 0f;
				m_FadeDownStartTime = 0f;
				m_FadingDownTime = 0f;
			}
			m_NextTextLine++;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if ((bool)m_ScrollRect && (bool)m_ScrollRect.verticalScrollbar)
		{
			m_VerticalScrollbar = m_ScrollRect.verticalScrollbar;
			m_VerticalScrollbar.onValueChanged.AddListener(OnScrollBarValueChanged);
			m_CurrentScrollBarPos = m_ScrollRect.verticalScrollbar.value;
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if ((bool)m_VerticalScrollbar)
		{
			m_VerticalScrollbar.onValueChanged.RemoveListener(OnScrollBarValueChanged);
			m_VerticalScrollbar = null;
			m_CurrentScrollBarPos = 0f;
		}
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_bPointerOver || m_bIsSelected)
		{
			m_bRemoveHUDItemOnPointerUp = false;
			m_HeldMouseButtons.Clear();
		}
		if (m_bRemoveHUDItemOnPointerUp)
		{
			bool flag = true;
			if (base.CurrentRewiredPlayer != null)
			{
				Rewired.Player.ControllerHelper controllers = base.CurrentRewiredPlayer.controllers;
				if (controllers != null && controllers.hasMouse)
				{
					Mouse mouse = controllers.Mouse;
					if (mouse != null)
					{
						for (int num = m_HeldMouseButtons.Count - 1; num >= 0; num--)
						{
							if (mouse.GetAnyButtonDown() && m_FrameDeselected != Time.frameCount)
							{
								flag = true;
								break;
							}
							if (mouse.GetButton(m_HeldMouseButtons[num]))
							{
								flag = false;
							}
						}
					}
				}
			}
			if (flag)
			{
				HUDMenuFlow.Instance.RemoveMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
				m_bRemoveHUDItemOnPointerUp = false;
			}
		}
		if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer.GetButtonDown("Chat"))
		{
			SelectInputField();
		}
		if (m_bFadeUpTimer)
		{
			if (m_FadingUpTime < m_FadeUpDuration)
			{
				m_FadingUpTime += Time.deltaTime;
				float alpha = Mathf.Lerp(m_CanvasGroup.alpha, 1f, m_FadingUpTime / m_FadeUpDuration);
				m_CanvasGroup.alpha = alpha;
			}
			else
			{
				m_bFadeUpTimer = false;
			}
		}
		else if (m_bFadeDownTimer)
		{
			if (m_FadeDownStartTime < m_TimeToBeginFadeDown)
			{
				m_FadeDownStartTime += Time.deltaTime;
			}
			else if (m_InputField == null || !m_InputField.isFocused)
			{
				if (m_FadingDownTime < m_FadeDownDuration)
				{
					m_FadingDownTime += UpdateManager.deltaTime;
					float alpha2 = Mathf.Lerp(m_CanvasGroup.alpha, m_FadeTransparency, m_FadingDownTime / m_FadeDownDuration);
					m_CanvasGroup.alpha = alpha2;
				}
				else
				{
					m_bFadeDownTimer = false;
				}
			}
		}
		UpdateMessageFeedQueue();
	}

	public void SelectInputField()
	{
		T17EventSystemsManager instance = T17EventSystemsManager.Instance;
		if (instance != null)
		{
			T17EventSystem eventSystemForGamer = instance.GetEventSystemForGamer(base.CurrentGamer);
			if (eventSystemForGamer != null && eventSystemForGamer.currentSelectedGameObject != m_InputField.gameObject)
			{
				m_InputField.m_bCanSelect = true;
				m_InputField.Select();
				m_bFadeDownTimer = false;
				m_bFadeUpTimer = true;
				m_FadingUpTime = 0f;
			}
		}
	}

	public void InputFieldSelected()
	{
		m_bFadeDownTimer = false;
		m_bFadeUpTimer = true;
		m_FadingUpTime = 0f;
		m_bIsSelected = true;
		HUDMenuFlow.Instance.AddMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
	}

	public void FadeUp()
	{
		m_bFadeDownTimer = false;
		m_bFadeUpTimer = true;
		m_FadingUpTime = 0f;
	}

	public void FadeDown()
	{
		m_bFadeDownTimer = true;
		m_bFadeUpTimer = false;
		m_FadingDownTime = 0f;
		m_FadeDownStartTime = m_TimeToBeginFadeDown + 1f;
	}

	public void OnMouseOver()
	{
		FadeUp();
	}

	public void OnPointerEnter()
	{
		HUDMenuFlow.Instance.AddMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
		m_bPointerOver = true;
	}

	public void OnPointerExit()
	{
		if (!m_bIsSelected)
		{
			HUDMenuFlow.Instance.RemoveMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
		}
		m_bPointerOver = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_InputField != null)
		{
			T17InputField inputField = m_InputField;
			inputField.FieldSelectedEvent = (T17InputField.SelectedHandler)Delegate.Remove(inputField.FieldSelectedEvent, new T17InputField.SelectedHandler(InputFieldSelected));
		}
		m_ChatFeedManager = null;
	}

	public bool IsRequiredForPlayer(int playerIndex)
	{
		return playerIndex == 0;
	}

	public void OnInputFieldDeselect()
	{
		m_InputField.m_bCanSelect = false;
		m_bIsSelected = false;
		m_HeldMouseButtons.Clear();
		if (base.CurrentRewiredPlayer != null && base.CurrentRewiredPlayer.controllers.GetLastActiveController().type == ControllerType.Mouse)
		{
			Rewired.Player.ControllerHelper controllers = base.CurrentRewiredPlayer.controllers;
			if (controllers != null && controllers.hasMouse)
			{
				Mouse mouse = controllers.Mouse;
				if (mouse != null)
				{
					for (int num = mouse.ElementIdentifiers.Count - 1; num >= 0; num--)
					{
						if (mouse.GetButtonDown(mouse.ElementIdentifiers[num].id) || mouse.GetButton(mouse.ElementIdentifiers[num].id))
						{
							m_HeldMouseButtons.Add(mouse.ElementIdentifiers[num].id);
						}
					}
				}
			}
		}
		if (m_HeldMouseButtons.Count > 0)
		{
			m_bRemoveHUDItemOnPointerUp = true;
			m_FrameDeselected = Time.frameCount;
		}
		else if (!m_bPointerOver)
		{
			HUDMenuFlow.Instance.RemoveMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}

	public void OnScrollBarValueChanged(float value)
	{
		if (m_VerticalScrollbar != null && base.CachedEventSystem != null)
		{
			if (m_VerticalScrollbar.gameObject == base.CachedEventSystem.currentSelectedGameObject && base.CurrentRewiredPlayer != null && base.CurrentRewiredPlayer.controllers.GetLastActiveController().type == ControllerType.Joystick && m_CurrentScrollBarPos != value)
			{
				m_VerticalScrollbar.value = m_CurrentScrollBarPos;
			}
			m_CurrentScrollBarPos = m_VerticalScrollbar.value;
		}
	}
}

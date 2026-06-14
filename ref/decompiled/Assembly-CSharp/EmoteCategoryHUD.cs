using System;
using System.Collections;
using UnityEngine;

public class EmoteCategoryHUD : BaseMenuBehaviour
{
	[Serializable]
	public class EmoteCategory
	{
		[Serializable]
		public class Emote
		{
			public enum EmoteSpecialAction
			{
				None,
				Surrender
			}

			public EmoteSpecialAction specialAction;

			public string emoteName = string.Empty;

			public string uiTextKey = string.Empty;

			public string speechTextKey = string.Empty;

			public GameObject m_ExtraHudElements;
		}

		public string categoryName = string.Empty;

		public SpeechTone speechTone = SpeechTone.No_Sound;

		public Emote[] m_Emotes = new Emote[0];
	}

	[Serializable]
	public class AntiSpamSettings
	{
		public int count;

		public float period;

		public float cooldown;
	}

	public readonly string[] m_Inputs = new string[4] { "EmoteCategory1", "EmoteCategory2", "EmoteCategory3", "EmoteCategory4" };

	[Header("UI References")]
	public GameObject m_EmoteParent;

	private EmoteDisplayHUD[] m_Emotes;

	public Animator m_SlideAnimator;

	public string m_SlideTriggerStart = "SlideLeft";

	[Header("Settings")]
	public float m_AutoHideTime = 2f;

	public float m_UIInputDelay = 0.5f;

	public AntiSpamSettings m_AntiSpamSettings = new AntiSpamSettings();

	public EmoteCategory[] m_EmoteCategories = new EmoteCategory[0];

	private EmoteCategory m_ActiveCategory;

	private float m_SpamCounter;

	private float m_AntiSpamTimer;

	private float m_ShowTime;

	private bool m_bIsUIShowing;

	public GameObject m_EmotesExtraParent;

	public bool isAntiSpamActive => m_AntiSpamTimer > 0f;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_EmoteParent != null)
		{
			m_Emotes = m_EmoteParent.GetComponentsInChildren<EmoteDisplayHUD>(includeInactive: true);
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		HideCategoryUI();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		HideCategoryUI();
		return true;
	}

	protected override void Update()
	{
		base.Update();
		ProcessInput();
		if (m_bIsUIShowing && m_ShowTime < m_AutoHideTime)
		{
			m_ShowTime += UpdateManager.deltaTime;
			if (m_ShowTime >= m_AutoHideTime)
			{
				m_ShowTime = m_AutoHideTime;
				HideCategoryUI();
			}
		}
		if (m_AntiSpamTimer > 0f)
		{
			m_AntiSpamTimer -= UpdateManager.deltaTime;
			if (m_AntiSpamTimer <= 0f)
			{
				m_AntiSpamTimer = 0f;
			}
		}
		if (m_SpamCounter > 0f)
		{
			m_SpamCounter -= UpdateManager.deltaTime;
			if (m_SpamCounter <= 0f)
			{
				m_SpamCounter = 0f;
			}
		}
	}

	private bool ProcessInput()
	{
		if (!base.CurrentGamePlayer.CheckInputEnabled(Player.PlayerInputs.Emote) || base.CurrentGamePlayer.GetIsKnockedOut())
		{
			return false;
		}
		if (base.CurrentRewiredPlayer != null && !T17DialogBoxManager.HasDialogsForGamer(base.CurrentGamer))
		{
			int num = -1;
			for (int i = 0; i < m_Inputs.Length; i++)
			{
				if (base.CurrentRewiredPlayer.GetButtonDown(m_Inputs[i]))
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				if (!m_bIsUIShowing)
				{
					StartCoroutine(ShowCategoryUI(num));
					return true;
				}
				if (m_ShowTime >= m_UIInputDelay)
				{
					ShowEmoteFromCategory(m_ActiveCategory, num);
					HideCategoryUI();
					return true;
				}
			}
		}
		return false;
	}

	public IEnumerator ShowCategoryUI(int categoryIndex)
	{
		if (categoryIndex < 0 || categoryIndex >= m_EmoteCategories.Length)
		{
			yield break;
		}
		EmoteCategory category = m_EmoteCategories[categoryIndex];
		if (category == null)
		{
			yield break;
		}
		if (m_EmoteParent != null)
		{
			m_EmoteParent.gameObject.SetActive(value: true);
		}
		if (m_SlideAnimator != null && m_SlideAnimator.isInitialized)
		{
			m_SlideAnimator.SetTrigger(m_SlideTriggerStart);
			m_SlideAnimator.Update(UpdateManager.deltaTime);
		}
		HideExtraEmoteElements();
		yield return null;
		yield return null;
		int count = Mathf.Min(category.m_Emotes.Length, m_Emotes.Length, m_Inputs.Length);
		int index = 0;
		for (int i = 0; i < count; i++)
		{
			EmoteCategory.Emote emote = category.m_Emotes[i];
			if (emote != null)
			{
				m_Emotes[index].SetupEmote(emote.uiTextKey, m_Inputs[index], emote.m_ExtraHudElements);
				m_Emotes[index].Show(base.CurrentGamer, this, null, hideInvoker: false);
				index++;
			}
		}
		m_ActiveCategory = category;
		m_ShowTime = 0f;
		m_bIsUIShowing = true;
	}

	public void HideCategoryUI()
	{
		if (m_Emotes != null)
		{
			for (int i = 0; i < m_Emotes.Length; i++)
			{
				m_Emotes[i].Hide(restoreInvokerState: false);
			}
		}
		HideExtraEmoteElements();
		m_ActiveCategory = null;
		m_bIsUIShowing = false;
	}

	private void HideExtraEmoteElements()
	{
		if (!(m_EmotesExtraParent != null))
		{
			return;
		}
		for (int num = m_EmotesExtraParent.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = m_EmotesExtraParent.transform.GetChild(num);
			if (child.gameObject.activeSelf)
			{
				child.gameObject.SetActive(value: false);
			}
		}
	}

	private bool ShowEmoteFromCategory(EmoteCategory category, int emoteIndex)
	{
		if (base.CurrentGamePlayer == null)
		{
			return false;
		}
		if (emoteIndex < 0 || emoteIndex >= category.m_Emotes.Length)
		{
			return false;
		}
		EmoteCategory.Emote emote = category.m_Emotes[emoteIndex];
		if (emote == null)
		{
			return false;
		}
		if (isAntiSpamActive)
		{
			ChatFeedManager instance = ChatFeedManager.GetInstance();
			if (instance != null)
			{
				instance.DisplayMessageToUser(base.CurrentGamer, instance.m_EmoteSpamLocalization, ChatFeedManager.MessageTag.Emote, bLocalize: true);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(emote.uiTextKey))
			{
				ChatFeedManager instance2 = ChatFeedManager.GetInstance();
				if (instance2 != null)
				{
					instance2.SendChatMessage_RPC(base.CurrentGamer, emote.uiTextKey, ChatFeedManager.MessageTag.Emote, bNeedslocalize: true, filter: false);
				}
			}
			if (!string.IsNullOrEmpty(emote.speechTextKey))
			{
				SpeechManager instance3 = SpeechManager.GetInstance();
				if (instance3 != null)
				{
					instance3.SaySomething(base.CurrentGamePlayer, emote.speechTextKey, category.speechTone, -1f, 0, -1, ignoreStatus: true);
				}
			}
			EmoteCategory.Emote.EmoteSpecialAction specialAction = emote.specialAction;
			if (specialAction == EmoteCategory.Emote.EmoteSpecialAction.Surrender)
			{
				base.CurrentGamePlayer.SetIsSurrendered(surrendered: true);
			}
			m_SpamCounter += m_AntiSpamSettings.period / (float)m_AntiSpamSettings.count;
			if (m_SpamCounter >= m_AntiSpamSettings.period)
			{
				m_AntiSpamTimer = m_AntiSpamSettings.cooldown;
				m_SpamCounter = 0f;
			}
		}
		return true;
	}
}

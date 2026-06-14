using System;
using System.Collections.Generic;
using UnityEngine;

public class VoiceChatFeedHUD : BaseIngamePassiveUI
{
	[Serializable]
	public class VoiceAndGamerObject
	{
		public T17Text m_GamerName;

		public T17Image m_MicGamer;

		public float m_TimeInactive;

		private bool m_bIsActive = true;

		public void SetActive(bool active)
		{
			if (m_bIsActive != active)
			{
				m_bIsActive = active;
				if (m_GamerName != null)
				{
					m_GamerName.transform.parent.gameObject.SetActive(active);
				}
			}
		}

		public void SetName(string name)
		{
			if (m_GamerName != null)
			{
				m_GamerName.text = name;
			}
		}

		public void SetImage(Sprite sprite)
		{
			if (m_MicGamer != null)
			{
				m_MicGamer.sprite = sprite;
			}
		}
	}

	public Sprite m_TalkingSprite;

	public Sprite m_SilentSprite;

	public float m_InactiveHideTime = 2f;

	public VoiceAndGamerObject[] m_GamerVoiceFeeds = new VoiceAndGamerObject[4];

	public List<Platform.VoiceChatGamer> m_VoiceChatGamers = new List<Platform.VoiceChatGamer>();

	protected override void Awake()
	{
		for (int i = 0; i < 4; i++)
		{
			m_VoiceChatGamers.Add(new Platform.VoiceChatGamer());
		}
		base.Awake();
	}

	protected override void OnDestroy()
	{
		m_TalkingSprite = null;
		m_SilentSprite = null;
		for (int i = 0; i < 4; i++)
		{
			if (m_VoiceChatGamers[i] != null)
			{
				m_VoiceChatGamers[i] = null;
			}
		}
		m_VoiceChatGamers.Clear();
		for (int j = 0; j < m_GamerVoiceFeeds.Length; j++)
		{
			m_GamerVoiceFeeds[j].m_GamerName = null;
			m_GamerVoiceFeeds[j].m_MicGamer = null;
			m_GamerVoiceFeeds[j] = null;
		}
		base.OnDestroy();
	}

	public override bool Init(Player owner)
	{
		if (base.Init(owner))
		{
			for (int i = 0; i < m_GamerVoiceFeeds.Length; i++)
			{
				if (m_GamerVoiceFeeds[i] != null)
				{
					m_GamerVoiceFeeds[i].SetActive(active: false);
				}
			}
			return true;
		}
		return false;
	}

	private void Update()
	{
		for (int i = 0; i < m_VoiceChatGamers.Count; i++)
		{
			Platform.VoiceChatGamer voiceChatGamer = m_VoiceChatGamers[i];
			voiceChatGamer.m_Gamer = null;
			voiceChatGamer.m_GamerName = null;
			voiceChatGamer.m_bIsMuted = false;
			voiceChatGamer.m_bIsTalking = false;
		}
		Platform.GetInstance().GetTalkingGamers(ref m_VoiceChatGamers);
		for (int j = 0; j < m_VoiceChatGamers.Count; j++)
		{
			if (m_VoiceChatGamers[j] == null || m_GamerVoiceFeeds[j] == null)
			{
				continue;
			}
			bool active = true;
			if (m_VoiceChatGamers[j].m_Gamer != null && !m_VoiceChatGamers[j].m_bIsMuted)
			{
				m_GamerVoiceFeeds[j].SetName(m_VoiceChatGamers[j].m_GamerName);
				if (m_VoiceChatGamers[j].m_bIsTalking)
				{
					m_GamerVoiceFeeds[j].SetImage(m_TalkingSprite);
					m_GamerVoiceFeeds[j].m_TimeInactive = 0f;
				}
				else
				{
					m_GamerVoiceFeeds[j].SetImage(m_SilentSprite);
					m_GamerVoiceFeeds[j].m_TimeInactive += UpdateManager.deltaTime;
					if (m_GamerVoiceFeeds[j].m_TimeInactive > m_InactiveHideTime || m_SilentSprite == null)
					{
						active = false;
					}
				}
			}
			else
			{
				active = false;
			}
			m_GamerVoiceFeeds[j].SetActive(active);
		}
	}
}

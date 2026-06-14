using System.Collections.Generic;
using UnityEngine;

public class TutorialSpeechHUD : BaseMenuBehaviour
{
	public delegate void TutorialSpeechCallback(int id);

	public class SpeechData
	{
		public int id = -1;

		public string text;

		public bool isLocalised;

		public float speakTime;

		public float lineTime;

		public TutorialSpeechCallback finishedCallback;
	}

	[Header("UI References")]
	public RectTransform m_BubbleRoot;

	public T17Text m_Text;

	public UI_AnimationToRenderTexture m_FaceRenderer;

	[Header("Face Settings")]
	public Animator m_FaceAnimator;

	public string m_FaceTalkingParameter = string.Empty;

	[Header("Misc. Settings")]
	public float m_PauseBetweenLines;

	public Queue<SpeechData> m_SpeechQueue = new Queue<SpeechData>();

	private SpeechData m_CurrentSpeech;

	private int m_NextID;

	private int m_FaceTalkingParameterID = -1;

	private float m_RemainingAnimTime;

	private float m_RemainingLineTime;

	private float m_NextLineTime;

	private bool m_bIsBubbleShowing;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (!string.IsNullOrEmpty(m_FaceTalkingParameter))
		{
			m_FaceTalkingParameterID = Animator.StringToHash(m_FaceTalkingParameter);
		}
		if (m_FaceRenderer != null)
		{
			m_FaceRenderer.gameObject.SetActive(value: true);
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_bIsBubbleShowing)
		{
			ShowBubbleUI();
		}
		else
		{
			HideBubbleUI();
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		StopTalkingAnim();
		return true;
	}

	protected override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		ProcessQueue(deltaTime);
		ProcessVisuals(deltaTime);
	}

	private void ProcessQueue(float delta)
	{
		if (m_RemainingLineTime > 0f)
		{
			m_RemainingLineTime -= delta;
			if (m_RemainingLineTime <= 0f)
			{
				if (m_CurrentSpeech != null && m_CurrentSpeech.finishedCallback != null)
				{
					m_CurrentSpeech.finishedCallback(m_CurrentSpeech.id);
				}
				m_CurrentSpeech = null;
				m_NextLineTime = m_PauseBetweenLines;
				if (m_bIsBubbleShowing)
				{
					HideBubbleUI();
				}
			}
		}
		if (m_NextLineTime > 0f)
		{
			m_NextLineTime -= delta;
			if (m_NextLineTime <= 0f)
			{
				m_NextLineTime = 0f;
			}
		}
		if (m_CurrentSpeech != null || !(m_NextLineTime <= 0f))
		{
			return;
		}
		if (m_SpeechQueue.Count > 0)
		{
			SpeechData speechData = m_SpeechQueue.Dequeue();
			if (speechData != null)
			{
				ShowSpeech(speechData.text, speechData.isLocalised);
				ShowTalkingAnim(speechData.speakTime);
				m_RemainingLineTime = speechData.lineTime;
				m_NextLineTime = 0f;
				m_CurrentSpeech = speechData;
				return;
			}
			m_RemainingLineTime = -1f;
			m_NextLineTime = -1f;
			m_CurrentSpeech = null;
			if (m_bIsBubbleShowing)
			{
				HideBubbleUI();
			}
		}
		else if (m_bIsBubbleShowing)
		{
			HideBubbleUI();
		}
	}

	private void ProcessVisuals(float delta)
	{
		if (m_RemainingAnimTime > 0f)
		{
			m_RemainingAnimTime -= delta;
			if (m_RemainingAnimTime <= 0f)
			{
				StopTalkingAnim();
			}
		}
	}

	public int QueueSpeech(string speechKey, bool isLocalised = false, float lineTime = 0f, float speakTime = 0f, TutorialSpeechCallback onSpokenCallback = null)
	{
		int num = m_NextID++;
		SpeechData speechData = new SpeechData();
		speechData.id = num;
		speechData.text = speechKey;
		speechData.isLocalised = isLocalised;
		speechData.lineTime = lineTime;
		speechData.speakTime = speakTime;
		speechData.finishedCallback = onSpokenCallback;
		m_SpeechQueue.Enqueue(speechData);
		return num;
	}

	public void ClearSpeechQueue(bool succeed = false)
	{
		if (succeed)
		{
			if (m_CurrentSpeech != null && m_CurrentSpeech.finishedCallback != null)
			{
				m_CurrentSpeech.finishedCallback(m_CurrentSpeech.id);
			}
			m_CurrentSpeech = null;
			int count = m_SpeechQueue.Count;
			for (int i = 0; i < count; i++)
			{
				SpeechData speechData = m_SpeechQueue.Dequeue();
				if (speechData.finishedCallback != null)
				{
					speechData.finishedCallback(speechData.id);
				}
			}
			m_NextLineTime = m_PauseBetweenLines;
		}
		else
		{
			m_CurrentSpeech = null;
			m_SpeechQueue.Clear();
			m_NextLineTime = -1f;
		}
	}

	private bool ShowSpeech(string speechKey, bool isLocalised = false)
	{
		if (string.IsNullOrEmpty(speechKey))
		{
			return false;
		}
		if (!m_bIsBubbleShowing)
		{
			ShowBubbleUI();
		}
		if (!isLocalised)
		{
			m_Text.SetNewLocalizationTag(speechKey);
			m_Text.SetVerticesDirty();
		}
		else if (m_Text != null)
		{
			m_Text.m_bNeedsLocalization = false;
			m_Text.SetNewLocalizationTag(speechKey);
		}
		return true;
	}

	private void ClearSpeech(bool hideBubble = true)
	{
		if (m_Text != null)
		{
			m_Text.m_bNeedsLocalization = false;
			m_Text.SetNewLocalizationTag(string.Empty);
		}
		if (hideBubble)
		{
			HideBubbleUI();
		}
	}

	private void ShowTalkingAnim(float duration = 0f)
	{
		if (duration > 0f)
		{
			if (m_FaceAnimator != null)
			{
				m_FaceAnimator.SetBool(m_FaceTalkingParameterID, value: true);
			}
			m_RemainingAnimTime = duration;
		}
		else
		{
			m_RemainingAnimTime = 0f;
		}
	}

	private void StopTalkingAnim()
	{
		if (m_FaceAnimator != null)
		{
			m_FaceAnimator.SetBool(m_FaceTalkingParameterID, value: false);
		}
		m_RemainingAnimTime = 0f;
	}

	private void ShowBubbleUI()
	{
		if (m_BubbleRoot != null)
		{
			m_BubbleRoot.gameObject.SetActive(value: true);
		}
		m_bIsBubbleShowing = true;
	}

	private void HideBubbleUI()
	{
		if (m_BubbleRoot != null)
		{
			m_BubbleRoot.gameObject.SetActive(value: false);
		}
		m_bIsBubbleShowing = false;
	}

	private void OnDisable()
	{
		if (m_RemainingAnimTime > 0f)
		{
			StopTalkingAnim();
		}
	}
}

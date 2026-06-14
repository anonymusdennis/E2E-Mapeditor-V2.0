using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class CharacterSpeechBubbleHandler : T17MonoBehaviour, IControlledUpdate
{
	private class SpeechData
	{
		private Events[] m_PlayToneSFXEventNames = new Events[4]
		{
			Events.Play_Player_Speech_Positive,
			Events.Play_Player_Speech_Negative,
			Events.Play_Computer_Speech_Positive,
			Events.Play_Computer_Speech_Negative
		};

		private Events[] m_StopToneSFXEventNames = new Events[4]
		{
			Events.Stop_Player_Speech_Positive,
			Events.Stop_Player_Speech_Negative,
			Events.Stop_Computer_Speech_Positive,
			Events.Stop_Computer_Speech_Negative
		};

		public SpeechTone m_Tone;

		public float m_Duration;

		public float m_CutoffEpoch;

		public string m_Text;

		public int m_Priority;

		public bool m_SfxPlayed;

		public bool m_bAllowTextColourControl;

		public Events PlaySFX
		{
			get
			{
				int tone = (int)m_Tone;
				if (tone >= 0 && tone < 2)
				{
					return m_PlayToneSFXEventNames[tone];
				}
				if (tone >= 3 && tone <= 4)
				{
					return m_PlayToneSFXEventNames[tone - 1];
				}
				return m_PlayToneSFXEventNames[0];
			}
		}

		public Events StopSFX
		{
			get
			{
				int tone = (int)m_Tone;
				if (tone >= 0 && tone < 2)
				{
					return m_StopToneSFXEventNames[tone];
				}
				if (tone >= 3 && tone <= 4)
				{
					return m_StopToneSFXEventNames[tone - 1];
				}
				return m_StopToneSFXEventNames[0];
			}
		}

		public SpeechData(string text, SpeechTone tone, float duration, int priority)
		{
			m_Text = text;
			m_Tone = tone;
			m_Duration = duration;
			m_CutoffEpoch = UpdateManager.time + duration;
			m_Priority = priority;
			m_SfxPlayed = false;
			m_bAllowTextColourControl = false;
		}
	}

	public Sprite m_BackgroundSprite;

	public Sprite m_TailSprite;

	private LinkedList<SpeechData> m_SpeechBuffer = new LinkedList<SpeechData>();

	private float m_SpeechTime;

	private T17TrackedUIElement m_TrackedUIElement;

	private TrackableUIElementsReporter m_TrackedUIReporter;

	private void Start()
	{
		m_TrackedUIReporter = base.gameObject.GetComponentInChildren<TrackableUIElementsReporter>(includeInactive: true);
		if (m_TrackedUIReporter == null)
		{
			m_TrackedUIReporter = base.gameObject.AddComponent<TrackableUIElementsReporter>();
			string text = "TrackableUIElementsReporter is missing from '" + base.name + "'! Please attach one. " + DEBUG_PrintTransformHeirarchy(base.transform);
		}
	}

	protected virtual void OnDestroy()
	{
		m_BackgroundSprite = null;
		m_TailSprite = null;
		m_SpeechBuffer.Clear();
		if (m_TrackedUIElement != null)
		{
			T17TrackedUIElement trackedUIElement = m_TrackedUIElement;
			trackedUIElement.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Remove(trackedUIElement.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
			m_TrackedUIElement = null;
		}
		if (m_TrackedUIReporter != null)
		{
			m_TrackedUIReporter = null;
		}
	}

	private string DEBUG_PrintTransformHeirarchy(Transform theTransform)
	{
		string text = string.Empty;
		Transform transform = theTransform;
		while (transform.parent != null)
		{
			text = text + transform.parent.name + "/";
			transform = transform.parent;
		}
		return text;
	}

	public void ControlledUpdate()
	{
		if (m_SpeechBuffer.Count <= 0)
		{
			return;
		}
		if (m_TrackedUIElement == null)
		{
			RequestUIElement();
		}
		m_SpeechTime += UpdateManager.deltaTime;
		SpeechData value = m_SpeechBuffer.First.Value;
		if (!(m_SpeechTime >= value.m_Duration))
		{
			return;
		}
		if (value.m_SfxPlayed && value.m_Tone != SpeechTone.No_Sound)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, value.StopSFX, base.gameObject);
		}
		m_SpeechBuffer.RemoveFirst();
		m_SpeechTime = 0f;
		RemoveOldMessages();
		if (!(m_TrackedUIElement != null))
		{
			return;
		}
		if (m_SpeechBuffer.Count > 0)
		{
			value = m_SpeechBuffer.First.Value;
			m_TrackedUIElement.m_SpeechBubble.m_Text.text = value.m_Text;
			m_TrackedUIElement.m_SpeechBubble.m_Text.CheckMarkup();
			if (!value.m_SfxPlayed && value.m_Tone != SpeechTone.No_Sound)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, value.PlaySFX, base.gameObject);
				value.m_SfxPlayed = true;
			}
		}
		else
		{
			DisableSpeechBubble();
		}
	}

	private void RemoveOldMessages()
	{
		for (int num = m_SpeechBuffer.Count - 1; num >= 0; num--)
		{
			SpeechData value = m_SpeechBuffer.First.Value;
			if (UpdateManager.time > value.m_CutoffEpoch)
			{
				m_SpeechBuffer.RemoveFirst();
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void NewSpeech(string localizedSpeech, SpeechTone tone, float speechDuration, int priority, bool bAllowTextColourControl)
	{
		if (string.IsNullOrEmpty(localizedSpeech) || !(speechDuration > 0f))
		{
			return;
		}
		SpeechData speechData = new SpeechData(localizedSpeech, tone, speechDuration, priority);
		if (bAllowTextColourControl)
		{
			speechData.m_bAllowTextColourControl = true;
		}
		if (priority > 0 && m_SpeechBuffer.Count > 0)
		{
			LinkedListNode<SpeechData> linkedListNode;
			for (linkedListNode = ((m_SpeechTime != 0f) ? m_SpeechBuffer.First.Next : m_SpeechBuffer.First); linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (priority > linkedListNode.Value.m_Priority)
				{
					m_SpeechBuffer.AddBefore(linkedListNode, speechData);
					break;
				}
			}
			if (linkedListNode == null)
			{
				m_SpeechBuffer.AddLast(speechData);
			}
		}
		else
		{
			m_SpeechBuffer.AddLast(speechData);
		}
	}

	public bool IsProcessingSpeech()
	{
		return m_SpeechBuffer.Count > 0;
	}

	private void DisableSpeechBubble()
	{
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.DisableSpeechBubble();
			m_TrackedUIElement = null;
		}
	}

	public void EnableSpeechBubble()
	{
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.EnableSpeechBubble();
		}
	}

	private void RequestUIElement()
	{
		if (m_TrackedUIReporter == null)
		{
			return;
		}
		m_TrackedUIElement = m_TrackedUIReporter.AssignAlwaysVisibleWorldCanvasUIElement();
		if (!(m_TrackedUIElement != null))
		{
			return;
		}
		T17TrackedUIElement trackedUIElement = m_TrackedUIElement;
		trackedUIElement.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Remove(trackedUIElement.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
		T17TrackedUIElement trackedUIElement2 = m_TrackedUIElement;
		trackedUIElement2.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Combine(trackedUIElement2.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
		if (m_TrackedUIElement.EnableSpeechBubble())
		{
			m_TrackedUIElement.m_SpeechBubble.m_Text.text = m_SpeechBuffer.First.Value.m_Text;
			m_TrackedUIElement.m_SpeechBubble.m_Text.CheckMarkup();
			if (m_SpeechBuffer.First.Value.m_bAllowTextColourControl)
			{
				m_TrackedUIElement.m_SpeechBubble.SetTextColour(m_SpeechBuffer.First.Value.m_Tone);
			}
			else
			{
				m_TrackedUIElement.m_SpeechBubble.SetTextColour(SpeechTone.No_Sound);
			}
			if (m_BackgroundSprite != null && m_TailSprite != null)
			{
				m_TrackedUIElement.m_SpeechBubble.m_Background.sprite = m_BackgroundSprite;
				m_TrackedUIElement.m_SpeechBubble.m_Tail.sprite = m_TailSprite;
			}
			if (!m_SpeechBuffer.First.Value.m_SfxPlayed && m_SpeechBuffer.First.Value.m_Tone != SpeechTone.No_Sound)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_SpeechBuffer.First.Value.PlaySFX, base.gameObject);
				m_SpeechBuffer.First.Value.m_SfxPlayed = true;
			}
		}
	}

	public void ClearSpeechBuffer()
	{
		m_SpeechBuffer.Clear();
		DisableSpeechBubble();
	}

	private void OnSpeechBubbleElementReleased(T17TrackedUIElement element)
	{
		if (element == m_TrackedUIElement)
		{
			T17TrackedUIElement trackedUIElement = m_TrackedUIElement;
			trackedUIElement.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Remove(trackedUIElement.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
			m_TrackedUIElement = null;
		}
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}
}

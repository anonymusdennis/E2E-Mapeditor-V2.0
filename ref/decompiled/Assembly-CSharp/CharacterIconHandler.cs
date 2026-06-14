using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIconHandler : MonoBehaviour, IControlledUpdate
{
	public enum IconType
	{
		InMenus,
		RobinsonQuest,
		Quest,
		Vendor,
		ClimbUp,
		ClimbDown,
		GuardReport,
		GuardInvestigate,
		GuardAlert,
		MultiplayerSingle,
		MultiplayerDouble,
		MultiplayerTriple,
		MultiplayerQuad,
		DogMissingKey,
		MAX
	}

	private class ActiveIcon
	{
		public IconType icon = IconType.MAX;

		public float remainingTime;
	}

	private TrackableUIElementsReporter m_TrackedUIReporter;

	private T17TrackedUIElement m_TrackedUIElement;

	private List<ActiveIcon> m_ActiveIcons = new List<ActiveIcon>();

	private int m_PrevIconCount;

	private IconType m_PrevIconType = IconType.MAX;

	private bool m_bHideCharacterIcon;

	private bool m_bHidden;

	private bool m_bHideMenuIcon;

	private T17NetView m_NetView;

	private void Awake()
	{
		m_NetView = GetComponent<T17NetView>();
	}

	private void Start()
	{
		m_TrackedUIReporter = base.gameObject.GetComponentInChildren<TrackableUIElementsReporter>(includeInactive: true);
		if (m_TrackedUIReporter == null)
		{
			T17NetManager.LogGoogleException("TrackableUIElementsReporter is missing from '" + base.name + "'! Please attach one.");
			m_TrackedUIReporter = base.gameObject.AddComponent<TrackableUIElementsReporter>();
		}
	}

	protected virtual void OnDestroy()
	{
		m_TrackedUIReporter = null;
		m_TrackedUIElement = null;
		m_ActiveIcons.Clear();
		m_NetView = null;
	}

	public void ControlledUpdate()
	{
		ProcessTimers();
	}

	public void ControlledFixedUpdate()
	{
	}

	private void ProcessTimers()
	{
		float deltaTime = UpdateManager.deltaTime;
		int count = m_ActiveIcons.Count;
		for (int i = 0; i < count; i++)
		{
			float remainingTime = m_ActiveIcons[i].remainingTime;
			if (remainingTime > 0f)
			{
				remainingTime -= deltaTime;
				m_ActiveIcons[i].remainingTime = remainingTime;
				if (remainingTime <= 0f)
				{
					m_ActiveIcons.RemoveAt(i);
					UpdateActiveIcon();
					return;
				}
			}
		}
		if (m_PrevIconCount == count && !(m_TrackedUIElement == null))
		{
			return;
		}
		if (m_TrackedUIElement != null)
		{
			ActiveIcon activeIcon = GetActiveIcon();
			if (activeIcon != null)
			{
				if (m_PrevIconType == IconType.Quest && activeIcon.icon == IconType.Vendor && m_TrackedUIReporter != null)
				{
					m_TrackedUIReporter.SetNameplateIcon(IconType.MAX);
				}
				m_PrevIconType = activeIcon.icon;
			}
		}
		UpdateActiveIcon();
		m_PrevIconCount = count;
	}

	public void DisplayIconRPC(IconType IType)
	{
		DisplayIconRPC(IType, -1f);
	}

	public void DisplayIconRPC(IconType IType, float duration)
	{
		if (m_NetView != null)
		{
			m_NetView.PostLevelLoadRPC("RPC_DisplayIcon", NetTargets.All, IType, duration);
		}
	}

	[PunRPC]
	public void RPC_DisplayIcon(IconType IType, float duration, PhotonMessageInfo info)
	{
		DisplayIcon(IType, duration);
	}

	public void DisplayIcon(IconType IType)
	{
		DisplayIcon(IType, -1f);
	}

	public void DisplayIcon(IconType IType, float duration)
	{
		switch (IType)
		{
		case IconType.MAX:
			return;
		case IconType.Quest:
			if (m_TrackedUIReporter != null && m_TrackedUIReporter.CharacterOwner != null && m_TrackedUIReporter.CharacterOwner.m_bIsRobinsonCharacter)
			{
				IType = IconType.RobinsonQuest;
			}
			break;
		}
		ActiveIcon activeIcon = null;
		for (int i = 0; i < m_ActiveIcons.Count; i++)
		{
			if (m_ActiveIcons[i].icon == IType)
			{
				activeIcon = m_ActiveIcons[i];
				break;
			}
		}
		if (activeIcon == null)
		{
			activeIcon = new ActiveIcon();
			activeIcon.icon = IType;
			m_ActiveIcons.Add(activeIcon);
		}
		activeIcon.remainingTime = duration;
		UpdateActiveIcon();
	}

	public void RemoveIconRPC(IconType IType)
	{
		if (m_NetView != null)
		{
			m_NetView.PostLevelLoadRPC("RPC_RemoveIcon", NetTargets.All, IType);
		}
	}

	[PunRPC]
	private void RPC_RemoveIcon(IconType IType, PhotonMessageInfo info)
	{
		RemoveIcon(IType);
	}

	public void RemoveIcon(IconType IType)
	{
		if (IType == IconType.Quest && m_TrackedUIReporter != null && m_TrackedUIReporter.CharacterOwner != null && m_TrackedUIReporter.CharacterOwner.m_bIsRobinsonCharacter)
		{
			IType = IconType.RobinsonQuest;
		}
		for (int num = m_ActiveIcons.Count - 1; num >= 0; num--)
		{
			if (m_ActiveIcons[num].icon == IType)
			{
				m_ActiveIcons.RemoveAt(num);
			}
		}
		UpdateActiveIcon();
	}

	private void UpdateActiveIcon()
	{
		ActiveIcon activeIcon = null;
		if (m_ActiveIcons.Count > 0 && (!m_bHidden || !m_bHideMenuIcon))
		{
			if (m_bHidden && !m_bHideMenuIcon)
			{
				for (int i = 0; i < m_ActiveIcons.Count; i++)
				{
					if (m_ActiveIcons[i].icon == IconType.InMenus)
					{
						activeIcon = m_ActiveIcons[i];
						break;
					}
				}
			}
			else
			{
				activeIcon = GetActiveIcon();
			}
		}
		if (activeIcon != null && !m_bHideCharacterIcon)
		{
			if (m_TrackedUIElement == null)
			{
				RequestUIElement();
				if (m_TrackedUIElement == null)
				{
					return;
				}
			}
			m_TrackedUIElement.SetIconImage(activeIcon.icon);
			m_TrackedUIReporter.SetNameplateIcon(activeIcon.icon);
		}
		else
		{
			if (m_TrackedUIElement != null)
			{
				DisableIcon();
			}
			m_TrackedUIReporter.SetNameplateIcon(IconType.MAX);
		}
	}

	public void SetIconsHidden(bool hidden, bool hideMenuIcon = false)
	{
		if (hidden != m_bHidden || hideMenuIcon != m_bHideMenuIcon)
		{
			m_bHidden = hidden;
			m_bHideMenuIcon = hideMenuIcon;
			UpdateActiveIcon();
		}
	}

	public void HideCharacterIcon(bool hide)
	{
		m_bHideCharacterIcon = hide;
		if (hide)
		{
			if (m_TrackedUIElement != null && m_TrackedUIElement.m_Icon != null)
			{
				m_TrackedUIElement.m_Icon.gameObject.SetActive(value: false);
			}
		}
		else if (m_TrackedUIElement != null && m_TrackedUIElement.m_Icon != null)
		{
			m_TrackedUIElement.m_Icon.gameObject.SetActive(value: true);
		}
	}

	private ActiveIcon GetActiveIcon()
	{
		if (m_ActiveIcons.Count == 0)
		{
			return null;
		}
		ActiveIcon activeIcon = m_ActiveIcons[0];
		for (int i = 1; i < m_ActiveIcons.Count; i++)
		{
			if (m_ActiveIcons[i].icon < activeIcon.icon)
			{
				activeIcon = m_ActiveIcons[i];
			}
		}
		return activeIcon;
	}

	private void RequestUIElement()
	{
		m_TrackedUIElement = m_TrackedUIReporter.AssignAlwaysVisibleWorldCanvasUIElement();
		if (m_TrackedUIElement != null)
		{
			T17TrackedUIElement trackedUIElement = m_TrackedUIElement;
			trackedUIElement.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Remove(trackedUIElement.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
			T17TrackedUIElement trackedUIElement2 = m_TrackedUIElement;
			trackedUIElement2.OnElementReleased = (T17TrackedUIElement.TrackedUIElementEvent)Delegate.Combine(trackedUIElement2.OnElementReleased, new T17TrackedUIElement.TrackedUIElementEvent(OnSpeechBubbleElementReleased));
			m_TrackedUIElement.EnableIcon();
		}
	}

	private void DisableIcon()
	{
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.DisableIcon();
			m_TrackedUIElement = null;
		}
	}

	private void OnSpeechBubbleElementReleased(T17TrackedUIElement element)
	{
		if (element == m_TrackedUIElement)
		{
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

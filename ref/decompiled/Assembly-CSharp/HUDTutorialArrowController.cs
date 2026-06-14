using System.Collections.Generic;
using UnityEngine;

public class HUDTutorialArrowController : T17MonoBehaviour
{
	public enum HUDTutorial
	{
		ItemSelection,
		COUNT
	}

	private List<HUDTutorial> m_ActiveTutorials = new List<HUDTutorial>();

	public RectTransform m_TutorialIndicator;

	private Transform m_IndicatorOriginalParent;

	private Transform m_TargetTransform;

	private HUDTutorialArrowHandler[] m_TutorialHandlers = new HUDTutorialArrowHandler[0];

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_TutorialIndicator != null && m_TutorialIndicator.parent != null)
		{
			m_IndicatorOriginalParent = m_TutorialIndicator.parent;
		}
		if (LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial)
		{
			base.enabled = false;
		}
		m_TutorialHandlers = GetComponentsInChildren<HUDTutorialArrowHandler>(includeInactive: true);
		if (m_TutorialHandlers != null)
		{
			for (int i = 0; i < m_TutorialHandlers.Length; i++)
			{
				m_TutorialHandlers[i].TutorialInit();
			}
		}
		return base.StartInit();
	}

	private void Update()
	{
		if (m_TutorialIndicator == null)
		{
			return;
		}
		Transform transform = null;
		HUDTutorialArrowHandler currentTutorial = GetCurrentTutorial();
		if (currentTutorial != null)
		{
			transform = currentTutorial.GetTutorialTargetTransform();
		}
		if (!(transform != m_TargetTransform))
		{
			return;
		}
		m_TargetTransform = transform;
		if (transform != null)
		{
			m_TutorialIndicator.SetParent(transform);
			m_TutorialIndicator.anchorMin = new Vector2(0f, 0f);
			m_TutorialIndicator.anchorMax = new Vector2(1f, 1f);
			m_TutorialIndicator.anchoredPosition = new Vector2(0f, 0f);
			m_TutorialIndicator.sizeDelta = new Vector2(0f, 0f);
			m_TutorialIndicator.SetParent(transform.parent);
			if (transform.parent != null)
			{
				int siblingIndex = 0;
				int childCount = transform.parent.childCount;
				for (int i = 0; i < transform.parent.childCount; i++)
				{
					Transform child = transform.parent.GetChild(i);
					if (child != null && (bool)child.GetComponent<T17ItemTooltip>())
					{
						break;
					}
					siblingIndex = i;
				}
				m_TutorialIndicator.SetSiblingIndex(siblingIndex);
			}
			m_TutorialIndicator.gameObject.SetActive(value: true);
		}
		else
		{
			m_TutorialIndicator.gameObject.SetActive(value: false);
			if (m_IndicatorOriginalParent != null)
			{
				m_TutorialIndicator.SetParent(m_IndicatorOriginalParent);
			}
		}
	}

	private void AddActiveTutorial(HUDTutorial tutorial)
	{
		if (!m_ActiveTutorials.Contains(tutorial))
		{
			m_ActiveTutorials.Add(tutorial);
		}
	}

	public void RemoveActiveTutorial(HUDTutorial tutorial)
	{
		m_ActiveTutorials.Remove(tutorial);
		HUDTutorialArrowHandler handler = GetHandler(tutorial);
		if (handler != null)
		{
			handler.ClearData();
		}
	}

	public HUDTutorialArrowHandler GetCurrentTutorial()
	{
		for (int i = 0; i < m_ActiveTutorials.Count; i++)
		{
			for (int j = 0; j < m_TutorialHandlers.Length; j++)
			{
				if (m_ActiveTutorials[i] == m_TutorialHandlers[j].GetTutorialType() && m_TutorialHandlers[j].IsActive())
				{
					return m_TutorialHandlers[j];
				}
			}
		}
		return null;
	}

	public HUDTutorialArrowHandler GetHandler(HUDTutorial type)
	{
		for (int i = 0; i < m_TutorialHandlers.Length; i++)
		{
			if (m_TutorialHandlers[i] != null && m_TutorialHandlers[i].GetTutorialType() == type)
			{
				return m_TutorialHandlers[i];
			}
		}
		return null;
	}

	public void StartTutorial(List<ItemData> targetItems, HUDTutorial type)
	{
		for (int i = 0; i < m_TutorialHandlers.Length; i++)
		{
			if (m_TutorialHandlers[i] != null && m_TutorialHandlers[i].GetTutorialType() == type)
			{
				m_TutorialHandlers[i].SetTutorialTarget(targetItems);
				AddActiveTutorial(type);
			}
		}
	}
}

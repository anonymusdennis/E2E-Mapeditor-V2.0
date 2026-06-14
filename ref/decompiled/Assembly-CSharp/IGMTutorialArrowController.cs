using System.Collections.Generic;
using UnityEngine;

public class IGMTutorialArrowController : T17MonoBehaviour
{
	public enum IGMTutorial
	{
		DeskMenu,
		CraftingMenu,
		LootingCharacter,
		COUNT
	}

	private List<IGMTutorial> m_ActiveTutorials = new List<IGMTutorial>();

	public RectTransform m_TutorialIndicator;

	private T17Image m_IndicatorSprite;

	private Transform m_IndicatorOriginalParent;

	private Sprite m_IndicatorOriginalSprite;

	private Transform m_TargetTransform;

	private IGMTutorialArrowHandler[] m_TutorialHandlers = new IGMTutorialArrowHandler[0];

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_TutorialIndicator != null && m_TutorialIndicator.parent != null)
		{
			m_IndicatorOriginalParent = m_TutorialIndicator.parent;
			m_IndicatorSprite = m_TutorialIndicator.GetComponentInChildren<T17Image>();
			if (m_IndicatorSprite != null)
			{
				m_IndicatorOriginalSprite = m_IndicatorSprite.sprite;
			}
		}
		if (LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial)
		{
			base.enabled = false;
		}
		m_TutorialHandlers = GetComponentsInChildren<IGMTutorialArrowHandler>(includeInactive: true);
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
		IGMTutorialArrowHandler currentTutorial = GetCurrentTutorial();
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
			if (m_IndicatorSprite != null)
			{
				Sprite overrideSprite = currentTutorial.GetOverrideSprite();
				if (overrideSprite != null)
				{
					m_IndicatorSprite.sprite = overrideSprite;
				}
				else
				{
					m_IndicatorSprite.sprite = m_IndicatorOriginalSprite;
				}
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
			if (m_IndicatorSprite != null)
			{
				m_IndicatorSprite.sprite = m_IndicatorOriginalSprite;
			}
		}
	}

	private void AddActiveTutorial(IGMTutorial tutorial)
	{
		if (!m_ActiveTutorials.Contains(tutorial))
		{
			m_ActiveTutorials.Add(tutorial);
		}
	}

	public void RemoveActiveTutorial(IGMTutorial tutorial)
	{
		m_ActiveTutorials.Remove(tutorial);
		IGMTutorialArrowHandler handler = GetHandler(tutorial);
		if (handler != null)
		{
			handler.ClearData();
		}
	}

	public IGMTutorialArrowHandler GetCurrentTutorial()
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

	public void StartTutorial(List<ItemData> targetItems, IGMTutorial type)
	{
		for (int i = 0; i < m_TutorialHandlers.Length; i++)
		{
			if (m_TutorialHandlers[i] != null && m_TutorialHandlers[i].GetTutorialType() == type)
			{
				m_TutorialHandlers[i].SetTutorialTarget(targetItems);
				AddActiveTutorial(type);
				break;
			}
		}
	}

	public IGMTutorialArrowHandler GetHandler(IGMTutorial type)
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
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CalendarMenu : MonoBehaviour
{
	private Player m_Player;

	private Gamer m_Gamer;

	private List<CalendarDay> m_CalendarDays = new List<CalendarDay>();

	public T17Text m_MonthsText;

	public T17Text m_DaysLeftText;

	private int m_CurrentMonth = -1;

	private int m_CurrentDayInMonth = -1;

	private int m_DaysToRelease = -1;

	public Sprite m_CrossSprite;

	public Sprite m_EventSprite;

	private IT17EventHelper[] m_EventHelperInterfaces;

	public CalendarTooltip m_CalendarTooltip;

	private const int MonthLength = 30;

	private void Awake()
	{
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		m_CalendarDays.AddRange(GetComponentsInChildren<CalendarDay>(includeInactive: true));
		for (int i = 0; i < m_CalendarDays.Count && i < 30; i++)
		{
			m_CalendarDays[i].m_DayDate.text = (i + 1).ToString();
			m_CalendarDays[i].m_CalendarTooltip = m_CalendarTooltip;
			m_CalendarDays[i].m_Index = i;
		}
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnDayChange += UpdateCalendar;
		}
		base.gameObject.SetActive(value: false);
	}

	public void ShowCalendar(Player player)
	{
		if (!base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: true);
			m_Player = player;
			m_Gamer = player.m_Gamer;
		}
		UpdateCalendar();
		if (m_EventHelperInterfaces != null)
		{
			T17EventSystem gamersEventSystem = null;
			if (m_Gamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
			}
			for (int i = 0; i < m_EventHelperInterfaces.Length; i++)
			{
				if (m_EventHelperInterfaces[i] != null && m_Gamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(m_Gamer, gamersEventSystem);
				}
			}
		}
		if (EventSystem.current != null && m_CalendarDays[0] != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_CalendarDays[0].gameObject);
		}
	}

	public void Hide()
	{
		if (base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: false);
		}
		m_Player = null;
		m_Gamer = null;
	}

	private void Update()
	{
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer.GetButtonDown("UI_Cancel") && m_Player != null)
		{
			m_Player.RequestStopInteraction();
		}
	}

	private void SetCrossedDays()
	{
		for (int i = 0; i < m_CalendarDays.Count && i < 30; i++)
		{
			if (i < m_CurrentDayInMonth - 1 && m_CalendarDays[i].m_DayIcon != null)
			{
				m_CalendarDays[i].m_DayIcon.enabled = true;
				m_CalendarDays[i].m_DayIcon.sprite = m_CrossSprite;
			}
		}
	}

	private void SetEventDays()
	{
		RoutineManager.DayType[] calendarEvents = RoutineManager.GetInstance().m_CalendarEvents;
		for (int i = 0; i < calendarEvents.Length && i < 30; i++)
		{
			if (calendarEvents[i] != 0)
			{
				m_CalendarDays[i].m_DayIcon.enabled = true;
				m_CalendarDays[i].m_DayIcon.sprite = m_EventSprite;
			}
			else
			{
				m_CalendarDays[i].m_DayIcon.enabled = false;
				m_CalendarDays[i].m_DayIcon.sprite = null;
			}
		}
	}

	private void SetText()
	{
		if (m_MonthsText != null && m_CurrentMonth != -1)
		{
			Localization.Get("Text.Calendar.Month", out var localized);
			m_MonthsText.text = localized + " " + m_CurrentMonth;
		}
		if (m_DaysLeftText != null && m_DaysToRelease != -1)
		{
			Localization.Get("Text.Calendar.DaysToRelease", out var localized2);
			m_DaysLeftText.text = m_DaysToRelease + " " + localized2;
		}
	}

	public void UpdateCalendar()
	{
		if (!base.gameObject.GetActive())
		{
			return;
		}
		if (RoutineManager.GetInstance() != null)
		{
			m_CurrentMonth = RoutineManager.GetInstance().GetMonthNo() + 1;
			m_CurrentDayInMonth = RoutineManager.GetInstance().GetNoOfDaysIntoMonth() + 1;
			if (m_Player != null)
			{
				m_DaysToRelease = m_Player.m_CharacterStats.RemainingSentence;
			}
		}
		SetText();
		SetEventDays();
		SetCrossedDays();
	}
}

using UnityEngine;

public class CalendarDay : MonoBehaviour
{
	public T17Image m_DayIcon;

	public T17Text m_DayDate;

	public T17Button m_DayButton;

	[HideInInspector]
	public int m_Index = -1;

	[HideInInspector]
	public CalendarTooltip m_CalendarTooltip;

	private void Awake()
	{
		if (m_DayButton != null)
		{
			m_DayButton.OnButtonSelect = CalendarEntryWasSelected;
		}
	}

	public void CalendarEntryWasSelected(T17Button sender)
	{
		if (!(m_CalendarTooltip != null))
		{
			return;
		}
		if (m_CalendarTooltip.m_DayName != null)
		{
			RoutineManager.DayType dayType = RoutineManager.GetInstance().m_CalendarEvents[m_Index];
			m_CalendarTooltip.m_DayName.text = RoutineManager.GetInstance().GetTextForDayType(dayType);
			if (string.IsNullOrEmpty(m_CalendarTooltip.m_DayName.text))
			{
				m_CalendarTooltip.m_DayName.text = "Event Name Not Found";
			}
			m_CalendarTooltip.SetPositionWithOffset(base.transform.position);
		}
		m_CalendarTooltip.gameObject.SetActive(value: true);
	}
}

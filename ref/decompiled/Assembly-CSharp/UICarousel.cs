using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using T17.UI.Carousel;

public abstract class UICarousel<T> : UICarouselBase
{
	public List<T> m_Options;

	protected virtual void Start()
	{
		if (m_Options != null)
		{
			if (m_Options.Count == 0)
			{
			}
			if (m_CurrentIndex < m_Options.Count)
			{
				UpdateUIForSelectedIndex(m_CurrentIndex);
			}
		}
	}

	public void SelectPrevious()
	{
		if (!m_bBLockedInput && m_Options.Count != 0)
		{
			int currentIndex = m_CurrentIndex;
			currentIndex = ((currentIndex != 0) ? (currentIndex - 1) : (m_Options.Count - 1));
			SelectIndex(currentIndex, SelectionDirections.Previous);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Tab, AudioController.UI_Audio_GO);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_Off, AudioController.UI_Audio_GO);
		}
	}

	public void SelectNext()
	{
		if (!m_bBLockedInput && m_Options.Count != 0)
		{
			SelectIndex((m_CurrentIndex + 1) % m_Options.Count, SelectionDirections.Next);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Tab, AudioController.UI_Audio_GO);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Highlight_On, AudioController.UI_Audio_GO);
		}
	}

	public void SelectIndex(int index, SelectionDirections directionTravelledIn = SelectionDirections.Unassigned)
	{
		if (m_bBLockedInput)
		{
			return;
		}
		m_CurrentIndex = index;
		if (m_Options != null && m_CurrentIndex < m_Options.Count)
		{
			if (!m_bDeferUpdatingUI)
			{
				UpdateUI();
			}
			if (m_Options.Count > 0)
			{
				RaiseIndexSelected(index, directionTravelledIn);
			}
		}
	}

	public T GetSelectedItem()
	{
		if (m_CurrentIndex >= m_Options.Count)
		{
			return default(T);
		}
		return m_Options[m_CurrentIndex];
	}

	public void SetCarouselOptions(List<T> options, int defaultIndex = 0)
	{
		m_Options = options;
		if (defaultIndex != -1 && defaultIndex < options.Count)
		{
			m_CurrentIndex = defaultIndex;
			m_DefaultIndex = defaultIndex;
			if (m_CurrentIndex < m_Options.Count)
			{
				UpdateUIForSelectedIndex(m_CurrentIndex);
			}
		}
	}

	public void UpdateCarouselOptionsWithoutResetIndex(List<T> options)
	{
		m_Options = options;
		if (m_CurrentIndex >= m_Options.Count)
		{
			m_CurrentIndex = 0;
		}
		if (m_CurrentIndex < options.Count)
		{
			UpdateUIForSelectedIndex(m_CurrentIndex);
		}
	}

	public override int GetNumOptions()
	{
		if (m_Options == null)
		{
			return 0;
		}
		return m_Options.Count;
	}
}

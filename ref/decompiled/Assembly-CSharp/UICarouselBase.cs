using T17.UI.Carousel;
using UnityEngine;

public abstract class UICarouselBase : MonoBehaviour
{
	public int m_DefaultIndex;

	protected int m_CurrentIndex;

	public bool m_bBLockedInput;

	public bool m_bDeferUpdatingUI;

	public event IndexSelectedHandler IndexSelectedEvent;

	protected abstract void UpdateUIForSelectedIndex(int index);

	public abstract int GetNumOptions();

	protected virtual void Awake()
	{
		m_CurrentIndex = m_DefaultIndex;
	}

	protected virtual void OnDestroy()
	{
		if (this.IndexSelectedEvent != null)
		{
			this.IndexSelectedEvent = null;
		}
	}

	public int GetSelectedIndex()
	{
		return m_CurrentIndex;
	}

	public void UpdateUI()
	{
		if (m_CurrentIndex < GetNumOptions())
		{
			UpdateUIForSelectedIndex(m_CurrentIndex);
		}
	}

	protected void RaiseIndexSelected(int index, SelectionDirections directionTravelledIn)
	{
		if (this.IndexSelectedEvent != null)
		{
			this.IndexSelectedEvent(index, directionTravelledIn);
		}
	}
}

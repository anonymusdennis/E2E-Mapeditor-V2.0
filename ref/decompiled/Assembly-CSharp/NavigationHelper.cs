using UnityEngine;
using UnityEngine.UI;

public class NavigationHelper : MonoBehaviour
{
	public Selectable[] m_Up = new Selectable[0];

	private int m_UpIndex = -1;

	public Selectable[] m_Down = new Selectable[0];

	private int m_DownIndex = -1;

	public Selectable[] m_Left = new Selectable[0];

	private int m_LeftIndex = -1;

	public Selectable[] m_Right = new Selectable[0];

	private int m_RightIndex = -1;

	public Selectable m_OurControl;

	private Navigation m_Navigation = default(Navigation);

	public void Start()
	{
		if (m_OurControl == null)
		{
			m_OurControl = GetComponent<Selectable>();
		}
		if (m_OurControl == null)
		{
			base.enabled = false;
		}
		else if (m_OurControl.navigation.mode != Navigation.Mode.Explicit)
		{
			base.enabled = false;
		}
		else
		{
			m_Navigation = m_OurControl.navigation;
		}
	}

	private bool GetEnabledControl(ref Selectable[] someSelectables, ref Selectable currentSelectable, ref int iCurrentIndex)
	{
		bool flag = true;
		int num = someSelectables.Length;
		for (int i = 0; i < num; i++)
		{
			Selectable selectable = someSelectables[i];
			if (!(selectable != null))
			{
				continue;
			}
			flag = false;
			if (selectable.enabled && selectable.interactable && selectable.gameObject.activeSelf && selectable.gameObject.activeInHierarchy)
			{
				if (i != iCurrentIndex)
				{
					iCurrentIndex = i;
					currentSelectable = selectable;
					return true;
				}
				return false;
			}
		}
		if (flag)
		{
			return false;
		}
		if (iCurrentIndex != -1)
		{
			iCurrentIndex = -1;
			currentSelectable = null;
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (m_OurControl == null)
		{
			base.enabled = false;
			return;
		}
		bool flag = false;
		Selectable currentSelectable = m_Navigation.selectOnUp;
		if (GetEnabledControl(ref m_Up, ref currentSelectable, ref m_UpIndex))
		{
			flag = true;
			m_Navigation.selectOnUp = currentSelectable;
		}
		currentSelectable = m_Navigation.selectOnDown;
		if (GetEnabledControl(ref m_Down, ref currentSelectable, ref m_DownIndex))
		{
			flag = true;
			m_Navigation.selectOnDown = currentSelectable;
		}
		currentSelectable = m_Navigation.selectOnLeft;
		if (GetEnabledControl(ref m_Left, ref currentSelectable, ref m_LeftIndex))
		{
			flag = true;
			m_Navigation.selectOnLeft = currentSelectable;
		}
		currentSelectable = m_Navigation.selectOnRight;
		if (GetEnabledControl(ref m_Right, ref currentSelectable, ref m_RightIndex))
		{
			flag = true;
			m_Navigation.selectOnRight = currentSelectable;
		}
		if (flag)
		{
			m_OurControl.navigation = m_Navigation;
		}
	}
}

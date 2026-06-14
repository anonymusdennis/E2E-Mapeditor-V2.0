using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class T17NavigableGrid : BaseMenuBehaviour
{
	private struct LayoutSize
	{
		public int x;

		public int y;
	}

	public RectTransform m_ContentParent;

	public bool m_ContentIsDynamic;

	public bool m_GenerateNavigationLinks = true;

	protected List<RectTransform> m_ContentSelectables = new List<RectTransform>();

	protected int m_PreviousSelected;

	protected int m_CurrentSelected;

	private bool m_bAltElementLayout;

	private bool m_AltLayoutIsVertical;

	private LayoutSize m_AltLayoutSize = default(LayoutSize);

	private bool m_bContentElementSelected;

	private Selectable m_DataCache;

	protected bool isAltLayout => m_bAltElementLayout;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_ContentParent != null)
		{
			T17GridLayoutGroup component = m_ContentParent.GetComponent<T17GridLayoutGroup>();
			if (component != null)
			{
				m_AltLayoutSize.x = component.m_CellCountX;
				m_AltLayoutSize.y = component.m_CellCountY;
				m_AltLayoutIsVertical = component.startAxis == T17GridLayoutGroup.Axis.Vertical;
				m_bAltElementLayout = true;
			}
			if (!m_ContentIsDynamic)
			{
				for (int i = 0; i < m_ContentParent.transform.childCount; i++)
				{
					AddNewObject(m_ContentParent.transform.GetChild(i).gameObject);
				}
			}
			if (component != null)
			{
				component.ForceRefresh();
			}
		}
		if (m_TopSelectable != null)
		{
			T17Image component2 = m_TopSelectable.GetComponent<T17Image>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
		if (m_LeftSelectable != null)
		{
			T17Image component3 = m_LeftSelectable.GetComponent<T17Image>();
			if (component3 != null)
			{
				component3.enabled = false;
			}
		}
		if (m_RightSelectable != null)
		{
			T17Image component4 = m_RightSelectable.GetComponent<T17Image>();
			if (component4 != null)
			{
				component4.enabled = false;
			}
		}
		if (m_BottomSelectable != null)
		{
			T17Image component5 = m_BottomSelectable.GetComponent<T17Image>();
			if (component5 != null)
			{
				component5.enabled = false;
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		if (m_ContentIsDynamic)
		{
			if (m_ContentParent != null)
			{
				T17GridLayoutGroup component = m_ContentParent.GetComponent<T17GridLayoutGroup>();
				if (component != null)
				{
					m_AltLayoutSize.x = component.m_CellCountX;
					m_AltLayoutSize.y = component.m_CellCountY;
				}
			}
			m_ContentSelectables.Clear();
			for (int i = 0; i < m_ContentParent.transform.childCount; i++)
			{
				GameObject gameObject = m_ContentParent.transform.GetChild(i).gameObject;
				if (gameObject.activeSelf)
				{
					AddNewObject(gameObject);
				}
			}
			if (m_bAltElementLayout)
			{
				T17GridLayoutGroup component2 = m_ContentParent.GetComponent<T17GridLayoutGroup>();
				if (component2 != null)
				{
					component2.ForceRefresh();
				}
			}
		}
		return result;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		bool result = base.Hide(restoreInvokerState, isTabSwitch);
		if (m_ContentIsDynamic)
		{
			m_ContentSelectables.Clear();
		}
		return result;
	}

	protected override void Update()
	{
		base.Update();
		if (m_DataCache != null)
		{
			T17EventSystem t17EventSystem = null;
			t17EventSystem = ((base.CurrentGamer != null) ? T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer) : T17EventSystemsManager.Instance.GetEventSystemForGamer(Gamer.GetPrimaryGamer()));
			if (t17EventSystem == null)
			{
				m_DataCache = null;
				return;
			}
			GameObject lastRequestedSelectedGameobject = t17EventSystem.GetLastRequestedSelectedGameobject();
			t17EventSystem.SetSelectedGameObject(null);
			if (m_DataCache == m_TopSelectable)
			{
				if (m_bContentElementSelected && m_TopSelectable.navigation.selectOnUp != null && m_TopSelectable.navigation.selectOnUp.IsInteractable() && m_TopSelectable.navigation.selectOnUp.gameObject.activeInHierarchy)
				{
					m_bContentElementSelected = false;
					t17EventSystem.SetSelectedGameObject(m_TopSelectable.navigation.selectOnUp.gameObject);
				}
				else if (!ReselectCurrent(ref t17EventSystem))
				{
					GameObject gameObject = ((!(m_TopSelectable.navigation.selectOnUp == null) && m_TopSelectable.navigation.selectOnUp.IsInteractable() && m_TopSelectable.navigation.selectOnUp.gameObject.activeInHierarchy) ? m_TopSelectable.navigation.selectOnUp.gameObject : null);
					if (gameObject == null && lastRequestedSelectedGameobject != null)
					{
						t17EventSystem.SetSelectedGameObject(lastRequestedSelectedGameobject);
					}
					else
					{
						t17EventSystem.SetSelectedGameObject(gameObject);
					}
				}
			}
			else if (m_DataCache == m_BottomSelectable)
			{
				if (m_bContentElementSelected && m_BottomSelectable.navigation.selectOnDown != null && m_BottomSelectable.navigation.selectOnDown.IsInteractable() && m_BottomSelectable.navigation.selectOnDown.gameObject.activeInHierarchy)
				{
					m_bContentElementSelected = false;
					t17EventSystem.SetSelectedGameObject(m_BottomSelectable.navigation.selectOnDown.gameObject);
				}
				else if (!ReselectCurrent(ref t17EventSystem))
				{
					if (m_BottomSelectable.navigation.selectOnDown == null || !m_BottomSelectable.navigation.selectOnDown.IsInteractable() || !m_BottomSelectable.navigation.selectOnDown.gameObject.activeInHierarchy)
					{
						t17EventSystem.SetSelectedGameObject(null);
					}
					else
					{
						t17EventSystem.SetSelectedGameObject(m_BottomSelectable.navigation.selectOnDown.gameObject);
					}
				}
			}
			else if (m_DataCache == m_LeftSelectable)
			{
				if (m_bContentElementSelected && m_LeftSelectable.navigation.selectOnLeft != null && m_LeftSelectable.navigation.selectOnLeft.IsInteractable() && m_LeftSelectable.navigation.selectOnLeft.gameObject.activeInHierarchy)
				{
					m_bContentElementSelected = false;
					t17EventSystem.SetSelectedGameObject(m_LeftSelectable.navigation.selectOnLeft.gameObject);
				}
				else if (!ReselectCurrent(ref t17EventSystem))
				{
					if (m_LeftSelectable.navigation.selectOnLeft == null || !m_LeftSelectable.navigation.selectOnLeft.IsInteractable() || !m_LeftSelectable.navigation.selectOnLeft.gameObject.activeInHierarchy)
					{
						t17EventSystem.SetSelectedGameObject(null);
					}
					else
					{
						t17EventSystem.SetSelectedGameObject(m_LeftSelectable.navigation.selectOnLeft.gameObject);
					}
				}
			}
			else if (m_DataCache == m_RightSelectable)
			{
				if (m_bContentElementSelected && m_RightSelectable.navigation.selectOnRight != null && m_RightSelectable.navigation.selectOnRight.IsInteractable() && m_RightSelectable.navigation.selectOnRight.gameObject.activeInHierarchy)
				{
					m_bContentElementSelected = false;
					t17EventSystem.SetSelectedGameObject(m_RightSelectable.navigation.selectOnRight.gameObject);
				}
				else if (!ReselectCurrent(ref t17EventSystem))
				{
					if (m_RightSelectable.navigation.selectOnRight == null || !m_RightSelectable.navigation.selectOnRight.IsInteractable() || !m_RightSelectable.navigation.selectOnRight.gameObject.activeInHierarchy)
					{
						t17EventSystem.SetSelectedGameObject(null);
					}
					else
					{
						t17EventSystem.SetSelectedGameObject(m_RightSelectable.navigation.selectOnRight.gameObject);
					}
				}
			}
		}
		m_DataCache = null;
	}

	public virtual void AddNewObject(GameObject newObject)
	{
		if (newObject == null)
		{
			return;
		}
		RectTransform component = newObject.GetComponent<RectTransform>();
		if (m_ContentSelectables.Contains(component))
		{
			return;
		}
		m_ContentSelectables.Add(component);
		newObject.transform.SetParent(m_ContentParent.transform);
		newObject.transform.localScale = Vector3.one;
		newObject.transform.localPosition = Vector3.zero;
		int currentIndex = m_ContentSelectables.Count - 1;
		Selectable[] componentsInChildren = newObject.GetComponentsInChildren<Selectable>(includeInactive: true);
		SelectableGroup selectableGroup = null;
		if (componentsInChildren.Length > 1)
		{
			selectableGroup = newObject.AddComponent<SelectableGroup>();
		}
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			Selectable sel = componentsInChildren[num];
			if (sel == null)
			{
				sel = newObject.AddComponent<Selectable>();
			}
			T17_UISelectDeselectEvents t17_UISelectDeselectEvents = sel.GetComponent<T17_UISelectDeselectEvents>();
			if (t17_UISelectDeselectEvents == null)
			{
				t17_UISelectDeselectEvents = sel.gameObject.AddComponent<T17_UISelectDeselectEvents>();
			}
			if (t17_UISelectDeselectEvents != null)
			{
				t17_UISelectDeselectEvents.m_OnSelectEvent.AddListener(delegate
				{
					OnElementSelected(sel, currentIndex);
				});
			}
			if (selectableGroup != null)
			{
				selectableGroup.AddNewSelectable(sel);
			}
			if (!m_GenerateNavigationLinks)
			{
				continue;
			}
			Navigation navigation = sel.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (!m_bAltElementLayout)
			{
				if (currentIndex == 0)
				{
					navigation.selectOnUp = m_TopSelectable;
				}
				else
				{
					Selectable selectable = m_ContentSelectables[currentIndex - 1].GetComponent<Selectable>();
					if (selectable == null)
					{
						Selectable componentInChildren = m_ContentSelectables[currentIndex - 1].GetComponentInChildren<Selectable>(includeInactive: true);
						if (componentInChildren == null)
						{
							continue;
						}
						selectable = componentInChildren;
					}
					Navigation navigation2 = selectable.navigation;
					navigation.selectOnUp = selectable;
					navigation2.selectOnDown = sel;
					selectable.navigation = navigation2;
				}
				navigation.selectOnLeft = m_LeftSelectable;
				navigation.selectOnRight = m_RightSelectable;
				navigation.selectOnDown = m_BottomSelectable;
			}
			else
			{
				if (currentIndex == 0)
				{
					navigation.selectOnUp = m_TopSelectable;
					navigation.selectOnLeft = m_LeftSelectable;
				}
				else
				{
					RectTransform rectTransform = null;
					if (m_AltLayoutIsVertical)
					{
						if (currentIndex % m_AltLayoutSize.y > 0)
						{
							rectTransform = m_ContentSelectables[currentIndex - 1];
						}
					}
					else
					{
						int num2 = currentIndex - m_AltLayoutSize.x;
						if (num2 >= 0)
						{
							rectTransform = m_ContentSelectables[num2];
						}
					}
					RectTransform rectTransform2 = null;
					if (m_AltLayoutIsVertical)
					{
						int num3 = currentIndex - m_AltLayoutSize.y;
						if (num3 >= 0)
						{
							rectTransform2 = m_ContentSelectables[num3];
						}
					}
					else if (m_AltLayoutSize.x != 0 && currentIndex % m_AltLayoutSize.x > 0)
					{
						rectTransform2 = m_ContentSelectables[currentIndex - 1];
					}
					if (rectTransform != null)
					{
						Selectable selectable2 = (navigation.selectOnUp = rectTransform.GetComponent<Selectable>());
						Navigation navigation3 = selectable2.navigation;
						navigation3.selectOnDown = sel;
						selectable2.navigation = navigation3;
					}
					else
					{
						navigation.selectOnUp = m_TopSelectable;
					}
					if (rectTransform2 != null)
					{
						Selectable selectable3 = (navigation.selectOnLeft = rectTransform2.GetComponent<Selectable>());
						Navigation navigation4 = selectable3.navigation;
						navigation4.selectOnRight = sel;
						selectable3.navigation = navigation4;
					}
					else
					{
						navigation.selectOnLeft = m_LeftSelectable;
					}
				}
				navigation.selectOnRight = m_RightSelectable;
				navigation.selectOnDown = m_BottomSelectable;
			}
			sel.navigation = navigation;
			Navigation navigation5 = m_BottomSelectable.navigation;
			navigation5.selectOnUp = sel;
			m_BottomSelectable.navigation = navigation5;
		}
	}

	public virtual void SelectFirstElement()
	{
		if (m_ContentSelectables != null && m_ContentSelectables.Count > 0)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(null);
				eventSystemForGamer.SetSelectedGameObject(GetSelectableGameObject(m_ContentSelectables[0]));
			}
		}
	}

	protected virtual bool ReselectCurrent(ref T17EventSystem system)
	{
		if (m_ContentSelectables != null && m_ContentSelectables.Count > 0)
		{
			if (m_CurrentSelected >= 0 && m_CurrentSelected < m_ContentSelectables.Count)
			{
				system.SetSelectedGameObject(GetSelectableGameObject(m_ContentSelectables[m_CurrentSelected]));
				return true;
			}
			if (m_ContentSelectables[0] != null)
			{
				system.SetSelectedGameObject(GetSelectableGameObject(m_ContentSelectables[0]));
				return true;
			}
		}
		return false;
	}

	protected virtual void OnElementSelected(Selectable sel, int index)
	{
		m_bContentElementSelected = true;
		m_PreviousSelected = m_CurrentSelected;
		m_CurrentSelected = index;
	}

	public void RedirectEdgeLink(Selectable data)
	{
		m_DataCache = data;
	}

	public void ClearContents()
	{
		for (int num = m_ContentSelectables.Count - 1; num >= 0; num--)
		{
			RectTransform rectTransform = m_ContentSelectables[num];
			if (rectTransform != null && rectTransform.gameObject != null)
			{
				rectTransform.gameObject.SetActive(value: false);
				Object.Destroy(rectTransform.gameObject);
			}
		}
		m_ContentSelectables.Clear();
	}

	public GameObject GetSelectableGameObject(RectTransform transform)
	{
		SelectableGroup component = transform.GetComponent<SelectableGroup>();
		if (component != null && component.GetLastSelectedSelectableInGroup() != null)
		{
			return component.GetLastSelectedSelectableInGroup().gameObject;
		}
		Selectable[] componentsInChildren = transform.GetComponentsInChildren<Selectable>();
		if (componentsInChildren.Length > 0)
		{
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				if (componentsInChildren[num].navigation.selectOnDown != null || componentsInChildren[num].navigation.selectOnLeft != null || componentsInChildren[num].navigation.selectOnRight != null || componentsInChildren[num].navigation.selectOnUp != null)
				{
					return componentsInChildren[num].gameObject;
				}
			}
		}
		return null;
	}
}

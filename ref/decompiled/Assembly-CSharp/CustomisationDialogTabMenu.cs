using UnityEngine;
using UnityEngine.UI;

public class CustomisationDialogTabMenu : FrontendMenuBehaviour
{
	[Header("Tab Settings")]
	public T17Image m_NewIndicator;

	[Header("Navigation")]
	public Selectable m_ConfirmLink;

	public Selectable m_CancelLink;

	private Selectable m_ConfirmButton;

	private Selectable m_CancelButton;

	private Customisation m_Customisation;

	private CustomisationSet m_AvailableAppearances;

	private CustomisationSet m_NewAppearances;

	private CustomisationSet m_SeenAppearances;

	private CustomisationCategorisedSet m_CategorisedAppearances;

	protected Customisation modifiableCustomisation => m_Customisation;

	protected CustomisationSet availableAppearances => m_AvailableAppearances;

	protected CustomisationSet newAppearances => m_NewAppearances;

	protected CustomisationSet seenAppearances => m_SeenAppearances;

	protected CustomisationCategorisedSet categorisedAppearances => m_CategorisedAppearances;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		LinkSelectableNavigations(m_ConfirmLink, m_ConfirmButton);
		LinkSelectableNavigations(m_CancelLink, m_CancelButton);
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void SetModifiableCustomisation(Customisation customisation)
	{
		m_Customisation = customisation;
	}

	public void SetAvailableOptions(CustomisationSet available, CustomisationCategorisedSet categorised)
	{
		m_AvailableAppearances = available;
		m_CategorisedAppearances = categorised;
	}

	public void SetVisibilityOptions(CustomisationSet newOptions, CustomisationSet seenOptions)
	{
		m_NewAppearances = newOptions;
		m_SeenAppearances = seenOptions;
	}

	public virtual bool HasNewOptions()
	{
		return false;
	}

	public void UpdateNewOptionsIcon()
	{
		bool flag = HasAvailableOptions() && HasNewOptions();
		if (m_NewIndicator != null)
		{
			m_NewIndicator.enabled = flag;
		}
	}

	public virtual bool HasAvailableOptions()
	{
		return true;
	}

	public void SetDialogButtons(Selectable confirm, Selectable cancel)
	{
		m_ConfirmButton = confirm;
		m_CancelButton = cancel;
	}

	private void LinkSelectableNavigations(Selectable topLeft, Selectable bottomRight, bool horizontal = false)
	{
		if (topLeft != null)
		{
			Navigation navigation = topLeft.navigation;
			if (horizontal)
			{
				navigation.selectOnRight = bottomRight;
			}
			else
			{
				navigation.selectOnDown = bottomRight;
			}
			topLeft.navigation = navigation;
		}
		if (bottomRight != null)
		{
			Navigation navigation2 = bottomRight.navigation;
			if (horizontal)
			{
				navigation2.selectOnLeft = topLeft;
			}
			else
			{
				navigation2.selectOnUp = topLeft;
			}
			bottomRight.navigation = navigation2;
		}
	}
}

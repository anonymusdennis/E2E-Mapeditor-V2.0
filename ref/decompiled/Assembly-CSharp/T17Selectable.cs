using UnityEngine.UI;

public class T17Selectable : Selectable
{
	public bool m_bIsControllersAllowed;

	public override bool IsInteractable()
	{
		if (base.IsInteractable())
		{
			if (T17RewiredStandaloneInputModule.IsCurrentActiveModuleUsingController())
			{
				return m_bIsControllersAllowed;
			}
			return true;
		}
		return false;
	}
}

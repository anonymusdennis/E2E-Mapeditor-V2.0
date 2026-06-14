using UnityEngine;

public class EditorFlowInterface : MonoBehaviour
{
	public void ReturnToNormalFrontend()
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontendUserPath.ClearPath();
			FrontEndFlow.Instance.StartFrontEndFromLevelEditor();
		}
	}

	public void SwitchBackToFrontendMenu(FrontendMenuBehaviour menu)
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontendUserPath.ClearPath();
			FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
		}
	}

	public void SwitchToFrontendMenu(FrontendMenuBehaviour menu)
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.SwitchToFrontendMenu(menu);
		}
	}

	public void SwitchBackToMainMenu()
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontendUserPath.ClearPath();
			FrontEndFlow.Instance.SwitchBackToMainMenu();
		}
	}

	public void SwitchMenuNoAnim(BaseMenuBehaviour menu)
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.SwitchMenuNoAnim(menu);
		}
	}

	public void OpenChildOnTopOfMenu(int index)
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.OpenChildOnTopOfMenu(index);
		}
	}
}

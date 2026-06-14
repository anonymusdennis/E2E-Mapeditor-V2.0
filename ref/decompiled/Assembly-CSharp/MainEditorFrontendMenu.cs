using UnityEngine;

public class MainEditorFrontendMenu : FrontendMenuBehaviour
{
	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		Platform.GetInstance().SetPresenceTag("Text.Editor.Presence.FrontEndGeneral");
		if (Gamer.GetPrimaryGamer() != null && Gamer.GetPrimaryGamer().m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(Gamer.GetPrimaryGamer().m_RewiredPlayer, T17EventSystem.InputCateogryStates.LevelEditor);
		}
		return true;
	}

	public void OnSteamWorkshopButtonClicked()
	{
		if (Platform.GetInstance() != null)
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent(FrontEndFlow.MenuType.LevelEditor.ToString(), "SteamWorkshop", string.Empty, 0L);
			Platform.GetInstance().RequestShowUGCHomePage();
		}
	}
}

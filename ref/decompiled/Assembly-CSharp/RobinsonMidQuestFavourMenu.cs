using UnityEngine;

public class RobinsonMidQuestFavourMenu : FavourMenu
{
	private Player m_CurrentPlayer;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (currentGamer != null)
		{
			m_CurrentPlayer = currentGamer.m_PlayerObject;
		}
		return true;
	}

	public override void Cancel()
	{
		if (m_CurrentPlayer != null)
		{
			m_CurrentPlayer.RequestStopInteraction();
		}
		if (m_CurrentPlayer != null)
		{
			m_CurrentPlayer.RequestCloseContainer();
		}
	}
}

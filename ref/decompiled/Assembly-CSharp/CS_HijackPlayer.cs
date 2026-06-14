using System.Collections.Generic;

public class CS_HijackPlayer : CS_HijackIngameCharacter
{
	public enum Players
	{
		FirstPlayer,
		SecondPlayer,
		ThirdPlayer,
		FourthPlayer,
		LocalPrimaryGamer,
		ScriptedPlayer
	}

	public Players m_TargetPlayer;

	public override Character GetCharacterToHijack()
	{
		int targetPlayer = (int)m_TargetPlayer;
		if (targetPlayer <= 3)
		{
			Player playerNumber = GetPlayerNumber(targetPlayer);
			if (!(playerNumber == null))
			{
				return playerNumber;
			}
		}
		else
		{
			if (m_TargetPlayer == Players.LocalPrimaryGamer)
			{
				if (Gamer.GetPrimaryGamer() != null && Gamer.GetPrimaryGamer().m_PlayerObject != null)
				{
					return Gamer.GetPrimaryGamer().m_PlayerObject;
				}
				return GetPlayerNumber(0);
			}
			if (m_TargetPlayer == Players.ScriptedPlayer)
			{
				Player overridePlayer = CutsceneManagerBase.GetInstance().GetOverridePlayer();
				if (overridePlayer == null)
				{
					return GetPlayerNumber(0);
				}
				return overridePlayer;
			}
		}
		return null;
	}

	private Player GetPlayerNumber(int targetPlayer)
	{
		int num = 0;
		List<Player> allPlayers = Player.GetAllPlayers();
		allPlayers.Sort((Player p1, Player p2) => p1.m_NetView.viewID.CompareTo(p2.m_NetView.viewID));
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.m_Gamer != null)
			{
				if (num == targetPlayer)
				{
					return player;
				}
				num++;
			}
		}
		return null;
	}
}

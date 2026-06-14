using System.Collections.Generic;
using UnityEngine;

public class MultiplayerPuzzle_Door : MultiplayerPuzzle_Base
{
	[Header("Door Puzzle")]
	public Door m_Door;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		return base.StartInit();
	}

	protected override void OnPuzzleStateChanged(bool solved)
	{
		if (m_Door != null)
		{
			m_Door.SetForceOpen(solved);
		}
		DoorManager instance = DoorManager.GetInstance();
		if (instance != null)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				instance.SetUpCharacterKeys(allPlayers[i]);
			}
		}
	}
}

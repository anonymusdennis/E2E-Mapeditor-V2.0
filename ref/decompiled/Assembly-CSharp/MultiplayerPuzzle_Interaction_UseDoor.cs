using UnityEngine;

public class MultiplayerPuzzle_Interaction_UseDoor : MultiplayerPuzzle_Interaction
{
	public override void InteractionReadyUpdate()
	{
		base.InteractionReadyUpdate();
		MultiplayerPuzzle_Shutter multiplayerPuzzle_Shutter = (MultiplayerPuzzle_Shutter)m_Puzzle;
		if (multiplayerPuzzle_Shutter != null)
		{
			MultiplayerPuzzle_Shutter.ShutterRotation shutterDirection = multiplayerPuzzle_Shutter.m_ShutterDirection;
			Directionx4 direction = Directionx4.Up;
			switch (shutterDirection)
			{
			case MultiplayerPuzzle_Shutter.ShutterRotation.Horizontal:
				if (m_vStartingPosition.y > m_vInteractPosition.y)
				{
					direction = Directionx4.Down;
				}
				if (m_vStartingPosition.y < m_vInteractPosition.y)
				{
					direction = Directionx4.Up;
				}
				break;
			case MultiplayerPuzzle_Shutter.ShutterRotation.Vertical:
				if (m_vStartingPosition.x < m_vInteractPosition.x)
				{
					direction = Directionx4.Right;
				}
				if (m_vStartingPosition.x > m_vInteractPosition.x)
				{
					direction = Directionx4.Left;
				}
				break;
			}
			Vector3 vector = Direction.DirectionToVector(direction);
			Walk(vector);
		}
		else
		{
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter && null != localCharacter.m_CharacterStats && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			StatSystem.GetInstance().SetStat(13, 1f, ((Player)localCharacter).m_Gamer);
		}
	}

	public override bool InteractionVisibility()
	{
		return true;
	}

	public override bool CanStartOrContinueInteraction(Character character)
	{
		return m_bIsInteractionVisible;
	}

	public override void OnCharacterFailedToStart(Character character)
	{
		base.OnCharacterFailedToStart(character);
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			SpeechManager.GetInstance().SaySomething(character, "Text.Multiplayer.AccessPointBubble", SpeechTone.Negative, 3f, 10);
		}
	}
}

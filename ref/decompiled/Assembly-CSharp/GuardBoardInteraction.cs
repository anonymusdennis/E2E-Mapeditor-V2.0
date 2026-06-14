using AUTOGEN_T17Wwise_Enums;

public class GuardBoardInteraction : InteractiveObject
{
	public override bool AllowedToInteract(Character localCharacter)
	{
		return CanInteract();
	}

	private bool CanInteract()
	{
		return true;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				InGameMenuFlow.Instance.OpenGuardBoard(player, player.m_PlayerCameraManagerBindingID);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Guard Board Read", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Guard Board Read", string.Empty, 0L);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_In, AudioController.UI_Audio_GO);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if ((bool)player)
			{
				InGameMenuFlow.Instance.HideGuardBoard(player, player.m_PlayerCameraManagerBindingID);
			}
		}
		base.OnExitInteraction(localCharacter);
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}

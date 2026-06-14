using AUTOGEN_T17Wwise_Enums;

public class JobTutorialBoardInteraction : InteractiveObject
{
	public BaseJob m_Job;

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
		TryShowJobsTutorial();
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_In, AudioController.UI_Audio_GO);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != m_interactingCharacter && null != m_interactingCharacter && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if ((bool)player)
			{
				InGameMenuFlow.Instance.HideJobTutorialBoard(player, player.m_PlayerCameraManagerBindingID);
			}
		}
		base.OnExitInteraction(localCharacter);
	}

	private void TryShowJobsTutorial()
	{
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				InGameMenuFlow.Instance.OpenJobTutorialBoard(player, player.m_PlayerCameraManagerBindingID, m_Job);
			}
		}
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

using AUTOGEN_T17Wwise_Enums;

public class InformationBoardInteraction : InteractiveObject
{
	public string m_BoardTitleTag;

	public string m_BoardBodyTag;

	public bool m_bLogAnalytics;

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
				InGameMenuFlow.Instance.OpenInformationBoard(player, player.m_PlayerCameraManagerBindingID, m_BoardTitleTag, m_BoardBodyTag);
				if (m_bLogAnalytics)
				{
					GoogleAnalyticsV3.LogCommericalAnalyticEvent("Information Board Read", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Information Board Read", base.gameObject.name, 0L);
				}
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_In, AudioController.UI_Audio_GO);
		}
		m_interactingCharacter.m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Stand, force: true);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if ((bool)player)
			{
				InGameMenuFlow.Instance.HideInformationBoard(player, player.m_PlayerCameraManagerBindingID);
			}
		}
		base.OnExitInteraction(localCharacter);
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

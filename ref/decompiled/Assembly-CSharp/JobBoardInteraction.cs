public class JobBoardInteraction : InteractiveObject
{
	protected override void Init()
	{
		base.Init();
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
		{
			base.gameObject.SetActive(value: false);
		}
	}

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
		TryShowJobsBoard();
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (null != player)
			{
				InGameMenuFlow.Instance.HideJobsBoard(player, player.m_PlayerCameraManagerBindingID);
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

	private void TryShowJobsBoard()
	{
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				InGameMenuFlow.Instance.OpenJobsBoard(player, player.m_PlayerCameraManagerBindingID);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Job Board Read", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Job Board Read", string.Empty, 0L);
			}
		}
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}
}

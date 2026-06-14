using AUTOGEN_T17Wwise_Enums;

public class LockerInteraction : AnimatedInteraction
{
	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.m_bIsHidden = true;
		}
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.m_bIsHidden = false;
		}
	}

	public override void InteractionReadyEndEvent(Character interactingCharacter)
	{
		base.InteractionReadyEndEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
			{
				interactingCharacter.ShowNPCPin();
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Locker_Exit.ToString(), interactingCharacter.gameObject);
		}
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
			{
				interactingCharacter.HideNPCPin();
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Locker_Enter.ToString(), interactingCharacter.gameObject);
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

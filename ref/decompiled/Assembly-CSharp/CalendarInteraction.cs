public class CalendarInteraction : InteractiveObject
{
	public Character m_CalendarOwner;

	public void SetOwner(Character owner)
	{
		m_CalendarOwner = owner;
		if (!(owner == null) && m_NetObjectLock.m_TrackableElementReporter != null)
		{
			Localization.Get(m_NetObjectLock.m_InteractActionNameTag, out var localized);
			localized = ((!string.IsNullOrEmpty(localized)) ? localized : m_NetObjectLock.m_InteractActionNameTag);
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(owner.m_CharacterCustomisation.m_DisplayName);
		}
	}

	public Character GetOwner()
	{
		return m_CalendarOwner;
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
		if (!(localCharacter != m_CalendarOwner))
		{
			base.OnStartInteraction(localCharacter);
			TryShowCalendar();
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
	}

	private void TryShowCalendar()
	{
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				InGameMenuFlow.Instance.OpenCalendar(player, player.m_PlayerCameraManagerBindingID);
			}
		}
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

public class MinstrelJob : HandymanJob
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	protected override void InitInteraction(HandymanInteraction interaction)
	{
		base.InitInteraction(interaction);
		LuteInteraction luteInteraction = interaction as LuteInteraction;
		if (luteInteraction != null)
		{
			luteInteraction.m_MasherSettings = m_MasherSettings;
		}
	}
}

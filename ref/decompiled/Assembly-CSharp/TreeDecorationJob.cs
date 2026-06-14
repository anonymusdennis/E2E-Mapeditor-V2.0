public class TreeDecorationJob : DecayingItemHandymanJob
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	protected override void InitInteraction(HandymanInteraction interaction)
	{
		base.InitInteraction(interaction);
		(interaction as TreeDecorationInteraction).m_MasherSettings = m_MasherSettings;
	}
}

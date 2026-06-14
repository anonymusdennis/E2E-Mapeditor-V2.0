public class ElectricianJob : HandymanJob
{
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	protected override void InitInteraction(HandymanInteraction interaction)
	{
		base.InitInteraction(interaction);
		(interaction as ElectricianInteraction).m_MasherSettings = m_MasherSettings;
	}
}

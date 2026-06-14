public class ControlOption
{
	public ControlSetting m_EnumValue;

	public string m_LocalisationTag;

	public ControlOption(ControlSetting enumValue, string localisationTag)
	{
		m_EnumValue = enumValue;
		m_LocalisationTag = localisationTag;
	}
}

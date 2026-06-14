using Slate;

namespace T17.Slate;

[Category("T17 Environment Settings")]
public class SetWeatherEnabled : DirectorActionClip
{
	public bool m_bWeatherEnabled;

	public override string info => "Set weather enabled: " + m_bWeatherEnabled;

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_bWeatherEnabled)
		{
			WeatherEffectManager.Enable();
		}
		else
		{
			WeatherEffectManager.Disable();
		}
	}
}

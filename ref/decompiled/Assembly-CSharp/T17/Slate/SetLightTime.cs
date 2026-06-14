using Slate;

namespace T17.Slate;

[Category("T17 Environment Settings")]
public class SetLightTime : DirectorActionClip
{
	public UIAnimatedEffectController.Effects m_Mode = UIAnimatedEffectController.Effects.FadeToTransparent;

	public int m_Hour;

	public int m_Min;

	public override string info => "Set time to " + m_Hour + ":" + m_Min;

	protected override void OnEnter()
	{
		base.OnEnter();
		LightingManager instance = LightingManager.GetInstance();
		if (instance != null)
		{
			instance.SetTimeOverride(m_Hour, m_Min);
		}
	}
}

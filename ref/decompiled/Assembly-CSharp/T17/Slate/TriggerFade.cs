using Slate;

namespace T17.Slate;

[Category("T17 Camera Effects")]
public class TriggerFade : DirectorActionClip
{
	public UIAnimatedEffectController.Effects m_Mode = UIAnimatedEffectController.Effects.FadeToTransparent;

	public float m_FadeTime = 1f;

	public override string info => m_FadeTime + "s Fullscreen " + m_Mode.ToString();

	protected override void OnEnter()
	{
		base.OnEnter();
		if (HUDMenuFlow.Instance != null && CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			HUDMenuFlow.Instance.PlayGlobalEffect(m_Mode, m_FadeTime);
		}
	}
}

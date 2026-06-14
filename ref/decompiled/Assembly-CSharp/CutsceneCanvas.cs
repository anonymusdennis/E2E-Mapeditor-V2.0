using UnityEngine;

public class CutsceneCanvas : MonoBehaviour
{
	public T17Text m_SkipLabel;

	public UIAnimatedEffectController m_EffectController;

	[HideInInspector]
	public float m_SkipTextDuration = 3f;

	private float m_TimeUntilSkipTextDisppears;

	private void Awake()
	{
		m_SkipLabel.gameObject.SetActive(value: false);
		if (CutsceneManagerBase.GetInstance() != null)
		{
			CutsceneManagerBase.GetInstance().m_OverarchingCanvas = this;
		}
	}

	public void PlayEffect(UIAnimatedEffectController.Effects effect, float time)
	{
		m_EffectController.PlayEffect(effect, time);
	}

	public void ResetEffects()
	{
		m_EffectController.ResetAllEffects();
	}

	public void OnCutsceneFinished()
	{
		m_SkipLabel.gameObject.SetActive(value: false);
	}

	public void ShowSkipText()
	{
		m_TimeUntilSkipTextDisppears = m_SkipTextDuration;
		m_SkipLabel.gameObject.SetActive(value: true);
	}

	private void Update()
	{
		if (m_TimeUntilSkipTextDisppears > 0f)
		{
			m_TimeUntilSkipTextDisppears -= Time.deltaTime;
			if (m_TimeUntilSkipTextDisppears <= 0f)
			{
				m_SkipLabel.gameObject.SetActive(value: false);
			}
		}
	}
}

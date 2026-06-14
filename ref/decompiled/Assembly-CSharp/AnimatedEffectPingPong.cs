using UnityEngine;

[RequireComponent(typeof(UIAnimatedEffect))]
public class AnimatedEffectPingPong : MonoBehaviour
{
	public delegate void StartingHoldHandler();

	public delegate void FinishedHoldHandler();

	public enum Stages
	{
		Idle,
		Start,
		Stop,
		Hold
	}

	private StartingHoldHandler m_StartingHoldCallback;

	private FinishedHoldHandler m_FinishedHoldCallback;

	private Stages m_CurrentStage;

	private float m_StartStopTime;

	private float m_HoldTime;

	private float m_TimeUntilNextStage;

	private UIAnimatedEffect m_LetterBoxEffect;

	private void Awake()
	{
		m_LetterBoxEffect = GetComponent<UIAnimatedEffect>();
		base.enabled = false;
		m_CurrentStage = Stages.Idle;
	}

	public void StartEffectForPlayer(float fadeTime, float holdTime, StartingHoldHandler startingHoldCallback = null, FinishedHoldHandler finishedHoldCallback = null)
	{
		if (m_CurrentStage == Stages.Idle)
		{
			base.enabled = true;
			m_CurrentStage = Stages.Start;
			m_StartStopTime = fadeTime;
			m_HoldTime = holdTime;
			m_TimeUntilNextStage = fadeTime;
			m_StartingHoldCallback = startingHoldCallback;
			m_FinishedHoldCallback = finishedHoldCallback;
			m_LetterBoxEffect.TriggerStart(fadeTime);
		}
	}

	private void Update()
	{
		if (m_CurrentStage == Stages.Idle)
		{
			return;
		}
		m_TimeUntilNextStage -= UpdateManager.deltaTime;
		if (!(m_TimeUntilNextStage <= 0f))
		{
			return;
		}
		switch (m_CurrentStage)
		{
		case Stages.Start:
			m_TimeUntilNextStage = m_HoldTime;
			m_CurrentStage = Stages.Hold;
			if (m_StartingHoldCallback != null)
			{
				m_StartingHoldCallback();
			}
			break;
		case Stages.Hold:
			m_TimeUntilNextStage = m_StartStopTime;
			m_CurrentStage = Stages.Stop;
			m_LetterBoxEffect.TriggerReverse(m_StartStopTime);
			if (m_FinishedHoldCallback != null)
			{
				m_FinishedHoldCallback();
			}
			break;
		case Stages.Stop:
			m_CurrentStage = Stages.Idle;
			base.enabled = false;
			break;
		}
	}
}

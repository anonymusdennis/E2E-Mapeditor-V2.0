using UnityEngine;

public class TimedLerpingMonobehaviour : MonoBehaviour
{
	public delegate void LerpFinishedHandler(TimedLerpingMonobehaviour sender);

	public float m_EffectTime = 2f;

	public bool m_bDisableOnFinish = true;

	protected bool m_bIsReverse;

	protected float m_TimeUntilCompletion = -1f;

	public event LerpFinishedHandler LerpFinishedEvent;

	protected virtual void Awake()
	{
		base.enabled = false;
	}

	protected void LateUpdate()
	{
		if (!(m_TimeUntilCompletion >= 0f))
		{
			return;
		}
		m_TimeUntilCompletion -= Time.unscaledDeltaTime;
		DoWork();
		if (m_TimeUntilCompletion <= 0f && m_bDisableOnFinish)
		{
			base.enabled = false;
			if (this.LerpFinishedEvent != null)
			{
				this.LerpFinishedEvent(this);
			}
		}
	}

	protected virtual void DoWork()
	{
	}

	protected float GetLerpValue()
	{
		float num = m_TimeUntilCompletion / m_EffectTime;
		if (!m_bIsReverse)
		{
			return 1f - num;
		}
		return num;
	}

	protected virtual void Reset()
	{
		m_bIsReverse = false;
		m_TimeUntilCompletion = -1f;
	}

	protected virtual void StartEffect(bool isReversed, bool disableOnFinish = false)
	{
		m_bIsReverse = isReversed;
		m_bDisableOnFinish = disableOnFinish;
		m_TimeUntilCompletion = m_EffectTime;
		base.enabled = true;
	}

	public void Stop()
	{
		base.enabled = false;
		Reset();
	}

	public float GetTimeUntilCompletion()
	{
		return m_TimeUntilCompletion;
	}
}

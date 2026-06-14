using UnityEngine;

public class VectorShake
{
	public float m_MaxRadius;

	public bool m_bLockZ = true;

	public float m_EffectTime = 1f;

	private float m_timeUntilEffectFinishes;

	private bool m_IsActive;

	private Vector3 m_CachedShakeVector = Vector3.zero;

	public void Init(float effectTime, float maxRadius)
	{
		m_EffectTime = effectTime;
		m_MaxRadius = maxRadius;
		Init();
	}

	public void Init()
	{
		m_timeUntilEffectFinishes = m_EffectTime;
		m_IsActive = true;
		RecalculateCachedShakeVector();
	}

	public void Update(float dt)
	{
		m_timeUntilEffectFinishes -= dt;
		if (m_timeUntilEffectFinishes <= 0f)
		{
			m_IsActive = false;
		}
		if (dt > 0f)
		{
			RecalculateCachedShakeVector();
		}
	}

	public Vector3 GetVector()
	{
		if (!IsActive())
		{
			return Vector3.zero;
		}
		return m_CachedShakeVector;
	}

	private void RecalculateCachedShakeVector()
	{
		m_CachedShakeVector = Random.insideUnitSphere * GetRatio() * m_MaxRadius;
		if (m_bLockZ)
		{
			m_CachedShakeVector.z = 0f;
		}
	}

	public float GetRatio()
	{
		return 1f - m_timeUntilEffectFinishes / m_EffectTime;
	}

	public bool IsActive()
	{
		return m_IsActive;
	}
}

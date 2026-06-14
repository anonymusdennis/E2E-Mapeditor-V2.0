using System;
using UnityEngine;

public class LightEffect_LockdownFadeOut : LightEffect
{
	[Serializable]
	private class SerializableData
	{
		public FadeInfo fadeOut;

		public FadeInfo fadeIn;
	}

	[Serializable]
	private class FadeInfo
	{
		public float delay;

		public float duration;
	}

	private FadeInfo m_FadeOutInfo = new FadeInfo();

	private FadeInfo m_FadeInInfo = new FadeInfo();

	private float m_FadeTimer;

	private float m_StartTimer;

	private float m_Duration;

	private float[] m_IntensityStarts = new float[0];

	private float[] m_IntensityEnds = new float[0];

	private bool m_bIsLockdownActive;

	public override Effects GetEffectType()
	{
		return Effects.LockdownFadeOut;
	}

	public override bool Init(LightingManager.LightGroup group)
	{
		if (SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().onLockdownChanged += OnLockdownStatusChanged;
		}
		int count = group.m_Lights.Count;
		m_IntensityStarts = new float[count];
		m_IntensityEnds = new float[count];
		return base.Init(group);
	}

	public override void OnGroupTurnedOn()
	{
		base.OnGroupTurnedOn();
	}

	public override void OnGroupTurnedOff()
	{
		base.OnGroupTurnedOff();
	}

	public override void OnGroupUpdated(float deltaTime)
	{
		base.OnGroupUpdated(deltaTime);
	}

	public override void UpdateEffect(float deltaTime)
	{
		base.UpdateEffect(deltaTime);
		if (m_StartTimer > 0f)
		{
			m_StartTimer -= deltaTime;
			return;
		}
		if (m_FadeTimer >= m_Duration || m_Duration <= 0f)
		{
			m_bIsActive = false;
			return;
		}
		m_FadeTimer += deltaTime;
		float t = Mathf.Clamp01(m_FadeTimer / m_Duration);
		for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
		{
			float intensity = Mathf.Lerp(m_IntensityStarts[i], m_IntensityEnds[i], t);
			m_ParentGroup.m_Lights[i].SetIntensity(intensity);
		}
	}

	private void OnLockdownStatusChanged(bool isLockdown)
	{
		if (isLockdown == m_bIsLockdownActive)
		{
			return;
		}
		m_bIsLockdownActive = isLockdown;
		m_bIsActive = true;
		if (m_bIsLockdownActive)
		{
			m_FadeTimer = 0f;
			m_StartTimer = m_FadeOutInfo.delay;
			m_Duration = m_FadeOutInfo.duration;
			for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
			{
				m_IntensityStarts[i] = m_ParentGroup.m_Lights[i].GetStoredIntensity();
				m_IntensityEnds[i] = 0f;
			}
		}
		else
		{
			m_FadeTimer = 0f;
			m_StartTimer = m_FadeInInfo.delay;
			m_Duration = m_FadeInInfo.duration;
			for (int j = 0; j < m_ParentGroup.m_Lights.Count; j++)
			{
				m_IntensityStarts[j] = 0f;
				m_IntensityEnds[j] = m_ParentGroup.m_Lights[j].GetStoredIntensity();
			}
		}
	}

	public override string Serialize()
	{
		SerializableData serializableData = new SerializableData();
		serializableData.fadeOut = m_FadeOutInfo;
		serializableData.fadeIn = m_FadeInInfo;
		return JsonUtility.ToJson(serializableData);
	}

	public override bool Deserialize(string serialized)
	{
		SerializableData serializableData = JsonUtility.FromJson<SerializableData>(serialized);
		if (serializableData == null)
		{
			return false;
		}
		m_FadeOutInfo = serializableData.fadeOut;
		m_FadeInInfo = serializableData.fadeIn;
		return true;
	}
}

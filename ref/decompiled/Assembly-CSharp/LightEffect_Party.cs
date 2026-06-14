using System;
using UnityEngine;

public class LightEffect_Party : LightEffect
{
	[Serializable]
	private class SerializableData
	{
		public float hueChangeRate;
	}

	private float m_HueChangeRate = 1f;

	private float[] m_HueProgression;

	private bool m_bIsLockdownActive;

	public override Effects GetEffectType()
	{
		return Effects.Party;
	}

	public override bool Init(LightingManager.LightGroup group)
	{
		if (SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().onLockdownChanged += OnLockdownStatusChanged;
		}
		m_HueProgression = new float[group.m_Lights.Count];
		for (int i = 0; i < group.m_Lights.Count; i++)
		{
			m_HueProgression[i] = UnityEngine.Random.value;
		}
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
		if (m_bIsLockdownActive == m_bIsActive)
		{
			return;
		}
		m_bIsActive = m_bIsLockdownActive;
		if (!m_bIsLockdownActive)
		{
			for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
			{
				m_ParentGroup.m_Lights[i].ResetColour();
			}
		}
	}

	public override void UpdateEffect(float deltaTime)
	{
		base.UpdateEffect(deltaTime);
		for (int i = 0; i < m_HueProgression.Length; i++)
		{
			m_HueProgression[i] += m_HueChangeRate * deltaTime;
			if (m_HueProgression[i] >= 1f)
			{
				m_HueProgression[i] -= 1f;
			}
		}
		for (int j = 0; j < m_ParentGroup.m_Lights.Count; j++)
		{
			Color colour = Color.HSVToRGB(m_HueProgression[j], 1f, 1f);
			m_ParentGroup.m_Lights[j].SetColour(colour);
		}
	}

	private void OnLockdownStatusChanged(bool isLockdown)
	{
		m_bIsLockdownActive = isLockdown;
	}

	public override string Serialize()
	{
		SerializableData serializableData = new SerializableData();
		serializableData.hueChangeRate = m_HueChangeRate;
		return JsonUtility.ToJson(serializableData);
	}

	public override bool Deserialize(string serialized)
	{
		SerializableData serializableData = JsonUtility.FromJson<SerializableData>(serialized);
		if (serializableData == null)
		{
			return false;
		}
		m_HueChangeRate = serializableData.hueChangeRate;
		return true;
	}
}

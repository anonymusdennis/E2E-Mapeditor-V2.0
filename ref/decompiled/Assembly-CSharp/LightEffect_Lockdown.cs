using System;
using UnityEngine;

public class LightEffect_Lockdown : LightEffect
{
	[Serializable]
	private class SerializableData
	{
		[Serializable]
		public class SerializableKeyFrame
		{
			public float time;

			public float value;

			public float inTangent;

			public float outTangent;
		}

		public Color colour = Color.white;

		public float pulseDuration;

		public float pulseMin;

		public float pulseMax;

		public float startDelay;

		public SerializableKeyFrame[] pulseKeyFrames;
	}

	private float m_StartDelay;

	private Color m_LightColour = Color.white;

	private float m_PulseDuration;

	private float m_PulseIntensityMin;

	private float m_PulseIntensityMax;

	private AnimationCurve m_PulseShape = new AnimationCurve();

	private bool m_bIsLockdownActive;

	private float m_StartTimer;

	private float m_PulseTimer;

	public override Effects GetEffectType()
	{
		return Effects.Lockdown;
	}

	public override bool Init(LightingManager.LightGroup group)
	{
		if (SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().onLockdownChanged += OnLockdownStatusChanged;
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
	}

	public override void UpdateEffect(float deltaTime)
	{
		base.UpdateEffect(deltaTime);
		if (m_StartTimer > 0f)
		{
			m_StartTimer -= deltaTime;
		}
		else
		{
			if (m_PulseDuration <= 0f)
			{
				return;
			}
			float intensity = 1f;
			if (m_PulseDuration > 0f)
			{
				m_PulseTimer += deltaTime;
				if (m_PulseTimer >= m_PulseDuration)
				{
					m_PulseTimer = 0f;
				}
				float time = Mathf.Clamp01(m_PulseTimer / m_PulseDuration);
				float num = m_PulseShape.Evaluate(time);
				float num2 = m_PulseIntensityMax - m_PulseIntensityMin;
				intensity = m_PulseIntensityMin + num * num2;
			}
			for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
			{
				m_ParentGroup.m_Lights[i].SetColour(m_LightColour);
				m_ParentGroup.m_Lights[i].SetIntensity(intensity);
			}
		}
	}

	private void OnLockdownStatusChanged(bool isLockdown)
	{
		if (isLockdown == m_bIsLockdownActive)
		{
			return;
		}
		m_bIsLockdownActive = isLockdown;
		m_bIsActive = m_bIsLockdownActive;
		m_PulseTimer = 0f;
		m_StartTimer = 0f;
		if (m_bIsLockdownActive)
		{
			m_StartTimer = m_StartDelay;
			return;
		}
		for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
		{
			m_ParentGroup.m_Lights[i].ResetColour();
			m_ParentGroup.m_Lights[i].ResetIntensity();
		}
	}

	public override string Serialize()
	{
		SerializableData serializableData = new SerializableData();
		serializableData.colour = m_LightColour;
		serializableData.pulseDuration = m_PulseDuration;
		serializableData.pulseMin = m_PulseIntensityMin;
		serializableData.pulseMax = m_PulseIntensityMax;
		serializableData.startDelay = m_StartDelay;
		serializableData.pulseKeyFrames = new SerializableData.SerializableKeyFrame[m_PulseShape.keys.Length];
		for (int i = 0; i < m_PulseShape.keys.Length; i++)
		{
			SerializableData.SerializableKeyFrame serializableKeyFrame = new SerializableData.SerializableKeyFrame();
			serializableKeyFrame.time = m_PulseShape.keys[i].time;
			serializableKeyFrame.value = m_PulseShape.keys[i].value;
			serializableKeyFrame.inTangent = m_PulseShape.keys[i].inTangent;
			serializableKeyFrame.outTangent = m_PulseShape.keys[i].outTangent;
			serializableData.pulseKeyFrames[i] = serializableKeyFrame;
		}
		return JsonUtility.ToJson(serializableData);
	}

	public override bool Deserialize(string serialized)
	{
		SerializableData serializableData = JsonUtility.FromJson<SerializableData>(serialized);
		if (serializableData == null)
		{
			return false;
		}
		m_LightColour = serializableData.colour;
		m_PulseDuration = serializableData.pulseDuration;
		m_PulseIntensityMin = serializableData.pulseMin;
		m_PulseIntensityMax = serializableData.pulseMax;
		m_StartDelay = serializableData.startDelay;
		SerializableData.SerializableKeyFrame[] pulseKeyFrames = serializableData.pulseKeyFrames;
		if (pulseKeyFrames != null && pulseKeyFrames.Length > 0)
		{
			Keyframe[] array = new Keyframe[pulseKeyFrames.Length];
			for (int i = 0; i < pulseKeyFrames.Length; i++)
			{
				ref Keyframe reference = ref array[i];
				reference = new Keyframe(pulseKeyFrames[i].time, pulseKeyFrames[i].value, pulseKeyFrames[i].inTangent, pulseKeyFrames[i].outTangent);
			}
			m_PulseShape = new AnimationCurve(array);
		}
		else
		{
			m_PulseShape = new AnimationCurve();
		}
		return true;
	}
}

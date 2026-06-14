using System;
using UnityEngine;

public class LightEffect_ShowTime : LightEffect
{
	[Serializable]
	private class SerializableDataA
	{
		public bool m_bLastLightIsFollowSpot = true;

		public float m_StartDelay;

		public float m_HoldDelay = 5f;

		public Vector3 m_CentreOffset;

		public float m_Speed = 1f;

		public float m_InnerRadius = 1f;

		public float m_IntensitySpeed = 0.1f;

		public float m_MaxIntensityMul = 2f;

		public float m_MinIntensityMul = 0.5f;

		public float m_FollowSpotIntensityMul = 1f;
	}

	public class EffectData
	{
		public Vector3 m_Dir;

		public Vector3 m_StartingPoint;

		public Vector3 m_TargetPoint;

		public bool m_bInWard;

		public bool m_bOutWard;

		public float m_IntensityMul;

		public float m_IntensityDir;

		public float m_InTimer;

		public bool m_bStayOnTarget;
	}

	public enum STAGE
	{
		In,
		Hold,
		Out
	}

	private float m_StartDelay;

	private float m_HoldDelay = 5f;

	private Vector3 m_CentreOffset;

	private Vector3 m_MaxShowTimeArea;

	private Vector3 m_MinShowTimeArea;

	private Character m_TargetWarden;

	private float m_Speed = 1f;

	private float m_InnerRadius = 1f;

	private float m_IntensitySpeed = 0.1f;

	private float m_MaxIntensityMul = 2f;

	private float m_MinIntensityMul = 0.5f;

	private float m_FollowSpotIntensityMul = 1f;

	private bool m_bInHold;

	private bool m_bJustGoneOut;

	private bool m_bDoFunArc;

	private bool m_bDoFunArcNext;

	private Vector3 m_CentrePoint = default(Vector3);

	private bool m_bLastLightIsFollowSpot = true;

	private bool m_bInitialStartDelay = true;

	private int m_LightLoopCount;

	private EffectData[] m_EffectDatas;

	private float m_StartTimer;

	public override Effects GetEffectType()
	{
		return Effects.ShowTimeEffect;
	}

	public override bool Init(LightingManager.LightGroup group)
	{
		bool result = base.Init(group);
		m_CentrePoint.Set(0f, 0f, 0f);
		m_EffectDatas = new EffectData[m_ParentGroup.m_Lights.Count];
		if (m_TargetWarden == null)
		{
			Character[] array = UnityEngine.Object.FindObjectsOfType<Character>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].m_CharacterRole == CharacterRole.Warden)
				{
					m_TargetWarden = array[i];
					break;
				}
			}
		}
		if (m_bLastLightIsFollowSpot)
		{
			m_LightLoopCount = m_ParentGroup.m_Lights.Count - 1;
		}
		else
		{
			m_LightLoopCount = m_ParentGroup.m_Lights.Count;
		}
		m_MaxShowTimeArea.x = -10000f;
		m_MaxShowTimeArea.y = -10000f;
		m_MinShowTimeArea.x = 10000f;
		m_MinShowTimeArea.y = 10000f;
		for (int i = 0; i < m_LightLoopCount; i++)
		{
			Vector3 position = m_ParentGroup.m_Lights[i].transform.position;
			m_CentrePoint += position;
			m_EffectDatas[i] = new EffectData();
			if (position.x > m_MaxShowTimeArea.x)
			{
				m_MaxShowTimeArea.x = position.x;
			}
			if (position.y > m_MaxShowTimeArea.y)
			{
				m_MaxShowTimeArea.y = position.y;
			}
			if (position.x < m_MinShowTimeArea.x)
			{
				m_MinShowTimeArea.x = position.x;
			}
			if (position.y < m_MinShowTimeArea.y)
			{
				m_MinShowTimeArea.y = position.y;
			}
		}
		m_CentrePoint /= (float)m_LightLoopCount;
		for (int i = 0; i < m_LightLoopCount; i++)
		{
			m_EffectDatas[i].m_StartingPoint = m_ParentGroup.m_Lights[i].transform.position;
		}
		if (m_bLastLightIsFollowSpot)
		{
			m_EffectDatas[m_LightLoopCount] = new EffectData();
			m_EffectDatas[m_LightLoopCount].m_StartingPoint = m_ParentGroup.m_Lights[m_LightLoopCount].transform.position;
		}
		return result;
	}

	public override void OnGroupTurnedOn()
	{
		base.OnGroupTurnedOn();
		m_bIsActive = true;
		m_StartTimer = m_StartDelay;
		m_bInitialStartDelay = true;
		m_bDoFunArcNext = false;
		m_bDoFunArc = false;
		for (int i = 0; i < m_LightLoopCount; i++)
		{
			m_ParentGroup.m_Lights[i].transform.position = m_EffectDatas[i].m_StartingPoint;
			m_EffectDatas[i].m_Dir = m_CentrePoint + m_CentreOffset - m_ParentGroup.m_Lights[i].transform.position;
			m_EffectDatas[i].m_Dir.z = 0f;
			m_EffectDatas[i].m_Dir.Normalize();
			m_EffectDatas[i].m_TargetPoint = m_CentrePoint + m_CentreOffset;
			m_EffectDatas[i].m_bInWard = true;
			m_EffectDatas[i].m_bOutWard = false;
			m_EffectDatas[i].m_IntensityMul = 0f;
			m_EffectDatas[i].m_IntensityDir = 1f;
			m_EffectDatas[i].m_InTimer = 0f;
			m_EffectDatas[i].m_bStayOnTarget = false;
			m_ParentGroup.m_Lights[i].SetIntensityMultiplier(0f);
		}
		if (m_bLastLightIsFollowSpot)
		{
			m_ParentGroup.m_Lights[m_LightLoopCount].transform.position = m_EffectDatas[m_LightLoopCount].m_StartingPoint;
			m_EffectDatas[m_LightLoopCount].m_TargetPoint = m_EffectDatas[m_LightLoopCount].m_StartingPoint;
			m_EffectDatas[m_LightLoopCount].m_bStayOnTarget = true;
			m_EffectDatas[m_LightLoopCount].m_IntensityMul = 0f;
			m_EffectDatas[m_LightLoopCount].m_IntensityDir = 1f;
			m_ParentGroup.m_Lights[m_LightLoopCount].SetIntensityMultiplier(0f);
		}
		m_bInHold = false;
		m_bJustGoneOut = false;
	}

	public override void OnGroupTurnedOff()
	{
		base.OnGroupTurnedOff();
		m_bIsActive = false;
	}

	public override void OnGroupUpdated(float deltaTime)
	{
		base.OnGroupUpdated(deltaTime);
	}

	public override void UpdateEffect(float deltaTime)
	{
		base.UpdateEffect(deltaTime);
		if (m_bLastLightIsFollowSpot && !m_bInitialStartDelay)
		{
			Vector3 position = m_ParentGroup.m_Lights[m_LightLoopCount].transform.position;
			if (m_TargetWarden != null)
			{
				position.x = m_TargetWarden.transform.position.x;
				position.y = m_TargetWarden.transform.position.y;
				if (position.x > m_MaxShowTimeArea.x)
				{
					position.x = m_MaxShowTimeArea.x;
				}
				if (position.y > m_MaxShowTimeArea.y)
				{
					position.y = m_MaxShowTimeArea.y;
				}
				if (position.x < m_MinShowTimeArea.x)
				{
					position.x = m_MinShowTimeArea.x;
				}
				if (position.y < m_MinShowTimeArea.y)
				{
					position.y = m_MinShowTimeArea.y;
				}
			}
			m_ParentGroup.m_Lights[m_LightLoopCount].transform.position = position;
			m_EffectDatas[m_LightLoopCount].m_IntensityMul += m_EffectDatas[m_LightLoopCount].m_IntensityDir * 2f * m_IntensitySpeed * deltaTime;
			if (m_EffectDatas[m_LightLoopCount].m_IntensityMul > m_FollowSpotIntensityMul)
			{
				m_EffectDatas[m_LightLoopCount].m_IntensityMul = m_FollowSpotIntensityMul;
				m_EffectDatas[m_LightLoopCount].m_IntensityDir = 0f;
			}
			m_ParentGroup.m_Lights[m_LightLoopCount].SetIntensityMultiplier(m_EffectDatas[m_LightLoopCount].m_IntensityMul);
		}
		if (m_StartTimer > 0f)
		{
			m_StartTimer -= deltaTime;
			return;
		}
		m_bInitialStartDelay = false;
		if (!m_bInHold)
		{
			bool flag = true;
			if (m_bDoFunArc)
			{
				flag = false;
			}
			for (int i = 0; i < m_LightLoopCount; i++)
			{
				Vector3 position = m_ParentGroup.m_Lights[i].transform.position;
				if (m_bDoFunArc)
				{
					position.x = m_EffectDatas[i].m_Dir.magnitude * Mathf.Cos(m_EffectDatas[i].m_InTimer) + m_CentrePoint.x;
					position.y = m_EffectDatas[i].m_Dir.magnitude * Mathf.Sin(m_EffectDatas[i].m_InTimer) + m_CentrePoint.y;
					m_EffectDatas[i].m_InTimer += m_Speed * deltaTime;
					if (m_EffectDatas[i].m_InTimer >= (float)Math.PI * 2f)
					{
						m_EffectDatas[i].m_InTimer -= (float)Math.PI * 2f;
						if (i == 0)
						{
							flag = true;
						}
					}
				}
				else if (m_EffectDatas[i].m_bInWard)
				{
					position += m_Speed * deltaTime * m_EffectDatas[i].m_Dir;
					m_EffectDatas[i].m_InTimer += deltaTime;
					flag = false;
				}
				else if (m_EffectDatas[i].m_bOutWard)
				{
					position -= m_Speed * deltaTime * m_EffectDatas[i].m_Dir;
					flag = false;
					m_EffectDatas[i].m_InTimer -= deltaTime;
					if (m_EffectDatas[i].m_InTimer <= 0f)
					{
						m_EffectDatas[i].m_bOutWard = false;
					}
				}
				m_EffectDatas[i].m_IntensityMul += m_EffectDatas[i].m_IntensityDir * 2f * m_IntensitySpeed * deltaTime;
				if (m_EffectDatas[i].m_IntensityMul > m_MaxIntensityMul)
				{
					m_EffectDatas[i].m_IntensityMul = m_MaxIntensityMul;
					m_EffectDatas[i].m_IntensityDir *= -1f;
				}
				else if (m_EffectDatas[i].m_IntensityMul < m_MinIntensityMul)
				{
					m_EffectDatas[i].m_IntensityMul = m_MinIntensityMul;
					m_EffectDatas[i].m_IntensityDir *= -1f;
				}
				if (position.x > m_MaxShowTimeArea.x)
				{
					position.x = m_MaxShowTimeArea.x;
				}
				if (position.y > m_MaxShowTimeArea.y)
				{
					position.y = m_MaxShowTimeArea.y;
				}
				if (position.x < m_MinShowTimeArea.x)
				{
					position.x = m_MinShowTimeArea.x;
				}
				if (position.y < m_MinShowTimeArea.y)
				{
					position.y = m_MinShowTimeArea.y;
				}
				m_ParentGroup.m_Lights[i].transform.position = position;
				m_ParentGroup.m_Lights[i].SetIntensityMultiplier(m_EffectDatas[i].m_IntensityMul);
				if ((position - m_EffectDatas[i].m_TargetPoint).magnitude < m_InnerRadius * m_InnerRadius && m_EffectDatas[i].m_bInWard)
				{
					m_EffectDatas[i].m_bInWard = false;
				}
			}
			if (flag)
			{
				m_StartTimer = m_HoldDelay;
				m_bInHold = true;
			}
			return;
		}
		m_bInHold = false;
		if (!m_bJustGoneOut)
		{
			for (int i = 0; i < m_ParentGroup.m_Lights.Count; i++)
			{
				m_EffectDatas[i].m_bOutWard = true;
			}
			m_bJustGoneOut = true;
			return;
		}
		m_bJustGoneOut = false;
		if (!m_bDoFunArcNext)
		{
			for (int i = 0; i < m_LightLoopCount; i++)
			{
				m_EffectDatas[i].m_bInWard = true;
				if (m_TargetWarden != null)
				{
					m_EffectDatas[i].m_Dir = m_CentrePoint + m_CentreOffset - m_ParentGroup.m_Lights[i].transform.position;
					m_EffectDatas[i].m_Dir.z = 0f;
					m_EffectDatas[i].m_Dir.Normalize();
					m_EffectDatas[i].m_TargetPoint = m_CentrePoint + m_CentreOffset;
					m_EffectDatas[i].m_TargetPoint.z = m_EffectDatas[i].m_StartingPoint.z;
				}
				m_EffectDatas[i].m_InTimer = 0f;
			}
			m_bDoFunArcNext = true;
			m_bDoFunArc = false;
		}
		else
		{
			m_bDoFunArcNext = false;
			m_bDoFunArc = true;
			for (int i = 0; i < m_LightLoopCount; i++)
			{
				m_EffectDatas[i].m_InTimer = (float)i * (float)Math.PI / 2f;
				m_EffectDatas[i].m_Dir = m_ParentGroup.m_Lights[i].transform.position - m_CentrePoint;
			}
		}
	}

	public override string Serialize()
	{
		SerializableDataA serializableDataA = new SerializableDataA();
		serializableDataA.m_bLastLightIsFollowSpot = m_bLastLightIsFollowSpot;
		serializableDataA.m_StartDelay = m_StartDelay;
		serializableDataA.m_Speed = m_Speed;
		serializableDataA.m_HoldDelay = m_HoldDelay;
		serializableDataA.m_InnerRadius = m_InnerRadius;
		serializableDataA.m_CentreOffset = m_CentreOffset;
		serializableDataA.m_MaxIntensityMul = m_MaxIntensityMul;
		serializableDataA.m_MinIntensityMul = m_MinIntensityMul;
		serializableDataA.m_FollowSpotIntensityMul = m_FollowSpotIntensityMul;
		return JsonUtility.ToJson(serializableDataA);
	}

	public override bool Deserialize(string serialized)
	{
		SerializableDataA serializableDataA = JsonUtility.FromJson<SerializableDataA>(serialized);
		if (serializableDataA == null)
		{
			return false;
		}
		m_bLastLightIsFollowSpot = serializableDataA.m_bLastLightIsFollowSpot;
		m_StartDelay = serializableDataA.m_StartDelay;
		m_Speed = serializableDataA.m_Speed;
		m_HoldDelay = serializableDataA.m_HoldDelay;
		m_InnerRadius = serializableDataA.m_InnerRadius;
		m_IntensitySpeed = serializableDataA.m_IntensitySpeed;
		m_CentreOffset = serializableDataA.m_CentreOffset;
		m_MaxIntensityMul = serializableDataA.m_MaxIntensityMul;
		m_MinIntensityMul = serializableDataA.m_MinIntensityMul;
		m_FollowSpotIntensityMul = serializableDataA.m_FollowSpotIntensityMul;
		return true;
	}
}

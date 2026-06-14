using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateChanger : MonoBehaviour
{
	[Serializable]
	public struct AnimatorState
	{
		public string VariableName;

		public bool Enabled;

		public float TimeInThisState;

		public List<ParticleSystem> m_ParticlesForState;
	}

	public Animator m_AnimatorToControl;

	public List<AnimatorState> m_StatesList = new List<AnimatorState>();

	private float m_ElapsedTime;

	private int m_CurrentState;

	private void Start()
	{
		if (m_StatesList.Count > 0)
		{
			if (m_AnimatorToControl != null)
			{
				m_AnimatorToControl.SetBool(m_StatesList[m_CurrentState].VariableName, m_StatesList[m_CurrentState].Enabled);
			}
			StartCurrentParticles();
		}
	}

	private void Update()
	{
		if (m_StatesList[m_CurrentState].TimeInThisState <= -1f)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		m_ElapsedTime += UpdateManager.deltaTime;
		if (m_ElapsedTime >= m_StatesList[m_CurrentState].TimeInThisState)
		{
			m_ElapsedTime = 0f;
			GotoNextState();
		}
	}

	public void GotoNextState()
	{
		StopCurrentParticles();
		m_CurrentState++;
		if (m_CurrentState >= m_StatesList.Count)
		{
			m_CurrentState = 0;
		}
		if (m_AnimatorToControl != null)
		{
			m_AnimatorToControl.SetBool(m_StatesList[m_CurrentState].VariableName, m_StatesList[m_CurrentState].Enabled);
		}
		StartCurrentParticles();
	}

	public void GotoPreviousState()
	{
		StopCurrentParticles();
		m_CurrentState--;
		if (m_CurrentState < 0)
		{
			m_CurrentState = m_StatesList.Count - 1;
		}
		if (m_AnimatorToControl != null)
		{
			m_AnimatorToControl.SetBool(m_StatesList[m_CurrentState].VariableName, m_StatesList[m_CurrentState].Enabled);
		}
		StartCurrentParticles();
	}

	public void GotoState(int state)
	{
		StopCurrentParticles();
		m_CurrentState = Mathf.Clamp(state, 0, m_StatesList.Count - 1);
		if (m_AnimatorToControl != null)
		{
			m_AnimatorToControl.SetBool(m_StatesList[m_CurrentState].VariableName, m_StatesList[m_CurrentState].Enabled);
		}
		StartCurrentParticles();
	}

	private void StopCurrentParticles()
	{
		if (m_StatesList[m_CurrentState].m_ParticlesForState != null && m_StatesList[m_CurrentState].m_ParticlesForState.Count > 0)
		{
			for (int i = 0; i < m_StatesList[m_CurrentState].m_ParticlesForState.Count; i++)
			{
				m_StatesList[m_CurrentState].m_ParticlesForState[i].Stop();
			}
		}
	}

	private void StartCurrentParticles()
	{
		if (m_StatesList[m_CurrentState].m_ParticlesForState != null && m_StatesList[m_CurrentState].m_ParticlesForState.Count > 0)
		{
			for (int i = 0; i < m_StatesList[m_CurrentState].m_ParticlesForState.Count; i++)
			{
				m_StatesList[m_CurrentState].m_ParticlesForState[i].Play();
			}
		}
	}
}

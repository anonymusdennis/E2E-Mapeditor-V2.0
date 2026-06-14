using System;
using UnityEngine;

public class MultiplayerPuzzle_Shutter : MultiplayerPuzzle_Base
{
	public enum ShutterRotation
	{
		Horizontal,
		Vertical
	}

	private const float VISUAL_LERP_FACTOR = 3f;

	private const float MIN_LERP_THRESHOLD = 0.01f;

	[Header("Shutter Puzzle")]
	public ShutterRotation m_ShutterDirection;

	[Space]
	public Animator m_ShutterToAnimate;

	public MultiplayerPuzzle_Interaction m_HoldOpenInteraction;

	public MultiplayerPuzzle_Interaction m_AccessInteraction;

	private float m_RequestedGotoPercentage;

	private float m_CurrentPercentage;

	private bool m_bContinuousUpdate;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_CurrentPercentage = 0f;
		return base.StartInit();
	}

	protected override void UpdatePuzzle(float deltaTime)
	{
		base.UpdatePuzzle(deltaTime);
		if (m_bContinuousUpdate && m_ShutterToAnimate != null)
		{
			float num = Mathf.Abs(m_RequestedGotoPercentage - m_CurrentPercentage);
			if (num <= 0.01f)
			{
				m_CurrentPercentage = m_RequestedGotoPercentage;
				m_bContinuousUpdate = false;
			}
			else
			{
				m_CurrentPercentage = Mathf.Lerp(m_CurrentPercentage, m_RequestedGotoPercentage, UpdateManager.deltaTime * 3f);
			}
			m_ShutterToAnimate.speed = 0f;
			m_ShutterToAnimate.Play("Open", -1, m_CurrentPercentage);
		}
	}

	protected override void OnPuzzleStateChanged(bool solved)
	{
		m_RequestedGotoPercentage = ((!solved) ? 0f : 1f);
		m_bContinuousUpdate = true;
		if (m_AccessInteraction != null)
		{
			m_AccessInteraction.SetInteractionActive(solved, notifyAll: false);
		}
		if (!(m_HoldOpenInteraction != null))
		{
			return;
		}
		bool flag = false;
		if (solved)
		{
			MultiplayerPuzzle_Interaction[] interactions = m_Solutions[m_ActiveSolutionIndex].interactions;
			if (interactions != null)
			{
				flag = Array.IndexOf(interactions, m_HoldOpenInteraction) >= 0;
			}
		}
		bool active = !solved || flag;
		m_HoldOpenInteraction.SetInteractionActive(active, notifyAll: false);
	}
}

using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class LookAround : ActionTask<AICharacter>
{
	public float m_fSpinMinTime = 0.8f;

	public float m_fSpinMaxTime = 1f;

	private float m_fSpinTimer;

	private List<Directionx4> m_DirectionsToLook = new List<Directionx4>(4);

	protected override void OnExecute()
	{
		m_DirectionsToLook.Clear();
		Directionx4 x4FacingDirection = base.agent.m_Character.m_x4FacingDirection;
		for (int i = 0; i < 4; i++)
		{
			Directionx4 directionx = (Directionx4)Direction.FourDirections[i];
			if (directionx != x4FacingDirection)
			{
				m_DirectionsToLook.Add(directionx);
			}
		}
		m_DirectionsToLook.Shuffle();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (m_DirectionsToLook.Count == 0 && m_fSpinTimer <= 0f)
		{
			EndAction(true);
			return;
		}
		m_fSpinTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_fSpinTimer <= 0f && m_DirectionsToLook.Count > 0)
		{
			m_fSpinTimer = Random.Range(m_fSpinMinTime, m_fSpinMaxTime);
			base.agent.m_Character.SetFaceDirection(m_DirectionsToLook[0]);
			m_DirectionsToLook.RemoveAt(0);
		}
	}
}

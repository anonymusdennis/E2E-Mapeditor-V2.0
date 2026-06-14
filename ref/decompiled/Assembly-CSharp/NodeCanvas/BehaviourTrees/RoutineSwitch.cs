using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("IndexSwitcher", false)]
[Category("Team17")]
[Description("Executes ONE child based on the current routine and return it's status. If 'case' change while a child is running, that child will be interrupted before the new child is executed")]
public class RoutineSwitch : BTComposite
{
	public Routines m_eRoutineEnum = Routines.UNASSIGNED;

	public float m_fRoutineChangeMaxDelay = 5f;

	public float m_fRoutineChangeMinDelay = 0.5f;

	private int current = -1;

	private int runningIndex;

	private int m_iNextRoutine;

	private float m_fNextRoutineTimer;

	private Character m_agentCharacter;

	public override string name => $"<color=#b3ff7f>{base.name.ToUpper()}</color>";

	public override void OnGraphStarted()
	{
		base.OnGraphStarted();
		RoutineManager.GetInstance().OnRoutineChanged += RoutineChanged;
	}

	private void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forced)
	{
		if (newRoutine != null)
		{
			m_iNextRoutine = (int)newRoutine.m_BaseRoutineType;
		}
		if (newRoutine != null && (newRoutine.m_BaseRoutineType == Routines.Lockdown || newRoutine.m_BaseRoutineType == Routines.LightsOut))
		{
			m_fNextRoutineTimer = Random.Range(0f, m_fRoutineChangeMinDelay);
		}
		else
		{
			m_fNextRoutineTimer = Random.Range(0f, m_fRoutineChangeMaxDelay);
		}
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (current == -1)
		{
			current = (int)RoutineManager.GetInstance().GetCurrentRoutineBaseType();
		}
		if (base.outConnections.Count == 0)
		{
			return Status.Failure;
		}
		if (m_fNextRoutineTimer > 0f)
		{
			m_fNextRoutineTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
			if (m_fNextRoutineTimer <= 0f)
			{
				current = m_iNextRoutine;
				if (m_agentCharacter == null)
				{
					m_agentCharacter = agent.GetComponent<Character>();
				}
				if (m_agentCharacter != null)
				{
					m_agentCharacter.RequestStopInteraction();
				}
			}
		}
		if (runningIndex != current)
		{
			base.outConnections[runningIndex].Reset();
		}
		if (current < 0 || current >= base.outConnections.Count)
		{
			return Status.Failure;
		}
		base.status = base.outConnections[current].Execute(agent, blackboard);
		if (base.status == Status.Running)
		{
			runningIndex = current;
		}
		return base.status;
	}
}

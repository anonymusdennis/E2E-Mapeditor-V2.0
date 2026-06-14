using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;

public class AIBehaviourRecoveryManager : T17MonoBehaviour, IControlledUpdate
{
	private static AIBehaviourRecoveryManager m_Instance;

	private FastList<BehaviourTreeOwner> m_TrackedAgents = new FastList<BehaviourTreeOwner>();

	public static AIBehaviourRecoveryManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance == null)
		{
			m_Instance = this;
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.SlowPeriodic);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.SlowPeriodic);
		}
		m_TrackedAgents.Clear();
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void RecoverAgent(BehaviourTreeOwner owner)
	{
		if (!m_TrackedAgents.Contains(owner))
		{
			owner.PauseBehaviour();
			m_TrackedAgents.Add(owner);
		}
	}

	public void ControlledUpdate()
	{
		if (m_TrackedAgents.Count == 0)
		{
			return;
		}
		BehaviourTreeOwner behaviourTreeOwner = m_TrackedAgents[0];
		m_TrackedAgents.RemoveAt(0);
		if (!(behaviourTreeOwner != null))
		{
			return;
		}
		if (behaviourTreeOwner.graph != null && behaviourTreeOwner.graph.primeNode != null)
		{
			behaviourTreeOwner.graph.primeNode.ForceReset();
		}
		Character component = behaviourTreeOwner.GetComponent<Character>();
		if (component != null)
		{
			component.ForceStopInteraction();
			if (component.m_CharacterAnimator != null)
			{
				component.PrepareAICharacter();
			}
		}
		behaviourTreeOwner.StartBehaviour();
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}
}

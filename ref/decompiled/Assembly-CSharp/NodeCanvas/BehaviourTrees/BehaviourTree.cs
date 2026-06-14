using System;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[GraphInfo(packageName = "NodeCanvas", docsURL = "http://nodecanvas.paradoxnotion.com/documentation/", resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/", forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/")]
public class BehaviourTree : Graph, IControlledUpdate
{
	[Serializable]
	private struct DerivedSerializationData
	{
		public bool repeat;

		public float updateInterval;
	}

	[SerializeField]
	public bool repeat = true;

	[SerializeField]
	public float updateInterval;

	public bool useUpdateManager = true;

	public UpdateCategory m_UpdateCategory = UpdateCategory.TimeSlicedNodeCanvas;

	private float intervalCounter;

	private Status _rootStatus = Status.Resting;

	private AICharacter m_AICharacter;

	public static float CurrentTimeSlicedDeltaTime => UpdateManager.deltaTime;

	public Status rootStatus
	{
		get
		{
			return _rootStatus;
		}
		private set
		{
			if (_rootStatus != value)
			{
				_rootStatus = value;
				if (this.onRootStatusChanged != null)
				{
					this.onRootStatusChanged(this, value);
				}
			}
		}
	}

	public override Type baseNodeType => typeof(BTNode);

	public override bool requiresAgent => true;

	public override bool requiresPrimeNode => true;

	public override bool autoSort => true;

	public override bool useLocalBlackboard => false;

	public event Action<BehaviourTree, Status> onRootStatusChanged;

	public override object OnDerivedDataSerialization()
	{
		DerivedSerializationData derivedSerializationData = default(DerivedSerializationData);
		derivedSerializationData.repeat = repeat;
		derivedSerializationData.updateInterval = updateInterval;
		return derivedSerializationData;
	}

	public override void OnDerivedDataDeserialization(object data)
	{
		if (data is DerivedSerializationData)
		{
			repeat = ((DerivedSerializationData)data).repeat;
			updateInterval = ((DerivedSerializationData)data).updateInterval;
		}
	}

	protected override void OnGraphStarted()
	{
		intervalCounter = updateInterval;
		rootStatus = base.primeNode.status;
		if (!useUpdateManager)
		{
			return;
		}
		if (UpdateManager.GetInstance() != null)
		{
			GraphOwner graphOwner = base.agent as GraphOwner;
			if (graphOwner != null)
			{
				m_UpdateCategory = graphOwner.m_UpdateCategory;
			}
			UpdateManager.GetInstance().Register(this, m_UpdateCategory);
		}
		UnRegisterFromMonoManagerUpdate();
	}

	protected override void OnGraphStoped()
	{
		if (useUpdateManager && UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, m_UpdateCategory);
		}
	}

	public void ControlledUpdate()
	{
		if (base.isRunning && (!(GlobalStart.GetInstance() != null) || GlobalStart.GetInstance().IsWithinLevel()) && PhotonNetwork.isMasterClient && Tick(base.agent, base.blackboard) != Status.Running && !repeat)
		{
			Stop(rootStatus == Status.Success);
		}
	}

	protected override void OnGraphUpdate()
	{
		if (useUpdateManager || (GlobalStart.GetInstance() != null && !GlobalStart.GetInstance().IsWithinLevel()) || !PhotonNetwork.isMasterClient)
		{
			return;
		}
		if (intervalCounter >= updateInterval)
		{
			intervalCounter = 0f;
			if (Tick(base.agent, base.blackboard) != Status.Running && !repeat)
			{
				Stop(rootStatus == Status.Success);
			}
		}
		if (updateInterval > 0f)
		{
			intervalCounter += UpdateManager.deltaTime;
		}
	}

	public Status Tick(Component _agent, IBlackboard blackboard)
	{
		if (rootStatus != Status.Running)
		{
			base.primeNode.PreInitialise();
			base.primeNode.Reset();
		}
		rootStatus = base.primeNode.Execute(_agent, blackboard);
		if (m_AICharacter == null)
		{
			m_AICharacter = _agent.GetComponent<AICharacter>();
		}
		if (m_AICharacter != null)
		{
			m_AICharacter.OnPostBTTick();
		}
		return rootStatus;
	}

	public void ControlledFixedUpdate()
	{
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

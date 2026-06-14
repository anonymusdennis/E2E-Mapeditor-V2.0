using System;
using System.Collections;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Services;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
[SpoofAOT]
public abstract class Task
{
	[AttributeUsage(AttributeTargets.Class)]
	protected class AgentTypeAttribute : Attribute
	{
		public Type type;

		public AgentTypeAttribute(Type type)
		{
			this.type = type;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	protected class EventReceiverAttribute : Attribute
	{
		public string[] eventMessages;

		public EventReceiverAttribute(params string[] args)
		{
			eventMessages = args;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	protected class GetFromAgentAttribute : Attribute
	{
	}

	[Serializable]
	public class TaskAgent : BBParameter<UnityEngine.Object>
	{
		public new UnityEngine.Object value
		{
			get
			{
				if (base.useBlackboard)
				{
					UnityEngine.Object @object = base.value;
					if (@object == null)
					{
						return null;
					}
					if (@object is GameObject)
					{
						return (@object as GameObject).transform;
					}
					if (@object is Component)
					{
						return (Component)@object;
					}
					return null;
				}
				return _value as Component;
			}
			set
			{
				_value = value;
			}
		}

		protected override object objectValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = (UnityEngine.Object)value;
			}
		}
	}

	[SerializeField]
	private bool _isDisabled;

	[SerializeField]
	private TaskAgent overrideAgent;

	[NonSerialized]
	private IBlackboard _blackboard;

	[NonSerialized]
	private ITaskSystem _ownerSystem;

	[NonSerialized]
	private Component current;

	[NonSerialized]
	private bool _agentTypeInit;

	[NonSerialized]
	private Type _agentType;

	[NonSerialized]
	private string _taskName;

	[NonSerialized]
	private string _taskDescription;

	[NonSerialized]
	private string _obsoleteInfo;

	public ITaskSystem ownerSystem
	{
		get
		{
			return _ownerSystem;
		}
		private set
		{
			_ownerSystem = value;
		}
	}

	public Component ownerAgent => (ownerSystem == null) ? null : ownerSystem.agent;

	public IBlackboard ownerBlackboard => (ownerSystem == null) ? null : ownerSystem.blackboard;

	protected float ownerElapsedTime => (ownerSystem == null) ? 0f : ownerSystem.elapsedTime;

	public bool isActive
	{
		get
		{
			return !_isDisabled;
		}
		set
		{
			_isDisabled = !value;
		}
	}

	public string obsolete
	{
		get
		{
			if (_obsoleteInfo == null)
			{
				ObsoleteAttribute obsoleteAttribute = GetType().RTGetAttribute<ObsoleteAttribute>(inherited: true);
				_obsoleteInfo = ((obsoleteAttribute == null) ? string.Empty : obsoleteAttribute.Message);
			}
			return _obsoleteInfo;
		}
	}

	public string name
	{
		get
		{
			if (_taskName == null)
			{
				NameAttribute nameAttribute = GetType().RTGetAttribute<NameAttribute>(inherited: false);
				_taskName = ((nameAttribute == null) ? GetType().FriendlyName().SplitCamelCase() : nameAttribute.name);
			}
			return _taskName;
		}
	}

	public string description
	{
		get
		{
			if (_taskDescription == null)
			{
				DescriptionAttribute descriptionAttribute = GetType().RTGetAttribute<DescriptionAttribute>(inherited: true);
				_taskDescription = ((descriptionAttribute == null) ? string.Empty : descriptionAttribute.description);
			}
			return _taskDescription;
		}
	}

	public virtual Type agentType
	{
		get
		{
			if (!_agentTypeInit)
			{
				AgentTypeAttribute agentTypeAttribute = GetType().RTGetAttribute<AgentTypeAttribute>(inherited: true);
				if (agentTypeAttribute != null)
				{
					if (!agentTypeAttribute.type.RTIsInterface())
					{
						Debug.LogWarning($"Using [AgentType] attribute for non-interface types is deprecated. Please use the generic version of ActionTask or ConditionTask to define the target type of the agent! (Task: '{GetType().Name}')");
					}
					_agentType = agentTypeAttribute.type;
				}
				_agentTypeInit = true;
			}
			return _agentType;
		}
	}

	public string summaryInfo
	{
		get
		{
			if (this is ActionTask)
			{
				return ((!agentIsOverride) ? string.Empty : "* ") + info;
			}
			if (this is ConditionTask)
			{
				return ((!agentIsOverride) ? string.Empty : "* ") + ((!(this as ConditionTask).invert) ? "If " : "If <b>!</b> ") + info;
			}
			return info;
		}
	}

	protected virtual string info => name;

	public string agentInfo => (overrideAgent == null) ? "<b>owner</b>" : overrideAgent.ToString();

	public bool agentIsOverride
	{
		get
		{
			return overrideAgent != null;
		}
		private set
		{
			if (!value && overrideAgent != null)
			{
				overrideAgent = null;
			}
			if (value && overrideAgent == null)
			{
				overrideAgent = new TaskAgent();
				overrideAgent.bb = blackboard;
			}
		}
	}

	public string overrideAgentParameterName => (overrideAgent == null) ? null : overrideAgent.name;

	protected Component agent
	{
		get
		{
			if (current != null)
			{
				return current;
			}
			Component component = ((!agentIsOverride) ? ownerAgent : ((Component)overrideAgent.value));
			if (component != null && agentType != null && !agentType.RTIsAssignableFrom(component.GetType()) && (agentType.RTIsSubclassOf(typeof(Component)) || agentType.RTIsInterface()))
			{
				component = component.GetComponent(agentType);
			}
			return component;
		}
	}

	protected IBlackboard blackboard
	{
		get
		{
			return _blackboard;
		}
		private set
		{
			if (_blackboard != value)
			{
				_blackboard = value;
				BBParameter.SetBBFields(this, value);
				if (overrideAgent != null)
				{
					overrideAgent.bb = value;
				}
			}
		}
	}

	public string firstWarningMessage { get; private set; }

	public Task()
	{
	}

	public static T Create<T>(ITaskSystem newOwnerSystem) where T : Task
	{
		return (T)Create(typeof(T), newOwnerSystem);
	}

	public static Task Create(Type type, ITaskSystem newOwnerSystem)
	{
		Task task = (Task)Activator.CreateInstance(type);
		task.SetOwnerSystem(newOwnerSystem);
		BBParameter.SetBBFields(task, newOwnerSystem.blackboard);
		task.OnValidate(newOwnerSystem);
		return task;
	}

	public virtual Task Duplicate(ITaskSystem newOwnerSystem)
	{
		Task task = JSONSerializer.Deserialize<Task>(JSONSerializer.Serialize(typeof(Task), this));
		task.SetOwnerSystem(newOwnerSystem);
		BBParameter.SetBBFields(task, newOwnerSystem.blackboard);
		task.OnValidate(newOwnerSystem);
		return task;
	}

	public virtual void OnValidate(ITaskSystem ownerSystem)
	{
	}

	public void SetOwnerSystem(ITaskSystem newOwnerSystem)
	{
		if (newOwnerSystem == null)
		{
			Debug.LogError("ITaskSystem set in task is null!!");
		}
		else
		{
			ownerSystem = newOwnerSystem;
		}
	}

	protected Coroutine StartCoroutine(IEnumerator routine)
	{
		return MonoManager.current.StartCoroutine(routine);
	}

	protected void StopCoroutine(Coroutine routine)
	{
		MonoManager.current.StopCoroutine(routine);
	}

	protected void SendEvent(string eventName)
	{
		SendEvent(new EventData(eventName));
	}

	protected void SendEvent<T>(string eventName, T value)
	{
		SendEvent(new EventData<T>(eventName, value));
	}

	protected void SendEvent(EventData eventData)
	{
		if (ownerSystem != null)
		{
			ownerSystem.SendEvent(eventData);
		}
	}

	protected virtual string OnInit()
	{
		return null;
	}

	protected bool Set(Component newAgent, IBlackboard newBB)
	{
		blackboard = newBB;
		if (agentIsOverride)
		{
			newAgent = (Component)overrideAgent.value;
		}
		if (current != null && newAgent != null && current.gameObject == newAgent.gameObject)
		{
			return isActive = true;
		}
		return isActive = Initialize(newAgent, agentType);
	}

	private bool Initialize(Component newAgent, Type newType)
	{
		UnRegisterAllEvents();
		if (newAgent != null && agentType != null && !agentType.RTIsAssignableFrom(newAgent.GetType()) && (agentType.RTIsSubclassOf(typeof(Component)) || agentType.RTIsInterface()))
		{
			newAgent = newAgent.GetComponent(agentType);
		}
		current = newAgent;
		if (newAgent == null && agentType != null)
		{
			return Error(string.Concat("Failed to resolve Agent to requested type '", agentType, "', or new Agent is NULL. Does the Agent has the requested Component?"));
		}
		EventReceiverAttribute eventReceiverAttribute = GetType().RTGetAttribute<EventReceiverAttribute>(inherited: true);
		if (eventReceiverAttribute != null)
		{
			RegisterEvents(eventReceiverAttribute.eventMessages);
		}
		if (!InitializeAttributes(newAgent))
		{
			return false;
		}
		string text = OnInit();
		if (text != null)
		{
			return Error(text);
		}
		return true;
	}

	private bool InitializeAttributes(Component newAgent)
	{
		FieldInfo[] array = GetType().RTGetFields();
		foreach (FieldInfo fieldInfo in array)
		{
			if (!typeof(Component).RTIsAssignableFrom(fieldInfo.FieldType))
			{
				continue;
			}
			GetFromAgentAttribute getFromAgentAttribute = fieldInfo.RTGetAttribute<GetFromAgentAttribute>(inherited: true);
			if (getFromAgentAttribute != null)
			{
				Component component = newAgent.GetComponent(fieldInfo.FieldType);
				fieldInfo.SetValue(this, component);
				if (object.ReferenceEquals(component, null))
				{
					return Error($"GetFromAgent Attribute failed to get the required Component of type '{fieldInfo.FieldType.Name}' from '{agent.gameObject.name}'. Does it exist?");
				}
			}
		}
		return true;
	}

	protected bool Error(string error)
	{
		Debug.LogError($"<b>({((ownerSystem == null) ? string.Empty : ownerSystem.contextObject.name)}) '{name}' Task Error</b>: '{error}' (Task Disabled)", (ownerSystem == null) ? null : ownerSystem.contextObject);
		return false;
	}

	public void RegisterEvent(string eventName)
	{
		RegisterEvents(eventName);
	}

	public void RegisterEvents(params string[] eventNames)
	{
		if (!(agent == null))
		{
			MessageRouter messageRouter = agent.GetComponent<MessageRouter>();
			if (messageRouter == null)
			{
				messageRouter = agent.gameObject.AddComponent<MessageRouter>();
			}
			messageRouter.Register(this, eventNames);
		}
	}

	public void UnRegisterEvent(string eventName)
	{
		UnRegisterEvents(eventName);
	}

	public void UnRegisterEvents(params string[] eventNames)
	{
		if (!(agent == null))
		{
			MessageRouter component = agent.GetComponent<MessageRouter>();
			if (component != null)
			{
				component.UnRegister(this, eventNames);
			}
		}
	}

	public void UnRegisterAllEvents()
	{
		if (!(agent == null))
		{
			MessageRouter component = agent.GetComponent<MessageRouter>();
			if (component != null)
			{
				component.UnRegister(this);
			}
		}
	}

	public string GetWarning()
	{
		firstWarningMessage = Internal_GetWarning();
		return firstWarningMessage;
	}

	private string Internal_GetWarning()
	{
		if (obsolete != string.Empty)
		{
			return $"Task is obsolete: '{obsolete}'.";
		}
		if (agent == null && agentType != null)
		{
			return $"'{agentType.Name}' target is currently null.";
		}
		FieldInfo[] array = GetType().RTGetFields();
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.RTGetAttribute<RequiredFieldAttribute>(inherited: true) == null)
			{
				continue;
			}
			object value = fieldInfo.GetValue(this);
			if (value == null || value.Equals(null))
			{
				return $"Required field '{fieldInfo.Name.SplitCamelCase()}' is currently null.";
			}
			if (fieldInfo.FieldType == typeof(string) && string.IsNullOrEmpty((string)value))
			{
				return $"Required string field '{fieldInfo.Name.SplitCamelCase()}' is currently null or empty.";
			}
			if (typeof(BBParameter).RTIsAssignableFrom(fieldInfo.FieldType))
			{
				if (!(value is BBParameter bBParameter))
				{
					return $"BBParameter '{fieldInfo.Name.SplitCamelCase()}' is null.";
				}
				if (bBParameter.isNull)
				{
					return $"Required parameter '{fieldInfo.Name.SplitCamelCase()}' is currently null.";
				}
			}
		}
		return null;
	}

	public sealed override string ToString()
	{
		return $"{agentInfo} {summaryInfo}";
	}

	public virtual void OnDrawGizmos()
	{
	}

	public virtual void OnDrawGizmosSelected()
	{
	}
}

using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas;

[AddComponentMenu("NodeCanvas/Action List")]
public class ActionListPlayer : MonoBehaviour, ITaskSystem, ISerializationCallbackReceiver
{
	[NonSerialized]
	private ActionList _actionList;

	[SerializeField]
	private Blackboard _blackboard;

	[SerializeField]
	private List<UnityEngine.Object> _objectReferences;

	[SerializeField]
	private string _serializedList;

	public ActionList actionList
	{
		get
		{
			return _actionList;
		}
		set
		{
			_actionList = value;
			SendTaskOwnerDefaults();
		}
	}

	public Component agent => this;

	public IBlackboard blackboard
	{
		get
		{
			return _blackboard;
		}
		set
		{
			if (!object.ReferenceEquals(_blackboard, value))
			{
				_blackboard = (Blackboard)value;
				SendTaskOwnerDefaults();
			}
		}
	}

	public float elapsedTime => actionList.elapsedTime;

	public UnityEngine.Object contextObject => this;

	public void OnBeforeSerialize()
	{
		_objectReferences = new List<UnityEngine.Object>();
		_serializedList = JSONSerializer.Serialize(typeof(ActionList), actionList, pretyJson: false, _objectReferences);
	}

	public void OnAfterDeserialize()
	{
		actionList = JSONSerializer.Deserialize<ActionList>(_serializedList, _objectReferences);
		if (actionList == null)
		{
			actionList = (ActionList)Task.Create(typeof(ActionList), this);
		}
	}

	public static ActionListPlayer Create()
	{
		return new GameObject("ActionList").AddComponent<ActionListPlayer>();
	}

	public void SendTaskOwnerDefaults()
	{
		actionList.SetOwnerSystem(this);
		foreach (ActionTask action in actionList.actions)
		{
			action.SetOwnerSystem(this);
		}
	}

	void ITaskSystem.SendEvent(EventData eventData)
	{
		Debug.LogWarning("Sending events to action lists has no effect");
	}

	[ContextMenu("Play")]
	public void Play()
	{
		if (Application.isPlaying)
		{
			Play(this, blackboard, null);
		}
	}

	public void Play(Action<bool> OnFinish)
	{
		Play(this, blackboard, OnFinish);
	}

	public void Play(Component agent, IBlackboard blackboard, Action<bool> OnFinish)
	{
		actionList.ExecuteAction(agent, blackboard, OnFinish);
	}
}

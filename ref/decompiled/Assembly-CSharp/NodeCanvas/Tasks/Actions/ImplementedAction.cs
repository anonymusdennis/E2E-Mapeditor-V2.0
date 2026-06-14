using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Script Control/Standalone Only")]
[Description("Calls a function that has signature of 'public Status NAME()' or 'public Status NAME(T)'. You should return Status.Success, Failure or Running within that function.")]
public class ImplementedAction : ActionTask, ISubParametersContainer
{
	[SerializeField]
	protected ReflectedFunctionWrapper functionWrapper;

	private Status actionStatus = Status.Resting;

	private MethodInfo targetMethod => (functionWrapper == null) ? null : functionWrapper.GetMethod();

	public override Type agentType => (targetMethod == null) ? typeof(Transform) : targetMethod.RTReflectedType();

	protected override string info
	{
		get
		{
			if (functionWrapper == null)
			{
				return "No Action Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {functionWrapper.GetMethodString()} *</color>";
			}
			return $"[ {base.agentInfo}.{targetMethod.Name}({((functionWrapper.GetVariables().Length != 2) ? string.Empty : functionWrapper.GetVariables()[1].ToString())}) ]";
		}
	}

	BBParameter[] ISubParametersContainer.GetIncludeParseParameters()
	{
		return (functionWrapper == null) ? null : functionWrapper.GetVariables();
	}

	public override void OnValidate(ITaskSystem ownerSystem)
	{
		if (functionWrapper != null && functionWrapper.HasChanged())
		{
			SetMethod(functionWrapper.GetMethod());
		}
		if (functionWrapper != null && targetMethod == null)
		{
			Error($"Missing Method '{functionWrapper.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (functionWrapper == null)
		{
			return "No Method selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Method '{functionWrapper.GetMethodString()}'";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "ImplementedAction Error";
		}
	}

	protected override void OnExecute()
	{
		Forward();
	}

	protected override void OnUpdate()
	{
		Forward();
	}

	private void Forward()
	{
		if (functionWrapper == null)
		{
			EndAction(false);
			return;
		}
		actionStatus = (Status)functionWrapper.Call();
		if (actionStatus == Status.Success)
		{
			EndAction(true);
		}
		else if (actionStatus == Status.Failure)
		{
			EndAction(false);
		}
	}

	protected override void OnStop()
	{
		actionStatus = Status.Resting;
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			functionWrapper = ReflectedFunctionWrapper.Create(method, base.blackboard);
		}
	}
}

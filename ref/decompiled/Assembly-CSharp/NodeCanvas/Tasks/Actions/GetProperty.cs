using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Get a property of a script and save it to the blackboard")]
[Category("✫ Script Control/Standalone Only")]
public class GetProperty : ActionTask, ISubParametersContainer
{
	[SerializeField]
	protected ReflectedFunctionWrapper functionWrapper;

	private MethodInfo targetMethod => (functionWrapper == null) ? null : functionWrapper.GetMethod();

	public override Type agentType => (targetMethod == null) ? typeof(Transform) : targetMethod.RTReflectedType();

	protected override string info
	{
		get
		{
			if (functionWrapper == null)
			{
				return "No Property Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {functionWrapper.GetMethodString()} *</color>";
			}
			return $"{functionWrapper.GetVariables()[0]} = {base.agentInfo}.{targetMethod.Name}";
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
			Error($"Missing Property '{functionWrapper.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (functionWrapper == null)
		{
			return "No Property selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Property '{functionWrapper.GetMethodString()}'";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "GetProperty Error";
		}
	}

	protected override void OnExecute()
	{
		if (functionWrapper == null)
		{
			EndAction(false);
			return;
		}
		functionWrapper.Call();
		EndAction();
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			functionWrapper = ReflectedFunctionWrapper.Create(method, base.blackboard);
		}
	}
}

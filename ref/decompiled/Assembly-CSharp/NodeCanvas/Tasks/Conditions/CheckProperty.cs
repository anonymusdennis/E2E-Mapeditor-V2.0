using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("✫ Script Control/Standalone Only")]
[Description("Check a property on a script and return if it's equal or not to the check value")]
public class CheckProperty : ConditionTask, ISubParametersContainer
{
	[SerializeField]
	protected ReflectedFunctionWrapper functionWrapper;

	[SerializeField]
	protected BBParameter checkValue;

	[SerializeField]
	protected CompareMethod comparison;

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
			return $"{base.agentInfo}.{targetMethod.Name}{OperationTools.GetCompareString(comparison) + checkValue.ToString()}";
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
		if (targetMethod == null)
		{
			return "CheckProperty Error";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "CheckProperty Error";
		}
	}

	protected override bool OnCheck()
	{
		if (functionWrapper == null)
		{
			return true;
		}
		if (checkValue.varType == typeof(float))
		{
			return OperationTools.Compare((float)functionWrapper.Call(), (float)checkValue.value, comparison, 0.05f);
		}
		if (checkValue.varType == typeof(int))
		{
			return OperationTools.Compare((int)functionWrapper.Call(), (int)checkValue.value, comparison);
		}
		return object.Equals(functionWrapper.Call(), checkValue.value);
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			functionWrapper = ReflectedFunctionWrapper.Create(method, base.blackboard);
			checkValue = BBParameter.CreateInstance(method.ReturnType, base.blackboard);
			comparison = CompareMethod.EqualTo;
		}
	}
}

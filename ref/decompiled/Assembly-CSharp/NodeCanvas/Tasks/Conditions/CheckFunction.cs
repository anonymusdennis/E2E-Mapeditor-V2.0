using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Description("Call a function with none or up to 6 parameters on a component and return whether or not the return value is equal to the check value")]
[Category("✫ Script Control/Standalone Only")]
public class CheckFunction : ConditionTask, ISubParametersContainer
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
				return "No Method Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {functionWrapper.GetMethodString()} *</color>";
			}
			BBParameter[] variables = functionWrapper.GetVariables();
			string text = string.Empty;
			for (int i = 1; i < variables.Length; i++)
			{
				text = text + ((i == 1) ? string.Empty : ", ") + variables[i].ToString();
			}
			return $"{base.agentInfo}.{targetMethod.Name}({text}){OperationTools.GetCompareString(comparison) + checkValue}";
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
		if (targetMethod == null)
		{
			return "CheckFunction Error";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "CheckFunction Error";
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

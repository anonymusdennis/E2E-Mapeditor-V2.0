using System;
using System.Collections.Generic;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("✫ Script Control/Multiplatform")]
[Description("Call a function on a component and return whether or not the return value is equal to the check value")]
[Name("Check Function (mp)")]
public class CheckFunction_Multiplatform : ConditionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[SerializeField]
	protected List<BBObjectParameter> parameters = new List<BBObjectParameter>();

	[BlackboardOnly]
	[SerializeField]
	protected BBObjectParameter checkValue;

	[SerializeField]
	protected CompareMethod comparison;

	private object[] args;

	private MethodInfo targetMethod => (method == null) ? null : method.Get();

	public override Type agentType => (targetMethod == null) ? typeof(Transform) : targetMethod.RTReflectedType();

	protected override string info
	{
		get
		{
			if (method == null)
			{
				return "No Method Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {method.GetMethodString()} *</color>";
			}
			string text = string.Empty;
			for (int i = 0; i < parameters.Count; i++)
			{
				text = text + ((i == 0) ? string.Empty : ", ") + parameters[i].ToString();
			}
			return $"{base.agentInfo}.{targetMethod.Name}({text}){OperationTools.GetCompareString(comparison) + checkValue}";
		}
	}

	public override void OnValidate(ITaskSystem ownerSystem)
	{
		if (method != null && method.HasChanged())
		{
			SetMethod(method.Get());
		}
		if (method != null && method.Get() == null)
		{
			Error($"Missing Method '{method.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (targetMethod == null)
		{
			return "CheckFunction Error";
		}
		if (args == null)
		{
			args = new object[parameters.Count];
		}
		return null;
	}

	protected override bool OnCheck()
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			args[i] = parameters[i].value;
		}
		if (checkValue.varType == typeof(float))
		{
			return OperationTools.Compare((float)targetMethod.Invoke(base.agent, args), (float)checkValue.value, comparison, 0.05f);
		}
		if (checkValue.varType == typeof(int))
		{
			return OperationTools.Compare((int)targetMethod.Invoke(base.agent, args), (int)checkValue.value, comparison);
		}
		return object.Equals(targetMethod.Invoke(base.agent, args), checkValue.value);
	}

	private void SetMethod(MethodInfo method)
	{
		if (method == null)
		{
			return;
		}
		this.method = new SerializedMethodInfo(method);
		parameters.Clear();
		ParameterInfo[] array = method.GetParameters();
		foreach (ParameterInfo parameterInfo in array)
		{
			BBObjectParameter bBObjectParameter = new BBObjectParameter(parameterInfo.ParameterType);
			bBObjectParameter.bb = base.blackboard;
			BBObjectParameter bBObjectParameter2 = bBObjectParameter;
			if (parameterInfo.IsOptional)
			{
				bBObjectParameter2.value = parameterInfo.DefaultValue;
			}
			parameters.Add(bBObjectParameter2);
		}
		checkValue = new BBObjectParameter(method.ReturnType)
		{
			bb = base.blackboard
		};
		comparison = CompareMethod.EqualTo;
	}
}

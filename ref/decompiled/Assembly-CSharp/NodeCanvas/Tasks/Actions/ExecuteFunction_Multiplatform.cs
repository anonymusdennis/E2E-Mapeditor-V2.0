using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Execute a function on a script and save the return if any. If function is an IEnumerator it will execute as a coroutine.")]
[Category("✫ Script Control/Multiplatform")]
[Name("Execute Function (mp)")]
public class ExecuteFunction_Multiplatform : ActionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[SerializeField]
	protected List<BBObjectParameter> parameters = new List<BBObjectParameter>();

	[SerializeField]
	[BlackboardOnly]
	protected BBObjectParameter returnValue;

	private object[] args;

	private bool routineRunning;

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
			string text = ((targetMethod.ReturnType != typeof(void) && targetMethod.ReturnType != typeof(IEnumerator)) ? (returnValue.ToString() + " = ") : string.Empty);
			string text2 = string.Empty;
			for (int i = 0; i < parameters.Count; i++)
			{
				text2 = text2 + ((i == 0) ? string.Empty : ", ") + parameters[i].ToString();
			}
			return $"{text}{base.agentInfo}.{targetMethod.Name}({text2})";
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
		if (method == null)
		{
			return "No Method selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Method '{method.GetMethodString()}'";
		}
		if (args == null)
		{
			args = new object[parameters.Count];
		}
		return null;
	}

	protected override void OnExecute()
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			args[i] = parameters[i].value;
		}
		if (targetMethod.ReturnType == typeof(IEnumerator))
		{
			StartCoroutine(InternalCoroutine((IEnumerator)targetMethod.Invoke(base.agent, args)));
			return;
		}
		returnValue.value = targetMethod.Invoke(base.agent, args);
		EndAction();
	}

	protected override void OnStop()
	{
		routineRunning = false;
	}

	private IEnumerator InternalCoroutine(IEnumerator routine)
	{
		routineRunning = true;
		while (routineRunning && routine.MoveNext())
		{
			if (!routineRunning)
			{
				yield break;
			}
			yield return routine.Current;
		}
		if (routineRunning)
		{
			EndAction();
		}
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
		if (method.ReturnType != typeof(void) && targetMethod.ReturnType != typeof(IEnumerator))
		{
			returnValue = new BBObjectParameter(method.ReturnType)
			{
				bb = base.blackboard
			};
		}
		else
		{
			returnValue = null;
		}
	}
}

using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Description("Check a property on a script and return if it's equal or not to the check value")]
[Name("Check Property (mp)")]
[Category("✫ Script Control/Multiplatform")]
public class CheckProperty_Multiplatform : ConditionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[SerializeField]
	protected BBObjectParameter checkValue;

	[SerializeField]
	protected CompareMethod comparison;

	private MethodInfo targetMethod => (method == null) ? null : method.Get();

	public override Type agentType => (targetMethod == null) ? typeof(Transform) : targetMethod.RTReflectedType();

	protected override string info
	{
		get
		{
			if (method == null)
			{
				return "No Property Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {method.GetMethodString()} *</color>";
			}
			return $"{base.agentInfo}.{targetMethod.Name}{OperationTools.GetCompareString(comparison) + checkValue.ToString()}";
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
			Error($"Missing Property '{method.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (targetMethod == null)
		{
			return "CheckProperty Error";
		}
		return null;
	}

	protected override bool OnCheck()
	{
		if (checkValue.varType == typeof(float))
		{
			return OperationTools.Compare((float)targetMethod.Invoke(base.agent, null), (float)checkValue.value, comparison, 0.05f);
		}
		if (checkValue.varType == typeof(int))
		{
			return OperationTools.Compare((int)targetMethod.Invoke(base.agent, null), (int)checkValue.value, comparison);
		}
		return object.Equals(targetMethod.Invoke(base.agent, null), checkValue.value);
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			this.method = new SerializedMethodInfo(method);
			checkValue.SetType(method.ReturnType);
			comparison = CompareMethod.EqualTo;
		}
	}
}

using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a property on a script")]
[Category("✫ Script Control/Multiplatform")]
[Name("Set Property (mp)")]
public class SetProperty_Multiplatform : ActionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[SerializeField]
	protected BBObjectParameter parameter;

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
			return $"{base.agentInfo}.{targetMethod.Name} = {parameter.ToString()}";
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
		if (method == null)
		{
			return "No property selected";
		}
		if (targetMethod == null)
		{
			return $"Missing property '{method.GetMethodString()}'";
		}
		return null;
	}

	protected override void OnExecute()
	{
		targetMethod.Invoke(base.agent, new object[1] { parameter.value });
		EndAction();
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			this.method = new SerializedMethodInfo(method);
			parameter.SetType(method.GetParameters()[0].ParameterType);
		}
	}
}

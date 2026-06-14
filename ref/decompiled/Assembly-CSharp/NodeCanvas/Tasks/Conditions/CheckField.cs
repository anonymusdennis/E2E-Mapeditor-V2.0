using System;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Description("Check a field on a script and return if it's equal or not to a value")]
[Category("✫ Script Control/Common")]
public class CheckField : ConditionTask
{
	[SerializeField]
	protected BBParameter checkValue;

	[SerializeField]
	protected Type targetType;

	[SerializeField]
	protected string fieldName;

	[SerializeField]
	protected CompareMethod comparison;

	private FieldInfo field;

	public override Type agentType => (targetType == null) ? typeof(Transform) : targetType;

	protected override string info
	{
		get
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return "No Field Selected";
			}
			return $"{base.agentInfo}.{fieldName}{((checkValue.varType != typeof(bool)) ? (OperationTools.GetCompareString(comparison) + checkValue.ToString()) : string.Empty)}";
		}
	}

	protected override string OnInit()
	{
		field = agentType.RTGetField(fieldName);
		if (field == null)
		{
			return "Missing Field Info";
		}
		return null;
	}

	protected override bool OnCheck()
	{
		if (checkValue.varType == typeof(float))
		{
			return OperationTools.Compare((float)field.GetValue(base.agent), (float)checkValue.value, comparison, 0.05f);
		}
		if (checkValue.varType == typeof(int))
		{
			return OperationTools.Compare((int)field.GetValue(base.agent), (int)checkValue.value, comparison);
		}
		return object.Equals(field.GetValue(base.agent), checkValue.value);
	}
}

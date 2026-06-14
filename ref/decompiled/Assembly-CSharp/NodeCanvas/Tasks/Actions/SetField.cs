using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a variable on a script")]
[Category("✫ Script Control/Common")]
public class SetField : ActionTask
{
	[SerializeField]
	protected BBObjectParameter setValue;

	[SerializeField]
	protected Type targetType;

	[SerializeField]
	protected string fieldName;

	private FieldInfo field;

	public override Type agentType => targetType ?? typeof(Transform);

	protected override string info
	{
		get
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				return "No Field Selected";
			}
			return $"{base.agentInfo}.{fieldName} = {setValue}";
		}
	}

	protected override string OnInit()
	{
		field = agentType.RTGetField(fieldName);
		if (field == null)
		{
			return "Missing Field: " + fieldName;
		}
		return null;
	}

	protected override void OnExecute()
	{
		field.SetValue(base.agent, setValue.value);
		EndAction();
	}
}

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("You can either use a parameter name OR hashID. Leave the parameter name empty or none to use hashID instead.")]
[Name("Set Parameter Float")]
[Category("Animator")]
public class MecanimSetFloat : ActionTask<Animator>
{
	public BBParameter<string> parameter;

	public BBParameter<int> parameterHashID;

	public BBParameter<float> setTo;

	[SliderField(0, 1)]
	public float transitTime = 0.25f;

	private float currentValue;

	protected override string info => $"Mec.SetFloat {((!string.IsNullOrEmpty(parameter.value)) ? parameter.ToString() : parameterHashID.ToString())} to {setTo}";

	protected override void OnExecute()
	{
		if (transitTime <= 0f)
		{
			Set(setTo.value);
			EndAction();
		}
		else
		{
			currentValue = Get();
		}
	}

	protected override void OnUpdate()
	{
		Set(Mathf.Lerp(currentValue, setTo.value, base.elapsedTime / transitTime));
		if (base.elapsedTime >= transitTime)
		{
			EndAction(true);
		}
	}

	private float Get()
	{
		if (!string.IsNullOrEmpty(parameter.value))
		{
			return base.agent.GetFloat(parameter.value);
		}
		return base.agent.GetFloat(parameterHashID.value);
	}

	private void Set(float newValue)
	{
		if (!string.IsNullOrEmpty(parameter.value))
		{
			base.agent.SetFloat(parameter.value, newValue);
		}
		else
		{
			base.agent.SetFloat(parameterHashID.value, newValue);
		}
	}
}

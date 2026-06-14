using UnityEngine;

namespace Slate.ActionClips;

public class AnimateTrigger : MecanimBaseClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	public string triggerName;

	[AnimatableParameter]
	public bool value;

	public override bool isValid => base.isValid && HasParameter(triggerName);

	public override string info => $"'{triggerName}' Trigger";

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	protected override void OnUpdate(float time)
	{
		if (value)
		{
			base.actor.SetTrigger(triggerName);
		}
		else
		{
			base.actor.ResetTrigger(triggerName);
		}
	}
}

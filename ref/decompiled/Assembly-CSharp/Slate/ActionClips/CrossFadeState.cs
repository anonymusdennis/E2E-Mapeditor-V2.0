using UnityEngine;

namespace Slate.ActionClips;

[Description("CrossFades to an Animator state within a period of time.")]
public class CrossFadeState : MecanimBaseClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[Required]
	public string stateName;

	public override string info => $"CrossFade State\n'{stateName}'";

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

	public override float blendIn => length;

	protected override void OnEnter()
	{
		base.actor.CrossFade(stateName, length, -1, float.NegativeInfinity);
	}
}

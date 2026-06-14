using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Animation")]
public class PlayAnimationAdvanced : ActionTask<Animation>
{
	[RequiredField]
	public BBParameter<AnimationClip> animationClip;

	public WrapMode animationWrap;

	public AnimationBlendMode blendMode;

	[SliderField(0, 2)]
	public float playbackSpeed = 1f;

	[SliderField(0, 1)]
	public float crossFadeTime = 0.25f;

	public PlayDirections playDirection;

	public BBParameter<string> mixTransformName;

	public BBParameter<int> animationLayer;

	public bool queueAnimation;

	public bool waitActionFinish = true;

	private string animationToPlay = string.Empty;

	private int dir = -1;

	private Transform mixTransform;

	protected override string info => "Anim " + animationClip.ToString();

	protected override string OnInit()
	{
		base.agent.AddClip(animationClip.value, animationClip.value.name);
		animationClip.value.legacy = true;
		return null;
	}

	protected override void OnExecute()
	{
		if (playDirection == PlayDirections.Toggle)
		{
			dir = -dir;
		}
		if (playDirection == PlayDirections.Backward)
		{
			dir = -1;
		}
		if (playDirection == PlayDirections.Forward)
		{
			dir = 1;
		}
		base.agent.AddClip(animationClip.value, animationClip.value.name);
		animationToPlay = animationClip.value.name;
		if (!string.IsNullOrEmpty(mixTransformName.value))
		{
			mixTransform = FindTransform(base.agent.transform, mixTransformName.value);
			if (!mixTransform)
			{
				Debug.LogWarning("Cant find transform with name '" + mixTransformName.value + "' for PlayAnimation Action");
			}
		}
		else
		{
			mixTransform = null;
		}
		animationToPlay = animationClip.value.name;
		if ((bool)mixTransform)
		{
			base.agent[animationToPlay].AddMixingTransform(mixTransform, recursive: true);
		}
		base.agent[animationToPlay].layer = animationLayer.value;
		base.agent[animationToPlay].speed = (float)dir * playbackSpeed;
		base.agent[animationToPlay].normalizedTime = Mathf.Clamp01(-dir);
		base.agent[animationToPlay].wrapMode = animationWrap;
		base.agent[animationToPlay].blendMode = blendMode;
		if (queueAnimation)
		{
			base.agent.CrossFadeQueued(animationToPlay, crossFadeTime);
		}
		else
		{
			base.agent.CrossFade(animationToPlay, crossFadeTime);
		}
		if (!waitActionFinish)
		{
			EndAction(true);
		}
	}

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= base.agent[animationToPlay].length / playbackSpeed - crossFadeTime)
		{
			EndAction(true);
		}
	}

	private Transform FindTransform(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}
}

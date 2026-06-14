using UnityEngine;

namespace Slate;

[Description("With the Properties Track, you can select to animate any supported type property or field on any component on the actor, or within it's whole transform hierarchy.\n\nNote, that you can do exactly the same, by using the 'Animate Properties' ActionClip added in an 'Action Track'.")]
[Name("Properties Track")]
[UniqueElement]
public abstract class PropertiesTrack : CutsceneTrack, IKeyable, IDirectable
{
	[SerializeField]
	[HideInInspector]
	private AnimationDataCollection _animationData = new AnimationDataCollection();

	public AnimationDataCollection animationData
	{
		get
		{
			return _animationData;
		}
		set
		{
			_animationData = value;
		}
	}

	public object animatedParametersTarget => base.actor;

	protected override void OnEnter()
	{
		animationData.SetTransformContext(GetSpaceTransform(TransformSpace.CutsceneSpace));
		animationData.SetSnapshot(animatedParametersTarget);
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		animationData.SetEvaluatedValues(animatedParametersTarget, time, previousTime);
	}

	protected override void OnReverse()
	{
		animationData.RestoreSnapshot(animatedParametersTarget);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Slate.ActionClips;
using UnityEngine;
using UnityEngine.Experimental.Director;

namespace Slate;

[UniqueElement]
[Icon("Animator Icon")]
[Description("The Animator Track works with an 'Animator' Component attached on the actor, but does not require or use the Controller assigned. Instead animation clips can be played directly. The 'Base Animation Clip' will be played along the whole track length when no other animation clip is playing. This can usualy be something like an Idle.")]
[Attachable(new Type[] { typeof(ActorGroup) })]
public class AnimatorTrack : CutsceneTrack
{
	public bool IsAnimatorInChildren = true;

	private const int ROOTMOTION_FRAMERATE = 30;

	public AnimationClip baseAnimationClip;

	public bool useRootMotion;

	private Animator animator;

	private AnimationMixerPlayable mixerPlayable;

	private Dictionary<PlayAnimatorClip, int> ports;

	private int activeClips;

	private bool useBakedRootMotion;

	private List<Vector3> rmPositions;

	private List<Quaternion> rmRotations;

	private Vector3 endRootMotionPosition;

	private Quaternion endRootMotionRotation;

	private RuntimeAnimatorController wasController;

	private AnimatorCullingMode wasCullingMode;

	private bool wasRootMotion;

	private bool wasEnabled;

	public override string info => string.Format("Base Clip: {0}", (!baseAnimationClip) ? "NONE" : baseAnimationClip.name);

	private Animator AssignAnimator()
	{
		if (!IsAnimatorInChildren)
		{
			animator = base.actor.GetComponent<Animator>();
		}
		else
		{
			animator = base.actor.GetComponentInChildren<Animator>(includeInactive: true);
		}
		return animator;
	}

	protected override bool OnInitialize()
	{
		AssignAnimator();
		if (animator == null)
		{
			Debug.LogError("Animator Track requires that the actor has the Animator Component attached.", base.actor);
			return false;
		}
		return true;
	}

	private void CreateAndPlayTree()
	{
		ports = new Dictionary<PlayAnimatorClip, int>();
		mixerPlayable = AnimationMixerPlayable.Create();
		AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(baseAnimationClip);
		animationClipPlayable.state = PlayState.Paused;
		mixerPlayable.AddInput(animationClipPlayable);
		foreach (PlayAnimatorClip item in base.actions.OfType<PlayAnimatorClip>())
		{
			AnimationClipPlayable animationClipPlayable2 = AnimationClipPlayable.Create(item.animationClip);
			animationClipPlayable2.state = PlayState.Paused;
			int num = mixerPlayable.AddInput(animationClipPlayable2);
			mixerPlayable.SetInputWeight(num, 0f);
			ports[item] = num;
		}
		animator.SetTimeUpdateMode(DirectorUpdateMode.Manual);
		animator.Play(mixerPlayable);
		mixerPlayable.state = PlayState.Paused;
	}

	private void BakeRootMotion()
	{
		useBakedRootMotion = false;
		animator.applyRootMotion = true;
		rmPositions = new List<Vector3>();
		rmRotations = new List<Quaternion>();
		IEnumerable<IDirectable> children = ((IDirectable)this).children;
		float num = -1f;
		float num2 = 1f / 30f;
		int num3 = 0;
		for (float num4 = startTime; num4 <= endTime + num2; num4 += num2)
		{
			foreach (IDirectable item in children)
			{
				if (num4 >= item.startTime && num < item.startTime)
				{
					num3++;
					item.Enter();
				}
				if (num4 >= item.startTime && num4 <= item.endTime)
				{
					item.Update(num4 - item.startTime, num4 - item.startTime - num2);
				}
				if ((num4 > item.endTime || num4 >= endTime) && num <= item.endTime)
				{
					num3--;
					item.Exit();
				}
			}
			if (num3 > 0)
			{
				animator.Update(num2);
			}
			rmPositions.Add(base.actor.transform.localPosition);
			rmRotations.Add(base.actor.transform.localRotation);
			num = num4;
		}
		animator.applyRootMotion = false;
		useBakedRootMotion = true;
	}

	private void ApplyBakedRootMotion(float time)
	{
		int num = Mathf.FloorToInt(time * 30f);
		int num2 = num + 1;
		if (num2 < rmPositions.Count)
		{
			float a = (float)num * (1f / 30f);
			float b = (float)num2 * (1f / 30f);
			Vector3 a2 = rmPositions[num];
			Vector3 b2 = rmPositions[num2];
			Vector3 localPosition = Vector3.Lerp(a2, b2, Mathf.InverseLerp(a, b, time));
			base.actor.transform.localPosition = localPosition;
			endRootMotionPosition = localPosition;
			Quaternion a3 = rmRotations[num];
			Quaternion b3 = rmRotations[num2];
			Quaternion localRotation = Quaternion.Lerp(a3, b3, Mathf.InverseLerp(a, b, time));
			base.actor.transform.localRotation = localRotation;
			endRootMotionRotation = localRotation;
		}
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (!(animator == null) && mixerPlayable.IsValid() && animator.gameObject.activeInHierarchy)
		{
			if (!animator.isInitialized)
			{
				animator.Play(mixerPlayable);
			}
			if (baseAnimationClip != null)
			{
				Playable input = mixerPlayable.GetInput(0);
				input.time = time;
				mixerPlayable.SetInput(input, 0);
			}
			animator.Update(0f);
			if (useRootMotion && useBakedRootMotion)
			{
				ApplyBakedRootMotion(time);
			}
		}
	}

	public void EnableClip(PlayAnimatorClip playAnimClip)
	{
		if (!(animator == null) && mixerPlayable.IsValid())
		{
			activeClips++;
			int inputIndex = ports[playAnimClip];
			float clipWeight = playAnimClip.GetClipWeight();
			mixerPlayable.SetInputWeight(0, (activeClips != 2) ? (1f - clipWeight) : 0f);
			mixerPlayable.SetInputWeight(inputIndex, clipWeight);
		}
	}

	public void DisableClip(PlayAnimatorClip playAnimClip)
	{
		if (!(animator == null) && mixerPlayable.IsValid())
		{
			activeClips--;
			int inputIndex = ports[playAnimClip];
			mixerPlayable.SetInputWeight(0, (activeClips == 0) ? 1 : 0);
			mixerPlayable.SetInputWeight(inputIndex, 0f);
		}
	}

	public void UpdateClip(PlayAnimatorClip playAnimClip, float clipTime, float clipPrevious, float weight)
	{
		if (!(animator == null) && mixerPlayable.IsValid())
		{
			int num = ports[playAnimClip];
			Playable input = mixerPlayable.GetInput(num);
			input.time = clipTime;
			mixerPlayable.SetInput(input, num);
			mixerPlayable.SetInputWeight(num, weight);
			mixerPlayable.SetInputWeight(0, (activeClips != 2) ? (1f - weight) : 0f);
		}
	}

	private void StoreSet()
	{
		wasController = animator.runtimeAnimatorController;
		wasRootMotion = animator.applyRootMotion;
		wasCullingMode = animator.cullingMode;
		wasEnabled = animator.enabled;
		animator.applyRootMotion = false;
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		animator.enabled = false;
	}

	private void Restore()
	{
		mixerPlayable.Destroy();
		if (animator != null)
		{
			animator.runtimeAnimatorController = wasController;
			animator.applyRootMotion = wasRootMotion;
			animator.cullingMode = wasCullingMode;
			animator.enabled = wasEnabled;
		}
		if (useRootMotion)
		{
			base.actor.transform.localPosition = endRootMotionPosition;
			base.actor.transform.localRotation = endRootMotionRotation;
		}
	}

	protected override void OnEnter()
	{
		AssignAnimator();
		if (!(animator == null))
		{
			StoreSet();
			CreateAndPlayTree();
			if (useRootMotion)
			{
				BakeRootMotion();
			}
		}
	}

	protected override void OnReverseEnter()
	{
		AssignAnimator();
		if (!(animator == null))
		{
			StoreSet();
			CreateAndPlayTree();
		}
	}

	protected override void OnExit()
	{
		Restore();
	}

	protected override void OnReverse()
	{
		Restore();
	}
}

using System;
using UnityEngine;

namespace Slate;

[Description("** This Track will be deprecated in the future. Please use Animator Track instead. **\n\nThe Mecanim Track works with an 'Animator' component attached on the actor and with it's assigned Controller by modifying the Controller's parameters.\n\nConsider working with the new Animator Track instead to playback animation clips directly without the need of a Controller, which is more intuitive for animations.")]
[Icon("Animator Icon")]
[Attachable(new Type[] { typeof(ActorGroup) })]
public class MecanimTrack : CutsceneTrack
{
	private Animator animator;

	private AnimatorDispatcher _dispatcher;

	public AnimatorDispatcher dispatcher
	{
		get
		{
			if (base.actor == null)
			{
				return null;
			}
			if (_dispatcher == null || _dispatcher.gameObject != base.actor.gameObject)
			{
				_dispatcher = base.actor.GetComponent<AnimatorDispatcher>();
				if (_dispatcher == null)
				{
					_dispatcher = base.actor.gameObject.AddComponent<AnimatorDispatcher>();
				}
			}
			return _dispatcher;
		}
	}

	protected override bool OnInitialize()
	{
		animator = base.actor.GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Mecanim Track requires that the actor has the Animator Component attached.", base.actor);
			return false;
		}
		if (animator.runtimeAnimatorController == null)
		{
			Debug.LogWarning($"The Mecanim Track requires the target actor '{base.actor.name}' to have an assigned Runtime Animator Controller");
			return false;
		}
		return true;
	}

	protected override void OnReverse()
	{
		DestroyDispatcher();
	}

	protected override void OnExit()
	{
		DestroyDispatcher();
	}

	private void DestroyDispatcher()
	{
		AnimatorDispatcher component = base.actor.GetComponent<AnimatorDispatcher>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
	}
}

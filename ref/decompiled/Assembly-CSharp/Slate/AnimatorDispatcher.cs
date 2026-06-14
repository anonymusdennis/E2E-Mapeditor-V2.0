using System;
using UnityEngine;

namespace Slate;

[ExecuteInEditMode]
public class AnimatorDispatcher : MonoBehaviour
{
	private bool wasRootMotion;

	private Animator _animator;

	private Animator animator => (!(_animator != null)) ? (_animator = GetComponent<Animator>()) : _animator;

	public event Action onAnimatorMove;

	public event Action<int> onAnimatorIK;

	private void Awake()
	{
		wasRootMotion = animator.applyRootMotion;
	}

	private void OnAnimatorMove()
	{
		if (this.onAnimatorMove != null)
		{
			this.onAnimatorMove();
		}
		else if (wasRootMotion)
		{
			animator.ApplyBuiltinRootMotion();
		}
	}

	private void OnAnimatorIK(int index)
	{
		if (this.onAnimatorIK != null)
		{
			this.onAnimatorIK(index);
		}
	}
}

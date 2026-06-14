using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarryableObjectConsumer : MonoBehaviour, ICarryableObjectConsumer
{
	public delegate void ConsumedObjectHandler(CarryableObjectConsumer consumer, CarryObjectInteraction theObject);

	public delegate void ConsumedHandler(CarryableObjectConsumer consumer);

	[Header("The tags for carryable objects that we will accept as input")]
	public List<uint> m_AcceptedTags;

	[Header("The tags for carryable objects that we are hoping to get as part of some sort of goal")]
	public List<uint> m_ProcessingTags;

	[HideInInspector]
	public bool m_bDelegateToExternalForDeletionLogic = true;

	public Animator m_Animator;

	private AnimationEffectCompletdEchoer m_AnimatorEventEchoer;

	public string m_ConsumedObjectTrigger;

	private bool m_bIsConsumingObject;

	private bool m_bFinishedConsumingObject;

	public event ConsumedObjectHandler InputDroppedOnUsEvent;

	public event ConsumedHandler FinishedConsumingEvent;

	protected virtual void Awake()
	{
		if (m_Animator == null)
		{
			m_Animator = GetComponent<Animator>();
		}
		if (m_Animator != null && m_AnimatorEventEchoer == null)
		{
			m_AnimatorEventEchoer = m_Animator.GetComponent<AnimationEffectCompletdEchoer>();
		}
		if (m_AnimatorEventEchoer != null)
		{
			m_AnimatorEventEchoer.AnimationFinishedEvent += AnimatorEventEchoer_AnimationFinishedEvent;
		}
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		if (m_AnimatorEventEchoer != null)
		{
			m_AnimatorEventEchoer.AnimationFinishedEvent -= AnimatorEventEchoer_AnimationFinishedEvent;
		}
	}

	private void AnimatorEventEchoer_AnimationFinishedEvent(AnimationEffectCompletdEchoer sender)
	{
	}

	public virtual bool WillAcceptInput(CarryObjectInteraction theObject)
	{
		return !m_bIsConsumingObject && m_AcceptedTags.Contains(theObject.m_Tag);
	}

	public bool OnCarriedObjectDroppedOnUs(CarryObjectInteraction theObject)
	{
		if (WillAcceptInput(theObject))
		{
			AcceptedDropedObject(theObject);
			if (!m_bDelegateToExternalForDeletionLogic)
			{
				Object.Destroy(theObject.gameObject);
			}
			return true;
		}
		return false;
	}

	protected virtual void AcceptedDropedObject(CarryObjectInteraction theObject)
	{
		m_bIsConsumingObject = true;
		m_bFinishedConsumingObject = false;
		if (m_Animator != null)
		{
			if (!string.IsNullOrEmpty(m_ConsumedObjectTrigger))
			{
				m_Animator.SetTrigger(m_ConsumedObjectTrigger);
				StartCoroutine(WaitForCurrentClipThenFinishConsuming());
			}
			else
			{
				m_bIsConsumingObject = false;
			}
		}
		else
		{
			m_bIsConsumingObject = false;
		}
		OnInputDroppedOnUs(this, theObject);
		if (this.InputDroppedOnUsEvent != null)
		{
			this.InputDroppedOnUsEvent(this, theObject);
		}
		if (!m_bIsConsumingObject)
		{
			FinishConsuming();
		}
	}

	protected virtual void OnInputDroppedOnUs(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
	}

	private void ConsumeAnimationFinished()
	{
	}

	private IEnumerator WaitForCurrentClipThenFinishConsuming()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(m_Animator.GetCurrentAnimatorStateInfo(0).length);
		FinishConsuming();
	}

	private void FinishConsuming()
	{
		m_bIsConsumingObject = false;
		m_bFinishedConsumingObject = true;
		if (this.FinishedConsumingEvent != null)
		{
			this.FinishedConsumingEvent(this);
		}
	}

	public bool IsProcessing()
	{
		return m_bIsConsumingObject;
	}

	public bool HasItems()
	{
		return m_bFinishedConsumingObject;
	}

	public static List<uint> GetAllPossibleSpawnTags(List<CarryableObjectConsumer> consumers)
	{
		List<uint> list = new List<uint>();
		for (int num = consumers.Count - 1; num >= 0; num--)
		{
			list.AddRange(consumers[num].m_AcceptedTags);
		}
		return new HashSet<uint>(list).ToList();
	}
}

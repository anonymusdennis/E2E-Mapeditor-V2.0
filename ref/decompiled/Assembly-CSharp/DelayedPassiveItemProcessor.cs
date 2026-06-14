using SaveHelpers;
using UnityEngine;

[RequireComponent(typeof(ItemContainer))]
public class DelayedPassiveItemProcessor : ItemProcessorBase
{
	public enum State
	{
		Idle,
		StartCreateItem,
		CreatingItem,
		ProcessingItem,
		FinishedCreatingItem,
		Interrupted
	}

	protected State m_CurrentState;

	protected float m_ProcessingTime = 5f;

	protected float m_ProcessingTimer;

	private Vector3 m_StartPosition = Vector3.zero;

	public Animator m_Animator;

	public bool m_bIsInterruptable;

	public string m_StateAnimatorInt = "ProcessorState";

	public string m_AnimatorInterruptedTriggerName = "InterruptProcessing";

	public string m_AudioEventOnStartProcessing;

	protected override void Awake()
	{
		base.Awake();
		m_StartPosition = base.transform.position;
		if (m_Animator == null)
		{
			m_Animator = GetComponentInChildren<Animator>(includeInactive: true);
		}
	}

	private void Update()
	{
		if (T17NetManager.IsMasterClient)
		{
			switch (GetState())
			{
			case State.StartCreateItem:
				if (m_ItemContainer.GetItemCount() > 0)
				{
					Item item = m_ItemContainer.GetItem(0);
					ItemData initialSpawnItem = GetInitialSpawnItem(item);
					m_ItemContainer.RemoveItemRPC(item, releaseToManager: true);
					if (initialSpawnItem != null)
					{
						SetState(State.CreatingItem);
						RequestItemCreation(0, initialSpawnItem.m_ItemDataID);
					}
					else
					{
						SetState(State.Idle);
					}
				}
				break;
			case State.FinishedCreatingItem:
				if (m_ItemContainer.GetItemCount() == 0)
				{
					SetState(State.Idle);
				}
				break;
			}
		}
		if (GetState() == State.ProcessingItem)
		{
			UpdateProcessingItemState();
		}
	}

	protected override void OnItemManagerCreatedItemForUs(Item item, int eventId)
	{
		bool flag = m_ItemContainer.AddItemRPC(item);
		if (!flag)
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
		if (GetState() == State.CreatingItem)
		{
			if (flag)
			{
				SetState(State.ProcessingItem);
			}
			else
			{
				SetState(State.Idle);
			}
		}
	}

	protected virtual void UpdateProcessingItemState()
	{
		m_ProcessingTimer += UpdateManager.deltaTime;
		if (m_ProcessingTimer >= m_ProcessingTime && T17NetManager.IsMasterClient)
		{
			SetState(State.FinishedCreatingItem);
		}
	}

	public State GetState()
	{
		return m_CurrentState;
	}

	public void StartProcessingItem(Item item)
	{
		if ((GetState() == State.Idle || GetState() == State.Interrupted) && !(GetOutputItem(item) == null))
		{
			SetState(State.StartCreateItem);
		}
	}

	public Item CancelProcessing(bool removeFromProcessor)
	{
		if (!m_bIsInterruptable)
		{
			return null;
		}
		if (GetState() == State.ProcessingItem)
		{
			if (removeFromProcessor)
			{
				SetState(State.Interrupted);
			}
			else
			{
				SetState(State.FinishedCreatingItem);
			}
			Item item = m_ItemContainer.GetItem(0);
			if (item == null)
			{
				return null;
			}
			if (removeFromProcessor)
			{
				m_ItemContainer.RemoveItemRPC(item);
			}
			return item;
		}
		return null;
	}

	public bool IsBusy()
	{
		return !IsIdle() && !IsFinishedCreatingItem();
	}

	public override bool IsIdle()
	{
		return GetState() == State.Idle || GetState() == State.Interrupted;
	}

	public override bool IsFinishedCreatingItem()
	{
		return GetState() == State.FinishedCreatingItem;
	}

	public virtual void SetProcessingTime(float time)
	{
		m_ProcessingTime = time;
	}

	protected virtual ItemData GetInitialSpawnItem(Item inputItem)
	{
		return GetOutputItem(inputItem);
	}

	public float GetProcessTime()
	{
		return m_ProcessingTimer;
	}

	private void SetState(State newState)
	{
		m_NetView.PostLevelLoadRPC("RPC_SetState", NetTargets.All, (int)newState);
	}

	public float GetNormalisedProgress()
	{
		return Mathf.Min(m_ProcessingTimer / m_ProcessingTime, 1f);
	}

	protected void Local_SetProcessorTime(float time)
	{
		m_ProcessingTimer = time;
	}

	public virtual void Local_SetState(State newState)
	{
		if (m_CurrentState == newState)
		{
			return;
		}
		m_CurrentState = newState;
		if (m_Animator != null)
		{
			if (newState != State.Interrupted)
			{
				m_Animator.SetInteger(m_StateAnimatorInt, (int)m_CurrentState);
			}
			else
			{
				m_Animator.SetTrigger(m_AnimatorInterruptedTriggerName);
				m_Animator.SetInteger(m_StateAnimatorInt, 0);
			}
		}
		switch (m_CurrentState)
		{
		case State.Idle:
		case State.Interrupted:
			base.transform.position = m_StartPosition;
			break;
		case State.StartCreateItem:
			m_ProcessingTimer = 0f;
			if (!string.IsNullOrEmpty(m_AudioEventOnStartProcessing))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_AudioEventOnStartProcessing, base.gameObject);
			}
			if (m_Animator != null)
			{
				m_Animator.SetInteger("AnimState", 0);
				m_Animator.SetTrigger("SpecialToIdle");
				m_Animator.SetBool("HoldSpecialAnim", value: true);
			}
			break;
		case State.FinishedCreatingItem:
			if (m_Animator != null)
			{
				m_Animator.SetBool("HoldSpecialAnim", value: false);
			}
			break;
		case State.CreatingItem:
		case State.ProcessingItem:
			break;
		}
	}

	[PunRPC]
	protected void RPC_SetState(int newState, PhotonMessageInfo info)
	{
		Local_SetState((State)newState);
	}

	public override bool NeedsSaving()
	{
		return GetState() != 0 || m_ProcessingTimer != 0f;
	}

	public override void Serialise(ref BitField bitfield)
	{
		base.Serialise(ref bitfield);
		bitfield.Set(4, (uint)GetState());
		bitfield.Set(6, (uint)GetProcessTime());
	}

	protected override void DeserialiseWithBitfield(ref BitField bitfield)
	{
		base.DeserialiseWithBitfield(ref bitfield);
		Local_SetState((State)bitfield.GetUInt(4));
		Local_SetProcessorTime(bitfield.GetUInt(6));
	}

	public override int GetBitsPerEntry()
	{
		return base.GetBitsPerEntry() + 4 + 6;
	}
}

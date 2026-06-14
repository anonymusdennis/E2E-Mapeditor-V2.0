using UnityEngine;

public class StonemasonDesk : MonoBehaviour
{
	public enum State
	{
		NoStone,
		StonePlaced,
		StoneCarved
	}

	public T17NetView m_NetViewID;

	public NetObjectLock m_NetObjectLock;

	public CarryableObjectConsumer m_Consumer;

	public CarriedObjectDispenser m_ObjectDispenser;

	public StonemasonCarvingInteraction m_CarvingInteraction;

	private Animator m_Animator;

	public const string m_DeskNoStoneTrigger = "OnDeskNoStone";

	public const string m_DeskStonePlacedTrigger = "OnDeskWithStone";

	public const string m_DeskStoneCarvedTrigger = "OnDeskStoneCarved";

	public string m_DeskNoStoneText = "HUD.Interact.Job.Stonemason.NoStone";

	public string m_DeskStonePlacedText = "HUD.Interact.Job.Stonemason.StonePlaced";

	public string m_DeskStoneCarvedText = "HUD.Interact.Job.Stonemason.StoneCarved";

	private State m_CurrentState;

	protected virtual void Awake()
	{
		if (m_Consumer == null)
		{
			m_Consumer = GetComponent<CarryableObjectConsumer>();
		}
		if (m_ObjectDispenser == null)
		{
			m_ObjectDispenser = GetComponent<CarriedObjectDispenser>();
		}
		if (m_CarvingInteraction == null)
		{
			m_CarvingInteraction = GetComponent<StonemasonCarvingInteraction>();
		}
		if (m_NetObjectLock == null)
		{
			m_NetObjectLock = GetComponent<NetObjectLock>();
		}
		if (m_Animator == null)
		{
			m_Animator = GetComponentInChildren<Animator>();
		}
		RPC_SetState(State.NoStone, silent: true);
	}

	public void InitInteractions(bool canBeUsedOutsideJobTime)
	{
		m_ObjectDispenser.SetCanBeUsedOutsideJobTime(canBeUsedOutsideJobTime);
		m_CarvingInteraction.SetCanBeUsedOutsideJobTime(canBeUsedOutsideJobTime);
	}

	public bool StoneCarvingEnabled()
	{
		return m_CurrentState == State.StonePlaced;
	}

	public void StonecarvingComplete()
	{
		SetStateRPC(State.StoneCarved, silent: false);
	}

	public bool StoneConsumerEnabled()
	{
		return m_CurrentState == State.NoStone;
	}

	public void OnStoneConsumed()
	{
		SetStateRPC(State.StonePlaced, silent: false);
	}

	public bool StatueDispenserEnabled()
	{
		return m_CurrentState == State.StoneCarved;
	}

	public void OnStatuePickedUp()
	{
		SetStateRPC(State.NoStone, silent: false);
	}

	public void SetStateRPC(State stage, bool silent)
	{
		m_NetViewID.PostLevelLoadRPC("RPC_SetState", NetTargets.All, stage, silent);
	}

	[PunRPC]
	public void RPC_SetState(State state, bool silent)
	{
		m_CurrentState = state;
		if (m_CurrentState == State.NoStone)
		{
			m_Animator.SetTrigger("OnDeskNoStone");
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_DeskNoStoneText, localise: true);
		}
		else if (m_CurrentState == State.StonePlaced)
		{
			m_Animator.SetTrigger("OnDeskWithStone");
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_DeskStonePlacedText, localise: true);
		}
		else if (m_CurrentState == State.StoneCarved)
		{
			m_Animator.SetTrigger("OnDeskStoneCarved");
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_DeskStoneCarvedText, localise: true);
		}
	}

	public State GetState()
	{
		return m_CurrentState;
	}
}

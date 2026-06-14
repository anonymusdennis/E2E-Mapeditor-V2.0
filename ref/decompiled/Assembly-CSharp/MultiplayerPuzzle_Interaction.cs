using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MultiplayerPuzzle_Interaction : AnimatedInteraction
{
	public enum PlayersRequired
	{
		One,
		Two,
		Three,
		Four
	}

	[Serializable]
	protected class BaseInteractionState
	{
		public bool visible;

		public bool used;

		public bool valid;
	}

	[Header("Puzzle Settings")]
	public MultiplayerPuzzle_Base m_Puzzle;

	[Header("Base Interaction Settings")]
	public PlayersRequired m_PlayersRequiredIcon;

	public Transform m_MovingObjectToObserve;

	protected Vector3 m_MovingPartInitialPos = Vector3.zero;

	protected bool m_bIsInteractionVisible = true;

	private bool m_bIsValid;

	public bool IsValid()
	{
		return m_bIsValid;
	}

	protected override void Init()
	{
		base.Init();
		if (m_MovingObjectToObserve != null)
		{
			m_MovingPartInitialPos = m_MovingObjectToObserve.position;
		}
		if (m_PlayersRequiredIcon > PlayersRequired.One)
		{
			CharacterIconHandler.IconType nameplateIcon = CharacterIconHandler.IconType.MultiplayerSingle;
			switch (m_PlayersRequiredIcon)
			{
			case PlayersRequired.One:
				nameplateIcon = CharacterIconHandler.IconType.MultiplayerSingle;
				break;
			case PlayersRequired.Two:
				nameplateIcon = CharacterIconHandler.IconType.MultiplayerDouble;
				break;
			case PlayersRequired.Three:
				nameplateIcon = CharacterIconHandler.IconType.MultiplayerTriple;
				break;
			case PlayersRequired.Four:
				nameplateIcon = CharacterIconHandler.IconType.MultiplayerQuad;
				break;
			}
			m_NetObjectLock.m_TrackableElementReporter.SetNameplateIcon(nameplateIcon);
		}
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_MovingObjectToObserve != null && m_interactingCharacter != null)
		{
			Vector3 vector = m_vInteractPosition + (m_MovingObjectToObserve.position - m_MovingPartInitialPos);
			m_interactingCharacter.transform.position = vector;
			m_interactingCharacter.m_CachedCurrentPosition = vector;
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (localCharacter != null && localCharacter.m_CharacterStats != null && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return base.AllowedToInteract(localCharacter);
		}
		return false;
	}

	public void SetInteractionStateRPC(bool valid)
	{
		if (valid != m_bIsValid)
		{
			m_NetObjectLock.m_NetView.RPC("RPC_SetInteractionState", NetTargets.All, valid);
		}
	}

	[PunRPC]
	protected void RPC_SetInteractionState(bool valid, PhotonMessageInfo info)
	{
		m_bIsValid = valid;
		OnInteractionStateChanged(valid);
		if (T17NetManager.IsMasterClient && m_Puzzle != null)
		{
			m_Puzzle.OnInteractionStateChanged(this, valid);
		}
	}

	protected virtual void OnInteractionStateChanged(bool valid)
	{
	}

	public void SetInteractionActive(bool active, bool notifyAll = true)
	{
		if (active != m_bIsInteractionVisible)
		{
			SetInteractionActive_Internal(active);
			if (notifyAll)
			{
				m_NetObjectLock.m_NetView.RPC("RPC_SetInteractionActive", NetTargets.Others, active);
			}
		}
	}

	[PunRPC]
	protected void RPC_SetInteractionActive(bool active, PhotonMessageInfo info)
	{
		SetInteractionActive_Internal(active);
	}

	private void SetInteractionActive_Internal(bool active)
	{
		m_bIsInteractionVisible = active;
		if (!active && m_interactingCharacter != null)
		{
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	public override bool InteractionVisibility()
	{
		return base.InteractionVisibility() && m_bIsInteractionVisible;
	}

	protected virtual BaseInteractionState CreateInteractionStateInfo()
	{
		return new BaseInteractionState();
	}

	protected virtual void SetInteractionStateFromInfo(BaseInteractionState state)
	{
	}

	public byte[] GetSerializedInteractionState()
	{
		BaseInteractionState baseInteractionState = CreateInteractionStateInfo();
		byte[] result = new byte[0];
		if (baseInteractionState != null)
		{
			baseInteractionState.visible = m_bIsInteractionVisible;
			baseInteractionState.valid = m_bIsValid;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, baseInteractionState);
			result = memoryStream.ToArray();
		}
		return result;
	}

	public void DeserializeStateData(byte[] data)
	{
		BaseInteractionState baseInteractionState = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(data))
		{
			baseInteractionState = (BaseInteractionState)binaryFormatter.Deserialize(serializationStream);
		}
		if (baseInteractionState != null)
		{
			m_bIsValid = baseInteractionState.valid;
			SetInteractionActive_Internal(baseInteractionState.visible);
			SetInteractionStateFromInfo(baseInteractionState);
			OnInteractionStateChanged(baseInteractionState.valid);
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}

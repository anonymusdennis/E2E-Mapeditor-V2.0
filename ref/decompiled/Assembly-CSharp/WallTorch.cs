using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class WallTorch : AnimatedInteraction
{
	public enum TorchState
	{
		Unassigned,
		UnfueledTorch,
		FueledTorch,
		LitTorch
	}

	public delegate void WallTorchStageHandler(WallTorch sender);

	public Animator m_TorchAnimator;

	public ItemData m_FuelItem;

	public ItemData m_LighterItem;

	public int m_ItemHealthDecayPerFuelUse = 50;

	public int m_ItemHealthDecayPerLighterUse = 50;

	public float m_fInteractTime = 1f;

	private float m_fInteractTimer;

	public string m_UnfueledTorchText = "HUD.Interact.Job.RelightTorches.TorchJobA";

	public string m_FueledTorchText = "HUD.Interact.Job.RelightTorches.TorchJobB";

	public string m_LitTorchText = "HUD.Interact.Job.RelightTorches.TorchJobC";

	public string m_EquipFuelSpeech = "HUD.Interact.Job.RelightTorches.NeedFuel";

	public string m_EquipLighterSpeech = "HUD.Interact.Job.RelightTorches.NeedLighter";

	private TorchState m_State;

	public event WallTorchStageHandler OnWallTorchLitEvent;

	protected override void Awake()
	{
		base.Awake();
		if (m_NetObjectLock == null)
		{
			m_NetObjectLock = GetComponent<NetObjectLock>();
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		if (T17NetManager.IsMasterClient)
		{
			SetStateRPC(TorchState.UnfueledTorch);
		}
		else if (m_State == TorchState.Unassigned)
		{
			RPC_SetTorchState(TorchState.UnfueledTorch, silent: true);
		}
		return result;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.OnWallTorchLitEvent = null;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		bool flag = base.AllowedToInteract(localCharacter);
		if (m_State == TorchState.LitTorch)
		{
			return false;
		}
		bool flag2 = true;
		Item equippedItem = localCharacter.GetEquippedItem();
		int num = -1;
		if (equippedItem != null && equippedItem.m_ItemData != null)
		{
			num = equippedItem.m_ItemData.m_ItemDataID;
		}
		if (m_State == TorchState.UnfueledTorch && num != m_FuelItem.m_ItemDataID)
		{
			flag2 = false;
		}
		else if (m_State == TorchState.FueledTorch && num != m_LighterItem.m_ItemDataID)
		{
			flag2 = false;
		}
		return flag && flag2;
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		ShowPlayerDialogForCurrentState(localCharacter);
		return base.OnPlayerNotAllowedToInteract(localCharacter);
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		Item equippedItem = localCharacter.GetEquippedItem();
		int num = -1;
		if (equippedItem != null && equippedItem.m_ItemData != null)
		{
			num = equippedItem.m_ItemData.m_ItemDataID;
		}
		if (num == m_FuelItem.m_ItemDataID && m_State == TorchState.UnfueledTorch)
		{
			SetStateRPC(TorchState.FueledTorch);
			equippedItem.DecreaseHealth(m_ItemHealthDecayPerFuelUse);
			if (localCharacter.IsPlayer())
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_DLC_05_Torch_Interact_01, base.gameObject);
			}
		}
		else if (num == m_LighterItem.m_ItemDataID && m_State == TorchState.FueledTorch)
		{
			SetStateRPC(TorchState.LitTorch);
			OnWallTorchLitRPC();
			equippedItem.DecreaseHealth(m_ItemHealthDecayPerLighterUse);
			if (localCharacter.IsPlayer())
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_DLC_05_Torch_Interact_02, base.gameObject);
			}
		}
		m_fInteractTimer = 0f;
	}

	private void ShowPlayerDialogForCurrentState(Character character)
	{
		if (character.IsPlayer())
		{
			if (m_State == TorchState.UnfueledTorch)
			{
				SpeechManager.GetInstance().SaySomething(character, m_EquipFuelSpeech, SpeechTone.Negative, 3f, 10);
			}
			else if (m_State == TorchState.FueledTorch)
			{
				SpeechManager.GetInstance().SaySomething(character, m_EquipLighterSpeech, SpeechTone.Negative, 3f, 10);
			}
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_fInteractTimer < m_fInteractTime)
		{
			m_fInteractTimer += Time.deltaTime;
			if (m_fInteractTimer >= m_fInteractTime)
			{
				RequestStopInteraction(m_interactingCharacter);
			}
		}
	}

	public override bool LeaveCharacterPositionUnAltered()
	{
		return true;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public void OnWallTorchLitRPC()
	{
		m_NetViewID.RPC("RPC_OnWallTorchLit", NetTargets.All);
	}

	[PunRPC]
	public void RPC_OnWallTorchLit(PhotonMessageInfo info)
	{
		if (this.OnWallTorchLitEvent != null)
		{
			this.OnWallTorchLitEvent(this);
		}
	}

	public void SetStateRPC(TorchState state, bool silent = false)
	{
		m_NetViewID.RPC("RPC_SetTorchState", NetTargets.All, (int)state, silent);
	}

	[PunRPC]
	public void RPC_SetTorchState(TorchState state, bool silent = false)
	{
		bool flag = m_State == TorchState.LitTorch;
		m_State = state;
		SetVisualsForState(m_State, silent);
		if (m_State == TorchState.LitTorch)
		{
			if (!flag)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_DLC_05_Torch_Interact_Flames, base.gameObject);
			}
		}
		else if (flag)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_DLC_05_Torch_Interact_Flames, base.gameObject);
		}
	}

	private void SetVisualsForState(TorchState state, bool silent)
	{
		if (m_TorchAnimator == null)
		{
			return;
		}
		m_TorchAnimator.SetInteger("WallTorchState", (int)state);
		switch (m_State)
		{
		case TorchState.UnfueledTorch:
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_bIsVisibleToProximityDetector = true;
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_UnfueledTorchText, localise: true);
			}
			break;
		case TorchState.FueledTorch:
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_bIsVisibleToProximityDetector = true;
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_FueledTorchText, localise: true);
			}
			break;
		case TorchState.LitTorch:
			if (m_NetObjectLock != null)
			{
				if (!string.IsNullOrEmpty(m_LitTorchText))
				{
					m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_LitTorchText, localise: true);
				}
				else
				{
					m_NetObjectLock.m_bIsVisibleToProximityDetector = false;
				}
			}
			break;
		}
	}

	public TorchState GetTorchState()
	{
		return m_State;
	}
}

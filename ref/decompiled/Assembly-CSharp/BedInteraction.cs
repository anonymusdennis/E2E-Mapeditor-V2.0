using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class BedInteraction : AnimatedInteraction
{
	private Character m_BedOwner;

	public float m_HealthIncrease = 1f;

	public float m_EnergyIncrease = 1f;

	public float m_IncreaseStatTime = 0.5f;

	private float m_IncreaseStatTimer;

	private CellBed_ItemInteraction m_CellBedInteraction;

	public GameObject m_PinLocation;

	private bool m_bCanRecoverStamina = true;

	public bool m_bOnLevelEnteredInteraction;

	protected override void Init()
	{
		base.Init();
		m_CellBedInteraction = GetComponent<CellBed_ItemInteraction>();
	}

	public void SetOwner(Character owner)
	{
		m_BedOwner = owner;
	}

	public Character GetOwner()
	{
		return m_BedOwner;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (LevelScript.GetCurrentLevelInfo() != null && LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial)
		{
			m_bCanRecoverStamina = false;
		}
		else
		{
			m_bCanRecoverStamina = true;
		}
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
		}
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_IncreaseStatTimer = 0f;
		m_interactingCharacter.SetCharacterSleeping(isSleeping: true);
		OpenBedSaveMenu(m_interactingCharacter);
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.SetCharacterSleeping(isSleeping: false);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
			CloseBedSaveMenu(m_interactingCharacter);
		}
	}

	public override void InteractionReadyUpdate()
	{
		m_IncreaseStatTimer += UpdateManager.deltaTime;
		if (m_IncreaseStatTimer > m_IncreaseStatTime)
		{
			m_IncreaseStatTimer = 0f;
			m_interactingCharacter.m_CharacterStats.IncreaseHealthRPC(m_HealthIncrease);
			if (m_bCanRecoverStamina)
			{
				m_interactingCharacter.m_CharacterStats.IncreaseEnergyRPC(m_EnergyIncrease);
			}
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
			CloseBedSaveMenu(interactingCharacter);
		}
	}

	public override bool ShouldShowNameplateWhenNearby()
	{
		if (base.ShouldShowNameplateWhenNearby())
		{
			if (m_CellBedInteraction != null)
			{
				return !m_CellBedInteraction.HasBedDummy();
			}
			return true;
		}
		return false;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			if (m_CellBedInteraction == null || !localCharacter.m_CharacterStats.m_bIsPlayer)
			{
				return true;
			}
			return !m_CellBedInteraction.HasBedDummy();
		}
		return false;
	}

	private void OpenBedSaveMenu(Character interactingCharacter)
	{
		if (m_bOnLevelEnteredInteraction)
		{
			m_bOnLevelEnteredInteraction = false;
		}
		else
		{
			if (interactingCharacter.m_CharacterStats.GetCharacterState() != StatModifierEnum.SleepingInOwnBed || !interactingCharacter.IsPlayer())
			{
				return;
			}
			SaveManager instance = SaveManager.GetInstance();
			if (!(instance != null) || instance.CurrentSaveMode != SaveManager.SaveMode.Manual)
			{
				return;
			}
			GlobalStart instance2 = GlobalStart.GetInstance();
			CutsceneManagerBase.States state = CutsceneManagerBase.GetState();
			if (instance2 != null && !instance2.IsLoadingFlowActive() && state != CutsceneManagerBase.States.Playing)
			{
				Player player = (Player)interactingCharacter;
				if (player != null && player.m_Gamer.m_bPrimaryLocal)
				{
					InGameMenuFlow.Instance.OpenBedSave(player, player.m_PlayerCameraManagerBindingID);
				}
			}
		}
	}

	private void CloseBedSaveMenu(Character interactingCharacter)
	{
		if (!interactingCharacter.IsPlayer())
		{
			return;
		}
		Player player = (Player)interactingCharacter;
		if (player != null)
		{
			InGameMenuFlow.PlayerIGMData data = null;
			if (InGameMenuFlow.Instance.GetCorrectIGMData(player.m_PlayerCameraManagerBindingID, out data) && data != null && data.m_BedSaveMenu.gameObject.activeSelf)
			{
				InGameMenuFlow.Instance.HideBedSave(player, player.m_PlayerCameraManagerBindingID);
			}
		}
	}
}

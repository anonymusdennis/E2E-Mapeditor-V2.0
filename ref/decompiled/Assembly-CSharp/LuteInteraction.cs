using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class LuteInteraction : HandymanInteraction
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	public string m_WwiseStartLoopEvent = "Play_DLC_05_Lute_Tune";

	public string m_WwiseStopLoopEvent = "Stop_DLC_05_Lute_Tune";

	public GameObject m_MusicSource;

	public string m_NPCReactionText = "HUD.Interact.Job.Minstrel.Reaction";

	public float m_fReactionCooldownTime = 3f;

	private float m_fReactionCooldownTimer;

	protected SolitaryPotatoMasher m_Masher;

	private Vector3 m_Position;

	private RoomBlob m_LuteRoom;

	private Character m_PreviousCharacter;

	protected override void Init()
	{
		base.Init();
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus && m_NetObjectLock != null)
		{
			m_NetObjectLock.m_bIsVisibleToProximityDetector = false;
		}
		if (m_MusicSource == null)
		{
			m_MusicSource = base.gameObject;
		}
		m_Position = base.transform.position;
	}

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		m_Masher = trackedUIElements.GetMinstrelMasher();
		m_Masher.SetMasherSettings(m_MasherSettings);
		return m_Masher;
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		if (m_MusicSource != null)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_WwiseStartLoopEvent, m_MusicSource);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		m_Masher = null;
		m_PreviousCharacter = null;
		m_fReactionCooldownTimer = 0f;
		base.OnStartInteraction(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_Masher != null)
		{
			m_fReactionCooldownTimer += UpdateManager.deltaTime;
			if (m_fReactionCooldownTimer > m_fReactionCooldownTime)
			{
				TriggerNearbyNPCReactionRPC();
				m_fReactionCooldownTimer = 0f;
			}
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		if (m_MusicSource != null)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_WwiseStopLoopEvent, m_MusicSource);
		}
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
	}

	protected override void OnFixed()
	{
		base.OnFixed();
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
		}
	}

	private void TriggerNearbyNPCReactionRPC()
	{
		m_NetViewID.RPC("RPC_TriggerNearbyNPCReaction", NetTargets.MasterClient);
	}

	[PunRPC]
	public void RPC_TriggerNearbyNPCReaction()
	{
		if (m_LuteRoom == null && RoomManager.GetInstance() != null)
		{
			m_LuteRoom = RoomManager.GetInstance().LookUpRoom(m_Position);
		}
		if (m_LuteRoom == null)
		{
			return;
		}
		List<Character> charactersInRoom = m_LuteRoom.GetCharactersInRoom();
		if (charactersInRoom == null)
		{
			return;
		}
		for (int i = 0; i < charactersInRoom.Count; i++)
		{
			Character character = charactersInRoom[i];
			if (!(character != null) || !(character != m_PreviousCharacter) || character.m_CharacterRole == CharacterRole.Dog || character.IsPlayer())
			{
				continue;
			}
			if (character.m_CharacterRole == CharacterRole.Inmate || character.m_CharacterRole == CharacterRole.Guard)
			{
				AICharacter component = character.GetComponent<AICharacter>();
				if (component != null && (component.IsInCombatState() || component.IsRunning()))
				{
					continue;
				}
			}
			SpeechManager.GetInstance().SaySomething(character, m_NPCReactionText, SpeechTone.Positive, 2f);
			character.PauseMovement(0.75f);
			character.FacePosition(m_Position);
			m_PreviousCharacter = character;
			break;
		}
	}
}

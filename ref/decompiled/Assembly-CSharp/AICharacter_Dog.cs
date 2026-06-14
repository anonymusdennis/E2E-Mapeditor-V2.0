using System.Collections.Generic;
using UnityEngine;

public class AICharacter_Dog : AICharacter
{
	public GameObject m_MyKennel;

	private float m_fMaxPauseTime = 2f;

	private float m_fPauseTimeReduction = 2f;

	private float m_fContrabandHeat = 100f;

	private int m_DogLoveOpinion = 60;

	private float m_SpottedContrabandTimer;

	private float m_fAnimPauseTimer;

	private bool m_bPaused = true;

	private AnimState m_PausedAnim = AnimState.Idle3;

	private const float kWaitTime = 4f;

	public bool m_CanDetectContraband = true;

	public bool CanDetectContraband => m_CanDetectContraband;

	protected override void OnStart()
	{
		m_fMaxPauseTime = ConfigManager.GetInstance().aiConfig.GetDogMaxPauseTime();
		m_fPauseTimeReduction = ConfigManager.GetInstance().aiConfig.GetDogPauseTimeReduction();
		m_DogLoveOpinion = ConfigManager.GetInstance().aiConfig.GetDogLoveOpinion();
		m_fContrabandHeat = ConfigManager.GetInstance().aiConfig.GetDogContrabandHeat();
		NPCManager.GetInstance().AddDoggie(this);
		bool flag = true;
		if (LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison && LevelDetailsManager.c_CurrentLevelDataVersionNumber != LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			flag = false;
		}
		if (flag)
		{
			RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(base.transform.position);
			if (!(roomBlob == null))
			{
				RoomBlob_Kennel roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_Kennel>();
				if (roomBlobData == null)
				{
				}
				m_MyKennel = roomBlobData.GetNextKennel();
			}
		}
		if (!(m_MyKennel == null))
		{
		}
	}

	protected override void OnDestroy()
	{
		m_MyKennel = null;
		base.OnDestroy();
	}

	protected override void OnAddingEventToMemory(AIEvent aiEvent, AIEventMemory memory, bool silent)
	{
		if (aiEvent != null && !(aiEvent.m_EventData == null) && aiEvent.m_EventData.m_eEventType == AIEvent.EventType.ItemMissing && !(aiEvent.m_Target == null))
		{
			Item component = aiEvent.m_Target.GetComponent<Item>();
			if (!(component == null))
			{
				KeyItemContainerChanged(component);
				component.OnItemContainerChanged += KeyItemContainerChanged;
			}
		}
	}

	protected override void OnForgetEverything()
	{
	}

	protected override void OnUpdate()
	{
		if (m_SpottedContrabandTimer < m_fMaxPauseTime)
		{
			m_SpottedContrabandTimer += UpdateManager.deltaTime;
		}
		if (m_fAnimPauseTimer >= 0f)
		{
			m_fAnimPauseTimer -= UpdateManager.deltaTime;
		}
		else if (m_bPaused)
		{
			m_bPaused = false;
			m_Character.m_CharacterAnimator.StopAnimation(m_PausedAnim);
		}
	}

	public void CharacterHasContraband(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory == null || aiEventMemory.m_CharacterResponsible == null)
		{
			return;
		}
		int opinionOf = m_Character.GetOpinionOf(aiEventMemory.m_CharacterResponsible);
		if (opinionOf >= m_DogLoveOpinion)
		{
			TryPlayReaction(4f, AnimState.Idle3);
			SpeechManager.GetInstance().SaySomething(m_Character, "Text.Love", SpeechTone.No_Sound, 3f, 40);
		}
		else if (!JobsManager.GetInstance().IsCharactersContrabrandAllForActiveJob(aiEventMemory.m_CharacterResponsible))
		{
			TryPlayReaction(4f, AnimState.IdleBark);
			aiEventMemory.m_CharacterResponsible.m_CharacterStats.IncreaseHeat(m_fContrabandHeat);
			if (aiEventMemory.m_CharacterResponsible.m_CharacterStats.m_bIsPlayer)
			{
				EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, m_Character.GetStatChangeEffectPosition(), m_NetView.photonView);
			}
			SpeechManager.GetInstance().SaySomething(m_Character, "Text.Contraband", SpeechTone.No_Sound, 3f, 40);
		}
		aiEventMemory.m_AIEvent.StartCooldown(4f, CharacterRole.Dog);
		ForgetEvent(aiEventMemory);
	}

	public void SpottedContraband()
	{
		m_SpottedContrabandTimer -= m_fPauseTimeReduction;
		float pauseTime = Mathf.Max(m_SpottedContrabandTimer, 0f);
		TryPlayReaction(pauseTime, (!(Random.value > 0.7f)) ? AnimState.IdleBark : AnimState.Idle3);
	}

	private void TryPlayReaction(float pauseTime, AnimState anim)
	{
		m_Character.PauseMovement(pauseTime);
		if (!m_bPaused)
		{
			m_bPaused = true;
			m_fAnimPauseTimer = pauseTime;
			m_PausedAnim = anim;
			m_Character.m_CharacterAnimator.StartAnimation(m_PausedAnim);
		}
	}

	public GameObject GetMyKennel()
	{
		if (m_MyKennel == null)
		{
			return null;
		}
		return m_MyKennel;
	}

	public void KeyItemContainerChanged(Item item)
	{
		List<AIEventMemory> eventMemories = GetEventMemories(AIEvent.EventType.ItemMissing);
		if (eventMemories == null || item == null)
		{
			return;
		}
		for (int i = 0; i < eventMemories.Count; i++)
		{
			AIEventMemory aIEventMemory = eventMemories[i];
			if (!(aIEventMemory.GetTarget() == item.gameObject) || !(item.gameObject != null))
			{
				continue;
			}
			aIEventMemory.m_CharacterResponsible = null;
			if (item.IsOnInmate())
			{
				Character characterHoldingItem = item.GetCharacterHoldingItem();
				if (!(characterHoldingItem == null))
				{
					aIEventMemory.m_CharacterResponsible = characterHoldingItem;
				}
			}
			break;
		}
	}

	public void ReturnMissingItem(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory != null)
		{
			ItemEventManager itemEventManagerFromMemory = GetItemEventManagerFromMemory(aiEventMemory);
			if (!(itemEventManagerFromMemory == null) && !(itemEventManagerFromMemory.m_Item == null))
			{
				itemEventManagerFromMemory.m_Item.OnItemContainerChanged -= KeyItemContainerChanged;
				itemEventManagerFromMemory.m_Item.LocateAndDestroyItemRPC(RPC_CallContexts.Master);
			}
		}
	}

	private ItemEventManager GetItemEventManagerFromMemory(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory == null || aiEventMemory.m_AIEvent == null || aiEventMemory.m_AIEvent.m_Manager == null)
		{
			return null;
		}
		EventManager manager = aiEventMemory.m_AIEvent.m_Manager;
		if (manager == null)
		{
			return null;
		}
		ItemEventManager component = manager.GetComponent<ItemEventManager>();
		if (component == null)
		{
			return null;
		}
		return component;
	}
}

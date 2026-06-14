using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class ContrabandDetector : MonoBehaviour, IControlledUpdate
{
	private Animator[] m_Animators;

	private LocationEventManager m_LocationEventManager;

	public ItemData m_RegularContrabandPouch;

	public ItemData m_DurableContrabandPouch;

	public float m_StayAlertedTime = 5f;

	private float m_DeactivateTime;

	private List<Character> m_CharactersInRange = new List<Character>();

	private List<Character> m_DetectedCharacters = new List<Character>();

	private int m_AnimStateHash = -1;

	public CustomLight m_Light;

	public Color m_IdleColour = Color.green;

	public float m_IdleIntesity = 2.5f;

	public Color m_AlertColour = Color.red;

	public float m_AlertIntesity = 2.5f;

	public Vector3 m_EffectOffsetPosition;

	private int m_CurrentAnimState = -1;

	private T17NetView m_NetView;

	private void Awake()
	{
		m_AnimStateHash = Animator.StringToHash("AnimState");
		if (m_Light == null)
		{
			m_Light = GetComponentInChildren<CustomLight>();
		}
		m_NetView = GetComponent<T17NetView>();
	}

	private void Start()
	{
		m_Animators = GetComponentsInChildren<Animator>();
		m_LocationEventManager = GetComponent<LocationEventManager>();
		UpdateManager.GetInstance().Register(this, UpdateCategory.RapidPeriodic);
		if (m_Light != null)
		{
			m_Light.SetColour(m_IdleColour);
			m_Light.SetIntensity(m_IdleIntesity);
		}
	}

	protected virtual void OnDestroy()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_NetView = null;
	}

	public void ControlledUpdate()
	{
		for (int i = 0; i < m_CharactersInRange.Count; i++)
		{
			Character character = m_CharactersInRange[i];
			if (!m_DetectedCharacters.Contains(character))
			{
				bool bHasContraband = false;
				bool bPouchUsed = false;
				DetectContraband(character, out bHasContraband, out bPouchUsed);
				if (bHasContraband || bPouchUsed)
				{
					m_DetectedCharacters.Add(character);
				}
			}
		}
		bool flag = PrisonPowerManager.GetInstance().PowerIsActive();
		if (m_DeactivateTime > 0f)
		{
			m_DeactivateTime -= UpdateManager.deltaTime;
			if (!flag)
			{
				m_DeactivateTime = 0f;
				SetAnimationsState(2);
			}
			else if (m_DeactivateTime <= 0f)
			{
				SetAnimationsState(0);
			}
		}
		else if (flag)
		{
			SetAnimationsState(0);
		}
		else
		{
			SetAnimationsState(2);
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		Character componentInParent = other.GetComponentInParent<Character>();
		if (!(componentInParent != null) || m_CharactersInRange.Contains(componentInParent))
		{
			return;
		}
		m_CharactersInRange.Add(componentInParent);
		if (componentInParent.m_CharacterRole == CharacterRole.Inmate)
		{
			bool bHasContraband = false;
			bool bPouchUsed = false;
			DetectContraband(componentInParent, out bHasContraband, out bPouchUsed);
			if ((bHasContraband || bPouchUsed) && !m_DetectedCharacters.Contains(componentInParent))
			{
				m_DetectedCharacters.Add(componentInParent);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Character componentInParent = other.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			if (m_CharactersInRange.Contains(componentInParent) && !m_DetectedCharacters.Contains(componentInParent))
			{
				bool bHasContraband = false;
				bool bPouchUsed = false;
				DetectContraband(componentInParent, out bHasContraband, out bPouchUsed);
			}
			m_CharactersInRange.Remove(componentInParent);
			m_DetectedCharacters.Remove(componentInParent);
		}
	}

	private void DetectContraband(Character character, out bool bHasContraband, out bool bPouchUsed)
	{
		bHasContraband = false;
		bPouchUsed = false;
		if (character == null || !PrisonPowerManager.GetInstance().PowerIsActive() || (character.GetIsKnockedOut() && character.GetPickedUpByCharacter() != null && character.GetPickedUpByCharacter().m_CharacterRole == CharacterRole.Medic))
		{
			return;
		}
		if (IsCharacterCarryingContraband(character))
		{
			bHasContraband = true;
			if (T17NetManager.IsMasterClient)
			{
				if (!character.GetIsKnockedOut())
				{
					character.m_CharacterStats.IncreaseHeat(100f);
				}
				if (character.m_CharacterStats.m_bIsPlayer)
				{
					EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, base.transform.position + m_EffectOffsetPosition);
				}
				AIEvent investigateObjectEvent = m_LocationEventManager.GetInvestigateObjectEvent();
				NPCManager.GetInstance().CallGuards(investigateObjectEvent);
			}
			PlayAnimationRPC(1, m_StayAlertedTime);
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				ShowCharacterSpeech(character, "Text.Player.ContrabandDetect");
			}
		}
		else if (character.m_CharacterStats.m_bIsPlayer)
		{
			Item firstItemWithItemFunctionality = character.m_ItemContainer.GetFirstItemWithItemFunctionality(BaseItemFunctionality.Functionality.HideContraband);
			if (!(firstItemWithItemFunctionality != null))
			{
				return;
			}
			ContrabandHiderFunctionality contrabandHiderFunctionality = firstItemWithItemFunctionality.HasFunctionality(BaseItemFunctionality.Functionality.HideContraband) as ContrabandHiderFunctionality;
			if (contrabandHiderFunctionality != null && contrabandHiderFunctionality.IsDegradedByDetector())
			{
				if (T17NetManager.IsMasterClient)
				{
					contrabandHiderFunctionality.StartUsing(AnimState.COUNT, 0f);
				}
				bPouchUsed = true;
				if (character.m_CharacterStats.m_bIsPlayer && contrabandHiderFunctionality.IsOnLastUse())
				{
					ShowCharacterSpeech(character, "Text.Player.PouchUsed");
				}
			}
		}
		else
		{
			if (!character.IsBeingCarried())
			{
				return;
			}
			bHasContraband = character.m_ItemContainer.HasContrabandItems();
			bPouchUsed = character.m_ItemContainer.HasItemWithFunctionality(BaseItemFunctionality.Functionality.HideContraband) != 0;
			if (bHasContraband && !bPouchUsed)
			{
				if (T17NetManager.IsMasterClient)
				{
					AIEvent investigateObjectEvent2 = m_LocationEventManager.GetInvestigateObjectEvent();
					NPCManager.GetInstance().CallGuards(investigateObjectEvent2);
				}
				PlayAnimationRPC(1, m_StayAlertedTime);
			}
		}
	}

	private bool IsCharacterCarryingContraband(Character character)
	{
		if (character.m_CharacterEventManager != null)
		{
			List<AIEvent> visibleEvents = character.m_CharacterEventManager.GetVisibleEvents();
			if (visibleEvents != null)
			{
				for (int i = 0; i < visibleEvents.Count; i++)
				{
					if (visibleEvents[i].m_EventData.m_eEventType == AIEvent.EventType.Character_HasContraband)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void SetAnimationsState(int stateFlag)
	{
		if (stateFlag == m_CurrentAnimState)
		{
			return;
		}
		m_CurrentAnimState = stateFlag;
		switch (stateFlag)
		{
		default:
		{
			for (int j = 0; j < m_Animators.Length; j++)
			{
				m_Animators[j].SetInteger(m_AnimStateHash, stateFlag);
			}
			if (m_Light != null)
			{
				m_Light.gameObject.SetActive(value: true);
				m_Light.SetColour(m_IdleColour);
				m_Light.SetIntensity(m_IdleIntesity);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Contraband_Detector_Alarm, base.gameObject);
			}
			break;
		}
		case 1:
		{
			for (int k = 0; k < m_Animators.Length; k++)
			{
				m_Animators[k].SetInteger(m_AnimStateHash, stateFlag);
			}
			if (m_Light != null)
			{
				m_Light.gameObject.SetActive(value: true);
				m_Light.SetColour(m_AlertColour);
				m_Light.SetIntensity(m_AlertIntesity);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Contraband_Detector_Alarm, base.gameObject);
			}
			break;
		}
		case 2:
		{
			for (int i = 0; i < m_Animators.Length; i++)
			{
				m_Animators[i].SetInteger(m_AnimStateHash, 0);
			}
			if (m_Light != null)
			{
				m_Light.gameObject.SetActive(value: false);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Contraband_Detector_Alarm, base.gameObject);
			}
			break;
		}
		}
	}

	private void ShowCharacterSpeech(Character character, string textID)
	{
		if (character != null && !character.IsSpeaking())
		{
			SpeechManager instance = SpeechManager.GetInstance();
			if (instance != null)
			{
				instance.SaySomething(character, textID, SpeechTone.Negative, -1f, 10);
			}
		}
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	private void PlayAnimationRPC(int animID, float deactivateTime = -1f)
	{
		m_NetView.RPC("RPC_PlayAnimation", NetTargets.All, animID, deactivateTime);
	}

	[PunRPC]
	private void RPC_PlayAnimation(int animID, float deactivateTime, PhotonMessageInfo info)
	{
		if (deactivateTime >= 0f)
		{
			m_DeactivateTime = deactivateTime;
		}
		SetAnimationsState(animID);
	}
}

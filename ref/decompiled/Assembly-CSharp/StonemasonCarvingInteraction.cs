using System.Collections;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[RequireComponent(typeof(StonemasonDesk))]
public class StonemasonCarvingInteraction : MinigameInteraction
{
	public StonemasonDesk m_StonemasonDesk;

	public string m_RequiresStoneSpeech = "HUD.Interact.Job.Stonemason.NeedStone";

	private StonemasonDeskDispenser m_DispenserInteraction;

	protected override void Awake()
	{
		base.Awake();
		if (m_StonemasonDesk == null)
		{
			m_StonemasonDesk = GetComponent<StonemasonDesk>();
		}
		m_DispenserInteraction = GetComponent<StonemasonDeskDispenser>();
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (m_StonemasonDesk == null)
		{
			return false;
		}
		bool flag = base.AllowedToInteract(localCharacter);
		return flag & m_StonemasonDesk.StoneCarvingEnabled();
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			if (!m_StonemasonDesk.StoneCarvingEnabled())
			{
				SpeechManager.GetInstance().SaySomething(localCharacter, m_RequiresStoneSpeech, SpeechTone.Negative);
				return true;
			}
			return false;
		}
		return true;
	}

	protected override void OnMiniGameComplete()
	{
		base.OnMiniGameComplete();
		m_StonemasonDesk.StonecarvingComplete();
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
			StartCoroutine(DelayedPickUpStone(m_interactingCharacter));
		}
	}

	private IEnumerator DelayedPickUpStone(Character localCharacter)
	{
		localCharacter.PauseMovement(0.3f);
		yield return new WaitForSeconds(0.3f);
		if (localCharacter.IsPlayer())
		{
			(localCharacter as Player).SwallowStopInteraction();
		}
		m_DispenserInteraction.StartSpawnningForCharacter(localCharacter);
		m_StonemasonDesk.OnStatuePickedUp();
	}
}

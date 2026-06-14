using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Interact : ActionTask<Character>
{
	private enum CallBackStatus
	{
		None,
		Success,
		Failure
	}

	public BBParameter<GameObject> m_interactionTarget;

	public BBParameter<InteractiveObject> m_interactionObjTarget;

	public string m_SpecificInteraction = string.Empty;

	public bool m_bReturnOnStart;

	public bool m_bComplainAboutKick = true;

	private CallBackStatus m_bCallbackStatus;

	[BlackboardOnly]
	public BBParameter<Character> kickedByCharacter;

	protected override void OnExecute()
	{
		kickedByCharacter.value = null;
		m_bCallbackStatus = CallBackStatus.None;
		DoInteract();
	}

	protected override void OnUpdate()
	{
		CheckStatus();
	}

	private void CheckStatus()
	{
		if (m_bCallbackStatus == CallBackStatus.Failure)
		{
			EndAction(false);
		}
		else if (m_bCallbackStatus == CallBackStatus.Success)
		{
			EndAction(true);
		}
	}

	protected override void OnStop()
	{
		m_bCallbackStatus = CallBackStatus.None;
	}

	private void DoInteract()
	{
		if (m_interactionTarget.value == null && m_interactionObjTarget.value == null)
		{
			EndAction(false);
			return;
		}
		InteractiveObject interactiveObject = null;
		if (m_interactionObjTarget.value != null)
		{
			interactiveObject = m_interactionObjTarget.value;
		}
		else if (string.IsNullOrEmpty(m_SpecificInteraction))
		{
			interactiveObject = m_interactionTarget.value.GetComponentInChildren<InteractiveObject>();
		}
		else
		{
			Type type = Type.GetType(m_SpecificInteraction, throwOnError: false, ignoreCase: true);
			interactiveObject = (InteractiveObject)m_interactionTarget.value.GetComponentInChildren(type);
		}
		if (interactiveObject == null)
		{
			EndAction(false);
			return;
		}
		if (base.agent.IsInteracting())
		{
			base.agent.ForceStopInteraction();
		}
		interactiveObject.Interact(base.agent, OnInteractResponse, OnInteractionEnded);
		CheckStatus();
	}

	public void OnInteractResponse(bool success)
	{
		if (!success || m_bReturnOnStart)
		{
			if (success)
			{
				m_bCallbackStatus = CallBackStatus.Success;
			}
			else
			{
				m_bCallbackStatus = CallBackStatus.Failure;
			}
		}
	}

	public void OnInteractionEnded(bool success, int kickingCharacter)
	{
		if (!success && m_bComplainAboutKick && !base.agent.m_CharacterStats.m_bIsPlayer)
		{
			KickedByCharacter(kickingCharacter);
		}
		if (success)
		{
			m_bCallbackStatus = CallBackStatus.Success;
		}
		else
		{
			m_bCallbackStatus = CallBackStatus.Failure;
		}
	}

	private void KickedByCharacter(int kickingCharacter)
	{
		Character character = T17NetView.Find<Character>(kickingCharacter);
		kickedByCharacter.value = character;
		if (character == null)
		{
			return;
		}
		bool bAllowTextRecolour = false;
		if (character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
		{
			bAllowTextRecolour = true;
		}
		if (SpeechManager.GetInstance() != null)
		{
			SpeechManager.GetInstance().SaySomething(base.agent, "Text.Inmates.DetachedFromEquipment", SpeechTone.Negative, 2f, 5, -1, ignoreStatus: true, bAllowTextRecolour);
		}
		base.agent.PauseMovement(1f);
		base.agent.FaceCharacter(character);
		if (ConfigManager.GetInstance() != null)
		{
			AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
			if (aiConfig != null && base.agent.m_CharacterOpinions != null)
			{
				base.agent.m_CharacterOpinions.DecreaseOpinionOf(character, aiConfig.GetOpinionDecreaseWhenKickedOffInterativeObject());
				EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, character.GetStatChangeEffectPosition());
			}
		}
	}
}

using UnityEngine;

[RequireComponent(typeof(MinigameMap))]
public class MinigameInteraction : AnimatedInteraction
{
	private MinigameMap m_MinigameMap;

	private bool m_bStopInteractionRequested;

	[Tooltip("If true, if the user doesn't keep doing the minigame we will go into some idle animations")]
	[Header("MinigameInteraction")]
	public bool m_bShouldStopWithNoInput;

	public bool m_bRequireInputAtStart = true;

	public float m_NoRepsStopInterval = 4f;

	public AnimState m_NoRepsState = AnimState.Idle;

	private float m_InteractingAnimStartedTimestamp;

	private float m_InteractingAnimDuration;

	public bool m_bShouldDrainStamina;

	public float m_StaminaDrainPerMoment;

	private float m_TimeUntilIdleStop;

	private bool m_bWantsToStopInteractingAnim;

	private bool m_bHasStoppedInteractingAnim;

	private const float FINISHED_ANIM_TIME_THRESHOLD = 0.15f;

	private bool m_bHasEverCompletedARep;

	private bool m_bHaveHiddenIcon;

	protected override void Awake()
	{
		base.Awake();
		m_MinigameMap = GetComponent<MinigameMap>();
		m_bHaveHiddenIcon = false;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_MinigameMap.RestAndShowMinigame(localCharacter);
		m_bStopInteractionRequested = false;
		if (!m_bRequireInputAtStart)
		{
			m_TimeUntilIdleStop = m_NoRepsStopInterval;
		}
		else
		{
			m_TimeUntilIdleStop = 0f;
		}
		if (m_AnimationData != null && m_AnimationData.interactingAnimation != AnimState.COUNT)
		{
			m_InteractingAnimDuration = localCharacter.m_CharacterAnimator.GetAnimationLength(m_AnimationData.interactingAnimation);
		}
		m_bWantsToStopInteractingAnim = false;
		m_bHasStoppedInteractingAnim = false;
		m_bHasEverCompletedARep = false;
		m_bHaveHiddenIcon = false;
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.ShowTime && !localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			localCharacter.m_IconHandler.HideCharacterIcon(hide: true);
			m_bHaveHiddenIcon = true;
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (m_bHaveHiddenIcon)
		{
			localCharacter.m_IconHandler.HideCharacterIcon(hide: false);
		}
		base.OnExitInteraction(localCharacter);
		localCharacter.m_CharacterAnimator.StopAnimation(m_NoRepsState);
		m_MinigameMap.DisableMinigameHud(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_bStopInteractionRequested || m_interactingCharacter == null || (m_interactingCharacter.IsPlayer() && !m_MinigameMap.IsReadyForUpdate()))
		{
			return;
		}
		m_MinigameMap.UpdateInteraction(m_interactingCharacter, out var hasCompletedRep, out var hasCompletedMinigame);
		m_bHasEverCompletedARep = m_bHasEverCompletedARep || hasCompletedRep;
		if (m_bShouldStopWithNoInput && m_interactingCharacter.IsPlayer())
		{
			ProcessLogicForUserInputAnimationStop(hasCompletedRep);
		}
		if (m_bShouldDrainStamina && !m_bHasStoppedInteractingAnim && m_MinigameMap.IsSignificantMomentInMinigame())
		{
			m_interactingCharacter.m_CharacterStats.DecreaseEnergyRPC(m_StaminaDrainPerMoment);
			EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, GetEffectPositionForCharacterInInteraction(m_interactingCharacter));
		}
		if (hasCompletedMinigame)
		{
			OnMiniGameComplete();
			if (ShouldStopInteractionOnMinigameComplete())
			{
				RequestMinigameStopInteraction();
			}
		}
	}

	protected void RequestMinigameStopInteraction()
	{
		m_bStopInteractionRequested = true;
		RequestStopInteraction(m_interactingCharacter);
	}

	protected virtual bool ShouldStopInteractionOnMinigameComplete()
	{
		return true;
	}

	protected virtual void OnMiniGameComplete()
	{
	}

	private void ProcessLogicForUserInputAnimationStop(bool hasCompletedRep)
	{
		if (hasCompletedRep)
		{
			m_TimeUntilIdleStop += m_NoRepsStopInterval;
			m_bWantsToStopInteractingAnim = false;
			if (m_bHasStoppedInteractingAnim)
			{
				m_bHasStoppedInteractingAnim = false;
				SetInteractionObjectAnimatorState(2);
				if (m_AnimationData != null && m_interactingCharacter.m_CharacterAnimator.GetAnimState() != m_AnimationData.interactingAnimation)
				{
					m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_NoRepsState);
					m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_AnimationData.interactingAnimation);
				}
			}
		}
		else if (!m_bWantsToStopInteractingAnim && m_TimeUntilIdleStop >= 0f)
		{
			m_TimeUntilIdleStop -= UpdateManager.deltaTime;
			if (m_TimeUntilIdleStop < 0f)
			{
				m_bWantsToStopInteractingAnim = true;
			}
		}
		if (m_bWantsToStopInteractingAnim && !m_bHasStoppedInteractingAnim && ((!m_bHasEverCompletedARep && m_bRequireInputAtStart) || (UpdateManager.time - m_InteractingAnimStartedTimestamp) % m_InteractingAnimDuration < 0.15f))
		{
			m_bHasStoppedInteractingAnim = true;
			SetInteractionObjectAnimatorState(0);
			if (m_AnimationData != null && m_AnimationData.interactingAnimation != AnimState.COUNT)
			{
				m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.interactingAnimation);
				m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_NoRepsState);
				m_interactingCharacter.SetFaceDirection(m_NonAnimatingFaceDirection);
			}
		}
	}

	public override void SetInteractionObjectAnimatorState(int state)
	{
		base.SetInteractionObjectAnimatorState(state);
		if (state == 2)
		{
			m_InteractingAnimStartedTimestamp = UpdateManager.time;
		}
	}

	protected override bool CanEnterPlayState(Character localCharacter)
	{
		return base.CanEnterPlayState(localCharacter) && (!localCharacter.IsPlayer() || !m_bRequireInputAtStart || m_bHasEverCompletedARep);
	}

	public override bool ShouldResetAnimatorWithInteractiveUser()
	{
		return true;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		SetInteractionObjectAnimatorState(0);
	}
}

public class MultistageInputProgressorInteraction : MultistageInputInteraction
{
	public MultistageInputInteraction m_InteractionToProgress;

	public int m_InteractionMinRequiredStage;

	public SpeechPODO m_NoMoreInteractionSpeech;

	protected override bool OnInteractedWithFinalStage(Character interactingCharacter)
	{
		if (base.OnInteractedWithFinalStage(interactingCharacter))
		{
			m_InteractionToProgress.ProgressToNextStage();
			return true;
		}
		return false;
	}

	public override bool CanInteract(Character localCharacter)
	{
		if (!HasCompletedAllStages() && base.CanInteract(localCharacter))
		{
			if (m_InteractionToProgress is ConstructEndgameInteraction)
			{
				ConstructEndgameInteraction constructEndgameInteraction = m_InteractionToProgress as ConstructEndgameInteraction;
				if (!constructEndgameInteraction.CheckCorrectAmountOfPlayers(localCharacter, out var characterDialog))
				{
					PlayDialogOnCharacter(localCharacter, characterDialog);
					return false;
				}
			}
			if (HasCompletedAllStages())
			{
				PlayDialogOnCharacter(localCharacter, m_NoMoreInteractionSpeech);
				return false;
			}
			return m_InteractionToProgress.GetCurrentStage() >= m_InteractionMinRequiredStage && base.CanInteract(localCharacter);
		}
		if (HasCompletedAllStages())
		{
			PlayDialogOnCharacter(localCharacter, m_NoMoreInteractionSpeech);
			return false;
		}
		return false;
	}

	public override bool IsInteractionVisible()
	{
		return m_InteractionToProgress.GetCurrentStage() >= m_InteractionMinRequiredStage && base.IsInteractionVisible();
	}
}

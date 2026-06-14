using System;
using UnityEngine.Serialization;

[Serializable]
public class MinigameCompletionHelper
{
	public int m_NumRepsForCompletion = 1;

	[FormerlySerializedAs("m_TimeForAutoCompletion")]
	public float m_TimeForAiAutoCompletion = 5f;

	private float m_TimeUntilAiAutoCompletion = 5f;

	private int m_NumRepsDone;

	private Character m_InteractingCharacter;

	public void ResetForNewUser(Character newCharacter)
	{
		m_NumRepsDone = 0;
		m_TimeUntilAiAutoCompletion = m_TimeForAiAutoCompletion;
		m_InteractingCharacter = newCharacter;
	}

	public bool HasFinishedMinigame()
	{
		return (!m_InteractingCharacter.IsPlayer()) ? (m_TimeUntilAiAutoCompletion <= 0f) : (m_NumRepsDone == m_NumRepsForCompletion);
	}

	public bool UpdateUser(bool hasUserCompletedRep)
	{
		if (m_InteractingCharacter.IsPlayer())
		{
			if (hasUserCompletedRep)
			{
				m_NumRepsDone++;
				if (m_NumRepsDone == m_NumRepsForCompletion)
				{
					return true;
				}
			}
		}
		else
		{
			m_TimeUntilAiAutoCompletion -= UpdateManager.deltaTime;
			if (m_TimeUntilAiAutoCompletion < 0f)
			{
				return true;
			}
		}
		return false;
	}
}

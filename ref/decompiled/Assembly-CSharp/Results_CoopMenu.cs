using UnityEngine;

public class Results_CoopMenu : BaseResultsScreen
{
	public string m_LocalisedEscapeString = "TBT You Escaped!";

	public string m_TimedPrisonTimesUp = "TBT Time's Up!";

	protected override string GetMainTitleText()
	{
		if (RoutineManager.GetInstance().IsTimedPrisonTimeUp())
		{
			return m_TimedPrisonTimesUp;
		}
		return m_LocalisedEscapeString;
	}

	protected override string GetGradedScore(ScoreManager.PlayerScorePODO scorePodo, out Sprite theGradeSprite, out int gradeLevel)
	{
		if (RoutineManager.GetInstance().IsTimedPrisonTimeUp())
		{
			theGradeSprite = null;
			gradeLevel = int.MaxValue;
			return string.Empty;
		}
		return ScoreManager.GetGradedScore(scorePodo.m_IngameSecondsTakenToEscape, out theGradeSprite, out gradeLevel);
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return true;
		}
		return false;
	}

	public void RequestExit()
	{
		ResultsFlow.Instance.SetExitRequested();
	}
}

namespace NodeCanvas.BehaviourTrees;

public class MealTimeDecorator : StateDecorator
{
	protected override void OnEnter()
	{
	}

	protected override void OnExit()
	{
		m_AICharacter.m_Character.SetHasTray(hasTray: false);
	}
}

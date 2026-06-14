using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check whether a particular character has already recieved a visitor gift this free-time")]
[Category("★T17 Events")]
public class CheckCharacterRecievedVisitorGift : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_CharacterID;

	protected override string info => "Check Already Has Visitor Gift";

	protected override bool OnCheck()
	{
		if (base.agent.m_Character != null)
		{
			VisitorCustomisationManager instance = VisitorCustomisationManager.GetInstance();
			if (instance != null && m_CharacterID.value > 0)
			{
				Character character = T17NetView.Find<Character>(m_CharacterID.value);
				return instance.HasPlayerRecievedGift(character);
			}
		}
		return false;
	}
}

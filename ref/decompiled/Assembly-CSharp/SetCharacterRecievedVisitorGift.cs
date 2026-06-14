using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetCharacterRecievedVisitorGift : ActionTask<AICharacter>
{
	public BBParameter<int> m_TargetCharacterID;

	protected override void OnExecute()
	{
		VisitorCustomisationManager instance = VisitorCustomisationManager.GetInstance();
		if (instance != null && m_TargetCharacterID.value > 0)
		{
			Character character = T17NetView.Find<Character>(m_TargetCharacterID.value);
			if (character != null)
			{
				instance.SetPlayerRecievedGiftRPC(character);
			}
		}
		EndAction(true);
	}
}

using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetVisitorCustomisation : ActionTask<AICharacter>
{
	public BBParameter<List<string>> m_LocalizationTags = new BBParameter<List<string>>();

	public BBParameter<int> m_GiftID;

	protected override void OnExecute()
	{
		bool flag = false;
		VisitorCustomisationManager instance = VisitorCustomisationManager.GetInstance();
		if (instance != null)
		{
			instance.SetupNextVisitorRPC(base.agent.m_Character);
			VisitorCustomisationManager.VisitorInfo infoForVisitor = instance.GetInfoForVisitor(base.agent.m_Character);
			if (infoForVisitor != null)
			{
				base.agent.m_Character.m_CharacterCustomisation.SetCustomisation(infoForVisitor.appearance);
				base.agent.m_Character.m_CharacterCustomisation.SetOutfit(infoForVisitor.appearance.defaultOutfit);
				m_LocalizationTags.value = infoForVisitor.setupInfo.m_SpeechLines;
				m_GiftID.value = infoForVisitor.giftDataID;
				flag = true;
			}
		}
		if (!flag)
		{
		}
		EndAction(flag);
	}
}

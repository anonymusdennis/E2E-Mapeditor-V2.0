using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class PickUpItem : ActionTask<AICharacter_Guard>
{
	[BlackboardOnly]
	public BBParameter<AIEventMemory> m_EventTarget;

	protected override void OnExecute()
	{
		if (base.agent.m_Character == null || base.agent.m_Character.m_ItemContainer == null)
		{
			EndAction(false);
			return;
		}
		AIEventMemory value = m_EventTarget.value;
		if (value == null)
		{
			EndAction(false);
			return;
		}
		GameObject target = value.m_Target;
		if (target == null)
		{
			EndAction(false);
			return;
		}
		Item component = target.GetComponent<Item>();
		if (component == null)
		{
			EndAction(false);
			return;
		}
		if (component.m_ItemData.m_AlertnessIncreaseWhenFound > 0)
		{
			PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(component.m_ItemData.m_AlertnessIncreaseWhenFound, PrisonAlertnessManager.AlertnessReason.ContrabandOnFloor);
		}
		bool flag = base.agent.MoveItemToContrabandDesk(component);
		LevelScript.GetInstance().m_LevelItemContainer.RemoveItemRPC(component, !flag);
		bool flag2 = false;
		if (base.agent.m_Character.m_CharacterRole == CharacterRole.Guard && value.m_TargetCharacter != null && value.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
		{
			flag2 = true;
		}
		SpeechManager instance = SpeechManager.GetInstance();
		Character character = base.agent.m_Character;
		string textID = "Text.Guard.SentToContrabandDesk";
		SpeechTone tone = SpeechTone.Negative;
		float duration = 1f;
		bool bAllowTextRecolour = flag2;
		instance.SaySomething(character, textID, tone, duration, 0, -1, ignoreStatus: false, bAllowTextRecolour);
		base.agent.ForgetEvent(m_EventTarget.value);
		EndAction(true);
	}
}

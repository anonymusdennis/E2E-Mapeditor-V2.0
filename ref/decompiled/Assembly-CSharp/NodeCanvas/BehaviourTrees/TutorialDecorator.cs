using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

public class TutorialDecorator : StateDecorator
{
	public TutorialSubject m_TutorialSubject = TutorialSubject.UNASSIGNED;

	public BBParameter<Player> m_TutorialTargetPlayer;

	public BBParameter<GameObject> m_TutorialTargetObj;

	protected override void OnEnter()
	{
		if (m_TutorialSubject == TutorialSubject.UNASSIGNED)
		{
			return;
		}
		TutorialManager instance = TutorialManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		if (m_TutorialTargetPlayer != null && m_TutorialTargetPlayer.value != null)
		{
			instance.StartTutorialRPC(m_TutorialTargetPlayer.value, m_TutorialSubject);
			return;
		}
		if (m_TutorialTargetObj != null && m_TutorialTargetObj.value != null)
		{
			Player component = m_TutorialTargetObj.value.GetComponent<Player>();
			if (component != null)
			{
				instance.StartTutorialRPC(component, m_TutorialSubject);
			}
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null)
		{
			return;
		}
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (allPlayers[i] != null && allPlayers[i].m_Gamer != null)
			{
				instance.StartTutorialRPC(allPlayers[i], m_TutorialSubject);
			}
		}
	}
}

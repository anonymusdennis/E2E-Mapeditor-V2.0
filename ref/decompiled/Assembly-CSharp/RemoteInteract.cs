using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class RemoteInteract : ActionTask<AICharacter>
{
	public BBParameter<GameObject> m_characterTarget;

	public BBParameter<GameObject> m_interactionTarget;

	public string m_SpecificInteraction = string.Empty;

	protected override void OnExecute()
	{
		if (m_interactionTarget.value == null)
		{
			EndAction(false);
			return;
		}
		if (m_characterTarget.value == null)
		{
			EndAction(false);
			return;
		}
		InteractiveObject interactiveObject = null;
		if (string.IsNullOrEmpty(m_SpecificInteraction))
		{
			interactiveObject = m_interactionTarget.value.GetComponentInChildren<InteractiveObject>();
		}
		else
		{
			Type type = Type.GetType(m_SpecificInteraction, throwOnError: false, ignoreCase: true);
			interactiveObject = (InteractiveObject)m_interactionTarget.value.GetComponentInChildren(type);
		}
		if (interactiveObject == null || interactiveObject.m_NetObjectLock == null)
		{
			EndAction(false);
			return;
		}
		Character component = m_characterTarget.value.GetComponent<Character>();
		if (component == null)
		{
			EndAction(false);
			return;
		}
		int viewID = base.agent.m_Character.m_NetView.viewID;
		interactiveObject.m_NetObjectLock.KickInteractingCharacter(viewID);
		component.RemoteForceInteraction(interactiveObject);
		EndAction(true);
	}
}

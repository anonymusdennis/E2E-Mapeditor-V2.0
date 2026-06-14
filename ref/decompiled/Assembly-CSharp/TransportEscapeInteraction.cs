using System.Collections.Generic;
using UnityEngine;

public class TransportEscapeInteraction : ConstructEndgameInteraction
{
	[Header("Protecting Guards")]
	public List<Character> m_ProtectingGuards = new List<Character>();

	public SpeechPODO m_NeedToDealWithGuardsDialog;

	protected override bool Child_CanInteract(Character localCharacter)
	{
		if (IsFinalStage() && !HaveAllGuardsBeenDealtWith(localCharacter))
		{
			PlayDialogOnCharacter(localCharacter, m_NeedToDealWithGuardsDialog);
			return false;
		}
		return true;
	}

	private bool HaveAllGuardsBeenDealtWith(Character localCharacter)
	{
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(localCharacter.transform.position, localCharacter.CurrentFloor);
		if (roomBlob != null)
		{
			for (int num = m_ProtectingGuards.Count - 1; num >= 0; num--)
			{
				if (m_ProtectingGuards[num] != null && m_ProtectingGuards[num].m_CurrentLocation == roomBlob && !m_ProtectingGuards[num].GetIsKnockedOut())
				{
					return false;
				}
			}
		}
		return true;
	}
}

using UnityEngine;

public class CivilianClothesEscapeInteraction : ConstructEndgameInteraction
{
	[Header("Required Outfit")]
	public ItemData m_RequiredOutfit;

	public SpeechPODO m_IncorrectOutfitDialog;

	protected override bool Child_CanInteract(Character localCharacter)
	{
		if (IsFinalStage() && !HasCorrectOutfit(localCharacter))
		{
			PlayDialogOnCharacter(localCharacter, m_IncorrectOutfitDialog);
			return false;
		}
		return true;
	}

	private bool HasCorrectOutfit(Character localCharacter)
	{
		if (localCharacter == null)
		{
			return false;
		}
		if (m_RequiredOutfit == null)
		{
			return true;
		}
		Item outFit = localCharacter.GetOutFit();
		return outFit != null && outFit.ItemDataID == m_RequiredOutfit.m_ItemDataID;
	}
}

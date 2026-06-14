using System.Collections.Generic;
using UnityEngine;

public class GliderEscapeInteraction : ConstructEndgameInteraction
{
	[Header("Required Tiles")]
	public List<DamagableTile> m_TilesToDestroy = new List<DamagableTile>();

	public SpeechPODO m_NeedDestroyTilesDialog;

	protected override bool Child_CanInteract(Character localCharacter)
	{
		if (IsFinalStage() && !HaveTilesBeenDestroyed())
		{
			PlayDialogOnCharacter(localCharacter, m_NeedDestroyTilesDialog);
			return false;
		}
		return true;
	}

	private bool HaveTilesBeenDestroyed()
	{
		for (int num = m_TilesToDestroy.Count - 1; num >= 0; num--)
		{
			if (m_TilesToDestroy[num] != null && !m_TilesToDestroy[num].IsFullyDamaged())
			{
				return false;
			}
		}
		return true;
	}
}

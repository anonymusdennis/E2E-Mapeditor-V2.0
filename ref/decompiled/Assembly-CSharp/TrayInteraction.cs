using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class TrayInteraction : AnimatedInteraction
{
	public Material[] m_TrayMaterials;

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		Material randomTrayMaterial = GetRandomTrayMaterial();
		localCharacter.SetHasTray(hasTray: true, randomTrayMaterial);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item, base.gameObject);
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		RequestStopInteraction(m_interactingCharacter);
	}

	public override bool LeaveCharacterPositionUnAltered()
	{
		return true;
	}

	public override bool InteractionVisibility()
	{
		return IsMealTime();
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return IsMealTime();
	}

	private bool IsMealTime()
	{
		return RoutineManager.GetInstance() != null && RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.MealTime;
	}

	public Material GetRandomTrayMaterial()
	{
		Material result = null;
		if (m_TrayMaterials.Length > 0)
		{
			result = m_TrayMaterials[Random.Range(0, m_TrayMaterials.Length)];
		}
		return result;
	}

	protected override void OnDestroy()
	{
		if (m_TrayMaterials != null)
		{
			for (int num = m_TrayMaterials.Length - 1; num >= 0; num--)
			{
				if (m_TrayMaterials[num] != null)
				{
					m_TrayMaterials[num] = null;
				}
			}
		}
		base.OnDestroy();
	}
}

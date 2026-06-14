using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class SignPostInteraction : InteractiveObject
{
	public string m_TitleText = string.Empty;

	public string m_BodyText = string.Empty;

	public Sprite m_SignPostImage;

	public override bool AllowedToInteract(Character localCharacter)
	{
		return CanInteract();
	}

	private bool CanInteract()
	{
		return true;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				InGameMenuFlow.Instance.OpenSignPost(player, player.m_PlayerCameraManagerBindingID, m_TitleText, m_BodyText, m_SignPostImage);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Sign Post Read", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Sign Post Read", base.gameObject.name, 0L);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_JobBoard_In, AudioController.UI_Audio_GO);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if ((bool)player)
			{
				InGameMenuFlow.Instance.HideSignPost(player, player.m_PlayerCameraManagerBindingID);
			}
		}
		base.OnExitInteraction(localCharacter);
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}

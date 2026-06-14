using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChatFeedManager : T17MonoBehaviour
{
	public enum MessageTag
	{
		Unassigned,
		PlayerMsg,
		Emote,
		System,
		Prison,
		Unlock
	}

	[Serializable]
	public struct MessageFilter
	{
		public string m_FilterStart;

		public string m_FilterEnd;

		[HideInInspector]
		public Regex m_FilterRegex;

		public MessageFilter(string filterStart, string filterEnd, Regex filterRegex)
		{
			m_FilterStart = filterStart;
			m_FilterEnd = filterEnd;
			m_FilterRegex = filterRegex;
		}
	}

	private static ChatFeedManager m_Instance;

	private T17NetView m_NetView;

	public List<MessageFilter> m_MessageFilters = new List<MessageFilter>();

	public Color m_PlayerMsgTextColour = Color.white;

	public Color m_EmoteTextColour = Color.yellow;

	public Color m_SystemTextColour = Color.red;

	public Color m_PrisonTextColour = Color.cyan;

	public Color m_UnlockTextColour = Color.green;

	public string m_HostChangedlocalization = "Text.System.HostChanged";

	public string m_HostToken = "$HostName";

	public string m_EmoteSpamLocalization = "Text.Message.EmoteSpam";

	public int m_MaxNameLength = 16;

	private PlayerDataManager m_PlayerDataManager;

	private HUDMenuFlow m_HUDMenuFlow;

	private int prevFrameNum = -1;

	private string prevlocalizationTagMain;

	private string prevlocalizationTagLockdownReason;

	private string prevcharacterToken;

	private int prevcharacterViewID = -1;

	public static ChatFeedManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		m_Instance = this;
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (!(m_NetView == null))
			{
			}
		}
		for (int num = m_MessageFilters.Count - 1; num >= 0; num--)
		{
			if (string.IsNullOrEmpty(m_MessageFilters[num].m_FilterStart))
			{
				m_MessageFilters.RemoveAt(num);
			}
			else if (!string.IsNullOrEmpty(m_MessageFilters[num].m_FilterEnd))
			{
				Regex filterRegex = new Regex($"\\{m_MessageFilters[num].m_FilterStart}.*?\\{m_MessageFilters[num].m_FilterEnd}");
				m_MessageFilters[num] = new MessageFilter(m_MessageFilters[num].m_FilterStart, m_MessageFilters[num].m_FilterEnd, filterRegex);
			}
		}
		T17NetManager.OnBecameMasterClient -= AnnounceBecomingMasterClient;
		T17NetManager.OnBecameMasterClient += AnnounceBecomingMasterClient;
		base.Awake();
	}

	private void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		ClearUpGameSceneReferences();
		m_NetView = null;
		m_MessageFilters.Clear();
		if (m_Instance != null)
		{
			m_Instance = null;
		}
	}

	private void ClearUpGameSceneReferences()
	{
		m_PlayerDataManager = null;
		m_HUDMenuFlow = null;
	}

	public void SendChatMessage_RPC(Gamer sender, string message, MessageTag tag, bool bNeedslocalize, bool filter = true)
	{
		if (!(m_NetView != null) || sender == null || !(sender.m_PlayerObject != null))
		{
			return;
		}
		if (m_PlayerDataManager == null)
		{
			m_PlayerDataManager = PlayerDataManager.GetInstance();
		}
		if (m_PlayerDataManager != null)
		{
			string text = sender.m_GamerName;
			if (text.Length > m_MaxNameLength)
			{
				text = text.Substring(0, m_MaxNameLength) + "...";
			}
			string text2 = ColorUtility.ToHtmlStringRGB(m_PlayerDataManager.GetPlayerSpecificStuff(sender.m_PlayerObject.m_PlayerNumber).colour);
			if (!bNeedslocalize)
			{
				string text3 = ColorUtility.ToHtmlStringRGB(GetColourFromTag(tag));
				string[] array = null;
				array = new string[4] { text2, text, text3, message };
				string text4 = string.Format("<color=#{0}>{1}: </color><color=#{2}>{3}</color>", array);
				m_NetView.RPC("RPC_DisplayChatMessage_Desktop", NetTargets.All, text4, filter);
			}
			else
			{
				m_NetView.RPC("RPC_DisplayChatMessageLocalized", NetTargets.All, message, text, text2, (byte)tag);
			}
		}
	}

	public void SendSystemMessageLocalized_RPC(string localizationTag, MessageTag messageTag, bool bMasterClientOnly = true)
	{
		if ((!bMasterClientOnly || T17NetManager.IsMasterClient) && m_NetView != null)
		{
			m_NetView.RPC("RPC_DisplaySystemMessageLocalized", NetTargets.All, localizationTag, (byte)messageTag);
		}
	}

	public void SendSystemMessageLocalized_RPC(string localizationTag, string characterToken, string contentSafeName, MessageTag messageTag, bool bMasterClientOnly = true)
	{
		if ((!bMasterClientOnly || T17NetManager.IsMasterClient) && m_NetView != null)
		{
			m_NetView.RPC("RPC_DisplaySystemMessageLocalizedSafe", NetTargets.All, localizationTag, characterToken, contentSafeName, (byte)messageTag);
		}
	}

	public void SendSystemMessageLocalized_RPC(string localizationTag, string characterToken, int characterViewID, MessageTag messageTag, bool bMasterClientOnly = true)
	{
		if ((!bMasterClientOnly || T17NetManager.IsMasterClient) && m_NetView != null)
		{
			m_NetView.RPC("RPC_DisplaySystemMessageLocalized", NetTargets.All, localizationTag, characterToken, characterViewID, (byte)messageTag);
		}
	}

	public void SendAlertnessMessage_RPC(string localizationTagMain, string localizationTagLockdownReason, string characterToken, int characterViewID, MessageTag messageTag)
	{
		if ((prevFrameNum != UpdateManager.frameCount || !(prevlocalizationTagMain == localizationTagMain) || !(prevlocalizationTagLockdownReason == localizationTagLockdownReason) || !(prevcharacterToken == characterToken) || prevcharacterViewID != characterViewID) && m_NetView != null)
		{
			prevFrameNum = UpdateManager.frameCount;
			prevlocalizationTagMain = localizationTagMain;
			prevlocalizationTagLockdownReason = localizationTagLockdownReason;
			prevcharacterToken = characterToken;
			prevcharacterViewID = characterViewID;
			m_NetView.RPC("RPC_DisplayAlertnessMessage", NetTargets.All, localizationTagMain, localizationTagLockdownReason, characterToken, characterViewID, (byte)messageTag);
		}
	}

	[PunRPC]
	private void RPC_DisplayChatMessage_Desktop(string message, bool filter)
	{
		if (filter)
		{
			FilterMessage(ref message);
			Platform.GetInstance().FilterString(ref message);
		}
		SendMessageToHUD(message);
	}

	[PunRPC]
	private void RPC_DisplayChatMessageLocalized(string localizationTag, string senderName, string senderCol, byte eType)
	{
		string localized = string.Empty;
		if (Localization.Get(localizationTag, out localized))
		{
			string text = ColorUtility.ToHtmlStringRGB(GetColourFromTag((MessageTag)eType));
			string message = $"<color=#{senderCol}>{senderName}: </color><color=#{text}>{localized}</color>";
			SendMessageToHUD(message);
		}
	}

	[PunRPC]
	private void RPC_DisplaySystemMessageLocalized(string localizationTag, byte eType)
	{
		string localized = string.Empty;
		if (Localization.Get(localizationTag, out localized))
		{
			string arg = ColorUtility.ToHtmlStringRGB(GetColourFromTag((MessageTag)eType));
			string message = $"<color=#{arg}>{localized}</color>";
			SendMessageToHUD(message);
		}
	}

	[PunRPC]
	private void RPC_DisplaySystemMessageLocalizedSafe(string localizationTag, string characterToken, string contentSafecharacterName, byte eType)
	{
		string localized = string.Empty;
		if (!Localization.Get(localizationTag, out localized))
		{
			localized = localizationTag + " LOCALIZATION COULD NOT BE FOUND";
		}
		localized = localized.Replace(characterToken, contentSafecharacterName);
		string arg = ColorUtility.ToHtmlStringRGB(GetColourFromTag((MessageTag)eType));
		string message = $"<color=#{arg}>{localized}</color>";
		SendMessageToHUD(message);
	}

	[PunRPC]
	private void RPC_DisplaySystemMessageLocalized(string localizationTag, string characterToken, int characterViewID, byte eType)
	{
		string localized = string.Empty;
		if (!Localization.Get(localizationTag, out localized))
		{
			return;
		}
		if (characterViewID != -1)
		{
			Character character = T17NetView.Find<Character>(characterViewID);
			if (character != null)
			{
				localized = localized.Replace(characterToken, character.m_CharacterCustomisation.m_DisplayName);
			}
		}
		string arg = ColorUtility.ToHtmlStringRGB(GetColourFromTag((MessageTag)eType));
		string message = $"<color=#{arg}>{localized}</color>";
		SendMessageToHUD(message);
	}

	[PunRPC]
	private void RPC_DisplayAlertnessMessage(string localizationTagMain, string localizationTagLockdownReason, string characterToken, int characterViewID, byte eType)
	{
		string localized = string.Empty;
		if (!Localization.Get(localizationTagMain, out localized))
		{
			return;
		}
		string localized2 = string.Empty;
		if (!Localization.Get(localizationTagLockdownReason, out localized2))
		{
			return;
		}
		if (!string.IsNullOrEmpty(localized2) && !string.IsNullOrEmpty(characterToken) && characterViewID != -1)
		{
			Character character = T17NetView.Find<Character>(characterViewID);
			if (character != null)
			{
				localized2 = localized2.Replace(characterToken, character.m_CharacterCustomisation.m_DisplayName);
			}
		}
		string arg = localized + " " + localized2;
		string arg2 = ColorUtility.ToHtmlStringRGB(GetColourFromTag((MessageTag)eType));
		string message = $"<color=#{arg2}>{arg}</color>";
		SendMessageToHUD(message);
	}

	private Color GetColourFromTag(MessageTag tag)
	{
		return tag switch
		{
			MessageTag.PlayerMsg => m_PlayerMsgTextColour, 
			MessageTag.Emote => m_EmoteTextColour, 
			MessageTag.System => m_SystemTextColour, 
			MessageTag.Prison => m_PrisonTextColour, 
			MessageTag.Unlock => m_UnlockTextColour, 
			_ => Color.magenta, 
		};
	}

	private void FilterMessage(ref string message)
	{
		for (int i = 0; i < m_MessageFilters.Count; i++)
		{
			if (m_MessageFilters[i].m_FilterRegex != null)
			{
				message = m_MessageFilters[i].m_FilterRegex.Replace(message, string.Empty);
			}
			else
			{
				message = message.Replace(m_MessageFilters[i].m_FilterStart, string.Empty);
			}
		}
	}

	private void AnnounceBecomingMasterClient()
	{
		if (m_NetView != null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				m_NetView.RPC("RPC_DisplaySystemMessageLocalizedSafe", NetTargets.All, m_HostChangedlocalization, m_HostToken, primaryGamer.m_GamerName, (byte)3);
			}
		}
	}

	private void SendMessageToHUD(string message)
	{
		if (m_HUDMenuFlow == null)
		{
			m_HUDMenuFlow = HUDMenuFlow.Instance;
		}
		if (!(m_HUDMenuFlow != null))
		{
			return;
		}
		for (int i = 0; i < m_HUDMenuFlow.m_PlayersHUDData.Count; i++)
		{
			if (m_HUDMenuFlow.m_PlayersHUDData[i].m_PlayerBindingID != 0 && m_HUDMenuFlow.m_PlayersHUDData[i].m_ChatFeedHUD != null)
			{
				m_HUDMenuFlow.m_PlayersHUDData[i].m_ChatFeedHUD.AddFeedMessage(message);
			}
		}
	}

	public void DisplayMessageToUser(Gamer target, string message, MessageTag messageType, bool bLocalize)
	{
		if (target == null || !target.IsLocal() || !(target.m_PlayerObject != null) || !(m_HUDMenuFlow != null))
		{
			return;
		}
		m_HUDMenuFlow.GetCorrectHUDData(target.m_PlayerObject.m_PlayerCameraManagerBindingID, out var data);
		if (data == null || !(data.m_ChatFeedHUD != null))
		{
			return;
		}
		if (bLocalize)
		{
			string localized = string.Empty;
			if (!Localization.Get(message, out localized))
			{
				return;
			}
			message = localized;
		}
		string arg = ColorUtility.ToHtmlStringRGB(GetColourFromTag(messageType));
		string message2 = $"<color=#{arg}>{message}</color>";
		data.m_ChatFeedHUD.AddFeedMessage(message2);
	}

	public void ShowGameModeMessage(Gamer target, PrisonConfig.ConfigType gameMode)
	{
		switch (gameMode)
		{
		case PrisonConfig.ConfigType.Cooperative:
			DisplayMessageToUser(target, "Text.GameMode.Coop", MessageTag.System, bLocalize: true);
			break;
		case PrisonConfig.ConfigType.Versus:
			DisplayMessageToUser(target, "Text.GameMode.Versus", MessageTag.System, bLocalize: true);
			break;
		case PrisonConfig.ConfigType.Singleplayer:
			DisplayMessageToUser(target, "Text.GameMode.Singleplayer", MessageTag.System, bLocalize: true);
			break;
		}
	}

	public void ShowOnlineModeMessage(T17NetRoomGameView.GameRoomType onlineMode, bool bDisplayToAllPlayers = true, Gamer target = null)
	{
		string text = string.Empty;
		switch (onlineMode)
		{
		case T17NetRoomGameView.GameRoomType.Undefined:
			return;
		case T17NetRoomGameView.GameRoomType.Offline:
			text = "Text.OnlineMode.Offline";
			break;
		case T17NetRoomGameView.GameRoomType.Public:
			text = "Text.OnlineMode.Public";
			break;
		case T17NetRoomGameView.GameRoomType.Private:
			text = "Text.OnlineMode.Private";
			break;
		}
		if (bDisplayToAllPlayers)
		{
			SendSystemMessageLocalized_RPC(text, MessageTag.System, bMasterClientOnly: false);
		}
		else if (target != null)
		{
			DisplayMessageToUser(target, text, MessageTag.System, bLocalize: true);
		}
	}

	public static void ForceCleanup()
	{
		if (m_Instance != null)
		{
			m_Instance.ClearUpGameSceneReferences();
		}
	}
}

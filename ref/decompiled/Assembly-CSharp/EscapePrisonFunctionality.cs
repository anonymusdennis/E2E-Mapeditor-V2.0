using System.Collections.Generic;
using Slate;
using UnityEngine;

public class EscapePrisonFunctionality : MonoBehaviour
{
	private static EscapePrisonFunctionality s_Instance;

	private List<Gamer> m_GamersInEscapeTriggers = new List<Gamer>();

	private Player m_LastEscapedPlayer;

	private EscapeMethod m_LastEscapeMethod;

	private Cutscene m_LastEscapedCharacterRequestedCutscene;

	private bool m_bEscapeInProgress;

	private T17NetView m_NetView;

	private bool m_bPlayedEscapeCutscene;

	public static EscapePrisonFunctionality GetInstance()
	{
		return s_Instance;
	}

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
			Gamer.OnDeleteImminent += Gamer_OnDeleteImminent;
			Gamer.OnDeleted += Gamer_OnDeleted;
		}
		else
		{
			Object.Destroy(this);
		}
		m_NetView = GetComponent<T17NetView>();
		GlobalStart.EnteredLevelEvent += EnteredLevel;
	}

	protected virtual void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		Gamer.OnDeleteImminent -= Gamer_OnDeleteImminent;
		Gamer.OnDeleted -= Gamer_OnDeleted;
		GlobalStart.EnteredLevelEvent -= EnteredLevel;
		m_NetView = null;
	}

	private void Gamer_OnDeleteImminent(Gamer gamer)
	{
		m_GamersInEscapeTriggers.Remove(gamer);
	}

	private void Gamer_OnDeleted()
	{
		if (GlobalStart.GetInstance().GetMode() != GlobalStart.GLOBALSTART_MODE.IN_LEVEL)
		{
			return;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		switch (instance.gameType)
		{
		case PrisonConfig.ConfigType.Cooperative:
		case PrisonConfig.ConfigType.Singleplayer:
			HandleGamerDeletedCoop();
			break;
		case PrisonConfig.ConfigType.Versus:
			if (!T17NetInvites.HasInvite())
			{
				HandleGamerDeletedVersus();
			}
			break;
		}
	}

	private void HandleGamerDeletedCoop()
	{
		if (T17NetManager.IsMasterClient && CanTriggerEscape())
		{
			if (m_LastEscapedPlayer != null && m_LastEscapedCharacterRequestedCutscene != null)
			{
				TriggerEscapeRPC(m_LastEscapedCharacterRequestedCutscene, m_LastEscapeMethod, m_LastEscapedPlayer);
			}
			else
			{
				GlobalStart.GetInstance().EndLevel(bShowResults: true);
			}
		}
	}

	private void EnteredLevel()
	{
		if (T17NetworkManager.CheckIfLastPlayerVersus())
		{
			GlobalStart.GetInstance().EndLevel(bShowResults: true);
		}
	}

	private void HandleGamerDeletedVersus()
	{
		if (T17NetworkManager.CheckIfLastPlayerVersus())
		{
			GlobalStart.GetInstance().EndLevel(bShowResults: true);
		}
	}

	private void ForceGlobalStartToExitToVersusScreen()
	{
		GlobalStart instance = GlobalStart.GetInstance();
		instance.m_ReturnToFrontendRoute = GlobalStart.ReturnToFrontendRoutes.Versus;
		instance.EndLevel(bShowResults: false);
	}

	public static bool IsEscapeSpecial(EscapeMethod method)
	{
		return method != 0 && method != EscapeMethod.NothingSpecial;
	}

	public static bool IsEscapeClassicSpecial(EscapeMethod method)
	{
		return method == EscapeMethod.Crate || method == EscapeMethod.Documentary || method == EscapeMethod.Dolphin || method == EscapeMethod.EscapePod || method == EscapeMethod.Glider || method == EscapeMethod.HumanDisguise || method == EscapeMethod.Jetpack || method == EscapeMethod.Motorcycle || method == EscapeMethod.MultiplayerTrashBag || method == EscapeMethod.ScubaGear || method == EscapeMethod.Sewers || method == EscapeMethod.SingleplayerTrashBag || method == EscapeMethod.StolenJeep || method == EscapeMethod.Submarine || method == EscapeMethod.UFO || method == EscapeMethod.Zipline;
	}

	public void CharacterLeftEscapeTriggerRPC(Character character, EscapeMethod method)
	{
		if (character != null && character.m_CharacterStats.m_bIsPlayer)
		{
			m_NetView.RPC("RPC_PlayerLeftEscapeTrigger", NetTargets.All, character.m_NetView.viewID, method);
		}
	}

	[PunRPC]
	private void RPC_PlayerLeftEscapeTrigger(int characterViewId, EscapeMethod method)
	{
		Player player = T17NetView.Find<Player>(characterViewId);
		if (player != null && player.m_Gamer != null && !m_GamersInEscapeTriggers.Remove(player.m_Gamer))
		{
		}
	}

	public void CharacterReachedEscapeTriggerRPC(Character character, EscapeMethod method, Cutscene cutsceneToPlayIfEscaping)
	{
		if (character != null && character.m_CharacterStats.m_bIsPlayer)
		{
			int num = -1;
			if (CutsceneManagerBase.GetInstance() != null)
			{
				num = CutsceneManagerBase.GetInstance().GetCutsceneIndex(cutsceneToPlayIfEscaping);
			}
			m_NetView.RPC("RPC_PlayerReachedEscapeTrigger", NetTargets.All, character.m_NetView.viewID, method, num);
		}
	}

	[PunRPC]
	private void RPC_PlayerReachedEscapeTrigger(int characterViewId, EscapeMethod method, int cutsceneIndexIfEscaping)
	{
		Player player = T17NetView.Find<Player>(characterViewId);
		if (!(player != null))
		{
			return;
		}
		m_LastEscapeMethod = method;
		m_LastEscapedPlayer = player;
		if (player.m_Gamer == null)
		{
			return;
		}
		if (!m_GamersInEscapeTriggers.Contains(player.m_Gamer))
		{
			m_GamersInEscapeTriggers.Add(player.m_Gamer);
		}
		Cutscene cutscene = ((!(CutsceneManagerBase.GetInstance() != null)) ? null : CutsceneManagerBase.GetInstance().GetCutsceneAtIndex(cutsceneIndexIfEscaping));
		if (cutscene != null)
		{
			m_LastEscapedCharacterRequestedCutscene = cutscene;
		}
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (CanTriggerEscape())
		{
			TriggerEscapeRPC(cutscene, method, m_LastEscapedPlayer);
			return;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null)
		{
			PrisonConfig.ConfigType gameType = instance.gameType;
			if ((gameType == PrisonConfig.ConfigType.Cooperative || gameType == PrisonConfig.ConfigType.Singleplayer) && !AllCharactersEscaped())
			{
				SpeechManager.GetInstance().SaySomething(player, "Text.Escape.Helicopter.MultiplayerProximity", SpeechTone.Negative, 3f);
			}
		}
	}

	private bool CanTriggerEscape()
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null)
		{
			switch (instance.gameType)
			{
			case PrisonConfig.ConfigType.Cooperative:
			case PrisonConfig.ConfigType.Singleplayer:
				return AllCharactersEscaped();
			case PrisonConfig.ConfigType.Versus:
				return m_GamersInEscapeTriggers.Count > 0 || Gamer.GetGamerCount() <= 1;
			}
		}
		return false;
	}

	private bool AllCharactersEscaped()
	{
		return Gamer.GetGamerCount() != 0 && m_GamersInEscapeTriggers.Count == Gamer.GetGamerCount();
	}

	public void TriggerEscape()
	{
		CutsceneManagerBase instance = CutsceneManagerBase.GetInstance();
		if (instance != null && instance.m_GenericEscapeCutscene != null)
		{
			TriggerEscapeRPC(instance.m_GenericEscapeCutscene, EscapeMethod.Unknown);
		}
		else
		{
			T17NetworkManager.GetInstance().EndLevelRPC();
		}
	}

	public void TriggerEscapeRPC(Cutscene escapeCutscene, EscapeMethod escapeMethod, Character escapingCharacter = null)
	{
		CutsceneManagerBase instance = CutsceneManagerBase.GetInstance();
		int num = -1;
		if (instance != null)
		{
			num = instance.GetCutsceneIndex(escapeCutscene);
		}
		int num2 = -1;
		if (escapingCharacter != null)
		{
			num2 = escapingCharacter.m_NetView.viewID;
		}
		m_NetView.RPC("RPC_EscapePrison", NetTargets.All, num, escapeMethod, num2);
	}

	[PunRPC]
	private void RPC_EscapePrison(int cutsceneIndex, EscapeMethod escapeMethod, int escapingCharacterViewId)
	{
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.EscapeState, 1);
			ConfigManager instance = ConfigManager.GetInstance();
			if (null != instance && instance.gameType == PrisonConfig.ConfigType.Cooperative && T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
			{
				T17NetRoomManager instance2 = T17NetRoomManager.Instance;
				if (null != instance2)
				{
					instance2.SetPropertiesForGameroomType(T17NetRoomGameView.GameRoomType.Private);
				}
			}
		}
		if (m_bEscapeInProgress)
		{
			return;
		}
		m_bEscapeInProgress = true;
		Player player = T17NetView.Find<Player>(escapingCharacterViewId);
		bool flag = Gamer.GetGamerCount() > 1;
		bool flag2 = IsEscapeSpecial(escapeMethod);
		if (T17NetManager.IsMasterClient)
		{
			if (player != null && flag && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
			{
				if (flag2)
				{
					ScoreManager.EventRPC(ScoreManager.Events.SpecialEscapeTriggerReached, player);
				}
				else
				{
					ScoreManager.EventRPC(ScoreManager.Events.EscapeTriggerReached, player);
				}
			}
			else
			{
				List<Player> allPlayers = Player.GetAllPlayers();
				for (int num = allPlayers.Count - 1; num >= 0; num--)
				{
					if (allPlayers[num] != null && allPlayers[num].m_Gamer != null)
					{
						if (IsEscapeClassicSpecial(escapeMethod))
						{
							ScoreManager.EventRPC(ScoreManager.Events.SpecialEscapeTriggerReached, allPlayers[num]);
						}
						else
						{
							ScoreManager.EventRPC(ScoreManager.Events.EscapeTriggerReached, allPlayers[num]);
						}
					}
				}
			}
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		ConfigManager instance3 = ConfigManager.GetInstance();
		if (instance3 != null && instance3.gameType == PrisonConfig.ConfigType.Cooperative)
		{
			if (T17NetManager.IsMasterClient && currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Number of Players On Escape", "User Generated Content Escapes", Gamer.GetGamerCount() + " Player", 0L);
			}
			Gamer[] allGamers = Gamer.GetAllGamers();
			foreach (Gamer gamer in allGamers)
			{
				if (gamer == null || !gamer.IsLocal())
				{
					continue;
				}
				StatSystem.GetInstance().AddIDStat(8, (int)escapeMethod, gamer);
				if (flag2 && currentLevelInfo != null && currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Normal)
				{
					StatSystem.GetInstance().AddIDStat(29, (int)escapeMethod, gamer);
				}
				if (currentLevelInfo != null)
				{
					int num2 = -1;
					LevelScript.PRISON_ENUM prisonEnum = currentLevelInfo.m_PrisonEnum;
					switch (prisonEnum)
					{
					case LevelScript.PRISON_ENUM.Centre_Perks:
						num2 = 33;
						break;
					case LevelScript.PRISON_ENUM.OldWestFort:
						num2 = 34;
						break;
					case LevelScript.PRISON_ENUM.POW_Camp:
						num2 = 35;
						break;
					case LevelScript.PRISON_ENUM.Space_Prison:
						num2 = 38;
						break;
					case LevelScript.PRISON_ENUM.Gulag_Prison:
						num2 = 36;
						break;
					case LevelScript.PRISON_ENUM.Oil_Rig:
						num2 = 37;
						break;
					case LevelScript.PRISON_ENUM.Area_17:
						num2 = 39;
						break;
					case LevelScript.PRISON_ENUM.Dictator:
						num2 = 40;
						break;
					case LevelScript.PRISON_ENUM.DLC02:
						num2 = 46;
						break;
					case LevelScript.PRISON_ENUM.DLC03:
						num2 = 50;
						break;
					case LevelScript.PRISON_ENUM.DLC04:
						num2 = 52;
						break;
					case LevelScript.PRISON_ENUM.DLC05:
						num2 = 55;
						break;
					case LevelScript.PRISON_ENUM.DLC06:
						num2 = 59;
						break;
					case LevelScript.PRISON_ENUM.Transport_Train:
						num2 = 43;
						break;
					case LevelScript.PRISON_ENUM.Transport_Boat:
						num2 = 41;
						break;
					case LevelScript.PRISON_ENUM.Transport_Plane:
						num2 = 42;
						break;
					case LevelScript.PRISON_ENUM.Tutorial:
						num2 = 45;
						break;
					}
					if (num2 >= 0)
					{
						StatSystem.GetInstance().AddIDStat(num2, (int)escapeMethod, gamer);
					}
					StatSystem.GetInstance().AddIDStat(7, (int)prisonEnum, gamer);
				}
				if (gamer.m_bPrimaryLocal)
				{
					KeyAwardManager.OnPrisonEscaped(gamer, currentLevelInfo, escapeMethod);
				}
			}
		}
		if (T17NetManager.IsMasterClient && instance3.gameType == PrisonConfig.ConfigType.Versus)
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Versus Escape Method", currentLevelInfo.m_PrisonEnum.ToString() + " Versus Escape", escapeMethod.ToString(), 0L);
		}
		m_LastEscapeMethod = escapeMethod;
		m_LastEscapedPlayer = player;
		CutsceneManagerBase instance4 = CutsceneManagerBase.GetInstance();
		Cutscene cutscene = null;
		if (instance4 != null)
		{
			cutscene = instance4.GetCutsceneAtIndex(cutsceneIndex);
			if (cutscene == null)
			{
				cutscene = instance4.m_GenericEscapeCutscene;
			}
		}
		if (cutscene == null || (instance3 != null && instance3.gameType == PrisonConfig.ConfigType.Versus))
		{
			m_bPlayedEscapeCutscene = false;
			if ((bool)GlobalStart.GetInstance() && GlobalStart.GetInstance().m_PreviewEditorLevel)
			{
				CutsceneFinishedEvent_LocallyEndLevel(0f);
				return;
			}
			FadeManager instance5 = FadeManager.GetInstance();
			if (instance5 != null)
			{
				instance5.StartCurtainLower(delegate
				{
					CutsceneFinishedEvent_LocallyEndLevel(0f);
				});
			}
			else
			{
				CutsceneFinishedEvent_LocallyEndLevel(0f);
			}
		}
		else if (cutscene != null)
		{
			m_bPlayedEscapeCutscene = true;
			if (flag)
			{
				instance4.MasterPlayMultiplayerCutsceneRPC(cutscene, UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects.FadeToOpaque_Hold);
			}
			else
			{
				instance4.PlayCutsceneSetupRPC(cutscene, UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects.FadeToOpaque_Hold);
			}
			Platform.GetInstance().SetNativeVoiceChatEnabled(state: false);
			CutsceneManagerBase.CutsceneFinishedEvent += CutsceneFinishedEvent_LocallyEndLevel;
		}
	}

	private void CutsceneFinishedEvent_LocallyEndLevel(float timeUntilCurtainRaised)
	{
		Platform.GetInstance().SetNativeVoiceChatEnabled(state: true);
		CutsceneManagerBase.CutsceneFinishedEvent -= CutsceneFinishedEvent_LocallyEndLevel;
		GlobalStart.GetInstance().EndLevel(bShowResults: true);
	}

	public Character GetEscapingCharacter()
	{
		return m_LastEscapedPlayer;
	}

	public EscapeMethod GetEscapeMethodUsed()
	{
		return m_LastEscapeMethod;
	}

	public bool DidPlayEscapeCutscene()
	{
		return m_bPlayedEscapeCutscene;
	}
}

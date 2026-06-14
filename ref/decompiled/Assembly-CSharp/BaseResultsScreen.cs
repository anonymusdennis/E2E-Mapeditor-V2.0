using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseResultsScreen : FrontendMenuBehaviour
{
	[Header("Results screen")]
	public List<Results_CharacterProfile> m_PlayerProfiles = new List<Results_CharacterProfile>();

	[Header("Shared UI")]
	public T17Text m_MainTitleLabel;

	public T17Text m_MediumTitleLabel;

	public T17Text m_SmallTitleLabel;

	public T17Text m_StickerLabel;

	public string m_NobodyEscapedText = "TBT: Nobody Escaped";

	public string m_NoOtherPlayersLeftText = "TBT: Longest Serving Inmate";

	public Image m_BackgroundImage;

	public GameObject m_UnlockableContainer;

	public Animator m_UnlockableAnimator;

	public string m_UnlockedTrigger;

	[Header("Personal Score Labels")]
	public T17Text m_ItemsCraftedLabel;

	public T17Text m_KnockoutsLabel;

	public T17Text m_FavoursCompletedLabel;

	public T17Text m_TilesDamagedLabel;

	public T17Text m_DistanceTravelledLabel;

	public T17Text m_TimeTakenLabel;

	public T17Image m_EscapeGradeImage;

	public GameObject m_EscapeTick;

	public string m_EscapeTickAnimation;

	private Animator m_EscapeTickAnimator;

	[Header("Intro Sequence")]
	public float m_IntroSequenceInitialDelay = 1f;

	public float m_IntroSequenceElementDelay = 0.5f;

	public int m_MinTickThreshold = 3;

	public List<GameObject> m_IntroSequence;

	private int m_TheGradeLevel = -1;

	private LevelScript.PRISON_ENUM m_CachedPrisonEnum;

	private bool m_bScoreLoopPlaying;

	protected abstract string GetMainTitleText();

	protected abstract string GetGradedScore(ScoreManager.PlayerScorePODO scorePodo, out Sprite theGradeSprite, out int gradeLevel);

	protected override void Awake()
	{
		base.Awake();
		if (m_EscapeTick != null)
		{
			m_EscapeTickAnimator = m_EscapeTick.GetComponentInChildren<Animator>();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_BackgroundImage.sprite = null;
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Remove(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		ClearupAudioEvents();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		SetupCharacterProfiles();
		ScoreManager.PlayerScorePODO scorePodoForCharacter = ScoreManager.GetScorePodoForCharacter(GetWinningPlayer());
		if (scorePodoForCharacter != null)
		{
			SetupTitles(scorePodoForCharacter);
			SetupFooterLabels(scorePodoForCharacter);
			StartCoroutine("DoIntroSequence", scorePodoForCharacter);
			if (T17NetManager.IsMasterClient)
			{
				LevelScript.PRISON_ENUM prisonEnum = LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
				if (prisonEnum != LevelScript.PRISON_ENUM.CustomPrison)
				{
					GoogleAnalyticsV3.LogCommericalAnalyticEvent("Prison Escaped Play Time (ingame days)", prisonEnum.ToString() + " Total Play Time (ingame days)", "Ingame days to escape", (long)(scorePodoForCharacter.m_IngameSecondsTakenToEscape / 86400f));
				}
			}
		}
		SetupBackgroundImage();
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Combine(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] != null && allGamers[i].m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(allGamers[i].m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGameMenu);
			}
		}
		return true;
	}

	protected virtual Player GetWinningPlayer()
	{
		return Gamer.GetPrimaryGamer().m_PlayerObject;
	}

	private void SetupBackgroundImage()
	{
		PrisonData levelSetup = LevelScript.GetInstance().m_LevelSetup;
		if (levelSetup != null && !string.IsNullOrEmpty(levelSetup.m_RoundResultsImagePath))
		{
			Sprite sprite = Resources.Load<Sprite>(levelSetup.m_RoundResultsImagePath);
			if (m_BackgroundImage != null && sprite != null)
			{
				m_BackgroundImage.sprite = sprite;
			}
		}
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Remove(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
		StopCoroutine("DoIntroSequence");
		ClearupAudioEvents();
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	private void ProcessUnlockables()
	{
		bool flag = CheckForUnlockableItems();
		if (flag && m_UnlockableAnimator != null)
		{
			m_UnlockableAnimator.SetTrigger(m_UnlockedTrigger);
		}
		if (m_UnlockableContainer != null)
		{
			m_UnlockableContainer.SetActive(flag);
		}
	}

	private bool CheckForUnlockableItems()
	{
		ProgressManager instance = ProgressManager.GetInstance();
		if (instance != null)
		{
			int numberOfPendingRewards = instance.GetNumberOfPendingRewards();
			return numberOfPendingRewards > 0;
		}
		return false;
	}

	private void SetupTitles(ScoreManager.PlayerScorePODO scorePodo)
	{
		PrisonData levelSetup = LevelScript.GetInstance().m_LevelSetup;
		if (m_MainTitleLabel != null)
		{
			m_MainTitleLabel.SetLocalisedTextCatchAll(GetMainTitleText());
		}
		if (m_MediumTitleLabel != null)
		{
			if (levelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
			{
				m_MediumTitleLabel.SetNonLocalizedText(levelSetup.m_NameLocalizationKey);
			}
			else
			{
				m_MediumTitleLabel.SetLocalisedTextCatchAll(levelSetup.m_NameLocalizationKey);
			}
		}
		EscapePrisonFunctionality instance = EscapePrisonFunctionality.GetInstance();
		EscapeMethod escapeMethodUsed = instance.GetEscapeMethodUsed();
		Character escapingCharacter = instance.GetEscapingCharacter();
		if (m_SmallTitleLabel != null)
		{
			string localisedTextCatchAll;
			if (escapeMethodUsed == EscapeMethod.Unknown && escapingCharacter == null)
			{
				bool flag = false;
				List<Player> allPlayers = Player.GetAllPlayers();
				for (int num = allPlayers.Count - 1; num >= 0; num--)
				{
					if (allPlayers[num] != null && allPlayers[num].m_Gamer != null && allPlayers[num].m_Gamer != Gamer.GetPrimaryGamer())
					{
						flag = true;
					}
				}
				localisedTextCatchAll = ((!flag) ? m_NoOtherPlayersLeftText : m_NobodyEscapedText);
			}
			else
			{
				localisedTextCatchAll = "EscapeMethods." + escapeMethodUsed;
			}
			m_SmallTitleLabel.SetLocalisedTextCatchAll(localisedTextCatchAll);
		}
		m_TheGradeLevel = -1;
		Sprite theGradeSprite;
		string gradedScore = GetGradedScore(scorePodo, out theGradeSprite, out m_TheGradeLevel);
		if (m_StickerLabel != null)
		{
			if (scorePodo.m_IngameSecondsTakenToEscape > 0f)
			{
				m_StickerLabel.SetLocalisedTextCatchAll(gradedScore);
			}
			else
			{
				m_StickerLabel.gameObject.SetActive(value: false);
			}
		}
		if (m_EscapeGradeImage != null)
		{
			if (theGradeSprite == null)
			{
				m_IntroSequence.Remove(m_EscapeGradeImage.gameObject);
				m_EscapeGradeImage.gameObject.SetActive(value: false);
				m_EscapeGradeImage.sprite = null;
			}
			else
			{
				m_EscapeGradeImage.sprite = theGradeSprite;
			}
		}
		bool flag2 = T17NetManager.NetOnlineMode || Gamer.GetGamerCount() > 1;
		Platform.LeaderboardGameType leaderboardGameType = Platform.LeaderboardGameType.COUNT;
		ConfigManager instance2 = ConfigManager.GetInstance();
		bool flag3 = true;
		if (escapeMethodUsed == EscapeMethod.Unknown || escapingCharacter == null || scorePodo.m_IngameSecondsTakenToEscape < 1f)
		{
			flag3 = false;
		}
		else
		{
			bool isMasterClient = T17NetManager.IsMasterClient;
			if (instance2 != null)
			{
				switch (instance2.gameType)
				{
				case PrisonConfig.ConfigType.Cooperative:
				case PrisonConfig.ConfigType.Singleplayer:
				{
					if (flag2)
					{
						leaderboardGameType = Platform.LeaderboardGameType.Multiplayer;
						flag3 = isMasterClient;
						break;
					}
					leaderboardGameType = Platform.LeaderboardGameType.SinglePlayer;
					flag3 = false;
					if (!PrisonSnapshotIO.IsCurrentPrisonAllowedToPostToSPLeaderboard())
					{
						break;
					}
					EscapePrisonFunctionality instance4 = EscapePrisonFunctionality.GetInstance();
					if (!(instance4 != null))
					{
						break;
					}
					Character escapingCharacter3 = instance4.GetEscapingCharacter();
					if (escapingCharacter3 != null)
					{
						Player player2 = escapingCharacter3 as Player;
						if (player2 != null && player2.m_Gamer == Gamer.GetPrimaryGamer())
						{
							flag3 = true;
						}
					}
					break;
				}
				case PrisonConfig.ConfigType.Versus:
				{
					if (T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Offline)
					{
						flag3 = false;
						break;
					}
					leaderboardGameType = Platform.LeaderboardGameType.Versus;
					flag3 = false;
					EscapePrisonFunctionality instance3 = EscapePrisonFunctionality.GetInstance();
					if (!(instance3 != null))
					{
						break;
					}
					Character escapingCharacter2 = instance3.GetEscapingCharacter();
					if (escapingCharacter2 != null)
					{
						Player player = escapingCharacter2 as Player;
						if (player != null && player.m_Gamer == Gamer.GetPrimaryGamer())
						{
							flag3 = true;
						}
					}
					break;
				}
				}
			}
		}
		if (flag3 && levelSetup.m_LevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.Tutorial)
		{
			m_CachedPrisonEnum = levelSetup.m_LevelInfo.m_PrisonEnum;
			Platform.GetInstance().PostToLeaderboard(m_CachedPrisonEnum, leaderboardGameType, LeaderboardPostCallback, (int)scorePodo.m_IngameSecondsTakenToEscape, 0);
		}
		if (T17NetManager.IsMasterClient)
		{
			if (levelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Tutorial", "Tutorial Completed", string.Empty, 0L);
			}
			else if (instance2.gameType != PrisonConfig.ConfigType.Versus)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Campaign Prison Escape", levelSetup.m_LevelInfo.m_PrisonEnum.ToString() + " Escaped", escapeMethodUsed.ToString(), 0L);
				if (leaderboardGameType == Platform.LeaderboardGameType.SinglePlayer)
				{
					GoogleAnalyticsV3.LogCommericalAnalyticEvent("Prison Escape Single Player Only", levelSetup.m_LevelInfo.m_PrisonEnum.ToString() + " Escaped SP Only", escapeMethodUsed.ToString(), 0L);
				}
			}
			if (instance2.gameType != PrisonConfig.ConfigType.Versus)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Non-Versus Escape Time", levelSetup.m_LevelInfo.m_PrisonEnum.ToString() + " Escaped in Days", levelSetup.m_LevelInfo.m_PrisonEnum.ToString(), (long)(scorePodo.m_IngameSecondsTakenToEscape / 86400f));
			}
		}
		if (instance2.gameType != PrisonConfig.ConfigType.Versus)
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Quests Completed on Escape", levelSetup.m_LevelInfo.m_PrisonEnum.ToString() + " Completed on Escape", levelSetup.m_LevelInfo.m_PrisonEnum.ToString(), (int)scorePodo.m_FavoursCompleted);
		}
	}

	private void LeaderboardPostCallback(bool bBetterScore, bool result)
	{
	}

	private void SetupFooterLabels(ScoreManager.PlayerScorePODO scorePodo)
	{
		if (m_ItemsCraftedLabel != null)
		{
			m_ItemsCraftedLabel.m_bNeedsLocalization = false;
			m_ItemsCraftedLabel.text = scorePodo.m_ItemsCrafted.ToString();
		}
		if (m_KnockoutsLabel != null)
		{
			m_KnockoutsLabel.m_bNeedsLocalization = false;
			m_KnockoutsLabel.text = scorePodo.m_CharactersKnockedOut.ToString();
		}
		if (m_FavoursCompletedLabel != null)
		{
			m_FavoursCompletedLabel.m_bNeedsLocalization = false;
			m_FavoursCompletedLabel.text = scorePodo.m_FavoursCompleted.ToString();
		}
		if (m_TilesDamagedLabel != null)
		{
			m_TilesDamagedLabel.m_bNeedsLocalization = false;
			m_TilesDamagedLabel.text = scorePodo.m_TilesDestroyed.ToString();
		}
		if (m_DistanceTravelledLabel != null)
		{
			m_DistanceTravelledLabel.m_bNeedsLocalization = false;
			m_DistanceTravelledLabel.text = ((float)scorePodo.m_Steps / ScoreManager.GetInstance().m_NumStepsInKilometer).ToString();
		}
		if (m_TimeTakenLabel != null)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(scorePodo.m_IngameSecondsTakenToEscape);
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if ((currentLevelInfo != null && currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Tutorial) || scorePodo.m_IngameSecondsTakenToEscape == 0f)
			{
				m_TimeTakenLabel.m_bNeedsLocalization = true;
				m_TimeTakenLabel.SetLocalisedTextCatchAll("Text.General.N/A");
				return;
			}
			m_TimeTakenLabel.m_bNeedsLocalization = false;
			Localization.Get("text.ui.day", out var localized);
			string text = $"{localized} {timeSpan.Days + 1:D2}   {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
			m_TimeTakenLabel.text = text;
		}
	}

	private void SetupCharacterProfiles()
	{
		List<Player> list = Player.GetAllPlayers().FindAll((Player x) => x != null && x.m_Gamer != null);
		int count = list.Count;
		if (count > m_PlayerProfiles.Count)
		{
			return;
		}
		ScoreManager instance = ScoreManager.GetInstance();
		int num = ((!(instance == null)) ? instance.GetPlayerIndexMostCrafted() : 0);
		int num2 = ((!(instance == null)) ? instance.GetPlayerIndexMostKnockouts() : 0);
		int num3 = ((!(instance == null)) ? instance.GetPlayerIndexMostFavoursCompleted() : 0);
		int num4 = ((!(instance == null)) ? instance.GetPlayerIndexMostTilesDestroyed() : 0);
		int i;
		for (i = 0; i < count; i++)
		{
			Results_CharacterProfile results_CharacterProfile = m_PlayerProfiles[i];
			m_IntroSequence.Remove(results_CharacterProfile.m_MostCraftedAward);
			m_IntroSequence.Remove(results_CharacterProfile.m_MostKnockoutsAward);
			m_IntroSequence.Remove(results_CharacterProfile.m_MostFavoursAward);
			m_IntroSequence.Remove(results_CharacterProfile.m_MostPrisonTilesAward);
			results_CharacterProfile.SetupForPlayer(list[i]);
			results_CharacterProfile.gameObject.SetActive(value: true);
			int spawnIndex = list[i].m_SpawnIndex;
			if (m_ItemsCraftedLabel != null && spawnIndex == num)
			{
				InsertIntoIntroSeq(results_CharacterProfile.m_MostCraftedAward, m_ItemsCraftedLabel.gameObject);
			}
			if (m_KnockoutsLabel != null && spawnIndex == num2)
			{
				InsertIntoIntroSeq(results_CharacterProfile.m_MostKnockoutsAward, m_KnockoutsLabel.gameObject);
			}
			if (m_FavoursCompletedLabel != null && spawnIndex == num3)
			{
				InsertIntoIntroSeq(results_CharacterProfile.m_MostFavoursAward, m_FavoursCompletedLabel.gameObject);
			}
			if (m_TilesDamagedLabel != null && spawnIndex == num4)
			{
				InsertIntoIntroSeq(results_CharacterProfile.m_MostPrisonTilesAward, m_TilesDamagedLabel.gameObject);
			}
			results_CharacterProfile.SetMostCraftedAwardActive(state: false);
			results_CharacterProfile.SetMostKnockoutsAwardActive(state: false);
			results_CharacterProfile.SetMostFavoursAwardActive(state: false);
			results_CharacterProfile.SetMostDestroyedTilesAwardActive(state: false);
		}
		for (; i < m_PlayerProfiles.Count; i++)
		{
			m_PlayerProfiles[i].gameObject.SetActive(value: false);
		}
	}

	protected void InsertIntoIntroSeq(GameObject theObject, GameObject insertAfter)
	{
		if (theObject == null)
		{
			return;
		}
		for (int i = 0; i < m_IntroSequence.Count; i++)
		{
			if (m_IntroSequence[i] == theObject)
			{
				return;
			}
		}
		if (insertAfter != null)
		{
			int j = 0;
			for (int num = m_IntroSequence.Count - 1; j < num; j++)
			{
				if (m_IntroSequence[j] == insertAfter)
				{
					m_IntroSequence.Insert(j + 1, theObject);
					return;
				}
			}
		}
		m_IntroSequence.Add(theObject);
	}

	protected virtual void OnEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 16)
		{
			ReturnToLobby();
		}
	}

	protected void ReturnToLobby()
	{
		ResultsFlow instance = ResultsFlow.Instance;
		if (instance != null)
		{
			ClearupAudioEvents();
			instance.ReturnToLobby();
		}
	}

	private void ClearupAudioEvents()
	{
		if (m_bScoreLoopPlaying)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, "Stop_UI_RoundResults_Score_Loop", base.gameObject);
		}
	}

	private IEnumerator DoIntroSequence(ScoreManager.PlayerScorePODO scorePodo)
	{
		for (int k = 0; k < m_IntroSequence.Count; k++)
		{
			if (m_IntroSequence[k] != null)
			{
				m_IntroSequence[k].SetActive(value: false);
			}
		}
		yield return new WaitForSecondsRealtime(m_IntroSequenceInitialDelay);
		for (int i = 0; i < m_IntroSequence.Count; i++)
		{
			if (!(m_IntroSequence[i] != null))
			{
				continue;
			}
			m_IntroSequence[i].SetActive(value: true);
			int targetValue = -1;
			T17Text destTextItem = null;
			if (m_ItemsCraftedLabel != null && m_IntroSequence[i] == m_ItemsCraftedLabel.gameObject)
			{
				targetValue = scorePodo.m_ItemsCrafted;
				destTextItem = m_ItemsCraftedLabel;
			}
			if (m_KnockoutsLabel != null && m_IntroSequence[i] == m_KnockoutsLabel.gameObject)
			{
				targetValue = scorePodo.m_CharactersKnockedOut;
				destTextItem = m_KnockoutsLabel;
			}
			if (m_FavoursCompletedLabel != null && m_IntroSequence[i] == m_FavoursCompletedLabel.gameObject)
			{
				targetValue = scorePodo.m_FavoursCompleted;
				destTextItem = m_FavoursCompletedLabel;
			}
			if (m_TilesDamagedLabel != null && m_IntroSequence[i] == m_TilesDamagedLabel.gameObject)
			{
				targetValue = scorePodo.m_TilesDestroyed;
				destTextItem = m_TilesDamagedLabel;
			}
			if (m_DistanceTravelledLabel != null && m_IntroSequence[i] == m_DistanceTravelledLabel.gameObject)
			{
				targetValue = (int)((float)scorePodo.m_Steps / ScoreManager.GetInstance().m_NumStepsInKilometer);
				destTextItem = m_DistanceTravelledLabel;
			}
			if (m_EscapeGradeImage != null && m_TheGradeLevel != -1 && m_IntroSequence[i] == m_EscapeGradeImage.gameObject)
			{
				if (m_TheGradeLevel <= 2)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, "Play_UI_RoundResults_Pass", base.gameObject);
				}
				else
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, "Play_UI_RoundResults_Fail", base.gameObject);
				}
			}
			if (m_EscapeTick != null && m_EscapeTickAnimator != null && m_IntroSequence[i] == m_EscapeTick)
			{
				PrisonData levelSetup = LevelScript.GetInstance().m_LevelSetup;
				KeyAwardManager instance = KeyAwardManager.GetInstance();
				EscapePrisonFunctionality instance2 = EscapePrisonFunctionality.GetInstance();
				if (instance2 != null && instance != null && instance.GetNumberOfTimesEscaped(levelSetup.m_LevelInfo.m_PrisonEnum, instance2.GetEscapeMethodUsed()) == 1)
				{
					m_EscapeTickAnimator.Play(m_EscapeTickAnimation, 0);
				}
				else
				{
					m_EscapeTick.SetActive(value: false);
				}
			}
			if (destTextItem != null)
			{
				m_bScoreLoopPlaying = true;
				float t = 0f;
				int j = targetValue;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, "Play_UI_RoundResults_Score_Loop", base.gameObject);
				while (t <= m_IntroSequenceElementDelay)
				{
					if (targetValue < m_MinTickThreshold)
					{
						destTextItem.text = targetValue.ToString("D6");
					}
					else
					{
						j = (int)Mathf.Lerp(0f, targetValue, t / m_IntroSequenceElementDelay);
						destTextItem.text = j.ToString("D6");
					}
					yield return null;
					t += Time.unscaledDeltaTime;
					if ((j >= targetValue || t > m_IntroSequenceElementDelay) && m_bScoreLoopPlaying)
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, "Stop_UI_RoundResults_Score_Loop", base.gameObject);
						m_bScoreLoopPlaying = false;
					}
				}
				destTextItem.text = targetValue.ToString("D6");
			}
			else
			{
				yield return new WaitForSecondsRealtime(m_IntroSequenceElementDelay);
			}
		}
	}
}

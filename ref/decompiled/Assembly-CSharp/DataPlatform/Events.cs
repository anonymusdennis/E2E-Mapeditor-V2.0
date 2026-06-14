using System;
using System.Diagnostics;

namespace DataPlatform;

public class Events
{
	[Conditional("UNITY_XBOXONE")]
	public static void SendAddBandTime(string UserId, ref Guid PlayerSessionId, float BandPlayTimeInMinutes)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendAddNakedTime(string UserId, ref Guid PlayerSessionId, float TimeInMinutes)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendAddShowerTime(string UserId, ref Guid PlayerSessionId, float ShowerTimeInMinutes)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendAddSolitaryTime(string UserId, ref Guid PlayerSessionId, float SolitaryTimesInMinutes)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendBumpInTheNight(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCakeAttack(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCardioStatChanged(string UserId, ref Guid PlayerSessionId, int CurrentCardioValue)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendClowningAroundQuest(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCompletedAJob(string UserId, ref Guid PlayerSessionId, int JobID, bool WasNaked)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendCustomizedCharacter(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendDaysTakenToEscape(string UserId, ref Guid PlayerSessionId, uint DaysTaken, int MapID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendDaysWithoutHeat(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendEarnedCash(string UserId, ref Guid PlayerSessionId, uint CashEarned)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendEnergySwordDuel(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendEternallyGratefulQuest(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendFavourCompleted(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendFriendlyDog(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendGainedMultiplayerOnlyAreaAccess(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendGameProgress(string UserId, ref Guid PlayerSessionId, float CompletionPercent)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendGiftedTea(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendGuardKnockedOut(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendHighScore(string UserId, ref Guid PlayerSessionId, uint Score, int MapID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendHighscoreMultiplayer(string UserId, ref Guid PlayerSessionId, uint MultiplayerHighscore, int MapID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendHighscoreVersus(string UserId, ref Guid PlayerSessionId, uint VersusHighscore, int MapID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendIllusionNotTrickQuest(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendIncreaseDaysServed(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendIncreaseKnockOuts(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendIncreaseSolitaryVisits(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendInmateKnockedOut(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendIntellectStatChanged(string UserId, ref Guid PlayerSessionId, int CurrentIntellectValue)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendItemAcquired(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ItemId, int AcquisitionMethodId, float LocationX, float LocationY, float LocationZ)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendItemCrafted(string UserId, ref Guid PlayerSessionId, uint UniqueCount)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendItemUsed(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ItemId, float LocationX, float LocationY, float LocationZ)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendKnockedEmAllOut(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMauledByADog(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMediaUsage(string AppSessionId, string AppSessionStartDateTime, uint UserIdType, string UserId, string SubscriptionTierType, string SubscriptionTier, string MediaType, string ProviderId, string ProviderMediaId, string ProviderMediaInstanceId, ref Guid BingId, ulong MediaLengthMs, uint MediaControlAction, float PlaybackSpeed, ulong MediaPositionMs, ulong PlaybackDurationMs, string AcquisitionType, string AcquisitionContext, string AcquisitionContextType, string AcquisitionContextId, int PlaybackIsStream, int PlaybackIsTethered, string MarketplaceLocation, string ContentLocale, float TimeZoneOffset, uint ScreenState)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMultiplayerRoundEnd(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId, float TimeInSeconds, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMultiplayerRoundStart(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendMuseumQuest(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendNakedDinner(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendObjectiveEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendObjectiveStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPageAction(string UserId, ref Guid PlayerSessionId, int ActionTypeId, int ActionInputMethodId, string Page, string TemplateId, string DestinationPage, string Content)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPageView(string UserId, ref Guid PlayerSessionId, string Page, string RefererPage, int PageTypeId, string PageTags, string TemplateId, string Content)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayedAClassicGameOnline(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayedAVersusGame(string UserId, ref Guid PlayerSessionId, bool WasOnline, bool WasOnlineWin, int AwardID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionEnd(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionPause(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionResume(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPlayerSessionStart(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendPrisonCompleted(string UserId, ref Guid PlayerSessionId, int PrisonID, int EscapeMethodID)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendRevivedByMedic(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendRoyalFlush(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSectionEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSectionStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSeenAGhost(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendSetCurrentPrisonDays(string UserId, ref Guid PlayerSessionId, uint DaysInCurrentPrison)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendStageFright(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendStrengthStatChanged(string UserId, ref Guid PlayerSessionId, int CurrentStrengthValue)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendTaggedSomething(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendTriggeredEscape(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendTriggeredLockdown(string UserId, ref Guid PlayerSessionId)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendViewOffer(string UserId, ref Guid PlayerSessionId, ref Guid OfferGuid, ref Guid ProductGuid)
	{
	}

	[Conditional("UNITY_XBOXONE")]
	public static void SendWhatItDoesInTheShadows(string UserId, ref Guid PlayerSessionId)
	{
	}
}

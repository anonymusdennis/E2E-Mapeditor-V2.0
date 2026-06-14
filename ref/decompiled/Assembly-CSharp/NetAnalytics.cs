using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameAnalytics;
using UnityEngine;

public class NetAnalytics : MonoBehaviour
{
	public enum Tracker
	{
		NONE = 1,
		SCREENS = 2,
		EXCEPTIONS = 4,
		STARTSESSIONS = 8,
		STOPSESSIONS = 0x10,
		EVENTS = 0x20,
		CUSTOMEVENTS = 0x40,
		SOCIAL = 0x80,
		TIMIMG = 0x100,
		TRANSACTIONS = 0x200,
		ITEMS = 0x400,
		GAMEANALYTICS = 0x800,
		MAX_TRACKERS = 0x1000
	}

	public enum EventCategory
	{
		NONE = 1,
		ACHIEVEMENT = 2,
		ANALYTICS = 4,
		LEADERBOARD = 8,
		NETWORK = 0x10,
		ONLINE = 0x20,
		UI = 0x40,
		MISSION = 0x80,
		ITEM = 0x100,
		MAX_EVENTS = 0x200
	}

	public enum EventAction
	{
		NONE = 1,
		ENTER = 2,
		FORCE = 4,
		GET = 8,
		LEAVE = 0x10,
		LOGIN = 0x20,
		POST = 0x40,
		PRESS = 0x80,
		UNLOCK = 0x100,
		STATS = 0x200,
		PURCHASE = 0x400,
		UPGRADE = 0x800,
		MAX_ACTIONS = 0x1000
	}

	private static NetAnalytics m_instance;

	private bool m_googleAnalyticsV3Running;

	private GoogleAnalyticsV3 m_googleAnalyticsV3;

	private GoogleAnalyticsMPV3 m_googleAnalyticsMPV3;

	private string m_playerUserID = string.Empty;

	public static NetAnalytics Instance => m_instance;

	private void Awake()
	{
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		GAHelpers.TrackerActive(Tracker.NONE, active: true);
		GAHelpers.CategoryActive(EventCategory.NONE, active: true);
		GAHelpers.TrackerActive(Tracker.EXCEPTIONS, active: false);
	}

	internal static void UpdateAnalyticsTracker(int trackerFlags)
	{
		if (trackerFlags == 0)
		{
			return;
		}
		GAHelpers.ActiveTrackerHelper.ClearActiveTrackers();
		int num = Enum.GetNames(typeof(Tracker)).Length - 1;
		for (int i = 1; i < num; i++)
		{
			long num2 = 1 << i;
			bool flag = (trackerFlags & num2) == num2;
			if (flag)
			{
				Tracker logTracker = (Tracker)(1 << i);
				GAHelpers.TrackerActive(logTracker, flag);
				if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
				{
				}
			}
		}
	}

	internal static void UpdateAnalyticsEvents(int eventFlags)
	{
		if (eventFlags == 0 || !GAHelpers.TrackerActive(Tracker.EVENTS))
		{
			return;
		}
		GAHelpers.ActiveCategoriesHelper.ClearActiveCategories();
		int num = Enum.GetNames(typeof(EventCategory)).Length - 1;
		for (int i = 1; i < num; i++)
		{
			long num2 = 1 << i;
			bool flag = (eventFlags & num2) == num2;
			if (flag)
			{
				EventCategory category = (EventCategory)(1 << i);
				GAHelpers.CategoryActive(category, flag);
				if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
				{
				}
			}
		}
	}

	public static void UpdateAnalyticsTrackersAndEvents(string trackerFlags, string eventFlags)
	{
		if (!string.IsNullOrEmpty(trackerFlags))
		{
		}
		if (string.IsNullOrEmpty(eventFlags))
		{
		}
	}

	public void StartSession()
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.STARTSESSIONS))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			m_playerUserID = "Undefined";
			if (m_googleAnalyticsV3 != null)
			{
				m_googleAnalyticsV3.SetUserIDOverride(m_playerUserID);
				m_googleAnalyticsV3.StartSession();
			}
		}
	}

	public void StopSession()
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.STARTSESSIONS))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				m_googleAnalyticsV3.StopSession();
			}
		}
	}

	public void LogException(string exceptionDescription, bool isFatal, string userData)
	{
	}

	public void LogScreen(string screenTitle)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.SCREENS))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				AppViewHitBuilder builder = new AppViewHitBuilder().SetScreenName(screenTitle);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogScreen(builder);
			}
		}
	}

	public void LogEvent(EventCategory eventCategory, EventAction eventAction, string eventLabel, long value, ReadOnlyCollection<KeyValuePair<int, string>> extraCustomDimensions = null)
	{
		if (!m_googleAnalyticsV3Running || !GAHelpers.TrackerActive(Tracker.EVENTS) || !GAHelpers.CategoryActive(eventCategory))
		{
			return;
		}
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
		{
		}
		if (!(m_googleAnalyticsV3 != null))
		{
			return;
		}
		EventHitBuilder eventHitBuilder = new EventHitBuilder().SetEventCategory(eventCategory.ToString()).SetEventAction(eventAction.ToString()).SetEventLabel(eventLabel)
			.SetEventValue(value);
		AddDefaultCustomDimensionsAndMetrics(eventHitBuilder);
		if (extraCustomDimensions != null)
		{
			IEnumerator<KeyValuePair<int, string>> enumerator = extraCustomDimensions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				eventHitBuilder.SetCustomDimension(enumerator.Current.Key, enumerator.Current.Value);
			}
		}
		m_googleAnalyticsV3.LogEvent(eventHitBuilder);
	}

	public void LogCustomEvent(EventCategory eventCategory, EventAction eventAction, string eventLabel, long value, Dictionary<string, object> customEventDictionary)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.EVENTS) && GAHelpers.CategoryActive(eventCategory))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null && m_googleAnalyticsMPV3 != null)
			{
				m_googleAnalyticsV3.LogEvent(eventCategory.ToString(), eventAction.ToString(), eventLabel.ToUpper(), value);
				EventHitBuilder eventHitBuilder = new EventHitBuilder();
				eventHitBuilder.SetEventCategory(eventCategory.ToString());
				eventHitBuilder.SetEventAction(eventAction.ToString());
				eventHitBuilder.SetEventLabel(eventLabel.ToUpper());
				eventHitBuilder.SetEventValue(value);
				AddDefaultCustomDimensionsAndMetrics(eventHitBuilder);
				m_googleAnalyticsMPV3.LogEvent(eventHitBuilder);
			}
		}
	}

	public void LogTiming(string timingCategory, long timingInterval, string timingName, string timingLabel)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.TIMIMG))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				TimingHitBuilder builder = new TimingHitBuilder().SetTimingCategory(timingCategory).SetTimingInterval(timingInterval).SetTimingName(timingName)
					.SetTimingLabel(timingLabel);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogTiming(builder);
			}
		}
	}

	public void LogSocial(string socialNetwork, string socialAction, string socialTarget)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.SOCIAL))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				SocialHitBuilder builder = new SocialHitBuilder().SetSocialNetwork(socialNetwork).SetSocialAction(socialAction).SetSocialTarget(socialTarget);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogSocial(builder);
			}
		}
	}

	public void LogTransaction(string transID, string affiliation, double revenue, double tax, double shipping)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.SOCIAL))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				TransactionHitBuilder builder = new TransactionHitBuilder().SetTransactionID(transID).SetAffiliation(affiliation).SetRevenue(revenue)
					.SetTax(tax)
					.SetShipping(shipping)
					.SetCurrencyCode(string.Empty);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogTransaction(builder);
			}
		}
	}

	public void LogTransaction(string transID, string affiliation, double revenue, double tax, double shipping, string currencyCode)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.SOCIAL))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				TransactionHitBuilder builder = new TransactionHitBuilder().SetTransactionID(transID).SetAffiliation(affiliation).SetRevenue(revenue)
					.SetTax(tax)
					.SetShipping(shipping)
					.SetCurrencyCode(currencyCode);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogTransaction(builder);
			}
		}
	}

	public void LogItem(string transID, string name, string SKU, string category, double price, long quantity)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.ITEMS))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				ItemHitBuilder builder = new ItemHitBuilder().SetTransactionID(transID).SetName(name).SetSKU(SKU)
					.SetCategory(category)
					.SetPrice(price)
					.SetQuantity(quantity)
					.SetCurrencyCode(null);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogItem(builder);
			}
		}
	}

	public void LogItem(string transID, string name, string SKU, string category, double price, long quantity, string currencyCode)
	{
		if (m_googleAnalyticsV3Running && GAHelpers.TrackerActive(Tracker.ITEMS))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			if (m_googleAnalyticsV3 != null)
			{
				ItemHitBuilder builder = new ItemHitBuilder().SetTransactionID(transID).SetName(name).SetSKU(SKU)
					.SetCategory(category)
					.SetPrice(price)
					.SetQuantity(quantity)
					.SetCurrencyCode(currencyCode);
				AddDefaultCustomDimensionsAndMetrics(builder);
				m_googleAnalyticsV3.LogItem(builder);
			}
		}
	}

	public void LogGameAnalytics(GameAnalyticsBase gameAnalyticsBase)
	{
		if (!m_googleAnalyticsV3Running || !GAHelpers.TrackerActive(Tracker.GAMEANALYTICS))
		{
			return;
		}
		string value = gameAnalyticsBase.SerializeToJson();
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
		{
		}
		if (!(m_googleAnalyticsV3 != null) || m_googleAnalyticsMPV3 == null)
		{
			return;
		}
		EventHitBuilder eventHitBuilder = new EventHitBuilder().SetEventCategory(gameAnalyticsBase.eventCategory).SetEventAction(gameAnalyticsBase.eventAction).SetEventValue(1L)
			.SetCustomDimension(5, value);
		ReadOnlyCollection<KeyValuePair<int, string>> extraCustomDimensions = gameAnalyticsBase.GetExtraCustomDimensions();
		if (extraCustomDimensions != null)
		{
			IEnumerator<KeyValuePair<int, string>> enumerator = extraCustomDimensions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				eventHitBuilder.SetCustomDimension(enumerator.Current.Key, enumerator.Current.Value);
			}
		}
		m_googleAnalyticsMPV3.LogEvent(eventHitBuilder);
	}

	private void AddDefaultCustomDimensionsAndMetrics<T>(HitBuilder<T> builder)
	{
		IEnumerator<KeyValuePair<int, string>> enumerator = GAHelpers.DefaultCustomDimensions.GetEnumerator();
		while (enumerator.MoveNext())
		{
			builder.SetCustomDimension(enumerator.Current.Key, enumerator.Current.Value);
		}
		enumerator = GAHelpers.DefaultCustomMetrics.GetEnumerator();
		builder.SetCustomMetric(enumerator.Current.Key, enumerator.Current.Value);
	}

	public void InitialiseTracker()
	{
		m_googleAnalyticsV3 = UnityEngine.Object.FindObjectOfType<GoogleAnalyticsV3>();
		if (m_googleAnalyticsV3 == null)
		{
			Debug.LogWarningFormat("NetAnalytics:Start: FAILED : Failed to hook up GoogleAnalyticsV3 Plugin");
			return;
		}
		m_googleAnalyticsV3.logLevel = GoogleAnalyticsV3.DebugMode.ERROR;
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
		{
		}
		if (m_googleAnalyticsMPV3 == null)
		{
			m_googleAnalyticsMPV3 = new GoogleAnalyticsMPV3();
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			m_googleAnalyticsMPV3.SetTrackingCode(m_googleAnalyticsV3.otherTrackingCode);
			m_googleAnalyticsMPV3.SetBundleIdentifier(m_googleAnalyticsV3.bundleIdentifier);
			m_googleAnalyticsMPV3.SetAppName(m_googleAnalyticsV3.productName);
			string text = "0";
			string text2 = T17NetConfig.GetPlatformType();
			string versionString = BuildVersion.m_VersionString;
			if (!versionString.Contains("Mini"))
			{
				text = ((!versionString.Contains("*1*")) ? versionString : "100");
			}
			else
			{
				string text3 = versionString.Remove(0, 4);
				text = text3;
				text2 += ".Mini";
			}
			string text4 = text2 + "." + text;
			m_googleAnalyticsV3.bundleVersion = text4;
			m_googleAnalyticsMPV3.SetAppVersion(text4);
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			m_googleAnalyticsMPV3.SetAnonymizeIP(m_googleAnalyticsV3.anonymizeIP);
			m_googleAnalyticsMPV3.SetDryRun(m_googleAnalyticsV3.dryRun);
			m_googleAnalyticsMPV3.SetOptOut(optOut: false);
			m_googleAnalyticsMPV3.SetLogLevelValue(GoogleAnalyticsV3.DebugMode.ERROR);
			m_googleAnalyticsMPV3.InitializeTracker();
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetAnalytics))
			{
			}
			m_googleAnalyticsV3.gameObject.SetActive(value: true);
			m_googleAnalyticsV3Running = true;
		}
	}
}

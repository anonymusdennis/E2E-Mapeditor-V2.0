using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameAnalytics;

internal class GAHelpers
{
	public class ActiveTrackerHelper
	{
		public static BitArray ActiveTracker;

		static ActiveTrackerHelper()
		{
			ActiveTracker = new BitArray(4096, defaultValue: false);
			ActiveTracker.SetAll(value: false);
		}

		public static void ClearActiveTrackers()
		{
			ActiveTracker.SetAll(value: false);
		}
	}

	public class ActiveCategoriesHelper
	{
		public static BitArray ActiveCategories;

		static ActiveCategoriesHelper()
		{
			ActiveCategories = new BitArray(512, defaultValue: false);
			ActiveCategories.SetAll(value: false);
		}

		public static void ClearActiveCategories()
		{
			ActiveCategories.SetAll(value: false);
		}
	}

	private static Guid m_guid;

	private static string m_changesetNumberStr = string.Empty;

	private static int m_changesetNumber;

	private static int m_patchablesFileSetId = 0;

	private static ReadOnlyCollection<KeyValuePair<int, string>> m_defaultCustomDimensions;

	private static ReadOnlyCollection<KeyValuePair<int, string>> m_defaultCustomMetrics;

	public static string SessionID
	{
		get
		{
			if (m_guid == Guid.Empty)
			{
				m_guid = Guid.NewGuid();
			}
			return m_guid.ToString();
		}
	}

	public static int ChangesetNumber
	{
		get
		{
			if (string.IsNullOrEmpty(m_changesetNumberStr))
			{
				TextAsset textAsset = (TextAsset)Resources.Load("Changeset", typeof(TextAsset));
				if (textAsset != null)
				{
					m_changesetNumberStr = textAsset.ToString();
					int.TryParse(m_changesetNumberStr.Substring(3), out m_changesetNumber);
				}
				else
				{
					m_changesetNumberStr = "<unknown>";
				}
			}
			return m_changesetNumber;
		}
	}

	public static int PatchablesFileSetId
	{
		get
		{
			return m_patchablesFileSetId;
		}
		set
		{
			if (m_patchablesFileSetId != value)
			{
				m_patchablesFileSetId = value;
				m_defaultCustomDimensions = null;
				m_defaultCustomMetrics = null;
			}
		}
	}

	public static ReadOnlyCollection<KeyValuePair<int, string>> DefaultCustomDimensions
	{
		get
		{
			if (m_defaultCustomDimensions == null)
			{
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				list.Add(new KeyValuePair<int, string>(2, 1.ToString()));
				list.Add(new KeyValuePair<int, string>(3, "beta"));
				list.Add(new KeyValuePair<int, string>(4, ChangesetNumber.ToString()));
				list.Add(new KeyValuePair<int, string>(6, Application.platform.ToString()));
				list.Add(new KeyValuePair<int, string>(7, PatchablesFileSetId.ToString()));
				m_defaultCustomDimensions = list.AsReadOnly();
			}
			return m_defaultCustomDimensions;
		}
	}

	public static ReadOnlyCollection<KeyValuePair<int, string>> DefaultCustomMetrics
	{
		get
		{
			if (m_defaultCustomMetrics == null)
			{
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
				list.Add(new KeyValuePair<int, string>(1, 1.ToString()));
				list.Add(new KeyValuePair<int, string>(2, ChangesetNumber.ToString()));
				list.Add(new KeyValuePair<int, string>(3, PatchablesFileSetId.ToString()));
				m_defaultCustomMetrics = list.AsReadOnly();
			}
			return m_defaultCustomMetrics;
		}
	}

	public static bool TrackerActive(NetAnalytics.Tracker logTracker)
	{
		return ActiveTrackerHelper.ActiveTracker.Get((int)logTracker);
	}

	public static void TrackerActive(NetAnalytics.Tracker logTracker, bool active)
	{
		ActiveTrackerHelper.ActiveTracker.Set((int)logTracker, active);
	}

	public static bool CategoryActive(NetAnalytics.EventCategory category)
	{
		return ActiveCategoriesHelper.ActiveCategories.Get((int)category);
	}

	public static void CategoryActive(NetAnalytics.EventCategory category, bool active)
	{
		ActiveCategoriesHelper.ActiveCategories.Set((int)category, active);
	}
}

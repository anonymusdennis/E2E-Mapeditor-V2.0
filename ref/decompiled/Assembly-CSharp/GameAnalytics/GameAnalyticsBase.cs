using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using UnityEngine;

namespace GameAnalytics;

public abstract class GameAnalyticsBase
{
	public const int GameAnalyticsVersion = 1;

	public const string GameVersionLabel = "beta";

	public double timestampUnixUTC { get; set; }

	public string eventCategory { get; set; }

	public string eventAction { get; set; }

	public int analyticsVersion { get; set; }

	public string sessionID { get; set; }

	public int changeset { get; set; }

	public string gameVersion { get; set; }

	public string runtimePlatform { get; set; }

	public int patchablesFileSetId { get; set; }

	public GameAnalyticsBase(string EventCategory, string EventAction)
	{
		timestampUnixUTC = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		eventCategory = EventCategory;
		eventAction = EventAction + "_VERBOSE";
		analyticsVersion = 1;
		sessionID = GAHelpers.SessionID;
		changeset = GAHelpers.ChangesetNumber;
		gameVersion = "beta";
		runtimePlatform = Application.platform.ToString();
		patchablesFileSetId = GAHelpers.PatchablesFileSetId;
	}

	internal string SerializeToJson()
	{
		return JsonConvert.SerializeObject(this);
	}

	internal virtual ReadOnlyCollection<KeyValuePair<int, string>> GetExtraCustomDimensions()
	{
		return null;
	}
}

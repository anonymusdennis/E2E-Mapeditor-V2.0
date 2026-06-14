using System.Collections.Generic;

public static class EC2AnalyticsHelper
{
	private struct PlaytimeThresholds
	{
		public int m_MinutesNeeded;

		public string m_Description;
	}

	private static readonly PlaytimeThresholds[] m_Thresholds = new PlaytimeThresholds[41]
	{
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 3,
			m_Description = "3 Minutes"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 10,
			m_Description = "10 Minutes"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 30,
			m_Description = "30 Minutes"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 45,
			m_Description = "45 Minutes"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 60,
			m_Description = "1 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 120,
			m_Description = "2 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 180,
			m_Description = "3 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 240,
			m_Description = "4 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 300,
			m_Description = "5 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 360,
			m_Description = "6 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 480,
			m_Description = "8 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 600,
			m_Description = "10 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 900,
			m_Description = "15 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 1200,
			m_Description = "20 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 1500,
			m_Description = "25 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 1800,
			m_Description = "30 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 2100,
			m_Description = "35 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 2400,
			m_Description = "40 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 2700,
			m_Description = "45 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 3000,
			m_Description = "50 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 3600,
			m_Description = "60 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 4200,
			m_Description = "70 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 4800,
			m_Description = "80 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 5400,
			m_Description = "90 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 6000,
			m_Description = "100 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 7800,
			m_Description = "130 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 9600,
			m_Description = "160 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 11400,
			m_Description = "190 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 13200,
			m_Description = "220 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 15000,
			m_Description = "250 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 18000,
			m_Description = "300 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 21000,
			m_Description = "350 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 24000,
			m_Description = "400 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 27000,
			m_Description = "450 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 30000,
			m_Description = "500 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 33000,
			m_Description = "550 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 36000,
			m_Description = "600 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 42000,
			m_Description = "700 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 48000,
			m_Description = "800 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 54000,
			m_Description = "900 Hour"
		},
		new PlaytimeThresholds
		{
			m_MinutesNeeded = 60000,
			m_Description = "1000+ Hour"
		}
	};

	private const string LAST_UPLOADED_SUFFIX = "_LastUploded";

	public static void UpdatePlaytimeRecord(int additionalMinutesPlayed, string saveKey, string analyticsCategory, string analyticsActionPrefix)
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		instance.Get(saveKey, out var value, 0);
		value += additionalMinutesPlayed;
		instance.Set(saveKey, value);
		instance.Get(saveKey + "_LastUploded", out var value2, 0);
		if (value >= m_Thresholds[0].m_MinutesNeeded)
		{
			PlaytimeThresholds playtimeThresholds = m_Thresholds[0];
			for (int i = 0; i < m_Thresholds.Length && value >= m_Thresholds[i].m_MinutesNeeded; i++)
			{
				playtimeThresholds = m_Thresholds[i];
			}
			if (value2 != playtimeThresholds.m_MinutesNeeded)
			{
				instance.Set(saveKey + "_LastUploded", playtimeThresholds.m_MinutesNeeded);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent(analyticsCategory, analyticsActionPrefix + playtimeThresholds.m_Description, string.Empty, 0L);
			}
		}
	}

	public static void UpdateNumberOfSubscribedPrisons(List<Platform.UGCItem> currentSubscribedItems)
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		instance.Get("Analytics_SubscribedPrisonIds", out var value, new long[0]);
		List<long> list = new List<long>();
		for (int i = 0; i < value.Length; i++)
		{
			list.Add(value[i]);
		}
		for (int num = currentSubscribedItems.Count - 1; num >= 0; num--)
		{
			if (currentSubscribedItems[num].m_Type == Platform.UGCType.eCustomLevel)
			{
				long iD = (long)currentSubscribedItems[num].m_ID;
				if (!list.Contains(iD))
				{
					list.Add(iD);
				}
			}
		}
		if (value.Length < list.Count)
		{
			int count = list.Count;
			for (int j = value.Length; j < count; j++)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Custom Prison Subscribed Count", "Custom Prisons Subscribed - " + (j + 1), string.Empty, 0L);
			}
			instance.Set("Analytics_SubscribedPrisonIds", list.ToArray());
		}
	}
}

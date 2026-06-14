using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaylistData", menuName = "Team17/PrisonData and Playlists/Create Playlist")]
public class PlaylistData : ScriptableObject
{
	[Serializable]
	public class PrisonSetup
	{
		public PrisonData m_PrisonData;

		public int m_ConfigIndex;

		public PrisonSetup(PrisonData prison, int config)
		{
			m_PrisonData = prison;
			m_ConfigIndex = config;
		}
	}

	[Serializable]
	public class NetPlaylistData
	{
		public string m_GUID;

		public List<NetPrisonSetup> m_PrisonSetups;

		public NetPlaylistData(PlaylistData pd)
		{
			m_GUID = pd.m_GUID;
			m_PrisonSetups = pd.StripMapRotationsForNet();
		}

		public NetPrisonSetup GetPrison(int index)
		{
			return m_PrisonSetups[index];
		}
	}

	[Serializable]
	public class NetPrisonSetup
	{
		public PrisonData.LevelInfo m_PrisonInfo;

		public int m_ConfigIndex;

		public NetPrisonSetup(PrisonSetup ps)
		{
			m_PrisonInfo = ps.m_PrisonData.m_LevelInfo;
			m_ConfigIndex = ps.m_ConfigIndex;
		}
	}

	[Localization]
	public string m_NameLocalisationKey;

	[Localization]
	public string m_DescriptionLocalisationKey;

	public string m_ImagePath;

	public string m_ImageLockedPath;

	public List<PrisonSetup> m_Prisons;

	public string m_GUID;

	public bool m_bIsDebug;

	public ProgressMilestone m_UnlockMilestone;

	public PlaylistData()
	{
		m_Prisons = new List<PrisonSetup>();
	}

	public PrisonData GetPrisonDataForLevelInfo(PrisonData.LevelInfo levelInfo)
	{
		for (int num = m_Prisons.Count - 1; num >= 0; num--)
		{
			if (m_Prisons[num].m_PrisonData.m_LevelInfo.InfoMatches(levelInfo))
			{
				return m_Prisons[num].m_PrisonData;
			}
		}
		return null;
	}

	public NetPlaylistData StripForNet()
	{
		return new NetPlaylistData(this);
	}

	public List<NetPrisonSetup> StripMapRotationsForNet()
	{
		List<NetPrisonSetup> list = new List<NetPrisonSetup>();
		for (int i = 0; i < m_Prisons.Count; i++)
		{
			list.Add(new NetPrisonSetup(m_Prisons[i]));
		}
		return list;
	}
}

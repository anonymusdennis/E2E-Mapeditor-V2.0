using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class LevelDataManager : MonoBehaviour
{
	public enum PlaylistTypes
	{
		Campaign,
		Versus,
		External
	}

	private static LevelDataManager m_Instance;

	private PrisonData[] m_T17Levels;

	private List<PlaylistData> m_CampaignPlaylists;

	private List<PlaylistData> m_VersusPlaylists;

	private List<PlaylistData> m_CustomPlaylists = new List<PlaylistData>();

	private LevelScript.PRISON_ENUM m_ForceFirstPrison = LevelScript.PRISON_ENUM.POW_Camp;

	public LevelScript.PRISON_ENUM[] m_Order;

	public PrisonConfig[] m_CustomPrisonConfigs = new PrisonConfig[3];

	private T17NetView m_NetView;

	public static LevelDataManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		if (m_Instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
			m_Instance = this;
		}
		m_NetView = GetComponent<T17NetView>();
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	public void LoadData()
	{
		Platform instance = Platform.GetInstance();
		instance.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Remove(instance.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(RefreshCampaignPlaylistsforDLC));
		Platform instance2 = Platform.GetInstance();
		instance2.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Combine(instance2.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(RefreshCampaignPlaylistsforDLC));
		m_T17Levels = Resources.LoadAll<PrisonData>("Prefabs/PrisonLevelData");
		if (m_T17Levels == null || m_T17Levels.Length == 0)
		{
		}
		m_CampaignPlaylists = new List<PlaylistData>();
		for (int i = 0; i < m_T17Levels.Length; i++)
		{
			PrisonData prisonData = m_T17Levels[i];
			if (prisonData.m_bIsDebug || (m_T17Levels[i].m_bIsDLC && (!(m_T17Levels[i].m_DLCData != null) || !Platform.GetInstance().IsDLCAvailable(m_T17Levels[i].m_DLCData))))
			{
				continue;
			}
			int num = -1;
			if (prisonData.m_Configs != null && prisonData.m_Configs.Count > 0)
			{
				PrisonConfig.ConfigType type = PrisonConfig.ConfigType.Cooperative;
				switch (prisonData.m_LevelInfo.m_PrisonType)
				{
				case LevelScript.PRISON_TYPE.Normal:
				case LevelScript.PRISON_TYPE.Transport:
					type = PrisonConfig.ConfigType.Cooperative;
					break;
				case LevelScript.PRISON_TYPE.Tutorial:
					type = PrisonConfig.ConfigType.Singleplayer;
					break;
				}
				num = prisonData.m_Configs.FindIndex((PrisonConfig x) => x.m_ConfigType == type);
			}
			if (num >= 0)
			{
				PlaylistData playlistData = ScriptableObject.CreateInstance(typeof(PlaylistData)) as PlaylistData;
				playlistData.m_DescriptionLocalisationKey = prisonData.m_DescriptionLocalizationKey;
				playlistData.m_NameLocalisationKey = prisonData.m_NameLocalizationKey;
				playlistData.m_ImagePath = prisonData.m_ImagePath;
				playlistData.m_Prisons.Add(new PlaylistData.PrisonSetup(prisonData, num));
				playlistData.m_GUID = "T17_Default_Campaign_" + playlistData.m_NameLocalisationKey;
				playlistData.m_ImageLockedPath = prisonData.m_ImageLockedPath;
				playlistData.m_UnlockMilestone = prisonData.m_UnlockMilestone;
				m_CampaignPlaylists.Add(playlistData);
			}
			if (m_T17Levels[i].m_bIsDLC && Platform.GetInstance().IsDLCAvailable(m_T17Levels[i].m_DLCData) && UnlockManager.GetInstance() != null && m_T17Levels[i].m_UnlockRewards.count > 0)
			{
				UnlockManager.GetInstance().UnlockCustomisationSet(m_T17Levels[i].m_UnlockRewards);
			}
		}
		m_CampaignPlaylists.Sort(SortCampaignPlaylist);
		PlaylistData[] array = Resources.LoadAll<PlaylistData>("Prefabs/VersusPlaylists");
		if (array == null || array.Length == 0)
		{
		}
		m_VersusPlaylists = new List<PlaylistData>();
		if (array == null)
		{
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j].m_bIsDebug)
			{
				m_VersusPlaylists.Add(array[j]);
			}
		}
	}

	public bool SetUpOrderSize()
	{
		int length = Enum.GetValues(typeof(LevelScript.PRISON_ENUM)).Length;
		if (m_Order == null || m_Order.Length != length)
		{
			if (m_Order == null)
			{
				m_Order = new LevelScript.PRISON_ENUM[length];
			}
			else
			{
				LevelScript.PRISON_ENUM[] array = new LevelScript.PRISON_ENUM[m_Order.Length];
				for (int i = 0; i < m_Order.Length; i++)
				{
					array[i] = m_Order[i];
				}
				int num = ((length >= m_Order.Length) ? m_Order.Length : length);
				m_Order = new LevelScript.PRISON_ENUM[length];
				for (int i = 0; i < num; i++)
				{
					m_Order[i] = array[i];
				}
			}
			return true;
		}
		return false;
	}

	public bool IsDLCLevel(LevelScript.PRISON_ENUM prisonEnum)
	{
		bool flag = false;
		PrisonData prisonDataForPrison = GetPrisonDataForPrison(prisonEnum);
		return !(prisonDataForPrison == null) && prisonDataForPrison.m_bIsDLC;
	}

	public bool IsLevelAvailable(LevelScript.PRISON_ENUM prisonEnum)
	{
		if (prisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
		{
			return true;
		}
		PrisonData prisonDataForPrison = GetPrisonDataForPrison(prisonEnum);
		Platform instance = Platform.GetInstance();
		if (prisonDataForPrison != null && instance != null)
		{
			return !prisonDataForPrison.m_bIsDLC || (prisonDataForPrison.m_DLCData != null && instance.IsDLCAvailable(prisonDataForPrison.m_DLCData));
		}
		return false;
	}

	public void AddCustomLevelPlaylist(PlaylistData customPlaylist)
	{
		if (customPlaylist != null)
		{
			m_CustomPlaylists.Add(customPlaylist);
		}
	}

	public void ClearCustomLevelPlaylists()
	{
		m_CustomPlaylists.Clear();
	}

	public void NotifyOthersToSelectPlaylistRPC(PlaylistData playlist)
	{
		if (!T17NetManager.OfflineMode && T17NetRoomManager.IsInRoom())
		{
			GetPlaylistRPCInformation(playlist, out var type, out var index);
			m_NetView.RPC("RPC_LoadPlaylistLocally", NetTargets.Others, type, index);
		}
	}

	public void NotifyClientToSelectPlaylistRPC(PhotonPlayer reciever, PlaylistData playlist)
	{
		GetPlaylistRPCInformation(playlist, out var type, out var index);
		m_NetView.RPC("RPC_LoadPlaylistLocally", reciever, type, index);
	}

	private void GetPlaylistRPCInformation(PlaylistData playlist, out PlaylistTypes type, out int index)
	{
		type = PlaylistTypes.Campaign;
		index = m_CampaignPlaylists.IndexOf(playlist);
		if (index != -1)
		{
			type = PlaylistTypes.Campaign;
		}
		else
		{
			type = PlaylistTypes.Versus;
			index = m_VersusPlaylists.IndexOf(playlist);
		}
		if (index == -1)
		{
			type = PlaylistTypes.External;
		}
	}

	[PunRPC]
	private void RPC_LoadPlaylistLocally(PlaylistTypes playlistType, int index)
	{
		PlaylistData playlistData = null;
		switch (playlistType)
		{
		case PlaylistTypes.Campaign:
			if (index < m_CampaignPlaylists.Count)
			{
				playlistData = m_CampaignPlaylists[index];
			}
			break;
		case PlaylistTypes.Versus:
			if (index < m_VersusPlaylists.Count)
			{
				playlistData = m_VersusPlaylists[index];
			}
			break;
		}
		if (playlistData != null)
		{
			GlobalStart.GetInstance().SetSelectedPlaylist(playlistData);
			GlobalStart.GetInstance().SetSelectedLevelToNetRoomCurrent();
		}
	}

	public PrisonData[] GetT17LevelData()
	{
		return m_T17Levels;
	}

	public List<PlaylistData> GetCampaignPlaylists()
	{
		return m_CampaignPlaylists;
	}

	public List<PlaylistData> GetVersusPlaylists()
	{
		return m_VersusPlaylists;
	}

	public PrisonData GetPrisonDataForPrison(LevelScript.PRISON_ENUM prisonEnum)
	{
		switch (prisonEnum)
		{
		case LevelScript.PRISON_ENUM.Unassigned:
			return null;
		case LevelScript.PRISON_ENUM.CustomPrison:
			return null;
		default:
		{
			for (int i = 0; i < m_T17Levels.Length; i++)
			{
				if (m_T17Levels[i].m_LevelInfo.m_PrisonEnum == prisonEnum)
				{
					return m_T17Levels[i];
				}
			}
			return null;
		}
		}
	}

	public PlaylistData GetCampaignForPrison(LevelScript.PRISON_ENUM prisonEnum)
	{
		for (int i = 0; i < m_CampaignPlaylists.Count; i++)
		{
			PlaylistData.PrisonSetup prisonSetup = m_CampaignPlaylists[i].m_Prisons[0];
			if (prisonSetup.m_PrisonData.m_LevelInfo.m_PrisonEnum == prisonEnum)
			{
				return m_CampaignPlaylists[i];
			}
		}
		return null;
	}

	public void RefreshCampaignPlaylistsforDLC()
	{
		for (int i = 0; i < m_T17Levels.Length; i++)
		{
			PrisonData loopedPrison = m_T17Levels[i];
			if (loopedPrison.m_bIsDebug || !m_T17Levels[i].m_bIsDLC)
			{
				continue;
			}
			int num = -1;
			if (loopedPrison.m_Configs != null && loopedPrison.m_Configs.Count > 0)
			{
				PrisonConfig.ConfigType type = PrisonConfig.ConfigType.Cooperative;
				switch (loopedPrison.m_LevelInfo.m_PrisonType)
				{
				case LevelScript.PRISON_TYPE.Normal:
				case LevelScript.PRISON_TYPE.Transport:
					type = PrisonConfig.ConfigType.Cooperative;
					break;
				case LevelScript.PRISON_TYPE.Tutorial:
					type = PrisonConfig.ConfigType.Singleplayer;
					break;
				}
				num = loopedPrison.m_Configs.FindIndex((PrisonConfig x) => x.m_ConfigType == type);
			}
			if (num >= 0)
			{
				bool flag = m_T17Levels[i].m_DLCData != null && Platform.GetInstance().IsDLCAvailable(m_T17Levels[i].m_DLCData);
				bool flag2 = m_CampaignPlaylists.Exists((PlaylistData pl) => pl.m_GUID == "T17_Default_Campaign_" + loopedPrison.m_NameLocalizationKey);
				if (!flag2 && flag)
				{
					PlaylistData playlistData = ScriptableObject.CreateInstance(typeof(PlaylistData)) as PlaylistData;
					playlistData.m_DescriptionLocalisationKey = loopedPrison.m_DescriptionLocalizationKey;
					playlistData.m_NameLocalisationKey = loopedPrison.m_NameLocalizationKey;
					playlistData.m_ImagePath = loopedPrison.m_ImagePath;
					playlistData.m_Prisons.Add(new PlaylistData.PrisonSetup(loopedPrison, num));
					playlistData.m_GUID = "T17_Default_Campaign_" + playlistData.m_NameLocalisationKey;
					playlistData.m_ImageLockedPath = loopedPrison.m_ImageLockedPath;
					playlistData.m_UnlockMilestone = loopedPrison.m_UnlockMilestone;
					m_CampaignPlaylists.Add(playlistData);
				}
				else if (flag2 && !flag)
				{
					for (int num2 = m_CampaignPlaylists.Count - 1; num2 >= 0; num2--)
					{
						if (m_CampaignPlaylists[num2].m_GUID == "T17_Default_Campaign_" + loopedPrison.m_NameLocalizationKey)
						{
							m_CampaignPlaylists.RemoveAt(num2);
							break;
						}
					}
				}
			}
			else if (num > 0)
			{
			}
			if (m_T17Levels[i].m_bIsDLC && Platform.GetInstance().IsDLCAvailable(m_T17Levels[i].m_DLCData) && UnlockManager.GetInstance() != null && m_T17Levels[i].m_UnlockRewards.count > 0)
			{
				UnlockManager.GetInstance().UnlockCustomisationSet(m_T17Levels[i].m_UnlockRewards);
			}
		}
		m_CampaignPlaylists.Sort(SortCampaignPlaylist);
	}

	private int SortCampaignPlaylist(PlaylistData a, PlaylistData b)
	{
		if (m_Order != null)
		{
			int num = 0;
			int num2 = m_Order.Length;
			int num3 = m_Order.Length;
			for (num = 0; num < m_Order.Length; num++)
			{
				if (a.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == m_Order[num])
				{
					num2 = num;
				}
				if (b.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == m_Order[num])
				{
					num3 = num;
				}
			}
			if (num2 == m_Order.Length)
			{
				if (num3 == m_Order.Length)
				{
					return 0;
				}
				return 1;
			}
			if (num3 == m_Order.Length)
			{
				return -1;
			}
			if (num2 < num3)
			{
				return -1;
			}
			return 1;
		}
		if (m_ForceFirstPrison != LevelScript.PRISON_ENUM.CustomPrison)
		{
			if (a.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == m_ForceFirstPrison)
			{
				return -1;
			}
			if (b.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == m_ForceFirstPrison)
			{
				return 1;
			}
		}
		return a.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum.CompareTo(b.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum);
	}
}

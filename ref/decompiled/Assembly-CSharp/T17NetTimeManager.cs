using System;
using ExitGames.Client.Photon;
using Photon;

public class T17NetTimeManager : MonoBehaviour
{
	public enum ServerTimeMode
	{
		LocalTimeMode,
		MasterServerTimeMode,
		GameServerTimeMode,
		UndefinedTimeMode
	}

	private string m_timeStampPattern = "HH:mm:ss.fff";

	private string m_briefTimeStampPattern = "mm:ss.fff";

	private DateTime m_serverDateTime = default(DateTime);

	private DateTime m_localDateTime = default(DateTime);

	private uint m_servertimeinms;

	private bool m_bMasterServerActive;

	private bool m_bLobbyServerActive;

	private bool m_bRoomServerActive;

	private const string m_tmModeLocal = "LC";

	private const string m_tmModeMasterServer = "MS";

	private const string m_tmModeGameServer = "GS";

	private const string m_tmModeUndefined = "UD";

	private static T17NetTimeManager m_instance;

	public static T17NetTimeManager Instance => m_instance;

	public bool LobbyServerActive
	{
		get
		{
			return m_bLobbyServerActive;
		}
		set
		{
			m_bLobbyServerActive = value;
		}
	}

	public bool MasterServerActive
	{
		get
		{
			return m_bMasterServerActive;
		}
		set
		{
			m_bMasterServerActive = value;
		}
	}

	public bool RoomServerActive
	{
		get
		{
			return m_bRoomServerActive;
		}
		set
		{
			m_bRoomServerActive = value;
		}
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		m_instance = this;
		m_serverDateTime = DateTime.Now;
		m_localDateTime = DateTime.Now;
	}

	private void Update()
	{
		m_localDateTime = new DateTime((uint)(SupportClass.GetTickCount() * 1000));
		m_servertimeinms = (uint)PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds;
		if (m_servertimeinms == 0)
		{
			m_serverDateTime = new DateTime((uint)PhotonNetwork.time * 10000000);
			return;
		}
		string serverTimeMode = GetServerTimeMode();
		if (serverTimeMode == "MS" || serverTimeMode == "GS")
		{
			m_serverDateTime = new DateTime(m_servertimeinms * 10000);
		}
		else
		{
			m_serverDateTime = new DateTime(m_servertimeinms * 1000);
		}
	}

	public uint GetServerTimeInMS()
	{
		return m_servertimeinms;
	}

	public string GetBriefimeStamp()
	{
		return m_serverDateTime.ToString(m_briefTimeStampPattern);
	}

	public string GetBriefServerTimeStamp()
	{
		return m_serverDateTime.ToString(m_briefTimeStampPattern);
	}

	public string GetServerTimeStamp()
	{
		return m_serverDateTime.ToString(m_timeStampPattern);
	}

	public long GetLocalTime()
	{
		return m_localDateTime.ToBinary();
	}

	public string GetLocalTimeStamp()
	{
		return m_localDateTime.ToString(m_timeStampPattern);
	}

	public string GetLocalTimeBriefStamp()
	{
		return m_localDateTime.ToString(m_briefTimeStampPattern);
	}

	public uint GetLocalTimeInMS()
	{
		return (uint)SupportClass.GetTickCount();
	}

	public string GetLocalMachineTimeStamp()
	{
		return DateTime.Now.ToString(m_timeStampPattern);
	}

	public string GetServerTimeMode()
	{
		string empty = string.Empty;
		if (!MasterServerActive && !LobbyServerActive && !RoomServerActive)
		{
			return "LC";
		}
		if (MasterServerActive || LobbyServerActive)
		{
			return "MS";
		}
		if (RoomServerActive)
		{
			return "GS";
		}
		return "UD";
	}

	public string GetTimeModeString(ServerTimeMode serverTimeMode)
	{
		return serverTimeMode switch
		{
			ServerTimeMode.LocalTimeMode => "LC", 
			ServerTimeMode.MasterServerTimeMode => "MS", 
			ServerTimeMode.GameServerTimeMode => "GS", 
			ServerTimeMode.UndefinedTimeMode => "UD", 
			_ => "UD", 
		};
	}
}

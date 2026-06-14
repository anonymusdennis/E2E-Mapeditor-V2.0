using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class T17NetMatchmakingAgent : T17NetAgentBase
{
	public delegate void OnComplete(bool bSuccess);

	private NetMatchmakingConfig m_Config;

	private string m_strLobbyName;

	private byte m_iMaxPlayers;

	private List<string>.Enumerator m_CurrentSearch;

	private List<int>.Enumerator m_CurrentSeachTimeoutMillis;

	private Stopwatch m_Timer = new Stopwatch();

	private T17NetJoinFilterRoomAgent m_JoinFilterAgent = new T17NetJoinFilterRoomAgent();

	private static event OnComplete OnCompleted;

	public bool Start()
	{
		if (null != m_Config && m_Config.m_strGameSearches != null && m_Config.m_iGameSearchDurations != null && T17NetMatchmakingAgent.OnCompleted != null && m_Config.m_strGameSearches.Count == m_Config.m_iGameSearchDurations.Count)
		{
			m_CurrentSearch = m_Config.m_strGameSearches.GetEnumerator();
			m_CurrentSeachTimeoutMillis = m_Config.m_iGameSearchDurations.GetEnumerator();
			return StartNextSearch();
		}
		return false;
	}

	private bool StartNextSearch()
	{
		if (m_CurrentSearch.MoveNext() && m_CurrentSeachTimeoutMillis.MoveNext())
		{
			string strSQLGameSearch = m_Config.m_strSearchPrefix + m_CurrentSearch.Current;
			m_JoinFilterAgent.SetSearchParameters(m_strLobbyName, strSQLGameSearch, m_iMaxPlayers);
			m_Timer.Reset();
			m_Timer.Start();
			return m_JoinFilterAgent.Start();
		}
		return false;
	}

	private void Stop()
	{
		m_Timer.Stop();
	}

	public void SetMatchmakingConfig(string lobbyName, NetMatchmakingConfig config, byte maxPlayers, OnComplete completeCallback)
	{
		m_strLobbyName = lobbyName;
		m_Config = config;
		m_iMaxPlayers = maxPlayers;
		T17NetMatchmakingAgent.OnCompleted = completeCallback;
	}

	public void OnConnectedToMaster()
	{
		m_JoinFilterAgent.OnConnectedToMaster();
	}

	public void OnLeftRoom()
	{
		m_JoinFilterAgent.OnLeftRoom();
	}

	public void OnJoinedRoomFailed()
	{
		if (m_Timer.ElapsedMilliseconds > m_CurrentSeachTimeoutMillis.Current && m_CurrentSeachTimeoutMillis.Current >= 0)
		{
			if (!StartNextSearch())
			{
				Stop();
				if (T17NetMatchmakingAgent.OnCompleted != null)
				{
					T17NetMatchmakingAgent.OnCompleted(bSuccess: false);
				}
			}
		}
		else
		{
			m_JoinFilterAgent.OnJoinedRoomFailed();
		}
	}

	public void OnDisconnected()
	{
		m_JoinFilterAgent.OnDisconnected();
	}

	public void OnJoinedRoom()
	{
		UnityEngine.Debug.Log("  ++++++++++    OnJoinedRoom    ++++++=");
		Gamer.DeleteRemoteGamers();
		Stop();
		if (T17NetMatchmakingAgent.OnCompleted != null)
		{
			T17NetMatchmakingAgent.OnCompleted(bSuccess: true);
		}
	}

	public void OnCreateRoomFailed()
	{
		m_JoinFilterAgent.OnCreateRoomFailed();
	}

	public virtual void OnPlatformReadyToConnect()
	{
	}

	public virtual void Update()
	{
	}
}

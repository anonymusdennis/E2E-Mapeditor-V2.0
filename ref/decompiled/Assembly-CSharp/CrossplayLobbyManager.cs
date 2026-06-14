using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossplayLobbyManager : MonoBehaviour
{
	public enum SupportedPlatforms
	{
		Steam,
		GOG,
		Origin,
		EpicGames,
		Count
	}

	private enum LobbyState
	{
		None,
		Creating,
		Created
	}

	public static readonly SupportedPlatforms m_MyPlatform;

	private int m_MyPlatformIndex;

	private List<PhotonPlayer>[] m_PhotonPlayers = new List<PhotonPlayer>[4];

	private LobbyState[] m_LobbyStates = new LobbyState[4];

	private T17NetView m_NetView;

	private Room m_PhotonRoom;

	private int m_CurrentPlayerCount;

	private IEnumerator Start()
	{
		int i = 0;
		for (int num = 4; i < num; i++)
		{
			m_PhotonPlayers[i] = new List<PhotonPlayer>();
			m_LobbyStates[i] = LobbyState.None;
		}
		m_PhotonPlayers[m_MyPlatformIndex].Add(PhotonNetwork.player);
		while (base.gameObject.GetComponent<PhotonView>() == null)
		{
			yield return null;
		}
		m_NetView = base.gameObject.GetComponent<T17NetView>();
	}

	private void Update()
	{
		m_PhotonRoom = PhotonNetwork.room;
		if (!T17NetManager.IsMasterClient || !T17NetManager.IsConnectedOnline() || m_PhotonRoom == null || m_PhotonRoom.PlayerCount == m_CurrentPlayerCount)
		{
			return;
		}
		if (m_PhotonRoom.PlayerCount < m_CurrentPlayerCount)
		{
			PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
			int j = 0;
			for (int num = m_PhotonPlayers.Length; j < num; j++)
			{
				List<PhotonPlayer> players = m_PhotonPlayers[j];
				for (int i = players.Count - 1; i >= 0; i--)
				{
					if (players[i] != PhotonNetwork.player && otherPlayers.FindIndex((PhotonPlayer x) => x != null && x == players[i]) == -1)
					{
						players.Remove(players[i]);
					}
				}
				m_PhotonPlayers[j] = players;
				m_NetView.RPC("RPC_UpdateLobbyMembers", NetTargets.Others, players.ToArray(), (SupportedPlatforms)j);
				if (players.Count == 0)
				{
					m_NetView.RPC("RPC_UpdateLobbyState", NetTargets.All, LobbyState.None, (SupportedPlatforms)j);
				}
			}
		}
		m_CurrentPlayerCount = m_PhotonRoom.PlayerCount;
	}

	public void RegisterLobbyMemberRPC()
	{
		m_NetView.RPC("RPC_RegisterLobbyMember", NetTargets.MasterClient, m_MyPlatform);
	}

	[PunRPC]
	private void RPC_RegisterLobbyMember(SupportedPlatforms platform, PhotonMessageInfo info)
	{
		if (!T17NetManager.IsMasterClient || m_PhotonPlayers[(int)platform].Contains(info.sender))
		{
			return;
		}
		if (m_PhotonPlayers[(int)platform].Count == 0)
		{
			m_NetView.RPC("RPC_MakePlatformLobby", info.sender);
			m_NetView.RPC("RPC_UpdateLobbyState", NetTargets.All, LobbyState.Creating, platform);
		}
		int i = 0;
		for (int num = 4; i < num; i++)
		{
			List<PhotonPlayer> list = m_PhotonPlayers[i];
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				if (list[j] != PhotonNetwork.player)
				{
					m_NetView.RPC("RPC_AddLobbyMember", list[j], info.sender, platform);
				}
			}
		}
		m_PhotonPlayers[(int)platform].Add(info.sender);
		SetPlatformForGamer(info.sender.ID, platform);
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		List<SupportedPlatforms> list3 = new List<SupportedPlatforms>();
		int k = 0;
		for (int num2 = 4; k < num2; k++)
		{
			int count2 = list2.Count;
			list2.AddRange(m_PhotonPlayers[k]);
			int l = count2;
			for (int count3 = list2.Count; l < count3; l++)
			{
				list3.Add((SupportedPlatforms)k);
			}
		}
		m_NetView.RPC("RPC_SetLobbyMembers", info.sender, list2.ToArray(), list3.ToArray(), m_LobbyStates);
	}

	[PunRPC]
	private void RPC_SetLobbyMembers(PhotonPlayer[] players, SupportedPlatforms[] platforms, LobbyState[] lobbyStates, PhotonMessageInfo info)
	{
		if (players.Length != platforms.Length)
		{
			Debug.LogError("CrossplayLobbyManager: Bad call to RPC_UpdateLobbyMembers! Players and platforms did not match lengths!");
		}
		int i = 0;
		for (int num = m_PhotonPlayers.Length; i < num; i++)
		{
			m_PhotonPlayers[i].Clear();
		}
		int j = 0;
		for (int num2 = players.Length; j < num2; j++)
		{
			m_PhotonPlayers[(int)platforms[j]].Add(players[j]);
			SetPlatformForGamer(players[j].ID, platforms[j]);
		}
		m_LobbyStates = lobbyStates;
		m_CurrentPlayerCount = m_PhotonRoom.PlayerCount;
	}

	[PunRPC]
	private void RPC_UpdateLobbyMembers(PhotonPlayer[] players, SupportedPlatforms platform, PhotonMessageInfo info)
	{
		m_PhotonPlayers[(int)platform].Clear();
		m_PhotonPlayers[(int)platform].AddRange(players);
		m_CurrentPlayerCount = m_PhotonRoom.PlayerCount;
		int i = 0;
		for (int num = players.Length; i < num; i++)
		{
			SetPlatformForGamer(players[i].ID, platform);
		}
	}

	[PunRPC]
	private void RPC_AddLobbyMember(PhotonPlayer player, SupportedPlatforms platform, PhotonMessageInfo info)
	{
		if (m_PhotonPlayers[(int)platform].Contains(player))
		{
			Debug.LogError("Sending duplicate player! Something must have gone wrong (Add)");
			return;
		}
		m_PhotonPlayers[(int)platform].Add(player);
		SetPlatformForGamer(player.ID, platform);
		m_CurrentPlayerCount = m_PhotonRoom.PlayerCount;
	}

	[PunRPC]
	private void RPC_RemoveLobbyMemeber(PhotonPlayer player, SupportedPlatforms platform, PhotonMessageInfo info)
	{
		if (!m_PhotonPlayers[(int)platform].Contains(player))
		{
			Debug.LogError("Sending duplicate player! Something must have gone wrong (Remove)");
			return;
		}
		m_PhotonPlayers[(int)platform].Remove(player);
		m_CurrentPlayerCount = m_PhotonRoom.PlayerCount;
	}

	[PunRPC]
	private void RPC_UpdateLobbyState(LobbyState state, SupportedPlatforms platform, PhotonMessageInfo info)
	{
		m_LobbyStates[(int)platform] = state;
	}

	[PunRPC]
	private void RPC_MakePlatformLobby(PhotonMessageInfo info)
	{
		Platform.GetInstance().MakePlatformLobby();
	}

	public void OnPlatformLobbyMade(bool bSucceeded)
	{
		m_NetView.RPC("RPC_PlatformLobbyMade", NetTargets.MasterClient, m_MyPlatform, bSucceeded);
	}

	[PunRPC]
	private void RPC_PlatformLobbyMade(SupportedPlatforms platform, bool bSucceeded, PhotonMessageInfo info)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_LobbyStates[(int)platform] == LobbyState.Created)
		{
			Debug.LogError("CrossplayLobbyManager: We received multiple lobby creates for one platform!");
			return;
		}
		if (!bSucceeded)
		{
			if (info.sender != PhotonNetwork.player)
			{
				List<PhotonPlayer> list = m_PhotonPlayers[(int)platform];
				if (list.Count > 0 && list[0] != null)
				{
					m_NetView.RPC("RPC_MakePlatformLobby", list[0]);
					return;
				}
				m_NetView.RPC("RPC_UpdateLobbyState", NetTargets.All, LobbyState.None, platform);
			}
			return;
		}
		if (info.sender == PhotonNetwork.player && !m_PhotonPlayers[(int)platform].Contains(PhotonNetwork.player))
		{
			m_PhotonPlayers[(int)platform].Add(PhotonNetwork.player);
			SetPlatformForGamer(info.sender.ID, platform);
		}
		m_NetView.RPC("RPC_UpdateLobbyState", NetTargets.All, LobbyState.Created, platform);
		List<PhotonPlayer> list2 = m_PhotonPlayers[(int)platform];
		int i = 0;
		for (int count = list2.Count; i < count; i++)
		{
			if (list2[i] != info.sender)
			{
				m_NetView.RPC("RPC_JoinPlatformLobby", list2[i]);
			}
		}
	}

	[PunRPC]
	private void RPC_JoinPlatformLobby(PhotonMessageInfo info)
	{
		Platform.GetInstance().JoinPlatformLobby();
	}

	private void SetPlatformForGamer(int photonID, SupportedPlatforms platform)
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		int num = allGamers.FindIndex((Gamer x) => x != null && x.m_PhotonID == photonID);
		if (num != -1)
		{
			allGamers[num].m_MyPlatform = platform;
		}
	}

	public bool IsWaitingForPlatformLobby()
	{
		return m_LobbyStates[m_MyPlatformIndex] != LobbyState.Created;
	}

	public void OnSessionLeft()
	{
		int i = 0;
		for (int num = 4; i < num; i++)
		{
			m_PhotonPlayers[i].Clear();
			m_LobbyStates[i] = LobbyState.None;
		}
		m_PhotonPlayers[m_MyPlatformIndex].Add(PhotonNetwork.player);
	}

	public bool IsGamerForMyPlatform(Gamer gamer)
	{
		if (!T17NetManager.IsConnectedOnline() || gamer == null || gamer.IsLocal())
		{
			return true;
		}
		return m_PhotonPlayers[m_MyPlatformIndex].FindIndex((PhotonPlayer x) => x != null && x.ID == gamer.m_PhotonID) != -1;
	}

	public bool IsGamerForMyPlatformOffline(Gamer gamer)
	{
		return m_MyPlatform == gamer.m_MyPlatform;
	}
}

using Rewired;
using UnityEngine;

public class Gamer
{
	public enum Location : byte
	{
		LOCAL,
		REMOTE
	}

	public enum CharacterSelectionStage
	{
		PreMenu,
		CharacterRequested,
		CharacterGranted,
		CharacterOwned,
		InMenu,
		InGame,
		EnabledInGame
	}

	public delegate void GamerEventHandler(Gamer gamer);

	public delegate void DeleteEventHandler();

	public const int INVALID_CONTROLLERINDEX = -1;

	public const int MaxNumPlayers = 4;

	public const int MinNumOnlinePlayers = 2;

	public const int MaxNumSinglePlayers = 1;

	public CrossplayLobbyManager.SupportedPlatforms m_MyPlatform;

	private string m_CachedClippedName;

	public bool m_bIsInPlayerSelectMenu;

	private static Gamer[] m_Gamers = new Gamer[4];

	public static int m_GamerCount = 0;

	public int m_iControllerIndex { get; private set; }

	public bool m_bPrimaryLocal { get; private set; }

	public int m_PhotonID { get; private set; }

	public int m_NetViewID { get; private set; }

	public Rewired.Player m_RewiredPlayer { get; private set; }

	public bool m_bActive { get; private set; }

	public string m_GamerName
	{
		get
		{
			return m_CachedClippedName;
		}
		private set
		{
			m_CachedClippedName = value;
			if (value != null && value.Length > 16)
			{
				m_CachedClippedName = value.Substring(0, 16);
				m_CachedClippedName += "...";
			}
		}
	}

	public string m_PlatformUniqueID { get; private set; }

	public Player m_PlayerObject { get; private set; }

	public Location m_eLocation { get; private set; }

	public CharacterSelectionStage m_eCharacterSelectionStage { get; set; }

	public static event GamerEventHandler OnDeleteRequested;

	public static event GamerEventHandler OnDeleteImminent;

	public static event DeleteEventHandler OnDeleted;

	public static event GamerEventHandler OnCreate;

	public static event GamerEventHandler OnUpdated;

	private Gamer()
	{
		m_eCharacterSelectionStage = CharacterSelectionStage.PreMenu;
		m_eLocation = Location.LOCAL;
		m_iControllerIndex = -1;
		m_bPrimaryLocal = false;
		m_NetViewID = -1;
		m_RewiredPlayer = null;
		m_PhotonID = -1;
		m_PlayerObject = null;
		m_PlatformUniqueID = string.Empty;
		m_bActive = true;
	}

	public bool IsLocal()
	{
		return m_eLocation != Location.REMOTE;
	}

	public static int GetGamerCount()
	{
		return m_GamerCount;
	}

	public static Gamer[] GetAllGamers()
	{
		return m_Gamers;
	}

	public static Gamer FindGamer(int iControllerIndex, int playerID, int netViewID, Location location, string uniqueID = null)
	{
		Gamer gamer = GetGamerByPlayerID(playerID, iControllerIndex, location);
		if (gamer == null && netViewID != -1)
		{
			gamer = GetGamerByViewID(netViewID);
		}
		if (gamer == null && uniqueID != null)
		{
			gamer = GetGamerByUniqueID(uniqueID);
		}
		return gamer;
	}

	public static Gamer GetGamerByUniqueID(string uniqueID)
	{
		for (int i = 0; i < 4; i++)
		{
			Gamer gamer = m_Gamers[i];
			if (gamer != null && gamer.m_PlatformUniqueID == uniqueID)
			{
				return gamer;
			}
		}
		return null;
	}

	public static void InvalidateOnlineIds(bool bNetView, bool bPlayerId)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null)
			{
				bool flag = false;
				if (bNetView && m_Gamers[num].m_NetViewID != -1)
				{
					m_Gamers[num].m_NetViewID = -1;
					flag = true;
				}
				if (bPlayerId && m_Gamers[num].m_PhotonID != -1)
				{
					if (m_Gamers[num].IsLocal())
					{
						m_Gamers[num].m_PhotonID = T17NetManager.PhotonPlayerID;
					}
					else
					{
						m_Gamers[num].m_PhotonID = -1;
					}
					flag = true;
				}
				if (flag && Gamer.OnUpdated != null)
				{
					Gamer.OnUpdated(m_Gamers[num]);
				}
			}
		}
	}

	public static Gamer InsertGamer(int iControllerIndex, int PlayerID, int NetViewID, string GamerName, Player PlayerObject, bool bPrimary)
	{
		int num = m_Gamers.Length;
		if (m_GamerCount >= num)
		{
			return null;
		}
		Gamer gamer = null;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (m_Gamers[num2] == null)
			{
				m_Gamers[num2] = new Gamer();
				gamer = m_Gamers[num2];
				m_GamerCount++;
				break;
			}
		}
		if (T17NetManager.PhotonPlayerID == PlayerID)
		{
			gamer.m_eLocation = Location.LOCAL;
			gamer.m_iControllerIndex = iControllerIndex;
			gamer.m_RewiredPlayer = ReInput.players.GetPlayer(iControllerIndex);
			gamer.m_MyPlatform = CrossplayLobbyManager.m_MyPlatform;
			if (FrontEndFlow.Instance != null)
			{
				T17EventSystem.ApplyCategories(gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Frontend);
			}
			else if (InGameMenuFlow.Instance != null)
			{
				T17EventSystem.ApplyCategories(gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.InGame);
			}
			else
			{
				T17EventSystem.ApplyCategories(gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Loading);
			}
		}
		else
		{
			gamer.m_eLocation = Location.REMOTE;
			gamer.m_iControllerIndex = iControllerIndex;
			gamer.m_RewiredPlayer = null;
		}
		gamer.m_NetViewID = NetViewID;
		gamer.m_PhotonID = PlayerID;
		gamer.m_GamerName = GamerName;
		gamer.m_PlayerObject = PlayerObject;
		gamer.m_bPrimaryLocal = bPrimary;
		if (Gamer.OnCreate != null)
		{
			Gamer.OnCreate(gamer);
		}
		return gamer;
	}

	public Gamer UpdateGamer(int iControllerIndex, int PlayerID, int NetViewID, string GamerName, Player PlayerObject, bool bPrimarySet, bool bPrimary, string uniquePlatformID)
	{
		bool flag = false;
		if (iControllerIndex != -1 && m_iControllerIndex != iControllerIndex)
		{
			m_iControllerIndex = iControllerIndex;
			if (PlayerID == PhotonNetwork.player.ID)
			{
				Rewired.Player player = ReInput.players.GetPlayer(iControllerIndex);
				if (player != null && m_RewiredPlayer != player)
				{
					if (m_RewiredPlayer != null)
					{
						T17EventSystem.ApplyCategories(m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
					}
					m_RewiredPlayer = player;
				}
			}
			flag = true;
		}
		if (m_NetViewID != NetViewID)
		{
			m_NetViewID = NetViewID;
			flag = true;
		}
		if (PlayerID >= 0 && m_PhotonID != PlayerID)
		{
			m_PhotonID = PlayerID;
			flag = true;
		}
		if (GamerName != null && m_GamerName != GamerName)
		{
			m_GamerName = GamerName;
			flag = true;
		}
		if (null != PlayerObject && m_PlayerObject != PlayerObject)
		{
			m_PlayerObject = PlayerObject;
			flag = true;
		}
		if (bPrimarySet && m_bPrimaryLocal != bPrimary)
		{
			m_bPrimaryLocal = bPrimary;
			flag = true;
		}
		if (!string.IsNullOrEmpty(uniquePlatformID) && m_PlatformUniqueID != uniquePlatformID)
		{
			m_PlatformUniqueID = uniquePlatformID;
			flag = true;
		}
		if (flag && Gamer.OnUpdated != null)
		{
			Gamer.OnUpdated(this);
		}
		return this;
	}

	public static Gamer UpsertGamer(int iControllerIndex, int PlayerID, int NetViewID, string GamerName, Player PlayerObject, bool bPrimarySet, bool bPrimary)
	{
		bool flag = PlayerID == T17NetManager.PhotonPlayerID;
		Gamer gamer = FindGamer(iControllerIndex, PlayerID, NetViewID, (!flag) ? Location.REMOTE : Location.LOCAL);
		if (gamer == null && m_GamerCount < m_Gamers.Length)
		{
			return InsertGamer(iControllerIndex, PlayerID, NetViewID, GamerName, PlayerObject, bPrimary);
		}
		return gamer?.UpdateGamer(iControllerIndex, PlayerID, NetViewID, GamerName, PlayerObject, bPrimarySet, bPrimary, null);
	}

	public static Gamer GetPrimaryGamer()
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && m_Gamers[num].m_bPrimaryLocal)
			{
				return m_Gamers[num];
			}
		}
		return null;
	}

	public static Gamer GetGamerByRewiredId(int Id)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && m_Gamers[num].m_RewiredPlayer.id == Id)
			{
				return m_Gamers[num];
			}
		}
		return null;
	}

	public static Gamer GetGamerByPlayerID(int playerID, int iControllerIndex, Location location)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && m_Gamers[num].m_iControllerIndex == iControllerIndex && m_Gamers[num].m_PhotonID == playerID && m_Gamers[num].m_eLocation == location)
			{
				return m_Gamers[num];
			}
		}
		return null;
	}

	public static Gamer GetGamerByViewID(int viewID)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && m_Gamers[num].m_NetViewID == viewID)
			{
				return m_Gamers[num];
			}
		}
		return null;
	}

	public static int GetNumLocalGamers()
	{
		int num = 0;
		for (int num2 = 3; num2 >= 0; num2--)
		{
			if (m_Gamers[num2] != null && m_Gamers[num2].IsLocal())
			{
				num++;
			}
		}
		return num;
	}

	public static Gamer[] GetLocalGamers()
	{
		Gamer[] array = new Gamer[GetNumLocalGamers()];
		int num = 0;
		for (int num2 = 3; num2 >= 0; num2--)
		{
			if (m_Gamers[num2] != null && m_Gamers[num2].IsLocal())
			{
				array[num++] = m_Gamers[num2];
			}
		}
		return array;
	}

	public static int GetNumRemoteGamers()
	{
		int num = 0;
		for (int num2 = 3; num2 >= 0; num2--)
		{
			if (m_Gamers[num2] != null && !m_Gamers[num2].IsLocal())
			{
				num++;
			}
		}
		return num;
	}

	public static void DumpGamers()
	{
		Debug.Log(" ............................... Gamers ..............");
		for (int i = 0; i < 4; i++)
		{
			DumpGamer(i);
		}
		Debug.Log(" ............................... Gamers ..............");
	}

	public static void DumpGamer(int iIndex)
	{
		if (iIndex >= 0 && iIndex < 4 && m_Gamers[iIndex] != null)
		{
			Debug.LogFormat("G{0} : ViewID:{1} PPID:{2} GamerName:{3} Controller:{4} Location:{5} Primary:{6} Active:{7}", iIndex, m_Gamers[iIndex].m_NetViewID, m_Gamers[iIndex].m_PhotonID, m_Gamers[iIndex].m_GamerName, m_Gamers[iIndex].m_iControllerIndex, m_Gamers[iIndex].m_eLocation.ToString(), m_Gamers[iIndex].m_bPrimaryLocal.ToString(), m_Gamers[iIndex].m_bActive.ToString());
		}
	}

	public static void DumpGamer(Gamer theGamer)
	{
		if (theGamer != null)
		{
			Debug.LogFormat("ViewID:{0} PPID:{1} GamerName:{2} Controller:{3} Location:{4} Primary:{5} Active:{6}", theGamer.m_NetViewID, theGamer.m_PhotonID, theGamer.m_GamerName, theGamer.m_iControllerIndex, theGamer.m_eLocation.ToString(), theGamer.m_bPrimaryLocal.ToString(), theGamer.m_bActive.ToString());
		}
	}

	public static void DeleteGamerByPhotonPlayerID(int photonID)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && photonID == m_Gamers[num].m_PhotonID)
			{
				DeleteGamer(num);
			}
		}
	}

	public static void DeleteGamerByNetviewID(int netviewID)
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && netviewID == m_Gamers[num].m_NetViewID)
			{
				DeleteGamer(num);
			}
		}
	}

	public static void DeleteGamer(int index, bool clearRewiredMaps = false)
	{
		if (index >= 0 && index < m_Gamers.Length && m_Gamers[index] != null)
		{
			if (clearRewiredMaps && m_Gamers[index].m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(m_Gamers[index].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
			}
			if (Gamer.OnDeleteRequested != null)
			{
				Gamer.OnDeleteRequested(m_Gamers[index]);
			}
			if (Gamer.OnDeleteImminent != null)
			{
				Gamer.OnDeleteImminent(m_Gamers[index]);
			}
			m_Gamers[index].m_bActive = false;
			m_Gamers[index] = null;
			m_GamerCount--;
			if (Gamer.OnDeleted != null)
			{
				Gamer.OnDeleted();
			}
		}
	}

	public static void DeleteRemoteGamers()
	{
		for (int num = 3; num >= 0; num--)
		{
			if (m_Gamers[num] != null && !m_Gamers[num].IsLocal())
			{
				DeleteGamer(num);
			}
		}
	}

	public static void DeleteLocalNonPrimaryGamers()
	{
		for (int num = 3; num >= 0; num--)
		{
			Gamer gamer = m_Gamers[num];
			if (gamer != null && !gamer.m_bPrimaryLocal)
			{
				DeleteGamer(num, clearRewiredMaps: true);
			}
		}
	}

	public void ForceNullOfPlayerObject()
	{
		m_PlayerObject = null;
	}
}

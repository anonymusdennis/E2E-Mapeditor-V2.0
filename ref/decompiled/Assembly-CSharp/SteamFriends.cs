using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamFriends : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void RequestFriendsList(Platform.RequestFriendsListCallback requestFriendsCallback)
	{
		int friendCount = Steamworks.SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		List<Platform.DisplayableFriend> list = new List<Platform.DisplayableFriend>(friendCount);
		List<Platform.DisplayableFriend> list2 = new List<Platform.DisplayableFriend>(friendCount / 10);
		List<Platform.DisplayableFriend> list3 = new List<Platform.DisplayableFriend>(friendCount / 4);
		List<Platform.DisplayableFriend> list4 = new List<Platform.DisplayableFriend>(friendCount / 4);
		List<Platform.DisplayableFriend> list5 = new List<Platform.DisplayableFriend>(friendCount / 2);
		CGameID cGameID = new CGameID(SteamPlatform.GetAppId());
		for (int i = 0; i < friendCount; i++)
		{
			CSteamID friendByIndex = Steamworks.SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			if (!friendByIndex.IsValid())
			{
				continue;
			}
			Platform.DisplayableFriend displayableFriend = new Platform.DisplayableFriend();
			EPersonaState friendPersonaState = Steamworks.SteamFriends.GetFriendPersonaState(friendByIndex);
			displayableFriend.m_Gamer = null;
			displayableFriend.m_Name = Steamworks.SteamFriends.GetFriendPersonaName(friendByIndex);
			displayableFriend.m_OnlineID = friendByIndex.ToString();
			displayableFriend.m_Presence = string.Empty;
			FriendGameInfo_t pFriendGameInfo = default(FriendGameInfo_t);
			if (Steamworks.SteamFriends.GetFriendGamePlayed(friendByIndex, out pFriendGameInfo))
			{
				if (pFriendGameInfo.m_gameID == cGameID)
				{
					displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Ingame;
					list2.Add(displayableFriend);
				}
				else
				{
					displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Online;
					list3.Add(displayableFriend);
				}
				continue;
			}
			switch (friendPersonaState)
			{
			case EPersonaState.k_EPersonaStateOffline:
				displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Offline;
				list5.Add(displayableFriend);
				break;
			case EPersonaState.k_EPersonaStateOnline:
				displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Online;
				list3.Add(displayableFriend);
				break;
			case EPersonaState.k_EPersonaStateBusy:
				displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Online;
				list3.Add(displayableFriend);
				break;
			case EPersonaState.k_EPersonaStateAway:
			case EPersonaState.k_EPersonaStateSnooze:
				displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Away;
				list4.Add(displayableFriend);
				break;
			default:
				displayableFriend.m_ActivityState = Platform.DisplayableFriend.ActivityState.Unknown;
				list5.Add(displayableFriend);
				break;
			}
		}
		list.AddRange(list2);
		list.AddRange(list3);
		list.AddRange(list4);
		list.AddRange(list5);
		requestFriendsCallback(list);
	}

	public void CancelFriendsListRequest()
	{
	}
}

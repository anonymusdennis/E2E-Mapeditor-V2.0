using System;
using UnityEngine;

public class T17NetRoomManager : T17NetSendMonoMessageTarget
{
	public delegate void RoomTypeChanged(T17NetRoomGameView.GameRoomType roomType);

	private static T17NetRoomManager _instance;

	private T17NetRoomGameView.GameRoomType m_LastKnownRoomType = T17NetRoomGameView.GameRoomType.Undefined;

	private T17NetRoomGameView.GameRoomType m_DeferredRoomTypeChange = T17NetRoomGameView.GameRoomType.Undefined;

	private string m_DeferredRoomPasswordChange = string.Empty;

	private bool m_UpdatePassword;

	private static string m_CurrentRoomPassword = string.Empty;

	public RoomTypeChanged OnRoomTypeChanged;

	public GUISkin Skin;

	public static T17NetRoomManager Instance => _instance;

	public static T17NetRoomGameView.GameRoomType CurrentGameRoomType
	{
		get
		{
			if (!PhotonNetwork.inRoom)
			{
				return T17NetRoomGameView.GameRoomType.Undefined;
			}
			if (!PhotonNetwork.offlineMode)
			{
				if (PhotonNetwork.room.IsVisible)
				{
					return T17NetRoomGameView.GameRoomType.Public;
				}
				return T17NetRoomGameView.GameRoomType.Private;
			}
			return T17NetRoomGameView.GameRoomType.Offline;
		}
	}

	public static string CurrentRoomPassword
	{
		get
		{
			if (!PhotonNetwork.inRoom)
			{
				return string.Empty;
			}
			if (!PhotonNetwork.offlineMode)
			{
				if (PhotonNetwork.room.IsVisible)
				{
					return m_CurrentRoomPassword;
				}
				return string.Empty;
			}
			return string.Empty;
		}
	}

	public static string GetCurrentRoomPassword()
	{
		return m_CurrentRoomPassword;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		_instance = this;
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
	}

	private void OnBecameMasterClient()
	{
		string outValue = string.Empty;
		T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.Password, ref outValue, PhotonNetwork.room.CustomProperties);
		m_DeferredRoomPasswordChange = Encryption.Decrypt(outValue, "default");
		m_UpdatePassword = true;
	}

	private void OnDestroy()
	{
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
	}

	private void Update()
	{
		if (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient && PhotonNetwork.room != null)
		{
			if (m_DeferredRoomTypeChange != T17NetRoomGameView.GameRoomType.Undefined && ChangeRoomType(m_DeferredRoomTypeChange))
			{
				m_DeferredRoomTypeChange = T17NetRoomGameView.GameRoomType.Undefined;
			}
			if (m_UpdatePassword && ChangeRoomPassword(m_DeferredRoomPasswordChange))
			{
				m_DeferredRoomPasswordChange = string.Empty;
				m_UpdatePassword = false;
			}
			if (PhotonNetwork.room.IsOpen && Gamer.m_GamerCount >= 4)
			{
				PhotonNetwork.room.IsOpen = false;
			}
			else if (!PhotonNetwork.room.IsOpen && Gamer.m_GamerCount < 4)
			{
				PhotonNetwork.room.IsOpen = true;
			}
		}
		if (m_DeferredRoomTypeChange == T17NetRoomGameView.GameRoomType.Undefined && m_LastKnownRoomType != CurrentGameRoomType)
		{
			if (OnRoomTypeChanged != null)
			{
				OnRoomTypeChanged(CurrentGameRoomType);
			}
			m_LastKnownRoomType = CurrentGameRoomType;
		}
	}

	public void SetPropertiesForGameroomType(T17NetRoomGameView.GameRoomType type)
	{
		m_DeferredRoomTypeChange = T17NetRoomGameView.GameRoomType.Undefined;
		if (T17NetManager.IsMasterClient && !ChangeRoomType(type))
		{
			m_DeferredRoomTypeChange = type;
		}
	}

	public void SetPropertiesForGameroomPassword(string password)
	{
		m_DeferredRoomPasswordChange = string.Empty;
		if (T17NetManager.IsMasterClient && !ChangeRoomPassword(password))
		{
			m_DeferredRoomPasswordChange = password;
			m_UpdatePassword = true;
		}
	}

	private bool ChangeRoomType(T17NetRoomGameView.GameRoomType type)
	{
		if (PhotonNetwork.room != null && T17NetManager.IsMasterClient)
		{
			switch (type)
			{
			case T17NetRoomGameView.GameRoomType.Offline:
				PhotonNetwork.room.IsOpen = false;
				PhotonNetwork.room.IsVisible = false;
				break;
			case T17NetRoomGameView.GameRoomType.Public:
				PhotonNetwork.room.IsOpen = true;
				PhotonNetwork.room.IsVisible = true;
				Platform.GetInstance().SessionTypeChange(Platform.SessionType.SESSION_TYPE_PUBLIC);
				break;
			case T17NetRoomGameView.GameRoomType.Private:
				PhotonNetwork.room.IsOpen = true;
				PhotonNetwork.room.IsVisible = false;
				Platform.GetInstance().SessionTypeChange(Platform.SessionType.SESSION_TYPE_PRIVATE);
				break;
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.RoomType, (int)type);
			return true;
		}
		return false;
	}

	private bool ChangeRoomPassword(string password)
	{
		if (PhotonNetwork.room != null && T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.Password, password);
			m_CurrentRoomPassword = password;
			m_UpdatePassword = false;
			return true;
		}
		return false;
	}

	public static bool IsInRoom()
	{
		return PhotonNetwork.inRoom && T17NetEncryptionKeys.HaveEncryptionKey();
	}
}

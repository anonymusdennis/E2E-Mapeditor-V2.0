using System;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class NetBluePrintDetails : MonoBehaviour
{
	[Serializable]
	public class SerializableData
	{
		public PlaylistData.NetPlaylistData m_Playlist;

		public int m_mapRotationIndex;
	}

	private bool m_updateCustomProperty;

	private T17NetView m_netView;

	private static NetBluePrintDetails m_instance;

	private SerializableData m_serializableData;

	public static NetBluePrintDetails Instance => m_instance;

	public int MapRotationIndex
	{
		get
		{
			return m_serializableData.m_mapRotationIndex;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
			{
			}
			m_serializableData.m_mapRotationIndex = value;
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonBlueprint, SerializeToJson());
		}
	}

	public PlaylistData.NetPlaylistData Playlist
	{
		get
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
			{
			}
			return m_serializableData.m_Playlist;
		}
		set
		{
			if (m_serializableData != null)
			{
				if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
				{
				}
				if (T17NetManager.IsMasterClient)
				{
					m_serializableData.m_Playlist = value;
					T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonBlueprint, SerializeToJson());
				}
			}
		}
	}

	public string LevelSceneName
	{
		get
		{
			PlaylistData.NetPrisonSetup currentPrisonConfig = CurrentPrisonConfig;
			if (currentPrisonConfig != null)
			{
				PrisonData.LevelInfo prisonInfo = currentPrisonConfig.m_PrisonInfo;
				if (prisonInfo != null)
				{
					string associatedFile = prisonInfo.m_AssociatedFile;
					if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
					{
					}
					return associatedFile;
				}
			}
			return null;
		}
	}

	public int LevelConfigID
	{
		get
		{
			PlaylistData.NetPrisonSetup currentPrisonConfig = CurrentPrisonConfig;
			if (currentPrisonConfig != null)
			{
				int configIndex = currentPrisonConfig.m_ConfigIndex;
				if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
				{
				}
				return configIndex;
			}
			return -1;
		}
	}

	public string LevelNameViaPlaylists => CurrentPrisonConfig?.m_PrisonInfo.m_AssociatedFile;

	public PlaylistData.NetPrisonSetup CurrentPrisonConfig
	{
		get
		{
			if (m_serializableData == null)
			{
				return null;
			}
			int num = m_serializableData.m_mapRotationIndex;
			if (num < 0)
			{
				num = 0;
			}
			if (m_serializableData.m_Playlist == null)
			{
				return null;
			}
			return m_serializableData.m_Playlist.m_PrisonSetups[num];
		}
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			Debug.LogError("More than one UserManager instance has been created, it expects to be a singleton.", this);
			return;
		}
		m_instance = this;
		ResetBluePrint();
	}

	private void Start()
	{
		m_netView = GetComponent<T17NetView>();
		if (m_netView != null && !DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetViewID))
		{
		}
	}

	protected virtual void OnDestroy()
	{
		m_netView = null;
	}

	public void Update()
	{
		if (!m_updateCustomProperty)
		{
			return;
		}
		bool flag = false;
		if (T17NetManager.OfflineMode || T17NetManager.IsMasterClient)
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
			{
			}
			flag = T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonBlueprint, SerializeToJson());
		}
		if (flag)
		{
			m_updateCustomProperty = false;
		}
	}

	public void ForcePushBluePrint()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_updateCustomProperty = true;
		}
	}

	public void ResetBluePrint()
	{
		if (m_serializableData == null)
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
			{
			}
			m_serializableData = new SerializableData();
		}
		GetProfileCharacterChoices();
	}

	public void NextPrison()
	{
		if (T17NetManager.IsMasterClient && m_serializableData != null && m_serializableData.m_Playlist != null && m_serializableData.m_Playlist.m_PrisonSetups != null)
		{
			int count = m_serializableData.m_Playlist.m_PrisonSetups.Count;
			if (++MapRotationIndex >= count)
			{
				MapRotationIndex = 0;
			}
		}
	}

	public string SerializeToJson()
	{
		return JsonUtility.ToJson(m_serializableData);
	}

	public void SerializeFromJson(string json)
	{
		try
		{
			m_serializableData = JsonUtility.FromJson<SerializableData>(json);
		}
		catch
		{
			m_serializableData = new SerializableData();
		}
	}

	private void GetProfileCharacterChoices()
	{
	}
}

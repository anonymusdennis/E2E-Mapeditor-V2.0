using System;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class NetPrisonViewDetails : MonoBehaviour
{
	[Serializable]
	public class SerializableData
	{
	}

	private T17NetView m_netView;

	private static NetPrisonViewDetails m_instance;

	private SerializableData m_serializableData;

	public string m_CarriedObjectsData;

	public string m_ToiletData;

	public string m_CctvCamerasData;

	public string m_SolitaryData;

	public string m_OpinionData;

	public string m_PlayerAppearanceData;

	public string m_PlayerTagData;

	public string m_PlayerItemTrackingData;

	public static NetPrisonViewDetails Instance => m_instance;

	public string ToiletInteractionData
	{
		get
		{
			return m_ToiletData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_ToiletData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.ToiletData, T17NetManager.IsMasterClient);
		}
	}

	public string CctvCameraData
	{
		get
		{
			return m_CctvCamerasData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_CctvCamerasData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.CCTVCameraData, T17NetManager.IsMasterClient);
		}
	}

	public string CarriedObjectsData
	{
		get
		{
			return m_CarriedObjectsData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_CarriedObjectsData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.CarriedObjectsData, T17NetManager.IsMasterClient);
		}
	}

	public string OpinionData
	{
		get
		{
			return m_OpinionData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_OpinionData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.OpinionData, T17NetManager.IsMasterClient);
		}
	}

	public string SolitaryData
	{
		get
		{
			return m_SolitaryData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_SolitaryData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.SolitaryData, T17NetManager.IsMasterClient);
		}
	}

	public string PlayerAppearanceData
	{
		get
		{
			return m_PlayerAppearanceData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_PlayerAppearanceData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerAppearanceData, T17NetManager.IsMasterClient);
		}
	}

	public string PlayerTagData
	{
		get
		{
			return m_PlayerTagData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_PlayerTagData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerTagData, T17NetManager.IsMasterClient);
		}
	}

	public string PlayerItemTrackingData
	{
		get
		{
			return m_PlayerItemTrackingData;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			m_PlayerItemTrackingData = value;
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerItemTrackingData, T17NetManager.IsMasterClient);
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
		ResetPrisonView();
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
		bool isMasterClient = T17NetManager.IsMasterClient;
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.PrisonDetails))
		{
			string value = SerializeToJson();
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonDetails, value);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PrisonDetails, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.CarriedObjectsData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.CarriedObjectsData, m_CarriedObjectsData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.CarriedObjectsData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.ToiletData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.ToiletData, m_ToiletData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.ToiletData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.CCTVCameraData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.CCTVCameraData, m_CctvCamerasData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.CCTVCameraData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.SolitaryData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.SolitaryData, m_SolitaryData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.SolitaryData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.OpinionData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.OpinionData, m_OpinionData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.OpinionData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.PlayerAppearanceData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PlayerAppearanceData, m_PlayerAppearanceData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerAppearanceData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.PlayerTagData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PlayerTagData, m_PlayerTagData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerTagData, bDirty: false);
		}
		if (isMasterClient && T17NetRoomGameView.GetCustomPropertyDirty(T17NetRoomGameView.CustomProperty.PlayerItemTrackingData))
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPrisonViewDetails))
			{
			}
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PlayerItemTrackingData, m_PlayerItemTrackingData);
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PlayerItemTrackingData, bDirty: false);
		}
	}

	public void ForcePushPrisonView()
	{
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.SetCustomPropertyDirtyFlag(T17NetRoomGameView.CustomProperty.PrisonDetails, bDirty: true);
		}
	}

	public void ResetPrisonView()
	{
		m_serializableData = new SerializableData();
		m_CarriedObjectsData = string.Empty;
		m_ToiletData = string.Empty;
		m_CctvCamerasData = string.Empty;
		m_SolitaryData = string.Empty;
		m_OpinionData = string.Empty;
		m_PlayerAppearanceData = string.Empty;
		m_PlayerTagData = string.Empty;
		m_PlayerItemTrackingData = string.Empty;
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
}

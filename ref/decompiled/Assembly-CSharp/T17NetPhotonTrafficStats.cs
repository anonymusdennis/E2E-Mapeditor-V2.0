using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using UnityEngine;

public class T17NetPhotonTrafficStats
{
	[Serializable]
	public class StatInvocationRecord
	{
		public bool Inbound { get; private set; }

		public int Bytes { get; private set; }

		public int SendTime { get; private set; }

		public object[] MethodParameters { get; private set; }

		public StatInvocationRecord(bool inbound, int byteCountLastOperation, int sendTime, object[] methodParameters)
		{
			Inbound = inbound;
			Bytes = byteCountLastOperation;
			SendTime = sendTime;
			MethodParameters = methodParameters;
		}
	}

	[Serializable]
	public class StatViewIDRecord
	{
		private List<StatInvocationRecord> m_records = new List<StatInvocationRecord>();

		public int ViewId { get; private set; }

		public string Name { get; private set; }

		public int BytesInbound { get; private set; }

		public int BytesOutbound { get; private set; }

		public int TotalBytes => BytesInbound + BytesOutbound;

		public int CountInbound { get; private set; }

		public int CountOutbound { get; private set; }

		public int Count => CountInbound + CountOutbound;

		public int Min { get; private set; }

		public int Max { get; private set; }

		public int Range => Max - Min;

		public int Average => (Count > 0) ? ((int)Math.Ceiling((float)TotalBytes / (float)Count)) : 0;

		public ReadOnlyCollection<StatInvocationRecord> StatInvocationRecords => m_records.AsReadOnly();

		[JsonIgnore]
		public GameObject GameObject { get; set; }

		public StatViewIDRecord(int netViewID)
		{
			ViewId = netViewID;
			GameObject = LookupViewID(netViewID);
			Name = ((!(GameObject == null)) ? GameObject.name : string.Empty);
			Min = int.MaxValue;
			Max = int.MinValue;
		}

		internal void Capture(bool inbound, int byteCountLastOperation, int sendTime, object[] methodParameters)
		{
			if (inbound)
			{
				BytesInbound += byteCountLastOperation;
				CountInbound++;
			}
			else
			{
				BytesOutbound += byteCountLastOperation;
				CountOutbound++;
			}
			Min = Math.Min(Min, byteCountLastOperation);
			Max = Math.Max(Max, byteCountLastOperation);
			m_records.Add(new StatInvocationRecord(inbound, byteCountLastOperation, sendTime, methodParameters));
		}
	}

	[Serializable]
	public class RPCStatMethodRecord
	{
		private Dictionary<int, StatViewIDRecord> m_StatInvocationRecords = new Dictionary<int, StatViewIDRecord>();

		public string MethodName { get; private set; }

		public int BytesInbound { get; private set; }

		public int BytesOutbound { get; private set; }

		public int CountInbound { get; private set; }

		public int CountOutbound { get; private set; }

		public int Min { get; private set; }

		public int Max { get; private set; }

		public int Count => CountInbound + CountOutbound;

		public int TotalBytes => BytesInbound + BytesOutbound;

		public int Range => Max - Min;

		public int Average => (Count > 0) ? ((int)Math.Ceiling((float)TotalBytes / (float)Count)) : 0;

		public ReadOnlyCollection<StatViewIDRecord> StatInvocationRecords => (m_StatInvocationRecords != null) ? m_StatInvocationRecords.Values.ToList().AsReadOnly() : null;

		public RPCStatMethodRecord(string methodName)
		{
			MethodName = methodName;
			Min = int.MaxValue;
			Max = int.MinValue;
		}

		internal void Capture(bool inbound, int byteCountLastOperation, int netViewID, int sendTime, object[] methodParameters)
		{
			if (inbound)
			{
				BytesInbound += byteCountLastOperation;
				CountInbound++;
			}
			else
			{
				BytesOutbound += byteCountLastOperation;
				CountOutbound++;
			}
			Min = Math.Min(Min, byteCountLastOperation);
			Max = Math.Max(Max, byteCountLastOperation);
			StatViewIDRecord value = null;
			if (!m_StatInvocationRecords.TryGetValue(netViewID, out value))
			{
				value = new StatViewIDRecord(netViewID);
				m_StatInvocationRecords.Add(netViewID, value);
			}
			value?.Capture(inbound, byteCountLastOperation, sendTime, methodParameters);
		}
	}

	private static Dictionary<string, RPCStatMethodRecord> m_RPCStatMethodRecords;

	private static Dictionary<int, StatViewIDRecord> m_SerializerStatViewIDRecords;

	private static Dictionary<int, GameObject> m_ViewIDLookup;

	private static Vector2 m_scrollPosition;

	private static Rect m_windowRect;

	private const int m_windowID = 64001;

	public static ReadOnlyCollection<RPCStatMethodRecord> RPCStatMethodRecords => m_RPCStatMethodRecords.Values.ToList().AsReadOnly();

	public static ReadOnlyCollection<StatViewIDRecord> SerializerStatViewIDRecords => (m_SerializerStatViewIDRecords != null) ? m_SerializerStatViewIDRecords.Values.ToList().AsReadOnly() : null;

	public static bool EditorDirty { get; set; }

	public static bool ResetOnLevelLoad { get; set; }

	public static bool CaptureDisabled { get; set; }

	public static bool ShowWindow { get; set; }

	static T17NetPhotonTrafficStats()
	{
		m_SerializerStatViewIDRecords = new Dictionary<int, StatViewIDRecord>();
		m_windowRect = new Rect(0f, 100f, 250f, 100f);
		Reset();
		ResetOnLevelLoad = true;
	}

	public static void Reset()
	{
		m_RPCStatMethodRecords = new Dictionary<string, RPCStatMethodRecord>(StringComparer.Ordinal);
		m_SerializerStatViewIDRecords = new Dictionary<int, StatViewIDRecord>();
		m_ViewIDLookup = new Dictionary<int, GameObject>();
		EditorDirty = true;
	}

	internal static GameObject LookupViewID(int netViewID)
	{
		GameObject value = null;
		if (!m_ViewIDLookup.TryGetValue(netViewID, out value))
		{
			T17NetView t17NetView = T17NetView.Find<T17NetView>(netViewID);
			if (t17NetView != null)
			{
				value = t17NetView.gameObject;
			}
			m_ViewIDLookup.Add(netViewID, value);
		}
		return value;
	}

	public static void CaptureRPCStats(bool inbound, object customEventContent, int byteCountLastOperation)
	{
		if (CaptureDisabled || !(customEventContent is Hashtable hashtable))
		{
			return;
		}
		int netViewID = (int)hashtable[(byte)0];
		int sendTime = (int)hashtable[(byte)2];
		string methodName = string.Empty;
		if (hashtable.ContainsKey((byte)5))
		{
			int num = (short)hashtable[(byte)5];
			if (num <= PhotonNetwork.PhotonServerSettings.RpcList.Count - 1)
			{
				methodName = PhotonNetwork.PhotonServerSettings.RpcList[num];
			}
		}
		else
		{
			methodName = (string)hashtable[(byte)3];
		}
		object[] array = null;
		if (hashtable.ContainsKey((byte)4))
		{
			array = (object[])T17NetEncryptionKeys.Decrypt(hashtable[(byte)4]);
		}
		if (array == null)
		{
			array = new object[0];
		}
		CaptureRPCStats(inbound, methodName, byteCountLastOperation, netViewID, sendTime, array);
	}

	private static void CaptureRPCStats(bool inbound, string methodName, int byteCountLastOperation, int netViewID, int sendTime, object[] inMethodParameters)
	{
		RPCStatMethodRecord value = null;
		if (!m_RPCStatMethodRecords.TryGetValue(methodName, out value))
		{
			value = new RPCStatMethodRecord(methodName);
			m_RPCStatMethodRecords.Add(methodName, value);
		}
		value?.Capture(inbound, byteCountLastOperation, netViewID, sendTime, inMethodParameters);
		EditorDirty = true;
	}

	internal static void CaptureSerializeStats(bool inbound, object customEventContent, int byteCountLastOperation)
	{
		if (CaptureDisabled || !(customEventContent is Hashtable hashtable))
		{
			return;
		}
		int sendTime = (int)hashtable[(byte)0];
		short num = 1;
		if (hashtable.ContainsKey((byte)1))
		{
			num = 2;
		}
		num = 10;
		for (short num2 = num; num2 < hashtable.Count; num2++)
		{
			if (hashtable[num2] is Hashtable hashtable2)
			{
				int num3 = (int)hashtable2[(byte)0];
				object[] methodParameters = null;
				if (hashtable2.ContainsKey((byte)1))
				{
					methodParameters = hashtable2[(byte)1] as object[];
				}
				else if (hashtable2.ContainsKey((byte)2))
				{
					methodParameters = hashtable2[(byte)2] as object[];
				}
				StatViewIDRecord value = null;
				if (!m_SerializerStatViewIDRecords.TryGetValue(num3, out value))
				{
					value = new StatViewIDRecord(num3);
					m_SerializerStatViewIDRecords.Add(num3, value);
				}
				if (value != null)
				{
					value.Capture(inbound, byteCountLastOperation, sendTime, methodParameters);
					EditorDirty = true;
				}
			}
		}
	}

	public static void TrafficStatsWindow(int windowID)
	{
		m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
		GUILayoutOption gUILayoutOption = GUILayout.Width(m_windowRect.width / 7f);
		GUILayoutOption gUILayoutOption2 = GUILayout.Width(m_windowRect.width * 2f / 7f);
		GUILayout.BeginHorizontal();
		GUILayout.Label("MethodName", gUILayoutOption2);
		GUILayout.Label("Count", gUILayoutOption);
		GUILayout.Label("TotalBytes", gUILayoutOption);
		GUILayout.Label("Min", gUILayoutOption);
		GUILayout.Label("Max", gUILayoutOption);
		GUILayout.Label("Average", gUILayoutOption);
		GUILayout.EndHorizontal();
		foreach (RPCStatMethodRecord value in m_RPCStatMethodRecords.Values)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(value.MethodName, gUILayoutOption2);
			GUILayout.Label(value.Count.ToString(), gUILayoutOption);
			GUILayout.Label(T17NetPhotonTrafficStatsGui.SizeSuffix(value.TotalBytes), gUILayoutOption);
			GUILayout.Label(T17NetPhotonTrafficStatsGui.SizeSuffix(value.Min), gUILayoutOption);
			GUILayout.Label(T17NetPhotonTrafficStatsGui.SizeSuffix(value.Max), gUILayoutOption);
			GUILayout.Label(T17NetPhotonTrafficStatsGui.SizeSuffix(value.Average), gUILayoutOption);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		DragAndResize();
	}

	private static void DragAndResize()
	{
		Rect position = new Rect(m_windowRect.width - 20f, m_windowRect.height - 20f, 20f, 20f);
		GUI.Label(position, "∆");
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
	}
}

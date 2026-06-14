using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class T17NetPhotonTrafficStatsGui : MonoBehaviour
{
	private class BytesPerSec
	{
		private int m_start;

		private float m_startTime;

		public int Peak { get; private set; }

		public int Display { get; private set; }

		internal BytesPerSec()
		{
			Reset();
		}

		internal void Capture(int bytes, float timeStamp)
		{
			if (m_startTime > 0f)
			{
				Display = (int)((float)(bytes - m_start) / (timeStamp - m_startTime));
				Peak = Math.Max(Display, Peak);
			}
			m_start = bytes;
			m_startTime = timeStamp;
		}

		internal void Reset()
		{
			m_startTime = -1f;
			Display = 0;
			Peak = 0;
		}
	}

	private static T17NetPhotonTrafficStatsGui m_instance = null;

	private bool m_statsWindowOn;

	private bool m_statsOn;

	private bool m_healthStatsVisible;

	private bool m_trafficStatsOn;

	private float m_nextBytesPerSecCapture;

	private BytesPerSec m_bytesPerSecIn = new BytesPerSec();

	private BytesPerSec m_bytesPerSecOut = new BytesPerSec();

	private BytesPerSec m_bytesPerSecIn_GlobalStart = new BytesPerSec();

	private BytesPerSec m_bytesPerSecOut_GlobalStart = new BytesPerSec();

	private bool m_buttonsOn = true;

	private Rect m_statsRect = new Rect(0f, 200f, 250f, 100f);

	private long m_lastElapsedTimeInMs;

	private int m_elapsedTimeInSec;

	private static readonly string[] SizeSuffixes = new string[9] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

	public static T17NetPhotonTrafficStatsGui Instance => m_instance;

	public bool NetStatGuiWindowOn
	{
		get
		{
			return m_statsWindowOn;
		}
		set
		{
			m_statsWindowOn = value;
		}
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		m_instance = this;
	}

	public void Start()
	{
		m_statsRect.x = (float)Screen.width - m_statsRect.width;
		m_statsOn = Bootstrap.CmdLineOptions.NetTrafficEnabled;
		m_statsWindowOn = Bootstrap.CmdLineOptions.NetTrafficGuiEnabled;
		m_healthStatsVisible = Bootstrap.CmdLineOptions.NetTrafficShowHealth;
		m_trafficStatsOn = Bootstrap.CmdLineOptions.NetTrafficShowTraffic;
		m_statsOn = true;
		PhotonNetwork.networkingPeer.TrafficStatsEnabled = true;
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
		{
			m_statsWindowOn = !m_statsWindowOn;
			m_statsOn = true;
		}
	}

	public static string SizeSuffix(long value)
	{
		if (value < 0)
		{
			return "-" + SizeSuffix(-value);
		}
		if (value == 0)
		{
			return "0 B";
		}
		int num = (int)Math.Log(value, 1024.0);
		if (num == 0)
		{
			return $"{value} {SizeSuffixes[num]}";
		}
		decimal num2 = (decimal)value / (decimal)(1L << num * 10);
		return $"{num2:n1} {SizeSuffixes[num]}";
	}

	public string GetTrafficInStatsString()
	{
		m_bytesPerSecIn_GlobalStart.Capture(PhotonNetwork.networkingPeer.TrafficStatsIncoming.TotalPacketBytes, Time.realtimeSinceStartup);
		return SizeSuffix(m_bytesPerSecIn_GlobalStart.Display);
	}

	public string GetTrafficOutStatsString()
	{
		m_bytesPerSecOut_GlobalStart.Capture(PhotonNetwork.networkingPeer.TrafficStatsOutgoing.TotalPacketBytes, Time.realtimeSinceStartup);
		return SizeSuffix(m_bytesPerSecOut_GlobalStart.Display);
	}

	public int GetTrafficIn()
	{
		return m_bytesPerSecIn_GlobalStart.Display;
	}

	public int GetTrafficOut()
	{
		return m_bytesPerSecOut_GlobalStart.Display;
	}

	private string TrafficStatsToString(BytesPerSec bytesPerSec, TrafficStats ts)
	{
		return $"BPS:[{SizeSuffix(bytesPerSec.Display)}] MBPS[{SizeSuffix(bytesPerSec.Peak)}]";
	}

	public void TrafficStatsWindow(int windowID)
	{
		bool flag = false;
		TrafficStatsGameLevel trafficStatsGameLevel = PhotonNetwork.networkingPeer.TrafficStatsGameLevel;
		long trafficStatsElapsedMs = PhotonNetwork.networkingPeer.TrafficStatsElapsedMs;
		int num = (int)(trafficStatsElapsedMs - m_lastElapsedTimeInMs);
		long num2 = PhotonNetwork.networkingPeer.TrafficStatsElapsedMs / 1000;
		if (num2 == 0)
		{
			num2 = 1L;
		}
		if (m_elapsedTimeInSec != (int)num2)
		{
			m_elapsedTimeInSec = (int)num2;
			m_lastElapsedTimeInMs = trafficStatsElapsedMs;
		}
		GUILayout.BeginHorizontal();
		m_healthStatsVisible = GUILayout.Toggle(m_healthStatsVisible, "Health");
		m_trafficStatsOn = GUILayout.Toggle(m_trafficStatsOn, "Traffic");
		GUILayout.EndHorizontal();
		string text = string.Empty;
		if (trafficStatsGameLevel != null)
		{
			text = $"Pkt's Sum:{trafficStatsGameLevel.TotalMessageCount / num2} - Out:{trafficStatsGameLevel.TotalOutgoingMessageCount / num2} - In:{trafficStatsGameLevel.TotalIncomingMessageCount / num2} ";
		}
		TrafficStats trafficStatsIncoming = PhotonNetwork.networkingPeer.TrafficStatsIncoming;
		TrafficStats trafficStatsOutgoing = PhotonNetwork.networkingPeer.TrafficStatsOutgoing;
		string text2 = string.Empty;
		if (trafficStatsOutgoing != null)
		{
			text2 = $"OUT P:{trafficStatsOutgoing.TotalPacketCount / num2}:({trafficStatsOutgoing.TotalPacketBytes / num2}) R:{trafficStatsOutgoing.ReliableCommandCount / num2}:({trafficStatsOutgoing.ReliableCommandBytes / num2}) U:{trafficStatsOutgoing.UnreliableCommandCount / num2}:[{trafficStatsOutgoing.UnreliableCommandBytes / num2}]";
		}
		string text3 = string.Empty;
		if (trafficStatsIncoming != null)
		{
			text3 = $"IN P:{trafficStatsIncoming.TotalPacketCount / num2}:({trafficStatsIncoming.TotalPacketBytes / num2}) R:{trafficStatsIncoming.ReliableCommandCount / num2}:({trafficStatsIncoming.ReliableCommandBytes / num2}) U:{trafficStatsIncoming.UnreliableCommandCount / num2}:[{trafficStatsIncoming.UnreliableCommandBytes / num2}]";
		}
		string text4 = $"Session {num2}.{num}";
		GUILayout.Label(text4);
		GUILayout.Label(text);
		GUILayout.Label(text2);
		GUILayout.Label(text3);
		if (m_buttonsOn)
		{
			GUILayout.BeginHorizontal();
			m_statsOn = GUILayout.Toggle(m_statsOn, "stats on");
			if (GUILayout.Button("Reset"))
			{
				PhotonNetwork.networkingPeer.TrafficStatsReset();
				PhotonNetwork.networkingPeer.TrafficStatsEnabled = true;
				m_bytesPerSecIn.Reset();
				m_bytesPerSecOut.Reset();
			}
			flag = GUILayout.Button("To Log");
			GUILayout.EndHorizontal();
		}
		string text5 = string.Empty;
		string text6 = string.Empty;
		if (m_trafficStatsOn && PhotonNetwork.networkingPeer != null)
		{
			if (PhotonNetwork.networkingPeer.TrafficStatsIncoming != null && PhotonNetwork.networkingPeer.TrafficStatsOutgoing != null)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (realtimeSinceStartup > m_nextBytesPerSecCapture)
				{
					m_bytesPerSecIn.Capture(PhotonNetwork.networkingPeer.TrafficStatsIncoming.TotalPacketBytes, realtimeSinceStartup);
					m_bytesPerSecOut.Capture(PhotonNetwork.networkingPeer.TrafficStatsOutgoing.TotalPacketBytes, realtimeSinceStartup);
					m_nextBytesPerSecCapture = Time.realtimeSinceStartup + 1f;
				}
				text5 = TrafficStatsToString(m_bytesPerSecIn, PhotonNetwork.networkingPeer.TrafficStatsIncoming);
				text6 = TrafficStatsToString(m_bytesPerSecOut, PhotonNetwork.networkingPeer.TrafficStatsOutgoing);
			}
			GUILayout.Label("Traffic Outgoing");
			GUILayout.Label(text6);
			GUILayout.Label("Traffic Incoming");
			GUILayout.Label(text5);
		}
		string text7 = string.Empty;
		if (m_healthStatsVisible && trafficStatsGameLevel != null && PhotonNetwork.networkingPeer != null)
		{
			text7 = string.Format("ping: {6}[+/-{7}]ms\nlongest delta between\nsend: {0,4}ms disp: {1,4}ms\nlongest time for:\nev({3}):{2,3}ms op({5}):{4,3}ms", trafficStatsGameLevel.LongestDeltaBetweenSending, trafficStatsGameLevel.LongestDeltaBetweenDispatching, trafficStatsGameLevel.LongestEventCallback, trafficStatsGameLevel.LongestEventCallbackCode, trafficStatsGameLevel.LongestOpResponseCallback, trafficStatsGameLevel.LongestOpResponseCallbackOpCode, PhotonNetwork.networkingPeer.RoundTripTime, PhotonNetwork.networkingPeer.RoundTripTimeVariance);
			GUILayout.Label(text7);
		}
		if (flag)
		{
			string text8 = $"{text4}\n{text}\n{text5}\n{text6}\n{text7}";
		}
		if (GUI.changed)
		{
			m_statsRect.height = 150f;
		}
		GUI.DragWindow();
	}

	public string IncomingMessageStatsInfo()
	{
		TrafficStats trafficStatsIncoming = PhotonNetwork.networkingPeer.TrafficStatsIncoming;
		int num = trafficStatsIncoming.TotalCommandBytes / m_elapsedTimeInSec;
		int num2 = trafficStatsIncoming.TotalCommandCount / m_elapsedTimeInSec;
		int num3 = trafficStatsIncoming.TotalCommandsInPackets / m_elapsedTimeInSec;
		return $"RX:\nCmdInPacket:{num3} \nCmdCount:{num2} \nCmdBytes:{num}";
	}

	public string SendingMessageStatsInfo()
	{
		TrafficStats trafficStatsOutgoing = PhotonNetwork.networkingPeer.TrafficStatsOutgoing;
		int totalCommandBytes = trafficStatsOutgoing.TotalCommandBytes;
		int totalCommandCount = trafficStatsOutgoing.TotalCommandCount;
		int totalCommandsInPackets = trafficStatsOutgoing.TotalCommandsInPackets;
		return $"TX\nCmdInPacket:{totalCommandsInPackets}\nCmdCount:{totalCommandCount}\nCmdBytes:{totalCommandBytes}";
	}
}

using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class T17NetPhotonLagSimulationGui : MonoBehaviour
{
	private static T17NetPhotonLagSimulationGui m_instance;

	private Rect m_windowRect = new Rect(0f, 200f, 150f, 150f);

	private bool m_lagSimulationGuiVisible;

	public static T17NetPhotonLagSimulationGui Instance => m_instance;

	public bool NetLagSimulationGuiOn
	{
		get
		{
			return m_lagSimulationGuiVisible;
		}
		set
		{
			m_lagSimulationGuiVisible = value;
		}
	}

	private PhotonPeer PeerConfig { get; set; }

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
		PeerConfig = PhotonNetwork.networkingPeer;
	}

	private void NetSimHasNoPeerWindow(int windowId)
	{
		GUILayout.Label("No peer to communicate with. ");
	}

	private void NetSimWindow(int windowId)
	{
		GUILayout.Label($"Rtt:{PeerConfig.RoundTripTime,4} +/-{PeerConfig.RoundTripTimeVariance,3}");
		bool isSimulationEnabled = PeerConfig.IsSimulationEnabled;
		bool flag = GUILayout.Toggle(isSimulationEnabled, "Simulate");
		if (flag != isSimulationEnabled)
		{
			PeerConfig.IsSimulationEnabled = flag;
		}
		float num = PeerConfig.NetworkSimulationSettings.IncomingLag;
		GUILayout.Label("Lag " + num);
		num = GUILayout.HorizontalSlider(num, 0f, 500f);
		PeerConfig.NetworkSimulationSettings.IncomingLag = (int)num;
		PeerConfig.NetworkSimulationSettings.OutgoingLag = (int)num;
		float num2 = PeerConfig.NetworkSimulationSettings.IncomingJitter;
		GUILayout.Label("Jit " + num2);
		num2 = GUILayout.HorizontalSlider(num2, 0f, 100f);
		PeerConfig.NetworkSimulationSettings.IncomingJitter = (int)num2;
		PeerConfig.NetworkSimulationSettings.OutgoingJitter = (int)num2;
		float num3 = PeerConfig.NetworkSimulationSettings.IncomingLossPercentage;
		GUILayout.Label("Loss " + num3);
		num3 = GUILayout.HorizontalSlider(num3, 0f, 10f);
		PeerConfig.NetworkSimulationSettings.IncomingLossPercentage = (int)num3;
		PeerConfig.NetworkSimulationSettings.OutgoingLossPercentage = (int)num3;
		if (GUI.changed)
		{
			m_windowRect.height = 100f;
		}
		GUI.DragWindow();
	}
}

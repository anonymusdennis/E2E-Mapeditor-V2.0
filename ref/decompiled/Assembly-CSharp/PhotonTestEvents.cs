using UnityEngine;

public class PhotonTestEvents
{
	public PhotonTestEvents()
	{
		T17NetManager.OnPhotonConnectionChangeEvent += OnConnected;
	}

	public void OnConnected(bool bOnline)
	{
		if (bOnline && Bootstrap.m_StartupConnectionState == NetConnectionState.OnlineMode_Idle && !string.IsNullOrEmpty(Bootstrap.CmdLineOptions.RequestConnectionState))
		{
			Debug.Log("Entered requested network connection state from command line: " + Bootstrap.CmdLineOptions.RequestConnectionState);
			Bootstrap.Quit(0);
		}
	}
}

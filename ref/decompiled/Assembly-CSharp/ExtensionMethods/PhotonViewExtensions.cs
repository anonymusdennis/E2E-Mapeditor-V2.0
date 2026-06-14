using System;
using UnityEngine;

namespace ExtensionMethods;

public static class PhotonViewExtensions
{
	public static void EnsureRegistered(this PhotonView photonView)
	{
		try
		{
			PhotonView value = null;
			PhotonNetwork.networkingPeer.photonViewList.TryGetValue(photonView.viewID, out value);
			if (value == null)
			{
				Debug.Log("  ***** EnsureRegistered " + photonView.viewID + "   " + photonView.gameObject.name);
				PhotonNetwork.networkingPeer.RegisterPhotonView(photonView);
				photonView.RegisteredByTeam17 = true;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void EnsureUnregistered(this PhotonView photonView)
	{
		try
		{
			if (photonView.RegisteredByTeam17 && !photonView.didAwake)
			{
				PhotonNetwork.networkingPeer.LocalCleanPhotonView(photonView);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}

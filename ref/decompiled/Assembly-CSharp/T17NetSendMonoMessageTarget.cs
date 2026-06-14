using System.Collections.Generic;
using Photon;
using UnityEngine;

public abstract class T17NetSendMonoMessageTarget : Photon.MonoBehaviour
{
	protected virtual void Awake()
	{
	}

	public virtual void OnEnable()
	{
		if (PhotonNetwork.SendMonoMessageTargets == null)
		{
			PhotonNetwork.SendMonoMessageTargets = new HashSet<GameObject>();
		}
		PhotonNetwork.SendMonoMessageTargets.Add(base.gameObject);
	}

	public virtual void OnDisable()
	{
		if (PhotonNetwork.SendMonoMessageTargets != null)
		{
			PhotonNetwork.SendMonoMessageTargets.Remove(base.gameObject);
		}
	}
}

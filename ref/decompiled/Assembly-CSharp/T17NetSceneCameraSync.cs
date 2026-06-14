using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class T17NetSceneCameraSync : MonoBehaviour, IPunObservable
{
	public static bool SyncEnabled;

	public T17NetView m_NetView;

	public void Start()
	{
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
	}

	public virtual void OnDestroy()
	{
		m_NetView = null;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!SyncEnabled)
		{
		}
	}
}

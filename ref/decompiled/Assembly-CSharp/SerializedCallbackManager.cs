using UnityEngine;

public class SerializedCallbackManager : MonoBehaviour, IPunObservable
{
	public delegate void OnSerializeView(PhotonStream stream, ref PhotonMessageInfo info);

	private int m_iTickIndex;

	private int m_iSerializedCallbackCount;

	private SerializedCallback[] m_SerializedCallbacks = new SerializedCallback[5];

	private T17NetView m_NetView;

	protected virtual void OnDestroy()
	{
		m_NetView = null;
	}

	public void GiveNetView(T17NetView view)
	{
		m_NetView = view;
	}

	~SerializedCallbackManager()
	{
		m_SerializedCallbacks = null;
		m_NetView = null;
	}

	public void ForceSerialize()
	{
		m_NetView.photonView.lastOnSerializeDataSent = null;
	}

	public void AddCallback(OnSerializeView callback)
	{
		SerializedCallback serializedCallback = new SerializedCallback((byte)m_iSerializedCallbackCount++, callback);
		if (m_SerializedCallbacks.Length < m_iSerializedCallbackCount)
		{
			SerializedCallback[] array = new SerializedCallback[m_iSerializedCallbackCount];
			for (int i = 0; i < m_iSerializedCallbackCount - 1; i++)
			{
				array[i] = m_SerializedCallbacks[i];
			}
			m_SerializedCallbacks = array;
		}
		m_SerializedCallbacks[m_iSerializedCallbackCount - 1] = serializedCallback;
		m_NetView.photonView.ObservedComponents.Add(this);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			m_SerializedCallbacks[m_iTickIndex]?.WriteSerialize(stream, ref info);
			if (++m_iTickIndex >= m_iSerializedCallbackCount)
			{
				m_iTickIndex = 0;
			}
		}
		else
		{
			byte b = (byte)stream.ReceiveNext();
			if (b >= 0 && b < m_SerializedCallbacks.Length)
			{
				m_SerializedCallbacks[b]?.ReadSerialize(stream, ref info);
			}
		}
	}
}

using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public abstract class NetCharacterObserved : MonoBehaviour, IPunObservable
{
	public bool TeleportEnabled = true;

	public float TeleportIfDistanceGreaterThan = 3f;

	public float TeleportIfHeightChangeGreaterThan = 1f;

	private int m_teleportedByCode;

	private int m_networkTeleportedByCode = -1;

	public Vector3 Position { get; protected set; }

	public Quaternion Rotation { get; protected set; }

	public double LastNetworkDataReceivedTime { get; private set; }

	public virtual void Awake()
	{
	}

	public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		stream.Serialize(ref m_teleportedByCode);
		if (stream.isReading)
		{
			LastNetworkDataReceivedTime = info.timestamp;
		}
	}

	internal bool GetTeleportedByCode(bool peekOnly = false)
	{
		bool result = m_networkTeleportedByCode != m_teleportedByCode;
		if (!peekOnly)
		{
			m_networkTeleportedByCode = m_teleportedByCode;
		}
		return result;
	}

	internal void SetTeleportedByCode()
	{
		m_teleportedByCode++;
	}
}

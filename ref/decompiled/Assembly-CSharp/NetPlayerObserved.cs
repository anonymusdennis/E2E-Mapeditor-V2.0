using UnityEngine;

public class NetPlayerObserved : NetCharacterObserved
{
	public float RotationSlerpSpeed = 0.75f;

	public float PositionLerpSpeed = 0.25f;

	public float AnimationLerpSpeed = 0.75f;

	public bool PositionDrawDebug;

	public float SmoothingDelay = 1f;

	private PhotonView m_playerPhotonView;

	public float Speed { get; protected set; }

	public void Start()
	{
		if (m_playerPhotonView == null)
		{
			m_playerPhotonView = base.gameObject.GetComponent<PhotonView>();
			if (m_playerPhotonView == null)
			{
				Debug.LogWarningFormat("NetPlayerObserved: {0} : failed to Find PhotonView", base.gameObject.name);
			}
		}
	}

	public void Update()
	{
		if (m_playerPhotonView != null && !m_playerPhotonView.isMine)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, base.Position, UpdateManager.deltaTime * SmoothingDelay);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, base.Rotation, UpdateManager.deltaTime * SmoothingDelay);
		}
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnPhotonSerializeView(stream, info);
		SerializeState(stream, info);
	}

	private void SerializeState(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(base.transform.position);
			stream.SendNext(base.transform.rotation);
		}
		else
		{
			base.Position = (Vector3)stream.ReceiveNext();
			base.Rotation = (Quaternion)stream.ReceiveNext();
		}
	}
}

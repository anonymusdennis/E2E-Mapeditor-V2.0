using UnityEngine;

public class PhotonTransformViewRotationControl
{
	private PhotonTransformViewRotationModel m_Model;

	private Quaternion m_NetworkRotation;

	public PhotonTransformViewRotationControl(PhotonTransformViewRotationModel model)
	{
		m_Model = model;
	}

	public Quaternion GetNetworkRotation()
	{
		return m_NetworkRotation;
	}

	public Quaternion GetRotation(Quaternion currentRotation)
	{
		return m_Model.InterpolateOption switch
		{
			PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards => Quaternion.RotateTowards(currentRotation, m_NetworkRotation, m_Model.InterpolateRotateTowardsSpeed * Time.deltaTime), 
			PhotonTransformViewRotationModel.InterpolateOptions.Lerp => Quaternion.Lerp(currentRotation, m_NetworkRotation, m_Model.InterpolateLerpSpeed * Time.deltaTime), 
			_ => m_NetworkRotation, 
		};
	}

	public void OnPhotonSerializeView(Quaternion currentRotation, PhotonStream stream, PhotonMessageInfo info)
	{
		if (m_Model.SynchronizeEnabled)
		{
			if (stream.isWriting)
			{
				stream.SendNext(currentRotation);
				m_NetworkRotation = currentRotation;
			}
			else
			{
				m_NetworkRotation = (Quaternion)stream.ReceiveNext();
			}
		}
	}
}

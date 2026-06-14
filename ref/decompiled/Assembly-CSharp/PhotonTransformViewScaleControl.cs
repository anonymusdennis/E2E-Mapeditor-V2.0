using UnityEngine;

public class PhotonTransformViewScaleControl
{
	private PhotonTransformViewScaleModel m_Model;

	private Vector3 m_NetworkScale = Vector3.one;

	public PhotonTransformViewScaleControl(PhotonTransformViewScaleModel model)
	{
		m_Model = model;
	}

	public Vector3 GetNetworkScale()
	{
		return m_NetworkScale;
	}

	public Vector3 GetScale(Vector3 currentScale)
	{
		return m_Model.InterpolateOption switch
		{
			PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards => Vector3.MoveTowards(currentScale, m_NetworkScale, m_Model.InterpolateMoveTowardsSpeed * Time.deltaTime), 
			PhotonTransformViewScaleModel.InterpolateOptions.Lerp => Vector3.Lerp(currentScale, m_NetworkScale, m_Model.InterpolateLerpSpeed * Time.deltaTime), 
			_ => m_NetworkScale, 
		};
	}

	public void OnPhotonSerializeView(Vector3 currentScale, PhotonStream stream, PhotonMessageInfo info)
	{
		if (m_Model.SynchronizeEnabled)
		{
			if (stream.isWriting)
			{
				stream.SendNext(currentScale);
				m_NetworkScale = currentScale;
			}
			else
			{
				m_NetworkScale = (Vector3)stream.ReceiveNext();
			}
		}
	}
}

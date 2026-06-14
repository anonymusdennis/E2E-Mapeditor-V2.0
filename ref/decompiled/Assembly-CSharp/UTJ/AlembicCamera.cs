using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicCamera : AlembicElement
{
	public AbcAPI.aiAspectRatioModeOverride m_aspectRatioMode = AbcAPI.aiAspectRatioModeOverride.InheritStreamSetting;

	public bool m_ignoreClippingPlanes;

	private Camera m_camera;

	private AbcAPI.aiCameraData m_abcData;

	private bool m_lastIgnoreClippingPlanes;

	public override void AbcSetup(AlembicStream abcStream, AbcAPI.aiObject abcObj, AbcAPI.aiSchema abcSchema)
	{
		base.AbcSetup(abcStream, abcObj, abcSchema);
		m_camera = GetOrAddComponent<Camera>();
	}

	public override void AbcGetConfig(ref AbcAPI.aiConfig config)
	{
		if (m_aspectRatioMode != AbcAPI.aiAspectRatioModeOverride.InheritStreamSetting)
		{
			config.aspectRatio = AbcAPI.GetAspectRatio((AbcAPI.aiAspectRatioMode)m_aspectRatioMode);
		}
	}

	public override void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged)
	{
		AbcAPI.aiCameraGetData(sample, ref m_abcData);
		AbcDirty();
	}

	public override void AbcUpdate()
	{
		if (AbcIsDirty() || m_lastIgnoreClippingPlanes != m_ignoreClippingPlanes)
		{
			m_trans.forward = -m_trans.parent.forward;
			m_camera.fieldOfView = m_abcData.fieldOfView;
			if (!m_ignoreClippingPlanes)
			{
				m_camera.nearClipPlane = m_abcData.nearClippingPlane;
				m_camera.farClipPlane = m_abcData.farClippingPlane;
			}
			AbcClean();
			m_lastIgnoreClippingPlanes = m_ignoreClippingPlanes;
		}
	}
}

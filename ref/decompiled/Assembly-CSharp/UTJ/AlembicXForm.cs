using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicXForm : AlembicElement
{
	private AbcAPI.aiXFormData m_abcData;

	public override void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged)
	{
		AbcAPI.aiXFormGetData(sample, ref m_abcData);
		AbcDirty();
	}

	public override void AbcUpdate()
	{
		if (AbcIsDirty())
		{
			if (m_abcData.inherits)
			{
				m_trans.localPosition = m_abcData.translation;
				m_trans.localRotation = m_abcData.rotation;
				m_trans.localScale = m_abcData.scale;
			}
			else
			{
				m_trans.position = m_abcData.translation;
				m_trans.rotation = m_abcData.rotation;
				m_trans.localScale = m_abcData.scale;
			}
			AbcClean();
		}
	}
}

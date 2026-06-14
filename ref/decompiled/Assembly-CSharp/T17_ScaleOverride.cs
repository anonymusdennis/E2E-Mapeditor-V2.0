using System;
using System.Collections.Generic;
using UnityEngine;

public class T17_ScaleOverride : MonoBehaviour
{
	[Serializable]
	public struct PlatformOverride
	{
		public Platform.PlatformOverride PlatformToOverride;

		public float OverrideScaleFactor;
	}

	public Platform.PlatformOverride m_TestPlatformOverride;

	[Tooltip("Set to test an override for a different platform to what the editor is set to. If set to unknown then will use the platform the editor is set to. Only works when running in the editor")]
	[SerializeField]
	public List<PlatformOverride> m_PlatformOverrides;

	private Vector3? m_ParentBaseScale;

	private void Start()
	{
		if ((base.transform.parent != null) & !m_ParentBaseScale.HasValue)
		{
			m_ParentBaseScale = base.transform.parent.localScale;
		}
		OverrideParentScale();
	}

	private void OverrideParentScale()
	{
		if (!Application.isPlaying || m_PlatformOverrides.Count <= 0 || !m_ParentBaseScale.HasValue)
		{
			return;
		}
		Platform.PlatformOverride platformOverride = Platform.PlatformOverride.Standalone;
		Vector3 value = m_ParentBaseScale.Value;
		for (int i = 0; i < m_PlatformOverrides.Count; i++)
		{
			PlatformOverride platformOverride2 = m_PlatformOverrides[i];
			if (platformOverride2.PlatformToOverride == platformOverride)
			{
				if (!Mathf.Approximately(platformOverride2.OverrideScaleFactor, 0f))
				{
					value.x *= platformOverride2.OverrideScaleFactor;
					value.y *= platformOverride2.OverrideScaleFactor;
					value.z *= platformOverride2.OverrideScaleFactor;
				}
				break;
			}
		}
		if (base.transform.parent.localScale != value)
		{
			base.transform.parent.localScale = value;
		}
	}
}

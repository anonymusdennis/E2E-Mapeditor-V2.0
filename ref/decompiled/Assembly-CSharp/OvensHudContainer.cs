using System.Collections.Generic;
using UnityEngine;

public class OvensHudContainer : MonoBehaviour
{
	public OvenUI[] m_UIs;

	private List<OvenUI> m_AvailableUIs;

	private List<OvenUI> m_UsedUIs;

	private PerPlayerTrackedUIElements m_ParentPlayerTrackedElements;

	private void Awake()
	{
		m_UIs = GetComponentsInChildren<OvenUI>(includeInactive: true);
		m_AvailableUIs = new List<OvenUI>();
		m_UsedUIs = new List<OvenUI>();
		for (int i = 0; i < m_UIs.Length; i++)
		{
			m_UIs[i].gameObject.SetActive(value: false);
		}
		m_AvailableUIs.AddRange(m_UIs);
		m_ParentPlayerTrackedElements = GetComponentInParent<PerPlayerTrackedUIElements>();
	}

	public OvenUI RequestUI()
	{
		if (m_AvailableUIs.Count > 0)
		{
			OvenUI result = m_AvailableUIs[0];
			m_AvailableUIs.RemoveAt(0);
			base.gameObject.SetActive(value: true);
			return result;
		}
		return null;
	}

	public void ReleaseUI(OvenUI ui)
	{
		m_UsedUIs.Remove(ui);
		m_AvailableUIs.Add(ui);
	}

	public CameraManager.PlayerBindingID GetCameraBinding()
	{
		if (m_ParentPlayerTrackedElements == null)
		{
			m_ParentPlayerTrackedElements = GetComponentInParent<PerPlayerTrackedUIElements>();
		}
		if (m_ParentPlayerTrackedElements != null)
		{
			return m_ParentPlayerTrackedElements.GetBinding();
		}
		return CameraManager.PlayerBindingID.CM_PBID_UNSET;
	}
}

using UnityEngine;

public class LevelEditorUISetup : MonoBehaviour
{
	private IT17EventHelper[] m_EventHelperInterfaces;

	public T17TabPanel m_FirstCatalogueTab;

	public T17TabPanel m_FirstLayerTab;

	private void Start()
	{
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (m_EventHelperInterfaces != null)
		{
			int num = m_EventHelperInterfaces.Length;
			T17EventSystem gamersEventSystem = null;
			if (primaryGamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(primaryGamer);
			}
			for (int i = 0; i < num; i++)
			{
				if (m_EventHelperInterfaces[i] != null && primaryGamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(primaryGamer, gamersEventSystem);
				}
			}
		}
		if (m_FirstCatalogueTab != null)
		{
			m_FirstCatalogueTab.Show(primaryGamer, null, null, hideInvoker: false);
		}
		if (m_FirstLayerTab != null)
		{
			m_FirstLayerTab.Show(primaryGamer, null, null, hideInvoker: false);
			m_FirstLayerTab.SetTabIndex(m_FirstLayerTab.m_Buttons.Length - 1);
		}
	}
}

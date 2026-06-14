public class DLCBreadcrumbManager : T17MonoBehaviour
{
	public enum BreadcrumbTypes
	{
		None,
		DLCStore,
		Prison
	}

	public DLCStoreFrontendMenu m_DLCStoreMenu;

	public static DLCBreadcrumbManager m_Instance;

	private DLCBreadcrumb[] m_Breadcrumbs;

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		m_Breadcrumbs = null;
		m_Breadcrumbs = GetComponentsInChildren<DLCBreadcrumb>(includeInactive: true);
		UpdateVisibleBreadcrumbs();
	}

	protected virtual void OnDestroy()
	{
		m_DLCStoreMenu = null;
		m_Breadcrumbs = null;
		m_Instance = null;
	}

	public void UpdateVisibleBreadcrumbs()
	{
		int num = m_Breadcrumbs.Length;
		if (num <= 0 || !(m_DLCStoreMenu != null) || !(GlobalSave.GetInstance() != null))
		{
			return;
		}
		bool value = false;
		int count = m_DLCStoreMenu.m_DLCList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!m_DLCStoreMenu.m_DLCList[i].m_bFreeDLC)
			{
				GlobalSave.GetInstance().Get("DLC:Breadcrumb" + m_DLCStoreMenu.m_DLCList[i].m_DLCID, out value, def: false);
				if (!value)
				{
					break;
				}
			}
		}
		bool active = !value;
		for (int j = 0; j < num; j++)
		{
			if (m_Breadcrumbs[j] != null)
			{
				switch (m_Breadcrumbs[j].m_DLCType)
				{
				case BreadcrumbTypes.Prison:
					m_Breadcrumbs[j].gameObject.SetActive(!value);
					break;
				case BreadcrumbTypes.DLCStore:
					m_Breadcrumbs[j].gameObject.SetActive(active);
					break;
				default:
					m_Breadcrumbs[j].gameObject.SetActive(value: false);
					break;
				}
			}
		}
	}

	public void SetSeenDLC(DLCFrontendData dlcData)
	{
		if (dlcData != null && GlobalSave.GetInstance() != null)
		{
			bool value = false;
			GlobalSave.GetInstance().Get("DLC:Breadcrumb" + dlcData.m_DLCID, out value, def: false);
			if (!value)
			{
				GlobalSave.GetInstance().Set("DLC:Breadcrumb" + dlcData.m_DLCID, value: true);
				GlobalSave.GetInstance().RequestSave();
			}
			UpdateVisibleBreadcrumbs();
		}
	}
}

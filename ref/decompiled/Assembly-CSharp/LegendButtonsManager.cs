using UnityEngine;

public class LegendButtonsManager : T17MonoBehaviour
{
	public GameObject m_RootObjectToSearchUnder;

	public GameObject m_Accept;

	public GameObject m_Back;

	public GameObject m_CycleTabs;

	public GameObject m_CyclePages;

	private bool m_bNeedUpdate;

	private IMenuEventDelegate m_MenuChangerComponent;

	protected override void Awake()
	{
		base.Awake();
		if (m_RootObjectToSearchUnder == null)
		{
			m_MenuChangerComponent = GetComponentInParent<IMenuEventDelegate>();
			m_RootObjectToSearchUnder = ((Component)m_MenuChangerComponent).gameObject;
		}
		if (!(m_RootObjectToSearchUnder == null))
		{
			m_MenuChangerComponent = m_RootObjectToSearchUnder.GetComponent<IMenuEventDelegate>();
			if (m_MenuChangerComponent != null)
			{
				m_MenuChangerComponent.MenuChangedEvent += m_RootObjectToSearchUnder_MenuChangedEvent;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_MenuChangerComponent != null)
		{
			m_MenuChangerComponent.MenuChangedEvent -= m_RootObjectToSearchUnder_MenuChangedEvent;
			m_MenuChangerComponent = null;
		}
		m_RootObjectToSearchUnder = null;
		m_Accept = null;
		m_Back = null;
		m_CycleTabs = null;
		m_CyclePages = null;
	}

	private void m_RootObjectToSearchUnder_MenuChangedEvent()
	{
		m_bNeedUpdate = true;
	}

	private void Start()
	{
		if (m_Accept != null)
		{
			m_Accept.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (m_bNeedUpdate)
		{
			CheckForElements();
			m_bNeedUpdate = false;
		}
	}

	private void CheckForElements()
	{
		if (!(m_RootObjectToSearchUnder == null))
		{
			SetGameobjectStateForActiveComponent<NavigateOnUICancel>(m_Back);
			SetGameobjectStateForActiveComponent<T17TabPanel>(m_CycleTabs);
			SetGameobjectStateForActiveComponent<T17ScrollView>(m_CyclePages);
		}
	}

	private void SetGameobjectStateForActiveComponent<T>(GameObject targetGo)
	{
		if (targetGo != null)
		{
			T componentInChildren = m_RootObjectToSearchUnder.GetComponentInChildren<T>();
			targetGo.SetActive(componentInChildren != null);
		}
	}

	private void OnEnable()
	{
		CheckForElements();
	}

	public void RequestButtonUptate()
	{
		m_bNeedUpdate = true;
	}
}

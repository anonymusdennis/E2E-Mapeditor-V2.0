using UnityEngine;
using UnityEngine.UI;

public class HUD : T17MonoBehaviour
{
	private static HUD m_Instance;

	public Text[] m_NamePlateTexts;

	public Image[] m_NamePlatePanels;

	public Canvas m_Canvas;

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		for (int i = 0; i < m_NamePlateTexts.Length; i++)
		{
			if (m_NamePlatePanels[i] != null)
			{
				m_NamePlatePanels[i].gameObject.SetActive(value: false);
			}
		}
		m_Instance = null;
	}

	public static HUD GetHUD()
	{
		return m_Instance;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		return base.StartInit();
	}

	private void Update()
	{
		if (!IsInited())
		{
		}
	}
}

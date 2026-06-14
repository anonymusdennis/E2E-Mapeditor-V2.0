using UnityEngine;

public class LevelEditor_BaseTab : MonoBehaviour
{
	private LevelEditor_BlockSection[] m_Sections = new LevelEditor_BlockSection[0];

	private BuildingBlockManager m_BlockMan;

	private BaseLevelManager m_LevelMan;

	private BaseLevelManager.LevelLayers m_CurrentLayer = BaseLevelManager.LevelLayers.TOTAL;

	private void Awake()
	{
		m_Sections = GetComponentsInChildren<LevelEditor_BlockSection>(includeInactive: true);
		m_LevelMan = BaseLevelManager.GetInstance();
		m_BlockMan = BuildingBlockManager.GetInstance();
	}

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		if (m_BlockMan == null)
		{
			m_BlockMan = BuildingBlockManager.GetInstance();
		}
		if (m_BlockMan == null)
		{
			return;
		}
		if (m_LevelMan == null)
		{
			m_LevelMan = BaseLevelManager.GetInstance();
		}
		if (!(m_LevelMan == null) && m_LevelMan.GetCurrentLayer() != m_CurrentLayer)
		{
			int i = 0;
			for (int num = m_Sections.Length; i < num; i++)
			{
				m_Sections[i].UpdateContent();
			}
			m_CurrentLayer = m_LevelMan.GetCurrentLayer();
		}
	}
}

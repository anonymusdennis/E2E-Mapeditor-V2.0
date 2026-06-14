public class LevelSetup_FloorFacade : BaseComponentSetup
{
	public FacadeFloor m_FacadeFloor;

	public BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.GroundFloor;

	public override SetupReturnState Setup()
	{
		if (m_FacadeFloor != null)
		{
			m_FacadeFloor = GetComponent<FacadeFloor>();
			if (m_FacadeFloor == null)
			{
				return FinishedAndRemove();
			}
		}
		if (m_FacadeFloor.m_FloorWidth != 120 || m_FacadeFloor.m_FloorHeight != 120)
		{
			return FinishedAndRemove();
		}
		if ((int)m_Layer < 1 || (int)m_Layer > 5)
		{
			return FinishedAndRemove();
		}
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		if (m_FacadeFloor.m_FloorMap.Length != 14400)
		{
			m_FacadeFloor.m_FloorMap = new int[14400];
		}
		BaseLevelManager.TileProperty[] tileProperties = instance.m_BuildingLayers[(uint)m_Layer].m_TileProperties;
		int num = 0;
		for (int num2 = 119; num2 >= 0; num2--)
		{
			for (int i = 0; i < 120; i++)
			{
				if ((tileProperties[num++] & BaseLevelManager.TileProperty.TileExistsMask) == BaseLevelManager.TileProperty.TileExistsMask)
				{
					m_FacadeFloor.FloorMap(i, num2, (int)m_Layer);
				}
				else
				{
					m_FacadeFloor.FloorMap(i, num2, 0);
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_2;
	}
}

public class LevelSetup_ConfigSetup : BaseComponentSetup
{
	public LevelScript m_LevelScript;

	public PrisonConfig m_Config_Camp_Easy;

	public PrisonConfig m_Config_Camp_Medium;

	public PrisonConfig m_Config_Camp_Hard;

	public PrisonConfig m_Config_VS_Easy;

	public PrisonConfig m_Config_VS_Medium;

	public PrisonConfig m_Config_VS_Hard;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_1;
	}

	public override SetupReturnState Setup()
	{
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

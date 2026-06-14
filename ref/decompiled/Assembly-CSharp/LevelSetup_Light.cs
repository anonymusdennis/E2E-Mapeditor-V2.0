using System.Collections.Generic;

public class LevelSetup_Light : BaseComponentSetup
{
	public enum LightingGroups
	{
		CellLights_Group,
		DeadRoom_Light_Group,
		Generator_Light_Group,
		JobRoom_Light_Group,
		EntranceAndOffice_Light_Group,
		GuardsRoom_Cell_Light_Group,
		GuardRoom_Light_Group,
		WardensRoom_Light_Group,
		Kennels_Light_Group,
		Kitchen_Light_Group,
		Hospital_Light_Group,
		RecRoom_Light_Group,
		Courtyard_Light_Group,
		MultiplayerRoom_Light_Group,
		Corridoor_Light_Group,
		Security_Light_Group,
		RoofEntrance_Light_Group,
		TOTAL
	}

	public static LightingManager c_LightManager;

	public List<LightingGroups> m_GroupsWeAreIn = new List<LightingGroups>();

	public LightControl m_LightControl;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_4;
	}

	public override SetupReturnState Setup()
	{
		if (c_LightManager == null)
		{
			c_LightManager = GetClassInstance<LightingManager>();
			if (c_LightManager == null)
			{
				return FinishedAndRemove();
			}
		}
		if (m_LightControl == null)
		{
			return FinishedAndRemove();
		}
		m_LightControl.m_Groups.Clear();
		m_LightControl.enabled = true;
		for (int num = m_GroupsWeAreIn.Count - 1; num >= 0; num--)
		{
			int groupIndex = c_LightManager.GetGroupIndex(m_GroupsWeAreIn[num].ToString());
			if (groupIndex != -1)
			{
				c_LightManager.AddLightToGroup_Index(m_LightControl, groupIndex);
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}

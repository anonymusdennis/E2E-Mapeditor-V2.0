using System.Collections.Generic;

public class BuildingBlock_Complex : BuildingBlock_Room
{
	public bool m_ZoneObject;

	public List<int> m_InBlockGroups = new List<int>();

	public override BuildingBlockType BlockType => BuildingBlockType.Complex;
}

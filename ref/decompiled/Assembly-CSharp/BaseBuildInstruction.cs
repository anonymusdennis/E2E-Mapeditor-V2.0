using System;

[Serializable]
public class BaseBuildInstruction
{
	public enum InstructionTypeEnum : byte
	{
		UNKNOWN,
		Draw_Once,
		Draw_Area,
		Complex,
		ChangeLayer,
		ChangeEnvironment,
		Command,
		Draw_OnceWall,
		Draw_AreaWall,
		Delete,
		PreventUndo,
		IncrementLayer,
		DecrementLayer,
		Zone
	}

	public InstructionTypeEnum m_Type;

	public int m_BuildingBrickID = -1;

	public sbyte m_XPosition;

	public sbyte m_YPosition;

	public int m_iRandomSeed;

	public sbyte m_XCount;

	public sbyte m_YCount;

	public BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.GroundFloor;

	public bool m_bInside = true;

	public byte[] m_ZonePrint = new byte[0];

	public InstructionTypeEnum InstructionType => m_Type;

	public static BaseBuildInstruction CreateOnce(sbyte xPos, sbyte yPos, int blockID, int seed)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.Draw_Once;
		baseBuildInstruction.m_XPosition = xPos;
		baseBuildInstruction.m_YPosition = yPos;
		baseBuildInstruction.m_BuildingBrickID = blockID;
		baseBuildInstruction.m_iRandomSeed = seed;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateOnceWall(sbyte xPos, sbyte yPos, int blockID, int seed)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.Draw_OnceWall;
		baseBuildInstruction.m_XPosition = xPos;
		baseBuildInstruction.m_YPosition = yPos;
		baseBuildInstruction.m_BuildingBrickID = blockID;
		baseBuildInstruction.m_iRandomSeed = seed;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateZone(ZoneDetailsManager.ZoneTypes eType, int xPos, int yPos, int width, int height, byte[] zonePrint, int iID = -1)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.Zone;
		baseBuildInstruction.m_XPosition = (sbyte)xPos;
		baseBuildInstruction.m_YPosition = (sbyte)yPos;
		baseBuildInstruction.m_XCount = (sbyte)width;
		baseBuildInstruction.m_YCount = (sbyte)height;
		baseBuildInstruction.m_iRandomSeed = (int)eType;
		baseBuildInstruction.m_bInside = true;
		baseBuildInstruction.m_ZonePrint = zonePrint;
		baseBuildInstruction.m_BuildingBrickID = iID;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction DeleteZone(sbyte xPos, sbyte yPos)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.Zone;
		baseBuildInstruction.m_XPosition = xPos;
		baseBuildInstruction.m_YPosition = yPos;
		baseBuildInstruction.m_bInside = false;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateArea(sbyte xPos, sbyte yPos, sbyte xCount, sbyte yCount, int blockID, int seed)
	{
		BaseBuildInstruction baseBuildInstruction = CreateOnce(xPos, yPos, blockID, seed);
		baseBuildInstruction.m_Type = InstructionTypeEnum.Draw_Area;
		baseBuildInstruction.m_XCount = xCount;
		baseBuildInstruction.m_YCount = yCount;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateAreaWall(sbyte xPos, sbyte yPos, sbyte xCount, sbyte yCount, int blockID, int seed)
	{
		BaseBuildInstruction baseBuildInstruction = CreateOnce(xPos, yPos, blockID, seed);
		baseBuildInstruction.m_Type = InstructionTypeEnum.Draw_AreaWall;
		baseBuildInstruction.m_XCount = xCount;
		baseBuildInstruction.m_YCount = yCount;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateLayerChange(BaseLevelManager.LevelLayers layer)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.ChangeLayer;
		baseBuildInstruction.m_Layer = layer;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction IncrementLayer()
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.IncrementLayer;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction DecrementLayer()
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.DecrementLayer;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateChangeEnvironment(bool bInside)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = InstructionTypeEnum.ChangeEnvironment;
		baseBuildInstruction.m_bInside = bInside;
		return baseBuildInstruction;
	}

	public static BaseBuildInstruction CreateFromInstruction(BaseBuildInstruction sourceInstruction)
	{
		BaseBuildInstruction baseBuildInstruction = new BaseBuildInstruction();
		baseBuildInstruction.m_Type = sourceInstruction.m_Type;
		baseBuildInstruction.m_BuildingBrickID = sourceInstruction.m_BuildingBrickID;
		baseBuildInstruction.m_XPosition = sourceInstruction.m_XPosition;
		baseBuildInstruction.m_YPosition = sourceInstruction.m_YPosition;
		baseBuildInstruction.m_iRandomSeed = sourceInstruction.m_iRandomSeed;
		baseBuildInstruction.m_XCount = sourceInstruction.m_XCount;
		baseBuildInstruction.m_YCount = sourceInstruction.m_YCount;
		baseBuildInstruction.m_Layer = sourceInstruction.m_Layer;
		baseBuildInstruction.m_bInside = sourceInstruction.m_bInside;
		int num = sourceInstruction.m_ZonePrint.Length;
		baseBuildInstruction.m_ZonePrint = new byte[num];
		for (int i = 0; i < num; i++)
		{
			baseBuildInstruction.m_ZonePrint[i] = sourceInstruction.m_ZonePrint[i];
		}
		return baseBuildInstruction;
	}
}

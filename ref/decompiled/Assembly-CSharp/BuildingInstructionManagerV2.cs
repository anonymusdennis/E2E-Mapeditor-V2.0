public class BuildingInstructionManagerV2 : BuildingInstructionManager
{
	public override bool AddFromInstructionsBlock(BuildingBlock_Room obj, sbyte X, sbyte Y, int seed, bool bCheckLimits = false)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease || obj.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
		{
			return base.AddFromInstructionsBlock(obj, X, Y, seed);
		}
		if (obj == null)
		{
			return false;
		}
		AddStartUndo();
		int count = obj.m_BlockInstructions.Count;
		for (int i = 0; i < count; i++)
		{
			BaseBuildInstruction baseBuildInstruction = obj.m_BlockInstructions[i];
			switch (baseBuildInstruction.m_Type)
			{
			case BaseBuildInstruction.InstructionTypeEnum.ChangeEnvironment:
				ChangeEnvironment(baseBuildInstruction.m_bInside);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.ChangeLayer:
				ChangeLayer(baseBuildInstruction.m_Layer);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.IncrementLayer:
				IncrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.DecrementLayer:
				DecrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
			case BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall:
				AddBlockOnce(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_iRandomSeed, bDontRun: false, bCheckLimits);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
			case BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall:
				AddBlockArea(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Complex:
				AddFromInstructionsBlock(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Zone:
				if (baseBuildInstruction.m_bInside)
				{
					CreateZone((ZoneDetailsManager.ZoneTypes)baseBuildInstruction.m_iRandomSeed, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_ZonePrint);
				}
				break;
			}
		}
		AddEndUndo();
		return true;
	}

	public bool AddFromCopyBlock(BuildingBlock_Room obj, sbyte X, sbyte Y, int seed)
	{
		if (obj == null)
		{
			return false;
		}
		AddStartUndo();
		int count = obj.m_BlockInstructions.Count;
		for (int i = 0; i < count; i++)
		{
			BaseBuildInstruction baseBuildInstruction = obj.m_BlockInstructions[i];
			switch (baseBuildInstruction.m_Type)
			{
			case BaseBuildInstruction.InstructionTypeEnum.ChangeEnvironment:
				ChangeEnvironment(baseBuildInstruction.m_bInside);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.ChangeLayer:
				ChangeLayer(baseBuildInstruction.m_Layer);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.IncrementLayer:
				IncrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.DecrementLayer:
				DecrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
			case BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall:
				AddBlockOnce(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
			case BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall:
				AddBlockArea(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Complex:
				AddFromInstructionsBlock(baseBuildInstruction.m_BuildingBrickID, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Zone:
				if (baseBuildInstruction.m_bInside)
				{
					CreateZone((ZoneDetailsManager.ZoneTypes)baseBuildInstruction.m_iRandomSeed, (sbyte)(baseBuildInstruction.m_XPosition + X), (sbyte)(baseBuildInstruction.m_YPosition + Y), baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_ZonePrint);
				}
				break;
			}
		}
		AddEndUndo();
		return true;
	}

	public override void RunInstruction(int iIndex, bool bStorePrevious, bool bIncreaseTotals = false)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			base.RunInstruction(iIndex, bStorePrevious, bIncreaseTotals);
			return;
		}
		int currentList = m_CurrentList;
		InstructionList instructionList = null;
		instructionList = ((m_CurrentList != -1) ? m_LevelInstructions.m_ComplexList.m_Instructions[m_CurrentList].m_ComplexInstructions : m_LevelInstructions.m_UserInstructions);
		if (iIndex == -1)
		{
			iIndex = instructionList.m_iTotal - 1;
		}
		if (m_BlockManager != null && m_LevelManager != null && instructionList.m_iTotalValid > iIndex)
		{
			int index = instructionList.m_Instructions[iIndex].m_Index;
			switch (instructionList.m_Instructions[iIndex].m_Type)
			{
			case BaseBuildInstruction.InstructionTypeEnum.Command:
				if (m_LevelInstructions.m_CommandList.m_iTotalValid > index)
				{
					InstructionCommandElement obj7 = m_LevelInstructions.m_CommandList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_CommandList.m_iTotal = index + 1;
					}
					m_LevelManager.AddCommand(ref obj7, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
				if (m_LevelInstructions.m_OnceList.m_iTotalValid > index)
				{
					InstructionOnceElement obj5 = m_LevelInstructions.m_OnceList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_OnceList.m_iTotal = index + 1;
					}
					m_LevelManager.AddSingle(ref obj5, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall:
				if (m_LevelInstructions.m_OnceWallList.m_iTotalValid > index)
				{
					InstructionOnceWallElement obj3 = m_LevelInstructions.m_OnceWallList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_OnceWallList.m_iTotal = index + 1;
					}
					m_LevelManager.AddSingleWall(ref obj3, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
				if (m_LevelInstructions.m_AreaList.m_iTotalValid > index)
				{
					InstructionAreaElement obj6 = m_LevelInstructions.m_AreaList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_AreaList.m_iTotal = index + 1;
					}
					m_LevelManager.AddArea(ref obj6, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall:
				if (m_LevelInstructions.m_AreaWallList.m_iTotalValid > index)
				{
					InstructionAreaWallElement obj2 = m_LevelInstructions.m_AreaWallList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_AreaWallList.m_iTotal = index + 1;
					}
					m_LevelManager.AddAreaWall(ref obj2, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Complex:
				if (bIncreaseTotals)
				{
					m_LevelInstructions.m_ComplexList.m_iTotal = index + 1;
				}
				if (m_LevelInstructions.m_ComplexList.m_iTotalValid > index)
				{
					InstructionComplexElement instructionComplexElement = m_LevelInstructions.m_ComplexList.m_Instructions[index];
					int currentList2 = m_CurrentList;
					m_CurrentList = index;
					int iTotal = instructionComplexElement.m_ComplexInstructions.m_iTotal;
					for (int i = 0; i < iTotal; i++)
					{
						RunInstruction(i, bStorePrevious, bIncreaseTotals);
					}
					m_CurrentList = currentList2;
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Delete:
				if (m_LevelInstructions.m_DeleteList.m_iTotalValid > index)
				{
					InstructionDeleteElement obj4 = m_LevelInstructions.m_DeleteList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_DeleteList.m_iTotal = index + 1;
					}
					m_LevelManager.AddDelete(ref obj4, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Zone:
				if (m_LevelInstructions.m_ZoneList.m_iTotalValid > index)
				{
					InstructionZoneElement obj = m_LevelInstructions.m_ZoneList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_ZoneList.m_iTotal = index + 1;
					}
					switch (obj.m_Action)
					{
					case InstructionZoneElement.ZoneAction.Create:
						m_LevelManager.CreateZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Delete:
						m_LevelManager.DeleteZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Add:
						m_LevelManager.AddToZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Subtract:
						m_LevelManager.SubtractFromZone(ref obj);
						break;
					}
				}
				break;
			}
		}
		m_CurrentList = currentList;
	}

	public override bool ConvertTheLevel()
	{
		m_LevelInstructions.m_FinishedLevel.QuantizeThelevel();
		ResetContents();
		return true;
	}
}

using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class BuildingBlock_Room : BaseBuildingBlock
{
	public enum LabelTypes
	{
		NoLabel,
		RoomsOfThisTypeShare,
		RoomsOfThisTypeNearShare,
		Unique
	}

	public class RepresentationHelper
	{
		public GameObject m_GameObject;

		public BaseBuildingBlock m_Block;

		public int m_BlockID = -1;

		public int m_X;

		public int m_Y;

		public RepresentationHelper(GameObject ourGameObject, BaseBuildingBlock ourBlock, int iBlockID, int X, int Y)
		{
			m_GameObject = ourGameObject;
			m_Block = ourBlock;
			m_BlockID = iBlockID;
			m_X = X;
			m_Y = Y;
		}
	}

	[SerializeField]
	public List<BaseBuildInstruction> m_Instructions = new List<BaseBuildInstruction>();

	[SerializeField]
	public List<BaseBuildInstruction> m_InstructionsV2 = new List<BaseBuildInstruction>();

	[NonSerialized]
	public List<BaseBuildInstruction> m_BlockInstructions;

	private LevelDetailsManager.LevelEditorDataVersion m_InstructionSetVersion;

	public Player_Footsteps m_FloorMaterial;

	public RoomBlob.RoomAffinity m_RoomAffinity = RoomBlob.RoomAffinity.Meh;

	public RoomBlob.RoomAffinity m_RoomAffinityGuard = RoomBlob.RoomAffinity.Meh;

	public RoomBlob.RoomAffinity m_RoomAffinitySupport = RoomBlob.RoomAffinity.Meh;

	public bool m_InmateSafeSpace = true;

	public bool m_GuardSafeSpace;

	public bool m_SupportSafeSpace;

	public RoomBlob.RoomSubIdentity_Rules m_subRules;

	public bool m_bAllowSniping = true;

	public List<int> m_AlternateBlocks = new List<int>();

	public int m_FlipBlock = -1;

	public LabelTypes m_LabelType;

	protected int[] m_SurroundingBlocks = new int[32]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1
	};

	public override BuildingBlockType BlockType => BuildingBlockType.Room;

	protected override void AddToFootprint(Footprint footPrint, int xOffset = 0, int yOffset = 0, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		if (m_Footprint == null)
		{
			m_Footprint = new Footprint(footPrint, xOffset, yOffset, bMultiLevel: true, layer);
		}
		else
		{
			m_Footprint.CombineFootprints(xOffset, yOffset, footPrint, layer);
		}
	}

	protected void AddToFootprint(byte[] bByte, sbyte xPosition, sbyte yPosition, sbyte iWidth, sbyte iHeight, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		Footprint footprint = new Footprint(bByte, iWidth, iHeight, Footprint.BlockTypes.Zone);
		if (m_Footprint == null)
		{
			m_Footprint = new Footprint(footprint, xPosition, yPosition, bMultiLevel: true, layer);
		}
		else
		{
			m_Footprint.CombineFootprints(xPosition, yPosition, footprint, layer);
		}
	}

	public override void MakeVisualRepresentation(int iIndex)
	{
		if (Application.isPlaying)
		{
			InitInstructionSet();
		}
		base.MakeVisualRepresentation(iIndex);
		bool bChangedLayer = false;
		BaseLevelManager.LevelLayers ourLayer = BaseLevelManager.LevelLayers.GroundFloor;
		while ((int)ourLayer < 6)
		{
			int num = 3 << (int)ourLayer * 2;
			if ((m_ValidLayers & num) != 0)
			{
				break;
			}
			ourLayer++;
		}
		BaseLevelManager.LevelLayers currentLayer = ourLayer;
		bool bDraw = true;
		List<RepresentationHelper> TMSObjects = new List<RepresentationHelper>();
		int iMinX = 10000;
		int iMaxX = -10000;
		int iMinY = 10000;
		int iMaxY = -10000;
		int iOffset = 0;
		PassInstructionsForBrush(m_BlockInstructions, ref bChangedLayer, ref bDraw, ref ourLayer, ref currentLayer, ref TMSObjects, ref iMinX, ref iMaxX, ref iMinY, ref iMaxY, ref iOffset, 0, 0);
		if (TMSObjects.Count != 0)
		{
			int num2 = iMaxX - iMinX + 1;
			int num3 = iMaxY - iMinY + 1;
			int num4 = num2 * 4;
			int[] array = new int[num4 * num3];
			for (int num5 = num2 * num3 * 4 - 1; num5 >= 0; num5--)
			{
				if (num5 % 4 == 3)
				{
					array[num5] = 0;
				}
				else
				{
					array[num5] = -1;
				}
			}
			for (int num6 = TMSObjects.Count - 1; num6 >= 0; num6--)
			{
				TMSObjects[num6].m_X -= iMinX;
				TMSObjects[num6].m_Y -= iMinY;
				iOffset = TMSObjects[num6].m_Y * num4 + TMSObjects[num6].m_X * 4;
				switch (TMSObjects[num6].m_Block.BlockType)
				{
				case BuildingBlockType.Tile:
					array[iOffset] = TMSObjects[num6].m_BlockID;
					break;
				case BuildingBlockType.Wall:
					array[iOffset + 1] = TMSObjects[num6].m_BlockID;
					break;
				case BuildingBlockType.Decoration:
				case BuildingBlockType.Object:
					array[iOffset + 2] = TMSObjects[num6].m_BlockID;
					break;
				}
				array[iOffset + 3] |= (int)TMSObjects[num6].m_Block.m_GroupFlags;
			}
			for (int num7 = TMSObjects.Count - 1; num7 >= 0; num7--)
			{
				RepresentationHelper representationHelper = TMSObjects[num7];
				if (representationHelper != null && representationHelper.m_GameObject != null)
				{
					iOffset = representationHelper.m_Y * num4 + representationHelper.m_X * 4;
					bool flag = representationHelper.m_X != 0;
					bool flag2 = representationHelper.m_X < num2 - 1;
					bool flag3 = representationHelper.m_Y < num3 - 1;
					bool flag4 = representationHelper.m_Y != 0;
					if (flag)
					{
						m_SurroundingBlocks[3] = array[iOffset - 4];
						m_SurroundingBlocks[11] = array[iOffset - 3];
						m_SurroundingBlocks[19] = array[iOffset - 2];
						m_SurroundingBlocks[27] = array[iOffset - 1];
						if (flag3)
						{
							m_SurroundingBlocks[0] = array[iOffset - 4 + num4];
							m_SurroundingBlocks[8] = array[iOffset - 3 + num4];
							m_SurroundingBlocks[16] = array[iOffset - 2 + num4];
							m_SurroundingBlocks[24] = array[iOffset - 1 + num4];
						}
						else
						{
							m_SurroundingBlocks[0] = -1;
							m_SurroundingBlocks[8] = -1;
							m_SurroundingBlocks[16] = -1;
							m_SurroundingBlocks[24] = 0;
						}
						if (flag4)
						{
							m_SurroundingBlocks[5] = array[iOffset - 4 - num4];
							m_SurroundingBlocks[13] = array[iOffset - 3 - num4];
							m_SurroundingBlocks[21] = array[iOffset - 2 - num4];
							m_SurroundingBlocks[29] = array[iOffset - 1 - num4];
						}
						else
						{
							m_SurroundingBlocks[5] = -1;
							m_SurroundingBlocks[13] = -1;
							m_SurroundingBlocks[21] = -1;
							m_SurroundingBlocks[29] = 0;
						}
					}
					else
					{
						m_SurroundingBlocks[3] = -1;
						m_SurroundingBlocks[11] = -1;
						m_SurroundingBlocks[19] = -1;
						m_SurroundingBlocks[27] = 0;
						m_SurroundingBlocks[0] = -1;
						m_SurroundingBlocks[8] = -1;
						m_SurroundingBlocks[16] = -1;
						m_SurroundingBlocks[24] = 0;
						m_SurroundingBlocks[5] = -1;
						m_SurroundingBlocks[13] = -1;
						m_SurroundingBlocks[21] = -1;
						m_SurroundingBlocks[29] = 0;
					}
					if (flag2)
					{
						m_SurroundingBlocks[4] = array[iOffset + 4];
						m_SurroundingBlocks[12] = array[iOffset + 5];
						m_SurroundingBlocks[20] = array[iOffset + 6];
						m_SurroundingBlocks[28] = array[iOffset + 7];
						if (flag3)
						{
							m_SurroundingBlocks[2] = array[iOffset + 4 + num4];
							m_SurroundingBlocks[10] = array[iOffset + 5 + num4];
							m_SurroundingBlocks[18] = array[iOffset + 6 + num4];
							m_SurroundingBlocks[26] = array[iOffset + 7 + num4];
						}
						else
						{
							m_SurroundingBlocks[2] = -1;
							m_SurroundingBlocks[10] = -1;
							m_SurroundingBlocks[18] = -1;
							m_SurroundingBlocks[26] = 0;
						}
						if (flag4)
						{
							m_SurroundingBlocks[7] = array[iOffset + 4 - num4];
							m_SurroundingBlocks[15] = array[iOffset + 5 - num4];
							m_SurroundingBlocks[23] = array[iOffset + 6 - num4];
							m_SurroundingBlocks[31] = array[iOffset + 7 - num4];
						}
						else
						{
							m_SurroundingBlocks[7] = -1;
							m_SurroundingBlocks[15] = -1;
							m_SurroundingBlocks[23] = -1;
							m_SurroundingBlocks[31] = 0;
						}
					}
					else
					{
						m_SurroundingBlocks[4] = -1;
						m_SurroundingBlocks[12] = -1;
						m_SurroundingBlocks[20] = -1;
						m_SurroundingBlocks[28] = 0;
						m_SurroundingBlocks[2] = -1;
						m_SurroundingBlocks[10] = -1;
						m_SurroundingBlocks[18] = -1;
						m_SurroundingBlocks[26] = 0;
						m_SurroundingBlocks[7] = -1;
						m_SurroundingBlocks[15] = -1;
						m_SurroundingBlocks[23] = -1;
						m_SurroundingBlocks[31] = 0;
					}
					if (flag3)
					{
						m_SurroundingBlocks[1] = array[iOffset + num4];
						m_SurroundingBlocks[9] = array[iOffset + 1 + num4];
						m_SurroundingBlocks[17] = array[iOffset + 2 + num4];
						m_SurroundingBlocks[25] = array[iOffset + 3 + num4];
					}
					else
					{
						m_SurroundingBlocks[1] = -1;
						m_SurroundingBlocks[9] = -1;
						m_SurroundingBlocks[17] = -1;
						m_SurroundingBlocks[25] = 0;
					}
					if (flag4)
					{
						m_SurroundingBlocks[6] = array[iOffset - num4];
						m_SurroundingBlocks[14] = array[iOffset + 1 - num4];
						m_SurroundingBlocks[22] = array[iOffset + 2 - num4];
						m_SurroundingBlocks[30] = array[iOffset + 3 - num4];
					}
					else
					{
						m_SurroundingBlocks[6] = -1;
						m_SurroundingBlocks[14] = -1;
						m_SurroundingBlocks[22] = -1;
						m_SurroundingBlocks[30] = 0;
					}
					int applicableTMSVariant = ((BuildingBlock_TMS)TMSObjects[num7].m_Block).GetApplicableTMSVariant(m_SurroundingBlocks, 0, bAllowNoFloor: false);
					if (applicableTMSVariant != -1)
					{
						GameObject visualRep = ((BuildingBlock_TMS)TMSObjects[num7].m_Block).GetVisualRep(applicableTMSVariant);
						if (visualRep != null)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(visualRep, m_Representations[0].transform);
							if (TMSObjects[num7].m_Block.BlockType == BuildingBlockType.Wall && gameObject.transform.localScale.y > 1.5f)
							{
								gameObject.transform.localPosition = TMSObjects[num7].m_GameObject.transform.localPosition + new Vector3(0f, 0.5f, 0f);
							}
							else
							{
								gameObject.transform.localPosition = TMSObjects[num7].m_GameObject.transform.localPosition;
							}
							gameObject.transform.localScale = visualRep.transform.localScale;
							gameObject.SetActive(value: true);
							UnityEngine.Object.DestroyImmediate(TMSObjects[num7].m_GameObject);
						}
					}
				}
			}
		}
		if (m_Footprint == null)
		{
			Footprint footPrint = new Footprint(0, 0, 1, 1, Footprint.BlockTypes.Tiles, bMultiLevel: true);
			AddToFootprint(footPrint, 0, 0, currentLayer);
		}
		m_Footprint.NormaliseFootPrint(bChangedLayer);
	}

	public void PassInstructionsForBrush(List<BaseBuildInstruction> instructions, ref bool bChangedLayer, ref bool bDraw, ref BaseLevelManager.LevelLayers ourLayer, ref BaseLevelManager.LevelLayers currentLayer, ref List<RepresentationHelper> TMSObjects, ref int iMinX, ref int iMaxX, ref int iMinY, ref int iMaxY, ref int iOffset, int iXPosOffset, int iYPosOffset)
	{
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			int num = iXPosOffset;
			int num2 = iYPosOffset;
			BaseBuildingBlock baseBuildingBlock = null;
			BaseBuildingBlock baseBuildingBlock2 = null;
			int num3 = 1;
			int num4 = 1;
			BaseBuildInstruction baseBuildInstruction = instructions[i];
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.ChangeLayer)
			{
				bChangedLayer = true;
				bDraw = baseBuildInstruction.m_Layer == ourLayer;
				currentLayer = baseBuildInstruction.m_Layer;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.IncrementLayer)
			{
				bChangedLayer = true;
				currentLayer++;
				bDraw = currentLayer == ourLayer;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.DecrementLayer)
			{
				bChangedLayer = true;
				currentLayer--;
				bDraw = currentLayer == ourLayer;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Zone)
			{
				AddToFootprint(baseBuildInstruction.m_ZonePrint, (sbyte)(iXPosOffset + baseBuildInstruction.m_XPosition), (sbyte)(iYPosOffset + baseBuildInstruction.m_YPosition), baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, currentLayer);
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_Once || baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall)
			{
				baseBuildingBlock = BuildingBlockManager.GetBlock(baseBuildInstruction.m_BuildingBrickID);
				if (baseBuildingBlock.BlockType == BuildingBlockType.Wall)
				{
					BuildingBlock_Wall buildingBlock_Wall = baseBuildingBlock as BuildingBlock_Wall;
					if (buildingBlock_Wall != null && buildingBlock_Wall.m_FloorTileID != -1)
					{
						baseBuildingBlock2 = baseBuildingBlock;
						baseBuildingBlock = BuildingBlockManager.GetBlock(buildingBlock_Wall.m_FloorTileID);
					}
				}
				num = baseBuildInstruction.m_XPosition + iXPosOffset;
				num2 = baseBuildInstruction.m_YPosition + iYPosOffset;
			}
			else if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_Area || baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall)
			{
				baseBuildingBlock = BuildingBlockManager.GetBlock(baseBuildInstruction.m_BuildingBrickID);
				if (baseBuildingBlock.BlockType == BuildingBlockType.Wall)
				{
					BuildingBlock_Wall buildingBlock_Wall2 = baseBuildingBlock as BuildingBlock_Wall;
					if (buildingBlock_Wall2 != null && buildingBlock_Wall2.m_FloorTileID != -1)
					{
						baseBuildingBlock2 = baseBuildingBlock;
						baseBuildingBlock = BuildingBlockManager.GetBlock(buildingBlock_Wall2.m_FloorTileID);
					}
				}
				num = baseBuildInstruction.m_XPosition + iXPosOffset;
				num2 = baseBuildInstruction.m_YPosition + iYPosOffset;
				num3 = baseBuildInstruction.m_XCount;
				num4 = baseBuildInstruction.m_YCount;
			}
			if (baseBuildingBlock.BlockType == BuildingBlockType.Complex || baseBuildingBlock.BlockType == BuildingBlockType.Room)
			{
				BuildingBlock_Room buildingBlock_Room = baseBuildingBlock as BuildingBlock_Room;
				PassInstructionsForBrush(buildingBlock_Room.m_BlockInstructions, ref bChangedLayer, ref bDraw, ref ourLayer, ref currentLayer, ref TMSObjects, ref iMinX, ref iMaxX, ref iMinY, ref iMaxY, ref iOffset, baseBuildInstruction.m_XPosition + iXPosOffset, baseBuildInstruction.m_YPosition + iYPosOffset);
				baseBuildingBlock = null;
			}
			while (baseBuildingBlock != null && baseBuildingBlock.GetVisualRep(0) != null)
			{
				GameObject defaultRepresentation = baseBuildingBlock.GetDefaultRepresentation();
				if (defaultRepresentation != null)
				{
					Vector3 localPosition = defaultRepresentation.transform.localPosition;
					for (int j = 0; j < num3; j++)
					{
						for (int k = 0; k < num4; k++)
						{
							if (bDraw)
							{
								GameObject gameObject = UnityEngine.Object.Instantiate(defaultRepresentation, m_Representations[0].transform);
								float num5 = ((baseBuildingBlock.BlockType != BuildingBlockType.Tile) ? 0f : 0.5f);
								num5 = baseBuildingBlock.BlockType switch
								{
									BuildingBlockType.Tile => 0.01f, 
									BuildingBlockType.Decoration => 0.005f, 
									_ => (100f - ((float)(num2 + k) + localPosition.y)) / -50f * 3f, 
								};
								gameObject.transform.localPosition = new Vector3((float)(num + j) + localPosition.x, (float)(num2 + k) + localPosition.y, num5);
								if (baseBuildingBlock.BlockType == BuildingBlockType.Tile || baseBuildingBlock.BlockType == BuildingBlockType.Wall)
								{
									if (j + num < iMinX)
									{
										iMinX = j + num;
									}
									if (j + num > iMaxX)
									{
										iMaxX = j + num;
									}
									if (k + num2 < iMinY)
									{
										iMinY = k + num2;
									}
									if (k + num2 > iMaxY)
									{
										iMaxY = k + num2;
									}
									RepresentationHelper representationHelper = null;
									representationHelper = new RepresentationHelper(gameObject, baseBuildingBlock, baseBuildingBlock.m_ID, j + num, k + num2);
									TMSObjects.Add(representationHelper);
								}
								else if (baseBuildingBlock.BlockType == BuildingBlockType.Decoration || baseBuildingBlock.BlockType == BuildingBlockType.Object)
								{
									int num6 = j + baseBuildingBlock.m_Footprint.m_iLeft;
									int num7 = k + baseBuildingBlock.m_Footprint.m_iBottom;
									for (int l = 0; l < baseBuildingBlock.m_Footprint.m_iH; l++)
									{
										for (int m = 0; m < baseBuildingBlock.m_Footprint.m_iW; m++)
										{
											if (num6 + num + m < iMinX)
											{
												iMinX = num6 + num + m;
											}
											if (num6 + num + m > iMaxX)
											{
												iMaxX = num6 + num + m;
											}
											if (num7 + num2 + l < iMinY)
											{
												iMinY = num7 + num2 + l;
											}
											if (num7 + num2 + l > iMaxY)
											{
												iMaxY = num7 + num2 + l;
											}
											RepresentationHelper representationHelper2 = null;
											representationHelper2 = new RepresentationHelper(null, baseBuildingBlock, baseBuildingBlock.m_ID, num6 + num + m, num7 + num2 + l);
											TMSObjects.Add(representationHelper2);
										}
									}
								}
							}
							AddToFootprint(baseBuildingBlock.m_Footprint, num + j, num2 + k, currentLayer);
						}
					}
				}
				baseBuildingBlock = baseBuildingBlock2;
				baseBuildingBlock2 = null;
			}
		}
	}

	public void CreatFootprint(List<BaseBuildInstruction> instructions = null)
	{
		if (instructions == null)
		{
			InitInstructionSet();
			instructions = m_BlockInstructions;
		}
		m_Footprint = null;
		bool bChangedLayer = false;
		BaseLevelManager.LevelLayers levelLayers = BaseLevelManager.LevelLayers.GroundFloor;
		while ((int)levelLayers < 6)
		{
			int num = 3 << (int)levelLayers * 2;
			if ((m_ValidLayers & num) != 0)
			{
				break;
			}
			levelLayers++;
		}
		BaseLevelManager.LevelLayers levelLayers2 = levelLayers;
		int count = instructions.Count;
		for (int i = 0; i < count; i++)
		{
			int num2 = 0;
			int num3 = 0;
			BaseBuildingBlock baseBuildingBlock = null;
			BaseBuildingBlock baseBuildingBlock2 = null;
			int num4 = 1;
			int num5 = 1;
			BaseBuildInstruction baseBuildInstruction = instructions[i];
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.ChangeLayer)
			{
				bChangedLayer = true;
				levelLayers2 = baseBuildInstruction.m_Layer;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.IncrementLayer)
			{
				bChangedLayer = true;
				levelLayers2++;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.DecrementLayer)
			{
				bChangedLayer = true;
				levelLayers2--;
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Zone)
			{
				AddToFootprint(baseBuildInstruction.m_ZonePrint, baseBuildInstruction.m_XPosition, baseBuildInstruction.m_YPosition, baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, levelLayers2);
				continue;
			}
			if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_Once || baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall)
			{
				baseBuildingBlock = BuildingBlockManager.GetBlock(baseBuildInstruction.m_BuildingBrickID);
				if (baseBuildingBlock.BlockType == BuildingBlockType.Wall)
				{
					BuildingBlock_Wall buildingBlock_Wall = baseBuildingBlock as BuildingBlock_Wall;
					if (buildingBlock_Wall != null && buildingBlock_Wall.m_FloorTileID != -1)
					{
						baseBuildingBlock2 = baseBuildingBlock;
						baseBuildingBlock = BuildingBlockManager.GetBlock(buildingBlock_Wall.m_FloorTileID);
					}
				}
				num2 = baseBuildInstruction.m_XPosition;
				num3 = baseBuildInstruction.m_YPosition;
			}
			else if (baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_Area || baseBuildInstruction.InstructionType == BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall)
			{
				baseBuildingBlock = BuildingBlockManager.GetBlock(baseBuildInstruction.m_BuildingBrickID);
				if (baseBuildingBlock.BlockType == BuildingBlockType.Wall)
				{
					BuildingBlock_Wall buildingBlock_Wall2 = baseBuildingBlock as BuildingBlock_Wall;
					if (buildingBlock_Wall2 != null && buildingBlock_Wall2.m_FloorTileID != -1)
					{
						baseBuildingBlock2 = baseBuildingBlock;
						baseBuildingBlock = BuildingBlockManager.GetBlock(buildingBlock_Wall2.m_FloorTileID);
					}
				}
				num2 = baseBuildInstruction.m_XPosition;
				num3 = baseBuildInstruction.m_YPosition;
				num4 = baseBuildInstruction.m_XCount;
				num5 = baseBuildInstruction.m_YCount;
			}
			while (baseBuildingBlock != null)
			{
				for (int j = 0; j < num4; j++)
				{
					for (int k = 0; k < num5; k++)
					{
						AddToFootprint(baseBuildingBlock.m_Footprint, num2 + j, num3 + k, levelLayers2);
					}
				}
				baseBuildingBlock = baseBuildingBlock2;
				baseBuildingBlock2 = null;
			}
		}
		if (m_Footprint == null)
		{
			Footprint footPrint = new Footprint(0, 0, 1, 1, Footprint.BlockTypes.Tiles, bMultiLevel: true);
			AddToFootprint(footPrint, 0, 0, levelLayers2);
		}
		m_Footprint.NormaliseFootPrint(bChangedLayer);
	}

	public override CompletionState GetBlockCompletionState(ref string strProblems, bool bCreateErrorString = false)
	{
		CompletionState result = base.GetBlockCompletionState(ref strProblems, bCreateErrorString);
		if (BlockType == BuildingBlockType.Room && m_LimitationGroup == -1)
		{
			if (bCreateErrorString)
			{
				strProblems += "Rooms must be in a limitation group so the game will know what they are\n";
			}
			result = CompletionState.Unfinished;
		}
		if (m_BlockInstructions == null)
		{
			InitInstructionSet();
		}
		if (m_BlockInstructions.Count == 0)
		{
			if (bCreateErrorString)
			{
				strProblems += "There are no Instructions\n";
			}
			result = CompletionState.Unfinished;
		}
		return result;
	}

	public BaseLevelManager.LevelLayers GetOtherLayer(BaseLevelManager.LevelLayers originalLayer)
	{
		BaseLevelManager.LevelLayers levelLayers = originalLayer;
		for (int i = 0; i < m_BlockInstructions.Count; i++)
		{
			if (m_BlockInstructions[i].m_Type == BaseBuildInstruction.InstructionTypeEnum.ChangeLayer)
			{
				levelLayers = m_BlockInstructions[i].m_Layer;
			}
			else if (m_BlockInstructions[i].m_Type == BaseBuildInstruction.InstructionTypeEnum.IncrementLayer && levelLayers != BaseLevelManager.LevelLayers.TOTAL)
			{
				levelLayers++;
			}
			else if (m_BlockInstructions[i].m_Type == BaseBuildInstruction.InstructionTypeEnum.DecrementLayer && levelLayers != BaseLevelManager.LevelLayers.TOTAL)
			{
				levelLayers--;
			}
			else if (originalLayer != levelLayers)
			{
				break;
			}
		}
		if (!IsValidForLayer(levelLayers))
		{
			return levelLayers;
		}
		return BaseLevelManager.LevelLayers.TOTAL;
	}

	protected override void OnDrawGizmosSelected()
	{
		if (BlockType == BuildingBlockType.Complex || LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			base.OnDrawGizmosSelected();
		}
		if (m_BlockInstructions == null)
		{
			InitInstructionSet();
		}
		int count = m_BlockInstructions.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_BlockInstructions[i] == null || m_BlockInstructions[i].m_Type != BaseBuildInstruction.InstructionTypeEnum.Zone)
			{
				continue;
			}
			float num = m_BlockInstructions[i].m_XCount;
			float num2 = m_BlockInstructions[i].m_YCount;
			float num3 = m_BlockInstructions[i].m_XPosition;
			float num4 = m_BlockInstructions[i].m_YPosition;
			int num5 = 0;
			int num6 = 0;
			int num7 = (int)num;
			int num8 = (int)num2;
			for (int j = 0; j < num8; j++)
			{
				for (int k = 0; k < num7; k++)
				{
					byte b = (byte)(1 << num5);
					if ((m_BlockInstructions[i].m_ZonePrint[num6] & b) != 0)
					{
						if (j + 1 < (int)num2)
						{
							int num9 = (j + 1) * num7 + k;
							int num10 = num9 / 8;
							num9 %= 8;
							b = (byte)(1 << num9);
							if ((m_BlockInstructions[i].m_ZonePrint[num10] & b) == 0)
							{
								Gizmos.color = Color.black;
								Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k, num4 + (float)j + 0.5f, -12f), new Vector3(1f, 0.2f, 1f));
							}
						}
						else
						{
							Gizmos.color = Color.black;
							Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k, num4 + (float)j + 0.5f, -12f), new Vector3(1f, 0.2f, 1f));
						}
						if (j > 0)
						{
							int num11 = (j - 1) * num7 + k;
							int num12 = num11 / 8;
							num11 %= 8;
							b = (byte)(1 << num11);
							if ((m_BlockInstructions[i].m_ZonePrint[num12] & b) == 0)
							{
								Gizmos.color = Color.black;
								Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k, num4 + (float)j - 0.5f, -12f), new Vector3(1f, 0.2f, 1f));
							}
						}
						else
						{
							Gizmos.color = Color.black;
							Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k, num4 + (float)j - 0.5f, -12f), new Vector3(1f, 0.2f, 1f));
						}
						if (k + 1 < (int)num)
						{
							int num13 = j * num7 + k + 1;
							int num14 = num13 / 8;
							num13 %= 8;
							b = (byte)(1 << num13);
							if ((m_BlockInstructions[i].m_ZonePrint[num14] & b) == 0)
							{
								Gizmos.color = Color.black;
								Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k + 0.5f, num4 + (float)j, -12f), new Vector3(0.2f, 1f, 1f));
							}
						}
						else
						{
							Gizmos.color = Color.black;
							Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k + 0.5f, num4 + (float)j, -12f), new Vector3(0.2f, 1f, 1f));
						}
						if (k > 0)
						{
							int num15 = j * num7 + k - 1;
							int num16 = num15 / 8;
							num15 %= 8;
							b = (byte)(1 << num15);
							if ((m_BlockInstructions[i].m_ZonePrint[num16] & b) == 0)
							{
								Gizmos.color = Color.black;
								Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k - 0.5f, num4 + (float)j, -12f), new Vector3(0.2f, 1f, 1f));
							}
						}
						else
						{
							Gizmos.color = Color.black;
							Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k - 0.5f, num4 + (float)j, -12f), new Vector3(0.2f, 1f, 1f));
						}
						Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
						Gizmos.DrawCube(base.transform.position + new Vector3(num3 + (float)k, num4 + (float)j, -11f), new Vector3(1f, 1f, 1f));
					}
					if (++num5 == 8)
					{
						num5 = 0;
						num6++;
					}
				}
			}
			break;
		}
	}

	public static void AutoAddZones()
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		int totalBlocks = instance.GetTotalBlocks();
		for (int i = 0; i < totalBlocks; i++)
		{
			BaseBuildingBlock buildingBlock = instance.GetBuildingBlock(i);
			if (!(buildingBlock != null) || buildingBlock.BlockType != BuildingBlockType.Room)
			{
				continue;
			}
			BuildingBlock_Room buildingBlock_Room = buildingBlock as BuildingBlock_Room;
			if (!(buildingBlock_Room != null))
			{
				continue;
			}
			List<BaseBuildInstruction> instructionsV = buildingBlock_Room.m_InstructionsV2;
			buildingBlock_Room.CreatFootprint(instructionsV);
			int count = instructionsV.Count;
			bool flag = false;
			for (int j = 0; j < count; j++)
			{
				if (instructionsV[j] != null && instructionsV[j].m_Type == BaseBuildInstruction.InstructionTypeEnum.Zone)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BuildingBlockManager.LimitationGroup theLimitationGroup = instance.GetTheLimitationGroup(buildingBlock_Room.m_LimitationGroup);
				if (theLimitationGroup != null && theLimitationGroup.m_ZoneType != 0)
				{
					int iLeft = 0;
					int iRight = 0;
					int iTop = 0;
					int iBottom = 0;
					byte[] zonePrint = new byte[0];
					buildingBlock_Room.m_Footprint.CreateZonePrint(ref iLeft, ref iRight, ref iTop, ref iBottom, ref zonePrint);
					int num = iRight - iLeft;
					int num2 = iTop - iBottom;
					BaseBuildInstruction item = BaseBuildInstruction.CreateZone(theLimitationGroup.m_ZoneType, iLeft, iBottom, num + 1, num2 + 1, zonePrint);
					instructionsV.Add(item);
				}
			}
		}
	}

	public static void AutoRemoveZones()
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		int totalBlocks = instance.GetTotalBlocks();
		for (int i = 0; i < totalBlocks; i++)
		{
			BaseBuildingBlock buildingBlock = instance.GetBuildingBlock(i);
			if (!(buildingBlock != null) || buildingBlock.BlockType != BuildingBlockType.Room)
			{
				continue;
			}
			BuildingBlock_Room buildingBlock_Room = buildingBlock as BuildingBlock_Room;
			if (!(buildingBlock_Room != null))
			{
				continue;
			}
			List<BaseBuildInstruction> list = buildingBlock_Room.m_Instructions;
			for (int j = 0; j < 2; j++)
			{
				int num = list.Count;
				for (int k = 0; k < num; k++)
				{
					if (list[k] != null && list[k].m_Type == BaseBuildInstruction.InstructionTypeEnum.Zone)
					{
						list.RemoveAt(k);
						num--;
						k--;
					}
				}
				list = buildingBlock_Room.m_InstructionsV2;
			}
		}
	}

	public static void AddRoomZones(BuildingBlock_Room blk)
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (!(instance != null) || !(blk != null) || blk.BlockType != BuildingBlockType.Room)
		{
			return;
		}
		if (!(blk != null))
		{
			return;
		}
		List<BaseBuildInstruction> instructionsV = blk.m_InstructionsV2;
		blk.CreatFootprint(instructionsV);
		int count = instructionsV.Count;
		for (int i = 0; i < count; i++)
		{
			if (instructionsV[i] != null && instructionsV[i].m_Type == BaseBuildInstruction.InstructionTypeEnum.Zone)
			{
				instructionsV.RemoveAt(i);
				break;
			}
		}
		BuildingBlockManager.LimitationGroup theLimitationGroup = instance.GetTheLimitationGroup(blk.m_LimitationGroup);
		if (theLimitationGroup != null && theLimitationGroup.m_ZoneType != 0)
		{
			int iLeft = 0;
			int iRight = 0;
			int iTop = 0;
			int iBottom = 0;
			byte[] zonePrint = new byte[0];
			blk.m_Footprint.CreateZonePrint(ref iLeft, ref iRight, ref iTop, ref iBottom, ref zonePrint);
			int num = iRight - iLeft;
			int num2 = iTop - iBottom;
			BaseBuildInstruction item = BaseBuildInstruction.CreateZone(theLimitationGroup.m_ZoneType, iLeft, iBottom, num + 1, num2 + 1, zonePrint);
			instructionsV.Add(item);
		}
	}

	public void InitInstructionSet()
	{
		if (BlockType == BuildingBlockType.Room)
		{
			if (m_InstructionSetVersion != LevelDetailsManager.c_CurrentLevelDataVersionNumber || m_BlockInstructions == null)
			{
				m_InstructionSetVersion = LevelDetailsManager.c_CurrentLevelDataVersionNumber;
				if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
				{
					m_BlockInstructions = m_Instructions;
				}
				else
				{
					m_BlockInstructions = m_InstructionsV2;
				}
			}
		}
		else
		{
			m_BlockInstructions = m_Instructions;
		}
	}

	public override void MakeActualObject(int iIndex)
	{
		if (BlockType == BuildingBlockType.Room)
		{
			InitInstructionSet();
			CreatFootprint();
		}
		base.MakeActualObject(iIndex);
	}
}

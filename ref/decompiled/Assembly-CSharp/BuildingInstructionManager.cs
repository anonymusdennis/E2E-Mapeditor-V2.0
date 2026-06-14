using System;
using System.Collections.Generic;
using System.Diagnostics;
using DataHelpers;
using UnityEngine;

public class BuildingInstructionManager : MonoBehaviour
{
	[Serializable]
	public class InstructionListElement
	{
		public BaseBuildInstruction.InstructionTypeEnum m_Type;

		public int m_Index = -1;

		public InstructionListElement(int iIndex, BaseBuildInstruction.InstructionTypeEnum insType)
		{
			m_Index = iIndex;
			m_Type = insType;
		}

		public InstructionListElement()
		{
			m_Type = BaseBuildInstruction.InstructionTypeEnum.UNKNOWN;
			m_Index = -1;
		}
	}

	[Serializable]
	public class InstructionList
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionListElement> m_Instructions = new List<InstructionListElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(202);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				dataCollection.Add((byte)m_Instructions[i].m_Type);
				ByteArrayConversion.AddInt(m_Instructions[i].m_Index, ref dataCollection);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 202)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionListElement instructionListElement = new InstructionListElement();
				instructionListElement.m_Type = (BaseBuildInstruction.InstructionTypeEnum)dataCollection[iIndex++];
				instructionListElement.m_Index = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				Add(instructionListElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionListElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionComplexElement
	{
		public int m_BuildingBlockID = -1;

		public InstructionList m_ComplexInstructions = new InstructionList();
	}

	[Serializable]
	public class Instruction_Complex
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionComplexElement> m_Instructions = new List<InstructionComplexElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(203);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBlockID, ref dataCollection);
				m_Instructions[i].m_ComplexInstructions.SerializeOurData(ref dataCollection);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 203)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionComplexElement instructionComplexElement = new InstructionComplexElement();
				instructionComplexElement.m_BuildingBlockID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionComplexElement.m_ComplexInstructions.DeserializeOurData(ref dataCollection, ref iIndex);
				Add(instructionComplexElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionComplexElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionOnceElement
	{
		public int m_BuildingBlockID = -1;

		public sbyte m_XPosition;

		public sbyte m_YPosition;

		public int m_iRandomSeed;

		[NonSerialized]
		public BaseLevelManager.TileIDData m_Previous = BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask;
	}

	[Serializable]
	public class Instruction_Once
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionOnceElement> m_Instructions = new List<InstructionOnceElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(204);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBlockID, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XPosition);
				dataCollection.Add((byte)m_Instructions[i].m_YPosition);
				ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 204)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionOnceElement instructionOnceElement = new InstructionOnceElement();
				instructionOnceElement.m_BuildingBlockID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionOnceElement.m_XPosition = (sbyte)dataCollection[iIndex++];
				instructionOnceElement.m_YPosition = (sbyte)dataCollection[iIndex++];
				instructionOnceElement.m_iRandomSeed = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				Add(instructionOnceElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionOnceElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionOnceWallElement
	{
		public int m_BuildingBlockID = -1;

		public sbyte m_XPosition;

		public sbyte m_YPosition;

		public int m_iRandomSeed;

		[NonSerialized]
		public BaseLevelManager.TileIDData m_Previous = BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask;

		[NonSerialized]
		public BaseLevelManager.TileIDData m_PreviousTile = BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask;
	}

	[Serializable]
	public class Instruction_OnceWall
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionOnceWallElement> m_Instructions = new List<InstructionOnceWallElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(205);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBlockID, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XPosition);
				dataCollection.Add((byte)m_Instructions[i].m_YPosition);
				ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 205)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionOnceWallElement instructionOnceWallElement = new InstructionOnceWallElement();
				instructionOnceWallElement.m_BuildingBlockID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionOnceWallElement.m_XPosition = (sbyte)dataCollection[iIndex++];
				instructionOnceWallElement.m_YPosition = (sbyte)dataCollection[iIndex++];
				instructionOnceWallElement.m_iRandomSeed = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				Add(instructionOnceWallElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionOnceWallElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionAreaElement
	{
		public int m_BuildingBlockID = -1;

		public sbyte m_XPosition;

		public sbyte m_YPosition;

		public int m_iRandomSeed;

		public sbyte m_XCount = 1;

		public sbyte m_YCount = 1;

		[NonSerialized]
		public BaseLevelManager.TileIDData[] m_Previous = new BaseLevelManager.TileIDData[0];
	}

	[Serializable]
	public class Instruction_Area
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionAreaElement> m_Instructions = new List<InstructionAreaElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(206);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBlockID, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XPosition);
				dataCollection.Add((byte)m_Instructions[i].m_YPosition);
				ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XCount);
				dataCollection.Add((byte)m_Instructions[i].m_YCount);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 206)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionAreaElement instructionAreaElement = new InstructionAreaElement();
				instructionAreaElement.m_BuildingBlockID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionAreaElement.m_XPosition = (sbyte)dataCollection[iIndex++];
				instructionAreaElement.m_YPosition = (sbyte)dataCollection[iIndex++];
				instructionAreaElement.m_iRandomSeed = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionAreaElement.m_XCount = (sbyte)dataCollection[iIndex++];
				instructionAreaElement.m_YCount = (sbyte)dataCollection[iIndex++];
				Add(instructionAreaElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionAreaElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionAreaWallElement
	{
		public int m_BuildingBlockID = -1;

		public sbyte m_XPosition;

		public sbyte m_YPosition;

		public int m_iRandomSeed;

		public sbyte m_XCount = 1;

		public sbyte m_YCount = 1;

		[NonSerialized]
		public BaseLevelManager.TileIDData[] m_Previous = new BaseLevelManager.TileIDData[0];

		[NonSerialized]
		public BaseLevelManager.TileIDData[] m_PreviousTile = new BaseLevelManager.TileIDData[0];
	}

	[Serializable]
	public class Instruction_AreaWall
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionAreaWallElement> m_Instructions = new List<InstructionAreaWallElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(207);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBlockID, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XPosition);
				dataCollection.Add((byte)m_Instructions[i].m_YPosition);
				ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
				dataCollection.Add((byte)m_Instructions[i].m_XCount);
				dataCollection.Add((byte)m_Instructions[i].m_YCount);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 207)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionAreaWallElement instructionAreaWallElement = new InstructionAreaWallElement();
				instructionAreaWallElement.m_BuildingBlockID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionAreaWallElement.m_XPosition = (sbyte)dataCollection[iIndex++];
				instructionAreaWallElement.m_YPosition = (sbyte)dataCollection[iIndex++];
				instructionAreaWallElement.m_iRandomSeed = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionAreaWallElement.m_XCount = (sbyte)dataCollection[iIndex++];
				instructionAreaWallElement.m_YCount = (sbyte)dataCollection[iIndex++];
				Add(instructionAreaWallElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionAreaWallElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionZoneElement
	{
		public enum ZoneAction : sbyte
		{
			Create,
			Delete,
			Add,
			Subtract
		}

		public ZoneAction m_Action;

		public ZoneDetailsManager.ZoneTypes m_ZoneType = ZoneDetailsManager.ZoneTypes.TOTAL;

		public sbyte m_Left;

		public sbyte m_Bottom;

		public sbyte m_Width = 1;

		public sbyte m_Height = 1;

		public byte[] m_ZonePrint = new byte[0];

		public int m_iID = -1;
	}

	[Serializable]
	public class Instruction_Zone
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionZoneElement> m_Instructions = new List<InstructionZoneElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(210);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				dataCollection.Add((byte)m_Instructions[i].m_Action);
				dataCollection.Add((byte)m_Instructions[i].m_ZoneType);
				dataCollection.Add((byte)m_Instructions[i].m_Left);
				dataCollection.Add((byte)m_Instructions[i].m_Bottom);
				dataCollection.Add((byte)m_Instructions[i].m_Width);
				dataCollection.Add((byte)m_Instructions[i].m_Height);
				ByteArrayConversion.AddInt(m_Instructions[i].m_iID, ref dataCollection);
				int num = m_Instructions[i].m_ZonePrint.Length;
				ByteArrayConversion.AddInt(num, ref dataCollection);
				for (int j = 0; j < num; j++)
				{
					dataCollection.Add(m_Instructions[i].m_ZonePrint[j]);
				}
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 210)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionZoneElement instructionZoneElement = new InstructionZoneElement();
				instructionZoneElement.m_Action = (InstructionZoneElement.ZoneAction)dataCollection[iIndex++];
				instructionZoneElement.m_ZoneType = (ZoneDetailsManager.ZoneTypes)dataCollection[iIndex++];
				instructionZoneElement.m_Left = (sbyte)dataCollection[iIndex++];
				instructionZoneElement.m_Bottom = (sbyte)dataCollection[iIndex++];
				instructionZoneElement.m_Width = (sbyte)dataCollection[iIndex++];
				instructionZoneElement.m_Height = (sbyte)dataCollection[iIndex++];
				instructionZoneElement.m_iID = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				int int2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				instructionZoneElement.m_ZonePrint = new byte[int2];
				for (int j = 0; j < int2; j++)
				{
					instructionZoneElement.m_ZonePrint[j] = dataCollection[iIndex++];
				}
				Add(instructionZoneElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionZoneElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	public enum CommandsEnum : byte
	{
		Do_Nothing,
		Set_Environment,
		Set_Layer,
		Start_Room,
		End_Room,
		Start_DeleteRoom,
		End_DeleteRoom,
		Set_Inside,
		Set_Outside,
		Inc_Layer,
		Dec_Layer,
		Start_Undo,
		End_Undo
	}

	[Serializable]
	public class InstructionCommandElement
	{
		public CommandsEnum m_Commmand;

		public int m_Value;

		[NonSerialized]
		public int m_PreviousValue;
	}

	[Serializable]
	public class Instruction_Command
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionCommandElement> m_Instructions = new List<InstructionCommandElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(208);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				dataCollection.Add((byte)m_Instructions[i].m_Commmand);
				ByteArrayConversion.AddInt(m_Instructions[i].m_Value, ref dataCollection);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 208)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionCommandElement instructionCommandElement = new InstructionCommandElement();
				instructionCommandElement.m_Commmand = (CommandsEnum)dataCollection[iIndex++];
				instructionCommandElement.m_Value = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
				Add(instructionCommandElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionCommandElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	[Serializable]
	public class InstructionDeleteElement
	{
		public enum DeleteType : sbyte
		{
			Tile,
			Wall,
			Object
		}

		public sbyte m_XPosition;

		public sbyte m_YPosition;

		public DeleteType m_DeleteType;

		[NonSerialized]
		public BaseLevelManager.TileIDData m_Previous = BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask;
	}

	[Serializable]
	public class Instruction_Delete
	{
		public int m_iTotal;

		public int m_iTotalValid;

		public List<InstructionDeleteElement> m_Instructions = new List<InstructionDeleteElement>();

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			dataCollection.Add(209);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(m_iTotal, ref dataCollection);
			for (int i = 0; i < m_iTotal; i++)
			{
				dataCollection.Add((byte)m_Instructions[i].m_DeleteType);
				dataCollection.Add((byte)m_Instructions[i].m_XPosition);
				dataCollection.Add((byte)m_Instructions[i].m_YPosition);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 209)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				InstructionDeleteElement instructionDeleteElement = new InstructionDeleteElement();
				instructionDeleteElement.m_DeleteType = (InstructionDeleteElement.DeleteType)dataCollection[iIndex++];
				instructionDeleteElement.m_XPosition = (sbyte)dataCollection[iIndex++];
				instructionDeleteElement.m_YPosition = (sbyte)dataCollection[iIndex++];
				Add(instructionDeleteElement);
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}

		private void Add(InstructionDeleteElement element)
		{
			m_Instructions.Add(element);
			m_iTotal++;
			m_iTotalValid++;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_iTotal = 0;
			m_iTotalValid = 0;
		}
	}

	public class FinishedLevelInstructions
	{
		public int m_CurrentInstruction;

		public List<BaseBuildInstruction> m_Instructions = new List<BaseBuildInstruction>();

		public bool ProcessNextInstruction(BuildingInstructionManager manager)
		{
			if (m_CurrentInstruction >= m_Instructions.Count)
			{
				m_Instructions.Clear();
				m_CurrentInstruction = 0;
				return true;
			}
			BaseBuildInstruction baseBuildInstruction = m_Instructions[m_CurrentInstruction++];
			switch (baseBuildInstruction.m_Type)
			{
			case BaseBuildInstruction.InstructionTypeEnum.ChangeLayer:
				manager.ChangeLayer(baseBuildInstruction.m_Layer);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.DecrementLayer:
				manager.DecrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.IncrementLayer:
				manager.IncrementLayer();
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
				manager.AddBlockOnce(baseBuildInstruction.m_BuildingBrickID, baseBuildInstruction.m_XPosition, baseBuildInstruction.m_YPosition, baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
				manager.AddBlockArea(baseBuildInstruction.m_BuildingBrickID, baseBuildInstruction.m_XPosition, baseBuildInstruction.m_YPosition, baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_iRandomSeed);
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Zone:
				if (baseBuildInstruction.m_bInside)
				{
					manager.CreateZone((ZoneDetailsManager.ZoneTypes)baseBuildInstruction.m_iRandomSeed, baseBuildInstruction.m_XPosition, baseBuildInstruction.m_YPosition, baseBuildInstruction.m_XCount, baseBuildInstruction.m_YCount, baseBuildInstruction.m_ZonePrint);
				}
				break;
			}
			return false;
		}

		public void Reset()
		{
			m_Instructions.Clear();
			m_CurrentInstruction = 0;
		}

		public void QuantizeThelevel()
		{
			Reset();
			m_Instructions = LevelEditorQuantizer.QuantizeLevel(bSave: false);
		}

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			QuantizeThelevel();
			int count = m_Instructions.Count;
			dataCollection.Add(100);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddInt(count, ref dataCollection);
			for (int i = 0; i < count; i++)
			{
				switch (m_Instructions[i].m_Type)
				{
				case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
					dataCollection.Add(101);
					dataCollection.Add((byte)m_Instructions[i].m_XPosition);
					dataCollection.Add((byte)m_Instructions[i].m_YPosition);
					ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBrickID, ref dataCollection);
					ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
					break;
				case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
					dataCollection.Add(102);
					dataCollection.Add((byte)m_Instructions[i].m_XPosition);
					dataCollection.Add((byte)m_Instructions[i].m_YPosition);
					ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBrickID, ref dataCollection);
					dataCollection.Add((byte)m_Instructions[i].m_XCount);
					dataCollection.Add((byte)m_Instructions[i].m_YCount);
					ByteArrayConversion.AddInt(m_Instructions[i].m_iRandomSeed, ref dataCollection);
					break;
				case BaseBuildInstruction.InstructionTypeEnum.ChangeLayer:
					dataCollection.Add(103);
					dataCollection.Add((byte)m_Instructions[i].m_Layer);
					break;
				case BaseBuildInstruction.InstructionTypeEnum.DecrementLayer:
					dataCollection.Add(106);
					break;
				case BaseBuildInstruction.InstructionTypeEnum.IncrementLayer:
					dataCollection.Add(105);
					break;
				case BaseBuildInstruction.InstructionTypeEnum.Zone:
				{
					dataCollection.Add(107);
					dataCollection.Add((byte)m_Instructions[i].m_iRandomSeed);
					dataCollection.Add((byte)m_Instructions[i].m_XPosition);
					dataCollection.Add((byte)m_Instructions[i].m_YPosition);
					dataCollection.Add((byte)m_Instructions[i].m_XCount);
					dataCollection.Add((byte)m_Instructions[i].m_YCount);
					ByteArrayConversion.AddInt(m_Instructions[i].m_BuildingBrickID, ref dataCollection);
					int num = m_Instructions[i].m_ZonePrint.Length;
					ByteArrayConversion.AddInt(num, ref dataCollection);
					for (int j = 0; j < num; j++)
					{
						dataCollection.Add(m_Instructions[i].m_ZonePrint[j]);
					}
					break;
				}
				default:
					dataCollection.Add(104);
					break;
				}
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			sbyte b = 0;
			sbyte b2 = 0;
			sbyte b3 = 0;
			sbyte b4 = 0;
			int num = 0;
			int num2 = 0;
			if (dataCollection[iIndex] != 100)
			{
				return false;
			}
			iIndex++;
			ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			for (int i = 0; i < @int; i++)
			{
				switch ((LevelDetailsManager.SerializationFlag)dataCollection[iIndex++])
				{
				case LevelDetailsManager.SerializationFlag.Instruction_DrawOnce:
				{
					b = (sbyte)dataCollection[iIndex++];
					b2 = (sbyte)dataCollection[iIndex++];
					num = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					num2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					BaseBuildInstruction item3 = BaseBuildInstruction.CreateOnce(b, b2, num, num2);
					m_Instructions.Add(item3);
					break;
				}
				case LevelDetailsManager.SerializationFlag.ChunkEnd:
				{
					b = (sbyte)dataCollection[iIndex++];
					b2 = (sbyte)dataCollection[iIndex++];
					num = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					b3 = (sbyte)dataCollection[iIndex++];
					b4 = (sbyte)dataCollection[iIndex++];
					num2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					BaseBuildInstruction item2 = BaseBuildInstruction.CreateArea(b, b2, b3, b4, num, num2);
					m_Instructions.Add(item2);
					break;
				}
				case LevelDetailsManager.SerializationFlag.Instruction_ChangeArea:
				{
					BaseBuildInstruction item6 = BaseBuildInstruction.CreateLayerChange((BaseLevelManager.LevelLayers)dataCollection[iIndex++]);
					m_Instructions.Add(item6);
					break;
				}
				case LevelDetailsManager.SerializationFlag.Instruction_IncrementLayer:
				{
					BaseBuildInstruction item5 = BaseBuildInstruction.IncrementLayer();
					m_Instructions.Add(item5);
					break;
				}
				case LevelDetailsManager.SerializationFlag.Instruction_DecrementLayer:
				{
					BaseBuildInstruction item4 = BaseBuildInstruction.DecrementLayer();
					m_Instructions.Add(item4);
					break;
				}
				case LevelDetailsManager.SerializationFlag.Instruction_Zone:
				{
					ZoneDetailsManager.ZoneTypes eType = (ZoneDetailsManager.ZoneTypes)dataCollection[iIndex++];
					b = (sbyte)dataCollection[iIndex++];
					b2 = (sbyte)dataCollection[iIndex++];
					b3 = (sbyte)dataCollection[iIndex++];
					b4 = (sbyte)dataCollection[iIndex++];
					num = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					int int2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					byte[] array = new byte[int2];
					for (int j = 0; j < int2; j++)
					{
						array[j] = dataCollection[iIndex++];
					}
					BaseBuildInstruction item = BaseBuildInstruction.CreateZone(eType, b, b2, b3, b4, array, num);
					m_Instructions.Add(item);
					break;
				}
				}
			}
			if (dataCollection[iIndex++] != 102)
			{
				return false;
			}
			return true;
		}
	}

	[Serializable]
	public class UserLevelData
	{
		public InstructionList m_UserInstructions = new InstructionList();

		public Instruction_Complex m_ComplexList = new Instruction_Complex();

		public Instruction_Once m_OnceList = new Instruction_Once();

		public Instruction_OnceWall m_OnceWallList = new Instruction_OnceWall();

		public Instruction_Area m_AreaList = new Instruction_Area();

		public Instruction_AreaWall m_AreaWallList = new Instruction_AreaWall();

		public Instruction_Command m_CommandList = new Instruction_Command();

		public Instruction_Delete m_DeleteList = new Instruction_Delete();

		public Instruction_Zone m_ZoneList = new Instruction_Zone();

		[NonSerialized]
		public FinishedLevelInstructions m_FinishedLevel = new FinishedLevelInstructions();

		public void SerializeOurData(ref List<byte> dataCollection, LevelDetailsManager.LevelType levelType)
		{
			dataCollection.Add(201);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			int count = dataCollection.Count;
			int iIndex2 = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			if (levelType == LevelDetailsManager.LevelType.WorkInProgress)
			{
				m_UserInstructions.SerializeOurData(ref dataCollection);
				m_ComplexList.SerializeOurData(ref dataCollection);
				m_OnceList.SerializeOurData(ref dataCollection);
				m_OnceWallList.SerializeOurData(ref dataCollection);
				m_AreaList.SerializeOurData(ref dataCollection);
				m_AreaWallList.SerializeOurData(ref dataCollection);
				m_CommandList.SerializeOurData(ref dataCollection);
				m_DeleteList.SerializeOurData(ref dataCollection);
				m_ZoneList.SerializeOurData(ref dataCollection);
				if (BuildingBlock_FilterManager.GetInstance() != null)
				{
					BuildingBlock_FilterManager.GetInstance().SerializeOurData(ref dataCollection);
				}
			}
			else
			{
				m_FinishedLevel.SerializeOurData(ref dataCollection);
			}
			int count2 = dataCollection.Count;
			int num = 0;
			for (int i = iIndex2 + 4; i < count2; i++)
			{
				num += dataCollection[i] ^ 0x5F;
			}
			ByteArrayConversion.StoreInt(num, ref dataCollection, ref iIndex2);
			int num2 = count2;
			byte b = (byte)((levelType != LevelDetailsManager.LevelType.Finished) ? 167u : 71u);
			byte b2 = (byte)((levelType != LevelDetailsManager.LevelType.Finished) ? 23u : 15u);
			for (int j = count; j < num2; j++)
			{
				dataCollection[j] = (byte)((dataCollection[j] + b2) ^ b);
				b2 = dataCollection[j];
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex, LevelDetailsManager.LevelType levelType)
		{
			m_UserInstructions.Reset();
			m_ComplexList.Reset();
			m_OnceList.Reset();
			m_OnceWallList.Reset();
			m_AreaList.Reset();
			m_AreaWallList.Reset();
			m_CommandList.Reset();
			m_DeleteList.Reset();
			m_FinishedLevel.Reset();
			m_ZoneList.Reset();
			if (dataCollection[iIndex] != 201)
			{
				return false;
			}
			iIndex++;
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int num = @int + iIndex;
			if (num > dataCollection.Count)
			{
				return false;
			}
			int num2 = iIndex;
			int num3 = num2 + @int - 1;
			byte b = (byte)((levelType != LevelDetailsManager.LevelType.Finished) ? 167u : 71u);
			byte b2 = (byte)((levelType != LevelDetailsManager.LevelType.Finished) ? 23u : 15u);
			for (int i = num2; i < num3; i++)
			{
				byte b3 = dataCollection[i];
				dataCollection[i] = (byte)((dataCollection[i] ^ b) - b2);
				b2 = b3;
			}
			int int2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int num4 = 0;
			for (int j = iIndex; j < num3; j++)
			{
				num4 += dataCollection[j] ^ 0x5F;
			}
			if (int2 != num4)
			{
				iIndex += @int;
				return false;
			}
			bool flag = true;
			while (flag && num >= iIndex)
			{
				switch ((LevelDetailsManager.SerializationFlag)dataCollection[iIndex])
				{
				case LevelDetailsManager.SerializationFlag.ChunkEnd:
					iIndex++;
					return true;
				case LevelDetailsManager.SerializationFlag.List_InstructionList_V1:
					flag = m_UserInstructions.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Complex_V1:
					flag = m_ComplexList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Single_V1:
					flag = m_OnceList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Wall_V1:
					flag = m_OnceWallList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Area_V1:
					flag = m_AreaList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Area_Wall_V1:
					flag = m_AreaWallList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Commands_V1:
					flag = m_CommandList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Delete_V1:
					flag = m_DeleteList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.List_Zone_V1:
					flag = m_ZoneList.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.Instruction_FinishedTotal:
					flag = m_FinishedLevel.DeserializeOurData(ref dataCollection, ref iIndex);
					break;
				case LevelDetailsManager.SerializationFlag.Filter_Settings_V1:
					if (BuildingBlock_FilterManager.GetInstance() != null)
					{
						flag = BuildingBlock_FilterManager.GetInstance().DeserializeOurData(ref dataCollection, ref iIndex);
					}
					break;
				default:
				{
					iIndex++;
					int int3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					iIndex += int3;
					break;
				}
				}
			}
			return false;
		}
	}

	[Flags]
	private enum AllowedToDelete
	{
		Tiles = 1,
		Walls = 2,
		Objects = 4,
		ALL = 7,
		InvertedTiles = -2,
		InvertedWalls = -3,
		InvertedObjects = -5,
		InvertedALL = -8
	}

	public class ScanAreas
	{
		public int X;

		public int Y;

		public int W;

		public int H;
	}

	protected static BuildingInstructionManager m_Instance;

	public BaseLevelManager m_LevelManager;

	[NonSerialized]
	public bool m_IgnoreChecks;

	public const int INVALID_INDEX = -1;

	public UserLevelData m_LevelInstructions = new UserLevelData();

	public BuildingBlockManager m_BlockManager;

	protected int m_CurrentList = -1;

	protected bool m_BuildingFromComplex;

	protected int m_Delete_X;

	protected int m_Delete_Y;

	protected BaseLevelManager.LayerDataCollection m_Delete_LayerData;

	protected int[] m_DeleteZoneMap = new int[0];

	protected InstructionComplexElement m_Delete_ComplexElement;

	protected int m_Delete_ComplexRoomNumber;

	protected InstructionComplexElement m_TempComplex = new InstructionComplexElement();

	private AllowedToDelete m_Delete_Allowed = AllowedToDelete.ALL;

	private int m_Delete_IndexIntoLayerData;

	protected bool m_StartedTimeSlice;

	private int m_PreviousStartedTimeSliceList = -1;

	private int m_CurrentStartedTimeSliceIndex;

	private Stopwatch m_TimeSlicedStopWatch = new Stopwatch();

	public static BuildingInstructionManager GetInstance()
	{
		return m_Instance;
	}

	public virtual bool ConvertTheLevel()
	{
		return false;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	private void Start()
	{
	}

	private void OnEnabled()
	{
		CacheManagers();
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void CacheManagers()
	{
		m_BlockManager = BuildingBlockManager.GetInstance();
		m_LevelManager = BaseLevelManager.GetInstance();
	}

	public bool AddBlockOnce(int iID, sbyte X, sbyte Y, int seed, bool bDontRun = false, bool bCheckLimit = false)
	{
		if (m_BlockManager != null)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iID);
			if (block != null)
			{
				switch (block.BlockType)
				{
				case BaseBuildingBlock.BuildingBlockType.Wall:
				{
					InstructionOnceWallElement obj = new InstructionOnceWallElement();
					obj.m_BuildingBlockID = iID;
					obj.m_iRandomSeed = seed;
					obj.m_XPosition = X;
					obj.m_YPosition = Y;
					int num = m_LevelInstructions.m_OnceWallList.m_iTotal++;
					if (m_LevelInstructions.m_OnceWallList.m_Instructions.Count != num)
					{
						m_LevelInstructions.m_OnceWallList.m_Instructions[num] = obj;
					}
					else
					{
						m_LevelInstructions.m_OnceWallList.m_Instructions.Add(obj);
					}
					m_LevelInstructions.m_OnceWallList.m_iTotalValid = num + 1;
					if (m_LevelManager != null && !bDontRun)
					{
						m_LevelManager.AddSingleWall(ref obj);
					}
					return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall);
				}
				case BaseBuildingBlock.BuildingBlockType.Object:
					if (bCheckLimit)
					{
						BuildingBlockManager instance = BuildingBlockManager.GetInstance();
						BuildingBlock_Object buildingBlock_Object = instance.GetBuildingBlock(iID) as BuildingBlock_Object;
						if (buildingBlock_Object != null && buildingBlock_Object.m_LimitationGroup != -1 && buildingBlock_Object.m_LimitationCount > 0)
						{
							BuildingBlockManager.LimitationGroup theLimitationGroup = instance.GetTheLimitationGroup(buildingBlock_Object.m_LimitationGroup);
							if (theLimitationGroup != null && theLimitationGroup.m_Max != 0 && theLimitationGroup.m_CurrentTotal + buildingBlock_Object.m_LimitationCount > theLimitationGroup.m_Max)
							{
								return false;
							}
						}
					}
					goto case BaseBuildingBlock.BuildingBlockType.Tile;
				case BaseBuildingBlock.BuildingBlockType.Tile:
				case BaseBuildingBlock.BuildingBlockType.Decoration:
				{
					InstructionOnceElement obj2 = new InstructionOnceElement();
					obj2.m_BuildingBlockID = iID;
					obj2.m_iRandomSeed = seed;
					obj2.m_XPosition = X;
					obj2.m_YPosition = Y;
					int num2 = m_LevelInstructions.m_OnceList.m_iTotal++;
					if (m_LevelInstructions.m_OnceList.m_Instructions.Count != num2)
					{
						m_LevelInstructions.m_OnceList.m_Instructions[num2] = obj2;
					}
					else
					{
						m_LevelInstructions.m_OnceList.m_Instructions.Add(obj2);
					}
					m_LevelInstructions.m_OnceList.m_iTotalValid = num2 + 1;
					if (m_LevelManager != null && !bDontRun)
					{
						m_LevelManager.AddSingle(ref obj2);
					}
					return AddToCurrentList(num2, BaseBuildInstruction.InstructionTypeEnum.Draw_Once);
				}
				case BaseBuildingBlock.BuildingBlockType.Complex:
					return AddFromInstructionsBlock(block as BuildingBlock_Room, X, Y, seed);
				case BaseBuildingBlock.BuildingBlockType.Room:
					return AddFromInstructionsBlock(block as BuildingBlock_Room, X, Y, seed, bCheckLimits: true);
				default:
					return false;
				}
			}
		}
		return false;
	}

	public bool AddFromInstructionsBlock(int iID, sbyte X, sbyte Y, int seed, bool bCheckLimits = false)
	{
		BuildingBlock_Room buildingBlock_Room = BuildingBlockManager.GetBlock(iID) as BuildingBlock_Room;
		if (buildingBlock_Room != null)
		{
			return AddFromInstructionsBlock(buildingBlock_Room, X, Y, seed, bCheckLimits);
		}
		return false;
	}

	public virtual bool AddFromInstructionsBlock(BuildingBlock_Room obj, sbyte X, sbyte Y, int seed, bool bCheckLimits = false)
	{
		if (obj == null)
		{
			return false;
		}
		int currentList = m_CurrentList;
		bool buildingFromComplex = m_BuildingFromComplex;
		m_BuildingFromComplex = true;
		InstructionComplexElement instructionComplexElement = new InstructionComplexElement();
		instructionComplexElement.m_BuildingBlockID = obj.m_ID;
		int num = m_LevelInstructions.m_ComplexList.m_iTotal++;
		if (m_LevelInstructions.m_ComplexList.m_Instructions.Count != num)
		{
			m_LevelInstructions.m_ComplexList.m_Instructions[num] = instructionComplexElement;
		}
		else
		{
			m_LevelInstructions.m_ComplexList.m_Instructions.Add(instructionComplexElement);
		}
		m_LevelInstructions.m_ComplexList.m_iTotalValid = num + 1;
		bool flag = AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Complex);
		if (flag)
		{
			m_CurrentList = num;
			if ((obj.BlockType == BaseBuildingBlock.BuildingBlockType.Room || obj.BlockType == BaseBuildingBlock.BuildingBlockType.Complex) && !buildingFromComplex && !m_IgnoreChecks)
			{
				AddCommand(CommandsEnum.Start_Room, obj.m_ID);
			}
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
				}
			}
			if ((obj.BlockType == BaseBuildingBlock.BuildingBlockType.Room || obj.BlockType == BaseBuildingBlock.BuildingBlockType.Complex) && !buildingFromComplex && !m_IgnoreChecks)
			{
				AddCommand(CommandsEnum.End_Room, obj.m_ID);
			}
		}
		m_CurrentList = currentList;
		m_BuildingFromComplex = buildingFromComplex;
		return flag;
	}

	public bool AddBlockArea(int iID, sbyte X, sbyte Y, sbyte Width, sbyte Height, int seed, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iID);
			if (iID == -1 || block != null)
			{
				if (block != null && block.BlockType == BaseBuildingBlock.BuildingBlockType.Wall)
				{
					InstructionAreaWallElement obj = new InstructionAreaWallElement();
					obj.m_BuildingBlockID = iID;
					obj.m_iRandomSeed = seed;
					obj.m_XPosition = X;
					obj.m_YPosition = Y;
					obj.m_XCount = Width;
					obj.m_YCount = Height;
					int num = m_LevelInstructions.m_AreaWallList.m_iTotal++;
					if (m_LevelInstructions.m_AreaWallList.m_Instructions.Count != num)
					{
						m_LevelInstructions.m_AreaWallList.m_Instructions[num] = obj;
					}
					else
					{
						m_LevelInstructions.m_AreaWallList.m_Instructions.Add(obj);
					}
					m_LevelInstructions.m_AreaWallList.m_iTotalValid = num + 1;
					if (m_LevelManager != null && !bDontRun)
					{
						m_LevelManager.AddAreaWall(ref obj);
					}
					return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall);
				}
				InstructionAreaElement obj2 = new InstructionAreaElement();
				obj2.m_BuildingBlockID = iID;
				obj2.m_iRandomSeed = seed;
				obj2.m_XPosition = X;
				obj2.m_YPosition = Y;
				obj2.m_XCount = Width;
				obj2.m_YCount = Height;
				int num2 = m_LevelInstructions.m_AreaList.m_iTotal++;
				if (m_LevelInstructions.m_AreaList.m_Instructions.Count != num2)
				{
					m_LevelInstructions.m_AreaList.m_Instructions[num2] = obj2;
				}
				else
				{
					m_LevelInstructions.m_AreaList.m_Instructions.Add(obj2);
				}
				m_LevelInstructions.m_AreaList.m_iTotalValid = num2 + 1;
				if (m_LevelManager != null && !bDontRun)
				{
					m_LevelManager.AddArea(ref obj2);
				}
				return AddToCurrentList(num2, BaseBuildInstruction.InstructionTypeEnum.Draw_Area);
			}
		}
		return false;
	}

	private List<ScanAreas> GetAreas(int iLayer, int iX, int iY, int iWidth, int iHeight, BaseLevelManager.TileProperty envResult)
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[iLayer];
		BaseLevelManager.LayerDataCollection layerDataCollection2 = null;
		if ((iLayer == 3 || iLayer == 5) && envResult == BaseLevelManager.TileProperty.EMPTY)
		{
			layerDataCollection2 = m_LevelManager.m_BuildingLayers[iLayer - 2];
		}
		int num = 120 * iY + iX;
		int num2 = 120 - iWidth;
		List<ScanAreas> list = new List<ScanAreas>();
		for (int i = 0; i < iHeight; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int num3;
				(tileProperties = layerDataCollection.m_TileProperties)[num3 = num++] = tileProperties[num3] & BaseLevelManager.TileProperty.InverseScanTileMask;
			}
			num += num2;
		}
		num = 120 * iY + iX;
		BaseLevelManager.TileProperty tileProperty = BaseLevelManager.TileProperty.EnvironmentMask;
		BaseLevelManager.TileProperty tileProperty2 = BaseLevelManager.TileProperty.EnvironmentMask | BaseLevelManager.TileProperty.ScanTileMask;
		int num4 = iX + iWidth;
		int num5 = iY + iHeight;
		for (int k = iY; k < num5; k++)
		{
			for (int l = iX; l < num4; l++)
			{
				BaseLevelManager.TileProperty tileProperty3 = layerDataCollection.m_TileProperties[num];
				if (layerDataCollection2 != null)
				{
					tileProperty = layerDataCollection2.m_TileProperties[num] & BaseLevelManager.TileProperty.EnvironmentMask;
				}
				if ((tileProperty3 & tileProperty2) == envResult && tileProperty == BaseLevelManager.TileProperty.EnvironmentMask)
				{
					int num6;
					BaseLevelManager.TileProperty[] tileProperties;
					(tileProperties = layerDataCollection.m_TileProperties)[num6 = num] = tileProperties[num6] | BaseLevelManager.TileProperty.ScanTileMask;
					ScanAreas scanAreas = new ScanAreas();
					scanAreas.X = l;
					scanAreas.Y = k;
					scanAreas.W = 1;
					scanAreas.H = 1;
					int num7 = num + 1;
					for (int m = l + 1; m < num4; m++)
					{
						tileProperty3 = layerDataCollection.m_TileProperties[num7];
						if (layerDataCollection2 != null)
						{
							tileProperty = layerDataCollection2.m_TileProperties[num7] & BaseLevelManager.TileProperty.EnvironmentMask;
						}
						if ((tileProperty3 & tileProperty2) != envResult || tileProperty == BaseLevelManager.TileProperty.EMPTY)
						{
							break;
						}
						int num8;
						(tileProperties = layerDataCollection.m_TileProperties)[num8 = num7++] = tileProperties[num8] | BaseLevelManager.TileProperty.ScanTileMask;
						scanAreas.W++;
					}
					num7 = num + 120;
					int num9 = 120 - scanAreas.W;
					for (int n = scanAreas.Y + 1; n < num5; n++)
					{
						bool flag = false;
						int num10 = num7;
						for (int num11 = 0; num11 < scanAreas.W; num11++)
						{
							if (layerDataCollection2 != null)
							{
								tileProperty = layerDataCollection2.m_TileProperties[num7] & BaseLevelManager.TileProperty.EnvironmentMask;
							}
							tileProperty3 = layerDataCollection.m_TileProperties[num7++];
							if ((tileProperty3 & tileProperty2) != envResult || tileProperty == BaseLevelManager.TileProperty.EMPTY)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
						for (int num12 = 0; num12 < scanAreas.W; num12++)
						{
							int num13;
							(tileProperties = layerDataCollection.m_TileProperties)[num13 = num10++] = tileProperties[num13] | BaseLevelManager.TileProperty.ScanTileMask;
						}
						num7 += num9;
						scanAreas.H++;
					}
					if (list.Count == 0)
					{
					}
					list.Add(scanAreas);
				}
				num++;
			}
			num += num2;
		}
		return list;
	}

	public bool ChangeLayer(BaseLevelManager.LevelLayers elayer)
	{
		return AddCommand(CommandsEnum.Set_Layer, (int)elayer);
	}

	public bool IncrementLayer()
	{
		return AddCommand(CommandsEnum.Inc_Layer, 0);
	}

	public bool DecrementLayer()
	{
		return AddCommand(CommandsEnum.Dec_Layer, 0);
	}

	public bool AddStartUndo()
	{
		return AddCommand(CommandsEnum.Start_Undo, 0);
	}

	public bool AddEndUndo()
	{
		return AddCommand(CommandsEnum.End_Undo, 0);
	}

	public bool ChangeEnvironment(bool bInside)
	{
		BaseLevelManager.LayersEnvironment iValue = ((!bInside) ? BaseLevelManager.LayersEnvironment.Outside : BaseLevelManager.LayersEnvironment.Inside);
		return AddCommand(CommandsEnum.Set_Environment, (int)iValue);
	}

	public bool AddCommand(CommandsEnum eCommand, int iValue, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			int num = AddCommandToCommandList(eCommand, iValue);
			if (num != -1)
			{
				if (m_LevelManager != null && !bDontRun)
				{
					InstructionCommandElement obj = m_LevelInstructions.m_CommandList.m_Instructions[num];
					m_LevelManager.AddCommand(ref obj);
				}
				return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Command);
			}
		}
		return false;
	}

	public int AddCommandToCommandList(CommandsEnum eCommand, int iValue)
	{
		if (m_BlockManager != null)
		{
			InstructionCommandElement instructionCommandElement = new InstructionCommandElement();
			instructionCommandElement.m_Commmand = eCommand;
			instructionCommandElement.m_Value = iValue;
			int num = m_LevelInstructions.m_CommandList.m_iTotal++;
			if (m_LevelInstructions.m_CommandList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_CommandList.m_Instructions[num] = instructionCommandElement;
			}
			else
			{
				m_LevelInstructions.m_CommandList.m_Instructions.Add(instructionCommandElement);
			}
			m_LevelInstructions.m_CommandList.m_iTotalValid = num + 1;
			return num;
		}
		return -1;
	}

	public int AddDeleteToDeleteList(int X, int Y, InstructionDeleteElement.DeleteType deleteType)
	{
		if (m_BlockManager != null)
		{
			InstructionDeleteElement instructionDeleteElement = new InstructionDeleteElement();
			instructionDeleteElement.m_XPosition = (sbyte)X;
			instructionDeleteElement.m_YPosition = (sbyte)Y;
			instructionDeleteElement.m_DeleteType = deleteType;
			int num = m_LevelInstructions.m_DeleteList.m_iTotal++;
			if (m_LevelInstructions.m_DeleteList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_DeleteList.m_Instructions[num] = instructionDeleteElement;
			}
			else
			{
				m_LevelInstructions.m_DeleteList.m_Instructions.Add(instructionDeleteElement);
			}
			m_LevelInstructions.m_DeleteList.m_iTotalValid = num + 1;
			return num;
		}
		return -1;
	}

	public int AddTileToTileList(int X, int Y, int iBlockID, int seed)
	{
		if (m_BlockManager != null && m_LevelManager != null)
		{
			InstructionOnceElement instructionOnceElement = new InstructionOnceElement();
			instructionOnceElement.m_BuildingBlockID = iBlockID;
			instructionOnceElement.m_iRandomSeed = seed;
			instructionOnceElement.m_XPosition = (sbyte)X;
			instructionOnceElement.m_YPosition = (sbyte)Y;
			int num = m_LevelInstructions.m_OnceList.m_iTotal++;
			if (m_LevelInstructions.m_OnceList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_OnceList.m_Instructions[num] = instructionOnceElement;
			}
			else
			{
				m_LevelInstructions.m_OnceList.m_Instructions.Add(instructionOnceElement);
			}
			m_LevelInstructions.m_OnceList.m_iTotalValid = num + 1;
			return num;
		}
		return -1;
	}

	public bool DeleteArea(int X, int Y, int iWidth, int iHeight)
	{
		bool result = false;
		if (X < 0 || X + iWidth > 120 || Y < 0 || Y + iHeight > 120)
		{
			return false;
		}
		CleanDeleteFlags();
		BaseLevelManager.TileProperty tileProperty = BaseLevelManager.TileProperty.EMPTY;
		BaseLevelManager.RoomProperty roomProperty = BaseLevelManager.RoomProperty.EMPTY;
		bool flag = false;
		bool flag2 = false;
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		LevelEditor_Controller instance2 = LevelEditor_Controller.GetInstance();
		AddStartUndo();
		if (instance != null && instance2 != null && m_LevelManager != null)
		{
			int totalZones = instance.GetTotalZones();
			BaseLevelManager.LevelLayers currentLayer = m_LevelManager.GetCurrentLayer();
			for (int i = 0; i < totalZones; i++)
			{
				LevelEditor_ZoneManager.Zone zone = instance.GetZone(i);
				if (zone != null && zone.m_Layer == currentLayer && zone.m_Left >= X && zone.m_Bottom >= Y && zone.m_Left + zone.m_Width <= X + iWidth && zone.m_Bottom + zone.m_Height <= Y + iHeight)
				{
					instance2.DeleteZone(zone);
				}
			}
		}
		if (m_BlockManager != null)
		{
			m_Delete_IndexIntoLayerData = Y * 120 + X;
			m_Delete_LayerData = m_LevelManager.m_BuildingLayers[(uint)m_LevelManager.m_CurrentLayer];
			m_DeleteZoneMap = LevelEditor_ZoneManager.GetInstance().GetZoneMap(m_LevelManager.m_CurrentLayer).m_Map;
			m_Delete_ComplexElement = new InstructionComplexElement();
			int num = 120 - iWidth;
			int num2 = Y + iHeight;
			int num3 = X + iWidth;
			int num4 = 1;
			if (m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.GroundFloor_Vent || m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.FirstFloor_Vent || m_IgnoreChecks)
			{
				m_Delete_Allowed = AllowedToDelete.ALL;
			}
			else if (m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.GroundFloor)
			{
				m_Delete_Allowed = AllowedToDelete.Walls | AllowedToDelete.Objects;
			}
			else
			{
				if (m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.Roof)
				{
					num4 = ((m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.FirstFloor_Vent && m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.GroundFloor_Vent) ? 3 : 2);
				}
				BaseLevelManager.TileProperty[] array = new BaseLevelManager.TileProperty[3]
				{
					BaseLevelManager.TileProperty.WallAndObjects,
					BaseLevelManager.TileProperty.BlockBitMask,
					BaseLevelManager.TileProperty.BlockBitMask
				};
				int num5 = m_Delete_IndexIntoLayerData;
				for (int j = Y; j < num2; j++)
				{
					for (int k = X; k < num3; k++)
					{
						for (int l = 0; l < num4; l++)
						{
							BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(int)m_LevelManager.m_CurrentLayer + l];
							tileProperty = layerDataCollection.m_TileProperties[num5];
							if ((tileProperty & array[l]) != 0)
							{
								if (l == 0)
								{
									flag2 = true;
								}
								else
								{
									flag = true;
								}
							}
							else if (layerDataCollection.m_RoomPropertiesMasks[num5] != 0)
							{
								if (l == 0)
								{
									flag2 = true;
								}
								else
								{
									flag = true;
								}
							}
						}
						num5++;
					}
					num5 += num;
				}
				if (!flag2)
				{
					m_Delete_Allowed = AllowedToDelete.ALL;
				}
				else
				{
					m_Delete_Allowed = AllowedToDelete.Walls | AllowedToDelete.Objects;
				}
			}
			for (m_Delete_Y = Y; m_Delete_Y < num2; m_Delete_Y++)
			{
				for (m_Delete_X = X; m_Delete_X < num3; m_Delete_X++)
				{
					tileProperty = m_Delete_LayerData.m_TileProperties[m_Delete_IndexIntoLayerData];
					int iZoneID = m_DeleteZoneMap[m_Delete_IndexIntoLayerData];
					if (m_Delete_LayerData.m_RoomPropertiesMasks[m_Delete_IndexIntoLayerData] != 0)
					{
						if ((tileProperty & BaseLevelManager.TileProperty.DeleteRoomMask) != BaseLevelManager.TileProperty.DeleteRoomMask)
						{
							int num6 = m_Delete_LayerData.m_RoomIDs[m_Delete_IndexIntoLayerData];
							int num7 = m_Delete_LayerData.m_RoomIDs[m_Delete_IndexIntoLayerData + 14400];
							int num8 = m_Delete_LayerData.m_RoomIDs[m_Delete_IndexIntoLayerData + 28800];
							int num9 = m_Delete_LayerData.m_RoomIDs[m_Delete_IndexIntoLayerData + 43200];
							if (num6 != 0)
							{
								DeleteComplex(num6);
							}
							if (num7 != 0)
							{
								DeleteComplex(num7);
							}
							if (num8 != 0)
							{
								DeleteComplex(num8);
							}
							if (num9 != 0)
							{
								DeleteComplex(num9);
							}
						}
					}
					else if (!flag2 && flag)
					{
						bool flag3 = true;
						for (int m = 1; m < num4; m++)
						{
							if ((m_LevelManager.m_BuildingLayers[(int)m_LevelManager.m_CurrentLayer + m].m_TileProperties[m_Delete_IndexIntoLayerData] & BaseLevelManager.TileProperty.BlockBitMask) != 0)
							{
								flag3 = false;
								break;
							}
						}
						if (flag3)
						{
							DeleteAtXY(tileProperty, iZoneID);
						}
					}
					else
					{
						DeleteAtXY(tileProperty, iZoneID);
					}
					m_Delete_IndexIntoLayerData++;
				}
				m_Delete_IndexIntoLayerData += num;
			}
			if (m_Delete_ComplexElement.m_ComplexInstructions.m_iTotal != 0)
			{
				int num10 = m_LevelInstructions.m_ComplexList.m_iTotal++;
				if (m_LevelInstructions.m_ComplexList.m_Instructions.Count != num10)
				{
					m_LevelInstructions.m_ComplexList.m_Instructions[num10] = m_Delete_ComplexElement;
				}
				else
				{
					m_LevelInstructions.m_ComplexList.m_Instructions.Add(m_Delete_ComplexElement);
				}
				m_LevelInstructions.m_ComplexList.m_iTotalValid = num10 + 1;
				AddToCurrentList(num10, BaseBuildInstruction.InstructionTypeEnum.Complex);
				RunInstruction(-1, bStorePrevious: true);
				m_LevelManager.UpdateTiles();
				result = true;
			}
			m_Delete_ComplexElement = null;
			m_Delete_LayerData = null;
			m_DeleteZoneMap = new int[0];
		}
		AddEndUndo();
		return result;
	}

	public bool DeleteComplex(int iComplexRoomIndex)
	{
		m_Delete_ComplexRoomNumber = iComplexRoomIndex;
		BaseLevelManager.LayerDataCollection delete_LayerData = m_Delete_LayerData;
		int delete_X = m_Delete_X;
		int delete_Y = m_Delete_Y;
		AllowedToDelete delete_Allowed = m_Delete_Allowed;
		int delete_IndexIntoLayerData = m_Delete_IndexIntoLayerData;
		int delete_IndexIntoLayerData2 = m_Delete_IndexIntoLayerData;
		m_Delete_Allowed = AllowedToDelete.ALL;
		int indexOfCommandMatchingValues = GetInstance().GetIndexOfCommandMatchingValues(CommandsEnum.End_Room, bValue: false, 0, bPrevious: true, m_Delete_ComplexRoomNumber);
		int num = AddCommandToCommandList(CommandsEnum.Start_DeleteRoom, indexOfCommandMatchingValues);
		if (num != -1)
		{
			AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Command);
		}
		BaseLevelManager.TileProperty tileProperty = BaseLevelManager.TileProperty.EMPTY;
		BaseLevelManager.LevelLayers currentLayer = m_LevelManager.m_CurrentLayer;
		for (int i = 1; i < 6; i++)
		{
			m_LevelManager.m_CurrentLayer = (BaseLevelManager.LevelLayers)i;
			m_Delete_IndexIntoLayerData = 0;
			m_Delete_LayerData = m_LevelManager.m_BuildingLayers[i];
			num = AddCommandToCommandList(CommandsEnum.Set_Layer, i);
			if (num != -1)
			{
				AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Command);
			}
			m_Delete_Allowed |= AllowedToDelete.Walls;
			for (m_Delete_Y = 0; m_Delete_Y < 120; m_Delete_Y++)
			{
				for (m_Delete_X = 0; m_Delete_X < 120; m_Delete_X++)
				{
					if (BaseLevelManager.IsRoomNumberInProperty(ref m_Delete_LayerData, m_Delete_IndexIntoLayerData, iComplexRoomIndex))
					{
						tileProperty = m_Delete_LayerData.m_TileProperties[m_Delete_IndexIntoLayerData];
						if (DeleteAtXY(tileProperty, -1))
						{
							BaseLevelManager.TileProperty[] tileProperties;
							int delete_IndexIntoLayerData3;
							(tileProperties = m_Delete_LayerData.m_TileProperties)[delete_IndexIntoLayerData3 = m_Delete_IndexIntoLayerData] = tileProperties[delete_IndexIntoLayerData3] | BaseLevelManager.TileProperty.DeleteRoomMask;
						}
					}
					m_Delete_IndexIntoLayerData++;
				}
			}
		}
		num = AddCommandToCommandList(CommandsEnum.Set_Layer, (int)currentLayer);
		if (num != -1)
		{
			AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Command);
		}
		num = AddCommandToCommandList(CommandsEnum.End_DeleteRoom, indexOfCommandMatchingValues);
		if (num != -1)
		{
			AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Command);
		}
		m_LevelManager.m_CurrentLayer = currentLayer;
		m_Delete_LayerData = delete_LayerData;
		m_Delete_X = delete_X;
		m_Delete_Y = delete_Y;
		m_Delete_Allowed = delete_Allowed;
		m_Delete_IndexIntoLayerData = delete_IndexIntoLayerData;
		m_Delete_IndexIntoLayerData = delete_IndexIntoLayerData2;
		m_Delete_ComplexRoomNumber = 0;
		return true;
	}

	private void CleanDeleteFlags()
	{
		if (m_LevelManager != null)
		{
			for (int num = 14399; num >= 0; num--)
			{
				int num2;
				BaseLevelManager.TileProperty[] tileProperties;
				(tileProperties = m_LevelManager.m_BuildingLayers[1].m_TileProperties)[num2 = num] = tileProperties[num2] & BaseLevelManager.TileProperty.InverseDeleteFlags;
				int num3;
				(tileProperties = m_LevelManager.m_BuildingLayers[2].m_TileProperties)[num3 = num] = tileProperties[num3] & BaseLevelManager.TileProperty.InverseDeleteFlags;
				int num4;
				(tileProperties = m_LevelManager.m_BuildingLayers[3].m_TileProperties)[num4 = num] = tileProperties[num4] & BaseLevelManager.TileProperty.InverseDeleteFlags;
				int num5;
				(tileProperties = m_LevelManager.m_BuildingLayers[4].m_TileProperties)[num5 = num] = tileProperties[num5] & BaseLevelManager.TileProperty.InverseDeleteFlags;
				int num6;
				(tileProperties = m_LevelManager.m_BuildingLayers[5].m_TileProperties)[num6 = num] = tileProperties[num6] & BaseLevelManager.TileProperty.InverseDeleteFlags;
			}
		}
	}

	public bool DeleteAtXY(BaseLevelManager.TileProperty prop, int iZoneID)
	{
		int num = 0;
		bool result = false;
		if ((m_Delete_Allowed & AllowedToDelete.Walls) == AllowedToDelete.Walls && (m_Delete_ComplexRoomNumber != 0 || (prop & BaseLevelManager.TileProperty.DeleteWallMask) == 0) && (prop & BaseLevelManager.TileProperty.WallMask) != 0)
		{
			BaseLevelManager.TileIDData buildingBrickID = m_Delete_LayerData.m_WallTileIDs[m_Delete_IndexIntoLayerData];
			BuildingBlock_Wall buildingBlock_Wall = BuildingBlockManager.GetBlock(buildingBrickID) as BuildingBlock_Wall;
			if (buildingBlock_Wall != null && !buildingBlock_Wall.m_AutomaticBlock)
			{
				num = AddDeleteToDeleteList(m_Delete_X, m_Delete_Y, InstructionDeleteElement.DeleteType.Wall);
				if (num != -1)
				{
					BaseLevelManager.TileProperty[] tileProperties;
					int delete_IndexIntoLayerData;
					(tileProperties = m_Delete_LayerData.m_TileProperties)[delete_IndexIntoLayerData = m_Delete_IndexIntoLayerData] = tileProperties[delete_IndexIntoLayerData] | BaseLevelManager.TileProperty.DeleteWallMask;
					AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Delete);
					result = true;
				}
				if (buildingBlock_Wall.m_FloorTileID != -1)
				{
					int num2 = -1;
					num2 = (((prop & BaseLevelManager.TileProperty.EnvironmentMask) != BaseLevelManager.TileProperty.EnvironmentMask) ? BuildingBlockManager.GetDefaultLayerBlock(m_LevelManager.m_CurrentLayer, BaseLevelManager.LayersEnvironment.Outside) : BuildingBlockManager.GetDefaultLayerBlock(m_LevelManager.m_CurrentLayer, BaseLevelManager.LayersEnvironment.Inside));
					if (num2 != -1 && !BuildingBlockManager.GetBlock(num2).m_AutomaticBlock)
					{
						num = AddTileToTileList(m_Delete_X, m_Delete_Y, num2, UnityEngine.Random.Range(0, 10000));
						if (num != -1)
						{
							AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Draw_Once);
						}
					}
				}
			}
		}
		if ((m_Delete_Allowed & AllowedToDelete.Tiles) == AllowedToDelete.Tiles && (m_Delete_ComplexRoomNumber != 0 || (prop & BaseLevelManager.TileProperty.DeleteTileMask) == 0) && (prop & BaseLevelManager.TileProperty.TileMask) != 0 && iZoneID == -1)
		{
			num = AddDeleteToDeleteList(m_Delete_X, m_Delete_Y, InstructionDeleteElement.DeleteType.Tile);
			if (num != -1)
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int delete_IndexIntoLayerData2;
				(tileProperties = m_Delete_LayerData.m_TileProperties)[delete_IndexIntoLayerData2 = m_Delete_IndexIntoLayerData] = tileProperties[delete_IndexIntoLayerData2] | BaseLevelManager.TileProperty.DeleteTileMask;
				AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Delete);
				result = true;
			}
			int num3 = -1;
			if (!m_IgnoreChecks)
			{
				num3 = (((prop & BaseLevelManager.TileProperty.EnvironmentMask) != BaseLevelManager.TileProperty.EnvironmentMask) ? BuildingBlockManager.GetDefaultLayerBlock(m_LevelManager.m_CurrentLayer, BaseLevelManager.LayersEnvironment.Outside) : BuildingBlockManager.GetDefaultLayerBlock(m_LevelManager.m_CurrentLayer, BaseLevelManager.LayersEnvironment.Inside));
			}
			if (num3 != -1 && !BuildingBlockManager.GetBlock(num3).m_AutomaticBlock && m_Delete_ComplexRoomNumber != 0)
			{
				num = AddTileToTileList(m_Delete_X, m_Delete_Y, num3, UnityEngine.Random.Range(0, 10000));
				if (num != -1)
				{
					AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Draw_Once);
					result = true;
				}
			}
		}
		if ((m_Delete_Allowed & AllowedToDelete.Objects) == AllowedToDelete.Objects && (prop & BaseLevelManager.TileProperty.DeleteObjMask) == 0 && ((prop & BaseLevelManager.TileProperty.ObjectMask) != 0 || (prop & BaseLevelManager.TileProperty.DecorationMask) != 0))
		{
			GameObject gameObject = m_Delete_LayerData.m_ObjectTileObjects[m_Delete_IndexIntoLayerData];
			if (gameObject != null)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock((int)(m_Delete_LayerData.m_ObjectTileIDs[m_Delete_IndexIntoLayerData] & BaseLevelManager.TileIDData.IDMask));
				if (block != null)
				{
					float num4 = gameObject.transform.localPosition.x - m_LevelManager.m_fPositionOffsetsX[(int)block.BlockType];
					float num5 = gameObject.transform.localPosition.y - m_LevelManager.m_fPositionOffsetsY[(int)block.BlockType];
					GameObject visualRep = block.GetVisualRep(0);
					num4 -= visualRep.transform.localPosition.x;
					num5 -= visualRep.transform.localPosition.y;
					num = -1;
					result = true;
					int num6 = (int)num4 + block.m_Footprint.m_iLeft;
					int num7 = (int)num5 + block.m_Footprint.m_iBottom;
					int num8 = Mathf.Clamp(num6 + block.m_Footprint.m_iW, 0, 120);
					int num9 = Mathf.Clamp(num7 + block.m_Footprint.m_iH, 0, 120);
					int num10 = 0;
					for (int i = num7; i < num9; i++)
					{
						for (int j = num6; j < num8; j++)
						{
							if ((block.m_Footprint.m_UsedTiles[num10++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
							{
								if (num == -1)
								{
									num = AddDeleteToDeleteList(j, i, InstructionDeleteElement.DeleteType.Object);
									AddToInstructionList(m_Delete_ComplexElement.m_ComplexInstructions, num, BaseBuildInstruction.InstructionTypeEnum.Delete);
								}
								BaseLevelManager.TileProperty[] tileProperties;
								int num11;
								(tileProperties = m_Delete_LayerData.m_TileProperties)[num11 = i * 120 + j] = tileProperties[num11] | BaseLevelManager.TileProperty.DeleteObjMask;
							}
						}
					}
				}
			}
			else
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int delete_IndexIntoLayerData3;
				(tileProperties = m_Delete_LayerData.m_TileProperties)[delete_IndexIntoLayerData3 = m_Delete_IndexIntoLayerData] = tileProperties[delete_IndexIntoLayerData3] & BaseLevelManager.TileProperty.InverseObjDecMask;
			}
		}
		return result;
	}

	public bool AddToCurrentList(int iIndex, BaseBuildInstruction.InstructionTypeEnum insType)
	{
		InstructionList instructionList = null;
		instructionList = ((m_CurrentList == -2) ? m_TempComplex.m_ComplexInstructions : ((m_CurrentList != -1) ? m_LevelInstructions.m_ComplexList.m_Instructions[m_CurrentList].m_ComplexInstructions : m_LevelInstructions.m_UserInstructions));
		int num = instructionList.m_iTotal++;
		if (instructionList.m_Instructions.Count != num)
		{
			instructionList.m_Instructions[num] = new InstructionListElement(iIndex, insType);
		}
		else
		{
			instructionList.m_Instructions.Add(new InstructionListElement(iIndex, insType));
		}
		instructionList.m_iTotalValid = num + 1;
		return true;
	}

	public bool AddToInstructionList(InstructionList insList, int iIndex, BaseBuildInstruction.InstructionTypeEnum insType)
	{
		int num = insList.m_iTotal++;
		if (insList.m_Instructions.Count != num)
		{
			insList.m_Instructions[num] = new InstructionListElement(iIndex, insType);
		}
		else
		{
			insList.m_Instructions.Add(new InstructionListElement(iIndex, insType));
		}
		insList.m_iTotalValid = num + 1;
		return true;
	}

	public bool TimeSlicedRunAllInstructions(long iMilliseconds = 300L)
	{
		CacheManagers();
		m_TimeSlicedStopWatch.Reset();
		if (!m_StartedTimeSlice)
		{
			m_PreviousStartedTimeSliceList = m_CurrentList;
			m_CurrentList = -1;
			m_CurrentStartedTimeSliceIndex = 0;
			m_StartedTimeSlice = true;
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (instance.GetLevelType() == LevelDetailsManager.LevelType.WorkInProgress)
		{
			int iTotal = m_LevelInstructions.m_UserInstructions.m_iTotal;
			m_TimeSlicedStopWatch.Start();
			while (m_TimeSlicedStopWatch.ElapsedMilliseconds < iMilliseconds)
			{
				if (m_CurrentStartedTimeSliceIndex >= iTotal)
				{
					m_StartedTimeSlice = false;
					m_CurrentList = m_PreviousStartedTimeSliceList;
					return true;
				}
				RunInstruction(m_CurrentStartedTimeSliceIndex++, bStorePrevious: true);
			}
			return false;
		}
		m_TimeSlicedStopWatch.Start();
		while (!m_LevelInstructions.m_FinishedLevel.ProcessNextInstruction(this))
		{
			if (m_TimeSlicedStopWatch.ElapsedMilliseconds > iMilliseconds)
			{
				return false;
			}
		}
		return true;
	}

	public bool TimeSlicedFinishedInstructions(long iMilliseconds = 300L)
	{
		CacheManagers();
		m_TimeSlicedStopWatch.Reset();
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		m_TimeSlicedStopWatch.Start();
		while (!m_LevelInstructions.m_FinishedLevel.ProcessNextInstruction(this))
		{
			if (m_TimeSlicedStopWatch.ElapsedMilliseconds > iMilliseconds)
			{
				return false;
			}
		}
		return true;
	}

	public void RunAllInstructions(bool bStorePrevious)
	{
		CacheManagers();
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null && instance.GetLevelType() == LevelDetailsManager.LevelType.Finished)
		{
			while (!m_LevelInstructions.m_FinishedLevel.ProcessNextInstruction(this))
			{
			}
			return;
		}
		int currentList = m_CurrentList;
		m_CurrentList = -1;
		int iTotal = m_LevelInstructions.m_UserInstructions.m_iTotal;
		for (int i = 0; i < iTotal; i++)
		{
			RunInstruction(i, bStorePrevious);
		}
		m_CurrentList = currentList;
	}

	public virtual void RunInstruction(int iIndex, bool bStorePrevious, bool bIncreaseTotals = false)
	{
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
					InstructionCommandElement obj4 = m_LevelInstructions.m_CommandList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_CommandList.m_iTotal = index + 1;
					}
					m_LevelManager.AddCommand(ref obj4, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
				if (m_LevelInstructions.m_OnceList.m_iTotalValid > index)
				{
					InstructionOnceElement obj2 = m_LevelInstructions.m_OnceList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_OnceList.m_iTotal = index + 1;
					}
					m_LevelManager.AddSingle(ref obj2, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall:
				if (m_LevelInstructions.m_OnceWallList.m_iTotalValid > index)
				{
					InstructionOnceWallElement obj5 = m_LevelInstructions.m_OnceWallList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_OnceWallList.m_iTotal = index + 1;
					}
					m_LevelManager.AddSingleWall(ref obj5, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
				if (m_LevelInstructions.m_AreaList.m_iTotalValid > index)
				{
					InstructionAreaElement obj3 = m_LevelInstructions.m_AreaList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_AreaList.m_iTotal = index + 1;
					}
					m_LevelManager.AddArea(ref obj3, bStorePrevious);
				}
				break;
			case BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall:
				if (m_LevelInstructions.m_AreaWallList.m_iTotalValid > index)
				{
					InstructionAreaWallElement obj6 = m_LevelInstructions.m_AreaWallList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_AreaWallList.m_iTotal = index + 1;
					}
					m_LevelManager.AddAreaWall(ref obj6, bStorePrevious);
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
					InstructionDeleteElement obj = m_LevelInstructions.m_DeleteList.m_Instructions[index];
					if (bIncreaseTotals)
					{
						m_LevelInstructions.m_DeleteList.m_iTotal = index + 1;
					}
					m_LevelManager.AddDelete(ref obj, bStorePrevious);
				}
				break;
			}
		}
		m_CurrentList = currentList;
	}

	public void UpdateLevel()
	{
		if (m_LevelManager != null)
		{
			m_LevelManager.UpdateTiles();
		}
	}

	[ContextMenu("Set All to Changed")]
	public void SetAllChanged()
	{
		if (m_LevelManager == null)
		{
			return;
		}
		m_LevelManager.InitializeData();
		for (int i = 1; i < 6; i++)
		{
			m_LevelManager.m_BuildingLayers[i].m_Changed = true;
			for (int j = 0; j < 14400; j++)
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int num;
				(tileProperties = m_LevelManager.m_BuildingLayers[i].m_TileProperties)[num = j] = tileProperties[num] | BaseLevelManager.TileProperty.ChangedMask;
			}
		}
		m_LevelManager.UpdateTiles();
	}

	[ContextMenu("Reset Contents")]
	public void ResetContents()
	{
		m_LevelInstructions.m_UserInstructions.m_iTotal = 0;
		m_LevelInstructions.m_UserInstructions.m_iTotalValid = 0;
		m_LevelInstructions.m_UserInstructions.m_Instructions.Clear();
		m_LevelInstructions.m_ComplexList.m_iTotal = 0;
		m_LevelInstructions.m_ComplexList.m_iTotalValid = 0;
		m_LevelInstructions.m_ComplexList.m_Instructions.Clear();
		m_LevelInstructions.m_OnceList.m_iTotal = 0;
		m_LevelInstructions.m_OnceList.m_iTotalValid = 0;
		m_LevelInstructions.m_OnceList.m_Instructions.Clear();
		m_LevelInstructions.m_OnceWallList.m_iTotal = 0;
		m_LevelInstructions.m_OnceWallList.m_iTotalValid = 0;
		m_LevelInstructions.m_OnceWallList.m_Instructions.Clear();
		m_LevelInstructions.m_AreaList.m_iTotal = 0;
		m_LevelInstructions.m_AreaList.m_iTotalValid = 0;
		m_LevelInstructions.m_AreaList.m_Instructions.Clear();
		m_LevelInstructions.m_AreaWallList.m_iTotal = 0;
		m_LevelInstructions.m_AreaWallList.m_iTotalValid = 0;
		m_LevelInstructions.m_AreaWallList.m_Instructions.Clear();
		m_LevelInstructions.m_CommandList.m_iTotal = 0;
		m_LevelInstructions.m_CommandList.m_iTotalValid = 0;
		m_LevelInstructions.m_CommandList.m_Instructions.Clear();
		m_LevelInstructions.m_DeleteList.m_iTotal = 0;
		m_LevelInstructions.m_DeleteList.m_iTotalValid = 0;
		m_LevelInstructions.m_DeleteList.m_Instructions.Clear();
		m_LevelInstructions.m_ZoneList.m_iTotal = 0;
		m_LevelInstructions.m_ZoneList.m_iTotalValid = 0;
		m_LevelInstructions.m_ZoneList.m_Instructions.Clear();
		m_CurrentList = -1;
		m_BuildingFromComplex = false;
		m_BlockManager = BuildingBlockManager.GetInstance();
		m_LevelManager = BaseLevelManager.GetInstance();
	}

	public bool CanUndo()
	{
		if (m_LevelInstructions.m_UserInstructions.m_iTotal == 0)
		{
			return false;
		}
		if (m_LevelInstructions.m_UserInstructions.m_Instructions[m_LevelInstructions.m_UserInstructions.m_iTotal - 1].m_Type == BaseBuildInstruction.InstructionTypeEnum.PreventUndo)
		{
			return false;
		}
		return true;
	}

	public bool CanRedo()
	{
		int iTotal = m_LevelInstructions.m_UserInstructions.m_iTotal;
		int iTotalValid = m_LevelInstructions.m_UserInstructions.m_iTotalValid;
		if (iTotalValid <= iTotal)
		{
			return false;
		}
		return true;
	}

	public void Undo()
	{
		int num = m_LevelInstructions.m_UserInstructions.m_iTotal;
		if (num == 0)
		{
			return;
		}
		do
		{
			num = (m_LevelInstructions.m_UserInstructions.m_iTotal = num - 1);
			List<InstructionListElement> ins = new List<InstructionListElement>();
			int index = m_LevelInstructions.m_UserInstructions.m_Instructions[num].m_Index;
			BaseBuildInstruction.InstructionTypeEnum type = m_LevelInstructions.m_UserInstructions.m_Instructions[num].m_Type;
			ins.Add(new InstructionListElement(index, type));
			if (type == BaseBuildInstruction.InstructionTypeEnum.Complex)
			{
				MakeInstructionFromComplex(ref ins, index);
			}
			for (int num2 = ins.Count - 1; num2 >= 0; num2--)
			{
				index = ins[num2].m_Index;
				switch (ins[num2].m_Type)
				{
				case BaseBuildInstruction.InstructionTypeEnum.Draw_Once:
				{
					InstructionOnceElement obj7 = m_LevelInstructions.m_OnceList.m_Instructions[--m_LevelInstructions.m_OnceList.m_iTotal];
					m_LevelManager.RemoveSingle(ref obj7);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Draw_OnceWall:
				{
					InstructionOnceWallElement obj6 = m_LevelInstructions.m_OnceWallList.m_Instructions[--m_LevelInstructions.m_OnceWallList.m_iTotal];
					m_LevelManager.RemoveSingleWall(ref obj6);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Draw_Area:
				{
					InstructionAreaElement obj5 = m_LevelInstructions.m_AreaList.m_Instructions[--m_LevelInstructions.m_AreaList.m_iTotal];
					m_LevelManager.RemoveArea(ref obj5);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Draw_AreaWall:
				{
					InstructionAreaWallElement obj4 = m_LevelInstructions.m_AreaWallList.m_Instructions[--m_LevelInstructions.m_AreaWallList.m_iTotal];
					m_LevelManager.RemoveAreaWall(ref obj4);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Complex:
					m_LevelInstructions.m_ComplexList.m_iTotal--;
					break;
				case BaseBuildInstruction.InstructionTypeEnum.Command:
				{
					InstructionCommandElement obj3 = m_LevelInstructions.m_CommandList.m_Instructions[--m_LevelInstructions.m_CommandList.m_iTotal];
					m_LevelManager.RemoveCommand(ref obj3);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Delete:
				{
					InstructionDeleteElement obj2 = m_LevelInstructions.m_DeleteList.m_Instructions[--m_LevelInstructions.m_DeleteList.m_iTotal];
					m_LevelManager.RestoreDelete(ref obj2);
					break;
				}
				case BaseBuildInstruction.InstructionTypeEnum.Zone:
				{
					InstructionZoneElement obj = m_LevelInstructions.m_ZoneList.m_Instructions[--m_LevelInstructions.m_ZoneList.m_iTotal];
					switch (obj.m_Action)
					{
					case InstructionZoneElement.ZoneAction.Create:
						m_LevelManager.DeleteZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Delete:
						m_LevelManager.CreateZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Add:
						m_LevelManager.SubtractFromZone(ref obj);
						break;
					case InstructionZoneElement.ZoneAction.Subtract:
						m_LevelManager.AddToZone(ref obj);
						break;
					}
					break;
				}
				}
			}
		}
		while (m_LevelManager.m_UndoCount != 0 && num > 0);
		m_LevelManager.UpdateTiles();
	}

	public int GetIndexOfCommandMatchingValues(CommandsEnum type, bool bValue, int iValue, bool bPrevious, int iPrevious)
	{
		for (int num = m_LevelInstructions.m_CommandList.m_iTotal - 1; num >= 0; num--)
		{
			if (m_LevelInstructions.m_CommandList.m_Instructions[num] != null && m_LevelInstructions.m_CommandList.m_Instructions[num].m_Commmand == type && (!bValue || m_LevelInstructions.m_CommandList.m_Instructions[num].m_Value == iValue) && (!bPrevious || m_LevelInstructions.m_CommandList.m_Instructions[num].m_PreviousValue == iPrevious))
			{
				return num;
			}
		}
		return -1;
	}

	public void ModifyPreviousCommandValue(CommandsEnum type, int oldValue, int newValue)
	{
		int indexOfCommandMatchingValues = GetIndexOfCommandMatchingValues(type, bValue: true, oldValue, bPrevious: false, 0);
		if (indexOfCommandMatchingValues != -1)
		{
			m_LevelInstructions.m_CommandList.m_Instructions[indexOfCommandMatchingValues].m_Value = newValue;
		}
	}

	public void ModifyPreviousCommandPreviousValue(CommandsEnum type, int oldValue, int newValue)
	{
		int indexOfCommandMatchingValues = GetIndexOfCommandMatchingValues(type, bValue: false, 0, bPrevious: true, oldValue);
		if (indexOfCommandMatchingValues != -1)
		{
			m_LevelInstructions.m_CommandList.m_Instructions[indexOfCommandMatchingValues].m_PreviousValue = newValue;
		}
	}

	public void SetCommandValue(int iIndex, int iValue)
	{
		if (m_LevelInstructions.m_CommandList.m_iTotalValid > iIndex)
		{
			m_LevelInstructions.m_CommandList.m_Instructions[iIndex].m_Value = iValue;
		}
	}

	public void SetCommandPreviousValue(int iIndex, int iPreviousValue)
	{
		if (m_LevelInstructions.m_CommandList.m_iTotalValid > iIndex)
		{
			m_LevelInstructions.m_CommandList.m_Instructions[iIndex].m_PreviousValue = iPreviousValue;
		}
	}

	public int GetCommandValue(int iIndex)
	{
		if (m_LevelInstructions.m_CommandList.m_iTotalValid > iIndex)
		{
			return m_LevelInstructions.m_CommandList.m_Instructions[iIndex].m_Value;
		}
		return -1;
	}

	public int GetCommandPreviousValue(int iIndex)
	{
		if (m_LevelInstructions.m_CommandList.m_iTotalValid > iIndex)
		{
			return m_LevelInstructions.m_CommandList.m_Instructions[iIndex].m_PreviousValue;
		}
		return -1;
	}

	public void MakeInstructionFromComplex(ref List<InstructionListElement> ins, int iList)
	{
		InstructionList complexInstructions = m_LevelInstructions.m_ComplexList.m_Instructions[iList].m_ComplexInstructions;
		int iTotal = complexInstructions.m_iTotal;
		for (int i = 0; i < iTotal; i++)
		{
			BaseBuildInstruction.InstructionTypeEnum type = complexInstructions.m_Instructions[i].m_Type;
			int index = complexInstructions.m_Instructions[i].m_Index;
			ins.Add(new InstructionListElement(index, type));
			if (type == BaseBuildInstruction.InstructionTypeEnum.Complex)
			{
				MakeInstructionFromComplex(ref ins, index);
			}
		}
	}

	public void Redo()
	{
		int currentList = m_CurrentList;
		m_CurrentList = -1;
		int iTotal = m_LevelInstructions.m_UserInstructions.m_iTotal;
		int iTotalValid = m_LevelInstructions.m_UserInstructions.m_iTotalValid;
		if (iTotalValid > iTotal)
		{
			do
			{
				RunInstruction(iTotal++, bStorePrevious: false, bIncreaseTotals: true);
				m_LevelInstructions.m_UserInstructions.m_iTotal++;
			}
			while (m_LevelManager.m_UndoCount != 0 && iTotalValid > iTotal);
			m_LevelManager.UpdateTiles();
		}
		m_CurrentList = currentList;
	}

	public void CollectLevelData(ref List<byte> dataCollected, ref int iIndex)
	{
	}

	public bool DeleteZone(LevelEditor_ZoneManager.Zone zone, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			InstructionZoneElement obj = new InstructionZoneElement();
			obj.m_Action = InstructionZoneElement.ZoneAction.Delete;
			obj.m_Left = (sbyte)zone.m_Left;
			obj.m_Bottom = (sbyte)zone.m_Bottom;
			obj.m_Width = (sbyte)zone.m_Width;
			obj.m_Height = (sbyte)zone.m_Height;
			obj.m_ZoneType = zone.m_ZoneType;
			obj.m_iID = zone.m_ID;
			obj.m_ZonePrint = new byte[zone.m_ZonePrint.Length];
			for (int num = zone.m_ZonePrint.Length - 1; num >= 0; num--)
			{
				obj.m_ZonePrint[num] = zone.m_ZonePrint[num];
			}
			int num2 = m_LevelInstructions.m_ZoneList.m_iTotal++;
			if (m_LevelInstructions.m_ZoneList.m_Instructions.Count != num2)
			{
				m_LevelInstructions.m_ZoneList.m_Instructions[num2] = obj;
			}
			else
			{
				m_LevelInstructions.m_ZoneList.m_Instructions.Add(obj);
			}
			m_LevelInstructions.m_ZoneList.m_iTotalValid = num2 + 1;
			if (m_LevelManager != null && !bDontRun)
			{
				m_LevelManager.DeleteZone(ref obj);
			}
			return AddToCurrentList(num2, BaseBuildInstruction.InstructionTypeEnum.Zone);
		}
		return false;
	}

	public bool CreateZone(ZoneDetailsManager.ZoneTypes eType, sbyte X, sbyte Y, sbyte Width, sbyte Height, byte[] zonePrint, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			InstructionZoneElement obj = new InstructionZoneElement();
			obj.m_Action = InstructionZoneElement.ZoneAction.Create;
			obj.m_ZoneType = eType;
			obj.m_Left = X;
			obj.m_Bottom = Y;
			obj.m_Width = Width;
			obj.m_Height = Height;
			obj.m_ZonePrint = zonePrint;
			int num = m_LevelInstructions.m_ZoneList.m_iTotal++;
			if (m_LevelInstructions.m_ZoneList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_ZoneList.m_Instructions[num] = obj;
			}
			else
			{
				m_LevelInstructions.m_ZoneList.m_Instructions.Add(obj);
			}
			m_LevelInstructions.m_ZoneList.m_iTotalValid = num + 1;
			if (m_LevelManager != null && !bDontRun)
			{
				m_LevelManager.CreateZone(ref obj);
			}
			return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Zone);
		}
		return false;
	}

	public bool AddToZone(int iID, sbyte X, sbyte Y, sbyte Width, sbyte Height, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			InstructionZoneElement obj = new InstructionZoneElement();
			obj.m_Action = InstructionZoneElement.ZoneAction.Add;
			obj.m_Left = X;
			obj.m_Bottom = Y;
			obj.m_Width = Width;
			obj.m_Height = Height;
			obj.m_iID = iID;
			int num = m_LevelInstructions.m_ZoneList.m_iTotal++;
			if (m_LevelInstructions.m_ZoneList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_ZoneList.m_Instructions[num] = obj;
			}
			else
			{
				m_LevelInstructions.m_ZoneList.m_Instructions.Add(obj);
			}
			m_LevelInstructions.m_ZoneList.m_iTotalValid = num + 1;
			if (m_LevelManager != null && !bDontRun)
			{
				m_LevelManager.AddToZone(ref obj);
			}
			return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Zone);
		}
		return false;
	}

	public bool SubtractFromZone(int iID, sbyte X, sbyte Y, sbyte Width, sbyte Height, bool bDontRun = false)
	{
		if (m_BlockManager != null)
		{
			InstructionZoneElement obj = new InstructionZoneElement();
			obj.m_Action = InstructionZoneElement.ZoneAction.Subtract;
			obj.m_Left = X;
			obj.m_Bottom = Y;
			obj.m_Width = Width;
			obj.m_Height = Height;
			obj.m_iID = iID;
			int num = m_LevelInstructions.m_ZoneList.m_iTotal++;
			if (m_LevelInstructions.m_ZoneList.m_Instructions.Count != num)
			{
				m_LevelInstructions.m_ZoneList.m_Instructions[num] = obj;
			}
			else
			{
				m_LevelInstructions.m_ZoneList.m_Instructions.Add(obj);
			}
			m_LevelInstructions.m_ZoneList.m_iTotalValid = num + 1;
			if (m_LevelManager != null && !bDontRun)
			{
				m_LevelManager.SubtractFromZone(ref obj);
			}
			return AddToCurrentList(num, BaseBuildInstruction.InstructionTypeEnum.Zone);
		}
		return false;
	}
}

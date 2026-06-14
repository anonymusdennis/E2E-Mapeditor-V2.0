using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using DataHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDetailsManager : MonoBehaviour
{
	public enum LevelState
	{
		Idle,
		CreateLevel_Pending,
		CreateLevel_LoadBuildingBlockScene,
		CreateLevel_WaitingOnBlocksScene,
		CreateLevel_GenerateBlocks,
		LoadLevel_Pending,
		LoadLevel_LoadInstructions,
		LoadLevel_LoadBuildingBlocksScene,
		LoadLevel_WaitingOnBlocksScene,
		LoadLevel_GenerateBlocks,
		LoadLevel_Build,
		SaveLevel_Pending,
		SaveLevel_SaveData,
		SetUpLevel_Collect,
		SetUpLevel_Process,
		MakeLevel_Init,
		MakeLevel_Pending,
		RemoveSelf,
		LoadLevel_UpdateLevel,
		LoadLevel_UpdateRegenerate,
		LoadLevel_ValidateZone,
		Success,
		Failed,
		UNKNOWN
	}

	public enum LevelEditorDataVersion
	{
		UNKNOWN,
		V1_InitialRelease,
		V2_AddedZoneEditing,
		ALL_VERSIONS
	}

	public enum RequestResultEnum
	{
		Failed,
		Success
	}

	public delegate void RequestResult(RequestResultEnum eResult);

	public enum LevelMode
	{
		Build,
		Play
	}

	public delegate void RoutineChanged(int iHour);

	public enum SerializationFlag : byte
	{
		ChunkEnd = 102,
		File_Header_V1 = 0,
		Type_Header_V1 = 1,
		Version_Header_V1 = 2,
		Diffeculty_Level_V1 = 3,
		Name_Header_V1 = 4,
		Desc_Header_V1 = 5,
		Author_Name_Header_V1 = 6,
		Edited_Name_Header_V1 = 7,
		Created_Date_Header_V1 = 8,
		Edited_Date_Header_V1 = 9,
		BuildingBlock_Type_V1 = 10,
		Routines_Header_V1 = 11,
		Inmates_Header_V1 = 12,
		Guards_Header_V1 = 13,
		Directory_Header_V1 = 14,
		RandomGroups_Header_V1 = 15,
		LastUploadedTo_Header_V1 = 16,
		Filter_Settings_V1 = 17,
		DataVersion_Header_V1 = 18,
		PrisonOutfitType_V1 = 19,
		PrisonMusicType_V1 = 20,
		Instruction_FinishedTotal = 100,
		Instruction_DrawOnce = 101,
		Instruction_DrawArea = 102,
		Instruction_ChangeArea = 103,
		Instruction_DoNothing = 104,
		Instruction_IncrementLayer = 105,
		Instruction_DecrementLayer = 106,
		Instruction_Zone = 107,
		LevelInstructions_V1 = 201,
		List_InstructionList_V1 = 202,
		List_Complex_V1 = 203,
		List_Single_V1 = 204,
		List_Wall_V1 = 205,
		List_Area_V1 = 206,
		List_Area_Wall_V1 = 207,
		List_Commands_V1 = 208,
		List_Delete_V1 = 209,
		List_Zone_V1 = 210
	}

	public enum LevelType : sbyte
	{
		UNKNOWN,
		WorkInProgress,
		Finished
	}

	public enum BuildingBlock_Type : sbyte
	{
		Standard,
		UNKNOWN
	}

	public struct MarkersFound
	{
		public sbyte m_X;

		public sbyte m_Y;

		public sbyte m_Layer;
	}

	public struct EscapeMarkersFound
	{
		public MarkersFound m_Marker;

		public int m_Block;
	}

	public struct MarkersFoundTyped
	{
		public MarkersFound m_Marker;

		public RoomMarker.MarkerType m_Type;
	}

	public class EscapeMarkersFoundV2
	{
		public List<int> m_Index = new List<int>();

		public int m_Layer;

		public int m_iInstanceID = -1;

		public int m_BlockID = -1;
	}

	public struct ErrorData
	{
		public enum ErrorType
		{
			BasicError,
			LocationError
		}

		public enum Severity
		{
			Warning,
			Error
		}

		public ErrorType m_ErrorType;

		public Severity m_Severity;

		public string m_ErrorString;

		public int m_Layer;

		public int m_X;

		public int m_Y;

		public int m_ErrorID;

		public ErrorData(ErrorType type, Severity sev, string strError, int iLayer, int iX, int iY)
		{
			m_ErrorType = type;
			m_Severity = sev;
			m_ErrorString = strError;
			m_Layer = iLayer;
			m_X = iX;
			m_Y = iY;
			m_ErrorID = -1;
		}

		public ErrorData(Severity sev, string strError, int iLayer, int iX, int iY)
		{
			m_ErrorType = ErrorType.LocationError;
			m_Severity = sev;
			m_ErrorString = strError;
			m_Layer = iLayer;
			m_X = iX;
			m_Y = iY;
			m_ErrorID = -1;
		}

		public ErrorData(Severity sev, string strError, int iLayer, int iX, int iY, int iErrorID = -1)
		{
			m_ErrorType = ErrorType.LocationError;
			m_Severity = sev;
			m_ErrorString = strError;
			m_Layer = iLayer;
			m_X = iX;
			m_Y = iY;
			m_ErrorID = iErrorID;
		}

		public ErrorData(Severity sev, string strError)
		{
			m_ErrorType = ErrorType.BasicError;
			m_Severity = sev;
			m_ErrorString = strError;
			m_Layer = 0;
			m_X = 0;
			m_Y = 0;
			m_ErrorID = -1;
		}

		public ErrorData(Severity sev, string strError, int iErrorID = -1)
		{
			m_ErrorType = ErrorType.BasicError;
			m_Severity = sev;
			m_ErrorString = strError;
			m_Layer = 0;
			m_X = 0;
			m_Y = 0;
			m_ErrorID = iErrorID;
		}

		public static bool DoesErrorExist(int iErrorID, ref List<ErrorData> listErrors)
		{
			if (iErrorID != -1)
			{
				for (int num = listErrors.Count - 1; num >= 0; num--)
				{
					if (listErrors[num].m_ErrorID == iErrorID)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void AddToErrorList(ErrorData errorData, ref List<ErrorData> listErrors)
		{
			if (!DoesErrorExist(errorData.m_ErrorID, ref listErrors))
			{
				listErrors.Add(errorData);
			}
		}
	}

	public enum DiffecultyLevel : sbyte
	{
		Easy,
		Medium,
		Hard
	}

	public class FrontendLevelDetails
	{
		public string m_LevelName = string.Empty;

		public string m_LevelDescription = string.Empty;

		public DiffecultyLevel m_DifficultyLevel = DiffecultyLevel.Medium;

		public int m_NumberOfGuards;

		public int m_NumberOfInmates;

		public LevelEditorDataVersion m_DataVersion = LevelEditorDataVersion.V1_InitialRelease;

		public LevelScript.PRISON_ENUM m_OutfitType = LevelScript.PRISON_ENUM.Centre_Perks;

		public LevelScript.PRISON_ENUM m_MusicType = LevelScript.PRISON_ENUM.Centre_Perks;

		public bool DeserializeFrontendData(ref List<byte> data)
		{
			int num = 0;
			num = FindHeader(ref data, 0, SerializationFlag.File_Header_V1);
			if (data[num] != 0)
			{
				return false;
			}
			num++;
			int @int = ByteArrayConversion.GetInt(ref data, ref num);
			int num2 = @int + num;
			int int2 = ByteArrayConversion.GetInt(ref data, ref num);
			int num3 = 0;
			for (int i = num; i < num2; i++)
			{
				num3 += data[i] ^ 0x7B;
			}
			if (int2 != num3)
			{
				num += @int;
				return false;
			}
			bool flag = true;
			int num4 = 0;
			while (flag && num2 >= num)
			{
				switch ((SerializationFlag)data[num++])
				{
				case SerializationFlag.ChunkEnd:
					return true;
				case SerializationFlag.Diffeculty_Level_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_DifficultyLevel = (DiffecultyLevel)data[num++];
					flag = data[num++] == 102;
					break;
				case SerializationFlag.DataVersion_Header_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_DataVersion = (LevelEditorDataVersion)ByteArrayConversion.GetInt(ref data, ref num);
					flag = data[num++] == 102;
					break;
				case SerializationFlag.PrisonMusicType_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_MusicType = (LevelScript.PRISON_ENUM)ByteArrayConversion.GetInt(ref data, ref num);
					flag = data[num++] == 102;
					break;
				case SerializationFlag.PrisonOutfitType_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_OutfitType = (LevelScript.PRISON_ENUM)ByteArrayConversion.GetInt(ref data, ref num);
					flag = data[num++] == 102;
					break;
				case SerializationFlag.Name_Header_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_LevelName = ByteArrayConversion.GetString(ref data, ref num, bEncript: true);
					flag = data[num++] == 102;
					break;
				case SerializationFlag.Desc_Header_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_LevelDescription = ByteArrayConversion.GetString(ref data, ref num, bEncript: true);
					flag = data[num++] == 102;
					break;
				case SerializationFlag.Guards_Header_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_NumberOfGuards = (sbyte)data[num++];
					flag = data[num++] == 102;
					break;
				case SerializationFlag.Inmates_Header_V1:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					m_NumberOfInmates = (sbyte)data[num++];
					flag = data[num++] == 102;
					break;
				default:
					num4 = ByteArrayConversion.GetInt(ref data, ref num);
					num += num4;
					break;
				}
			}
			return false;
		}
	}

	private class LevelDetails
	{
		public LevelType m_LevelType;

		public int m_LevelVersion;

		public DiffecultyLevel m_DiffecultyLevel = DiffecultyLevel.Medium;

		public BuildingBlock_Type m_BlockType;

		public string m_LevelName = string.Empty;

		public string m_LevelDescription = string.Empty;

		public string m_AuthorName = string.Empty;

		public string m_EditedName = string.Empty;

		public long m_DateCreated;

		public long m_DateLastEdited;

		public Routines[] m_Routines = new Routines[24];

		public sbyte m_NumberOfGuards;

		public sbyte m_NumberOfInmates;

		public string m_DirectoryName = string.Empty;

		public int[] m_RandomGroups = new int[5];

		public ulong m_LastUploadedToID;

		public LevelEditorDataVersion m_DataVersionNumber = LevelEditorDataVersion.V1_InitialRelease;

		public LevelScript.PRISON_ENUM m_OutfitType = LevelScript.PRISON_ENUM.Centre_Perks;

		public LevelScript.PRISON_ENUM m_MusicType = LevelScript.PRISON_ENUM.Centre_Perks;

		public void SerializeOurData(ref List<byte> dataCollection)
		{
			int num = 0;
			dataCollection.Add(0);
			int iIndex = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			int iIndex2 = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			dataCollection.Add(1);
			ByteArrayConversion.AddInt(2, ref dataCollection);
			dataCollection.Add((byte)m_LevelType);
			dataCollection.Add(102);
			dataCollection.Add(2);
			ByteArrayConversion.AddInt(5, ref dataCollection);
			ByteArrayConversion.AddInt(m_LevelVersion, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(18);
			ByteArrayConversion.AddInt(5, ref dataCollection);
			ByteArrayConversion.AddInt((int)m_DataVersionNumber, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(20);
			ByteArrayConversion.AddInt(5, ref dataCollection);
			ByteArrayConversion.AddInt((int)m_MusicType, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(19);
			ByteArrayConversion.AddInt(5, ref dataCollection);
			ByteArrayConversion.AddInt((int)m_OutfitType, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(3);
			ByteArrayConversion.AddInt(2, ref dataCollection);
			dataCollection.Add((byte)m_DiffecultyLevel);
			dataCollection.Add(102);
			dataCollection.Add(10);
			ByteArrayConversion.AddInt(2, ref dataCollection);
			dataCollection.Add((byte)m_BlockType);
			dataCollection.Add(102);
			dataCollection.Add(4);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddString(m_LevelName, ref dataCollection, bEncript: true);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(5);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddString(m_LevelDescription, ref dataCollection, bEncript: true);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(6);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddString(m_AuthorName, ref dataCollection, bEncript: true);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(7);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddString(m_EditedName, ref dataCollection, bEncript: true);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(8);
			ByteArrayConversion.AddInt(9, ref dataCollection);
			ByteArrayConversion.AddLong(m_DateCreated, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(9);
			ByteArrayConversion.AddInt(9, ref dataCollection);
			ByteArrayConversion.AddLong(m_DateLastEdited, ref dataCollection);
			dataCollection.Add(102);
			dataCollection.Add(11);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			dataCollection.Add((byte)m_Routines.Length);
			for (int i = 0; i < m_Routines.Length; i++)
			{
				dataCollection.Add((byte)m_Routines[i]);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(13);
			ByteArrayConversion.AddInt(2, ref dataCollection);
			dataCollection.Add((byte)m_NumberOfGuards);
			dataCollection.Add(102);
			dataCollection.Add(12);
			ByteArrayConversion.AddInt(2, ref dataCollection);
			dataCollection.Add((byte)m_NumberOfInmates);
			dataCollection.Add(102);
			dataCollection.Add(14);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddString(m_DirectoryName, ref dataCollection, bEncript: true);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(15);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			dataCollection.Add((byte)m_RandomGroups.Length);
			for (int j = 0; j < m_RandomGroups.Length; j++)
			{
				dataCollection.Add((byte)m_RandomGroups[j]);
			}
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(16);
			num = dataCollection.Count;
			ByteArrayConversion.AddInt(0, ref dataCollection);
			ByteArrayConversion.AddULong(m_LastUploadedToID, ref dataCollection);
			dataCollection.Add(102);
			ByteArrayConversion.StoreInt(dataCollection.Count - num - 4, ref dataCollection, ref num);
			dataCollection.Add(102);
			int count = dataCollection.Count;
			int num2 = 0;
			for (int k = iIndex2 + 4; k < count; k++)
			{
				num2 += dataCollection[k] ^ 0x7B;
			}
			ByteArrayConversion.StoreInt(num2, ref dataCollection, ref iIndex2);
			ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
		}

		public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
		{
			Reset();
			if (dataCollection[iIndex] != 0)
			{
				return false;
			}
			iIndex++;
			int @int = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int num = @int + iIndex;
			int int2 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
			int num2 = 0;
			for (int i = iIndex; i < num; i++)
			{
				num2 += dataCollection[i] ^ 0x7B;
			}
			if (int2 != num2)
			{
				iIndex += @int;
				return false;
			}
			bool flag = true;
			int num3 = 0;
			while (flag && num >= iIndex)
			{
				switch ((SerializationFlag)dataCollection[iIndex++])
				{
				case SerializationFlag.ChunkEnd:
					return true;
				case SerializationFlag.Type_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_LevelType = (LevelType)dataCollection[iIndex++];
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Version_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_LevelVersion = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.DataVersion_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_DataVersionNumber = (LevelEditorDataVersion)ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					c_CurrentLevelDataVersionNumber = m_DataVersionNumber;
					break;
				case SerializationFlag.PrisonMusicType_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_MusicType = (LevelScript.PRISON_ENUM)ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.PrisonOutfitType_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_OutfitType = (LevelScript.PRISON_ENUM)ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Diffeculty_Level_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_DiffecultyLevel = (DiffecultyLevel)dataCollection[iIndex++];
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.BuildingBlock_Type_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_BlockType = (BuildingBlock_Type)dataCollection[iIndex++];
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Name_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_LevelName = ByteArrayConversion.GetString(ref dataCollection, ref iIndex, bEncript: true);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Desc_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_LevelDescription = ByteArrayConversion.GetString(ref dataCollection, ref iIndex, bEncript: true);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Author_Name_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_AuthorName = ByteArrayConversion.GetString(ref dataCollection, ref iIndex, bEncript: true);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Edited_Name_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_EditedName = ByteArrayConversion.GetString(ref dataCollection, ref iIndex, bEncript: true);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Created_Date_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_DateCreated = ByteArrayConversion.GetLong(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Edited_Date_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_DateLastEdited = ByteArrayConversion.GetLong(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Routines_Header_V1:
				{
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_Routines = new Routines[dataCollection[iIndex++]];
					for (int k = 0; k < m_Routines.Length; k++)
					{
						m_Routines[k] = (Routines)dataCollection[iIndex++];
					}
					flag = dataCollection[iIndex++] == 102;
					if (GetInstance() != null)
					{
						GetInstance().RoutineHasChanged(0);
					}
					break;
				}
				case SerializationFlag.Guards_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_NumberOfGuards = (sbyte)dataCollection[iIndex++];
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Inmates_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_NumberOfInmates = (sbyte)dataCollection[iIndex++];
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.Directory_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_DirectoryName = ByteArrayConversion.GetString(ref dataCollection, ref iIndex, bEncript: true);
					flag = dataCollection[iIndex++] == 102;
					break;
				case SerializationFlag.RandomGroups_Header_V1:
				{
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_RandomGroups = new int[dataCollection[iIndex++]];
					for (int j = 0; j < m_RandomGroups.Length; j++)
					{
						m_RandomGroups[j] = dataCollection[iIndex++];
					}
					flag = dataCollection[iIndex++] == 102;
					break;
				}
				case SerializationFlag.LastUploadedTo_Header_V1:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					m_LastUploadedToID = ByteArrayConversion.GetULong(ref dataCollection, ref iIndex);
					flag = dataCollection[iIndex++] == 102;
					break;
				default:
					num3 = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
					iIndex += num3;
					break;
				}
			}
			return false;
		}

		public void Reset()
		{
			m_LevelType = LevelType.UNKNOWN;
			m_LevelVersion = 0;
			m_DiffecultyLevel = DiffecultyLevel.Medium;
			m_LevelName = string.Empty;
			m_LevelDescription = string.Empty;
			m_AuthorName = string.Empty;
			m_EditedName = string.Empty;
			m_DateCreated = 0L;
			m_DateLastEdited = 0L;
			m_DirectoryName = string.Empty;
			m_DataVersionNumber = LevelEditorDataVersion.V1_InitialRelease;
			c_CurrentLevelDataVersionNumber = LevelEditorDataVersion.V1_InitialRelease;
			m_MusicType = LevelScript.PRISON_ENUM.Centre_Perks;
			m_OutfitType = LevelScript.PRISON_ENUM.Centre_Perks;
			for (int num = m_Routines.Length - 1; num >= 0; num--)
			{
				if (num >= 8 && num <= 22)
				{
					m_Routines[num] = Routines.FreeTime;
				}
				else
				{
					m_Routines[num] = Routines.LightsOut;
				}
			}
			BuildingBlockManager instance = BuildingBlockManager.GetInstance();
			if (instance != null && instance.m_DefaultRoutines.Length == m_Routines.Length)
			{
				for (int num2 = m_Routines.Length - 1; num2 >= 0; num2--)
				{
					m_Routines[num2] = instance.m_DefaultRoutines[num2];
				}
			}
			if (GetInstance() != null)
			{
				GetInstance().RoutineHasChanged(0);
			}
			for (int i = 0; i < m_RandomGroups.Length; i++)
			{
				m_RandomGroups[i] = i;
			}
		}

		public static LevelEditorDataVersion GetMasterDataVersion()
		{
			return LevelEditorDataVersion.V2_AddedZoneEditing;
		}
	}

	private static LevelDetailsManager m_Instance;

	private string m_BuildingBlockSceneName = string.Empty;

	private string m_FileName = string.Empty;

	private bool m_bGenerateVisualBlocks = true;

	private LevelState m_State;

	private List<MarkersFound> m_SearchLayers = new List<MarkersFound>();

	private List<MarkersFound> m_SearchBlockers = new List<MarkersFound>();

	private const int c_ScanSize = 10000;

	private int[] m_ScanLocations = new int[10000];

	private int m_ScanFirstFree;

	private int m_ScanCurrentIndex;

	private int m_ScanTotalUsed;

	public static LevelEditorDataVersion c_CurrentLevelDataVersionNumber = LevelEditorDataVersion.V1_InitialRelease;

	private RequestResult m_RequestResult;

	public LevelMode m_Mode;

	public GameObject m_LevelMasterGameObject;

	public BaseLevelManager m_LevelManager;

	public BuildingInstructionManager m_InstructionManager;

	private BuildingBlockManager m_BlockManager;

	private BuildingBlock_Type m_LoadedBuildingBlocks = BuildingBlock_Type.UNKNOWN;

	public const int INVALID_ERROR_ID = -1;

	private List<BaseComponentSetup> m_SetupCompontents = new List<BaseComponentSetup>();

	private int m_SetupChecked;

	private bool m_bSetupListDirty;

	private LevelDetails m_LevelDetails = new LevelDetails();

	private List<byte> m_RawLevelData = new List<byte>();

	private List<byte> m_EncryptedLevelData = new List<byte>();

	private string m_SavePath = string.Empty;

	private bool m_bInitialized;

	private bool m_bDayBeforeMonth = true;

	private event RoutineChanged OnRoutineChanged;

	public static LevelDetailsManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		InitializeData();
	}

	private void Start()
	{
		if (m_LevelManager == null || m_InstructionManager == null)
		{
			base.enabled = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void InitializeData()
	{
		if (m_bInitialized)
		{
			return;
		}
		m_bInitialized = true;
		m_SavePath = Application.persistentDataPath + '\\' + "UserLevels" + '\\';
		string longDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
		int num = longDatePattern.IndexOf("d", StringComparison.OrdinalIgnoreCase);
		int num2 = longDatePattern.IndexOf("m", StringComparison.OrdinalIgnoreCase);
		if (num >= 0 && num2 >= 0 && num2 < num)
		{
			m_bDayBeforeMonth = false;
		}
		if (m_LevelManager == null)
		{
			m_LevelManager = GetComponent<BaseLevelManager>();
			if (m_LevelManager == null)
			{
				base.enabled = false;
			}
		}
		if (m_LevelManager != null)
		{
			m_LevelManager.enabled = false;
		}
		if (m_InstructionManager == null)
		{
			m_InstructionManager = GetComponent<BuildingInstructionManager>();
			if (m_InstructionManager == null)
			{
				base.enabled = false;
			}
		}
		m_LevelDetails.Reset();
	}

	public string GetLevelName()
	{
		return m_LevelDetails.m_LevelName;
	}

	public string GetLevelDirectory()
	{
		return m_LevelDetails.m_DirectoryName;
	}

	public string SetLevelName(string strLevelName)
	{
		if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().FilterString(ref strLevelName);
		}
		m_LevelDetails.m_LevelName = strLevelName;
		return m_LevelDetails.m_LevelName;
	}

	public string GetLevelDecription()
	{
		return m_LevelDetails.m_LevelDescription;
	}

	public string SetLevelDecription(string strLevelDesc)
	{
		if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().FilterString(ref strLevelDesc);
		}
		m_LevelDetails.m_LevelDescription = strLevelDesc;
		return m_LevelDetails.m_LevelDescription;
	}

	public void SetAuthor(string strName)
	{
		if (!string.IsNullOrEmpty(strName))
		{
			if (string.IsNullOrEmpty(m_LevelDetails.m_AuthorName))
			{
				m_LevelDetails.m_AuthorName = strName;
			}
			else if (m_LevelDetails.m_AuthorName != strName)
			{
				m_LevelDetails.m_EditedName = strName;
			}
		}
	}

	public string GetAuthor()
	{
		return m_LevelDetails.m_AuthorName;
	}

	public string GetLastEditedBy()
	{
		return m_LevelDetails.m_EditedName;
	}

	public LevelType GetLevelType()
	{
		return m_LevelDetails.m_LevelType;
	}

	public string GetBlockSceneName()
	{
		string result = string.Empty;
		if (m_LevelDetails.m_BlockType == BuildingBlock_Type.Standard)
		{
			result = "StandardLevelBlocks";
		}
		return result;
	}

	public BuildingBlock_Type GetBlockType()
	{
		return m_LevelDetails.m_BlockType;
	}

	public int GetLevelVersion()
	{
		if (m_LevelDetails.m_LevelType == LevelType.UNKNOWN)
		{
			return 1;
		}
		return m_LevelDetails.m_LevelVersion;
	}

	public LevelEditorDataVersion GetDataLevelVersion()
	{
		return m_LevelDetails.m_DataVersionNumber;
	}

	public LevelScript.PRISON_ENUM GetMusicType()
	{
		return m_LevelDetails.m_MusicType;
	}

	public LevelScript.PRISON_ENUM GetOutfitType()
	{
		return m_LevelDetails.m_OutfitType;
	}

	public string GetPrisonString(LevelScript.PRISON_ENUM prison)
	{
		string empty = string.Empty;
		if (prison == LevelScript.PRISON_ENUM.OldWestFort)
		{
			return "Text.Prison.OldWestFort";
		}
		return "Text.Prison.Centre_Perks";
	}

	public DiffecultyLevel GetLevelDifficulty()
	{
		return m_LevelDetails.m_DiffecultyLevel;
	}

	public void SetLevelDifficulty(DiffecultyLevel diff)
	{
		m_LevelDetails.m_DiffecultyLevel = diff;
	}

	public int GetNumberOfInmates()
	{
		return m_LevelDetails.m_NumberOfInmates;
	}

	public void SetNumberOfInmates(int numberOfInmates)
	{
		m_LevelDetails.m_NumberOfInmates = (sbyte)numberOfInmates;
	}

	public int GetNumberOfGuards()
	{
		return m_LevelDetails.m_NumberOfGuards;
	}

	public void SetNumberOfGuards(int numberOfGuards)
	{
		m_LevelDetails.m_NumberOfGuards = (sbyte)numberOfGuards;
	}

	public void SetMusicType(LevelScript.PRISON_ENUM musicType)
	{
		m_LevelDetails.m_MusicType = musicType;
	}

	public void SetOutfitType(LevelScript.PRISON_ENUM outfitType)
	{
		m_LevelDetails.m_OutfitType = outfitType;
	}

	public string GetDateCreated()
	{
		return TurnNumberIntoDateString(m_LevelDetails.m_DateCreated);
	}

	public string GetDateLastEdited()
	{
		return TurnNumberIntoDateString(m_LevelDetails.m_DateLastEdited);
	}

	public ulong GetLastUploadedToID()
	{
		return m_LevelDetails.m_LastUploadedToID;
	}

	public void SetLastUploadedToID(ulong ulID)
	{
		m_LevelDetails.m_LastUploadedToID = ulID;
	}

	public Routines GetRoutineAtTime(int iHour)
	{
		if (iHour >= 0 && iHour <= 23)
		{
			return m_LevelDetails.m_Routines[iHour];
		}
		return Routines.UNASSIGNED;
	}

	public Routines[] GetRoutines()
	{
		return m_LevelDetails.m_Routines;
	}

	public void SetRoutineAtTime(int iHour, Routines routine)
	{
		if (iHour >= 0 && iHour <= 23 && m_LevelDetails.m_Routines[iHour] != routine)
		{
			m_LevelDetails.m_Routines[iHour] = routine;
			RoutineHasChanged(iHour);
		}
	}

	public bool DoWeHaveRoutineSet(Routines routine)
	{
		for (int num = m_LevelDetails.m_Routines.Length - 1; num >= 0; num--)
		{
			if (m_LevelDetails.m_Routines[num] == routine)
			{
				return true;
			}
		}
		return false;
	}

	public void RoutineHasChanged(int iHour)
	{
		if (this.OnRoutineChanged != null)
		{
			this.OnRoutineChanged(iHour);
		}
	}

	public void RegisterRoutineChange(RoutineChanged routineChanged)
	{
		if (routineChanged != null)
		{
			OnRoutineChanged += routineChanged;
		}
	}

	public int GetRandomItemGroup(int iGroupIndex)
	{
		if (iGroupIndex < 0 || iGroupIndex >= m_LevelDetails.m_RandomGroups.Length)
		{
			return -1;
		}
		return m_LevelDetails.m_RandomGroups[iGroupIndex];
	}

	public void SetRandomItemGroup(int iGroupIndex, int iItemIndex)
	{
		if (iGroupIndex >= 0 && iGroupIndex < m_LevelDetails.m_RandomGroups.Length)
		{
			m_LevelDetails.m_RandomGroups[iGroupIndex] = iItemIndex;
		}
	}

	private string TurnNumberIntoDateString(long number)
	{
		string empty = string.Empty;
		long num = number / 100000000;
		number -= num * 100000000;
		long num2 = number / 1000000;
		number -= num2 * 1000000;
		long num3 = number / 10000;
		number -= num3 * 10000;
		long num4 = number / 100;
		number -= num4 * 100;
		long num5 = number;
		empty = ((!m_bDayBeforeMonth) ? (num2.ToString("D2") + "-" + num3.ToString("D2")) : (num3.ToString("D2") + "-" + num2.ToString("D2")));
		string text = empty;
		return text + "-" + num.ToString("D4") + "  " + num4.ToString("D2") + ":" + num5.ToString("D2");
	}

	public void ResetLevel()
	{
		m_LevelManager.CleanLevel();
		m_LevelManager.enabled = true;
		if (m_BlockManager != null)
		{
			m_BlockManager.InitializeBlockInstructions(bOnlyIfNull: false);
		}
	}

	private bool LoadUserLevel(string strFileName)
	{
		if (m_LevelDetails.m_LevelType != 0)
		{
			return true;
		}
		if (string.IsNullOrEmpty(strFileName))
		{
			return false;
		}
		m_LevelDetails.Reset();
		m_InstructionManager.ResetContents();
		m_RawLevelData.Clear();
		m_EncryptedLevelData.Clear();
		byte[] array = File.ReadAllBytes(strFileName);
		if (array.Length == 0)
		{
			return false;
		}
		m_RawLevelData.AddRange(array);
		return LoadUserLevel(ref m_RawLevelData);
	}

	public bool LoadUserLevel(ref List<byte> saveData)
	{
		m_EncryptedLevelData.Clear();
		if (m_LevelDetails.m_LevelType != 0)
		{
			return true;
		}
		if (!saveData.Equals(m_RawLevelData))
		{
			m_RawLevelData.Clear();
			m_RawLevelData.AddRange(saveData);
		}
		m_LevelDetails.Reset();
		m_InstructionManager.ResetContents();
		m_EncryptedLevelData.AddRange(saveData);
		int num = 0;
		num = FindHeader(ref m_RawLevelData, 0, SerializationFlag.File_Header_V1);
		if (num == -1)
		{
			return false;
		}
		if (!m_LevelDetails.DeserializeOurData(ref m_RawLevelData, ref num))
		{
			return false;
		}
		num = FindHeader(ref m_RawLevelData, 0, SerializationFlag.LevelInstructions_V1);
		if (num == -1)
		{
			return false;
		}
		if (!m_InstructionManager.m_LevelInstructions.DeserializeOurData(ref m_RawLevelData, ref num, m_LevelDetails.m_LevelType))
		{
			return false;
		}
		return true;
	}

	public bool SaveUserLevel(string strFileName, LevelType thisLevelType, RequestResult resultCallback = null)
	{
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (!base.enabled)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (m_InstructionManager == null)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		m_RawLevelData.Clear();
		m_RawLevelData.Capacity = 200000;
		long num = long.Parse(DateTime.Now.ToString("yyyyMMddHHmm"));
		if (m_LevelDetails.m_DateCreated == 0)
		{
			m_LevelDetails.m_DateCreated = num;
		}
		m_LevelDetails.m_DateLastEdited = num;
		if (m_LevelDetails.m_LevelType == thisLevelType)
		{
			m_LevelDetails.m_LevelVersion++;
		}
		else
		{
			m_LevelDetails.m_LevelType = thisLevelType;
			m_LevelDetails.m_LevelVersion = 1;
		}
		m_LevelDetails.SerializeOurData(ref m_RawLevelData);
		m_InstructionManager.m_LevelInstructions.SerializeOurData(ref m_RawLevelData, m_LevelDetails.m_LevelType);
		if (m_RawLevelData.Count > 0)
		{
			if (!Directory.Exists(m_SavePath))
			{
				Directory.CreateDirectory(m_SavePath);
			}
			File.WriteAllBytes(strFileName, m_RawLevelData.ToArray());
			resultCallback?.Invoke(RequestResultEnum.Success);
			return true;
		}
		resultCallback?.Invoke(RequestResultEnum.Failed);
		return false;
	}

	public static int FindHeader(ref List<byte> data, int iStart, SerializationFlag header)
	{
		if (data == null || data.Count <= iStart - 6)
		{
			return -1;
		}
		int count = data.Count;
		while (iStart < count)
		{
			SerializationFlag serializationFlag = (SerializationFlag)data[iStart++];
			if (header == serializationFlag)
			{
				return iStart - 1;
			}
			if (header == SerializationFlag.ChunkEnd)
			{
				return -1;
			}
			if (iStart + 4 < count)
			{
				int @int = ByteArrayConversion.GetInt(ref data, ref iStart);
				iStart += @int;
				continue;
			}
			return -1;
		}
		return -1;
	}

	public bool ResetAndCreateNewLevel(RequestResult resultCallback = null, BuildingBlock_Type blockType = BuildingBlock_Type.Standard)
	{
		m_bGenerateVisualBlocks = false;
		return CreateNewLevel(resultCallback, blockType);
	}

	public bool CreateNewLevel(RequestResult resultCallback = null, BuildingBlock_Type blockType = BuildingBlock_Type.Standard)
	{
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (!base.enabled)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		m_RequestResult = resultCallback;
		m_LevelDetails.Reset();
		if (LevelEditor_ZoneManager.GetInstance() != null)
		{
			LevelEditor_ZoneManager.GetInstance().ResetZones();
		}
		m_LevelDetails.m_LevelName = GetNextDefaultPrisonTitle();
		Localization.Get("Text.Editor.DefaultPrisonDescription", out var localized);
		m_LevelDetails.m_LevelDescription = localized;
		m_LevelDetails.m_BlockType = blockType;
		m_LevelDetails.m_DataVersionNumber = LevelDetails.GetMasterDataVersion();
		SetDataVersion(m_LevelDetails.m_DataVersionNumber);
		SetState(LevelState.CreateLevel_Pending);
		return true;
	}

	public bool StartMakingLevel(RequestResult resultCallback = null)
	{
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		m_RequestResult = resultCallback;
		SetState(LevelState.MakeLevel_Init);
		return true;
	}

	public bool StartRemoveSelf(RequestResult resultCallback = null)
	{
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		m_RequestResult = resultCallback;
		SetState(LevelState.RemoveSelf);
		return true;
	}

	public void SetupFromSave(string strSaveFile, RequestResult resultCallback = null, bool bGenerateVisualBlocks = true)
	{
		m_bGenerateVisualBlocks = bGenerateVisualBlocks;
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return;
		}
		if (!base.enabled)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return;
		}
		m_RequestResult = resultCallback;
		m_FileName = strSaveFile;
		SetState(LevelState.LoadLevel_Pending);
	}

	public void RunSetupComponents(RequestResult resultCallback = null)
	{
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return;
		}
		if (!base.enabled)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return;
		}
		m_RequestResult = resultCallback;
		SetState(LevelState.SetUpLevel_Collect, ActionNow: true);
	}

	private bool LoadBlocksScene()
	{
		if (m_LoadedBuildingBlocks == m_LevelDetails.m_BlockType)
		{
			return true;
		}
		m_BuildingBlockSceneName = GetBlockSceneName();
		if (!BuildVersion.m_UseAssetBundles && SceneUtility.GetBuildIndexByScenePath(m_BuildingBlockSceneName) <= 0)
		{
			return false;
		}
		if (BuildVersion.m_UseAssetBundles)
		{
			AssetManager.instance.LoadScene(m_BuildingBlockSceneName, LoadSceneMode.Additive, alsoLoadBundleOverrides: true);
		}
		else
		{
			SceneManager.LoadScene(m_BuildingBlockSceneName, LoadSceneMode.Additive);
		}
		return true;
	}

	public static void TestLoad()
	{
		if (Application.isPlaying)
		{
			GetInstance().SetupFromSave("level.dat");
		}
	}

	public static void TestNew()
	{
		if (Application.isPlaying)
		{
			GetInstance().CreateNewLevel();
		}
	}

	private void SetState(LevelState newState, bool ActionNow = false)
	{
		m_State = newState;
		if (ActionNow)
		{
			ProcessStates();
		}
	}

	public LevelState GetState()
	{
		return m_State;
	}

	public bool IsDetailsManagerBusy()
	{
		return m_State != LevelState.Idle;
	}

	public void Update()
	{
		ProcessStates();
	}

	private void ProcessStates()
	{
		LevelState levelState = LevelState.UNKNOWN;
		bool actionNow = false;
		switch (m_State)
		{
		case LevelState.LoadLevel_Pending:
			if (!string.IsNullOrEmpty(m_FileName))
			{
				m_LevelDetails.Reset();
				levelState = LevelState.LoadLevel_LoadInstructions;
				actionNow = true;
			}
			else
			{
				levelState = LevelState.Failed;
			}
			break;
		case LevelState.LoadLevel_LoadInstructions:
			if (!LoadUserLevel(m_FileName))
			{
				levelState = LevelState.Failed;
				break;
			}
			levelState = LevelState.LoadLevel_LoadBuildingBlocksScene;
			actionNow = true;
			break;
		case LevelState.LoadLevel_LoadBuildingBlocksScene:
			levelState = (LoadBlocksScene() ? LevelState.LoadLevel_WaitingOnBlocksScene : LevelState.Failed);
			break;
		case LevelState.LoadLevel_WaitingOnBlocksScene:
			if (!SceneManager.GetSceneByName(m_BuildingBlockSceneName).IsValid())
			{
				break;
			}
			if (LevelEditor_Controller.GetInstance() != null)
			{
				LevelEditor_Controller.GetInstance().Activate();
			}
			m_BlockManager = BuildingBlockManager.GetInstance();
			if (m_BlockManager == null)
			{
				if (!BuildVersion.m_UseAssetBundles)
				{
					levelState = LevelState.Failed;
				}
			}
			else
			{
				levelState = LevelState.LoadLevel_GenerateBlocks;
			}
			break;
		case LevelState.LoadLevel_GenerateBlocks:
			if (!m_bGenerateVisualBlocks || m_BlockManager.GenerateVisualBlockData())
			{
				m_bGenerateVisualBlocks = true;
				m_LoadedBuildingBlocks = m_LevelDetails.m_BlockType;
				ResetLevel();
				levelState = LevelState.LoadLevel_Build;
				actionNow = true;
			}
			break;
		case LevelState.LoadLevel_Build:
			if (!m_InstructionManager.TimeSlicedRunAllInstructions(300L))
			{
				break;
			}
			m_InstructionManager.UpdateLevel();
			if (m_LevelDetails.m_DataVersionNumber < LevelDetails.GetMasterDataVersion())
			{
				levelState = LevelState.LoadLevel_UpdateRegenerate;
				m_InstructionManager.ConvertTheLevel();
				m_LevelDetails.m_DataVersionNumber = LevelDetails.GetMasterDataVersion();
				SetDataVersion(LevelDetails.GetMasterDataVersion());
				ResetLevel();
				actionNow = true;
			}
			else
			{
				levelState = LevelState.LoadLevel_ValidateZone;
				actionNow = true;
				if (m_Mode == LevelMode.Build && m_LevelMasterGameObject != null)
				{
					m_LevelMasterGameObject.SetActive(value: true);
				}
			}
			break;
		case LevelState.LoadLevel_UpdateRegenerate:
			m_BlockManager.CleanRoomReps();
			levelState = LevelState.LoadLevel_UpdateLevel;
			actionNow = true;
			break;
		case LevelState.LoadLevel_UpdateLevel:
			if (m_InstructionManager.TimeSlicedFinishedInstructions(300L))
			{
				BuildingInstructionManager.GetInstance().AddToCurrentList(0, BaseBuildInstruction.InstructionTypeEnum.PreventUndo);
				m_InstructionManager.UpdateLevel();
				levelState = LevelState.LoadLevel_ValidateZone;
				actionNow = true;
				if (m_Mode == LevelMode.Build && m_LevelMasterGameObject != null)
				{
					m_LevelMasterGameObject.SetActive(value: true);
				}
			}
			break;
		case LevelState.LoadLevel_ValidateZone:
			if (LevelEditor_ZoneManager.GetInstance() != null)
			{
				LevelEditor_ZoneManager.GetInstance().ResetLayerChecks();
			}
			levelState = LevelState.Success;
			break;
		case LevelState.CreateLevel_Pending:
			m_InstructionManager.ResetContents();
			levelState = LevelState.CreateLevel_LoadBuildingBlockScene;
			actionNow = true;
			break;
		case LevelState.CreateLevel_LoadBuildingBlockScene:
			levelState = (LoadBlocksScene() ? LevelState.CreateLevel_WaitingOnBlocksScene : LevelState.Failed);
			break;
		case LevelState.CreateLevel_WaitingOnBlocksScene:
			if (!SceneManager.GetSceneByName(m_BuildingBlockSceneName).IsValid())
			{
				break;
			}
			if (LevelEditor_Controller.GetInstance() != null)
			{
				LevelEditor_Controller.GetInstance().Activate();
			}
			m_BlockManager = BuildingBlockManager.GetInstance();
			if (m_BlockManager == null)
			{
				if (!BuildVersion.m_UseAssetBundles)
				{
					levelState = LevelState.Failed;
				}
			}
			else
			{
				levelState = LevelState.CreateLevel_GenerateBlocks;
			}
			break;
		case LevelState.CreateLevel_GenerateBlocks:
			if (m_bGenerateVisualBlocks && !m_BlockManager.GenerateVisualBlockData())
			{
				break;
			}
			m_LoadedBuildingBlocks = m_LevelDetails.m_BlockType;
			ResetLevel();
			m_LevelManager.SetInitialLevel();
			if (m_LevelDetails != null && m_BlockManager != null && m_BlockManager.m_DefaultRoutines.Length == m_LevelDetails.m_Routines.Length)
			{
				for (int num = m_LevelDetails.m_Routines.Length - 1; num >= 0; num--)
				{
					m_LevelDetails.m_Routines[num] = m_BlockManager.m_DefaultRoutines[num];
				}
				if (GetInstance() != null)
				{
					GetInstance().RoutineHasChanged(0);
				}
			}
			levelState = LevelState.Success;
			break;
		case LevelState.SetUpLevel_Collect:
			CollectSetupComponents();
			break;
		case LevelState.SetUpLevel_Process:
			ProcessSetupComponents();
			break;
		case LevelState.MakeLevel_Init:
			m_BlockManager = BuildingBlockManager.GetInstance();
			ResetLevel();
			levelState = LevelState.MakeLevel_Pending;
			actionNow = true;
			break;
		case LevelState.MakeLevel_Pending:
			if (m_InstructionManager.TimeSlicedRunAllInstructions(300L))
			{
				m_InstructionManager.UpdateLevel();
				levelState = LevelState.SetUpLevel_Collect;
				actionNow = true;
			}
			break;
		case LevelState.RemoveSelf:
			if (m_LevelMasterGameObject != null)
			{
				m_LevelMasterGameObject.SetActive(value: true);
				base.gameObject.SetActive(value: false);
				levelState = LevelState.Idle;
			}
			break;
		case LevelState.Success:
			SetState(LevelState.Idle);
			if (m_RequestResult != null)
			{
				RequestResult requestResult2 = m_RequestResult;
				m_RequestResult = null;
				requestResult2(RequestResultEnum.Success);
			}
			break;
		case LevelState.Failed:
			SetState(LevelState.Idle);
			if (m_RequestResult != null)
			{
				RequestResult requestResult = m_RequestResult;
				m_RequestResult = null;
				requestResult(RequestResultEnum.Failed);
			}
			break;
		}
		if (levelState != LevelState.UNKNOWN)
		{
			SetState(levelState, actionNow);
		}
	}

	public bool AreWeGeneratingVisualBlocks()
	{
		if (m_State == LevelState.LoadLevel_GenerateBlocks || m_State == LevelState.CreateLevel_GenerateBlocks)
		{
			return true;
		}
		return false;
	}

	public void CollectSetupComponents()
	{
		m_SetupCompontents.Clear();
		m_SetupChecked = 0;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (!sceneAt.isLoaded || !sceneAt.IsValid())
			{
				continue;
			}
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			for (int num = rootGameObjects.Length - 1; num >= 0; num--)
			{
				if (rootGameObjects[num].GetComponent<BuildingBlockManager>() == null)
				{
					m_SetupCompontents.AddRange(rootGameObjects[num].GetComponentsInChildren<BaseComponentSetup>(includeInactive: true));
				}
			}
		}
		if (m_SetupCompontents.Count == 0)
		{
			SetState(LevelState.Success, ActionNow: true);
			return;
		}
		m_SetupCompontents.Sort((BaseComponentSetup a, BaseComponentSetup b) => a.GetPriority().CompareTo(b.GetPriority()));
		SetState(LevelState.SetUpLevel_Process, ActionNow: true);
	}

	public void ProcessSetupComponents()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int count = m_SetupCompontents.Count;
		while (stopwatch.ElapsedMilliseconds < 300)
		{
			BaseComponentSetup baseComponentSetup = m_SetupCompontents[m_SetupChecked];
			if (baseComponentSetup != null)
			{
				BaseComponentSetup.SetupReturnState setupReturnState = BaseComponentSetup.SetupReturnState.Finished;
				switch ((c_CurrentLevelDataVersionNumber != LevelEditorDataVersion.V1_InitialRelease) ? baseComponentSetup.SetupV2() : baseComponentSetup.Setup())
				{
				case BaseComponentSetup.SetupReturnState.Finished:
					m_SetupCompontents[m_SetupChecked] = null;
					m_SetupChecked++;
					break;
				case BaseComponentSetup.SetupReturnState.TakeABreak:
					return;
				}
			}
			else
			{
				m_SetupChecked++;
			}
			if (m_bSetupListDirty)
			{
				m_SetupCompontents.Sort(delegate(BaseComponentSetup a, BaseComponentSetup b)
				{
					if (object.ReferenceEquals(a, null))
					{
						return (!object.ReferenceEquals(b, null)) ? (-1) : 0;
					}
					return object.ReferenceEquals(b, null) ? 1 : a.GetPriority().CompareTo(b.GetPriority());
				});
				m_SetupChecked = 0;
				m_bSetupListDirty = false;
				count = m_SetupCompontents.Count;
			}
			if (m_SetupChecked >= count)
			{
				m_SetupCompontents.Clear();
				SetState(LevelState.Success, ActionNow: true);
				break;
			}
		}
	}

	public void RegisterSetupComponent(BaseComponentSetup setupComponent)
	{
		if (setupComponent != null)
		{
			m_SetupCompontents.Add(setupComponent);
			m_bSetupListDirty = true;
		}
	}

	public bool GetLevelDataValidationErrors(ref List<ErrorData> errorList)
	{
		if (m_BlockManager == null)
		{
			errorList.Add(new ErrorData(ErrorData.Severity.Error, "Could not find BuildingBlockManager"));
			return true;
		}
		int count = errorList.Count;
		int num = m_BlockManager.m_LimitationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_BlockManager.m_LimitationGroups[i].m_bValid)
			{
				if ((!RoutineHelper.IsValid(m_BlockManager.m_LimitationGroups[i].m_Routine) || DoWeHaveRoutineSet(m_BlockManager.m_LimitationGroups[i].m_Routine)) && !m_BlockManager.m_LimitationGroups[i].HasMetRequirements())
				{
					string localizedStringWithLocalizedSwap = GetLocalizedStringWithLocalizedSwap(m_BlockManager.m_LimitationGroups[i].m_ErrorResourceID, "%Object%", m_BlockManager.m_LimitationGroups[i].m_TextResourceName);
					ErrorData.AddToErrorList(new ErrorData(ErrorData.Severity.Error, localizedStringWithLocalizedSwap, m_BlockManager.m_LimitationGroups[i].m_ErrorID), ref errorList);
				}
				if (!m_BlockManager.m_LimitationGroups[i].IsWithinLimits())
				{
					string localizedStringWithLocalizedSwap2 = GetLocalizedStringWithLocalizedSwap(m_BlockManager.m_LimitationGroups[i].m_TooManyErrorResourceID, "%Object%", m_BlockManager.m_LimitationGroups[i].m_TextResourceName);
					ErrorData.AddToErrorList(new ErrorData(ErrorData.Severity.Error, localizedStringWithLocalizedSwap2, m_BlockManager.m_LimitationGroups[i].m_ErrorID), ref errorList);
				}
			}
		}
		int[] array = new int[9];
		for (int num2 = m_LevelDetails.m_Routines.Length - 1; num2 >= 0; num2--)
		{
			array[(int)m_LevelDetails.m_Routines[num2]]++;
		}
		return count != errorList.Count;
	}

	private string GetLocalizedString(string strID, string strDefault)
	{
		if (string.IsNullOrEmpty(strID))
		{
			return strDefault;
		}
		string localized = string.Empty;
		if (!Localization.Get(strID, out localized))
		{
			localized = "MISSING [" + strID + "]";
		}
		return localized;
	}

	private string GetLocalizedStringWithLocalizedSwap(string strID, string strKey, string strSwapID)
	{
		if (string.IsNullOrEmpty(strID))
		{
			return "NO STRING RESOURCE ID";
		}
		string localizedString = GetLocalizedString(strSwapID, "MISSING SWAP:" + strSwapID);
		string localised = string.Empty;
		if (!Localization.GetWithKeySwap(strID, out localised, strKey, localizedString))
		{
			localised = "MISSING [" + strID + "]";
		}
		return localised;
	}

	[ContextMenu("Route")]
	public void TestRout()
	{
		List<ErrorData> errorList = new List<ErrorData>();
		ValidateEverythingIsReachable(ref errorList);
	}

	public bool ValidateEverythingIsReachable(ref List<ErrorData> errorList)
	{
		if (c_CurrentLevelDataVersionNumber == LevelEditorDataVersion.V2_AddedZoneEditing)
		{
			return ValidateEverythingIsReachableV2(ref errorList);
		}
		List<MarkersFoundTyped> list = new List<MarkersFoundTyped>();
		int count = errorList.Count;
		m_SearchLayers.Clear();
		int namedLimitationIndex = BuildingBlockManager.GetInstance().GetNamedLimitationIndex(BuildingBlockManager.DefaultLimitationGroups.RollCall.ToString());
		if (namedLimitationIndex == -1)
		{
			return false;
		}
		int num = 0;
		int num2 = 14400;
		for (int i = 1; i < 6; i++)
		{
			BaseLevelManager.LayerDataCollection data = BaseLevelManager.GetInstance().m_BuildingLayers[i];
			for (int j = 0; j < num2; j++)
			{
				BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[j];
				BaseLevelManager.TileProperty[] tileProperties;
				int num3;
				(tileProperties = data.m_TileProperties)[num3 = j] = tileProperties[num3] & ~(BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.CanReachRollCallMask);
				if ((tileProperty & BaseLevelManager.TileProperty.ObjDecMask) == 0)
				{
					continue;
				}
				BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j]) as BuildingBlock_Object;
				if (!(buildingBlock_Object != null) || (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.AnyMarker) == 0)
				{
					continue;
				}
				bool flag = false;
				num = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
				MarkersFound markersFound = default(MarkersFound);
				markersFound.m_Layer = (sbyte)i;
				markersFound.m_Y = (sbyte)(j / 120);
				markersFound.m_X = (sbyte)(j % 120);
				if (num == 0)
				{
					flag = true;
				}
				else
				{
					int blockIDFromComplexAllocation = m_LevelManager.GetBlockIDFromComplexAllocation(num);
					if (blockIDFromComplexAllocation != -1)
					{
						BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
						if (block != null && block.m_LimitationGroup != -1)
						{
							if (block.m_LimitationGroup == namedLimitationIndex)
							{
								m_SearchLayers.Add(markersFound);
							}
							else
							{
								flag = true;
							}
						}
					}
				}
				if (flag)
				{
					MarkersFoundTyped item = default(MarkersFoundTyped);
					item.m_Marker = markersFound;
					if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Marker) == BuildingBlock_Object.SpecialFlagsEnum.Marker)
					{
						item.m_Type = RoomMarker.MarkerType.Normal;
					}
					else
					{
						item.m_Type = RoomMarker.MarkerType.Escape;
					}
					list.Add(item);
				}
			}
		}
		for (int k = 0; m_SearchLayers.Count > k; k++)
		{
			if (m_SearchLayers[k].m_Layer != -1)
			{
				int num4 = m_SearchLayers[k].m_X;
				int num5 = m_SearchLayers[k].m_Y;
				m_ScanLocations[0] = (num5 << 8) + num4;
				m_ScanCurrentIndex = 0;
				m_ScanFirstFree = 1;
				m_ScanTotalUsed = 1;
				BaseLevelManager.LayerDataCollection layerData = BaseLevelManager.GetInstance().m_BuildingLayers[m_SearchLayers[k].m_Layer];
				int ilayer = m_SearchLayers[k].m_Layer;
				while (m_ScanTotalUsed > 0)
				{
					ScanForFreeSpace(ilayer, layerData);
				}
			}
		}
		for (int num6 = list.Count - 1; num6 >= 0; num6--)
		{
			MarkersFound marker = list[num6].m_Marker;
			int num7 = marker.m_X + marker.m_Y * 120;
			BaseLevelManager.TileProperty tileProperty2 = BaseLevelManager.GetInstance().m_BuildingLayers[marker.m_Layer].m_TileProperties[num7];
			if ((tileProperty2 & BaseLevelManager.TileProperty.CanReachRollCallMask) != BaseLevelManager.TileProperty.CanReachRollCallMask && list[num6].m_Type == RoomMarker.MarkerType.Normal)
			{
				GameObject blockObject = null;
				int iRoom = 0;
				int blockWeAreOn = LevelEditor_Controller.GetInstance().GetBlockWeAreOn(marker.m_X, marker.m_Y, ref blockObject, ref iRoom);
				if (blockWeAreOn != -1)
				{
					BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(blockWeAreOn);
					if (block2 != null && block2.m_LimitationGroup != -1)
					{
						BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(block2.m_LimitationGroup);
						if (limitationGroup != null)
						{
							if (string.IsNullOrEmpty(limitationGroup.m_BlockedErrorResourceID))
							{
								errorList.Add(new ErrorData(ErrorData.Severity.Error, "The " + BuildingBlockManager.GetBlockName(blockWeAreOn) + " can't be reached by everything", marker.m_Layer, marker.m_X, marker.m_Y));
							}
							else
							{
								string localizedStringWithLocalizedSwap = GetLocalizedStringWithLocalizedSwap(limitationGroup.m_BlockedErrorResourceID, "%RoomName%", limitationGroup.m_TextResourceName);
								if (string.IsNullOrEmpty(localizedStringWithLocalizedSwap))
								{
									errorList.Add(new ErrorData(ErrorData.Severity.Error, "[" + limitationGroup.m_BlockedErrorResourceID + "]", marker.m_Layer, marker.m_X, marker.m_Y));
								}
								else
								{
									errorList.Add(new ErrorData(ErrorData.Severity.Error, localizedStringWithLocalizedSwap, marker.m_Layer, marker.m_X, marker.m_Y));
								}
							}
						}
					}
				}
			}
		}
		return count != errorList.Count;
	}

	public bool ValidateEverythingIsReachableV2(ref List<ErrorData> errorList)
	{
		int count = errorList.Count;
		UpdateReachableFlags();
		LevelEditor_ZoneManager levelEditor_ZoneManager = null;
		if (!(levelEditor_ZoneManager == null) || (levelEditor_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
		}
		int iStartingIndex = 0;
		LevelEditor_ZoneManager.Zone zoneOfType = levelEditor_ZoneManager.GetZoneOfType(ref iStartingIndex, ZoneDetailsManager.ZoneTypes.RollCall);
		if (zoneOfType == null)
		{
			return false;
		}
		if (zoneOfType.m_TotalBlocked > 0)
		{
			string strError = zoneOfType.m_ZoneDetails.GetCantReachErrorText().Replace("%Object%", zoneOfType.m_ZoneDetails.GetZoneNameText());
			errorList.Add(new ErrorData(ErrorData.Severity.Error, strError, zoneOfType.m_ZoneDetails.m_CantReachErrorID));
			return false;
		}
		int totalZones = levelEditor_ZoneManager.GetTotalZones();
		for (int i = 0; i < totalZones; i++)
		{
			LevelEditor_ZoneManager.Zone zone = levelEditor_ZoneManager.GetZone(i, bSupressWarning: true);
			if (zone != null && zone.m_bActive)
			{
				if (!zone.m_bValid)
				{
					string strError2 = zone.m_ZoneDetails.GetStandardErrorText().Replace("%Object%", zone.m_ZoneDetails.GetZoneNameText());
					ErrorData errorData = new ErrorData(ErrorData.Severity.Error, strError2, zone.m_ZoneDetails.m_ErrorID);
					ErrorData.AddToErrorList(errorData, ref errorList);
				}
				if (zone.m_TotalBlocked > 0)
				{
					string strError3 = zone.m_ZoneDetails.GetCantReachErrorText().Replace("%Object%", zone.m_ZoneDetails.GetZoneNameText());
					ErrorData errorData2 = new ErrorData(ErrorData.Severity.Error, strError3, zone.m_ZoneDetails.m_CantReachErrorID);
					ErrorData.AddToErrorList(errorData2, ref errorList);
					zone.m_bReachable = false;
				}
				else
				{
					zone.m_bReachable = true;
				}
			}
		}
		return count != errorList.Count;
	}

	private void ScanForFreeSpace(int ilayer, BaseLevelManager.LayerDataCollection layerData)
	{
		int num = m_ScanLocations[m_ScanCurrentIndex] >> 8;
		int num2 = m_ScanLocations[m_ScanCurrentIndex] & 0xFF;
		int num3 = num * 120 + num2;
		m_ScanCurrentIndex++;
		if (m_ScanCurrentIndex >= 10000)
		{
			m_ScanCurrentIndex = 0;
		}
		m_ScanTotalUsed--;
		int num4;
		BaseLevelManager.TileProperty[] tileProperties;
		(tileProperties = layerData.m_TileProperties)[num4 = num3] = tileProperties[num4] | BaseLevelManager.TileProperty.CanReachRollCallMask;
		if ((layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.EntranceMask) == BaseLevelManager.TileProperty.EntranceMask)
		{
			int iFoundInLayer = -1;
			int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref layerData, num3);
			int num5 = LevelSetup_Transistion.FindExit(ilayer, roomNumberFromProperty, ref iFoundInLayer);
			if (num5 != -1)
			{
				MarkersFound item = default(MarkersFound);
				item.m_Layer = (sbyte)iFoundInLayer;
				item.m_Y = (sbyte)(num5 / 120);
				item.m_X = (sbyte)(num5 % 120);
				m_SearchLayers.Add(item);
			}
		}
		if (num2 > 0 && (layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
		{
			BaseLevelManager.TileProperty tileProperty = layerData.m_TileProperties[num3 - 1];
			if ((tileProperty & (BaseLevelManager.TileProperty.AlreadyCheckedOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
			{
				AddtoScanList(num2 - 1, num);
				int num6;
				(tileProperties = layerData.m_TileProperties)[num6 = num3 - 1] = tileProperties[num6] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num2 + 1 < 120 && (layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
		{
			BaseLevelManager.TileProperty tileProperty2 = layerData.m_TileProperties[num3 + 1];
			if ((tileProperty2 & (BaseLevelManager.TileProperty.AlreadyCheckedOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
			{
				AddtoScanList(num2 + 1, num);
				int num7;
				(tileProperties = layerData.m_TileProperties)[num7 = num3 + 1] = tileProperties[num7] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num > 0 && (layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) != BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
		{
			BaseLevelManager.TileProperty tileProperty3 = layerData.m_TileProperties[num3 - 120];
			if ((tileProperty3 & (BaseLevelManager.TileProperty.AlreadyCheckedOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
			{
				AddtoScanList(num2, num - 1);
				int num8;
				(tileProperties = layerData.m_TileProperties)[num8 = num3 - 120] = tileProperties[num8] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num + 1 < 120 && (layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) != BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
		{
			BaseLevelManager.TileProperty tileProperty4 = layerData.m_TileProperties[num3 + 120];
			if ((tileProperty4 & (BaseLevelManager.TileProperty.AlreadyCheckedOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
			{
				AddtoScanList(num2, num + 1);
				int num9;
				(tileProperties = layerData.m_TileProperties)[num9 = num3 + 120] = tileProperties[num9] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
	}

	[ContextMenu("Save")]
	public void SaveIt()
	{
		StoreTheLevel();
	}

	public bool StoreTheLevel(RequestResult resultCallback = null, bool bForceNewSave = false)
	{
		SaveManager instance = SaveManager.GetInstance();
		if (instance == null)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (m_State != 0)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (!base.enabled)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (m_InstructionManager == null)
		{
			resultCallback?.Invoke(RequestResultEnum.Failed);
			return false;
		}
		if (string.IsNullOrEmpty(m_LevelDetails.m_LevelName))
		{
			m_LevelDetails.m_LevelName = GetNextDefaultPrisonTitle();
		}
		if (string.IsNullOrEmpty(m_LevelDetails.m_LevelDescription))
		{
			Localization.Get("Text.Editor.DefaultPrisonDescription", out var localized);
			m_LevelDetails.m_LevelDescription = localized;
		}
		if (c_CurrentLevelDataVersionNumber == LevelEditorDataVersion.V1_InitialRelease)
		{
			SetNumberOfGuards(m_BlockManager.GetLimitationTotal(BuildingBlockManager.DefaultLimitationGroups.Guard.ToString()));
		}
		else
		{
			SetNumberOfGuards(m_BlockManager.GetLimitationTotal(BuildingBlockManager.DefaultLimitationGroups.Guard.ToString()) + 2);
		}
		int numberOfInmates = Math.Max(m_BlockManager.GetLimitationTotal(BuildingBlockManager.DefaultLimitationGroups.InmateCell.ToString()) - 4, 0);
		SetNumberOfInmates(numberOfInmates);
		if (string.IsNullOrEmpty(m_LevelDetails.m_DirectoryName) || bForceNewSave)
		{
			m_LevelDetails.m_DirectoryName = Guid.NewGuid().ToString();
		}
		List<ErrorData> errorList = new List<ErrorData>();
		GetInstance().GetLevelDataValidationErrors(ref errorList);
		GetInstance().ValidateEverythingIsReachable(ref errorList);
		GetInstance().ValidateWalkableAreas(ref errorList);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < errorList.Count; i++)
		{
			switch (errorList[i].m_Severity)
			{
			case ErrorData.Severity.Warning:
				num++;
				break;
			case ErrorData.Severity.Error:
				num2++;
				break;
			}
		}
		m_RawLevelData = new List<byte>();
		m_RawLevelData.Capacity = 200000;
		long num3 = long.Parse(DateTime.Now.ToString("yyyyMMddHHmm"));
		if (m_LevelDetails.m_DateCreated == 0)
		{
			m_LevelDetails.m_DateCreated = num3;
		}
		m_LevelDetails.m_DateLastEdited = num3;
		m_LevelDetails.m_LevelVersion++;
		m_LevelDetails.m_LevelType = LevelType.WorkInProgress;
		m_LevelDetails.SerializeOurData(ref m_RawLevelData);
		m_InstructionManager.m_LevelInstructions.SerializeOurData(ref m_RawLevelData, LevelType.WorkInProgress);
		bool flag = SaveManager.GetInstance().SaveUserLevel(m_LevelDetails.m_DirectoryName, ref m_RawLevelData);
		if (num2 == 0)
		{
			m_LevelDetails.m_LevelType = LevelType.Finished;
			m_RawLevelData.Clear();
			m_LevelDetails.SerializeOurData(ref m_RawLevelData);
			m_InstructionManager.m_LevelInstructions.SerializeOurData(ref m_RawLevelData, LevelType.Finished);
			flag = SaveManager.GetInstance().SaveUserLevel(m_LevelDetails.m_DirectoryName, ref m_RawLevelData, bIsFinishedVersion: true);
		}
		if (flag)
		{
			resultCallback?.Invoke(RequestResultEnum.Success);
			return true;
		}
		resultCallback?.Invoke(RequestResultEnum.Failed);
		return false;
	}

	public bool GetRawLevelData(ref List<byte> levelData)
	{
		levelData.Clear();
		levelData.AddRange(m_RawLevelData);
		return m_RawLevelData.Count != 0;
	}

	public bool GetEncryptedLevelData(ref List<byte> levelData)
	{
		levelData.Clear();
		levelData.AddRange(m_EncryptedLevelData);
		return m_EncryptedLevelData.Count != 0;
	}

	public static FrontendLevelDetails GetFrontendDataForLevel(ref List<byte> LevelData)
	{
		FrontendLevelDetails frontendLevelDetails = new FrontendLevelDetails();
		frontendLevelDetails.DeserializeFrontendData(ref LevelData);
		return frontendLevelDetails;
	}

	public static string GetNextDefaultPrisonTitle()
	{
		int customPrisonCount = SaveManager.GetInstance().GetCustomPrisonCount();
		Localization.GetWithKeySwap("Text.Editor.DefaultPrisonName", out var localised, "$PrisonCount", customPrisonCount);
		return localised;
	}

	[ContextMenu("Test Walkable")]
	public void testWalkable()
	{
		List<ErrorData> errorList = new List<ErrorData>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		ValidateWalkableAreas(ref errorList);
		stopwatch.Stop();
		for (int i = 0; i < errorList.Count; i++)
		{
		}
	}

	public bool ValidateWalkableAreas(ref List<ErrorData> errorList, bool bTestForEscapes = true)
	{
		if (c_CurrentLevelDataVersionNumber == LevelEditorDataVersion.V2_AddedZoneEditing)
		{
			return ValidateWalkableAreasV2(ref errorList, bTestForEscapes);
		}
		if (m_BlockManager == null)
		{
			m_BlockManager = BuildingBlockManager.GetInstance();
			if (m_BlockManager == null)
			{
				return true;
			}
		}
		List<EscapeMarkersFound> list = new List<EscapeMarkersFound>();
		int count = errorList.Count;
		m_SearchLayers.Clear();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int namedLimitationIndex = BuildingBlockManager.GetInstance().GetNamedLimitationIndex("Escape");
		if (namedLimitationIndex == -1)
		{
			return false;
		}
		int namedLimitationIndex2 = BuildingBlockManager.GetInstance().GetNamedLimitationIndex(BuildingBlockManager.DefaultLimitationGroups.RollCall.ToString());
		int num = 0;
		int num2 = 14400;
		for (int i = 1; i < 6; i++)
		{
			BaseLevelManager.LayerDataCollection data = BaseLevelManager.GetInstance().m_BuildingLayers[i];
			for (int j = 0; j < num2; j++)
			{
				BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[j];
				BaseLevelManager.TileProperty[] tileProperties;
				int num3;
				(tileProperties = data.m_TileProperties)[num3 = j] = tileProperties[num3] & ~(BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.ScanBlockedMask | BaseLevelManager.TileProperty.SafeMask);
				num = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
				if (num == 0 || (tileProperty & BaseLevelManager.TileProperty.ObjDecMask) == 0)
				{
					continue;
				}
				BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j]) as BuildingBlock_Object;
				if (!(buildingBlock_Object != null) || (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.AnyMarker) == 0)
				{
					continue;
				}
				int blockIDFromComplexAllocation = m_LevelManager.GetBlockIDFromComplexAllocation(num);
				if (blockIDFromComplexAllocation == -1)
				{
					continue;
				}
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
				if (block != null && block.m_LimitationGroup != -1 && (block.m_LimitationGroup == namedLimitationIndex2 || block.m_LimitationGroup == namedLimitationIndex))
				{
					MarkersFound markersFound = default(MarkersFound);
					markersFound.m_Layer = (sbyte)i;
					markersFound.m_Y = (sbyte)(j / 120);
					markersFound.m_X = (sbyte)(j % 120);
					if (block.m_LimitationGroup == namedLimitationIndex2)
					{
						m_SearchLayers.Add(markersFound);
						continue;
					}
					EscapeMarkersFound item = default(EscapeMarkersFound);
					item.m_Marker = markersFound;
					item.m_Block = blockIDFromComplexAllocation;
					list.Add(item);
				}
			}
		}
		stopwatch.Stop();
		stopwatch.Reset();
		stopwatch.Start();
		for (int k = 0; m_SearchLayers.Count > k; k++)
		{
			if (m_SearchLayers[k].m_Layer != -1)
			{
				int num4 = m_SearchLayers[k].m_X;
				int num5 = m_SearchLayers[k].m_Y;
				m_ScanLocations[0] = (num5 << 8) + num4;
				m_ScanCurrentIndex = 0;
				m_ScanFirstFree = 1;
				m_ScanTotalUsed = 1;
				BaseLevelManager.LayerDataCollection layerData = BaseLevelManager.GetInstance().m_BuildingLayers[m_SearchLayers[k].m_Layer];
				int ilayer = m_SearchLayers[k].m_Layer;
				while (m_ScanTotalUsed > 0)
				{
					ScanForWalkableSpace(ilayer, layerData);
				}
			}
		}
		stopwatch.Stop();
		if (!bTestForEscapes)
		{
			return false;
		}
		stopwatch.Reset();
		stopwatch.Start();
		for (int num6 = list.Count - 1; num6 >= 0; num6--)
		{
			int num7 = list[num6].m_Marker.m_X + list[num6].m_Marker.m_Y * 120;
			BaseLevelManager.TileProperty tileProperty2 = BaseLevelManager.GetInstance().m_BuildingLayers[list[num6].m_Marker.m_Layer].m_TileProperties[num7];
			if ((tileProperty2 & BaseLevelManager.TileProperty.SafeMask) == BaseLevelManager.TileProperty.SafeMask)
			{
				BaseBuildingBlock buildingBlock = m_BlockManager.GetBuildingBlock(list[num6].m_Block);
				if (buildingBlock != null)
				{
					string localizedStringWithLocalizedSwap = GetLocalizedStringWithLocalizedSwap("Text.Editor.EscapePossible.Room", "%escapetype%", buildingBlock.m_BlockNameID);
					if (string.IsNullOrEmpty(localizedStringWithLocalizedSwap))
					{
						errorList.Add(new ErrorData(ErrorData.Severity.Warning, "[Text.Editor.EscapePossible.Room][" + buildingBlock.m_BlockNameID + "]", list[num6].m_Marker.m_Layer, list[num6].m_Marker.m_X, list[num6].m_Marker.m_Y));
					}
					else
					{
						errorList.Add(new ErrorData(ErrorData.Severity.Warning, localizedStringWithLocalizedSwap, list[num6].m_Marker.m_Layer, list[num6].m_Marker.m_X, list[num6].m_Marker.m_Y));
					}
				}
			}
		}
		int[] array = new int[3] { 1, 3, 5 };
		int num8 = 118;
		for (int l = 0; l < array.Length; l++)
		{
			BaseLevelManager.LayerDataCollection layer = BaseLevelManager.GetInstance().m_BuildingLayers[array[l]];
			if (SafeFound(0, 1, 120, layer) || SafeFound(120, 120, num8 - 2, layer) || SafeFound(239, 120, num8 - 2, layer) || SafeFound(120 * (num8 - 1), 1, 120, layer))
			{
				string localizedString = GetLocalizedString("Text.Editor.EscapePossible.Border", "Text.Editor.EscapePossible.Border");
				errorList.Add(new ErrorData(ErrorData.ErrorType.BasicError, ErrorData.Severity.Warning, localizedString, array[l], 60, 60));
				break;
			}
			num8++;
		}
		stopwatch.Stop();
		return count != errorList.Count;
	}

	public bool ValidateWalkableAreasV2(ref List<ErrorData> errorList, bool bTestForEscapes = true)
	{
		m_SearchLayers.Clear();
		if (m_BlockManager == null && (m_BlockManager = BuildingBlockManager.GetInstance()) == null)
		{
			return true;
		}
		LevelEditor_ZoneManager levelEditor_ZoneManager = null;
		if (!(levelEditor_ZoneManager == null) || (levelEditor_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
		}
		List<EscapeMarkersFoundV2> list = new List<EscapeMarkersFoundV2>();
		int count = errorList.Count;
		int iStartingIndex = 0;
		LevelEditor_ZoneManager.Zone zoneOfType = levelEditor_ZoneManager.GetZoneOfType(ref iStartingIndex, ZoneDetailsManager.ZoneTypes.RollCall);
		if (zoneOfType == null)
		{
			return false;
		}
		int count2 = zoneOfType.m_BlocksInZone.Count;
		for (int i = 0; i < count2; i++)
		{
			if (zoneOfType.m_BlocksInZone[i] != null)
			{
				BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(zoneOfType.m_BlocksInZone[i].m_BlockID) as BuildingBlock_Object;
				if (buildingBlock_Object != null && (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.RollCallMarker) == BuildingBlock_Object.SpecialFlagsEnum.RollCallMarker)
				{
					MarkersFound item = default(MarkersFound);
					item.m_Layer = (sbyte)zoneOfType.m_Layer;
					item.m_Y = (sbyte)(zoneOfType.m_BlocksInZone[i].m_Y + zoneOfType.m_Bottom);
					item.m_X = (sbyte)(zoneOfType.m_BlocksInZone[i].m_X + zoneOfType.m_Left);
					m_SearchLayers.Add(item);
					break;
				}
			}
		}
		if (m_SearchLayers.Count == 0 || zoneOfType.m_TotalBlocked > 0)
		{
			ZoneDetailsManager instance = ZoneDetailsManager.GetInstance();
			if (instance != null)
			{
				ZoneDetailsManager.ZoneDetails zoneDetails = instance.GetZoneDetails(ZoneDetailsManager.ZoneTypes.RollCall);
				if (zoneDetails != null)
				{
					string strError = zoneDetails.GetCantReachErrorText().Replace("%Object%", zoneDetails.GetZoneNameText());
					errorList.Add(new ErrorData(ErrorData.Severity.Error, strError, zoneDetails.m_CantReachErrorID));
				}
			}
			return false;
		}
		if (bTestForEscapes)
		{
			for (int j = 1; j < 6; j++)
			{
				BaseLevelManager.LayerDataCollection layerDataCollection = BaseLevelManager.GetInstance().m_BuildingLayers[j];
				int num = 0;
				for (int k = 0; k < 120; k++)
				{
					for (int l = 0; l < 120; l++)
					{
						BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num];
						BaseLevelManager.TileProperty[] tileProperties;
						int num2;
						(tileProperties = layerDataCollection.m_TileProperties)[num2 = num] = tileProperties[num2] & ~(BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.ScanBlockedMask | BaseLevelManager.TileProperty.SafeMask);
						if ((tileProperty & BaseLevelManager.TileProperty.ObjectMask) != 0)
						{
							BuildingBlock_Object buildingBlock_Object2 = BuildingBlockManager.GetBlock(layerDataCollection.m_ObjectTileIDs[num]) as BuildingBlock_Object;
							if (buildingBlock_Object2 != null)
							{
								GameObject gameObject = layerDataCollection.m_ObjectTileObjects[num];
								if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.EscapeMarkerV2) != 0 && gameObject != null)
								{
									int num3 = -1;
									int instanceID = gameObject.GetInstanceID();
									for (int num4 = list.Count - 1; num4 >= 0; num4--)
									{
										if (list[num4].m_iInstanceID == instanceID)
										{
											num3 = num4;
											break;
										}
									}
									if (num3 == -1)
									{
										EscapeMarkersFoundV2 escapeMarkersFoundV = new EscapeMarkersFoundV2();
										escapeMarkersFoundV.m_Layer = j;
										escapeMarkersFoundV.m_iInstanceID = gameObject.GetInstanceID();
										escapeMarkersFoundV.m_BlockID = buildingBlock_Object2.m_ID;
										list.Add(escapeMarkersFoundV);
										num3 = list.Count - 1;
									}
									list[num3].m_Index.Add(num);
									if (k > 0)
									{
										list[num3].m_Index.Add(num - 120);
									}
									if (k < 119)
									{
										list[num3].m_Index.Add(num + 120);
									}
									if (l > 0)
									{
										list[num3].m_Index.Add(num - 1);
									}
									if (l < 119)
									{
										list[num3].m_Index.Add(num + 1);
									}
								}
							}
						}
						num++;
					}
				}
			}
		}
		else
		{
			for (int m = 1; m < 6; m++)
			{
				BaseLevelManager.LayerDataCollection layerDataCollection2 = BaseLevelManager.GetInstance().m_BuildingLayers[m];
				for (int n = 0; n < 14400; n++)
				{
					BaseLevelManager.TileProperty[] tileProperties;
					int num5;
					(tileProperties = layerDataCollection2.m_TileProperties)[num5 = n] = tileProperties[num5] & ~(BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.ScanBlockedMask | BaseLevelManager.TileProperty.SafeMask);
				}
			}
		}
		for (int num6 = 0; m_SearchLayers.Count > num6; num6++)
		{
			if (m_SearchLayers[num6].m_Layer != -1)
			{
				int num7 = m_SearchLayers[num6].m_X;
				int num8 = m_SearchLayers[num6].m_Y;
				m_ScanLocations[0] = (num8 << 8) + num7;
				m_ScanCurrentIndex = 0;
				m_ScanFirstFree = 1;
				m_ScanTotalUsed = 1;
				BaseLevelManager.LayerDataCollection layerData = BaseLevelManager.GetInstance().m_BuildingLayers[m_SearchLayers[num6].m_Layer];
				int ilayer = m_SearchLayers[num6].m_Layer;
				while (m_ScanTotalUsed > 0)
				{
					ScanForWalkableSpace(ilayer, layerData);
				}
			}
		}
		if (!bTestForEscapes)
		{
			return false;
		}
		for (int num9 = list.Count - 1; num9 >= 0; num9--)
		{
			EscapeMarkersFoundV2 escapeMarkersFoundV2 = list[num9];
			for (int num10 = escapeMarkersFoundV2.m_Index.Count - 1; num10 >= 0; num10--)
			{
				int num11 = escapeMarkersFoundV2.m_Index[num10];
				BaseLevelManager.TileProperty tileProperty2 = BaseLevelManager.GetInstance().m_BuildingLayers[escapeMarkersFoundV2.m_Layer].m_TileProperties[num11];
				if ((tileProperty2 & BaseLevelManager.TileProperty.SafeMask) == BaseLevelManager.TileProperty.SafeMask)
				{
					BaseBuildingBlock buildingBlock = m_BlockManager.GetBuildingBlock(escapeMarkersFoundV2.m_BlockID);
					if (buildingBlock != null)
					{
						string localizedStringWithLocalizedSwap = GetLocalizedStringWithLocalizedSwap("Text.Editor.EscapePossible.Room", "%escapetype%", buildingBlock.m_BlockNameID);
						int iX = num11 % 120;
						int iY = num11 / 120;
						if (string.IsNullOrEmpty(localizedStringWithLocalizedSwap))
						{
							errorList.Add(new ErrorData(ErrorData.Severity.Warning, "[Text.Editor.EscapePossible.Room][" + buildingBlock.m_BlockNameID + "]", list[num9].m_Layer, iX, iY));
						}
						else
						{
							errorList.Add(new ErrorData(ErrorData.Severity.Warning, localizedStringWithLocalizedSwap, list[num9].m_Layer, iX, iY));
						}
					}
					break;
				}
			}
		}
		int[] array = new int[3] { 1, 3, 5 };
		int num12 = 118;
		for (int num13 = 0; num13 < array.Length; num13++)
		{
			BaseLevelManager.LayerDataCollection layer = BaseLevelManager.GetInstance().m_BuildingLayers[array[num13]];
			if (SafeFound(0, 1, 120, layer) || SafeFound(120, 120, num12 - 2, layer) || SafeFound(239, 120, num12 - 2, layer) || SafeFound(120 * (num12 - 1), 1, 120, layer))
			{
				string localizedString = GetLocalizedString("Text.Editor.EscapePossible.Border", "Text.Editor.EscapePossible.Border");
				errorList.Add(new ErrorData(ErrorData.ErrorType.BasicError, ErrorData.Severity.Warning, localizedString, array[num13], 60, 60));
				break;
			}
			num12++;
		}
		return count != errorList.Count;
	}

	public bool SafeFound(int iIndex, int iStep, int iCount, BaseLevelManager.LayerDataCollection layer)
	{
		while (iCount > 0)
		{
			if ((layer.m_TileProperties[iIndex] & BaseLevelManager.TileProperty.SafeMask) == BaseLevelManager.TileProperty.SafeMask)
			{
				return true;
			}
			iIndex += iStep;
			iCount--;
		}
		return false;
	}

	private void AddtoScanList(int iX, int iY)
	{
		if (m_ScanTotalUsed < 10000)
		{
			m_ScanLocations[m_ScanFirstFree] = (iY << 8) + iX;
			m_ScanFirstFree++;
			if (m_ScanFirstFree >= 10000)
			{
				m_ScanFirstFree = 0;
			}
			m_ScanTotalUsed++;
		}
	}

	private void ScanForWalkableSpace(int ilayer, BaseLevelManager.LayerDataCollection layerData)
	{
		int num = m_ScanLocations[m_ScanCurrentIndex] >> 8;
		int num2 = m_ScanLocations[m_ScanCurrentIndex] & 0xFF;
		int num3 = num * 120 + num2;
		int num4 = 0;
		m_ScanCurrentIndex++;
		if (m_ScanCurrentIndex >= 10000)
		{
			m_ScanCurrentIndex = 0;
		}
		m_ScanTotalUsed--;
		int num5;
		BaseLevelManager.TileProperty[] tileProperties;
		(tileProperties = layerData.m_TileProperties)[num5 = num3] = tileProperties[num5] | BaseLevelManager.TileProperty.SafeMask;
		if ((layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.EntranceMask) == BaseLevelManager.TileProperty.EntranceMask)
		{
			int iFoundInLayer = -1;
			int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref layerData, num3);
			num4 = LevelSetup_Transistion.FindExit(ilayer, roomNumberFromProperty, ref iFoundInLayer);
			if (num4 != -1)
			{
				MarkersFound item = default(MarkersFound);
				item.m_Layer = (sbyte)iFoundInLayer;
				item.m_Y = (sbyte)(num4 / 120);
				item.m_X = (sbyte)(num4 % 120);
				m_SearchLayers.Add(item);
			}
		}
		if (num2 > 0)
		{
			num4 = num3 - 1;
			BaseLevelManager.TileProperty tileProperty = layerData.m_TileProperties[num3];
			if ((tileProperty & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
			{
				tileProperty = layerData.m_TileProperties[num4];
				if ((tileProperty & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask && !IsSpotBlockedForPlayer(num4, layerData))
				{
					AddtoScanList(num2 - 1, num);
					int num6;
					(tileProperties = layerData.m_TileProperties)[num6 = num4] = tileProperties[num6] | BaseLevelManager.TileProperty.ScannedMask;
				}
				if ((tileProperty & (BaseLevelManager.TileProperty.ItsADoorMask | BaseLevelManager.TileProperty.WallMask)) == 0)
				{
					int num7;
					(tileProperties = layerData.m_TileProperties)[num7 = num4] = tileProperties[num7] | (BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.SafeMask);
				}
				else
				{
					int num8;
					(tileProperties = layerData.m_TileProperties)[num8 = num4] = tileProperties[num8] | BaseLevelManager.TileProperty.ScannedMask;
				}
			}
		}
		if (num2 + 1 < 120)
		{
			num4 = num3 + 1;
			BaseLevelManager.TileProperty tileProperty2 = layerData.m_TileProperties[num3];
			if ((tileProperty2 & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
			{
				tileProperty2 = layerData.m_TileProperties[num4];
				if ((tileProperty2 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask && !IsSpotBlockedForPlayer(num4, layerData))
				{
					AddtoScanList(num2 + 1, num);
					int num9;
					(tileProperties = layerData.m_TileProperties)[num9 = num4] = tileProperties[num9] | BaseLevelManager.TileProperty.ScannedMask;
				}
				if ((tileProperty2 & (BaseLevelManager.TileProperty.ItsADoorMask | BaseLevelManager.TileProperty.WallMask)) == 0)
				{
					int num10;
					(tileProperties = layerData.m_TileProperties)[num10 = num4] = tileProperties[num10] | (BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.SafeMask);
				}
				else
				{
					int num11;
					(tileProperties = layerData.m_TileProperties)[num11 = num4] = tileProperties[num11] | BaseLevelManager.TileProperty.ScannedMask;
				}
			}
		}
		if (num > 0)
		{
			num4 = num3 - 120;
			BaseLevelManager.TileProperty tileProperty3 = layerData.m_TileProperties[num3];
			if ((tileProperty3 & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) != BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
			{
				tileProperty3 = layerData.m_TileProperties[num4];
				if ((tileProperty3 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask && !IsSpotBlockedForPlayer(num4, layerData))
				{
					AddtoScanList(num2, num - 1);
					int num12;
					(tileProperties = layerData.m_TileProperties)[num12 = num4] = tileProperties[num12] | BaseLevelManager.TileProperty.ScannedMask;
				}
				if ((tileProperty3 & (BaseLevelManager.TileProperty.ItsADoorMask | BaseLevelManager.TileProperty.WallMask)) == 0)
				{
					int num13;
					(tileProperties = layerData.m_TileProperties)[num13 = num4] = tileProperties[num13] | (BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.SafeMask);
				}
				else
				{
					int num14;
					(tileProperties = layerData.m_TileProperties)[num14 = num4] = tileProperties[num14] | BaseLevelManager.TileProperty.ScannedMask;
				}
			}
		}
		if (num + 1 >= 120)
		{
			return;
		}
		num4 = num3 + 120;
		BaseLevelManager.TileProperty tileProperty4 = layerData.m_TileProperties[num3];
		if ((tileProperty4 & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) != BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
		{
			tileProperty4 = layerData.m_TileProperties[num4];
			if ((tileProperty4 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask && !IsSpotBlockedForPlayer(num4, layerData))
			{
				AddtoScanList(num2, num + 1);
				int num15;
				(tileProperties = layerData.m_TileProperties)[num15 = num4] = tileProperties[num15] | BaseLevelManager.TileProperty.ScannedMask;
			}
			if ((tileProperty4 & (BaseLevelManager.TileProperty.ItsADoorMask | BaseLevelManager.TileProperty.WallMask)) == 0)
			{
				int num16;
				(tileProperties = layerData.m_TileProperties)[num16 = num4] = tileProperties[num16] | (BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.SafeMask);
			}
			else
			{
				int num17;
				(tileProperties = layerData.m_TileProperties)[num17 = num4] = tileProperties[num17] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
	}

	private bool IsSpotBlockedForPlayer(int iIndex, BaseLevelManager.LayerDataCollection layerData)
	{
		BaseLevelManager.TileProperty tileProperty = layerData.m_TileProperties[iIndex];
		BaseLevelManager.TileIDData buildingBrickID;
		if ((tileProperty & BaseLevelManager.TileProperty.ObjDecMask) != 0)
		{
			buildingBrickID = layerData.m_ObjectTileIDs[iIndex];
			BuildingBlock_Object buildingBlock_Object = m_BlockManager.GetBuildingBlock(buildingBrickID) as BuildingBlock_Object;
			if (buildingBlock_Object != null && buildingBlock_Object.m_BlocksPlayer)
			{
				return true;
			}
		}
		buildingBrickID = layerData.m_TileTileIDs[iIndex];
		BuildingBlock_Tile buildingBlock_Tile = m_BlockManager.GetBuildingBlock(buildingBrickID) as BuildingBlock_Tile;
		if (buildingBlock_Tile != null && buildingBlock_Tile.m_BlocksPlayer)
		{
			return true;
		}
		return false;
	}

	public LevelEditorDataVersion SetDataVersion(LevelEditorDataVersion eVersion)
	{
		if (eVersion != 0 && eVersion != c_CurrentLevelDataVersionNumber)
		{
			c_CurrentLevelDataVersionNumber = eVersion;
			if (m_BlockManager != null)
			{
				m_BlockManager.InitializeBlockInstructions();
			}
		}
		return c_CurrentLevelDataVersionNumber;
	}

	public void UpdateReachableFlags()
	{
		m_SearchLayers.Clear();
		m_SearchBlockers.Clear();
		LevelEditor_ZoneManager levelEditor_ZoneManager = null;
		if (levelEditor_ZoneManager == null && (levelEditor_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
			return;
		}
		int iStartingIndex = 0;
		LevelEditor_ZoneManager.Zone zoneOfType = levelEditor_ZoneManager.GetZoneOfType(ref iStartingIndex, ZoneDetailsManager.ZoneTypes.RollCall);
		if (zoneOfType == null)
		{
			levelEditor_ZoneManager.SetEverythingAsReachable();
			return;
		}
		for (int i = 1; i < 6; i++)
		{
			BaseLevelManager.LayerDataCollection layerDataCollection = BaseLevelManager.GetInstance().m_BuildingLayers[i];
			for (int j = 0; j < 14400; j++)
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int num;
				(tileProperties = layerDataCollection.m_TileProperties)[num = j] = tileProperties[num] & ~(BaseLevelManager.TileProperty.ScannedMask | BaseLevelManager.TileProperty.CanReachRollCallMask | BaseLevelManager.TileProperty.SafeMask);
			}
		}
		int count = zoneOfType.m_BlocksInZone.Count;
		for (int k = 0; k < count; k++)
		{
			if (zoneOfType.m_BlocksInZone[k] != null)
			{
				BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(zoneOfType.m_BlocksInZone[k].m_BlockID) as BuildingBlock_Object;
				if (buildingBlock_Object != null && (buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.RollCallMarker) == BuildingBlock_Object.SpecialFlagsEnum.RollCallMarker)
				{
					MarkersFound item = default(MarkersFound);
					item.m_Layer = (sbyte)zoneOfType.m_Layer;
					item.m_Y = (sbyte)(zoneOfType.m_BlocksInZone[k].m_Y + zoneOfType.m_Bottom);
					item.m_X = (sbyte)(zoneOfType.m_BlocksInZone[k].m_X + zoneOfType.m_Left);
					m_SearchLayers.Add(item);
					break;
				}
			}
		}
		if (m_SearchLayers.Count == 0)
		{
			levelEditor_ZoneManager.SetEverythingAsReachable();
			return;
		}
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m_SearchLayers.Count > m; m++)
			{
				if (m_SearchLayers[m].m_Layer != -1)
				{
					int num2 = m_SearchLayers[m].m_X;
					int num3 = m_SearchLayers[m].m_Y;
					int num4 = m_SearchLayers[m].m_Layer;
					m_ScanLocations[0] = (num3 << 8) + num2;
					m_ScanCurrentIndex = 0;
					m_ScanFirstFree = 1;
					m_ScanTotalUsed = 1;
					BaseLevelManager.LayerDataCollection layerData = BaseLevelManager.GetInstance().m_BuildingLayers[num4];
					while (m_ScanTotalUsed > 0)
					{
						ScanForWalkableAndMarkBlockers(num4, layerData);
					}
				}
			}
			if (l == 0)
			{
				m_SearchLayers.Clear();
				CheckZonesForReachable(ZoneDetailsManager.MustBeReachableBy.Player);
				m_SearchLayers.AddRange(m_SearchBlockers);
				m_SearchBlockers.Clear();
			}
			else if (m_SearchBlockers.Count != 0)
			{
				m_SearchLayers.AddRange(m_SearchBlockers);
				m_SearchBlockers.Clear();
				l--;
			}
			else
			{
				CheckZonesForReachable(ZoneDetailsManager.MustBeReachableBy.Anyone);
			}
		}
		m_SearchLayers.Clear();
		m_SearchBlockers.Clear();
	}

	private void CheckZonesForReachable(ZoneDetailsManager.MustBeReachableBy reachableBy)
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		int totalZones = instance.GetTotalZones();
		for (int i = 0; i < totalZones; i++)
		{
			LevelEditor_ZoneManager.Zone zone = instance.GetZone(i, bSupressWarning: true);
			if (zone == null || zone.m_ZoneDetails == null || zone.m_ZoneDetails.m_MustBeReachableBy != reachableBy)
			{
				continue;
			}
			int[] map = instance.GetZoneMap(zone.m_Layer).m_Map;
			int totalBlocked = zone.m_TotalBlocked;
			zone.m_TotalBlocked = zone.m_BlocksInZone.Count;
			for (int num = zone.m_TotalBlocked - 1; num >= 0; num--)
			{
				LevelEditor_ZoneManager.Zone.ObjectsInZone objectsInZone = zone.m_BlocksInZone[num];
				objectsInZone.m_GoodInteractPoint = -1;
				if (objectsInZone.m_InteractPoints.Count != 0)
				{
					if (objectsInZone.m_ComplexID != 0)
					{
						objectsInZone.m_BeingBlocked = false;
						zone.m_TotalBlocked--;
						int count = objectsInZone.m_InteractPoints.Count;
						bool flag = false;
						for (int j = 0; j < count; j++)
						{
							if (objectsInZone.m_InteractPoints[j] == -1)
							{
								if (!flag)
								{
									objectsInZone.m_BeingBlocked = true;
									zone.m_TotalBlocked++;
									break;
								}
								flag = false;
								continue;
							}
							BaseLevelManager.TileProperty tileProperty = BaseLevelManager.GetInstance().m_BuildingLayers[(uint)zone.m_Layer].m_TileProperties[objectsInZone.m_InteractPoints[j]];
							if ((tileProperty & BaseLevelManager.TileProperty.CanReachRollCallMask) != BaseLevelManager.TileProperty.CanReachRollCallMask)
							{
								continue;
							}
							objectsInZone.m_GoodInteractPoint = objectsInZone.m_InteractPoints[j];
							if (map[objectsInZone.m_GoodInteractPoint] == zone.m_ID)
							{
								while (++j < count && objectsInZone.m_InteractPoints[j] != -1)
								{
								}
								flag = false;
							}
							else
							{
								flag = true;
							}
						}
					}
					else
					{
						objectsInZone.m_BeingBlocked = true;
						for (int num2 = objectsInZone.m_InteractPoints.Count - 1; num2 >= 0; num2--)
						{
							BaseLevelManager.TileProperty tileProperty2 = BaseLevelManager.GetInstance().m_BuildingLayers[(uint)zone.m_Layer].m_TileProperties[objectsInZone.m_InteractPoints[num2]];
							if ((tileProperty2 & BaseLevelManager.TileProperty.CanReachRollCallMask) == BaseLevelManager.TileProperty.CanReachRollCallMask)
							{
								if (objectsInZone.m_BeingBlocked)
								{
									objectsInZone.m_BeingBlocked = false;
									zone.m_TotalBlocked--;
								}
								objectsInZone.m_GoodInteractPoint = objectsInZone.m_InteractPoints[num2];
								if (map[objectsInZone.m_GoodInteractPoint] == zone.m_ID)
								{
									break;
								}
							}
						}
					}
				}
				else
				{
					objectsInZone.m_BeingBlocked = false;
					zone.m_TotalBlocked--;
				}
			}
			zone.m_ZoneUpdateCount++;
			zone.m_bReachable = zone.m_TotalBlocked == 0;
		}
	}

	private void ScanForWalkableAndMarkBlockers(int ilayer, BaseLevelManager.LayerDataCollection layerData)
	{
		int num = m_ScanLocations[m_ScanCurrentIndex] >> 8;
		int num2 = m_ScanLocations[m_ScanCurrentIndex] & 0xFF;
		int num3 = num * 120 + num2;
		int num4 = 0;
		m_ScanCurrentIndex++;
		if (m_ScanCurrentIndex >= 10000)
		{
			m_ScanCurrentIndex = 0;
		}
		m_ScanTotalUsed--;
		int num5;
		BaseLevelManager.TileProperty[] tileProperties;
		(tileProperties = layerData.m_TileProperties)[num5 = num3] = tileProperties[num5] | BaseLevelManager.TileProperty.CanReachRollCallMask;
		if ((layerData.m_TileProperties[num3] & BaseLevelManager.TileProperty.EntranceMask) == BaseLevelManager.TileProperty.EntranceMask)
		{
			int iFoundInLayer = -1;
			int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref layerData, num3);
			num4 = LevelSetup_Transistion.FindExit(ilayer, roomNumberFromProperty, ref iFoundInLayer);
			if (num4 != -1)
			{
				MarkersFound item = default(MarkersFound);
				item.m_Layer = (sbyte)iFoundInLayer;
				item.m_Y = (sbyte)(num4 / 120);
				item.m_X = (sbyte)(num4 % 120);
				m_SearchLayers.Add(item);
			}
		}
		if (num2 > 0)
		{
			num4 = num3 - 1;
			BaseLevelManager.TileProperty tileProperty = layerData.m_TileProperties[num3];
			if ((tileProperty & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
			{
				tileProperty = layerData.m_TileProperties[num4];
				if ((tileProperty & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
				{
					if (!IsSpotBlockedForPlayer(num4, layerData))
					{
						AddtoScanList(num2 - 1, num);
					}
					else
					{
						MarkersFound item2 = default(MarkersFound);
						item2.m_Layer = (sbyte)ilayer;
						item2.m_X = (sbyte)(num2 - 1);
						item2.m_Y = (sbyte)num;
						m_SearchBlockers.Add(item2);
					}
				}
				int num6;
				(tileProperties = layerData.m_TileProperties)[num6 = num4] = tileProperties[num6] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num2 + 1 < 120)
		{
			num4 = num3 + 1;
			BaseLevelManager.TileProperty tileProperty2 = layerData.m_TileProperties[num3];
			if ((tileProperty2 & BaseLevelManager.TileProperty.Blocked_Horizontal_Bits) != BaseLevelManager.TileProperty.Blocked_Horizontal_Bits)
			{
				tileProperty2 = layerData.m_TileProperties[num4];
				if ((tileProperty2 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Horizontal_Bits | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
				{
					if (!IsSpotBlockedForPlayer(num4, layerData))
					{
						AddtoScanList(num2 + 1, num);
					}
					else
					{
						MarkersFound item3 = default(MarkersFound);
						item3.m_Layer = (sbyte)ilayer;
						item3.m_X = (sbyte)(num2 + 1);
						item3.m_Y = (sbyte)num;
						m_SearchBlockers.Add(item3);
					}
				}
				int num7;
				(tileProperties = layerData.m_TileProperties)[num7 = num4] = tileProperties[num7] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num > 0)
		{
			num4 = num3 - 120;
			BaseLevelManager.TileProperty tileProperty3 = layerData.m_TileProperties[num3];
			if ((tileProperty3 & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) != BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
			{
				tileProperty3 = layerData.m_TileProperties[num4];
				if ((tileProperty3 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
				{
					if (!IsSpotBlockedForPlayer(num4, layerData))
					{
						AddtoScanList(num2, num - 1);
					}
					else
					{
						MarkersFound item4 = default(MarkersFound);
						item4.m_Layer = (sbyte)ilayer;
						item4.m_X = (sbyte)num2;
						item4.m_Y = (sbyte)(num - 1);
						m_SearchBlockers.Add(item4);
					}
				}
				int num8;
				(tileProperties = layerData.m_TileProperties)[num8 = num4] = tileProperties[num8] | BaseLevelManager.TileProperty.ScannedMask;
			}
		}
		if (num + 1 >= 120)
		{
			return;
		}
		num4 = num3 + 120;
		BaseLevelManager.TileProperty tileProperty4 = layerData.m_TileProperties[num3];
		if ((tileProperty4 & BaseLevelManager.TileProperty.Blocked_Vertical_Mask) == BaseLevelManager.TileProperty.Blocked_Vertical_Mask)
		{
			return;
		}
		tileProperty4 = layerData.m_TileProperties[num4];
		if ((tileProperty4 & (BaseLevelManager.TileProperty.AlreadySafeOrBlocked | BaseLevelManager.TileProperty.Blocked_Vertical_Mask | BaseLevelManager.TileProperty.ScannedMask)) == BaseLevelManager.TileProperty.TileMask)
		{
			if (!IsSpotBlockedForPlayer(num4, layerData))
			{
				AddtoScanList(num2, num + 1);
				int num9;
				(tileProperties = layerData.m_TileProperties)[num9 = num4] = tileProperties[num9] | BaseLevelManager.TileProperty.ScannedMask;
			}
			else
			{
				MarkersFound item5 = default(MarkersFound);
				item5.m_Layer = (sbyte)ilayer;
				item5.m_X = (sbyte)num2;
				item5.m_Y = (sbyte)(num + 1);
				m_SearchBlockers.Add(item5);
			}
		}
		int num10;
		(tileProperties = layerData.m_TileProperties)[num10 = num4] = tileProperties[num10] | BaseLevelManager.TileProperty.ScannedMask;
	}

	public static LevelEditorDataVersion GetMasterDataVersion()
	{
		return LevelDetails.GetMasterDataVersion();
	}
}

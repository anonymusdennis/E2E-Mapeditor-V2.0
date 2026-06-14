using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveManager : T17MonoBehaviour
{
	private struct PrisonName
	{
		public string m_strName;

		public string m_strSanitizedName;
	}

	public delegate void UserLoggedIn();

	public delegate void OnGameSavedHandler(bool bSuccess);

	[Serializable]
	public class PrisonsSaveInformation
	{
		[Serializable]
		public class PrisonData
		{
			[Serializable]
			public class SlotData
			{
				public enum SlotStatus
				{
					Empty,
					Used,
					Corrupt,
					OldVersion,
					INVALID
				}

				public string m_strFileName = string.Empty;

				public SlotStatus m_Status;

				public string m_strDate = "Text.BLANK";

				public int m_iDays = -1;

				public int m_iDataVersion = -1;

				public T17NetRoomGameView.GameRoomType m_GameRoomType;

				public bool m_bSomethingChanged = true;

				public long m_iDateAsLong;

				public string m_RoomPassword = string.Empty;
			}

			public enum PrisonType
			{
				eDefault,
				eCustomLevel,
				eUGCLevel
			}

			public string m_strPrisonName = string.Empty;

			public string m_strPrisonFileName = string.Empty;

			public PrisonType m_PrisonID;

			public string m_strExtra1 = string.Empty;

			public bool m_bNeedsSaving;

			public int m_iContinueSlot = -1;

			public string[] m_strSerializedSlots = new string[10];

			public string m_strPrisonTitle = string.Empty;

			public string m_strPrisonDescription = string.Empty;

			public int[] m_NumPrisonRoles = new int[2];

			public LevelDetailsManager.DiffecultyLevel m_ePrisonDifficultyLevel = LevelDetailsManager.DiffecultyLevel.Medium;

			public LevelScript.PRISON_ENUM m_OutfitType = LevelScript.PRISON_ENUM.Centre_Perks;

			public LevelDetailsManager.LevelEditorDataVersion m_EditorVersion = LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease;

			public bool m_bHasFinished;

			[NonSerialized]
			public SlotData[] m_Slots = new SlotData[10];

			[NonSerialized]
			public bool m_bUGCFound;

			public void SerializeData()
			{
				for (int i = 0; i < 10; i++)
				{
					m_strSerializedSlots[i] = JsonUtility.ToJson(m_Slots[i]);
				}
			}

			public void DeSerializeData()
			{
				for (int i = 0; i < 10; i++)
				{
					try
					{
						m_Slots[i] = JsonUtility.FromJson<SlotData>(m_strSerializedSlots[i]);
					}
					catch
					{
						m_Slots[i] = new SlotData();
					}
					m_strSerializedSlots[i] = string.Empty;
				}
			}

			public string GetSlotFileName(int iSlot)
			{
				if (iSlot < m_Slots.Length)
				{
					if (string.IsNullOrEmpty(m_Slots[iSlot].m_strFileName))
					{
						string text = ((!(PlatformIO.GetInstance() != null)) ? m_strPrisonName : PlatformIO.GetInstance().GetSanitisedString(m_strPrisonName));
						return "Save" + text + "S" + iSlot + ".sav";
					}
					return m_Slots[iSlot].m_strFileName;
				}
				return "ErrorFile" + iSlot;
			}

			public int GetMostRecentSave()
			{
				if (m_iContinueSlot != -1 && m_iContinueSlot >= 0 && m_iContinueSlot < m_Slots.Length && (m_Slots[m_iContinueSlot].m_Status == SlotData.SlotStatus.Used || m_Slots[m_iContinueSlot].m_Status == SlotData.SlotStatus.OldVersion))
				{
					return m_iContinueSlot;
				}
				int num = -1;
				long num2 = -1L;
				for (int num3 = m_Slots.Length - 1; num3 >= 0; num3--)
				{
					if ((m_Slots[num3].m_Status == SlotData.SlotStatus.Used || m_Slots[num3].m_Status == SlotData.SlotStatus.OldVersion) && m_Slots[num3].m_iDateAsLong > num2)
					{
						num2 = m_Slots[num3].m_iDateAsLong;
						num = num3;
					}
				}
				m_iContinueSlot = num;
				return num;
			}

			public string GetPrisonDirectoryName()
			{
				if (string.IsNullOrEmpty(m_strPrisonFileName))
				{
					string text = string.Empty;
					string empty = string.Empty;
					string empty2 = string.Empty;
					empty = ((!(PlatformIO.GetInstance() != null)) ? m_strPrisonName : PlatformIO.GetInstance().GetSanitisedString(m_strPrisonName));
					if (m_PrisonID == PrisonType.eDefault)
					{
						text = "ESC2P";
						empty2 = string.Empty;
					}
					else if (m_PrisonID == PrisonType.eCustomLevel)
					{
						text = "ESC2U";
						empty2 = string.Empty;
					}
					else if (m_PrisonID == PrisonType.eUGCLevel)
					{
						text = "ESC2UGC";
						empty2 = string.Empty;
					}
					return text + empty + empty2;
				}
				return m_strPrisonFileName;
			}
		}

		[NonSerialized]
		public List<PrisonData> m_Prisons = new List<PrisonData>();

		public List<string> m_strSerializedPrisons = new List<string>();

		public string SerializeData()
		{
			int count = m_Prisons.Count;
			m_strSerializedPrisons.Clear();
			for (int i = 0; i < count; i++)
			{
				m_Prisons[i].SerializeData();
				m_Prisons[i].m_bNeedsSaving = false;
				m_strSerializedPrisons.Add(JsonUtility.ToJson(m_Prisons[i]));
			}
			return JsonUtility.ToJson(this);
		}

		public void DeSerializeData()
		{
			int count = m_strSerializedPrisons.Count;
			m_Prisons.Clear();
			for (int i = 0; i < count; i++)
			{
				try
				{
					PrisonData item = JsonUtility.FromJson<PrisonData>(m_strSerializedPrisons[i]);
					m_Prisons.Add(item);
					m_Prisons[m_Prisons.Count - 1].DeSerializeData();
				}
				catch
				{
				}
			}
			m_strSerializedPrisons.Clear();
		}

		public int CreatePrison(string strPrisonName, PrisonData.PrisonType iPrisonID)
		{
			int prisonIndex = GetPrisonIndex(strPrisonName, iPrisonID);
			if (prisonIndex != -1)
			{
				return prisonIndex;
			}
			PrisonData prisonData = new PrisonData();
			prisonData.m_strPrisonName = strPrisonName;
			prisonData.m_PrisonID = iPrisonID;
			prisonData.m_bNeedsSaving = true;
			prisonData.m_strPrisonFileName = prisonData.GetPrisonDirectoryName();
			for (int i = 0; i < 10; i++)
			{
				prisonData.m_Slots[i] = new PrisonData.SlotData();
			}
			m_Prisons.Add(prisonData);
			return m_Prisons.Count - 1;
		}

		public void DeletePrison(string strPrisonName, PrisonData.PrisonType prisonID)
		{
			if (prisonID != PrisonData.PrisonType.eCustomLevel)
			{
				Debug.LogError("SaveManager - Tried to delete a non custom prison!!");
				return;
			}
			int prisonIndex = GetPrisonIndex(strPrisonName, prisonID);
			if (prisonIndex != -1)
			{
				m_Prisons.RemoveAt(prisonIndex);
			}
		}

		public void LoadPrison(string strSerializedPrisonData)
		{
			try
			{
				PrisonData prisonData = JsonUtility.FromJson<PrisonData>(strSerializedPrisonData);
				if (!string.IsNullOrEmpty(prisonData.m_strPrisonName) && GetPrisonIndex(prisonData.m_strPrisonName, prisonData.m_PrisonID) == -1)
				{
					prisonData.m_bNeedsSaving = false;
					m_Prisons.Add(prisonData);
				}
			}
			catch
			{
			}
		}

		public int GetPrisonIndex(string strPrisonName, PrisonData.PrisonType iPrisonID)
		{
			bool flag = iPrisonID != PrisonData.PrisonType.eDefault;
			for (int num = m_Prisons.Count - 1; num >= 0; num--)
			{
				if (string.CompareOrdinal(m_Prisons[num].m_strPrisonName, strPrisonName) == 0)
				{
					if (m_Prisons[num].m_PrisonID == iPrisonID)
					{
						return num;
					}
				}
				else if (flag && string.CompareOrdinal(m_Prisons[num].m_strPrisonFileName, strPrisonName) == 0 && m_Prisons[num].m_PrisonID == iPrisonID)
				{
					return num;
				}
			}
			return -1;
		}
	}

	public enum SaveManagerStatus
	{
		WaitingForLogin,
		WaitingForPlatform,
		WaitingToGetDirectory,
		Idle,
		GetPrisonDirectory,
		RemakePrisonDirectory,
		ScanPrisonIndex,
		ScanPrisonSlots,
		PendingSaveGame,
		SaveGame,
		SaveDirectory,
		WaitingForSaveSpaceCheck,
		StartGame,
		StartGameWithPendingSave,
		StartGameTutorial,
		DestroyAndRemakePrisonDirectory
	}

	public enum SaveUIMode
	{
		None,
		NewGame,
		LoadGame,
		ContinueGame,
		GuestSave
	}

	public enum SaveMode
	{
		Automatic,
		Manual,
		Off
	}

	public delegate void OnSaveModeChangedDelegate(SaveMode newSaveMode);

	private const string m_LevelFileName = "Level.dat";

	private const string m_LevelFinalFileName = "Level_Finished.dat";

	private string m_SaveSpaceFileName = string.Empty;

	private string m_SaveSpaceDirName = "Temp";

	private static SaveManager m_Instance;

	private const int m_NumberOfSlots = 10;

	public const int m_InvalidSlot = -1;

	private const int m_InvalidPrison = -1;

	private const int m_InvalidDay = -1;

	[Tooltip("How long before we give up on the platform telling us the file names")]
	public float m_FileListTimeout = 4f;

	[Tooltip("How long before we give up on the platform loading a file")]
	public float m_FileLoadTimeout = 4f;

	[Tooltip("How long before we give up on the platform saving a file")]
	public float m_FileSaveTimeout = 5f;

	[Tooltip("How long before we give up on the platform deleting a file")]
	public float m_FileDeleteTimeout = 5f;

	private int m_iSlot_Selected = -1;

	private SlotSelectionMenu m_CurrentSlotSelectionMenu;

	private int m_ScanningPrison = -1;

	private int m_ScanningSlot = -1;

	private string m_strCurrentSaveGame = string.Empty;

	private string m_strCurrentLoadedGame = string.Empty;

	private PlatformIO m_PlatformIO;

	public Sprite m_OfflineSprite;

	public Sprite m_PublicSprite;

	public Sprite m_PrivateSprite;

	private List<PrisonName> m_PrisonNames = new List<PrisonName>();

	private bool m_bDayBeforeMonth = true;

	private static UserLoggedIn c_UserLoggedInCallBack;

	private OnGameSavedHandler m_OnGameSaved;

	public PrisonsSaveInformation m_PrisonData;

	public int m_iCurrentPrison = -1;

	private string[] m_SlotStatusStrings = new string[5];

	private SaveUIMode m_eUIMode;

	private SaveManagerStatus m_eManagerState;

	public OnSaveModeChangedDelegate OnSaveModeChanged;

	private SaveMode m_CurrentSaveMode = SaveMode.Off;

	private SaveMode m_UserSelectedSaveMode;

	private const string m_UserSaveModeKey = "SaveManager:UserSaveMode";

	private bool m_bSaveModeLoaded;

	public SaveMode CurrentSaveMode
	{
		get
		{
			GlobalStart instance = GlobalStart.GetInstance();
			if (instance != null && instance.CurrentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.IN_LEVEL)
			{
				return m_CurrentSaveMode;
			}
			return m_UserSelectedSaveMode;
		}
		private set
		{
			if (m_CurrentSaveMode != value)
			{
				m_CurrentSaveMode = value;
				if (OnSaveModeChanged != null)
				{
					OnSaveModeChanged(m_CurrentSaveMode);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance != null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(this);
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		NetLoadSync.RegisterOnReadyToPlayInterest(OnPlayerReadyToPlay, bAdd: false);
		Gamer.OnDeleteRequested -= OnPlayerDeleteRequested;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public static SaveManager GetInstance()
	{
		return m_Instance;
	}

	private void Start()
	{
	}

	public void LoadSaveManagerData()
	{
		if (!m_bSaveModeLoaded)
		{
			GlobalSave instance = GlobalSave.GetInstance();
			if (!(instance == null))
			{
				m_bSaveModeLoaded = true;
				int value = 0;
				instance.Get("SaveManager:UserSaveMode", out value, 0);
				m_UserSelectedSaveMode = (SaveMode)value;
			}
		}
	}

	private void Update()
	{
		switch (m_eManagerState)
		{
		case SaveManagerStatus.WaitingForLogin:
			if (c_UserLoggedInCallBack != null)
			{
				SetManagerState(SaveManagerStatus.WaitingForPlatform);
			}
			break;
		case SaveManagerStatus.WaitingForPlatform:
			if (Platform.IsPlatformCreated() && PlatformIO.GetInstance() != null && PlatformIO.GetInstance().IsPlatformReady() && Localization.AreLanguagesLoaded())
			{
				if (Platform.GetInstance() != null)
				{
					Platform.GetInstance().RegisterOnUGCChange(OnUserGeneratedContentUpdated);
				}
				InitializeVars();
				SetManagerState(SaveManagerStatus.WaitingToGetDirectory);
			}
			break;
		case SaveManagerStatus.WaitingToGetDirectory:
			SetManagerState(SaveManagerStatus.GetPrisonDirectory);
			break;
		case SaveManagerStatus.GetPrisonDirectory:
			break;
		case SaveManagerStatus.RemakePrisonDirectory:
			break;
		case SaveManagerStatus.ScanPrisonIndex:
			break;
		case SaveManagerStatus.ScanPrisonSlots:
			break;
		case SaveManagerStatus.Idle:
			if (c_UserLoggedInCallBack != null)
			{
				SetManagerState(SaveManagerStatus.WaitingForPlatform);
			}
			else if (!string.IsNullOrEmpty(m_strCurrentSaveGame))
			{
				SetManagerState(SaveManagerStatus.PendingSaveGame);
			}
			else
			{
				if (m_PrisonData == null || m_PrisonData.m_Prisons == null)
				{
					break;
				}
				for (int num = m_PrisonData.m_Prisons.Count - 1; num >= 0; num--)
				{
					if (m_PrisonData.m_Prisons[num].m_bNeedsSaving)
					{
						SetManagerState(SaveManagerStatus.SaveDirectory);
						break;
					}
				}
			}
			break;
		case SaveManagerStatus.SaveDirectory:
			break;
		case SaveManagerStatus.PendingSaveGame:
			if (ShouldWeAbortSave())
			{
				SetManagerState(SaveManagerStatus.Idle);
			}
			else if (IsTheGameReadyToBeSaved() && IsThePlayerInTheGame())
			{
				SaveTheGame();
			}
			break;
		case SaveManagerStatus.SaveGame:
			break;
		case SaveManagerStatus.StartGameWithPendingSave:
			if (GlobalStart.GetInstance() != null)
			{
				SetManagerState(SaveManagerStatus.PendingSaveGame, bProcess: false);
				GlobalStart.GetInstance().SetupLoadStatesAndNewCampaignPrisonSetup();
			}
			break;
		case SaveManagerStatus.StartGame:
			ProcessStartGameState();
			break;
		case SaveManagerStatus.StartGameTutorial:
			ProcessStartTutorialState();
			break;
		case SaveManagerStatus.DestroyAndRemakePrisonDirectory:
			break;
		case SaveManagerStatus.WaitingForSaveSpaceCheck:
			break;
		}
	}

	private void ProcessStartGameState()
	{
		if (!(GlobalStart.GetInstance() != null))
		{
			return;
		}
		SetManagerState(SaveManagerStatus.Idle);
		if (m_eUIMode == SaveUIMode.NewGame)
		{
			GlobalStart.GetInstance().SetupLoadStatesAndNewCampaignPrisonSetup();
			return;
		}
		T17NetRoomGameView.GameRoomType roomType = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_GameRoomType;
		if (roomType == T17NetRoomGameView.GameRoomType.Public || roomType == T17NetRoomGameView.GameRoomType.Private)
		{
			NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: false, bProfanityCheck: false, delegate(bool isConnected)
			{
				if (isConnected)
				{
					RequestStartGameWithRoomtype(roomType);
				}
				else
				{
					PromptUserFailedOnlineGame();
				}
			}, delegate
			{
				m_strCurrentLoadedGame = string.Empty;
			});
		}
		else
		{
			RequestStartGameWithRoomtype(roomType);
		}
	}

	private void ProcessStartTutorialState()
	{
		SetManagerState(SaveManagerStatus.Idle);
		PrisonConfig.ConfigType currentSelectedConfigType = GlobalStart.GetInstance().GetCurrentSelectedConfigType();
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			for (int i = 0; i < currentSelectedPrisonData.m_RoleStartingOutfitData.Length; i++)
			{
				if (currentSelectedPrisonData.m_RoleStartingOutfitData[i] != null)
				{
					switch (currentSelectedPrisonData.m_RoleStartingOutfitData[i].m_OutfitData.m_Type)
					{
					case Item_Outfit.OutFitType.Inmate:
						currentSelectedPrisonData.m_RoleStartingOutfitData[i].m_OutfitData.m_OutfitAppearance = CustomisationData.Outfit.INMATE_01;
						break;
					case Item_Outfit.OutFitType.Guard:
						currentSelectedPrisonData.m_RoleStartingOutfitData[i].m_OutfitData.m_OutfitAppearance = CustomisationData.Outfit.GUARD_01;
						break;
					}
				}
			}
		}
		NetCreateRoomHelper.RequestCreateRoom(T17NetRoomGameView.GameRoomType.Offline, currentSelectedConfigType, delegate(bool roomSetupOk)
		{
			if (roomSetupOk)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Tutorial", "Tutorial Started", string.Empty, 0L);
				GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(GlobalStart.GLOBALSTART_GAME_MODES.SINGLE);
			}
			else
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.CreateRoom.FailedTitle", "Text.Dialog.Net.CreateRoom.FailedBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.Show();
			}
		}, showDialogs: true, string.Empty);
	}

	private void RequestStartGameWithRoomtype(T17NetRoomGameView.GameRoomType roomType)
	{
		PrisonConfig.ConfigType currentSelectedConfigType = GlobalStart.GetInstance().GetCurrentSelectedConfigType();
		T17NetRoomGameView.GameRoomType roomType2 = roomType;
		PrisonConfig.ConfigType config = currentSelectedConfigType;
		NetCreateRoomHelper.RoomJoinedHandler callback = delegate(bool setupOk)
		{
			if (setupOk)
			{
				if (roomType == T17NetRoomGameView.GameRoomType.Offline)
				{
					GlobalStart.GetInstance().SetupLoadStatesAndStartGameWithConfig();
				}
				else
				{
					GlobalStart.GetInstance().SetupLoadStatesAndStartGameWithConfig(GlobalStart.GLOBALSTART_GAME_MODES.ONLINE);
				}
			}
			else if (roomType != 0)
			{
				PromptUserFailedOnlineGame();
			}
			else
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog != null)
				{
					dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.CreateRoom.FailedTitle", "Text.Dialog.Net.CreateRoom.FailedBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
					dialog.SetSymbol(T17DialogBox.Symbols.Error);
					dialog.Show();
				}
			}
		};
		string password = ((m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_GameRoomType != T17NetRoomGameView.GameRoomType.Public) ? string.Empty : Encryption.Decrypt(m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_RoomPassword, "default"));
		NetCreateRoomHelper.RequestCreateRoom(roomType2, config, callback, showDialogs: true, password);
	}

	private void PromptUserFailedOnlineGame()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Dialog.Net.GoOnline.FailedTitle", "Text.Menu.Campaign.SetupOnlinePrisonFailed", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.SetSymbol(T17DialogBox.Symbols.Error);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, (T17DialogBox.DialogEvent)delegate
			{
				RequestStartGameWithRoomtype(T17NetRoomGameView.GameRoomType.Offline);
			});
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, (T17DialogBox.DialogEvent)delegate
			{
				m_strCurrentLoadedGame = string.Empty;
			});
			dialog.Show();
		}
	}

	public void ResetBackToIdle()
	{
		SetManagerState(SaveManagerStatus.Idle);
	}

	private bool ValidateIndexes(string strFrom, int iPrison, int iSlot)
	{
		if (m_PrisonData == null || m_PrisonData.m_Prisons == null)
		{
			return false;
		}
		if (iPrison == -1)
		{
			return false;
		}
		if (iPrison < 0 || iPrison >= m_PrisonData.m_Prisons.Count)
		{
			return false;
		}
		if (iSlot < 0 || iSlot >= 10)
		{
			return false;
		}
		return true;
	}

	public static int GetTotalSlots()
	{
		return 10;
	}

	private void SetManagerState(SaveManagerStatus eNewState, bool bProcess = true)
	{
		if (m_eManagerState != eNewState)
		{
		}
		m_eManagerState = eNewState;
		if (!bProcess)
		{
			return;
		}
		switch (m_eManagerState)
		{
		case SaveManagerStatus.WaitingForPlatform:
			break;
		case SaveManagerStatus.WaitingToGetDirectory:
			break;
		case SaveManagerStatus.GetPrisonDirectory:
			m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Next, "Master", "Directory", m_FileLoadTimeout, RequestDirectoryFileResult, null);
			break;
		case SaveManagerStatus.DestroyAndRemakePrisonDirectory:
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.High, m_SaveSpaceDirName, m_SaveSpaceFileName, m_FileDeleteTimeout, RequestDeleteSpaceCheckFileResultThenRemakeSavemanager, null);
			break;
		case SaveManagerStatus.RemakePrisonDirectory:
			m_PrisonData = new PrisonsSaveInformation();
			m_PlatformIO.RequestDirectoryList(PlatformIO.IORequest.IORequestPriority.Next, string.Empty, "*", m_FileListTimeout, GetPrisonDirectoryResult, null);
			break;
		case SaveManagerStatus.ScanPrisonIndex:
			if (m_ScanningPrison < m_PrisonData.m_Prisons.Count)
			{
				m_PlatformIO.RequestFileList(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_ScanningPrison), "*.sav", m_FileListTimeout, GetScanPrisonResult, null);
				break;
			}
			ValidateDirectoryData();
			DirectoryLoadFinished();
			SetManagerState(SaveManagerStatus.SaveDirectory);
			break;
		case SaveManagerStatus.ScanPrisonSlots:
			while (m_ScanningSlot < 10)
			{
				if (m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_Status == PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID)
				{
					m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_ScanningPrison), m_PrisonData.m_Prisons[m_ScanningPrison].GetSlotFileName(m_ScanningSlot), m_FileLoadTimeout, GetScanPrisonSlotResult, null);
					break;
				}
				m_ScanningSlot++;
			}
			if (m_ScanningSlot >= 10)
			{
				m_ScanningPrison++;
				SetManagerState(SaveManagerStatus.ScanPrisonIndex);
			}
			break;
		case SaveManagerStatus.Idle:
			if (m_iCurrentPrison == -1 && GlobalStart.GetInstance() != null)
			{
				string currentSelectedLevel = GlobalStart.GetInstance().GetCurrentSelectedLevel();
				if (!string.IsNullOrEmpty(currentSelectedLevel))
				{
					SetCurrentPrison(currentSelectedLevel, PrisonsSaveInformation.PrisonData.PrisonType.eDefault);
				}
			}
			break;
		case SaveManagerStatus.SaveDirectory:
			SaveDirectory();
			break;
		case SaveManagerStatus.PendingSaveGame:
			if (IsTheGameReadyToBeSaved() && IsThePlayerInTheGame())
			{
				SaveTheGame();
			}
			break;
		case SaveManagerStatus.SaveGame:
			break;
		case SaveManagerStatus.StartGame:
		case SaveManagerStatus.StartGameWithPendingSave:
		case SaveManagerStatus.StartGameTutorial:
			break;
		case SaveManagerStatus.WaitingForSaveSpaceCheck:
			break;
		}
	}

	public void ResetUIMode(bool setEverythingChanged = false)
	{
		if (setEverythingChanged)
		{
			SetEverythingChanged();
		}
		m_eUIMode = SaveUIMode.None;
		m_CurrentSaveMode = SaveMode.Off;
	}

	private void SetUIMode(SaveUIMode eNewState)
	{
		SetEverythingChanged();
		m_eUIMode = eNewState;
		switch (m_eUIMode)
		{
		case SaveUIMode.ContinueGame:
			CurrentSaveMode = m_UserSelectedSaveMode;
			break;
		case SaveUIMode.GuestSave:
			CurrentSaveMode = m_UserSelectedSaveMode;
			break;
		case SaveUIMode.LoadGame:
			CurrentSaveMode = m_UserSelectedSaveMode;
			break;
		case SaveUIMode.NewGame:
			CurrentSaveMode = m_UserSelectedSaveMode;
			break;
		case SaveUIMode.None:
			CurrentSaveMode = SaveMode.Off;
			break;
		}
	}

	public SaveUIMode GetUIMode()
	{
		return m_eUIMode;
	}

	private void SaveDirectory()
	{
		if (!ValidateIndexes("SaveDirectory", 0, 0))
		{
			SetManagerState(SaveManagerStatus.Idle);
			return;
		}
		string objectData = m_PrisonData.SerializeData();
		object platformData = null;
		m_PlatformIO.RequestSaveFile(PlatformIO.IORequest.IORequestPriority.Next, "Master", "Directory", objectData, m_FileSaveTimeout, SaveDirectoryResult, platformData);
	}

	private void SaveDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
			SetManagerState(SaveManagerStatus.Idle);
			break;
		case PlatformIO.IOResultEnum.IOCanceled:
			SetManagerState(SaveManagerStatus.Idle);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			ShowDialog("Text.SS.CorruptDir.Title", "Text.SS.CorruptDir.Body", CorruptDirResponceYes, CorruptDirResponceNo, T17DialogBox.Symbols.Error, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: true);
			break;
		default:
			SetManagerState(SaveManagerStatus.SaveDirectory);
			break;
		}
	}

	private void CorruptDirResponceYes(T17DialogBox dialog)
	{
		m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, "Master", "Directory", m_FileDeleteTimeout, RequestDeleteDirFileResult, null);
	}

	private void CorruptDirResponceNo(T17DialogBox dialog)
	{
		SetManagerState(SaveManagerStatus.SaveDirectory);
	}

	private void RequestDeleteDirFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			SetManagerState(SaveManagerStatus.SaveDirectory);
			break;
		case PlatformIO.IOResultEnum.IOCanceled:
			SetManagerState(SaveManagerStatus.Idle);
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			ShowMessageDialog("Text.SS.CantDelete.Title", "Text.SS.CantDelete.Body", CantDeleteDirDialogResponce, T17DialogBox.Symbols.Error);
			break;
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			SetManagerState(SaveManagerStatus.SaveDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			SetManagerState(SaveManagerStatus.SaveDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void CantDeleteDirDialogResponce(T17DialogBox dialog)
	{
		SetManagerState(SaveManagerStatus.Idle);
	}

	private void ValidateDirectoryData()
	{
		string[] names = Enum.GetNames(typeof(LevelScript.PRISON_ENUM));
		int num = names.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_PrisonData.GetPrisonIndex(names[i], PrisonsSaveInformation.PrisonData.PrisonType.eDefault) == -1)
			{
				m_PrisonData.CreatePrison(names[i], PrisonsSaveInformation.PrisonData.PrisonType.eDefault);
			}
		}
	}

	private void InitializeVars()
	{
		if (m_PlatformIO == null)
		{
			m_PlatformIO = PlatformIO.GetInstance();
			string[] names = Enum.GetNames(typeof(LevelScript.PRISON_ENUM));
			for (int num = names.Length - 1; num >= 0; num--)
			{
				PrisonName item = default(PrisonName);
				item.m_strName = names[num];
				item.m_strSanitizedName = m_PlatformIO.GetSanitisedString(names[num]);
				m_PrisonNames.Add(item);
			}
			string longDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
			int num2 = longDatePattern.IndexOf("d", StringComparison.OrdinalIgnoreCase);
			int num3 = longDatePattern.IndexOf("m", StringComparison.OrdinalIgnoreCase);
			if (num2 >= 0 && num3 >= 0 && num3 < num2)
			{
				m_bDayBeforeMonth = false;
			}
			NetLoadSync.RegisterOnReadyToPlayInterest(OnPlayerReadyToPlay);
			Gamer.OnDeleteRequested += OnPlayerDeleteRequested;
		}
	}

	private string GetPrisonNameFromSanitizedName(string strSanitizedPrison)
	{
		for (int num = m_PrisonNames.Count - 1; num >= 0; num--)
		{
			if (string.CompareOrdinal(strSanitizedPrison, m_PrisonNames[num].m_strSanitizedName) == 0)
			{
				return m_PrisonNames[num].m_strName;
			}
		}
		return string.Empty;
	}

	private void DirectoryLoadFinished()
	{
		if (c_UserLoggedInCallBack != null)
		{
			c_UserLoggedInCallBack();
			c_UserLoggedInCallBack = null;
		}
	}

	private string GetOwnerName(int iPrisonIndex)
	{
		if (!ValidateIndexes("GetOwnerName", iPrisonIndex, 0))
		{
			return string.Empty;
		}
		return m_PrisonData.m_Prisons[iPrisonIndex].GetPrisonDirectoryName();
	}

	private void SetSlotStatus(int iPrisonIndex, int iSlotNumber, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus eStatus)
	{
		if (ValidateIndexes("SetSlotStatus", iPrisonIndex, iSlotNumber))
		{
			switch (eStatus)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
				m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_iDays = -1;
				m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_strDate = string.Empty;
				break;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_iDays = -1;
				m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_strDate = string.Empty;
				break;
			}
			m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_Status = eStatus;
			m_PrisonData.m_Prisons[iPrisonIndex].m_Slots[iSlotNumber].m_bSomethingChanged = true;
			m_PrisonData.m_Prisons[iPrisonIndex].m_bNeedsSaving = true;
		}
	}

	private void GetPrisonDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
			DirectoryLoadFinished();
			SetManagerState(SaveManagerStatus.Idle);
			return;
		}
		List<string> list = m_PlatformIO.GetCache() as List<string>;
		Regex regex = new Regex("(^ESC2P)(.+)");
		int num = 3;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Match match = regex.Match(list[i]);
			if (match != null && match.Groups.Count == num)
			{
				string prisonNameFromSanitizedName = GetPrisonNameFromSanitizedName(match.Groups[2].Value);
				if (!string.IsNullOrEmpty(prisonNameFromSanitizedName) && m_PrisonData.GetPrisonIndex(prisonNameFromSanitizedName, PrisonsSaveInformation.PrisonData.PrisonType.eDefault) == -1)
				{
					int index = m_PrisonData.CreatePrison(prisonNameFromSanitizedName, PrisonsSaveInformation.PrisonData.PrisonType.eDefault);
					m_PrisonData.m_Prisons[index].m_bNeedsSaving = false;
				}
			}
		}
		Regex regex2 = new Regex("(^ESC2U)(.+)");
		num = 3;
		for (int j = 0; j < count; j++)
		{
			Match match2 = regex2.Match(list[j]);
			if (match2 == null || match2.Groups.Count != num)
			{
				continue;
			}
			string value = match2.Groups[2].Value;
			if (string.IsNullOrEmpty(value) || m_PrisonData.GetPrisonIndex(value, PrisonsSaveInformation.PrisonData.PrisonType.eDefault) != -1)
			{
				continue;
			}
			string path = m_PlatformIO.GetPath(list[j]) + "Level.dat";
			string path2 = m_PlatformIO.GetPath(list[j]) + "Level_Finished.dat";
			if (!File.Exists(path))
			{
				continue;
			}
			int index2 = m_PrisonData.CreatePrison(value, PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
			m_PrisonData.m_Prisons[index2].m_bNeedsSaving = false;
			try
			{
				if (File.Exists(path2))
				{
					m_PrisonData.m_Prisons[index2].m_bHasFinished = true;
				}
				else
				{
					m_PrisonData.m_Prisons[index2].m_bHasFinished = false;
				}
				List<byte> LevelData = new List<byte>();
				LevelData.AddRange(File.ReadAllBytes(path));
				LevelDetailsManager.FrontendLevelDetails frontendDataForLevel = LevelDetailsManager.GetFrontendDataForLevel(ref LevelData);
				m_PrisonData.m_Prisons[index2].m_strPrisonTitle = frontendDataForLevel.m_LevelName;
				m_PrisonData.m_Prisons[index2].m_strPrisonDescription = frontendDataForLevel.m_LevelDescription;
				m_PrisonData.m_Prisons[index2].m_ePrisonDifficultyLevel = frontendDataForLevel.m_DifficultyLevel;
				m_PrisonData.m_Prisons[index2].m_NumPrisonRoles[0] = frontendDataForLevel.m_NumberOfInmates;
				m_PrisonData.m_Prisons[index2].m_NumPrisonRoles[1] = frontendDataForLevel.m_NumberOfGuards;
				m_PrisonData.m_Prisons[index2].m_OutfitType = frontendDataForLevel.m_OutfitType;
				m_PrisonData.m_Prisons[index2].m_EditorVersion = frontendDataForLevel.m_DataVersion;
			}
			catch (Exception)
			{
			}
		}
		Regex regex3 = new Regex("(^ESC2UGC)(.+)");
		num = 3;
		for (int k = 0; k < count; k++)
		{
			Match match3 = regex3.Match(list[k]);
			if (match3 != null && match3.Groups.Count == num)
			{
				string value2 = match3.Groups[2].Value;
				if (!string.IsNullOrEmpty(value2) && m_PrisonData.GetPrisonIndex(value2, PrisonsSaveInformation.PrisonData.PrisonType.eDefault) == -1)
				{
					int index3 = m_PrisonData.CreatePrison(value2, PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
					m_PrisonData.m_Prisons[index3].m_bNeedsSaving = false;
					m_PrisonData.m_Prisons[index3].m_bUGCFound = false;
				}
			}
		}
		OnUserGeneratedContentUpdated();
		m_ScanningPrison = 0;
		SetManagerState(SaveManagerStatus.ScanPrisonIndex);
	}

	private void GetScanPrisonResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
			m_ScanningSlot = 0;
			SetManagerState(SaveManagerStatus.ScanPrisonSlots);
			return;
		}
		List<string> list = m_PlatformIO.GetCache() as List<string>;
		Regex regex = new Regex("(^Save" + m_PlatformIO.GetSanitisedString(m_PrisonData.m_Prisons[m_ScanningPrison].m_strPrisonName) + ")(S)(\\d+)");
		int count = list.Count;
		int result = 0;
		for (int i = 0; i < count; i++)
		{
			Match match = regex.Match(list[i]);
			if (match != null && match.Groups.Count == 4 && int.TryParse(match.Groups[3].Value, out result) && result >= 0 && result < 10)
			{
				m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[result].m_strFileName = "Save" + m_PlatformIO.GetSanitisedString(m_PrisonData.m_Prisons[m_ScanningPrison].m_strPrisonName) + "S" + result + ".sav";
				m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[result].m_Status = PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID;
			}
		}
		m_ScanningSlot = 0;
		SetManagerState(SaveManagerStatus.ScanPrisonSlots);
	}

	private void GetScanPrisonSlotResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			SetSlotStatus(m_ScanningPrison, m_ScanningSlot, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			break;
		case PlatformIO.IOResultEnum.IOCanceled:
			SetManagerState(SaveManagerStatus.WaitingToGetDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			SetSlotStatus(m_ScanningPrison, m_ScanningSlot, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt);
			break;
		case PlatformIO.IOResultEnum.IOSuccessful:
		{
			bool flag = false;
			try
			{
				PrisonSnapshotIO.SnapshotData_SaveGame_Base snapshotData_SaveGame_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_SaveGame_Base>(m_PlatformIO.GetCache() as string);
				if (snapshotData_SaveGame_Base.m_Version > 0)
				{
					m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_iDays = snapshotData_SaveGame_Base.m_iDaysInPrison;
					m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_strDate = snapshotData_SaveGame_Base.m_strDateSaved;
					m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_iDataVersion = snapshotData_SaveGame_Base.m_iDataVersion;
					m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_GameRoomType = (T17NetRoomGameView.GameRoomType)snapshotData_SaveGame_Base.m_GameRoomType;
					m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_iDateAsLong = snapshotData_SaveGame_Base.m_iDateAsLong;
					SetSlotStatus(m_ScanningPrison, m_ScanningSlot, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used);
					if (snapshotData_SaveGame_Base.m_iDataVersion > 14)
					{
						PrisonSnapshotIO.SnapshotData_SaveGame_V2 snapshotData_SaveGame_V = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_SaveGame_V2>(m_PlatformIO.GetCache() as string);
						m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_RoomPassword = snapshotData_SaveGame_V.m_RoomPassword;
					}
					if (snapshotData_SaveGame_Base.m_iDataVersion == 14 && PrisonSnapshotIO.GetDataVersion() == 15)
					{
						m_PrisonData.m_Prisons[m_ScanningPrison].m_Slots[m_ScanningSlot].m_RoomPassword = string.Empty;
					}
					else if (snapshotData_SaveGame_Base.m_iDataVersion != PrisonSnapshotIO.GetDataVersion())
					{
						SetSlotStatus(m_ScanningPrison, m_ScanningSlot, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion);
					}
					flag = true;
				}
			}
			catch
			{
			}
			if (!flag)
			{
				SetSlotStatus(m_ScanningPrison, m_ScanningSlot, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt);
			}
			break;
		}
		}
		m_ScanningSlot++;
		SetManagerState(SaveManagerStatus.ScanPrisonSlots);
	}

	private void RequestLoadingFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("RequestLoadingFileResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			return;
		}
		string text = string.Empty;
		bool flag = false;
		bool flag2 = false;
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
			m_strCurrentLoadedGame = m_PlatformIO.GetCache() as string;
			if (ValidateSave(m_strCurrentLoadedGame))
			{
				if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot != m_iSlot_Selected)
				{
					m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = m_iSlot_Selected;
					m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
				}
				CloseSlotSelection();
				SetManagerState(SaveManagerStatus.StartGame);
			}
			else
			{
				text = "0310: ";
				flag = true;
			}
			break;
		case PlatformIO.IOResultEnum.IOCanceled:
			SetManagerState(SaveManagerStatus.WaitingToGetDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			text = "0311: ";
			flag = true;
			break;
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			flag2 = true;
			break;
		case PlatformIO.IOResultEnum.IOFailed:
			text = "0312: ";
			flag = true;
			break;
		}
		if (!flag && !flag2)
		{
			return;
		}
		if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot == m_iSlot_Selected)
		{
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = -1;
		}
		m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
		if (flag)
		{
			string strMessage;
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1)
			{
				Localization.Get("Text.SS.Corrupt.BodyUsed", out var localized);
				strMessage = text + localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
			}
			else
			{
				Localization.Get("Text.SS.Corrupt.Body", out var localized2);
				strMessage = text + localized2;
			}
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt);
			ShowDialog("Text.SS.Corrupt.Title", strMessage, CorruptResponceYes, ClearSelectedSlot, T17DialogBox.Symbols.Error, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: false);
		}
		if (flag2)
		{
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			bool bLocalizeMessagae = true;
			string strMessage2;
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1)
			{
				Localization.Get("Text.SS.Missing.BodyUsed", out var localized3);
				strMessage2 = localized3.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
				bLocalizeMessagae = false;
			}
			else
			{
				strMessage2 = "Text.SS.Missing.Body";
			}
			ShowMessageDialog("Text.SS.Missing.Title", strMessage2, ClearSelectedSlot, T17DialogBox.Symbols.Error, bLocalizeTitle: true, bLocalizeMessagae);
		}
	}

	private void RequestDeleteFileCorruptSaveResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("RequestDeleteFileCorruptSaveResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			OnSaveGame(bSaved: false);
			return;
		}
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			SaveGame(m_OnGameSaved, bNow: true, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			SaveGame(m_OnGameSaved, bNow: true, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailed:
			ShowMessageDialog("Text.SS.CantDelete.Title", "Text.SS.CantDelete.Body", ClearSelectedSlot, T17DialogBox.Symbols.Error);
			SaveGame(m_OnGameSaved, bNow: true, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			SaveGame(m_OnGameSaved, bNow: true, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			OnSaveGame(bSaved: false);
			break;
		case PlatformIO.IOResultEnum.IOCanceled:
			SaveGame(m_OnGameSaved, bNow: true, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void RequestDeleteFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("RequestDeleteFileResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			return;
		}
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot == m_iSlot_Selected)
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = -1;
			}
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
		case PlatformIO.IOResultEnum.IOCanceled:
		{
			bool bLocalizeMessagae = true;
			string strMessage;
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1)
			{
				Localization.Get("Text.SS.CantDelete.BodyUsed", out var localized);
				strMessage = localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
				bLocalizeMessagae = false;
			}
			else
			{
				strMessage = "Text.SS.CantDelete.Body";
			}
			ShowMessageDialog("Text.SS.CantDelete.Title", strMessage, ClearSelectedSlot, T17DialogBox.Symbols.Error, bLocalizeTitle: true, bLocalizeMessagae);
			break;
		}
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			break;
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void RequestDeleteSpaceCheckFileResultThenRemakeSavemanager(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
		}
		m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.High, "Master", "Directory", m_FileDeleteTimeout, RequestDeleteMasterDirectoryResult, null);
	}

	private void RequestDeleteMasterDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
		}
		SetManagerState(SaveManagerStatus.RemakePrisonDirectory);
	}

	private void RequestDirectoryFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		{
			bool flag = false;
			try
			{
				m_PrisonData = JsonUtility.FromJson<PrisonsSaveInformation>(m_PlatformIO.GetCache() as string);
				m_PrisonData.DeSerializeData();
				OnUserGeneratedContentUpdated();
				ValidateDirectoryData();
				SetEverythingChanged();
				SetManagerState(SaveManagerStatus.Idle);
				flag = true;
				DirectoryLoadFinished();
			}
			catch (Exception)
			{
			}
			if (!flag)
			{
				m_PlatformIO.RequestDirectoryList(PlatformIO.IORequest.IORequestPriority.Next, string.Empty, "*", m_FileListTimeout, GetPrisonDirectoryResult, null);
				SetManagerState(SaveManagerStatus.RemakePrisonDirectory);
			}
			break;
		}
		case PlatformIO.IOResultEnum.IOCanceled:
			SetManagerState(SaveManagerStatus.RemakePrisonDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			PromptUserForCorruptMasterDirectoryRemake();
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			SetManagerState(SaveManagerStatus.RemakePrisonDirectory);
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void PromptUserForCorruptMasterDirectoryRemake()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.SS.CorruptDir.Title", "Text.SS.CorruptDir.Body", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.SetSymbol(T17DialogBox.Symbols.Error);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, (T17DialogBox.DialogEvent)delegate
			{
				SetManagerState(SaveManagerStatus.DestroyAndRemakePrisonDirectory);
			});
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, (T17DialogBox.DialogEvent)delegate
			{
				SetManagerState(SaveManagerStatus.GetPrisonDirectory);
			});
			dialog.Show();
		}
		else
		{
			SetManagerState(SaveManagerStatus.GetPrisonDirectory);
		}
	}

	private void RequestSaveFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("RequestSaveFileResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			return;
		}
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		{
			if (RoutineManager.GetInstance() != null)
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays = RoutineManager.GetInstance().GetDaysSinceInitialTime() + 1;
			}
			else
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays = 0;
			}
			T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
			if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue))
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_GameRoomType = outValue;
			}
			string outValue2 = string.Empty;
			if (outValue == T17NetRoomGameView.GameRoomType.Public && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.Password, ref outValue2))
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_RoomPassword = outValue2;
			}
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate = GetTodaysDateAndTime();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDataVersion = PrisonSnapshotIO.GetDataVersion();
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used);
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot != m_iSlot_Selected)
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = m_iSlot_Selected;
			}
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_bSomethingChanged = true;
			OnSaveGame(bSaved: true);
			SetManagerState(SaveManagerStatus.Idle);
			break;
		}
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
			SaveGame(m_OnGameSaved, bNow: false, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt);
			ShowDialog("Text.SS.CorruptDir.Title", "Text.SS.CorruptDir.Body", CorruptSaveResponceYes, CorruptSaveResponceNo, T17DialogBox.Symbols.Error, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: true);
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
		case PlatformIO.IOResultEnum.IOCanceled:
			SaveGame(m_OnGameSaved, bNow: false, bRequestIsARetryFromWithin: true);
			break;
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void EnoughRoomConfirm(T17DialogBox dialog)
	{
		SetManagerState(SaveManagerStatus.Idle);
	}

	private bool ValidateSave(string strSave)
	{
		if (string.IsNullOrEmpty(strSave))
		{
			return false;
		}
		try
		{
			PrisonSnapshotIO.SnapshotData_SaveGame_Base snapshotData_SaveGame_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_SaveGame_Base>(strSave);
			if (snapshotData_SaveGame_Base.m_Version == -1)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	private void GuestSaveSlotSelected(int iIndex)
	{
		if (ValidateIndexes("GuestSaveSlotSelected", m_iCurrentPrison, iIndex))
		{
			m_iSlot_Selected = iIndex;
			switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_Status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				CloseSlotSelection();
				SaveGame(null);
				break;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			{
				Localization.Get("Text.SS.Overwrite.BodyUsed", out var localized);
				ShowDialog("Text.SS.Overwrite.Title", localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate), DeleteAndOverwriteResponceYes, ClearSelectedSlot, T17DialogBox.Symbols.Warning, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: false);
				break;
			}
			}
		}
	}

	private void NewGameSlotSelected(int iIndex)
	{
		if (!ValidateIndexes("NewGameSlotSelected", m_iCurrentPrison, iIndex))
		{
			return;
		}
		m_iSlot_Selected = iIndex;
		switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_Status)
		{
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			m_strCurrentSaveGame = string.Empty;
			SetManagerState(SaveManagerStatus.WaitingForSaveSpaceCheck);
			m_SaveSpaceFileName = m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected);
			DoSaveSpaceCheck();
			break;
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
		{
			string strMessage;
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1)
			{
				Localization.Get("Text.SS.Overwrite.BodyUsed", out var localized);
				strMessage = localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
			}
			else
			{
				strMessage = "Text.SS.Overwrite.Body";
			}
			ShowDialog("Text.SS.Overwrite.Title", strMessage, OverwriteResponceYes, ClearSelectedSlot, T17DialogBox.Symbols.Warning, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: false);
			break;
		}
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			ShowDialog("Text.SS.Overwrite.Title", "Text.SS.Overwrite.Body", DeleteAndOverwriteResponceYes, ClearSelectedSlot, T17DialogBox.Symbols.Error, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: true);
			break;
		}
	}

	private void LoadGameSlotSelected(int iIndex)
	{
		if (!ValidateIndexes("LoadGameSlotSelected", m_iCurrentPrison, iIndex))
		{
			return;
		}
		m_iSlot_Selected = iIndex;
		switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_Status)
		{
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			m_iSlot_Selected = -1;
			break;
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
		{
			bool flag = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1;
			string strMessage;
			if (flag)
			{
				Localization.Get("Text.SS.Corrupt.BodyUsed", out var localized);
				strMessage = localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
			}
			else
			{
				strMessage = "Text.SS.Corrupt.Body";
			}
			ShowDialog("Text.SS.Corrupt.Title", strMessage, CorruptResponceYes, ClearSelectedSlot, T17DialogBox.Symbols.Error, bLocalizeDialogueTitle: true, !flag);
			break;
		}
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
			m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Normal, GetOwnerName(m_iCurrentPrison), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), m_FileLoadTimeout, RequestLoadingFileResult, null);
			break;
		}
	}

	public void SetDeleteSlotSelected(int slot)
	{
		m_iSlot_Selected = slot;
	}

	private void DeleteGameSlotSelected()
	{
		switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_Status)
		{
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			m_iSlot_Selected = -1;
			break;
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
			ShowDialog("Text.SS.Delete.Title", "Text.SS.Delete.Body", DeleteGameResponceYes, DeleteGameResponceNo, T17DialogBox.Symbols.Warning, bLocalizeDialogueTitle: true, bLocalizeDialogueBody: true);
			break;
		}
	}

	private void DeleteGameResponceYes(T17DialogBox dialog)
	{
		if (ValidateIndexes("DeleteGameResponceYes", m_iCurrentPrison, m_iSlot_Selected))
		{
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_iCurrentPrison), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), m_FileDeleteTimeout, RequestDeleteGameFileResult, null);
		}
	}

	private void DeleteGameResponceNo(T17DialogBox dialog)
	{
		if (ValidateIndexes("DeleteGameResponceNo", m_iCurrentPrison, m_iSlot_Selected))
		{
			SetManagerState(SaveManagerStatus.Idle);
		}
	}

	private void RequestDeleteGameFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("RequestDeleteGameFileResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			return;
		}
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot == m_iSlot_Selected)
			{
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = -1;
			}
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
			SetManagerState(SaveManagerStatus.Idle);
			if (m_eUIMode == SaveUIMode.LoadGame && !AreThereAnySaves())
			{
				CloseSlotSelection();
			}
			break;
		default:
			SetManagerState(SaveManagerStatus.Idle);
			break;
		}
	}

	public void ClearSelectedSlot(T17DialogBox dialog)
	{
		m_iSlot_Selected = -1;
	}

	public bool IsSlotValid()
	{
		return m_iSlot_Selected >= 0 && m_iSlot_Selected < 10;
	}

	private void DoNothing(T17DialogBox dialog)
	{
	}

	private void BackToIdleFromCorruptSave(T17DialogBox dialog)
	{
		OnSaveGame(bSaved: false);
		SetManagerState(SaveManagerStatus.Idle);
	}

	private void DeleteAndOverwriteResponceYes(T17DialogBox dialog)
	{
		m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_iCurrentPrison), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), m_FileDeleteTimeout, DeleteAndOverwriteResult, null);
	}

	private void DeleteAndOverwriteResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (!ValidateIndexes("DeleteAndOverwriteResult", m_iCurrentPrison, m_iSlot_Selected))
		{
			return;
		}
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			SetSlotStatus(m_iCurrentPrison, m_iSlot_Selected, PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty);
			switch (m_eUIMode)
			{
			case SaveUIMode.GuestSave:
				CloseSlotSelection();
				SetManagerState(SaveManagerStatus.PendingSaveGame);
				break;
			case SaveUIMode.NewGame:
				m_strCurrentSaveGame = string.Empty;
				SetManagerState(SaveManagerStatus.StartGameWithPendingSave);
				break;
			}
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
		case PlatformIO.IOResultEnum.IOCanceled:
		{
			string strMessage;
			if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_iDays != -1)
			{
				Localization.Get("Text.SS.CantDelete.BodyUsed", out var localized);
				strMessage = localized.Replace("$dayofsave", m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[m_iSlot_Selected].m_strDate);
			}
			else
			{
				strMessage = "Text.SS.CantDelete.Body";
			}
			ShowMessageDialog("Text.SS.CantDelete.Title", strMessage, ClearSelectedSlot, T17DialogBox.Symbols.Error);
			break;
		}
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			break;
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
		case PlatformIO.IOResultEnum.IOFailedBusy:
			break;
		}
	}

	private void OverwriteResponceYes(T17DialogBox dialog)
	{
		switch (m_eUIMode)
		{
		case SaveUIMode.GuestSave:
			CloseSlotSelection();
			SetManagerState(SaveManagerStatus.PendingSaveGame);
			break;
		case SaveUIMode.NewGame:
			m_strCurrentSaveGame = string.Empty;
			SetManagerState(SaveManagerStatus.StartGameWithPendingSave);
			break;
		}
	}

	private void CorruptResponceYes(T17DialogBox dialog)
	{
		if (ValidateIndexes("CorruptResponceYes", m_iCurrentPrison, m_iSlot_Selected))
		{
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_iCurrentPrison), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), m_FileDeleteTimeout, RequestDeleteFileResult, null);
		}
	}

	private void CorruptSaveResponceYes(T17DialogBox dialog)
	{
		if (!ValidateIndexes("CorruptSaveResponceYes", m_iCurrentPrison, m_iSlot_Selected))
		{
			OnSaveGame(bSaved: false);
		}
		else
		{
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, GetOwnerName(m_iCurrentPrison), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), m_FileDeleteTimeout, RequestDeleteFileCorruptSaveResult, null);
		}
	}

	private void CorruptSaveResponceNo(T17DialogBox dialog)
	{
		SetManagerState(SaveManagerStatus.PendingSaveGame);
	}

	public void ShowDialog(string strTitle, string strMessage, T17DialogBox.DialogEvent eventYes, T17DialogBox.DialogEvent eventNo, T17DialogBox.Symbols symbol, bool bLocalizeDialogueTitle, bool bLocalizeDialogueBody)
	{
		if (m_CurrentSlotSelectionMenu != null)
		{
			m_CurrentSlotSelectionMenu.gameObject.SetActive(value: false);
		}
		T17DialogBox diag = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (diag != null)
		{
			T17DialogBox t17DialogBox = diag;
			bool hasConfirm = true;
			bool hasDecline = true;
			bool hasCancel = false;
			string confirmBtn = "Text.Dialog.Prompt.Yes";
			string declineBtn = "Text.Dialog.Prompt.No";
			bool bLocalizeMessage = bLocalizeDialogueBody;
			bool bLocalizeTitle = bLocalizeDialogueTitle;
			t17DialogBox.Initialize(hasConfirm, hasDecline, hasCancel, strTitle, strMessage, confirmBtn, declineBtn, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle, bLocalizeMessage);
			diag.SetSymbol(symbol);
			T17DialogBox t17DialogBox2 = diag;
			t17DialogBox2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(t17DialogBox2.OnConfirm, (T17DialogBox.DialogEvent)delegate(T17DialogBox sender)
			{
				if (m_CurrentSlotSelectionMenu != null)
				{
					m_CurrentSlotSelectionMenu.gameObject.SetActive(value: true);
				}
				diag.SetSelectedGameobjectToPrepopup();
				eventYes(sender);
			});
			T17DialogBox t17DialogBox3 = diag;
			t17DialogBox3.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(t17DialogBox3.OnDecline, (T17DialogBox.DialogEvent)delegate(T17DialogBox sender)
			{
				if (m_CurrentSlotSelectionMenu != null)
				{
					m_CurrentSlotSelectionMenu.gameObject.SetActive(value: true);
				}
				diag.SetSelectedGameobjectToPrepopup();
				eventNo(sender);
			});
			diag.Show();
		}
		else if (eventNo != null)
		{
			eventNo(diag);
		}
	}

	public void ShowDialogOnlyOK(string strTitle, string strMessage, T17DialogBox.DialogEvent eventOK, T17DialogBox.Symbols symbol, bool bLocalizeDialogueTitle, bool bLocalizeDialogueBody)
	{
		if (m_CurrentSlotSelectionMenu != null)
		{
			m_CurrentSlotSelectionMenu.gameObject.SetActive(value: false);
		}
		T17DialogBox diag = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (!(diag != null))
		{
			return;
		}
		T17DialogBox t17DialogBox = diag;
		bool hasConfirm = true;
		bool hasDecline = false;
		bool hasCancel = false;
		string confirmBtn = "Text.Dialog.Prompt.Ok";
		string empty = string.Empty;
		bool bLocalizeMessage = bLocalizeDialogueBody;
		bool bLocalizeTitle = bLocalizeDialogueTitle;
		t17DialogBox.Initialize(hasConfirm, hasDecline, hasCancel, strTitle, strMessage, confirmBtn, empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle, bLocalizeMessage);
		diag.SetSymbol(symbol);
		T17DialogBox t17DialogBox2 = diag;
		t17DialogBox2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(t17DialogBox2.OnConfirm, (T17DialogBox.DialogEvent)delegate(T17DialogBox sender)
		{
			if (m_CurrentSlotSelectionMenu != null)
			{
				m_CurrentSlotSelectionMenu.gameObject.SetActive(value: true);
			}
			diag.SetSelectedGameobjectToPrepopup();
			eventOK(sender);
		});
		diag.Show();
	}

	private void ShowMessageDialog(string strTitle, string strMessage, T17DialogBox.DialogEvent eventContinue, T17DialogBox.Symbols symbol, bool bLocalizeTitle = true, bool bLocalizeMessagae = true)
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, strTitle, strMessage, "Text.Menu.Continue", string.Empty, string.Empty, symbol, bLocalizeTitle, bLocalizeMessagae);
			dialog.SetSymbol(symbol);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, eventContinue);
			dialog.Show();
		}
		else
		{
			eventContinue?.Invoke(dialog);
		}
	}

	private void CloseSlotSelection()
	{
		if (m_CurrentSlotSelectionMenu != null)
		{
			m_CurrentSlotSelectionMenu.Close();
		}
	}

	private void OpenSlotSelection()
	{
		SaveUIMode eUIMode = m_eUIMode;
		if (eUIMode == SaveUIMode.LoadGame || eUIMode == SaveUIMode.NewGame)
		{
			if (FrontEndFlow.Instance.GetCurrentMenuType() == FrontEndFlow.MenuType.GameFrontend)
			{
				FrontEndFlow.Instance.OpenChildOnTopOfMenu(1);
			}
			else if (FrontEndFlow.Instance.GetCurrentMenuType() == FrontEndFlow.MenuType.LevelEditor)
			{
				FrontEndFlow.Instance.OpenChildOnTopOfMenu(1);
			}
		}
	}

	private void SaveTheGame()
	{
		SetManagerState(SaveManagerStatus.SaveGame);
		object platformData = null;
		Debug.Log("---Asked to save the game - " + GetPrisonName());
		string objectData;
		if (string.IsNullOrEmpty(m_strCurrentSaveGame))
		{
			objectData = PrisonSnapshotIO.CreatePrisonSnapshot();
		}
		else
		{
			objectData = m_strCurrentSaveGame;
			m_strCurrentSaveGame = string.Empty;
		}
		m_PlatformIO.RequestSaveFile(PlatformIO.IORequest.IORequestPriority.High, m_PrisonData.m_Prisons[m_iCurrentPrison].GetPrisonDirectoryName(), m_PrisonData.m_Prisons[m_iCurrentPrison].GetSlotFileName(m_iSlot_Selected), objectData, m_FileSaveTimeout, RequestSaveFileResult, platformData);
	}

	private bool IsTheGameReadyToBeSaved()
	{
		if (!ValidateIndexes("IsTheGameReadyToBeSaved", m_iCurrentPrison, m_iSlot_Selected))
		{
			return false;
		}
		if (IsThereASavedGame() || (GlobalStart.GetInstance() != null && GlobalStart.GetInstance().IsWithinLevel()))
		{
			return true;
		}
		return false;
	}

	private bool IsThePlayerInTheGame()
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && instance != null && instance.IsWithinLevel())
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null && primaryGamer.m_eCharacterSelectionStage >= Gamer.CharacterSelectionStage.EnabledInGame && !CutsceneManagerBase.IsACutscenePlaying())
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldWeAbortSave()
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null)
		{
			GlobalStart.GLOBALSTART_MODE currentGlobalStartMode = instance.CurrentGlobalStartMode;
			if (currentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.START_LOAD_FRONTEND || currentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.RELOAD_FRONTEND)
			{
				return true;
			}
		}
		return false;
	}

	private void SetEverythingChanged()
	{
		for (int num = m_PrisonData.m_Prisons.Count - 1; num >= 0; num--)
		{
			PrisonsSaveInformation.PrisonData prisonData = m_PrisonData.m_Prisons[num];
			for (int num2 = 9; num2 >= 0; num2--)
			{
				prisonData.m_Slots[num2].m_bSomethingChanged = true;
			}
		}
	}

	public void SetCurrentPrison(string strPrison, PrisonsSaveInformation.PrisonData.PrisonType iPrisonID)
	{
		if (!(m_PlatformIO == null) && m_PrisonData != null && strPrison != null)
		{
			int num = m_PrisonData.GetPrisonIndex(strPrison, iPrisonID);
			if (num == -1)
			{
				num = m_PrisonData.CreatePrison(strPrison, iPrisonID);
			}
			m_iCurrentPrison = num;
		}
	}

	public void SaveGame(OnGameSavedHandler saveCallback, bool bNow = true, bool bRequestIsARetryFromWithin = false)
	{
		m_OnGameSaved = saveCallback;
		if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().HasActiveConfig() && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
		{
			OnSaveGame(bSaved: false);
			return;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null && instance != null && instance.gameType == PrisonConfig.ConfigType.Singleplayer)
		{
			OnSaveGame(bSaved: false);
		}
		else if (IsTheGameReadyToBeSaved() && IsThePlayerInTheGame())
		{
			if (string.IsNullOrEmpty(m_strCurrentSaveGame))
			{
				m_strCurrentSaveGame = PrisonSnapshotIO.CreatePrisonSnapshot();
			}
			if (bNow && CanWeSaveNow(bRequestIsARetryFromWithin))
			{
				SaveTheGame();
				MatchingGames.GetInstance().SaveGame();
			}
		}
		else
		{
			OnSaveGame(bSaved: false);
		}
	}

	public bool AreThereAnySaves()
	{
		if (!ValidateIndexes("AreThereAnySaves", m_iCurrentPrison, 0))
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[i].m_Status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
				return true;
			}
		}
		return false;
	}

	public bool AreThereAnySavesAllPrisons()
	{
		int i = 0;
		for (int count = m_PrisonData.m_Prisons.Count; i < count; i++)
		{
			if (!ValidateIndexes("AreThereAnySaves", i, 0))
			{
				continue;
			}
			for (int j = 0; j < 10; j++)
			{
				switch (m_PrisonData.m_Prisons[i].m_Slots[j].m_Status)
				{
				case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
					return true;
				case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
					return true;
				case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
					return true;
				}
			}
		}
		return false;
	}

	public bool CanWeContinue()
	{
		if (!ValidateIndexes("CanWeContinue", m_iCurrentPrison, 0))
		{
			return false;
		}
		int mostRecentSave = m_PrisonData.m_Prisons[m_iCurrentPrison].GetMostRecentSave();
		if (mostRecentSave != -1)
		{
			switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[mostRecentSave].m_Status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot = -1;
				break;
			}
		}
		return false;
	}

	public bool ShouldSaveSlotBeEnabled(int iSlotNumber)
	{
		if (!ValidateIndexes("CanSlotBeUsed", m_iCurrentPrison, iSlotNumber))
		{
			return false;
		}
		PrisonsSaveInformation.PrisonData.SlotData.SlotStatus status = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_Status;
		switch (m_eUIMode)
		{
		case SaveUIMode.None:
			return false;
		case SaveUIMode.GuestSave:
			switch (status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				return false;
			}
			break;
		case SaveUIMode.LoadGame:
		case SaveUIMode.ContinueGame:
			switch (status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				return true;
			}
			break;
		case SaveUIMode.NewGame:
			return true;
		}
		return false;
	}

	public bool CanSlotBeUsed(int iSlotNumber)
	{
		if (!ValidateIndexes("CanSlotBeUsed", m_iCurrentPrison, iSlotNumber))
		{
			return false;
		}
		PrisonsSaveInformation.PrisonData.SlotData.SlotStatus status = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_Status;
		switch (m_eUIMode)
		{
		case SaveUIMode.None:
			return true;
		case SaveUIMode.GuestSave:
			switch (status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				return false;
			}
			break;
		case SaveUIMode.LoadGame:
		case SaveUIMode.ContinueGame:
			switch (status)
			{
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
				return true;
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
			case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
				return false;
			}
			break;
		case SaveUIMode.NewGame:
			return true;
		}
		return false;
	}

	public bool IsThereASaveInSlot(int iSlotNumber)
	{
		if (!ValidateIndexes("IsThereASaveInSlot", m_iCurrentPrison, iSlotNumber))
		{
			return false;
		}
		switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_Status)
		{
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
			return true;
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
			return false;
		default:
			return false;
		}
	}

	public void GetSlotInfo(int iSlotNumber, out string strTitle, out bool bTitleLocalized, out string strExtra, out bool bExtraLocalized, out Sprite icon)
	{
		strTitle = "No Prison";
		bTitleLocalized = false;
		strExtra = "Text.BLANK";
		bExtraLocalized = false;
		icon = null;
		if (m_iCurrentPrison == -1)
		{
			return;
		}
		if (iSlotNumber < 0 || iSlotNumber >= 10)
		{
			strTitle = "Bad slot no.";
			strExtra = iSlotNumber.ToString();
			return;
		}
		PrisonsSaveInformation.PrisonData.SlotData.SlotStatus status = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_Status;
		if (string.IsNullOrEmpty(m_SlotStatusStrings[(int)status]))
		{
			Localization.Get("Text.SlotStatus." + status, out m_SlotStatusStrings[(int)status]);
		}
		switch (status)
		{
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Empty:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Corrupt:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.INVALID:
			strTitle = m_SlotStatusStrings[(int)status];
			bTitleLocalized = true;
			strExtra = "Text.BLANK";
			bExtraLocalized = false;
			break;
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.Used:
		case PrisonsSaveInformation.PrisonData.SlotData.SlotStatus.OldVersion:
		{
			strTitle = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_strDate;
			bTitleLocalized = true;
			int iDays = m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_iDays;
			strExtra = m_SlotStatusStrings[(int)status].Replace("$dayNumber", iDays.ToString());
			bExtraLocalized = true;
			switch (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotNumber].m_GameRoomType)
			{
			case T17NetRoomGameView.GameRoomType.Offline:
				icon = m_OfflineSprite;
				break;
			case T17NetRoomGameView.GameRoomType.Private:
				icon = m_PrivateSprite;
				break;
			case T17NetRoomGameView.GameRoomType.Public:
				icon = m_PublicSprite;
				break;
			}
			break;
		}
		}
	}

	public SaveManagerStatus GetSaveManagerState()
	{
		return m_eManagerState;
	}

	public void SetSlotSelected(int iIndex)
	{
		if (m_eManagerState != SaveManagerStatus.Idle && m_eManagerState != SaveManagerStatus.PendingSaveGame)
		{
			return;
		}
		SetManagerState(SaveManagerStatus.Idle, bProcess: false);
		switch (iIndex)
		{
		}
		switch (m_eUIMode)
		{
		case SaveUIMode.GuestSave:
			GuestSaveSlotSelected(iIndex);
			break;
		case SaveUIMode.LoadGame:
			LoadGameSlotSelected(iIndex);
			break;
		case SaveUIMode.NewGame:
			NewGameSlotSelected(iIndex);
			break;
		case SaveUIMode.ContinueGame:
			break;
		}
	}

	public void NewGameSelected(bool prisonTutorial)
	{
		if (!prisonTutorial)
		{
			SetUIMode(SaveUIMode.NewGame);
			OpenSlotSelection();
		}
		else
		{
			SetManagerState(SaveManagerStatus.StartGameTutorial);
		}
	}

	public void LoadGameSelected()
	{
		SetUIMode(SaveUIMode.LoadGame);
		OpenSlotSelection();
	}

	public void GuestSaveSelected()
	{
		SetUIMode(SaveUIMode.GuestSave);
	}

	public void ContinueSelected()
	{
		if (CanWeContinue())
		{
			SetUIMode(SaveUIMode.ContinueGame);
			LoadGameSlotSelected(m_PrisonData.m_Prisons[m_iCurrentPrison].m_iContinueSlot);
		}
	}

	public void DeleteSelected()
	{
		DeleteGameSlotSelected();
	}

	public void SetSlotSelectionMenu(SlotSelectionMenu slotMenu)
	{
		m_CurrentSlotSelectionMenu = slotMenu;
	}

	public string GetPrisonName(bool bSanitized = false, bool bGetCustomPathName = false)
	{
		if (m_iCurrentPrison == -1 || m_PlatformIO == null)
		{
			return "NO PRISON";
		}
		if (bSanitized)
		{
			return m_PlatformIO.GetSanitisedString(m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonName);
		}
		if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_PrisonID != 0)
		{
			if (bGetCustomPathName)
			{
				return m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonName;
			}
			return m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonTitle;
		}
		return m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonName;
	}

	public PrisonsSaveInformation.PrisonData.PrisonType GetPrisonID()
	{
		if (m_iCurrentPrison == -1 || m_PlatformIO == null)
		{
			return PrisonsSaveInformation.PrisonData.PrisonType.eDefault;
		}
		return m_PrisonData.m_Prisons[m_iCurrentPrison].m_PrisonID;
	}

	public string GetPurposeText()
	{
		string text = string.Empty;
		switch (m_eUIMode)
		{
		case SaveUIMode.ContinueGame:
			text = "ERR:ContinueGame";
			break;
		case SaveUIMode.GuestSave:
			text = "Text.SS.Purpose.GuestSave";
			break;
		case SaveUIMode.LoadGame:
			text = "Text.SS.Purpose.LoadGame";
			break;
		case SaveUIMode.NewGame:
			text = "Text.SS.Purpose.NewGame";
			break;
		case SaveUIMode.None:
			text = "ERR:None";
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "INVALID - " + m_eUIMode;
		}
		return text;
	}

	public bool IsThereASavedGame()
	{
		return !string.IsNullOrEmpty(m_strCurrentSaveGame);
	}

	public bool IsThereALoadedGame()
	{
		return !string.IsNullOrEmpty(m_strCurrentSaveGame);
	}

	public string GetLoadedGame()
	{
		string strCurrentLoadedGame = m_strCurrentLoadedGame;
		m_strCurrentLoadedGame = string.Empty;
		return strCurrentLoadedGame;
	}

	public bool CanWeSaveNow(bool bRequestIsARetryFromWithin)
	{
		if (!ValidateIndexes("CanWeSaveNow", m_iCurrentPrison, m_iSlot_Selected))
		{
			return false;
		}
		if (!bRequestIsARetryFromWithin)
		{
			return m_eManagerState == SaveManagerStatus.Idle;
		}
		return m_eManagerState == SaveManagerStatus.SaveGame;
	}

	public bool HasSomethingChanged(int iSlotIndex)
	{
		if (!ValidateIndexes("HasSomethingChanged", m_iCurrentPrison, iSlotIndex))
		{
			return true;
		}
		if (m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotIndex].m_bSomethingChanged)
		{
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_Slots[iSlotIndex].m_bSomethingChanged = false;
			return true;
		}
		return false;
	}

	public string GetTodaysDateAndTime()
	{
		string text = ((!m_bDayBeforeMonth) ? "M/d/yy H:mm" : "d/M/yy H:mm");
		return DateTime.Now.ToString(text);
	}

	public static void NotifyNewUserLoggedIn(UserLoggedIn callBack)
	{
		c_UserLoggedInCallBack = callBack;
	}

	private void OnSaveGame(bool bSaved)
	{
		if (m_OnGameSaved != null)
		{
			m_OnGameSaved(bSaved);
			m_OnGameSaved = null;
		}
	}

	private void OnPlayerReadyToPlay(int iPhotonId)
	{
		if (NetLoadSync.AllClientsReadyToPlay())
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (m_CurrentSaveMode != SaveMode.Manual || (primaryGamer != null && iPhotonId == primaryGamer.m_PhotonID))
			{
				SaveGame(m_OnGameSaved);
			}
		}
	}

	private void OnPlayerDeleteRequested(Gamer gamer)
	{
		if (GlobalStart.GetInstance().IsWithinLevel() && gamer != null && !gamer.IsLocal() && m_CurrentSaveMode != SaveMode.Manual)
		{
			SaveGame(m_OnGameSaved);
		}
	}

	private void DoSaveSpaceCheck()
	{
		RequestSaveSpaceDeleteResultThenStartGame(PlatformIO.IOResultEnum.IOSuccessful, 0);
	}

	private void RequestSaveSpaceResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
	}

	private void RequestSaveSpaceDeleteResultThenRecheckSaveSpace(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult == PlatformIO.IOResultEnum.IOSuccessful)
		{
			DoSaveSpaceCheck();
		}
		else
		{
			Debug.Log("*_*_* We failed to deletethe save space checker file. " + eResult);
		}
	}

	private void RequestSaveSpaceDeleteResultThenStartGame(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		Debug.Log("     RequestSaveSpaceDeleteResultThenStartGame   " + eResult);
		if (eResult != 0)
		{
			Debug.Log("*_*_* We failed to deletethe save space checker file. " + eResult);
		}
		CloseSlotSelection();
		SetManagerState(SaveManagerStatus.StartGame);
	}

	private void SpaceCheckResponceYes(T17DialogBox dialog)
	{
		if (ValidateIndexes("SpaceCheckResponceYes", m_iCurrentPrison, m_iSlot_Selected))
		{
			SetManagerState(SaveManagerStatus.Idle);
		}
	}

	public SaveManagerStatus GetStatus()
	{
		return m_eManagerState;
	}

	public bool SaveUserLevel(string strDirectory, ref List<byte> saveData, bool bIsFinishedVersion = false)
	{
		SetCurrentPrison(strDirectory, PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
		if (m_iCurrentPrison == -1)
		{
			return false;
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonTitle = instance.GetLevelName();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_strPrisonDescription = instance.GetLevelDecription();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_ePrisonDifficultyLevel = instance.GetLevelDifficulty();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_NumPrisonRoles[0] = instance.GetNumberOfInmates();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_NumPrisonRoles[1] = instance.GetNumberOfGuards();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_OutfitType = instance.GetOutfitType();
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_bHasFinished = bIsFinishedVersion;
			m_PrisonData.m_Prisons[m_iCurrentPrison].m_EditorVersion = LevelDetailsManager.c_CurrentLevelDataVersionNumber;
		}
		if (PlatformIO.GetInstance() == null)
		{
			return false;
		}
		string ownerName = GetOwnerName(m_iCurrentPrison);
		if (string.IsNullOrEmpty(ownerName))
		{
			return false;
		}
		ownerName = PlatformIO.GetInstance().GetPath(ownerName);
		if (!Directory.Exists(ownerName))
		{
			Directory.CreateDirectory(ownerName);
		}
		if (bIsFinishedVersion)
		{
			File.WriteAllBytes(ownerName + "Level_Finished.dat", saveData.ToArray());
		}
		else
		{
			if (File.Exists(ownerName + "Level_Finished.dat"))
			{
				File.Delete(ownerName + "Level_Finished.dat");
			}
			File.WriteAllBytes(ownerName + "Level.dat", saveData.ToArray());
		}
		m_PrisonData.m_Prisons[m_iCurrentPrison].m_bNeedsSaving = true;
		return true;
	}

	public bool LoadTheLevel(string strDirectory, ref List<byte> saveData)
	{
		saveData.Clear();
		SetCurrentPrison(strDirectory, PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
		if (m_iCurrentPrison == -1)
		{
			return false;
		}
		if (PlatformIO.GetInstance() == null)
		{
			return false;
		}
		string ownerName = GetOwnerName(m_iCurrentPrison);
		if (string.IsNullOrEmpty(ownerName))
		{
			return false;
		}
		ownerName = PlatformIO.GetInstance().GetPath(ownerName);
		if (!File.Exists(ownerName + "Level.dat"))
		{
			return false;
		}
		saveData.AddRange(File.ReadAllBytes(ownerName + "Level.dat"));
		return true;
	}

	public void EnumerateCustomPrisons(ref List<PrisonsSaveInformation.PrisonData> customPrisons, bool bUGC = false)
	{
		if (m_PrisonData == null || customPrisons == null)
		{
			return;
		}
		PrisonsSaveInformation.PrisonData.PrisonType prisonType = ((!bUGC) ? PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel : PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
		for (int i = 0; i < m_PrisonData.m_Prisons.Count; i++)
		{
			PrisonsSaveInformation.PrisonData prisonData = m_PrisonData.m_Prisons[i];
			if (prisonData != null && m_PrisonData.m_Prisons[i].m_PrisonID == prisonType && (prisonType != PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel || m_PrisonData.m_Prisons[i].m_bUGCFound))
			{
				customPrisons.Add(prisonData);
			}
		}
	}

	public void OnUserGeneratedContentUpdated()
	{
		Platform instance = Platform.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		List<Platform.UGCItem> ugcList = new List<Platform.UGCItem>();
		instance.EnumerateUGCItems(ref ugcList, Platform.UGCType.eCustomLevel);
		for (int i = 0; i < ugcList.Count; i++)
		{
			Platform.UGCItem uGCItem = ugcList[i];
			if (uGCItem == null)
			{
				continue;
			}
			int num = m_PrisonData.GetPrisonIndex("ESC2UGC" + uGCItem.m_ID, PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
			if (num == -1)
			{
				num = m_PrisonData.CreatePrison(uGCItem.m_ID.ToString(), PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
			}
			if (num == -1)
			{
				continue;
			}
			string path = uGCItem.m_AbsolutePath + PlatformIO.GetInstance().GetSeperationCharacter() + "Level_Finished.dat";
			if (File.Exists(path))
			{
				m_PrisonData.m_Prisons[num].m_bNeedsSaving = false;
				try
				{
					List<byte> LevelData = new List<byte>();
					LevelData.AddRange(File.ReadAllBytes(path));
					LevelDetailsManager.FrontendLevelDetails frontendDataForLevel = LevelDetailsManager.GetFrontendDataForLevel(ref LevelData);
					m_PrisonData.m_Prisons[num].m_strPrisonTitle = frontendDataForLevel.m_LevelName;
					m_PrisonData.m_Prisons[num].m_strPrisonDescription = frontendDataForLevel.m_LevelDescription;
					m_PrisonData.m_Prisons[num].m_ePrisonDifficultyLevel = frontendDataForLevel.m_DifficultyLevel;
					m_PrisonData.m_Prisons[num].m_NumPrisonRoles[0] = frontendDataForLevel.m_NumberOfInmates;
					m_PrisonData.m_Prisons[num].m_NumPrisonRoles[1] = frontendDataForLevel.m_NumberOfGuards;
					m_PrisonData.m_Prisons[num].m_OutfitType = frontendDataForLevel.m_OutfitType;
					m_PrisonData.m_Prisons[num].m_bUGCFound = true;
					m_PrisonData.m_Prisons[num].m_bHasFinished = true;
					m_PrisonData.m_Prisons[num].m_EditorVersion = frontendDataForLevel.m_DataVersion;
				}
				catch (Exception)
				{
				}
			}
		}
	}

	public string GetCustomLevelFilePath(string strPrisonName, bool bWithoutFinal = false)
	{
		int prisonIndex = m_PrisonData.GetPrisonIndex(strPrisonName, PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
		if (prisonIndex != -1)
		{
			List<Platform.UGCItem> ugcList = new List<Platform.UGCItem>();
			Platform.GetInstance().EnumerateUGCItems(ref ugcList, Platform.UGCType.eCustomLevel);
			Platform.UGCItem uGCItem = null;
			ulong ugcID = ulong.Parse(m_PrisonData.m_Prisons[prisonIndex].m_strPrisonName);
			uGCItem = ugcList.Find((Platform.UGCItem x) => x.m_ID == ugcID);
			if (uGCItem != null)
			{
				if (bWithoutFinal)
				{
					return uGCItem.m_AbsolutePath + PlatformIO.GetInstance().GetSeperationCharacter() + "Level.dat";
				}
				return uGCItem.m_AbsolutePath + PlatformIO.GetInstance().GetSeperationCharacter() + "Level_Finished.dat";
			}
		}
		prisonIndex = m_PrisonData.GetPrisonIndex(strPrisonName, PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
		if (prisonIndex != -1)
		{
			if (bWithoutFinal)
			{
				return PlatformIO.GetInstance().GetPath(strPrisonName) + "Level.dat";
			}
			return PlatformIO.GetInstance().GetPath(strPrisonName) + "Level_Finished.dat";
		}
		return string.Empty;
	}

	public bool IsCustomPrisonPlayable(string strPrisonName)
	{
		int prisonIndex = m_PrisonData.GetPrisonIndex(strPrisonName, PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
		if (prisonIndex != -1)
		{
			return m_PrisonData.m_Prisons[prisonIndex].m_bHasFinished;
		}
		return false;
	}

	public int GetCustomPrisonCount()
	{
		int num = 1;
		int count = m_PrisonData.m_Prisons.Count;
		for (int i = 0; i < count; i++)
		{
			PrisonsSaveInformation.PrisonData prisonData = m_PrisonData.m_Prisons[i];
			if (prisonData != null && prisonData.m_PrisonID == PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel)
			{
				num++;
			}
		}
		return num;
	}

	public bool DeletePrison(string strPrisonName, PrisonsSaveInformation.PrisonData.PrisonType prisonID)
	{
		if (string.IsNullOrEmpty(strPrisonName) || prisonID != PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel || m_eManagerState != SaveManagerStatus.Idle || PlatformIO.GetInstance() == null)
		{
			return false;
		}
		m_PrisonData.DeletePrison(strPrisonName, prisonID);
		SetManagerState(SaveManagerStatus.SaveDirectory);
		PlatformIO.GetInstance().RequestDeleteDirectory(PlatformIO.IORequest.IORequestPriority.Next, strPrisonName, m_FileDeleteTimeout, null, null);
		return true;
	}

	public void CycleSaveMode()
	{
		GlobalStart instance = GlobalStart.GetInstance();
		bool flag = false;
		bool flag2 = instance != null && instance.CurrentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.IN_LEVEL;
		SaveMode userSelectedSaveMode = m_UserSelectedSaveMode;
		switch ((!flag2) ? m_UserSelectedSaveMode : m_CurrentSaveMode)
		{
		case SaveMode.Automatic:
			m_UserSelectedSaveMode = SaveMode.Manual;
			break;
		case SaveMode.Manual:
			m_UserSelectedSaveMode = SaveMode.Automatic;
			break;
		}
		flag = userSelectedSaveMode != m_UserSelectedSaveMode;
		if (flag2)
		{
			CurrentSaveMode = m_UserSelectedSaveMode;
		}
		else if (flag && OnSaveModeChanged != null)
		{
			OnSaveModeChanged(m_UserSelectedSaveMode);
		}
		if (flag)
		{
			GlobalSave instance2 = GlobalSave.GetInstance();
			if (instance2 != null)
			{
				instance2.Set("SaveManager:UserSaveMode", (int)m_UserSelectedSaveMode);
			}
		}
	}
}

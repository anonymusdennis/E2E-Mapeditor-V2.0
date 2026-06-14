using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class GlobalSave : MonoBehaviour
{
	[Serializable]
	private class Entry
	{
		public string m_JSON;
	}

	[Serializable]
	private class EntInt
	{
		public int m_Value;

		public EntInt(int val)
		{
			m_Value = val;
		}
	}

	[Serializable]
	private class EntFloat
	{
		public float m_Value;

		public EntFloat(float val)
		{
			m_Value = val;
		}
	}

	[Serializable]
	private class EntString
	{
		public string m_Value;

		public EntString(string val)
		{
			m_Value = val;
		}
	}

	[Serializable]
	private class EntBool
	{
		public bool m_Value;

		public EntBool(bool val)
		{
			m_Value = val;
		}
	}

	[Serializable]
	private class EntInt64Array
	{
		public long[] m_Value;

		public EntInt64Array(long[] val)
		{
			m_Value = val;
		}
	}

	private class EntIntArray
	{
		public int[] m_Value;

		public EntIntArray(int[] val)
		{
			m_Value = val;
		}
	}

	[Serializable]
	private class GlobalData
	{
		public int m_Version = 2;

		public string m_What = "Esc2GSGD";

		public string[] m_Keys;

		public Entry[] m_Entries;
	}

	public enum GlobalSaveResult
	{
		GSR_NotReady,
		GSR_PendingSave,
		GSR_PendingLoad,
		GSR_Success,
		GSR_Failure,
		GSR_FailureNoSpace
	}

	private const int GLOBALSAVE_VERSION = 2;

	private const string GLOBALSAVE_NAME = "GlobalSave.json";

	private const string GLOBALDIR_NAME = "Global";

	private bool m_bUserReady;

	private bool m_bSavePending;

	private bool m_bLoadPending;

	private bool m_bDeletePending;

	private Dictionary<string, Entry> m_Data = new Dictionary<string, Entry>();

	private GlobalSaveResult m_Result;

	private GlobalData m_GlobalData = new GlobalData();

	private static GlobalSave m_TheInstance;

	public static GlobalSave GetInstance()
	{
		return m_TheInstance;
	}

	private void Awake()
	{
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_TheInstance == this)
		{
			m_TheInstance = null;
		}
	}

	public void LateUpdate()
	{
		if (m_bUserReady)
		{
			if (m_bSavePending)
			{
				DoSave();
			}
			else if (m_bLoadPending)
			{
				DoLoad();
			}
		}
	}

	public void NewUser()
	{
		m_bUserReady = true;
		m_bLoadPending = true;
	}

	public void RequestSave()
	{
		m_bSavePending = true;
	}

	public bool IsBusy(bool bSaveOnly = false)
	{
		if (bSaveOnly)
		{
			return m_bSavePending || m_Result == GlobalSaveResult.GSR_PendingSave;
		}
		return m_bSavePending || m_bLoadPending || m_bDeletePending || m_Result == GlobalSaveResult.GSR_PendingSave || m_Result == GlobalSaveResult.GSR_PendingLoad;
	}

	private void ShowDialog(string strTitle, string strMessage, T17DialogBox.DialogEvent eventYes, T17DialogBox.DialogEvent eventNo, T17DialogBox.Symbols symbol)
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, strTitle, strMessage, "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty, symbol);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, eventYes);
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, eventNo);
			dialog.Show();
		}
		else
		{
			eventNo?.Invoke(dialog);
		}
	}

	private void DoSave()
	{
		string objectData = ConvertDataToSave();
		if (PlatformIO.GetInstance() != null)
		{
			object platformData = null;
			m_bSavePending = false;
			m_Result = GlobalSaveResult.GSR_PendingSave;
			PlatformIO.GetInstance().RequestSaveFile(PlatformIO.IORequest.IORequestPriority.High, "Global", "GlobalSave.json", objectData, 4f, RequestSaveResult, platformData);
		}
	}

	private void RequestSaveResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
			m_Result = GlobalSaveResult.GSR_Success;
			break;
		case PlatformIO.IOResultEnum.IOFailedNotEnoughRoom:
			m_bSavePending = true;
			break;
		case PlatformIO.IOResultEnum.IOFailed:
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
		case PlatformIO.IOResultEnum.IOCanceled:
			m_Result = GlobalSaveResult.GSR_Failure;
			break;
		}
		if (eResult == PlatformIO.IOResultEnum.IOFailedCorrupt && m_Result == GlobalSaveResult.GSR_Failure)
		{
			PromptRemakeOfGlobalSave(wasTryingToLoad: false, string.Empty);
		}
	}

	private void DoLoad()
	{
		if (PlatformIO.GetInstance() != null)
		{
			m_bLoadPending = false;
			m_Result = GlobalSaveResult.GSR_PendingLoad;
			PlatformIO.GetInstance().RequestLoadFile(PlatformIO.IORequest.IORequestPriority.High, "Global", "GlobalSave.json", 4f, RequestLoadResult, null);
		}
	}

	private void RequestLoadResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		bool flag = false;
		bool flag2 = false;
		string strErrorCode = string.Empty;
		switch (eResult)
		{
		case PlatformIO.IOResultEnum.IOSuccessful:
		{
			string text = PlatformIO.GetInstance().GetCache() as string;
			if (!string.IsNullOrEmpty(text))
			{
				if (ConvertDataFromSave(text))
				{
					m_Result = GlobalSaveResult.GSR_Success;
					if (ReInput.userDataStore != null)
					{
						ReInput.userDataStore.Load();
					}
				}
				else
				{
					strErrorCode = "0210: ";
					flag = true;
				}
			}
			else
			{
				flag2 = true;
			}
			break;
		}
		case PlatformIO.IOResultEnum.IOCanceled:
			m_bLoadPending = true;
			break;
		case PlatformIO.IOResultEnum.IOFailedCorrupt:
			strErrorCode = "0211: ";
			flag = true;
			break;
		case PlatformIO.IOResultEnum.IOFailedDoesNotExist:
			flag2 = true;
			break;
		case PlatformIO.IOResultEnum.IOFailedInvalidRequest:
			strErrorCode = "0213: ";
			flag = true;
			break;
		case PlatformIO.IOResultEnum.IOFailedTimeOut:
			m_bLoadPending = true;
			break;
		case PlatformIO.IOResultEnum.IOFailed:
			strErrorCode = "0212: ";
			flag = true;
			break;
		}
		if (flag || flag2)
		{
			if (flag)
			{
				PromptRemakeOfGlobalSave(wasTryingToLoad: true, strErrorCode);
				return;
			}
			InitNewSave();
			m_bSavePending = true;
		}
	}

	private void PromptRemakeOfGlobalSave(bool wasTryingToLoad, string strErrorCode)
	{
		m_bDeletePending = true;
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			if (string.IsNullOrEmpty(strErrorCode))
			{
				dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.SS.CorruptDir.Title", "Text.SS.CorruptDir.Body", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			}
			else
			{
				Localization.Get("Text.SS.CorruptDir.Body", out var localized);
				dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.SS.CorruptDir.Title", strErrorCode + localized, "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: false);
			}
			dialog.SetSymbol(T17DialogBox.Symbols.Error);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, (T17DialogBox.DialogEvent)delegate
			{
				DeleteAndRemakeGlobalSave();
			});
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, (T17DialogBox.DialogEvent)delegate
			{
				m_bDeletePending = false;
				if (wasTryingToLoad)
				{
					DoLoad();
				}
				else
				{
					DoSave();
				}
			});
			dialog.Show();
		}
		else
		{
			DeleteAndRemakeGlobalSave();
		}
	}

	private void DeleteAndRemakeGlobalSave()
	{
		m_bDeletePending = true;
		PlatformIO.GetInstance().RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.High, "Global", "GlobalSave.json", 4f, RequestDeleteResult, null);
	}

	private void RequestDeleteResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		m_bDeletePending = false;
		if (eResult == PlatformIO.IOResultEnum.IOSuccessful)
		{
			InitNewSave();
			m_bSavePending = true;
		}
	}

	private string ConvertDataToSave()
	{
		int num = 0;
		m_GlobalData.m_Entries = new Entry[m_Data.Keys.Count];
		m_GlobalData.m_Keys = new string[m_Data.Keys.Count];
		foreach (string key in m_Data.Keys)
		{
			m_GlobalData.m_Keys[num] = key;
			m_GlobalData.m_Entries[num] = m_Data[key];
			num++;
		}
		return JsonUtility.ToJson(m_GlobalData);
	}

	private bool ConvertDataFromSave(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		try
		{
			JsonUtility.FromJsonOverwrite(json, m_GlobalData);
		}
		catch
		{
			return false;
		}
		m_Data.Clear();
		int num = m_GlobalData.m_Keys.Length;
		for (int i = 0; i < num; i++)
		{
			m_Data[m_GlobalData.m_Keys[i]] = m_GlobalData.m_Entries[i];
		}
		return true;
	}

	private void InitNewSave()
	{
		Set("Audio:MusicVol", AudioController.GetCurrentMusicVolume());
		Set("Audio:SFXVol", AudioController.GetCurrentSFXVolume());
		Set("Settings:Vibration", (!Platform.controllerVibrationEnabled) ? 0f : 1f);
	}

	public void Set(string key, int value)
	{
		FindOrCreateKey(key, out var entry);
		EntInt obj = new EntInt(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out int value, int def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntInt entInt = JsonUtility.FromJson<EntInt>(entry.m_JSON);
				value = entInt.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	public void Set(string key, float value)
	{
		FindOrCreateKey(key, out var entry);
		EntFloat obj = new EntFloat(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out float value, float def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntFloat entFloat = JsonUtility.FromJson<EntFloat>(entry.m_JSON);
				value = entFloat.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	public void Set(string key, string value)
	{
		FindOrCreateKey(key, out var entry);
		EntString obj = new EntString(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out string value, string def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntString entString = JsonUtility.FromJson<EntString>(entry.m_JSON);
				value = entString.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	public void Set(string key, bool value)
	{
		FindOrCreateKey(key, out var entry);
		EntBool obj = new EntBool(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out bool value, bool def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntBool entBool = JsonUtility.FromJson<EntBool>(entry.m_JSON);
				value = entBool.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	public void Set(string key, long[] value)
	{
		FindOrCreateKey(key, out var entry);
		EntInt64Array obj = new EntInt64Array(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out long[] value, long[] def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntInt64Array entInt64Array = JsonUtility.FromJson<EntInt64Array>(entry.m_JSON);
				value = entInt64Array.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	public void Set(string key, int[] value)
	{
		FindOrCreateKey(key, out var entry);
		EntIntArray obj = new EntIntArray(value);
		entry.m_JSON = JsonUtility.ToJson(obj);
	}

	public bool Get(string key, out int[] value, int[] def)
	{
		FindKey(key, out var entry);
		if (entry != null)
		{
			try
			{
				EntIntArray entIntArray = JsonUtility.FromJson<EntIntArray>(entry.m_JSON);
				value = entIntArray.m_Value;
				return true;
			}
			catch
			{
				value = def;
				return false;
			}
		}
		value = def;
		return false;
	}

	private void FindKey(string key, out Entry entry)
	{
		if (m_Data.ContainsKey(key))
		{
			entry = m_Data[key];
		}
		else
		{
			entry = null;
		}
	}

	private void FindOrCreateKey(string key, out Entry entry)
	{
		if (!m_Data.ContainsKey(key))
		{
			m_Data[key] = new Entry();
		}
		entry = m_Data[key];
	}

	public void ResetSave()
	{
		m_GlobalData = new GlobalData();
		m_Data.Clear();
		m_bUserReady = false;
		m_bSavePending = false;
		m_bLoadPending = false;
		m_bDeletePending = false;
	}
}

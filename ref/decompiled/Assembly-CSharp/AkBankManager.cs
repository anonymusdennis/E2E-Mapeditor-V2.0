using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class AkBankManager
{
	private static Dictionary<string, AkBankHandle> m_BankHandles = new Dictionary<string, AkBankHandle>();

	public static List<AkBankHandle> BanksToUnload = new List<AkBankHandle>();

	private static Mutex m_Mutex = new Mutex();

	public static void DoUnloadBanks()
	{
		foreach (AkBankHandle item in BanksToUnload)
		{
			if (item.decodeBank && !item.saveDecodedBank)
			{
				AkSoundEngine.PrepareBank(PreparationType.Preparation_Unload, item.m_BankID);
			}
			else
			{
				AkSoundEngine.UnloadBank(item.m_BankID, IntPtr.Zero, null, null);
			}
		}
		BanksToUnload.Clear();
	}

	public static void Reset()
	{
		m_BankHandles.Clear();
		BanksToUnload.Clear();
	}

	public static void GlobalBankCallback(uint in_bankID, IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie)
	{
		m_Mutex.WaitOne();
		AkBankHandle akBankHandle = (AkBankHandle)in_Cookie;
		AkCallbackManager.BankCallback bankCallback = akBankHandle.bankCallback;
		if (in_eLoadResult != AKRESULT.AK_Success)
		{
			Debug.LogWarning("WwiseUnity: Bank " + akBankHandle.bankName + " failed to load (" + in_eLoadResult.ToString() + ")");
			m_BankHandles.Remove(akBankHandle.bankName);
		}
		m_Mutex.ReleaseMutex();
		bankCallback?.Invoke(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, in_memPoolId, null);
	}

	public static bool IsBankLoaded(string name)
	{
		m_Mutex.WaitOne();
		AkBankHandle value = null;
		bool result = m_BankHandles.TryGetValue(name, out value);
		m_Mutex.ReleaseMutex();
		return result;
	}

	public static void LoadBank(string name, bool decodeBank, bool saveDecodedBank)
	{
		m_Mutex.WaitOne();
		AkBankHandle value = null;
		if (!m_BankHandles.TryGetValue(name, out value))
		{
			value = new AkBankHandle(name, decodeBank, saveDecodedBank);
			m_BankHandles.Add(name, value);
			m_Mutex.ReleaseMutex();
			value.LoadBank();
		}
		else
		{
			value.IncRef();
			m_Mutex.ReleaseMutex();
		}
	}

	public static void LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null)
	{
		m_Mutex.WaitOne();
		AkBankHandle value = null;
		if (!m_BankHandles.TryGetValue(name, out value))
		{
			value = new AkBankHandle(name, decode: false, save: false);
			m_BankHandles.Add(name, value);
			m_Mutex.ReleaseMutex();
			value.LoadBankAsync(callback);
		}
		else
		{
			value.IncRef();
			m_Mutex.ReleaseMutex();
		}
	}

	public static void UnloadBank(string name)
	{
		m_Mutex.WaitOne();
		AkBankHandle value = null;
		if (m_BankHandles.TryGetValue(name, out value))
		{
			value.DecRef();
			if (value.RefCount == 0)
			{
				m_BankHandles.Remove(name);
			}
		}
		m_Mutex.ReleaseMutex();
	}
}

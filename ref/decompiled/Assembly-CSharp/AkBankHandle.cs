using System.IO;
using UnityEngine;

public class AkBankHandle
{
	private int m_RefCount;

	public uint m_BankID;

	public string relativeBasePath;

	public string bankName;

	public bool decodeBank;

	public bool saveDecodedBank;

	public AkCallbackManager.BankCallback bankCallback;

	public int RefCount => m_RefCount;

	public AkBankHandle(string name, bool decode, bool save)
	{
		bankName = name;
		bankCallback = null;
		decodeBank = decode;
		saveDecodedBank = save;
		if (!decodeBank)
		{
			return;
		}
		string path = Path.Combine(AkInitializer.GetDecodedBankFullPath(), bankName + ".bnk");
		string path2 = Path.Combine(AkBasePathGetter.GetValidBasePath(), bankName + ".bnk");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			if (File.GetLastWriteTime(path) > File.GetLastWriteTime(path2))
			{
				relativeBasePath = AkInitializer.GetDecodedBankFolder();
				decodeBank = false;
			}
		}
		catch
		{
		}
	}

	public void LoadBank()
	{
		if (m_RefCount == 0)
		{
			AKRESULT aKRESULT = AKRESULT.AK_Fail;
			if (AkBankManager.BanksToUnload.Contains(this))
			{
				AkBankManager.BanksToUnload.Remove(this);
				IncRef();
				return;
			}
			if (!decodeBank)
			{
				string text = null;
				if (!string.IsNullOrEmpty(relativeBasePath))
				{
					text = AkBasePathGetter.GetValidBasePath();
					if (string.IsNullOrEmpty(text))
					{
						Debug.LogWarning("WwiseUnity: Bank " + bankName + " failed to load (could not obtain base path to set).");
						return;
					}
					aKRESULT = AkSoundEngine.SetBasePath(Path.Combine(text, relativeBasePath));
				}
				else
				{
					aKRESULT = AKRESULT.AK_Success;
				}
				if (aKRESULT == AKRESULT.AK_Success)
				{
					aKRESULT = AkSoundEngine.LoadBank(bankName, -1, out m_BankID);
					if (!string.IsNullOrEmpty(text))
					{
						AkSoundEngine.SetBasePath(text);
					}
				}
			}
			else
			{
				aKRESULT = AkSoundEngine.LoadAndDecodeBank(bankName, saveDecodedBank, out m_BankID);
			}
			if (aKRESULT != AKRESULT.AK_Success)
			{
				Debug.LogWarning("WwiseUnity: Bank " + bankName + " failed to load (" + aKRESULT.ToString() + ")");
			}
		}
		IncRef();
	}

	public void LoadBankAsync(AkCallbackManager.BankCallback callback = null)
	{
		if (m_RefCount == 0)
		{
			if (AkBankManager.BanksToUnload.Contains(this))
			{
				AkBankManager.BanksToUnload.Remove(this);
				IncRef();
				return;
			}
			bankCallback = callback;
			AkSoundEngine.LoadBank(bankName, AkBankManager.GlobalBankCallback, this, -1, out m_BankID);
		}
		IncRef();
	}

	public void IncRef()
	{
		m_RefCount++;
	}

	public void DecRef()
	{
		m_RefCount--;
		if (m_RefCount == 0)
		{
			AkBankManager.BanksToUnload.Add(this);
		}
	}
}

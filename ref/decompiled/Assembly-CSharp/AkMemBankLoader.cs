using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class AkMemBankLoader : MonoBehaviour
{
	public string bankName = string.Empty;

	public bool isLocalizedBank;

	private WWW ms_www;

	private GCHandle ms_pinnedArray;

	private IntPtr ms_pInMemoryBankPtr = IntPtr.Zero;

	[HideInInspector]
	public uint ms_bankID;

	private const int WaitMs = 50;

	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT = 16L;

	private const long AK_BANK_PLATFORM_DATA_ALIGNMENT_MASK = 15L;

	private string m_bankPath;

	private void Start()
	{
		if (isLocalizedBank)
		{
			LoadLocalizedBank(bankName);
		}
		else
		{
			LoadNonLocalizedBank(bankName);
		}
	}

	public void LoadNonLocalizedBank(string in_bankFilename)
	{
		string in_bankPath = "file://" + Path.Combine(AkBasePathGetter.GetPlatformBasePath(), in_bankFilename);
		DoLoadBank(in_bankPath);
	}

	public void LoadLocalizedBank(string in_bankFilename)
	{
		string in_bankPath = "file://" + Path.Combine(Path.Combine(AkBasePathGetter.GetPlatformBasePath(), AkInitializer.GetCurrentLanguage()), in_bankFilename);
		DoLoadBank(in_bankPath);
	}

	private IEnumerator LoadFile()
	{
		ms_www = new WWW(m_bankPath);
		yield return ms_www;
		uint in_uInMemoryBankSize2 = 0u;
		try
		{
			ms_pinnedArray = GCHandle.Alloc(ms_www.bytes, GCHandleType.Pinned);
			ms_pInMemoryBankPtr = ms_pinnedArray.AddrOfPinnedObject();
			in_uInMemoryBankSize2 = (uint)ms_www.bytes.Length;
			if ((ms_pInMemoryBankPtr.ToInt64() & 0xF) != 0)
			{
				byte[] array = new byte[(long)ms_www.bytes.Length + 16L];
				GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				IntPtr intPtr = gCHandle.AddrOfPinnedObject();
				int destinationIndex = 0;
				if ((intPtr.ToInt64() & 0xF) != 0)
				{
					long num = (intPtr.ToInt64() + 15) & -16;
					destinationIndex = (int)(num - intPtr.ToInt64());
					intPtr = new IntPtr(num);
				}
				Array.Copy(ms_www.bytes, 0, array, destinationIndex, ms_www.bytes.Length);
				ms_pInMemoryBankPtr = intPtr;
				ms_pinnedArray.Free();
				ms_pinnedArray = gCHandle;
			}
		}
		catch
		{
			yield break;
		}
		AKRESULT result = AkSoundEngine.LoadBank(ms_pInMemoryBankPtr, in_uInMemoryBankSize2, out ms_bankID);
		if (result != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: AkMemBankLoader: bank loading failed with result " + result);
		}
	}

	private void DoLoadBank(string in_bankPath)
	{
		m_bankPath = in_bankPath;
		StartCoroutine(LoadFile());
	}

	private void OnDestroy()
	{
		if (ms_pInMemoryBankPtr != IntPtr.Zero)
		{
			AKRESULT aKRESULT = AkSoundEngine.UnloadBank(ms_bankID, ms_pInMemoryBankPtr);
			if (aKRESULT == AKRESULT.AK_Success)
			{
				ms_pinnedArray.Free();
			}
		}
	}
}

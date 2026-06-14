using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkBank")]
public class AkBank : AkUnityEventHandler
{
	public string bankName = string.Empty;

	public bool loadAsynchronous;

	public bool decodeBank;

	public bool saveDecodedBank;

	public List<int> unloadTriggerList = new List<int> { -358577003 };

	protected override void Awake()
	{
		base.Awake();
		RegisterTriggers(unloadTriggerList, UnloadBank);
		if (unloadTriggerList.Contains(1151176110))
		{
			UnloadBank(null);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (unloadTriggerList.Contains(1281810935))
		{
			UnloadBank(null);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		if (!loadAsynchronous)
		{
			AkBankManager.LoadBank(bankName, decodeBank, saveDecodedBank);
		}
		else
		{
			AkBankManager.LoadBankAsync(bankName);
		}
	}

	public void UnloadBank(GameObject in_gameObject)
	{
		AkBankManager.UnloadBank(bankName);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnregisterTriggers(unloadTriggerList, UnloadBank);
		if (unloadTriggerList.Contains(-358577003))
		{
			UnloadBank(null);
		}
	}
}

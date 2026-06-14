using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLoadable;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
[RequireComponent(typeof(MultiStageTransferInteraction))]
[RequireComponent(typeof(ItemContainer))]
public abstract class MultistageItemConverter : T17MonoBehaviour, IMultistageTransferInteractionResponder, Saveable, INetworkLoadable
{
	[Serializable]
	public class ItemConverterConversions
	{
		public ItemData m_Input;

		public ItemData m_Output;
	}

	protected enum ConversionState
	{
		AwaitingInput,
		AwaitingOutput
	}

	[Serializable]
	protected class SaveData_MultistageItemConverter_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public int I;

		public ConversionState C;

		public SaveData_MultistageItemConverter_V1()
		{
			m_Version = 1;
		}
	}

	protected ConversionState m_ConversionState;

	protected ItemContainer m_Container;

	private ItemData m_InputedItem;

	private Dictionary<ItemData, ItemData> m_InputOutputItems;

	private SaveDataRegister m_SaveData;

	public T17NetView m_NetView;

	private int m_ItemMgrResponseID = -1;

	protected string m_LoadError = string.Empty;

	protected LOADSTATE m_LoadState;

	protected abstract void OnItemsProduced();

	protected abstract void OnItemsProducedFailed();

	protected abstract void OnInputRecieved();

	protected abstract void OnOutputGiven();

	protected abstract SaveData_MultistageItemConverter_V1 CreateSnapshotData();

	protected abstract SaveData_MultistageItemConverter_V1 RetrieveSnapshotData(PrisonSnapshotIO.SnapshotData_Base snapshotBase, string rawSaveData);

	protected abstract void StartedFromSnapshotWithData(SaveData_MultistageItemConverter_V1 data);

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_Container = GetComponent<ItemContainer>();
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView == null)
			{
				Debug.LogErrorFormat("MultistageItemConverter.Init: Failed to find NetView : {0}", base.gameObject.name);
				T17NetManager.LogGoogleException("MultistageItemConverter " + base.transform.name + " does not have T17NetView in prison " + LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
			}
		}
		if (m_NetView != null)
		{
			m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: false);
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		if (T17NetManager.IsMasterClient)
		{
			SetStateRPC(ConversionState.AwaitingInput);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_NetView = null;
		m_Container = null;
		m_InputedItem = null;
		m_InputOutputItems.Clear();
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
	}

	public void SetInputOutputItemTypes(ItemConverterConversions[] inOuts)
	{
		m_InputOutputItems = null;
		if (inOuts == null || inOuts.Length <= 0)
		{
			return;
		}
		foreach (ItemConverterConversions itemConverterConversions in inOuts)
		{
			if (itemConverterConversions.m_Input != null && itemConverterConversions.m_Output != null)
			{
				if (m_InputOutputItems == null)
				{
					m_InputOutputItems = new Dictionary<ItemData, ItemData>();
				}
				if (!m_InputOutputItems.ContainsKey(itemConverterConversions.m_Input))
				{
					m_InputOutputItems.Add(itemConverterConversions.m_Input, itemConverterConversions.m_Output);
				}
			}
		}
	}

	protected void SetInputItemRPC(ItemData input)
	{
		if (input != null)
		{
			m_NetView.RPC("RPC_SetInputItem", NetTargets.All, input.m_ItemDataID);
		}
		else
		{
			m_NetView.RPC("RPC_SetInputItem", NetTargets.All, -1);
		}
	}

	[PunRPC]
	public void RPC_SetInputItem(int itemDataId)
	{
		if (itemDataId != -1)
		{
			m_InputedItem = ItemManager.GetInstance().GetItemDataWithID(itemDataId);
			if (!(m_InputedItem == null))
			{
			}
		}
		else
		{
			m_InputedItem = null;
			SetStateRPC(ConversionState.AwaitingInput);
		}
	}

	private void SetStateRPC(ConversionState state)
	{
		m_NetView.PostLevelLoadRPC("RPC_SetState", NetTargets.All, (int)state);
	}

	[PunRPC]
	public void RPC_SetState(int state)
	{
		m_ConversionState = (ConversionState)state;
	}

	protected void RequestItemCreation(ItemData itemData)
	{
		if (itemData != null)
		{
			ItemManager.GetInstance().AssignItemRPC(0, itemData.m_ItemDataID, OnItemMgrResponseAddOutputItem, ref m_ItemMgrResponseID);
		}
	}

	private void OnItemMgrResponseAddOutputItem(Item item, int eventID)
	{
		if (item == null)
		{
			OnItemsProducedFailed();
		}
		else
		{
			if (eventID != m_ItemMgrResponseID)
			{
				return;
			}
			if (m_Container.AddItemRPC(item))
			{
				SetStateRPC(ConversionState.AwaitingOutput);
				OnItemsProduced();
				return;
			}
			OnItemsProducedFailed();
			if (item != null)
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
	}

	protected ItemData GetInputtedItem()
	{
		return m_InputedItem;
	}

	public virtual void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		if (!(item != null))
		{
			return;
		}
		if (to == m_Container)
		{
			if (m_ConversionState == ConversionState.AwaitingInput && !(GetOutputDataForInput(item) == null))
			{
				SetInputItemRPC(item.m_ItemData);
				OnInputRecieved();
			}
		}
		else if (m_ConversionState == ConversionState.AwaitingOutput)
		{
			SetInputItemRPC(null);
			m_Container.RemoveAllItems(releaseToManager: true);
			OnOutputGiven();
		}
	}

	public virtual void OnTransferFailed()
	{
		SetStateRPC(ConversionState.AwaitingInput);
	}

	public abstract bool CanInteract(Character localCharacter);

	public abstract bool IsInteractionVisible();

	public abstract void OnStartInteraction(Character localCharacter, out TransferItemsInteraction.TransferDirection direction, out ItemData[] itemTypesToTransfer);

	public abstract int GetNumberStages();

	public abstract int GetCurrentStage();

	public ItemData GetOutputForCurrentInput()
	{
		return GetOutputDataForInput(m_InputedItem);
	}

	public ItemData[] GetPossibleInputItemTypes()
	{
		if (m_InputOutputItems != null)
		{
			return m_InputOutputItems.Keys.ToArray();
		}
		return null;
	}

	public ItemData[] GetPossibleOutputItemTypes()
	{
		if (m_InputOutputItems != null)
		{
			return m_InputOutputItems.Values.ToArray();
		}
		return null;
	}

	public bool IsItemValidInput(Item theItem)
	{
		if (m_InputOutputItems == null)
		{
			return false;
		}
		return GetOutputDataForInput(theItem) != null;
	}

	public ItemData GetOutputDataForInput(Item input)
	{
		if (input == null)
		{
			return null;
		}
		return GetOutputDataForInput(input.m_ItemData);
	}

	public ItemData GetOutputDataForInput(ItemData input)
	{
		if (m_InputOutputItems == null)
		{
			return null;
		}
		if (input == null)
		{
			return null;
		}
		if (m_InputOutputItems == null)
		{
			T17NetManager.LogGoogleException("GetOutputDataForInput(ItemData input): m_InputOutputItems == null");
		}
		foreach (KeyValuePair<ItemData, ItemData> inputOutputItem in m_InputOutputItems)
		{
			if (inputOutputItem.Key == null)
			{
				T17NetManager.LogGoogleException("GetOutputDataForInput(ItemData input): element.Key == null");
			}
			if (inputOutputItem.Key.m_ItemDataID == input.m_ItemDataID)
			{
				return inputOutputItem.Value;
			}
		}
		return null;
	}

	public virtual string CreateSnapshot()
	{
		SaveData_MultistageItemConverter_V1 saveData_MultistageItemConverter_V = CreateSnapshotData();
		saveData_MultistageItemConverter_V.I = ((!(m_InputedItem != null)) ? (-1) : m_InputedItem.m_ItemDataID);
		saveData_MultistageItemConverter_V.C = m_ConversionState;
		return JsonUtility.ToJson(saveData_MultistageItemConverter_V);
	}

	public virtual void StartedFromSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (snapshotData_Base == null)
		{
			return;
		}
		SaveData_MultistageItemConverter_V1 saveData_MultistageItemConverter_V = RetrieveSnapshotData(snapshotData_Base, m_SaveData.GetSaveData());
		if (saveData_MultistageItemConverter_V != null)
		{
			int i = saveData_MultistageItemConverter_V.I;
			if (i != -1)
			{
				m_InputedItem = ItemManager.GetInstance().GetItemDataWithID(i);
				SetInputItemRPC(m_InputedItem);
			}
			SetStateRPC(saveData_MultistageItemConverter_V.C);
			StartedFromSnapshotWithData(saveData_MultistageItemConverter_V);
		}
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			if (m_LoadState == LOADSTATE.Finished_OK)
			{
				Master_RPCLoadDataToClient(player);
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No_MultistageItemConverter", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_MultistageItemConverter(PhotonMessageInfo info)
	{
		m_LoadError = "MultistageItemConverter RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	protected virtual void Master_RPCLoadDataToClient(PhotonPlayer player)
	{
		m_NetView.RPC("RPC_Client_LoadMultistageConverterBaseData", player, m_ConversionState, (!(m_InputedItem != null)) ? (-1) : m_InputedItem.m_ItemDataID);
	}

	[PunRPC]
	protected void RPC_Client_LoadMultistageConverterBaseData(ConversionState converstionState, int inputtedItemDataId)
	{
		SetItemConverterBaseData(converstionState, inputtedItemDataId);
		m_LoadState = LOADSTATE.Finished_OK;
	}

	protected void SetItemConverterBaseData(ConversionState converstionState, int inputtedItemDataId)
	{
		if (converstionState != m_ConversionState)
		{
			RPC_SetState((int)converstionState);
		}
		if (inputtedItemDataId == -1)
		{
			m_InputedItem = null;
		}
		else
		{
			m_InputedItem = ItemManager.GetInstance().GetItemDataWithID(inputtedItemDataId);
		}
	}
}

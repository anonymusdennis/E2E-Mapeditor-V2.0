using System;
using AUTOGEN_T17Wwise_Enums;
using NetworkLoadable;
using UnityEngine;

public class PlantPatch : MultistageItemConverter
{
	public delegate void PlantPatchHandler(PlantPatch plantPatch);

	public enum State
	{
		Unassigned,
		EmptyPatch,
		DugPatch,
		SeedsPlaced,
		SeedsGrowing,
		FullyGrown,
		IntermediateState
	}

	protected class SaveData_PlantPatch_V1 : SaveData_MultistageItemConverter_V1
	{
		public int S;

		public int P;
	}

	public GameObject EmptyPlotGO;

	public GameObject PlotWithHoleGO;

	public GameObject SeedsPlacedGO;

	public GameObject PlantGrowingGO;

	public GameObject PlantGO;

	private NetObjectLock m_NetObjectLock;

	public string m_EmptyPatchText = "Text.Interact.FarmingA";

	public string m_DugPatchText = "Text.Interact.FarmingB";

	public string m_SeedsPlacedText = "Text.Interact.FarmingC";

	public string m_FullyGrownText = "Text.Interact.FarmingD";

	private State m_State;

	private int m_PlantedDay;

	public event PlantPatchHandler SeedGrowingEvent;

	public event PlantPatchHandler GrownPlantTakenEvent;

	protected override void Awake()
	{
		base.Awake();
		m_NetObjectLock = GetComponent<NetObjectLock>();
		if (SeedsPlacedGO == null)
		{
			SeedsPlacedGO = PlantGO;
		}
		if (PlantGrowingGO == null)
		{
			PlantGrowingGO = PlantGO;
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		if (EmptyPlotGO == null || PlotWithHoleGO == null || PlantGO == null)
		{
		}
		if (T17NetManager.IsMasterClient)
		{
			SetStateRPC(State.EmptyPatch);
		}
		JobsManager.GetInstance().JobTimeStartedEvent += PlantPatch_JobTimeStartedEvent;
		m_Container.m_bShouldConsiderItemRefresh = false;
		return result;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (JobsManager.GetInstance() != null)
		{
			JobsManager.GetInstance().JobTimeStartedEvent -= PlantPatch_JobTimeStartedEvent;
		}
	}

	private void PlantPatch_JobTimeStartedEvent(bool isSaveRestore)
	{
		if (m_State == State.SeedsGrowing && !isSaveRestore)
		{
			SetStateRPC(State.FullyGrown);
		}
	}

	public void GardenFunctionalityInteractRPC()
	{
		switch (m_State)
		{
		case State.EmptyPatch:
			SetStateRPC(State.DugPatch);
			break;
		case State.SeedsPlaced:
			SetStateRPC(State.SeedsGrowing);
			RequestItemCreation(GetOutputForCurrentInput());
			break;
		}
	}

	public override bool CanInteract(Character localCharacter)
	{
		switch (m_State)
		{
		case State.EmptyPatch:
			return false;
		case State.DugPatch:
		{
			Item equippedItem = localCharacter.GetEquippedItem();
			return equippedItem != null && IsItemValidInput(localCharacter.GetEquippedItem());
		}
		case State.SeedsPlaced:
			return false;
		case State.SeedsGrowing:
			return false;
		case State.FullyGrown:
			return !localCharacter.m_ItemContainer.IsVisibleFull();
		case State.IntermediateState:
			return false;
		case State.Unassigned:
			return false;
		default:
			return false;
		}
	}

	public override bool IsInteractionVisible()
	{
		switch (m_State)
		{
		case State.EmptyPatch:
		case State.DugPatch:
		case State.SeedsPlaced:
		case State.FullyGrown:
			return true;
		default:
			return false;
		}
	}

	public override void OnStartInteraction(Character localCharacter, out TransferItemsInteraction.TransferDirection direction, out ItemData[] itemTypesToTransfer)
	{
		direction = TransferItemsInteraction.TransferDirection.Invalid;
		itemTypesToTransfer = null;
		if (CanInteract(localCharacter))
		{
			switch (m_State)
			{
			case State.EmptyPatch:
				SetStateRPC(State.DugPatch);
				break;
			case State.DugPatch:
				direction = TransferItemsInteraction.TransferDirection.FromCharacter;
				itemTypesToTransfer = new ItemData[1] { localCharacter.GetEquippedItem().m_ItemData };
				break;
			case State.SeedsPlaced:
				SetStateRPC(State.SeedsGrowing);
				RequestItemCreation(GetOutputForCurrentInput());
				break;
			case State.SeedsGrowing:
				break;
			case State.FullyGrown:
				direction = TransferItemsInteraction.TransferDirection.ToCharacter;
				itemTypesToTransfer = new ItemData[1] { GetOutputForCurrentInput() };
				break;
			case State.IntermediateState:
				break;
			case State.Unassigned:
				break;
			}
		}
	}

	protected override void OnItemsProduced()
	{
	}

	protected override void OnItemsProducedFailed()
	{
		SetStateRPC(State.DugPatch);
		m_Container.RemoveAllItems(releaseToManager: true);
	}

	private void SetStateRPC(State state, bool isRestore = false)
	{
		m_NetView.RPC("RPC_SetPlantState", NetTargets.All, (int)state, isRestore);
	}

	[PunRPC]
	public void RPC_SetPlantState(int state, bool isRestore = false)
	{
		m_State = (State)state;
		if (m_State == State.SeedsGrowing)
		{
			if (!isRestore)
			{
				m_PlantedDay = RoutineManager.GetInstance().GetDaysElapsed();
				if (this.SeedGrowingEvent != null)
				{
					this.SeedGrowingEvent(this);
				}
			}
		}
		else if (m_State == State.FullyGrown)
		{
			m_PlantedDay = -1;
		}
		SetVisualsForState(m_State);
	}

	private void SetVisualsForState(State state)
	{
		EmptyPlotGO.SetActive(value: false);
		PlotWithHoleGO.SetActive(value: false);
		SeedsPlacedGO.SetActive(value: false);
		PlantGrowingGO.SetActive(value: false);
		PlantGO.SetActive(value: false);
		switch (m_State)
		{
		case State.EmptyPatch:
			EmptyPlotGO.SetActive(value: true);
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_EmptyPatchText, localise: true);
			}
			break;
		case State.DugPatch:
			PlotWithHoleGO.SetActive(value: true);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Farming_Dig, base.gameObject);
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_DugPatchText, localise: true);
			}
			break;
		case State.SeedsPlaced:
			SeedsPlacedGO.SetActive(value: true);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Farming_Seeds, base.gameObject);
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_SeedsPlacedText, localise: true);
			}
			break;
		case State.SeedsGrowing:
			PlantGrowingGO.SetActive(value: true);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Farming_Fill, base.gameObject);
			break;
		case State.FullyGrown:
			PlantGO.SetActive(value: true);
			if (m_NetObjectLock != null)
			{
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_FullyGrownText, localise: true);
			}
			break;
		}
	}

	public override int GetNumberStages()
	{
		return Enum.GetValues(typeof(State)).Length;
	}

	public override int GetCurrentStage()
	{
		return (int)m_State;
	}

	protected override void OnInputRecieved()
	{
		SetStateRPC(State.SeedsPlaced);
	}

	protected override void OnOutputGiven()
	{
		SetStateRPC(State.EmptyPatch);
		if (this.GrownPlantTakenEvent != null)
		{
			this.GrownPlantTakenEvent(this);
		}
	}

	public override void OnTransferFailed()
	{
		base.OnTransferFailed();
		SetStateRPC(State.EmptyPatch);
	}

	public State GetPlantState()
	{
		return m_State;
	}

	protected override SaveData_MultistageItemConverter_V1 CreateSnapshotData()
	{
		SaveData_PlantPatch_V1 saveData_PlantPatch_V = new SaveData_PlantPatch_V1();
		saveData_PlantPatch_V.S = (int)m_State;
		saveData_PlantPatch_V.P = m_PlantedDay;
		return saveData_PlantPatch_V;
	}

	protected override SaveData_MultistageItemConverter_V1 RetrieveSnapshotData(PrisonSnapshotIO.SnapshotData_Base snapshotBase, string rawSaveData)
	{
		if (snapshotBase != null && snapshotBase.m_Version == 1)
		{
			SaveData_PlantPatch_V1 saveData_PlantPatch_V = null;
			try
			{
				saveData_PlantPatch_V = JsonUtility.FromJson<SaveData_PlantPatch_V1>(rawSaveData);
			}
			catch
			{
			}
			if (saveData_PlantPatch_V != null)
			{
				return saveData_PlantPatch_V;
			}
		}
		return null;
	}

	protected override void StartedFromSnapshotWithData(SaveData_MultistageItemConverter_V1 data)
	{
		if (data is SaveData_PlantPatch_V1 saveData_PlantPatch_V)
		{
			m_State = (State)saveData_PlantPatch_V.S;
			m_PlantedDay = saveData_PlantPatch_V.P;
			SetStateRPC(m_State, isRestore: true);
		}
	}

	protected override void Master_RPCLoadDataToClient(PhotonPlayer player)
	{
		ItemData inputtedItem = GetInputtedItem();
		int num = ((!(inputtedItem != null)) ? (-1) : inputtedItem.m_ItemDataID);
		m_NetView.RPC("RPC_Client_PlantPatchData", player, m_ConversionState, num, m_State, m_PlantedDay);
	}

	[PunRPC]
	protected void RPC_Client_PlantPatchData(ConversionState converstionState, int inputtedItemDataId, State plantState, int plantedDay)
	{
		SetItemConverterBaseData(converstionState, inputtedItemDataId);
		if (m_State != plantState)
		{
			RPC_SetPlantState((int)plantState, isRestore: true);
		}
		m_PlantedDay = plantedDay;
		m_LoadState = LOADSTATE.Finished_OK;
	}
}

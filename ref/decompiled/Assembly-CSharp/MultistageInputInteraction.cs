using System;
using System.Collections;
using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(ItemContainer))]
[RequireComponent(typeof(NetObjectLock))]
[RequireComponent(typeof(MultiStageTransferInteraction))]
[RequireComponent(typeof(T17NetView))]
public class MultistageInputInteraction : T17MonoBehaviour, IMultistageTransferInteractionResponder, Saveable, INetworkLoadable
{
	[Serializable]
	public class Stage
	{
		public ItemData m_ItemRequired;

		public SpeechPODO m_CharacterSucceedDialog;

		public SpeechPODO m_CharacterWrongDialog;

		public string m_SignDialog;

		public string m_StageCompleteSound;

		[FormerlySerializedAs("m_StageVisualObject")]
		public GameObject m_StageVisualObjectA;

		public GameObject m_StageVisualObjectB;

		public int m_MinimumStamina;

		public int m_MinimumIntellect;

		public int m_MinimumStrength;

		public bool m_bIsProgressedViaExternal;
	}

	public enum ModeSupportedTypes
	{
		SinglePlayer,
		MultiPlayer,
		Dynamic
	}

	[Serializable]
	protected class SaveData_MultistageInputInteraction_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public int S;

		public bool E;

		public bool CompletedAll;

		public SaveData_MultistageInputInteraction_V1()
		{
			m_Version = 1;
		}
	}

	[Header("Stages")]
	public List<Stage> m_Stages = new List<Stage>();

	public List<GameObject> m_ObjectsToDisableOnCompletion = new List<GameObject>();

	public List<GameObject> m_ObjectsToEnableOnCompletion = new List<GameObject>();

	public bool m_bForceBuildingID;

	[FormerlySerializedAs("m_EscapeType")]
	public ModeSupportedTypes m_SupportedMode = ModeSupportedTypes.Dynamic;

	[Header("Interaction State")]
	public bool m_IsInteractionEnabled = true;

	[Header("Animatation")]
	public Animator m_AnimatedVisualObject;

	public RuntimeAnimatorController m_AnimatedVisualObjectController;

	private Stage m_CurrentStage;

	protected Character m_LastInteractingCharacter;

	private bool m_bHasCompletedAllStages;

	protected ItemContainer m_Container;

	protected SaveDataRegister m_SaveData;

	protected T17NetView m_NetView;

	protected NetObjectLock m_NetObjectLock;

	private int m_AnimStateParamHash = Animator.StringToHash("State");

	private const float m_TimeToHideAnimatedObject = 0.6f;

	protected string m_LoadError = string.Empty;

	protected LOADSTATE m_LoadState;

	protected override void Awake()
	{
		base.Awake();
		m_Container = GetComponent<ItemContainer>();
		m_Container.m_MaxSize = m_Stages.Count;
		m_NetView = GetComponent<T17NetView>();
		if (m_NetView == null)
		{
			Debug.LogErrorFormat("MultistageItemConverter.Init: Failed to find NetView : {0}", base.gameObject.name);
		}
		m_NetObjectLock = GetComponent<NetObjectLock>();
		if (m_NetObjectLock == null)
		{
			Debug.LogErrorFormat("MultistageItemConverter.Init: Failed to find m_NetObjectLock : {0}", base.gameObject.name);
		}
		if (!T17NetManager.IsMasterClient)
		{
			GlobalStart.EnteredLevelEvent += GlobalStart_EnteredLevelEvent;
		}
	}

	private void GlobalStart_EnteredLevelEvent()
	{
		if (m_bHasCompletedAllStages)
		{
			SetVisualForCompletedAllStages();
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_AnimatedVisualObject != null && m_AnimatedVisualObjectController != null)
		{
			m_AnimatedVisualObject.runtimeAnimatorController = m_AnimatedVisualObjectController;
		}
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: false);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		if (T17NetManager.IsMasterClient)
		{
			SetStageRPC(0, isSaveRestore: true);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		GlobalStart.EnteredLevelEvent -= GlobalStart_EnteredLevelEvent;
		m_Stages.Clear();
		m_ObjectsToDisableOnCompletion.Clear();
		m_ObjectsToEnableOnCompletion.Clear();
		m_AnimatedVisualObject = null;
		m_AnimatedVisualObjectController = null;
		m_LastInteractingCharacter = null;
		m_Container = null;
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		if (m_NetObjectLock != null)
		{
			if (m_NetObjectLock.IsLocked() && m_NetObjectLock.m_NetView != null)
			{
				m_NetObjectLock.ReleaseLock();
			}
			m_NetObjectLock = null;
		}
		m_NetView = null;
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
	}

	public void EnableInteraction()
	{
		m_NetView.RPC("RPC_EnableInteraction", NetTargets.All, true);
	}

	public void DisableInteraction()
	{
		m_NetView.RPC("RPC_EnableInteraction", NetTargets.All, false);
	}

	[PunRPC]
	public virtual void RPC_EnableInteraction(bool bEnable)
	{
		m_IsInteractionEnabled = bEnable;
	}

	public virtual void ProgressToNextStage()
	{
		int num = m_Stages.IndexOf(m_CurrentStage) + 1;
		if (num < m_Stages.Count)
		{
			SetStageRPC(num);
		}
		else if (num == m_Stages.Count)
		{
			OnInteractedWithFinalStage(m_LastInteractingCharacter);
			m_NetView.RPC("RPC_All_SetHasCompletedAllStages", NetTargets.All, true);
		}
	}

	[PunRPC]
	public void RPC_All_SetHasCompletedAllStages(bool hasCompleted)
	{
		m_bHasCompletedAllStages = hasCompleted;
		if (m_bHasCompletedAllStages)
		{
			SetVisualForCompletedAllStages();
		}
	}

	protected virtual bool OnInteractedWithFinalStage(Character interactingCharacter)
	{
		return true;
	}

	protected void SetStageRPC(int stageIndex, bool isSaveRestore = false)
	{
		m_NetView.RPC("RPC_SetStage", NetTargets.All, stageIndex, isSaveRestore);
	}

	[PunRPC]
	public virtual void RPC_SetStage(int stage, bool isSaveRestore)
	{
		if (m_Stages != null)
		{
			Stage currentStage = m_CurrentStage;
			m_CurrentStage = m_Stages[stage];
			UpdateVisualsForStage(m_CurrentStage, isSaveRestore);
			if (!IsFinalStage())
			{
				m_bHasCompletedAllStages = false;
				Localization.Get(m_NetObjectLock.m_InteractActionNameTag, out var localized);
				localized = ((!string.IsNullOrEmpty(localized)) ? localized : m_NetObjectLock.m_InteractActionNameTag);
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localized);
			}
			if (!isSaveRestore && currentStage != null && !string.IsNullOrEmpty(currentStage.m_StageCompleteSound))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, currentStage.m_StageCompleteSound, base.gameObject);
			}
		}
	}

	protected void UpdateVisualsForStage(Stage stage, bool isSaveRestore)
	{
		int num = 0;
		if (m_Stages != null)
		{
			int count = m_Stages.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_Stages[i] == stage)
				{
					num = i;
				}
				bool flag = m_Stages[i] == stage;
				if (m_Stages[i].m_StageVisualObjectA != null)
				{
					m_Stages[i].m_StageVisualObjectA.SetActive(flag);
					if (flag)
					{
						AddToCullingBuckets(m_Stages[i].m_StageVisualObjectA);
					}
					else
					{
						RemoveFromCullingBuckets(m_Stages[i].m_StageVisualObjectA);
					}
				}
				if (m_Stages[i].m_StageVisualObjectB != null)
				{
					m_Stages[i].m_StageVisualObjectB.SetActive(flag);
					if (flag)
					{
						AddToCullingBuckets(m_Stages[i].m_StageVisualObjectB);
					}
					else
					{
						RemoveFromCullingBuckets(m_Stages[i].m_StageVisualObjectB);
					}
				}
			}
		}
		if (m_AnimatedVisualObject != null)
		{
			if (isSaveRestore)
			{
				m_AnimatedVisualObject.Play("Stage " + num, 0, 1f);
			}
			m_AnimatedVisualObject.SetInteger(m_AnimStateParamHash, num);
		}
	}

	protected void SetVisualForCompletedAllStages()
	{
		for (int num = m_ObjectsToDisableOnCompletion.Count - 1; num >= 0; num--)
		{
			if (m_ObjectsToDisableOnCompletion[num] != null)
			{
				m_ObjectsToDisableOnCompletion[num].SetActive(value: false);
				RemoveFromCullingBuckets(m_ObjectsToDisableOnCompletion[num]);
			}
		}
		for (int num2 = m_ObjectsToEnableOnCompletion.Count - 1; num2 >= 0; num2--)
		{
			if (m_ObjectsToEnableOnCompletion[num2] != null)
			{
				m_ObjectsToEnableOnCompletion[num2].SetActive(value: true);
				AddToCullingBuckets(m_ObjectsToEnableOnCompletion[num2]);
			}
		}
	}

	private void AddToCullingBuckets(GameObject gO)
	{
		MeshRenderer[] componentsInChildren = gO.GetComponentsInChildren<MeshRenderer>();
		Animator componentInParent = gO.GetComponentInParent<Animator>();
		if (componentInParent != null)
		{
			CullingObjectCollector.AnimatedWrapper animatedWrapper = new CullingObjectCollector.AnimatedWrapper(componentInParent.gameObject);
			animatedWrapper.Init();
			animatedWrapper.DisableLogic();
			CullingObjectCollector.GetInstance().Runtime_AddAnimWrapper(animatedWrapper);
			return;
		}
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_bForceBuildingID)
			{
				CullingObjectCollector.GetInstance().Runtime_AddToBucket(componentsInChildren[i], bCheckForMaterialBlock: false, bAlsoFloorsAbove: true, 1);
			}
			else
			{
				CullingObjectCollector.GetInstance().Runtime_AddToBucket(componentsInChildren[i], bCheckForMaterialBlock: false, bAlsoFloorsAbove: true);
			}
		}
	}

	private void RemoveFromCullingBuckets(GameObject gO)
	{
		MeshRenderer[] componentsInChildren = gO.GetComponentsInChildren<MeshRenderer>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			CullingObjectCollector.GetInstance().Runtime_RemoveFromBucket(componentsInChildren[i]);
		}
	}

	protected void PlaySucceedDialogOnCharacter(Character character)
	{
		if (m_CurrentStage != null)
		{
			PlayDialogOnCharacter(character, m_CurrentStage.m_CharacterSucceedDialog);
		}
	}

	protected void PlayFailedDialogOnCharacter(Character character)
	{
		if (m_CurrentStage != null)
		{
			PlayDialogOnCharacter(character, m_CurrentStage.m_CharacterWrongDialog);
		}
	}

	protected void PlayDialogOnCharacter(Character character, SpeechPODO dialog)
	{
		if (character != null && dialog != null)
		{
			SpeechManager.GetInstance().SaySomething(character, dialog);
		}
	}

	protected IEnumerator HideAnimatedObject()
	{
		while (CutsceneManagerBase.GetState() != CutsceneManagerBase.States.Playing)
		{
			yield return new WaitForEndOfFrame();
		}
		if (m_AnimatedVisualObject != null)
		{
			m_AnimatedVisualObject.gameObject.SetActive(value: false);
		}
	}

	public virtual void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		if (item != null && to == m_Container && m_CurrentStage.m_ItemRequired.m_ItemDataID == item.m_ItemData.m_ItemDataID)
		{
			PlaySucceedDialogOnCharacter(m_LastInteractingCharacter);
			ProgressToNextStage();
		}
	}

	public virtual void OnTransferFailed()
	{
		m_LastInteractingCharacter = null;
		SetStageRPC(0);
	}

	public virtual bool CanInteract(Character localCharacter)
	{
		if (!m_IsInteractionEnabled)
		{
			return false;
		}
		if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus && m_SupportedMode == ModeSupportedTypes.MultiPlayer)
		{
			return false;
		}
		if (!m_CurrentStage.m_bIsProgressedViaExternal && localCharacter != null && localCharacter.m_CharacterStats.Intellect >= (float)m_CurrentStage.m_MinimumIntellect && localCharacter.m_CharacterStats.Strength >= (float)m_CurrentStage.m_MinimumStrength && localCharacter.m_CharacterStats.Cardio >= (float)m_CurrentStage.m_MinimumStamina && HasCorrectItem(localCharacter))
		{
			return true;
		}
		PlayFailedDialogOnCharacter(localCharacter);
		return false;
	}

	public virtual bool IsInteractionVisible()
	{
		if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus && m_SupportedMode == ModeSupportedTypes.MultiPlayer)
		{
			return false;
		}
		return m_IsInteractionEnabled;
	}

	private bool HasCorrectItem(Character localCharacter)
	{
		if (localCharacter == null)
		{
			return false;
		}
		if (m_CurrentStage.m_ItemRequired == null)
		{
			return true;
		}
		Item equippedItem = localCharacter.GetEquippedItem();
		return equippedItem != null && equippedItem.ItemDataID == m_CurrentStage.m_ItemRequired.m_ItemDataID;
	}

	public void OnStartInteraction(Character localCharacter, out TransferItemsInteraction.TransferDirection direction, out ItemData[] itemTypesToTransfer)
	{
		direction = TransferItemsInteraction.TransferDirection.Invalid;
		itemTypesToTransfer = null;
		m_LastInteractingCharacter = localCharacter;
		if (m_CurrentStage.m_ItemRequired != null)
		{
			direction = TransferItemsInteraction.TransferDirection.FromCharacter;
			itemTypesToTransfer = new ItemData[1] { m_CurrentStage.m_ItemRequired };
		}
		else
		{
			PlaySucceedDialogOnCharacter(localCharacter);
			ProgressToNextStage();
		}
	}

	public int GetNumberStages()
	{
		if (m_Stages != null)
		{
			return m_Stages.Count;
		}
		return 0;
	}

	public int GetCurrentStage()
	{
		if (m_Stages != null && m_CurrentStage != null)
		{
			return m_Stages.IndexOf(m_CurrentStage);
		}
		return 0;
	}

	public int GetFinalStage()
	{
		if (m_Stages != null)
		{
			return m_Stages.Count - 1;
		}
		return 0;
	}

	public bool IsFinalStage()
	{
		return GetCurrentStage() == GetNumberStages() - 1;
	}

	public bool HasCompletedAllStages()
	{
		return m_bHasCompletedAllStages;
	}

	protected virtual SaveData_MultistageInputInteraction_V1 RetrieveSnapshotData(PrisonSnapshotIO.SnapshotData_Base snapshotBase, string rawSaveData)
	{
		if (snapshotBase != null && snapshotBase.m_Version == 1)
		{
			SaveData_MultistageInputInteraction_V1 saveData_MultistageInputInteraction_V = null;
			try
			{
				saveData_MultistageInputInteraction_V = JsonUtility.FromJson<SaveData_MultistageInputInteraction_V1>(rawSaveData);
			}
			catch
			{
			}
			if (saveData_MultistageInputInteraction_V != null)
			{
				return saveData_MultistageInputInteraction_V;
			}
		}
		return null;
	}

	protected virtual void StartedFromSnapshotWithData(SaveData_MultistageInputInteraction_V1 data)
	{
	}

	protected virtual SaveData_MultistageInputInteraction_V1 CreateSnapshotData()
	{
		return new SaveData_MultistageInputInteraction_V1();
	}

	public virtual string CreateSnapshot()
	{
		SaveData_MultistageInputInteraction_V1 saveData_MultistageInputInteraction_V = CreateSnapshotData();
		saveData_MultistageInputInteraction_V.S = GetCurrentStage();
		saveData_MultistageInputInteraction_V.E = m_IsInteractionEnabled;
		saveData_MultistageInputInteraction_V.CompletedAll = m_bHasCompletedAllStages;
		return JsonUtility.ToJson(saveData_MultistageInputInteraction_V);
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
		SaveData_MultistageInputInteraction_V1 saveData_MultistageInputInteraction_V = RetrieveSnapshotData(snapshotData_Base, m_SaveData.GetSaveData());
		if (saveData_MultistageInputInteraction_V != null)
		{
			int s = saveData_MultistageInputInteraction_V.S;
			if (s != -1)
			{
				RPC_SetStage(s, isSaveRestore: true);
			}
			RPC_EnableInteraction(saveData_MultistageInputInteraction_V.E);
			m_bHasCompletedAllStages = saveData_MultistageInputInteraction_V.CompletedAll;
			StartedFromSnapshotWithData(saveData_MultistageInputInteraction_V);
			if (m_bHasCompletedAllStages)
			{
				SetVisualForCompletedAllStages();
			}
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
				m_NetView.RPC("RPC_RequestStateResponce_No_MultistageInputInteraction", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_MultistageInputInteraction(PhotonMessageInfo info)
	{
		m_LoadError = "MultistageInputInteraction RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	protected virtual void Master_RPCLoadDataToClient(PhotonPlayer player)
	{
		m_NetView.RPC("RPC_Client_LoadMultistageInputBaseData", player, GetCurrentStage(), m_IsInteractionEnabled, m_bHasCompletedAllStages);
	}

	[PunRPC]
	public void RPC_Client_LoadMultistageInputBaseData(int stage, bool bInteractionEnabled, bool hasCompletedAllStages)
	{
		RPC_SetStage(stage, isSaveRestore: true);
		RPC_EnableInteraction(bInteractionEnabled);
		m_LoadState = LOADSTATE.Finished_OK;
		m_bHasCompletedAllStages = hasCompletedAllStages;
	}
}

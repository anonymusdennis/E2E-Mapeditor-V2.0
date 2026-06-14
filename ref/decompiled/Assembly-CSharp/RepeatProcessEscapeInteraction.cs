using System;
using NetworkLoadable;
using UnityEngine;

public class RepeatProcessEscapeInteraction : ConstructEndgameInteraction
{
	[Serializable]
	protected class SaveData_RepeatProcessEscapeInteraction_V1 : SaveData_MultistageInputInteraction_V1
	{
		public int C;

		public SaveData_RepeatProcessEscapeInteraction_V1()
		{
			m_Version = 1;
		}
	}

	[Header("Interaction Dialog")]
	public SpeechPODO m_MoreRepetitionNeededDialog;

	public SpeechPODO m_RepetitionsDoneDialog;

	private int m_Count;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		Gamer.OnCreate += GamerAddedEventHander;
		Gamer.OnDeleted += GamerRemovedEventHander;
		return result;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Gamer.OnCreate -= GamerAddedEventHander;
		Gamer.OnDeleted -= GamerRemovedEventHander;
	}

	private void GamerAddedEventHander(Gamer gamer)
	{
		OnNumberOfGamersChanged();
	}

	private void GamerRemovedEventHander()
	{
		OnNumberOfGamersChanged();
	}

	public override void ProgressToNextStage()
	{
		int targetCount = GetTargetCount();
		int currentStage = GetCurrentStage();
		int num = currentStage + 1;
		int finalStage = GetFinalStage();
		if (num == finalStage)
		{
			int num2 = IncrementCount();
			if (num2 < targetCount)
			{
				PlayDialogOnCharacter(m_LastInteractingCharacter, m_MoreRepetitionNeededDialog);
				num = 0;
			}
			else
			{
				PlayDialogOnCharacter(m_LastInteractingCharacter, m_RepetitionsDoneDialog);
			}
		}
		if (num != currentStage)
		{
			if (num <= finalStage)
			{
				SetStageRPC(num);
			}
			else
			{
				OnInteractedWithFinalStage(m_LastInteractingCharacter);
			}
		}
	}

	private void OnNumberOfGamersChanged()
	{
		if (T17NetManager.IsMasterClient)
		{
			int targetCount = GetTargetCount();
			int currentStage = GetCurrentStage();
			int finalStage = GetFinalStage();
			int num = currentStage;
			if (m_Count >= targetCount)
			{
				num = finalStage;
			}
			else if (currentStage == finalStage)
			{
				num = 0;
			}
			if (num != currentStage)
			{
				SetStageRPC(num, isSaveRestore: true);
			}
		}
	}

	private int GetTargetCount()
	{
		int gamerCount = Gamer.GetGamerCount();
		if (gamerCount > 1 && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
		{
			return 1;
		}
		return gamerCount;
	}

	private int IncrementCount()
	{
		int num = m_Count + 1;
		m_NetView.RPC("RPC_UpdateCount", NetTargets.All, num);
		return num;
	}

	[PunRPC]
	public void RPC_UpdateCount(int count)
	{
		m_Count = count;
	}

	protected override void Master_RPCLoadDataToClient(PhotonPlayer player)
	{
		m_NetView.RPC("RPC_Client_LoadRepeatProcessEscapeInteractionData", player, GetCurrentStage(), m_IsInteractionEnabled, m_Count);
	}

	[PunRPC]
	public void RPC_Client_LoadRepeatProcessEscapeInteractionData(int stage, bool bInteractionEnabled, int count)
	{
		RPC_SetStage(stage, isSaveRestore: true);
		RPC_EnableInteraction(bInteractionEnabled);
		RPC_UpdateCount(count);
		m_LoadState = LOADSTATE.Finished_OK;
	}

	public override string CreateSnapshot()
	{
		SaveData_RepeatProcessEscapeInteraction_V1 saveData_RepeatProcessEscapeInteraction_V = new SaveData_RepeatProcessEscapeInteraction_V1();
		saveData_RepeatProcessEscapeInteraction_V.S = GetCurrentStage();
		saveData_RepeatProcessEscapeInteraction_V.E = m_IsInteractionEnabled;
		saveData_RepeatProcessEscapeInteraction_V.C = m_Count;
		return JsonUtility.ToJson(saveData_RepeatProcessEscapeInteraction_V);
	}

	public override void StartedFromSnapshot()
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
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			SaveData_RepeatProcessEscapeInteraction_V1 saveData_RepeatProcessEscapeInteraction_V = null;
			try
			{
				saveData_RepeatProcessEscapeInteraction_V = JsonUtility.FromJson<SaveData_RepeatProcessEscapeInteraction_V1>(m_SaveData.GetSaveData());
			}
			catch
			{
			}
			if (saveData_RepeatProcessEscapeInteraction_V != null)
			{
				RPC_SetStage(saveData_RepeatProcessEscapeInteraction_V.S, isSaveRestore: true);
				RPC_EnableInteraction(saveData_RepeatProcessEscapeInteraction_V.E);
				RPC_UpdateCount(saveData_RepeatProcessEscapeInteraction_V.C);
			}
		}
	}
}

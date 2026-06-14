using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class MultiplayerPuzzle_Base : T17MonoBehaviour, IControlledUpdate, INetworkLoadable
{
	[Serializable]
	public class Solution
	{
		public MultiplayerPuzzle_Interaction[] interactions = new MultiplayerPuzzle_Interaction[0];
	}

	[Serializable]
	private class SaveDataInfo
	{
		public int activeSolution = -1;

		public int[] interactionViewIDs;

		public byte[][] interactionStates;
	}

	public List<Solution> m_Solutions = new List<Solution>();

	protected T17NetView m_NetView;

	protected int m_ActiveSolutionIndex = -1;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	private SaveDataRegister m_SaveData;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	private void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RegularPeriodic);
		}
		for (int i = 0; i < m_Solutions.Count; i++)
		{
			if (m_Solutions[i] == null)
			{
				continue;
			}
			MultiplayerPuzzle_Interaction[] interactions = m_Solutions[i].interactions;
			if (interactions != null && interactions.Length > 0)
			{
				for (int j = 0; j < interactions.Length; j++)
				{
					interactions[j] = null;
				}
			}
		}
		if (m_Solutions != null)
		{
			m_Solutions.Clear();
		}
		m_NetView = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RegularPeriodic);
		}
		OnPuzzleStateChanged(solved: false);
		return base.StartInit();
	}

	public virtual void ControlledUpdate()
	{
		UpdatePuzzle(UpdateManager.deltaTime);
	}

	public virtual void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	protected virtual void UpdatePuzzle(float deltaTime)
	{
	}

	public bool IsSolved()
	{
		return m_ActiveSolutionIndex >= 0;
	}

	public void OnInteractionStateChanged(MultiplayerPuzzle_Interaction interaction, bool valid)
	{
		EvaluatePuzzle();
	}

	protected virtual void EvaluatePuzzle()
	{
		int num = -1;
		for (int i = 0; i < m_Solutions.Count; i++)
		{
			if (m_Solutions[i] != null)
			{
				MultiplayerPuzzle_Interaction[] interactions = m_Solutions[i].interactions;
				if (interactions != null && interactions.Length > 0 && Array.TrueForAll(interactions, IsInteractionValid))
				{
					num = i;
					break;
				}
			}
		}
		if (num != m_ActiveSolutionIndex)
		{
			m_ActiveSolutionIndex = num;
			if (m_NetView != null)
			{
				m_NetView.RPC("RPC_SetPuzzleState", NetTargets.All, num);
			}
		}
	}

	[PunRPC]
	protected void RPC_SetPuzzleState(int solutionIndex, PhotonMessageInfo info)
	{
		m_ActiveSolutionIndex = solutionIndex;
		OnPuzzleStateChanged(solutionIndex != -1);
	}

	protected virtual void OnPuzzleStateChanged(bool solved)
	{
	}

	private static bool IsInteractionValid(MultiplayerPuzzle_Interaction interaction)
	{
		return interaction.IsValid();
	}

	private SaveDataInfo Serialize()
	{
		SaveDataInfo saveDataInfo = new SaveDataInfo();
		saveDataInfo.activeSolution = m_ActiveSolutionIndex;
		List<MultiplayerPuzzle_Interaction> list = new List<MultiplayerPuzzle_Interaction>();
		for (int i = 0; i < m_Solutions.Count; i++)
		{
			for (int j = 0; j < m_Solutions[i].interactions.Length; j++)
			{
				MultiplayerPuzzle_Interaction multiplayerPuzzle_Interaction = m_Solutions[i].interactions[j];
				if (multiplayerPuzzle_Interaction != null && !list.Contains(multiplayerPuzzle_Interaction))
				{
					list.Add(multiplayerPuzzle_Interaction);
				}
			}
		}
		int count = list.Count;
		saveDataInfo.interactionViewIDs = new int[count];
		saveDataInfo.interactionStates = new byte[count][];
		for (int k = 0; k < count; k++)
		{
			saveDataInfo.interactionViewIDs[k] = list[k].m_NetViewID.viewID;
			saveDataInfo.interactionStates[k] = list[k].GetSerializedInteractionState();
		}
		return saveDataInfo;
	}

	private bool Deserialize(SaveDataInfo data)
	{
		m_ActiveSolutionIndex = data.activeSolution;
		OnPuzzleStateChanged(data.activeSolution != -1);
		int num = Mathf.Min(data.interactionViewIDs.Length, data.interactionStates.Length);
		for (int i = 0; i < num; i++)
		{
			int viewID = data.interactionViewIDs[i];
			byte[] data2 = data.interactionStates[i];
			MultiplayerPuzzle_Interaction multiplayerPuzzle_Interaction = T17NetView.Find<MultiplayerPuzzle_Interaction>(viewID);
			if (multiplayerPuzzle_Interaction != null)
			{
				multiplayerPuzzle_Interaction.DeserializeStateData(data2);
			}
		}
		return true;
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
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			byte[] array = new byte[0];
			SaveDataInfo graph = Serialize();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, graph);
				array = memoryStream.ToArray();
			}
			m_NetView.RPC("RPC_SetWholePuzzleState", player, array);
		}
		else
		{
			m_NetView.RPC("RPC_RequestStateResponse_No", player);
		}
	}

	[PunRPC]
	public void RPC_SetWholePuzzleState(byte[] data, PhotonMessageInfo info)
	{
		SaveDataInfo saveDataInfo = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(data))
		{
			saveDataInfo = (SaveDataInfo)binaryFormatter.Deserialize(serializationStream);
		}
		if (saveDataInfo != null)
		{
			Deserialize(saveDataInfo);
		}
		m_LoadState = LOADSTATE.Finished_OK;
		m_LoadError = string.Empty;
	}

	[PunRPC]
	private void RPC_RequestStateResponse_No(PhotonMessageInfo info)
	{
		m_LoadError = "MultiplayerPuzzle RPC_RequestStateResponse_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	public string CreateSnapshot()
	{
		SaveDataInfo graph = Serialize();
		string text = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, graph);
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public void StartedFromSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		SaveDataInfo saveDataInfo = null;
		try
		{
			string saveData = m_SaveData.GetSaveData();
			byte[] buffer = Convert.FromBase64String(saveData);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream serializationStream = new MemoryStream(buffer);
			saveDataInfo = binaryFormatter.Deserialize(serializationStream) as SaveDataInfo;
		}
		catch
		{
		}
		if (saveDataInfo != null)
		{
			Deserialize(saveDataInfo);
		}
	}
}

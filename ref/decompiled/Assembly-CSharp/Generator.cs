using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using NetworkLoadable;
using UnityEngine;

public class Generator : T17MonoBehaviour, INetworkLoadable, Saveable
{
	[Serializable]
	protected class SaveData_Generator_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public float T;

		public SaveData_Generator_V1()
		{
			m_Version = 1;
		}
	}

	public T17NetView m_NetView;

	public Animator m_Animator;

	public List<ParticleSystem> m_Particles;

	public bool m_Disabled;

	public GeneratorInteraction m_Switch;

	public float m_InactiveTime = 30f;

	private float m_InactiveTimer;

	private int m_AnimOnHash = -1;

	private SaveDataRegister m_SaveData;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	protected override void Awake()
	{
		base.Awake();
		m_AnimOnHash = Animator.StringToHash("On");
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_Switch = null;
		if (m_Particles != null)
		{
			m_Particles.Clear();
		}
		m_Animator = null;
		m_NetView = null;
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: false);
		if (m_Switch != null)
		{
			m_Switch.SetGenerator(this);
		}
		UpdateGeneratorState(init: true);
		return base.StartInit();
	}

	public void Update()
	{
		bool flag = false;
		if (m_InactiveTimer > 0f)
		{
			flag = true;
			m_InactiveTimer -= UpdateManager.deltaTime;
			if (m_InactiveTimer < 0f)
			{
				flag = false;
			}
		}
		if (flag != m_Disabled)
		{
			m_Disabled = flag;
			UpdateGeneratorState();
		}
	}

	public void DisableGenerator()
	{
		m_NetView.RPC("RPC_DisableGenerator", NetTargets.MasterClient);
	}

	private void UpdateGeneratorState(bool init = false)
	{
		bool flag = GeneratorActive();
		UpdateVisuals(flag);
		if (flag)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Generator_Loop, base.gameObject);
		}
		else
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Generator_Loop, base.gameObject);
		}
		PrisonPowerManager.GetInstance().OnGeneratorStateChanged(this);
		if (T17NetManager.IsMasterClient && !init)
		{
			m_InactiveTimer = ((!flag) ? m_InactiveTime : 0f);
			m_NetView.RPC("RPC_SetGeneratorInactiveTimer", NetTargets.Others, m_InactiveTimer);
		}
	}

	public bool GeneratorActive()
	{
		return !m_Disabled;
	}

	private void UpdateVisuals(bool on)
	{
		m_Animator.SetBool(m_AnimOnHash, on);
		if (m_Switch != null)
		{
			m_Switch.SetState(on);
		}
		if (m_Particles == null)
		{
			return;
		}
		for (int i = 0; i < m_Particles.Count; i++)
		{
			if (m_Particles[i] != null)
			{
				if (on)
				{
					m_Particles[i].Play();
				}
				else
				{
					m_Particles[i].Stop();
				}
			}
		}
	}

	[PunRPC]
	private void RPC_DisableGenerator(PhotonMessageInfo info)
	{
		if (T17NetManager.IsMasterClient && GeneratorActive())
		{
			m_InactiveTimer = m_InactiveTime;
		}
	}

	[PunRPC]
	private void RPC_SetGeneratorInactiveTimer(float timer, PhotonMessageInfo info)
	{
		m_InactiveTimer = timer;
	}

	public string CreateSnapshot()
	{
		SaveData_Generator_V1 saveData_Generator_V = new SaveData_Generator_V1();
		saveData_Generator_V.T = m_InactiveTimer;
		return JsonUtility.ToJson(saveData_Generator_V);
	}

	public void StartedFromSnapshot()
	{
		if (m_SaveData == null)
		{
			return;
		}
		string saveData = m_SaveData.GetSaveData();
		if (string.IsNullOrEmpty(saveData))
		{
			return;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(saveData);
		}
		catch
		{
		}
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			SaveData_Generator_V1 saveData_Generator_V = null;
			try
			{
				saveData_Generator_V = JsonUtility.FromJson<SaveData_Generator_V1>(saveData);
			}
			catch
			{
			}
			if (saveData_Generator_V != null)
			{
				m_InactiveTimer = saveData_Generator_V.T;
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
				m_NetView.RPC("RPC_RequestStateResponce_Yes_LoadGenerator", player, m_InactiveTimer);
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No_LoadGenerator", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_LoadGenerator(PhotonMessageInfo info)
	{
		m_LoadError = "Generator RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_LoadGenerator(float timer, PhotonMessageInfo info)
	{
		RPC_SetGeneratorInactiveTimer(timer, info);
		m_LoadState = LOADSTATE.Finished_OK;
	}
}

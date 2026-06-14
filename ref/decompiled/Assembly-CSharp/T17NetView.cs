using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using ExtensionMethods;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class T17NetView : Photon.MonoBehaviour
{
	public class RPCWrapper
	{
		public int ID = -1;

		public NetTargets NetTarget = NetTargets.MasterClient;

		public PhotonPlayer PlayerTarget;

		public float ResendTime;

		public string MethodName;

		public object[] Parameters;

		private static int QuestionIDCount;

		public RPCWrapper()
		{
		}

		public RPCWrapper(byte[] input)
		{
			int offset = 0;
			int value = 0;
			int value2 = 0;
			Protocol.Deserialize(out ID, input, ref offset);
			Protocol.Deserialize(out value, input, ref offset);
			Protocol.Deserialize(out value2, input, ref offset);
			if (value > 0)
			{
				byte[] array = new byte[value];
				Array.Copy(input, offset, array, 0, value);
				MethodName = (string)Protocol.Deserialize(array);
				offset += value;
			}
			if (value2 > 0)
			{
				byte[] array2 = new byte[value2];
				Array.Copy(input, offset, array2, 0, value2);
				Parameters = (object[])Protocol.Deserialize(array2);
				offset += value2;
			}
		}

		public RPCWrapper(string method, int id, object[] parameters)
		{
			MethodName = method;
			Parameters = parameters;
			ID = id;
		}

		public RPCWrapper(string method, NetTargets target, int netViewID, object[] parameters)
		{
			ID = (netViewID << 20) | (QuestionIDCount++ & 0xFFFFF);
			ResetTime();
			MethodName = method;
			NetTarget = target;
			SetupParams(parameters);
		}

		public RPCWrapper(string method, PhotonPlayer target, int netViewID, object[] parameters)
		{
			ID = (netViewID << 20) | (QuestionIDCount++ & 0xFFFFF);
			ResetTime();
			MethodName = method;
			PlayerTarget = target;
			SetupParams(parameters);
		}

		public byte[] ToBytes()
		{
			byte[] array = null;
			lock (T17NetManager.memRPCWrapper)
			{
				byte[] memRPCWrapper = T17NetManager.memRPCWrapper;
				int targetOffset = 0;
				byte[] array2 = Protocol.Serialize(MethodName);
				byte[] array3 = Protocol.Serialize(Parameters);
				Protocol.Serialize(ID, memRPCWrapper, ref targetOffset);
				Protocol.Serialize(array2.Length, memRPCWrapper, ref targetOffset);
				Protocol.Serialize(array3.Length, memRPCWrapper, ref targetOffset);
				if (array2.Length > 0)
				{
					Array.Copy(array2, 0, memRPCWrapper, targetOffset, array2.Length);
					targetOffset += array2.Length;
				}
				if (array3.Length > 0)
				{
					Array.Copy(array3, 0, memRPCWrapper, targetOffset, array3.Length);
					targetOffset += array3.Length;
				}
				array = new byte[targetOffset];
				Array.Copy(memRPCWrapper, array, targetOffset);
				return array;
			}
		}

		private void SetupParams(object[] parameters)
		{
			int num = 1;
			if (parameters != null)
			{
				num += parameters.Length;
			}
			Parameters = new object[num];
			Parameters[0] = ID;
			for (int i = 1; i < num; i++)
			{
				Parameters[i] = parameters[i - 1];
			}
		}

		public string GetMethodName()
		{
			return MethodName;
		}

		public PhotonPlayer GetPlayerTarget()
		{
			return PlayerTarget;
		}

		public NetTargets GetNetTarget()
		{
			return NetTarget;
		}

		public object[] GetParameters()
		{
			return Parameters;
		}

		public int GetID()
		{
			return ID;
		}

		public float GetResendTime()
		{
			return ResendTime;
		}

		public void ResetTime()
		{
			ResendTime = Time.time + 5f;
		}

		public void SetTime(float value)
		{
			ResendTime = value;
		}
	}

	public const float ShowInfoOfPlayerCharacterSize = 0.25f;

	private PhotonView m_PhotonView;

	private bool m_hookedUp;

	private string m_offlineOwnerName = string.Empty;

	private List<RPCWrapper> m_WrappedQuestions = new List<RPCWrapper>();

	private List<KeyValuePair<int, PhotonPlayer>> m_PendingResponses = new List<KeyValuePair<int, PhotonPlayer>>();

	public const float fResentDelayTime = 5f;

	public static List<T17NetView> QuestionNetViews = new List<T17NetView>();

	private List<int> m_ResuableIntList = new List<int>();

	public T17NetShowInfo ShowInfoOfPlayer { get; private set; }

	public int viewID
	{
		get
		{
			int result = 0;
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				result = m_PhotonView.viewID;
			}
			return result;
		}
		set
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetViewID))
			{
			}
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				m_PhotonView.viewID = value;
			}
		}
	}

	public int ownerId
	{
		get
		{
			int result = 0;
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				result = m_PhotonView.ownerId;
			}
			return result;
		}
		set
		{
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				m_PhotonView.ownerId = value;
			}
		}
	}

	public bool isMine
	{
		get
		{
			if (m_PhotonView != null)
			{
				if (m_PhotonView.ownerId == PhotonNetwork.player.ID)
				{
					return true;
				}
				return PhotonNetwork.isMasterClient && !m_PhotonView.isOwnerActive;
			}
			return false;
		}
	}

	public bool isSceneView
	{
		get
		{
			bool result = false;
			if (m_PhotonView != null)
			{
				result = m_PhotonView.isSceneView;
			}
			return result;
		}
	}

	public bool isOwnerActive
	{
		get
		{
			bool result = false;
			if (m_PhotonView != null)
			{
				result = m_PhotonView.isOwnerActive;
			}
			return result;
		}
	}

	public bool isOwnerTheMasterClient
	{
		get
		{
			bool result = false;
			if (m_PhotonView != null)
			{
				PhotonPlayer owner = m_PhotonView.owner;
				if (owner != null)
				{
					result = PhotonNetwork.masterClient == owner;
				}
			}
			return result;
		}
	}

	public string ownerInfo
	{
		get
		{
			string result = string.Empty;
			if (m_PhotonView != null)
			{
				if (m_PhotonView.isSceneView)
				{
					result = "Scene";
				}
				else
				{
					PhotonPlayer owner = m_PhotonView.owner;
					string text = ((owner == null) ? "<no PhotonPlayer found>" : owner.NickName);
					if (string.IsNullOrEmpty(text))
					{
						text = "<no playername set>";
					}
					result = $"[{m_PhotonView.ownerId}] {text}";
				}
			}
			return result;
		}
	}

	public string ownerName
	{
		get
		{
			string result = string.Empty;
			if (m_PhotonView != null)
			{
				PhotonPlayer owner = m_PhotonView.owner;
				if (owner != null && owner.NickName != null)
				{
					result = ((!T17NetManager.NetOnlineMode) ? m_offlineOwnerName : owner.NickName);
				}
			}
			return result;
		}
		set
		{
			if (!(m_PhotonView != null))
			{
				return;
			}
			PhotonPlayer owner = m_PhotonView.owner;
			if (owner != null && isMine)
			{
				if (T17NetManager.NetOnlineMode)
				{
					owner.NickName = value;
				}
				else
				{
					m_offlineOwnerName = value;
				}
			}
		}
	}

	public T17NetViewSynchronization NetViewSynchronization
	{
		get
		{
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				return (T17NetViewSynchronization)m_PhotonView.synchronization;
			}
			return T17NetViewSynchronization.Off;
		}
		set
		{
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				m_PhotonView.synchronization = (ViewSynchronization)value;
			}
			if (PhotonNetwork.networkingPeer != null)
			{
				if (value != 0)
				{
					PhotonNetwork.networkingPeer.RegisterPhotonView_ToSerialize(m_PhotonView);
				}
				else
				{
					PhotonNetwork.networkingPeer.UnregisterPhotonView_ToSerialize(m_PhotonView);
				}
			}
		}
	}

	public int CreatorActorNr
	{
		get
		{
			int result = 0;
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				result = m_PhotonView.CreatorActorNr;
			}
			return result;
		}
	}

	public int OwnerActorNr
	{
		get
		{
			int result = 0;
			if (m_PhotonView == null && !m_hookedUp)
			{
				HookUp();
			}
			if (m_PhotonView != null)
			{
				result = m_PhotonView.OwnerActorNr;
			}
			return result;
		}
	}

	public void Awake()
	{
		if (!m_hookedUp)
		{
			HookUp();
		}
	}

	protected virtual void OnDestroy()
	{
		T17NetManager.OnPhotonPlayerDisconnectedEvent -= OnPhotonPlayerDisconnected;
		T17NetManager.OnNewMasterClient -= OnNewMasterClient;
		T17NetManager.OnLeftRoomEvent -= OnLeftRoomEvent;
		if (PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.UnregisterPhotonView_ToSerialize(m_PhotonView);
		}
		if (m_WrappedQuestions.Count > 0)
		{
			while (m_WrappedQuestions.Count > 0)
			{
				m_WrappedQuestions.RemoveAt(0);
			}
			m_WrappedQuestions.Clear();
			QuestionNetViews.Remove(this);
		}
		ShowInfoOfPlayer = null;
		if (m_PhotonView != null)
		{
			m_PhotonView.ClearRpcMonoBehaviourCache();
		}
		m_PhotonView = null;
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		List<RPCWrapper> list = new List<RPCWrapper>();
		for (int i = 0; i < m_WrappedQuestions.Count; i++)
		{
			RPCWrapper rPCWrapper = m_WrappedQuestions[i];
			if (rPCWrapper.GetPlayerTarget() == player)
			{
				list.Add(rPCWrapper);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			m_WrappedQuestions.Remove(list[j]);
		}
	}

	public void HookUp()
	{
		m_PhotonView = GetComponent<PhotonView>();
		ShowInfoOfPlayer = GetComponent<T17NetShowInfo>();
		m_hookedUp = true;
		if (m_PhotonView == null)
		{
			Debug.LogError($"NetView: Failed to locate PhotonView on Component {base.name} of type {GetType().ToString()}", this);
		}
		if (ShowInfoOfPlayer != null)
		{
			ShowInfoOfPlayer.enabled = false;
		}
		NetViewSynchronization = T17NetViewSynchronization.Off;
		T17NetManager.OnNewMasterClient += OnNewMasterClient;
		T17NetManager.OnPhotonPlayerDisconnectedEvent += OnPhotonPlayerDisconnected;
		T17NetManager.OnLeftRoomEvent += OnLeftRoomEvent;
	}

	public void OnLeftRoomEvent()
	{
		if (!GlobalStart.GetInstance().IsWithinLevel())
		{
			if (m_WrappedQuestions.Count > 0)
			{
				QuestionNetViews.Remove(this);
			}
			m_WrappedQuestions.Clear();
		}
	}

	private void RemoveRPCs()
	{
		if (m_PhotonView != null)
		{
			PhotonNetwork.RemoveRPCs(m_PhotonView);
		}
	}

	public static T Find<T>(int viewID) where T : Component
	{
		if (viewID == -1 || viewID == 0)
		{
			return (T)null;
		}
		PhotonView photonView = PhotonView.Find(viewID);
		if (photonView != null)
		{
			return photonView.GetComponent<T>();
		}
		return (T)null;
	}

	private string parametersToString(params object[] parameters)
	{
		StringBuilder stringBuilder = new StringBuilder("{ ", 256);
		if (parameters != null)
		{
			int num = parameters.Length;
			if (num > 0)
			{
				stringBuilder.AppendFormat("{0} ", parameters[0].ToString());
				for (int i = 1; i < num; i++)
				{
					stringBuilder.AppendFormat(", {0}", parameters[i].ToString());
				}
			}
		}
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}

	public void ProcessQuestions(float fRealTime)
	{
		for (int i = 0; i < m_WrappedQuestions.Count; i++)
		{
			RPCWrapper rPCWrapper = m_WrappedQuestions[i];
			if (fRealTime > rPCWrapper.GetResendTime())
			{
				rPCWrapper.ResetTime();
				if (rPCWrapper.GetPlayerTarget() != null)
				{
					RPC("RPC_ReceiveQuestion", rPCWrapper.GetPlayerTarget(), rPCWrapper.ToBytes());
				}
				else
				{
					RPC("RPC_ReceiveQuestion", rPCWrapper.GetNetTarget(), rPCWrapper.ToBytes());
				}
			}
		}
	}

	public void OnNewMasterClient(PhotonPlayer newMasterClient)
	{
		for (int num = m_WrappedQuestions.Count - 1; num >= 0; num--)
		{
			m_WrappedQuestions[num].SetTime(0f);
		}
	}

	public void RPCQuestion(string methodName, PhotonPlayer target, params object[] parameters)
	{
		if (m_WrappedQuestions.Count == 0)
		{
			QuestionNetViews.Add(this);
		}
		RPCWrapper rPCWrapper = new RPCWrapper(methodName, target, viewID, parameters);
		m_WrappedQuestions.Add(rPCWrapper);
		RPC("RPC_ReceiveQuestion", target, rPCWrapper.ToBytes());
	}

	public void RPCQuestion(string methodName, NetTargets target, params object[] parameters)
	{
		if (m_WrappedQuestions.Count == 0)
		{
			QuestionNetViews.Add(this);
		}
		RPCWrapper rPCWrapper = new RPCWrapper(methodName, target, viewID, parameters);
		m_WrappedQuestions.Add(rPCWrapper);
		RPC("RPC_ReceiveQuestion", target, rPCWrapper.ToBytes());
	}

	public void RPCResponse(string methodName, int RPCID, params object[] parameters)
	{
		PhotonPlayer photonPlayer = null;
		for (int num = m_PendingResponses.Count - 1; num >= 0; num--)
		{
			KeyValuePair<int, PhotonPlayer> keyValuePair = m_PendingResponses[num];
			if (keyValuePair.Key == RPCID)
			{
				photonPlayer = keyValuePair.Value;
				m_PendingResponses.RemoveAt(num);
				break;
			}
		}
		if (photonPlayer != null)
		{
			RPCWrapper rPCWrapper = new RPCWrapper(methodName, RPCID, parameters);
			RPC("RPC_ReceiveResponse", photonPlayer, rPCWrapper.ToBytes());
		}
	}

	[PunRPC]
	private void RPC_ReceiveQuestion(byte[] q, PhotonMessageInfo info)
	{
		RPCWrapper rPCWrapper = new RPCWrapper(q);
		m_PendingResponses.Add(new KeyValuePair<int, PhotonPlayer>(rPCWrapper.GetID(), info.sender));
		m_PhotonView.RPC(rPCWrapper.GetMethodName(), PhotonNetwork.player, rPCWrapper.GetParameters());
	}

	[PunRPC]
	public void RPC_ReceiveResponse(byte[] bytes, PhotonMessageInfo info)
	{
		RPCWrapper rPCWrapper = new RPCWrapper(bytes);
		string methodName = rPCWrapper.GetMethodName();
		if (!string.IsNullOrEmpty(methodName))
		{
			RPC(methodName, PhotonNetwork.player, rPCWrapper.GetParameters());
		}
		int iD = rPCWrapper.GetID();
		int count = m_WrappedQuestions.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_WrappedQuestions[i].GetID() == iD)
			{
				m_WrappedQuestions.RemoveAt(i);
				if (m_WrappedQuestions.Count == 0)
				{
					QuestionNetViews.Remove(this);
				}
				break;
			}
		}
	}

	public void GameplayRPC(string methodName, NetTargets target, params object[] parameters)
	{
		if (target == NetTargets.MasterClient || T17NetLoadSync.Instance.GetClientsReadyForGameplayRPC(ref m_ResuableIntList))
		{
			RPC(methodName, target, parameters);
		}
		else if (target == NetTargets.All || target == NetTargets.Others)
		{
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			foreach (PhotonPlayer photonPlayer in playerList)
			{
				if ((!photonPlayer.IsLocal || target != NetTargets.Others) && (photonPlayer.IsLocal || !m_ResuableIntList.Contains(photonPlayer.ID)))
				{
					RPC(methodName, photonPlayer, parameters);
				}
			}
		}
		else
		{
			RPC(methodName, target, parameters);
		}
	}

	public void PostLevelLoadRPC(string methodName, NetTargets target, params object[] parameters)
	{
		if (target == NetTargets.MasterClient || T17NetLoadSync.Instance.GetClientsPastLoadedLevel(ref m_ResuableIntList))
		{
			RPC(methodName, target, parameters);
		}
		else if (target == NetTargets.All || target == NetTargets.Others)
		{
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			foreach (PhotonPlayer photonPlayer in playerList)
			{
				if ((!photonPlayer.IsLocal || target != NetTargets.Others) && (photonPlayer.IsLocal || !m_ResuableIntList.Contains(photonPlayer.ID)))
				{
					RPC(methodName, photonPlayer, parameters);
				}
			}
		}
		else
		{
			RPC(methodName, target, parameters);
		}
	}

	public void RPC(string methodName, NetTargets target, params object[] parameters)
	{
		if (m_PhotonView != null)
		{
			m_PhotonView.RpcSecure(methodName, (PhotonTargets)target, encrypt: true, parameters);
		}
	}

	public void RPC(string methodName, T17NetView target, params object[] parameters)
	{
		if (m_PhotonView != null)
		{
			if (target.isOwnerActive && null != target.photonView && target.photonView.owner != null)
			{
				m_PhotonView.RpcSecure(methodName, target.photonView.owner, encrypt: true, parameters);
			}
			else
			{
				RPC(methodName, NetTargets.MasterClient, parameters);
			}
		}
	}

	public void RPC(string methodName, PhotonPlayer photonPlayer, params object[] parameters)
	{
		if (m_PhotonView != null && photonPlayer != null)
		{
			m_PhotonView.RpcSecure(methodName, photonPlayer, encrypt: true, parameters);
		}
	}

	public void OnExecuteRPC(object[] methodNameAndParameters)
	{
	}

	public void EnsureRegistered()
	{
		if (m_PhotonView == null && !m_hookedUp)
		{
			HookUp();
		}
		if (m_PhotonView != null)
		{
			m_PhotonView.EnsureRegistered();
		}
	}

	public void EnsureUnregistered()
	{
		if (m_PhotonView == null && !m_hookedUp)
		{
			HookUp();
		}
		if (m_PhotonView != null)
		{
			m_PhotonView.EnsureUnregistered();
		}
	}

	public static void EnsureInactiveViewsAreRegistered()
	{
		PhotonNetwork.ResetLastUsedViewSubId();
		T17NetView[] array = Resources.FindObjectsOfTypeAll<T17NetView>();
		foreach (T17NetView t17NetView in array)
		{
			GameObject gameObject = t17NetView.gameObject;
			if (gameObject != null && gameObject.hideFlags != HideFlags.NotEditable && gameObject.hideFlags != HideFlags.HideAndDontSave)
			{
				t17NetView.EnsureRegistered();
			}
		}
	}

	public static void EnsureInactiveViewsAreUnregistered()
	{
		T17NetView[] array = Resources.FindObjectsOfTypeAll<T17NetView>();
		foreach (T17NetView t17NetView in array)
		{
			GameObject gameObject = t17NetView.gameObject;
			if (gameObject != null && gameObject.hideFlags != HideFlags.NotEditable && gameObject.hideFlags != HideFlags.HideAndDontSave)
			{
				if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonViewList))
				{
				}
				t17NetView.EnsureUnregistered();
			}
		}
	}

	public void AddToObservedComponents(Component c)
	{
		if (m_PhotonView == null && !m_hookedUp)
		{
			HookUp();
		}
		if (!(m_PhotonView != null))
		{
			return;
		}
		if (m_PhotonView.ObservedComponents == null)
		{
			m_PhotonView.ObservedComponents = new List<Component>();
		}
		if (m_PhotonView.ObservedComponents != null)
		{
			if (m_PhotonView.ObservedComponents.Count > 0 && m_PhotonView.ObservedComponents[0] == null)
			{
				m_PhotonView.ObservedComponents[0] = c;
			}
			else
			{
				m_PhotonView.ObservedComponents.Add(c);
			}
		}
	}

	public void RemoveObservedComponent(Component c)
	{
		if (m_PhotonView != null && m_PhotonView.ObservedComponents != null && m_PhotonView.ObservedComponents.Count > 0)
		{
			m_PhotonView.ObservedComponents.Remove(c);
		}
	}

	internal void RequestOwnership()
	{
		if (m_PhotonView == null && !m_hookedUp)
		{
			HookUp();
		}
		if (m_PhotonView != null && !m_PhotonView.isMine)
		{
			m_PhotonView.RequestOwnership();
		}
	}

	public static int NetViewIDToOwnerID(int netViewID)
	{
		return netViewID / PhotonNetwork.MAX_VIEW_IDS;
	}

	internal void TransferOwnership(int newOwnerId)
	{
		if (m_PhotonView == null && !m_hookedUp)
		{
			HookUp();
		}
		if (m_PhotonView != null && m_PhotonView.OwnerActorNr != newOwnerId)
		{
			m_PhotonView.TransferOwnership(newOwnerId);
		}
	}

	public static bool ReceiveNext<T>(ref PhotonStream stream, ref T value)
	{
		object obj = null;
		obj = stream.ReceiveNext();
		if (obj != null)
		{
			value = (T)obj;
			return true;
		}
		return false;
	}
}

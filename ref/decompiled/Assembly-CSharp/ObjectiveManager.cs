using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour, Saveable
{
	public struct PlayerAndQuestTrees
	{
		public int CurrentEvaluationIndex;

		public Player AcceptingPlayer;

		public List<ObjectiveTree> ObjectiveTrees;

		public ObjectiveTree ObjectiveTreeToTrack;
	}

	public struct RuckusObservingData
	{
		public RoomBlob.eLocation Location;

		public Routines BaseRoutineType;

		public RoutineSubTypes SubRoutineType;

		public Player PlayerToWatch;

		public PhotonPlayer Requester;
	}

	public enum ObjectiveManState
	{
		Idle = -1,
		Saving,
		Evaluating
	}

	public delegate void ObjectiveManagerRuckusEvent(bool succesfull, RoomBlob.eLocation locationToObserve, int playerViewID, Routines BaseRoutineType, RoutineSubTypes SubRoutineType);

	private class PrisonObjectives
	{
		public Player m_Player;

		public List<ObjectiveTree> m_Trees = new List<ObjectiveTree>();

		public PrisonObjectives(Player player)
		{
			m_Player = player;
		}

		public bool AddTree(ObjectiveTree tree)
		{
			if (!m_Trees.Contains(tree))
			{
				tree.m_bShowInJournal = false;
				tree.m_bShowTrackingPins = false;
				tree.OnObjectiveTreeCompleted = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(tree.OnObjectiveTreeCompleted, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCompleted));
				tree.OnObjectiveTreeCanceled = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(tree.OnObjectiveTreeCanceled, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeCanceled));
				tree.OnObjectiveTreeFailed = (ObjectiveTree.OnObjectiveTreeEvent)Delegate.Combine(tree.OnObjectiveTreeFailed, new ObjectiveTree.OnObjectiveTreeEvent(OnObjectiveTreeFailed));
				m_Trees.Add(tree);
				return true;
			}
			return false;
		}

		public void RemoveTree(ObjectiveTree tree)
		{
			int num = m_Trees.IndexOf(tree);
			if (num >= 0)
			{
				if (tree.GetObjectiveStatus != ObjectiveStatus.Done)
				{
					tree.EndTreeEarly(isTreeFailed: false);
				}
				m_Trees.RemoveAt(num);
			}
		}

		public void RemoveAllTrees()
		{
			for (int i = 0; i < m_Trees.Count; i++)
			{
				if (m_Trees[i] != null && m_Trees[i].GetObjectiveStatus != ObjectiveStatus.Done)
				{
					m_Trees[i].EndTreeEarly(isTreeFailed: false);
				}
			}
			m_Trees.Clear();
		}

		private void OnObjectiveTreeCompleted(ObjectiveTree tree)
		{
			RemoveTree(tree);
		}

		private void OnObjectiveTreeCanceled(ObjectiveTree tree)
		{
			RemoveTree(tree);
		}

		private void OnObjectiveTreeFailed(ObjectiveTree tree)
		{
			RemoveTree(tree);
		}
	}

	[Serializable]
	private class SaveData_ObjectiveManager_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public List<string> LPQ = new List<string>();

		public SaveData_ObjectiveManager_V1()
		{
			m_Version = 1;
		}
	}

	private static ObjectiveManager m_sInstance;

	public TextAsset m_PrisonObjectiveData;

	public SpriteAnimation m_QuestTargetAnimation = new SpriteAnimation();

	public ObjectiveManagerRuckusEvent OnRuckusEventHappened;

	private List<PlayerAndQuestTrees> m_PerPlayerObjectiveTrees = new List<PlayerAndQuestTrees>();

	private List<RuckusObservingData> m_RuckusListenRequesters = new List<RuckusObservingData>();

	private ObjectiveManState m_CurrentState = ObjectiveManState.Idle;

	private bool _snapshotIsReady;

	private Dictionary<Player, PrisonObjectives> m_PlayerPrisonObjectives = new Dictionary<Player, PrisonObjectives>();

	private SaveDataRegister m_SaveData;

	private T17NetView m_NetView;

	private float m_EvaluationTimeOut = 0.1f;

	private float m_ElapsedTimeOut;

	private Dictionary<TextAsset, JObject> m_JsonMap = new Dictionary<TextAsset, JObject>();

	public static bool SnapshotIsReady => m_sInstance._snapshotIsReady;

	public static ObjectiveManager GetInstance()
	{
		return m_sInstance;
	}

	private void Awake()
	{
		if (m_sInstance != null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		m_sInstance = this;
		m_NetView = GetComponent<T17NetView>();
		_snapshotIsReady = false;
	}

	public void Init(int playerCount)
	{
		if (playerCount > 0)
		{
			m_PerPlayerObjectiveTrees = new List<PlayerAndQuestTrees>(playerCount);
		}
	}

	protected virtual void OnDestroy()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers != null && allPlayers.Count > 0)
		{
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (allPlayers[i] != null)
				{
					CleanupPrisonObjectives(allPlayers[i]);
				}
			}
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		if (m_sInstance == this)
		{
			m_sInstance = null;
		}
		m_NetView = null;
	}

	private void Update()
	{
		switch (m_CurrentState)
		{
		case ObjectiveManState.Idle:
			break;
		case ObjectiveManState.Saving:
			break;
		case ObjectiveManState.Evaluating:
		{
			m_ElapsedTimeOut += UpdateManager.deltaTime;
			if (!(m_ElapsedTimeOut >= m_EvaluationTimeOut))
			{
				break;
			}
			m_ElapsedTimeOut = 0f;
			if (m_PerPlayerObjectiveTrees == null || m_PerPlayerObjectiveTrees.Count <= 0)
			{
				break;
			}
			for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
			{
				PlayerAndQuestTrees value = m_PerPlayerObjectiveTrees[i];
				if (value.ObjectiveTrees.Count <= 0)
				{
					continue;
				}
				if (EvaluateTree(value.ObjectiveTrees[value.CurrentEvaluationIndex]))
				{
					ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(value.AcceptingPlayer.m_PlayerCameraManagerBindingID);
					bool flag = false;
					if (playerObjectiveHUD != null)
					{
						flag = playerObjectiveHUD.IsTreeCurrentlyTracked(value.ObjectiveTrees[value.CurrentEvaluationIndex]);
					}
					value.ObjectiveTrees.RemoveAt(value.CurrentEvaluationIndex);
					if (value.ObjectiveTrees.Count > 0 && flag)
					{
						playerObjectiveHUD.SetObjectiveTreeToTrack(value.ObjectiveTrees[0], force: true);
					}
				}
				if (value.CurrentEvaluationIndex >= value.ObjectiveTrees.Count - 1)
				{
					value.CurrentEvaluationIndex = 0;
				}
				else
				{
					value.CurrentEvaluationIndex++;
				}
				m_PerPlayerObjectiveTrees[i] = value;
			}
			break;
		}
		}
	}

	public void RegisterToSaveSystem()
	{
		m_SaveData = new SaveDataRegister(this, 2147483637, bIsMajorManagerComponent: true, 12);
	}

	public void StartEvaluating()
	{
		m_CurrentState = ObjectiveManState.Evaluating;
	}

	public void PauseEvaluating()
	{
		m_CurrentState = ObjectiveManState.Idle;
	}

	public void AddActiveTrees(Player player, List<ObjectiveTree> activeTrees)
	{
		if (m_PerPlayerObjectiveTrees != null && m_PerPlayerObjectiveTrees.Count > 0)
		{
			for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
			{
				if (m_PerPlayerObjectiveTrees[i].AcceptingPlayer == player)
				{
					m_PerPlayerObjectiveTrees[i].ObjectiveTrees.AddRange(activeTrees);
					return;
				}
			}
		}
		PlayerAndQuestTrees item = default(PlayerAndQuestTrees);
		item.AcceptingPlayer = player;
		item.ObjectiveTrees = new List<ObjectiveTree>(activeTrees);
		item.CurrentEvaluationIndex = 0;
		item.ObjectiveTreeToTrack = null;
		m_PerPlayerObjectiveTrees.Add(item);
	}

	public void GetActiveTrees(Player player, out List<ObjectiveTree> activeTrees)
	{
		GetTrees(player, out activeTrees, bGetActiveTrees: true);
	}

	public void GetTrees(Player player, out List<ObjectiveTree> trees, bool bGetActiveTrees)
	{
		trees = new List<ObjectiveTree>();
		if (m_PerPlayerObjectiveTrees == null || m_PerPlayerObjectiveTrees.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
		{
			if (!(m_PerPlayerObjectiveTrees[i].AcceptingPlayer == player))
			{
				continue;
			}
			for (int j = 0; j < m_PerPlayerObjectiveTrees[i].ObjectiveTrees.Count; j++)
			{
				if (!bGetActiveTrees || m_PerPlayerObjectiveTrees[i].ObjectiveTrees[j].GetObjectiveStatus == ObjectiveStatus.InComplete)
				{
					trees.Add(m_PerPlayerObjectiveTrees[i].ObjectiveTrees[j]);
				}
			}
			break;
		}
	}

	public bool RemoveActiveTree(Player player, int treeID)
	{
		if (m_PerPlayerObjectiveTrees != null && m_PerPlayerObjectiveTrees.Count > 0)
		{
			for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
			{
				PlayerAndQuestTrees playerAndQuestTrees = m_PerPlayerObjectiveTrees[i];
				if (!(playerAndQuestTrees.AcceptingPlayer == player))
				{
					continue;
				}
				for (int j = 0; j < playerAndQuestTrees.ObjectiveTrees.Count; j++)
				{
					if (playerAndQuestTrees.ObjectiveTrees[j].ActiveTreeID == treeID)
					{
						playerAndQuestTrees.ObjectiveTrees.RemoveAt(j);
						return true;
					}
				}
			}
		}
		return false;
	}

	public void RemoveAllTrees(Player player)
	{
		if (m_PerPlayerObjectiveTrees == null || m_PerPlayerObjectiveTrees.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
		{
			PlayerAndQuestTrees playerAndQuestTrees = m_PerPlayerObjectiveTrees[i];
			if (playerAndQuestTrees.AcceptingPlayer == player)
			{
				ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(playerAndQuestTrees.AcceptingPlayer.m_PlayerCameraManagerBindingID);
				playerAndQuestTrees.ObjectiveTreeToTrack = null;
				if (playerObjectiveHUD != null)
				{
					playerObjectiveHUD.SetObjectiveTreeToTrack(playerAndQuestTrees.ObjectiveTreeToTrack, force: true);
				}
				for (int num = playerAndQuestTrees.ObjectiveTrees.Count - 1; num >= 0; num--)
				{
					playerAndQuestTrees.ObjectiveTrees.RemoveAt(num);
				}
				playerAndQuestTrees.ObjectiveTrees = new List<ObjectiveTree>();
			}
		}
	}

	public void ShowCurrentTrackingObjective(Player player)
	{
		if (m_PerPlayerObjectiveTrees == null || m_PerPlayerObjectiveTrees.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_PerPlayerObjectiveTrees.Count; i++)
		{
			if (m_PerPlayerObjectiveTrees[i].AcceptingPlayer == player && m_PerPlayerObjectiveTrees[i].ObjectiveTreeToTrack != null)
			{
				ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(m_PerPlayerObjectiveTrees[i].AcceptingPlayer.m_PlayerCameraManagerBindingID);
				if (playerObjectiveHUD != null)
				{
					playerObjectiveHUD.SetObjectiveTreeToTrack(m_PerPlayerObjectiveTrees[i].ObjectiveTreeToTrack, force: true);
				}
				break;
			}
		}
	}

	private bool EvaluateTree(ObjectiveTree tree)
	{
		return tree?.EvaluateCurrentGoal() ?? false;
	}

	public ObjectiveTree CreateObjectiveTree()
	{
		ObjectiveTree objectiveTree = new ObjectiveTree();
		PlayerAndQuestTrees item = default(PlayerAndQuestTrees);
		item.AcceptingPlayer = null;
		item.ObjectiveTrees = new List<ObjectiveTree>();
		item.ObjectiveTreeToTrack = null;
		item.ObjectiveTrees.Add(objectiveTree);
		item.CurrentEvaluationIndex = 0;
		m_PerPlayerObjectiveTrees.Add(item);
		return objectiveTree;
	}

	public bool AssignPrisonObjectives(Player player)
	{
		if (m_PrisonObjectiveData == null)
		{
			return true;
		}
		bool flag = false;
		if (!m_PlayerPrisonObjectives.ContainsKey(player))
		{
			List<ObjectiveTree> newTrees = new List<ObjectiveTree>();
			flag = LoadObjectiveTrees(m_PrisonObjectiveData, player, ref newTrees);
			if (flag)
			{
				PrisonObjectives prisonObjectives = new PrisonObjectives(player);
				for (int i = 0; i < newTrees.Count; i++)
				{
					prisonObjectives.AddTree(newTrees[i]);
				}
				m_PlayerPrisonObjectives.Add(player, prisonObjectives);
				if (prisonObjectives.m_Trees.Count > 0)
				{
					ObjectiveTrackerHUD playerObjectiveHUD = HUDMenuFlow.Instance.GetPlayerObjectiveHUD(player.m_PlayerCameraManagerBindingID);
					if (playerObjectiveHUD != null)
					{
						for (int j = 0; j < m_PerPlayerObjectiveTrees.Count; j++)
						{
							PlayerAndQuestTrees value = m_PerPlayerObjectiveTrees[j];
							if (value.AcceptingPlayer == player)
							{
								if (value.ObjectiveTreeToTrack == null)
								{
									value.ObjectiveTreeToTrack = prisonObjectives.m_Trees[0];
									m_PerPlayerObjectiveTrees[j] = value;
								}
								break;
							}
						}
					}
					else
					{
						playerObjectiveHUD.SetObjectiveTreeToTrack(prisonObjectives.m_Trees[0], force: true);
					}
				}
			}
		}
		return flag;
	}

	public void CleanupPrisonObjectives(Player player)
	{
		PrisonObjectives value = null;
		if (m_PlayerPrisonObjectives.TryGetValue(player, out value))
		{
			value.RemoveAllTrees();
			m_PlayerPrisonObjectives.Remove(player);
		}
	}

	public ObjectiveTree GetPrisonObjectiveTree(Player player)
	{
		PrisonObjectives value = null;
		if (m_PlayerPrisonObjectives.TryGetValue(player, out value) && value.m_Trees.Count > 0)
		{
			return value.m_Trees[0];
		}
		return null;
	}

	public void RequestListenForRuckusEvents(RoomBlob.eLocation locationToObserve, int playerViewID, Routines baseRoutineType, RoutineSubTypes subRoutineType)
	{
		m_NetView.RPC("RPC_MASTER_ListenForRuckusEvents", NetTargets.MasterClient, (byte)locationToObserve, playerViewID, (byte)baseRoutineType, (byte)subRoutineType);
	}

	[PunRPC]
	private void RPC_MASTER_ListenForRuckusEvents(byte locationToObserve, int playerViewID, byte baseRoutineType, byte subRoutineType, PhotonMessageInfo info)
	{
		RuckusObservingData item = default(RuckusObservingData);
		item.Location = (RoomBlob.eLocation)locationToObserve;
		item.BaseRoutineType = (Routines)baseRoutineType;
		item.SubRoutineType = (RoutineSubTypes)subRoutineType;
		PhotonView photonView = PhotonView.Find(playerViewID);
		if (photonView != null)
		{
			item.PlayerToWatch = photonView.gameObject.GetComponent<Player>();
			item.Requester = info.sender;
			if (m_RuckusListenRequesters.Count == 0)
			{
				List<Character> guards = QuestManager.GetInstance().GetGuards();
				for (int i = 0; i < guards.Count; i++)
				{
					Character character = guards[i];
					character.OnCharacterSetTargetCharacter = (Character.CharacterToCharacterEvent)Delegate.Remove(character.OnCharacterSetTargetCharacter, new Character.CharacterToCharacterEvent(OnCharacterSetTargetCharacter));
					Character character2 = guards[i];
					character2.OnCharacterSetTargetCharacter = (Character.CharacterToCharacterEvent)Delegate.Combine(character2.OnCharacterSetTargetCharacter, new Character.CharacterToCharacterEvent(OnCharacterSetTargetCharacter));
				}
			}
			m_RuckusListenRequesters.Add(item);
		}
		else
		{
			item.PlayerToWatch = null;
			m_NetView.RPC("RPC_CLIENT_RuckusEventHappened", info.sender, false, locationToObserve, playerViewID, baseRoutineType, subRoutineType);
		}
	}

	[PunRPC]
	public void RPC_CLIENT_RuckusEventHappened(bool succesfull, byte locationToObserve, int playerViewID, byte baseRoutineType, byte subRoutineType)
	{
		if (OnRuckusEventHappened != null)
		{
			OnRuckusEventHappened(succesfull, (RoomBlob.eLocation)locationToObserve, playerViewID, (Routines)baseRoutineType, (RoutineSubTypes)subRoutineType);
		}
	}

	private void OnCharacterSetTargetCharacter(Character observed, Character target)
	{
		if (observed.m_CharacterRole != CharacterRole.Guard)
		{
			return;
		}
		for (int num = m_RuckusListenRequesters.Count - 1; num >= 0; num--)
		{
			RuckusObservingData ruckusObservingData = m_RuckusListenRequesters[num];
			if (observed.m_CurrentLocation.location == ruckusObservingData.Location && target == ruckusObservingData.PlayerToWatch)
			{
				RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
				if (currentRoutine.m_BaseRoutineType == ruckusObservingData.BaseRoutineType && currentRoutine.m_SubRoutineType == ruckusObservingData.SubRoutineType)
				{
					m_NetView.RPC("RPC_CLIENT_RuckusEventHappened", ruckusObservingData.Requester, true, (byte)ruckusObservingData.Location, ruckusObservingData.PlayerToWatch.m_NetView.viewID, (byte)ruckusObservingData.BaseRoutineType, (byte)ruckusObservingData.SubRoutineType);
					m_RuckusListenRequesters.RemoveAt(num);
				}
			}
		}
		if (m_RuckusListenRequesters.Count == 0)
		{
			List<Character> guards = QuestManager.GetInstance().GetGuards();
			for (int i = 0; i < guards.Count; i++)
			{
				Character character = guards[i];
				character.OnCharacterSetTargetCharacter = (Character.CharacterToCharacterEvent)Delegate.Remove(character.OnCharacterSetTargetCharacter, new Character.CharacterToCharacterEvent(OnCharacterSetTargetCharacter));
			}
		}
	}

	public string CreateSnapshot()
	{
		SaveData_ObjectiveManager_V1 saveData_ObjectiveManager_V = new SaveData_ObjectiveManager_V1();
		int count = m_PerPlayerObjectiveTrees.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerAndQuestTrees playerAndQuestTrees = m_PerPlayerObjectiveTrees[i];
			JObject jObject = new JObject(new JProperty("AP", playerAndQuestTrees.AcceptingPlayer.m_NetView.viewID.ToString()), new JProperty("EI", playerAndQuestTrees.CurrentEvaluationIndex.ToString()));
			if (playerAndQuestTrees.ObjectiveTrees != null)
			{
				JProperty jProperty = new JProperty("OBJTREES");
				JArray jArray = new JArray();
				int count2 = playerAndQuestTrees.ObjectiveTrees.Count;
				for (int j = 0; j < count2; j++)
				{
					jArray.Add(playerAndQuestTrees.ObjectiveTrees[j].SaveInGameObjectiveTrees());
				}
				jProperty.Add(jArray);
				jObject.Add(jProperty);
			}
			saveData_ObjectiveManager_V.LPQ.Add(jObject.ToString());
		}
		return JsonUtility.ToJson(saveData_ObjectiveManager_V);
	}

	public void StartedFromSnapshot()
	{
		RestoreSnapshot();
	}

	private void RestoreSnapshot()
	{
		if (m_SaveData == null)
		{
			_snapshotIsReady = true;
			return;
		}
		if (string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			_snapshotIsReady = true;
			return;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
			_snapshotIsReady = true;
		}
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			SaveData_ObjectiveManager_V1 saveData_ObjectiveManager_V = null;
			try
			{
				saveData_ObjectiveManager_V = JsonUtility.FromJson<SaveData_ObjectiveManager_V1>(m_SaveData.GetSaveData());
			}
			catch
			{
			}
			if (saveData_ObjectiveManager_V != null)
			{
				int count = saveData_ObjectiveManager_V.LPQ.Count;
				PlayerAndQuestTrees item = default(PlayerAndQuestTrees);
				for (int i = 0; i < count; i++)
				{
					JObject jObject = JObject.Parse(saveData_ObjectiveManager_V.LPQ[i]);
					if (jObject == null)
					{
						continue;
					}
					ObjectiveTree objectiveTree = null;
					item.AcceptingPlayer = null;
					item.CurrentEvaluationIndex = -1;
					item.ObjectiveTrees = new List<ObjectiveTree>();
					item.ObjectiveTreeToTrack = null;
					JProperty jProperty = jObject.Property("AP");
					int result = -1;
					if (jProperty != null && int.TryParse((string)jProperty.Value, out result) && result != -1)
					{
						item.AcceptingPlayer = PhotonView.Find(result).GetComponent<Player>();
					}
					JProperty jProperty2 = jObject.Property("EI");
					if (jProperty2 != null)
					{
						int.TryParse((string)jProperty2.Value, out item.CurrentEvaluationIndex);
					}
					JProperty jProperty3 = jObject.Property("OBJTREES");
					if (jProperty3 != null && jProperty3.Value.Type == JTokenType.Array)
					{
						JArray jArray = (JArray)jProperty3.Value;
						for (int j = 0; j < jArray.Count; j++)
						{
							if (jArray[j] != null && jArray[j].Type == JTokenType.String)
							{
								ObjectiveTree objectiveTree2 = new ObjectiveTree();
								objectiveTree2.LoadInGameObjectiveTree((string)jArray[j]);
								item.ObjectiveTrees.Add(objectiveTree2);
								if (objectiveTree2.isBeingTracked)
								{
									objectiveTree = objectiveTree2;
								}
							}
						}
					}
					if (objectiveTree != null && item.AcceptingPlayer != null)
					{
						item.ObjectiveTreeToTrack = objectiveTree;
					}
					m_PerPlayerObjectiveTrees.Add(item);
				}
			}
		}
		_snapshotIsReady = true;
	}

	private bool LoadObjectiveTrees(TextAsset objectiveData, Player owner, ref List<ObjectiveTree> newTrees)
	{
		if (newTrees == null)
		{
			newTrees = new List<ObjectiveTree>();
		}
		if (LoadObjectiveTreeData(objectiveData, ref newTrees))
		{
			for (int i = 0; i < newTrees.Count; i++)
			{
				newTrees[i].BuildOrderList();
				newTrees[i].MainBranch.SetBaseInfo(owner, owner);
				newTrees[i].MainBranch.PickAllRandomTargets();
				newTrees[i].Initialize();
			}
			AddActiveTrees(owner, newTrees);
			return true;
		}
		return false;
	}

	public JObject CacheObjectTreeData(TextAsset objectiveData)
	{
		if (m_JsonMap.ContainsKey(objectiveData))
		{
			return m_JsonMap[objectiveData];
		}
		if (objectiveData == null)
		{
			return null;
		}
		string text = objectiveData.text;
		JObject jObject = JObject.Parse(text);
		m_JsonMap.Add(objectiveData, jObject);
		return jObject;
	}

	public void ClearObjectTreeDataCache()
	{
		m_JsonMap.Clear();
	}

	public bool LoadObjectiveTreeData(TextAsset objectiveData, ref List<ObjectiveTree> list, bool bUpdateNetworkService = false)
	{
		JObject jObject = CacheObjectTreeData(objectiveData);
		if (jObject == null)
		{
			return false;
		}
		JProperty jProperty = jObject.Property("ObjectiveTrees");
		if (jProperty == null || jProperty.Value.Type != JTokenType.Array)
		{
			return false;
		}
		JArray source = (JArray)jProperty.Value;
		List<JObject> list2 = source.Select((JToken c) => (JObject)c).ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			ObjectiveTree objectiveTree = new ObjectiveTree();
			if (objectiveTree.LoadEditorObjectiveTree(list2[i], bUpdateNetworkService))
			{
				list.Add(objectiveTree);
			}
		}
		UpdateManager.AquireHeavyCpuLock();
		return true;
	}

	[Conditional("UNITY_EDITOR")]
	public static void AddLoadError(string error)
	{
	}
}

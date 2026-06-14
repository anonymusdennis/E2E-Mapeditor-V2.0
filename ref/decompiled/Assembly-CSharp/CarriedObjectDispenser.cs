using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkLoadable;
using SaveHelpers;
using UnityEngine;

public class CarriedObjectDispenser : AnimatedInteraction, Saveable, INetworkLoadable
{
	[Serializable]
	private class SaveData_Dispenser_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public SaveData_DispensedObject_V1[] O;

		public SaveData_Dispenser_V1()
		{
			m_Version = 1;
		}
	}

	[Serializable]
	private class SaveData_DispensedObject_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public int I;

		public ulong C;

		public uint T;

		public SaveData_DispensedObject_V1()
		{
			m_Version = 1;
		}
	}

	[Header("Carried Object Dispenser")]
	public GameObject m_CarryableObjectPrefab;

	public List<uint> m_PossibleSpawnTags;

	private List<CarryObjectInteraction> m_SpawnPool = new List<CarryObjectInteraction>();

	private List<CarryObjectInteraction> m_ActiveObjects = new List<CarryObjectInteraction>();

	private static int m_NumSpawnedObjects;

	public int m_MaxActiveObjects = 3;

	private bool m_bHasInitRan;

	[Header("Hazardous Items")]
	public List<ItemData> m_RequiredItems;

	public SpeechPODO m_NoEquipmentSpeech;

	private SaveDataRegister m_SaveData;

	private bool m_bHasRequestedExit;

	private static int m_ThisLevelNumObjectDispensers = -1;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	protected override void Awake()
	{
		base.Awake();
		m_SaveData = new SaveDataRegister(this, m_NetViewID.viewID, bIsMajorManagerComponent: false);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected override void OnDestroy()
	{
		for (int num = m_SpawnPool.Count - 1; num >= 0; num--)
		{
			CarryObjectInteraction obj = m_SpawnPool[num];
			UnityEngine.Object.Destroy(obj);
			m_NumSpawnedObjects--;
		}
		for (int num2 = m_ActiveObjects.Count - 1; num2 >= 0; num2--)
		{
			CarryObjectInteraction obj2 = m_ActiveObjects[num2];
			UnityEngine.Object.Destroy(obj2);
			m_NumSpawnedObjects--;
		}
		m_SpawnPool.Clear();
		m_ActiveObjects.Clear();
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		m_ThisLevelNumObjectDispensers = -1;
		base.OnDestroy();
	}

	public void SetDispensedObject(GameObject prefab, List<ItemData> requiredItems, SpeechPODO noEquipmentSpeech, List<uint> possibleSpawnTags)
	{
		m_CarryableObjectPrefab = prefab;
		m_PossibleSpawnTags = possibleSpawnTags;
		m_RequiredItems = requiredItems;
		m_NoEquipmentSpeech = noEquipmentSpeech;
	}

	public void JobManager_Init()
	{
		if (m_ThisLevelNumObjectDispensers == -1)
		{
			CarriedObjectDispenser[] array = UnityEngine.Object.FindObjectsOfType<CarriedObjectDispenser>();
			m_ThisLevelNumObjectDispensers = array.Length;
		}
		if (m_MaxActiveObjects <= 0)
		{
			m_MaxActiveObjects = 1;
		}
		int num = Mathf.Min(m_MaxActiveObjects, 50 / m_ThisLevelNumObjectDispensers);
		for (int i = 0; i < num; i++)
		{
			CreateNewInstanceForSpawnPool();
		}
		if (T17NetManager.IsMasterClient)
		{
			SaveData_Dispenser_V1 snapshotData = GetSnapshotData();
			if (snapshotData != null && snapshotData.O != null && snapshotData.O.Length > 0)
			{
				DeserialiseObjects(snapshotData.O);
			}
		}
		m_bHasInitRan = true;
	}

	private void CreateNewInstanceForSpawnPool(int netViewOverride = -1)
	{
		CarryObjectInteraction carryObjectInteraction = SpawnNewInstanceOfPrefab(m_CarryableObjectPrefab, netViewOverride);
		if (carryObjectInteraction != null)
		{
			m_SpawnPool.Add(carryObjectInteraction);
			carryObjectInteraction.gameObject.SetActive(value: false);
		}
	}

	private CarryObjectInteraction FindObjectWithNetID(int viewId, ref bool isFromSpawnPool)
	{
		CarryObjectInteraction carryObjectInteraction = m_SpawnPool.Find((CarryObjectInteraction x) => x.m_NetViewID.viewID == viewId);
		isFromSpawnPool = true;
		if (carryObjectInteraction == null)
		{
			carryObjectInteraction = m_ActiveObjects.Find((CarryObjectInteraction x) => x != null && x.m_NetViewID.viewID == viewId);
			isFromSpawnPool = false;
		}
		return carryObjectInteraction;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return !localCharacter.IsCarrying() && localCharacter.HasItemsOnPerson(m_RequiredItems) && base.AllowedToInteract(localCharacter) && m_ActiveObjects.Count < m_MaxActiveObjects;
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			if (!localCharacter.HasItemsOnPerson(m_RequiredItems))
			{
				SpeechManager.GetInstance().SaySomething(localCharacter, m_NoEquipmentSpeech);
				return true;
			}
			return false;
		}
		return true;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_bHasRequestedExit = false;
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (!m_bHasRequestedExit)
		{
			m_bHasRequestedExit = true;
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		StartSpawnningForCharacter(localCharacter);
	}

	public override bool InteractionVisibility()
	{
		return base.InteractionVisibility() && m_ActiveObjects.Count < m_MaxActiveObjects;
	}

	public void StartSpawnningForCharacter(Character character)
	{
		m_NetViewID.RPC("RPC_MasterPickNewInstanceId", NetTargets.MasterClient, character.m_NetView.viewID);
	}

	[PunRPC]
	protected void RPC_MasterPickNewInstanceId(int characterId)
	{
		int num = -1;
		if (m_SpawnPool.Count != 0)
		{
			CarryObjectInteraction carryObjectInteraction = m_SpawnPool[0];
			num = carryObjectInteraction.m_NetViewID.viewID;
			m_SpawnPool.Remove(carryObjectInteraction);
			m_ActiveObjects.Add(carryObjectInteraction);
			carryObjectInteraction.gameObject.SetActive(value: true);
		}
		int num2 = 0;
		if (m_PossibleSpawnTags != null && m_PossibleSpawnTags.Count != 0)
		{
			num2 = (int)m_PossibleSpawnTags[UnityEngine.Random.Range(0, m_PossibleSpawnTags.Count)];
		}
		m_NetViewID.RPC("RPC_SpawnObject", NetTargets.All, characterId, num, num2);
	}

	[PunRPC]
	protected void RPC_SpawnObject(int characterId, int objectInstanceId, int spawnTag)
	{
		if (!m_bHasInitRan)
		{
			return;
		}
		CarryObjectInteraction carryObjectInteraction = null;
		bool isFromSpawnPool = false;
		if (objectInstanceId != -1)
		{
			carryObjectInteraction = FindObjectWithNetID(objectInstanceId, ref isFromSpawnPool);
		}
		if (carryObjectInteraction == null)
		{
			carryObjectInteraction = SpawnNewInstanceOfPrefab(m_CarryableObjectPrefab, objectInstanceId);
		}
		Character character = T17NetView.Find<Character>(characterId);
		if (character == null || !(carryObjectInteraction != null))
		{
			return;
		}
		if (isFromSpawnPool)
		{
			m_SpawnPool.Remove(carryObjectInteraction);
		}
		carryObjectInteraction.gameObject.SetActive(value: true);
		if (!m_ActiveObjects.Contains(carryObjectInteraction))
		{
			m_ActiveObjects.Add(carryObjectInteraction);
		}
		if (m_RequiredItems.Count != 0)
		{
			HazardousCarryableObjectInteraction hazardousCarryableObjectInteraction = carryObjectInteraction as HazardousCarryableObjectInteraction;
			if (hazardousCarryableObjectInteraction != null)
			{
				hazardousCarryableObjectInteraction.m_RequiredItems = m_RequiredItems;
				hazardousCarryableObjectInteraction.m_NoEquipmentSpeech = m_NoEquipmentSpeech;
			}
		}
		carryObjectInteraction.SetTag((uint)spawnTag);
		carryObjectInteraction.transform.position = base.transform.position;
		if (T17NetManager.IsMasterClient)
		{
			StartCoroutine(DelayedCharacterPickup(character, carryObjectInteraction));
		}
	}

	private CarryObjectInteraction SpawnNewInstanceOfPrefab(GameObject selectedPrefab, int idOverride = -1)
	{
		if (m_NumSpawnedObjects >= 50)
		{
			return null;
		}
		if (selectedPrefab == null)
		{
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(selectedPrefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		int viewID = ((idOverride != -1) ? idOverride : (T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.CarriedObjectDispenserStart) + m_NumSpawnedObjects));
		PhotonView photonViewInDictionary = PhotonNetwork.networkingPeer.GetPhotonViewInDictionary(viewID);
		if (photonViewInDictionary != null)
		{
			return null;
		}
		m_NumSpawnedObjects++;
		gameObject.GetComponent<T17NetView>().viewID = viewID;
		CarryObjectInteraction component = gameObject.GetComponent<CarryObjectInteraction>();
		if (component != null)
		{
			component.m_bUpdateNetSaveData = false;
		}
		return component;
	}

	private string DEBUG_PrintTransformHeirarchy(Transform theTransform)
	{
		string text = string.Empty;
		Transform transform = theTransform;
		while (transform.parent != null)
		{
			text = text + transform.parent.name + "/";
			transform = transform.parent;
		}
		return text;
	}

	public void ReleaseAllActiveObjectsWithEffect()
	{
		List<CarryObjectInteraction> list = new List<CarryObjectInteraction>(m_ActiveObjects);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			CarryObjectInteraction carryObjectInteraction = list[num];
			if (!carryObjectInteraction.IsPickedUp)
			{
				EffectManager.GetInstance().PlayEffect_LocalOnly(EffectManager.effectType.PlayerLeaveDust, list[num].transform.position);
				AddObjectBackToSpawnPool(list[num]);
			}
		}
	}

	public void AddObjectBackToSpawnPool(CarryObjectInteraction theObject)
	{
		if (m_ActiveObjects.Contains(theObject))
		{
			m_ActiveObjects.Remove(theObject);
			m_SpawnPool.Add(theObject);
			theObject.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator DelayedCharacterPickup(Character localCharacter, CarryObjectInteraction theObject)
	{
		yield return new WaitForEndOfFrame();
		m_NetViewID.RPC("RPC_CharacterInteractWithObject", localCharacter.m_NetView, localCharacter.m_NetView.viewID, theObject.m_NetViewID.viewID);
	}

	[PunRPC]
	protected void RPC_CharacterInteractWithObject(int characterId, int objectId)
	{
		Character character = T17NetView.Find<Character>(characterId);
		CarryObjectInteraction carryObjectInteraction = T17NetView.Find<CarryObjectInteraction>(objectId);
		if (!(character == null) && !(carryObjectInteraction == null))
		{
			carryObjectInteraction.Interact(character);
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	private SaveData_DispensedObject_V1[] SerialiseActiveObjects()
	{
		SaveData_DispensedObject_V1[] array = new SaveData_DispensedObject_V1[m_ActiveObjects.Count];
		for (int num = m_ActiveObjects.Count - 1; num >= 0; num--)
		{
			CarryObjectInteraction carryObjectInteraction = m_ActiveObjects[num];
			if (!(carryObjectInteraction == null))
			{
				SaveData_DispensedObject_V1 saveData_DispensedObject_V = new SaveData_DispensedObject_V1();
				saveData_DispensedObject_V.C = carryObjectInteraction.m_CachedObjData;
				saveData_DispensedObject_V.I = carryObjectInteraction.m_NetViewID.viewID;
				saveData_DispensedObject_V.T = carryObjectInteraction.m_Tag;
				array[num] = saveData_DispensedObject_V;
			}
		}
		return array;
	}

	private void DeserialiseObjects(SaveData_DispensedObject_V1[] objects)
	{
		bool isFromSpawnPool = false;
		foreach (SaveData_DispensedObject_V1 saveData_DispensedObject_V in objects)
		{
			CarryObjectInteraction carryObjectInteraction = FindObjectWithNetID(saveData_DispensedObject_V.I, ref isFromSpawnPool);
			if (carryObjectInteraction != null)
			{
				carryObjectInteraction.m_CachedObjData = saveData_DispensedObject_V.C;
				BitField bitField = new BitField(saveData_DispensedObject_V.C);
				carryObjectInteraction.SetTag(saveData_DispensedObject_V.T);
				carryObjectInteraction.Deserialize(bitField, burnNetViewBits: true);
				if (!m_ActiveObjects.Contains(carryObjectInteraction))
				{
					carryObjectInteraction.gameObject.SetActive(value: true);
					carryObjectInteraction.OnDispenserDeserializeActive();
					m_ActiveObjects.Add(carryObjectInteraction);
					m_SpawnPool.Remove(carryObjectInteraction);
				}
			}
		}
	}

	public string CreateSnapshot()
	{
		SaveData_Dispenser_V1 saveData_Dispenser_V = new SaveData_Dispenser_V1();
		saveData_Dispenser_V.O = SerialiseActiveObjects();
		return JsonUtility.ToJson(saveData_Dispenser_V);
	}

	public void StartedFromSnapshot()
	{
	}

	private SaveData_Dispenser_V1 GetSnapshotData()
	{
		if (m_SaveData == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return null;
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
			string saveData = m_SaveData.GetSaveData();
			SaveData_Dispenser_V1 saveData_Dispenser_V = null;
			try
			{
				saveData_Dispenser_V = JsonUtility.FromJson<SaveData_Dispenser_V1>(saveData);
			}
			catch
			{
			}
			if (saveData_Dispenser_V != null)
			{
				return saveData_Dispenser_V;
			}
		}
		return null;
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
			SaveData_DispensedObject_V1[] graph = SerialiseActiveObjects();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, graph);
			m_NetViewID.RPC("RPC_ClientRecieveActiveObjects", player, memoryStream.ToArray());
		}
	}

	[PunRPC]
	protected void RPC_ClientRecieveActiveObjects(byte[] objects, PhotonMessageInfo info)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(objects))
		{
			SaveData_DispensedObject_V1[] array = binaryFormatter.Deserialize(serializationStream) as SaveData_DispensedObject_V1[];
			if (array.Length == 0)
			{
			}
			DeserialiseObjects(array);
		}
		m_LoadState = LOADSTATE.Finished_OK;
	}
}

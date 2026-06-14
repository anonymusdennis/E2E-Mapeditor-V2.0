using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour, IControlledUpdate
{
	private struct DoorColliderData
	{
		public BoxCollider DoorCollider;

		public int DoorID;
	}

	private FastList<Door> m_AllDoors = new FastList<Door>();

	private Dictionary<Character, List<DoorColliderData>> m_CharacterDoorIgnores = new Dictionary<Character, List<DoorColliderData>>(Character.CharacterTComparer);

	private Dictionary<int, Door> m_DoorIdToDoor = new Dictionary<int, Door>();

	private static DoorManager m_Instance;

	private bool m_bPurpleLocksChanged;

	private bool m_bPurpleLocksAreOpen;

	public static DoorManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	private void Start()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RapidPeriodic);
		}
	}

	protected virtual void OnDestroy()
	{
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_AllDoors.Clear();
		m_DoorIdToDoor.Clear();
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
		ProcessPurpleLocks();
		int count = m_AllDoors.Count;
		for (int i = 0; i < count; i++)
		{
			m_AllDoors[i].UpdateDoor();
		}
	}

	public void AddDoor(Door door)
	{
		if (!(door == null))
		{
			m_AllDoors.Add(door);
			if (!m_DoorIdToDoor.ContainsKey(door.Local_DoorID))
			{
				m_DoorIdToDoor.Add(door.Local_DoorID, door);
			}
		}
	}

	public void OnCharacterDestroy(Character character)
	{
		if (character != null && m_CharacterDoorIgnores.ContainsKey(character))
		{
			m_CharacterDoorIgnores.Remove(character);
		}
	}

	private Collider GetCorrectCharacterCollider(Character character)
	{
		if (character.m_PhysicsCollider == null)
		{
			return null;
		}
		return character.m_PhysicsSphereCol;
	}

	private void SetDoorAsAllowedForCharacter(Collider charCollider, Door door, Character character, Item itemAllowingAccess = null)
	{
		if (charCollider != null)
		{
			Physics.IgnoreCollision(charCollider, door.Collider, ignore: true);
		}
		DoorColliderData item = default(DoorColliderData);
		item.DoorCollider = door.Collider;
		item.DoorID = door.Local_DoorID;
		m_CharacterDoorIgnores[character].Add(item);
		character.AddAllowedDoor(door, itemAllowingAccess);
	}

	public bool IsDoorAllowedForCharacter(Character character, Door door)
	{
		if (m_CharacterDoorIgnores.ContainsKey(character))
		{
			List<DoorColliderData> list = m_CharacterDoorIgnores[character];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].DoorID == door.Local_DoorID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetUpCharacterKeys(Character character)
	{
		if ((LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null && LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison && character.m_CharacterRole == CharacterRole.JobGuy) || character.m_CharacterRole == CharacterRole.Ghost)
		{
			return;
		}
		character.ClearAllowedDoors();
		Collider correctCharacterCollider = GetCorrectCharacterCollider(character);
		if (!m_CharacterDoorIgnores.ContainsKey(character))
		{
			m_CharacterDoorIgnores.Add(character, new List<DoorColliderData>());
		}
		else
		{
			if (correctCharacterCollider != null)
			{
				List<DoorColliderData> list = m_CharacterDoorIgnores[character];
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					Physics.IgnoreCollision(correctCharacterCollider, list[i].DoorCollider, ignore: false);
				}
			}
			m_CharacterDoorIgnores[character].Clear();
		}
		if (null != character.m_ItemContainer)
		{
			for (int j = 0; j < character.m_ItemContainer.GetItemCount(); j++)
			{
				Item item = character.m_ItemContainer.GetItem(j);
				KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (!(keyFunctionality != null))
				{
					continue;
				}
				KeyFunctionality.KeyColour keyColour = keyFunctionality.m_KeyColour;
				for (int i = 0; i < m_AllDoors.Count; i++)
				{
					Door door = m_AllDoors[i];
					if ((door.m_DoorKeyColour != KeyFunctionality.KeyColour.Silver || keyColour == KeyFunctionality.KeyColour.Silver) && door.m_DoorOutfitType == Item_Outfit.OutFitType.None && (keyColour == KeyFunctionality.KeyColour.Black || door.m_DoorKeyColour == keyColour) && (keyColour == KeyFunctionality.KeyColour.Black || keyFunctionality.SubCode == 0 || door.m_DoorKeySubCode == keyFunctionality.SubCode))
					{
						SetDoorAsAllowedForCharacter(correctCharacterCollider, door, character, item);
					}
				}
			}
			if (character.GetEquippedItem() != null)
			{
				Item equippedItem = character.GetEquippedItem();
				KeyFunctionality keyFunctionality2 = (KeyFunctionality)equippedItem.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (keyFunctionality2 != null)
				{
					KeyFunctionality.KeyColour keyColour2 = keyFunctionality2.m_KeyColour;
					for (int i = 0; i < m_AllDoors.Count; i++)
					{
						Door door2 = m_AllDoors[i];
						if ((door2.m_DoorKeyColour != KeyFunctionality.KeyColour.Silver || keyColour2 == KeyFunctionality.KeyColour.Silver) && (door2.m_DoorOutfitType == Item_Outfit.OutFitType.None || keyColour2 == KeyFunctionality.KeyColour.Black) && (keyColour2 == KeyFunctionality.KeyColour.Black || door2.m_DoorKeyColour == keyColour2) && (keyColour2 == KeyFunctionality.KeyColour.Black || keyFunctionality2.SubCode == 0 || door2.m_DoorKeySubCode == keyFunctionality2.SubCode))
						{
							SetDoorAsAllowedForCharacter(correctCharacterCollider, door2, character, equippedItem);
						}
					}
				}
			}
			for (int j = 0; j < character.m_ItemContainer.GetHiddenItemCount(); j++)
			{
				Item hiddenItem = character.m_ItemContainer.GetHiddenItem(j);
				KeyFunctionality keyFunctionality3 = (KeyFunctionality)hiddenItem.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (!(keyFunctionality3 != null))
				{
					continue;
				}
				KeyFunctionality.KeyColour keyColour3 = keyFunctionality3.m_KeyColour;
				for (int i = 0; i < m_AllDoors.Count; i++)
				{
					Door door3 = m_AllDoors[i];
					if ((door3.m_DoorOutfitType == Item_Outfit.OutFitType.None || keyColour3 == KeyFunctionality.KeyColour.Black) && (keyColour3 == KeyFunctionality.KeyColour.Black || door3.m_DoorKeyColour == keyColour3) && (keyColour3 == KeyFunctionality.KeyColour.Black || keyFunctionality3.SubCode == 0 || door3.m_DoorKeySubCode == keyFunctionality3.SubCode))
					{
						SetDoorAsAllowedForCharacter(correctCharacterCollider, door3, character, hiddenItem);
					}
				}
			}
		}
		if (character.GetOutFit() != null)
		{
			Item outFit = character.GetOutFit();
			Item_Outfit outfitData = outFit.OutfitData;
			if (outfitData != null)
			{
				for (int i = 0; i < m_AllDoors.Count; i++)
				{
					Door door4 = m_AllDoors[i];
					if (door4.m_DoorOutfitType != 0 && door4.m_DoorOutfitType == outfitData.m_Type)
					{
						SetDoorAsAllowedForCharacter(correctCharacterCollider, door4, character, outFit);
					}
				}
			}
		}
		for (int i = 0; i < m_AllDoors.Count; i++)
		{
			Door door5 = m_AllDoors[i];
			if ((door5.m_DoorKeyColour == KeyFunctionality.KeyColour.None && door5.m_DoorOutfitType == Item_Outfit.OutFitType.None) || (door5.m_DoorKeyColour == KeyFunctionality.KeyColour.Purple && RoutineManager.GetInstance().PurpleDoorsOpen) || door5.IsForceOpened())
			{
				SetDoorAsAllowedForCharacter(correctCharacterCollider, door5, character);
			}
			else if (door5.DoesContainCharacter(character))
			{
				door5.SetTempAllowed(character);
				if (correctCharacterCollider != null)
				{
					Physics.IgnoreCollision(correctCharacterCollider, door5.Collider, ignore: true);
				}
			}
		}
	}

	public void RegisterToRoutineManager()
	{
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnPurpleDoorLockStatusChanged += PurpleLocksChanged;
		}
	}

	private void PurpleLocksChanged(bool arePurpleDoorsOpen)
	{
		m_bPurpleLocksChanged = true;
		m_bPurpleLocksAreOpen = arePurpleDoorsOpen;
	}

	private void ProcessPurpleLocks()
	{
		if (!m_bPurpleLocksChanged || UpdateManager.IsHeavyCpuLocked())
		{
			return;
		}
		m_bPurpleLocksChanged = false;
		foreach (KeyValuePair<Character, List<DoorColliderData>> characterDoorIgnore in m_CharacterDoorIgnores)
		{
			Character key = characterDoorIgnore.Key;
			if (key.m_CharacterRole != 0)
			{
				continue;
			}
			for (int i = 0; i < m_AllDoors.Count; i++)
			{
				Door door = m_AllDoors[i];
				if (door.m_DoorKeyColour != KeyFunctionality.KeyColour.Purple)
				{
					continue;
				}
				int local_DoorID = door.Local_DoorID;
				if (m_bPurpleLocksAreOpen)
				{
					if (!key.IsAllowedThroughDoor(local_DoorID))
					{
						SetDoorAsAllowedForCharacter(GetCorrectCharacterCollider(key), door, key);
					}
				}
				else
				{
					if (!key.IsAllowedThroughDoor(local_DoorID))
					{
						continue;
					}
					for (int j = 0; j < characterDoorIgnore.Value.Count; j++)
					{
						DoorColliderData doorColliderData = characterDoorIgnore.Value[j];
						if (doorColliderData.DoorID == local_DoorID)
						{
							Physics.IgnoreCollision(GetCorrectCharacterCollider(key), doorColliderData.DoorCollider, ignore: false);
							key.RemoveAllowedDoor(local_DoorID);
							characterDoorIgnore.Value.RemoveAt(j);
							break;
						}
					}
				}
			}
		}
	}

	public Door GetDoorByID(int id)
	{
		Door value = null;
		m_DoorIdToDoor.TryGetValue(id, out value);
		return value;
	}

	public bool IsPendingPurpleLockProcess()
	{
		return m_bPurpleLocksChanged;
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
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return true;
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
}

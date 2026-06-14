using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Door : T17MonoBehaviour
{
	public enum DoorOneWayDirection
	{
		None,
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	public bool IsVerticalDoor = true;

	public bool IsDoubleDoor;

	public DoorOneWayDirection m_OneWayWhenLocked;

	public KeyFunctionality.KeyColour m_DoorKeyColour = KeyFunctionality.KeyColour.None;

	[Range(0f, 99f)]
	public int m_DoorKeySubCode;

	public Item_Outfit.OutFitType m_DoorOutfitType;

	private Animator m_DoorAnimator;

	private int m_OpenParamID;

	private int m_OpenFastParamID;

	private bool m_bDoorOpenThisFrame;

	private bool m_bDoorOpen;

	private Vector3 m_CachedPosition;

	private Collider m_LastTriggeredCollider;

	private Character m_LastTriggeredChar;

	private FastList<Character> m_TempAllowedCharacter = new FastList<Character>();

	private BoxCollider m_OurCollider;

	private Dictionary<Character, KeyFunctionality> m_EnteredCharacters = new Dictionary<Character, KeyFunctionality>(Character.CharacterTComparer);

	private bool m_bIsForceOpened;

	private int m_MyLocal_DoorID = -999;

	private static int DOOR_ID = 123;

	public BoxCollider Collider => m_OurCollider;

	public int Local_DoorID => m_MyLocal_DoorID;

	public bool IsForceOpened()
	{
		return m_bIsForceOpened;
	}

	protected override void Awake()
	{
		base.Awake();
		m_MyLocal_DoorID = DOOR_ID;
		DOOR_ID++;
	}

	private void Start()
	{
		m_OurCollider = null;
		BoxCollider[] components = GetComponents<BoxCollider>();
		if (components != null)
		{
			int i = 0;
			for (int num = components.Length; i < num; i++)
			{
				if (components[i] != null && !components[i].isTrigger)
				{
					m_OurCollider = components[i];
					break;
				}
			}
		}
		m_CachedPosition = base.transform.position;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		DoorManager.GetInstance().AddDoor(this);
		m_OpenParamID = Animator.StringToHash("Open");
		m_OpenFastParamID = Animator.StringToHash("OpenFast");
		if (m_DoorAnimator == null)
		{
			m_DoorAnimator = GetComponentInChildren<Animator>(includeInactive: true);
		}
		if (m_DoorAnimator != null)
		{
			m_DoorAnimator.SetBool(m_OpenParamID, value: false);
			m_DoorAnimator.ResetTrigger(m_OpenFastParamID);
		}
		UpdateAINode();
		OneWayDoorAISkip();
		return base.StartInit();
	}

	public bool GetDoorOpen()
	{
		return m_bDoorOpen;
	}

	public void UpdateDoor()
	{
		if (m_bIsForceOpened)
		{
			m_bDoorOpenThisFrame = true;
		}
		if (!m_bDoorOpenThisFrame && m_bDoorOpen)
		{
			m_bDoorOpen = false;
			if (m_DoorAnimator != null)
			{
				m_DoorAnimator.SetBool(m_OpenParamID, value: false);
				m_DoorAnimator.ResetTrigger(m_OpenFastParamID);
			}
			for (int i = 0; i < m_TempAllowedCharacter.Count; i++)
			{
				if (!(m_TempAllowedCharacter[i] == null) && !(m_TempAllowedCharacter[i].m_PhysicsSphereCol == null) && !(m_OurCollider == null) && !m_TempAllowedCharacter[i].IsAllowedThroughDoor(m_MyLocal_DoorID))
				{
					Physics.IgnoreCollision(m_TempAllowedCharacter[i].m_PhysicsSphereCol, m_OurCollider, ignore: false);
				}
			}
			m_TempAllowedCharacter.Clear();
			Dictionary<Character, KeyFunctionality>.Enumerator enumerator = m_EnteredCharacters.GetEnumerator();
			while (enumerator.MoveNext())
			{
				OnExit(enumerator.Current.Key);
			}
			m_EnteredCharacters.Clear();
		}
		else if (m_bDoorOpenThisFrame && !m_bDoorOpen)
		{
			m_bDoorOpen = true;
			if (m_DoorAnimator != null)
			{
				m_DoorAnimator.SetBool(m_OpenParamID, value: true);
			}
		}
		m_bDoorOpenThisFrame = false;
	}

	private KeyFunctionality OnEnter(Character character)
	{
		KeyFunctionality keyFunctionality = null;
		FastList<Item> itemsAllowingThroughDoor = character.GetItemsAllowingThroughDoor(m_MyLocal_DoorID);
		if (itemsAllowingThroughDoor != null)
		{
			for (int i = 0; i < itemsAllowingThroughDoor.Count; i++)
			{
				Item item = itemsAllowingThroughDoor[i];
				if (item == null || item.OutfitData != null)
				{
					keyFunctionality = null;
					break;
				}
				KeyFunctionality keyFunctionality2 = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (keyFunctionality2 != null && (keyFunctionality == null || keyFunctionality2.IsDurable))
				{
					keyFunctionality = keyFunctionality2;
				}
			}
		}
		return keyFunctionality;
	}

	private void OnExit(Character character)
	{
		if (character != null && character.m_NetView != null && character.m_NetView.isMine)
		{
			KeyFunctionality keyFunctionality = m_EnteredCharacters[character];
			if (keyFunctionality != null)
			{
				keyFunctionality.OnPostUse();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (m_DoorAnimator == null)
		{
			return;
		}
		if (other != m_LastTriggeredCollider)
		{
			m_LastTriggeredChar = other.transform.parent.GetComponent<Character>();
			m_LastTriggeredCollider = other;
		}
		if (m_LastTriggeredChar == null)
		{
			return;
		}
		bool flag = m_LastTriggeredChar.IsAllowedThroughDoor(m_MyLocal_DoorID);
		bool flag2 = false;
		if (flag)
		{
			bool flag3 = ShouldDoorOpenForCharacter(m_LastTriggeredChar);
			if (flag3 && !m_EnteredCharacters.ContainsKey(m_LastTriggeredChar))
			{
				KeyFunctionality value = OnEnter(m_LastTriggeredChar);
				m_EnteredCharacters.Add(m_LastTriggeredChar, value);
				flag2 = true;
			}
			else if (!flag3 && m_EnteredCharacters.ContainsKey(m_LastTriggeredChar))
			{
				OnExit(m_LastTriggeredChar);
				m_EnteredCharacters.Remove(m_LastTriggeredChar);
			}
		}
		if (m_bDoorOpenThisFrame)
		{
			return;
		}
		if (!flag && m_TempAllowedCharacter.Contains(m_LastTriggeredChar) && m_OurCollider.bounds.Contains(m_LastTriggeredChar.m_CachedCurrentPosition))
		{
			flag = true;
		}
		if (flag || ((m_DoorKeyColour == KeyFunctionality.KeyColour.Purple || m_DoorKeyColour == KeyFunctionality.KeyColour.Green) && m_OneWayWhenLocked != 0))
		{
			Directionx4 x4FacingDirection = m_LastTriggeredChar.m_x4FacingDirection;
			Vector2 vector = m_CachedPosition - m_LastTriggeredChar.m_CachedCurrentPosition;
			bool flag4 = false;
			if (OneWayKey() && !flag)
			{
				bool flag5 = m_TempAllowedCharacter.Contains(m_LastTriggeredChar);
				if (!flag5)
				{
					switch (m_OneWayWhenLocked)
					{
					case DoorOneWayDirection.LeftToRight:
						if (x4FacingDirection == Directionx4.Right && vector.x > 0f)
						{
							flag4 = true;
						}
						break;
					case DoorOneWayDirection.RightToLeft:
						if (x4FacingDirection == Directionx4.Left && vector.x < 0f)
						{
							flag4 = true;
						}
						break;
					case DoorOneWayDirection.TopToBottom:
						if (x4FacingDirection == Directionx4.Down && vector.y < 0f)
						{
							flag4 = true;
						}
						break;
					case DoorOneWayDirection.BottomToTop:
						if (x4FacingDirection == Directionx4.Up && vector.y > 0f)
						{
							flag4 = true;
						}
						break;
					}
				}
				else
				{
					flag4 = true;
				}
				if (!flag4)
				{
					return;
				}
				if (!flag5)
				{
					m_TempAllowedCharacter.Add(m_LastTriggeredChar);
					if (m_LastTriggeredChar.m_PhysicsSphereCol != null)
					{
						Physics.IgnoreCollision(m_LastTriggeredChar.m_PhysicsSphereCol, m_OurCollider, ignore: true);
					}
					if (!m_bDoorOpen)
					{
						PerformFastOpenCheckOnLastTriggered();
					}
				}
				m_bDoorOpenThisFrame = true;
			}
			else
			{
				if (!m_bDoorOpen && flag2)
				{
					PerformFastOpenCheckOnLastTriggered();
				}
				m_bDoorOpenThisFrame = ShouldDoorOpenForCharacter(m_LastTriggeredChar);
			}
		}
		else if (m_LastTriggeredChar.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_LastTriggeredChar;
			if (player != null)
			{
				TutorialManager.GetInstance().StartTutorialRPC(player, TutorialSubject.Keys);
			}
		}
	}

	private void PerformFastOpenCheckOnLastTriggered()
	{
		if (m_LastTriggeredChar.m_CharacterMovement.GetSpeed() == CharacterSpeed.Run)
		{
			m_DoorAnimator.SetTrigger(m_OpenFastParamID);
		}
	}

	private bool ShouldDoorOpenForCharacter(Character character)
	{
		Vector2 to = m_CachedPosition - character.m_CachedCurrentPosition;
		if (character.m_CharacterRole == CharacterRole.Ghost)
		{
			return false;
		}
		if (to.sqrMagnitude <= 0.5f)
		{
			return true;
		}
		if (m_OurCollider.bounds.Contains(character.m_CachedCurrentPosition))
		{
			return true;
		}
		Vector2 facingDirection = character.GetFacingDirection();
		to.Normalize();
		if (IsVerticalDoor)
		{
			to.y = 0f;
			if (((facingDirection.x >= 0f && to.x >= 0f) || (facingDirection.x <= 0f && to.x <= 0f)) && Vector2.Angle(facingDirection, to) < 60f)
			{
				return true;
			}
		}
		else
		{
			to.x = 0f;
			if (((facingDirection.y >= 0f && to.y >= 0f) || (facingDirection.y <= 0f && to.y <= 0f)) && Vector2.Angle(facingDirection, to) < 60f)
			{
				return true;
			}
		}
		return false;
	}

	private bool OneWayKey()
	{
		return m_DoorKeyColour == KeyFunctionality.KeyColour.Purple || m_DoorKeyColour == KeyFunctionality.KeyColour.Green;
	}

	private void OneWayDoorAISkip()
	{
		if (m_OneWayWhenLocked != 0)
		{
			Vector3 cachedPosition = m_CachedPosition;
			Vector3 cachedPosition2 = m_CachedPosition;
			switch (m_OneWayWhenLocked)
			{
			case DoorOneWayDirection.LeftToRight:
				cachedPosition2 += Direction.m_vLeft;
				cachedPosition += Direction.m_vRight;
				break;
			case DoorOneWayDirection.RightToLeft:
				cachedPosition2 += Direction.m_vRight;
				cachedPosition += Direction.m_vLeft;
				break;
			case DoorOneWayDirection.TopToBottom:
				cachedPosition2 += Direction.m_vUp;
				cachedPosition += Direction.m_vDown;
				break;
			case DoorOneWayDirection.BottomToTop:
				cachedPosition2 += Direction.m_vDown;
				cachedPosition += Direction.m_vUp;
				break;
			}
			if (!IsDoubleDoor)
			{
				NavMeshUtil.CreateManualConnection(cachedPosition2, cachedPosition, 1u, biDirectional: false);
			}
			else if (m_OneWayWhenLocked == DoorOneWayDirection.LeftToRight || m_OneWayWhenLocked == DoorOneWayDirection.RightToLeft)
			{
				NavMeshUtil.CreateManualConnection(cachedPosition2 + Direction.m_vUp * 0.5f, cachedPosition + Direction.m_vUp * 0.5f, 1u, biDirectional: false);
				NavMeshUtil.CreateManualConnection(cachedPosition2 - Direction.m_vUp * 0.5f, cachedPosition - Direction.m_vUp * 0.5f, 1u, biDirectional: false);
			}
			else
			{
				NavMeshUtil.CreateManualConnection(cachedPosition2 + Direction.m_vRight * 0.5f, cachedPosition + Direction.m_vRight * 0.5f, 1u, biDirectional: false);
				NavMeshUtil.CreateManualConnection(cachedPosition2 - Direction.m_vRight * 0.5f, cachedPosition - Direction.m_vRight * 0.5f, 1u, biDirectional: false);
			}
		}
	}

	private void UpdateAINode()
	{
		if (m_DoorKeyColour == KeyFunctionality.KeyColour.None || !(null != AstarPath.active))
		{
			return;
		}
		if (!IsDoubleDoor)
		{
			GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(m_CachedPosition);
			UpdateNode(nearestGraphNode);
			return;
		}
		GraphNode graphNode = null;
		GraphNode graphNode2 = null;
		if (IsVerticalDoor)
		{
			graphNode = NavMeshUtil.GetNearestGraphNode(m_CachedPosition + Direction.m_vUp * 0.5f);
			graphNode2 = NavMeshUtil.GetNearestGraphNode(m_CachedPosition + Direction.m_vDown * 0.5f);
		}
		else
		{
			graphNode = NavMeshUtil.GetNearestGraphNode(m_CachedPosition + Direction.m_vLeft * 0.5f);
			graphNode2 = NavMeshUtil.GetNearestGraphNode(m_CachedPosition + Direction.m_vRight * 0.5f);
		}
		UpdateNode(graphNode);
		UpdateNode(graphNode2);
	}

	private void UpdateNode(GraphNode node)
	{
		if (node != null)
		{
			uint keyTag = (uint)AIMovement.GetKeyTag(m_DoorKeyColour);
			NavMeshUtil.SetNodeTag(node, keyTag);
		}
	}

	public void Cutscene_SetDoorOpen(bool shouldOpenDoor)
	{
		SetForceOpen(shouldOpenDoor);
	}

	public void SetForceOpen(bool open)
	{
		m_bDoorOpenThisFrame = open;
		m_bIsForceOpened = open;
	}

	public bool DoesContainCharacter(Character theCharacter)
	{
		if (m_OurCollider != null)
		{
			return m_OurCollider.bounds.Contains(theCharacter.m_CachedCurrentPosition);
		}
		return false;
	}

	public void SetTempAllowed(Character theCharacter)
	{
		if (!m_TempAllowedCharacter.Contains(theCharacter))
		{
			m_TempAllowedCharacter.Add(theCharacter);
		}
	}

	public void OnDrawGizmos()
	{
		string value = string.Empty;
		switch (m_DoorKeyColour)
		{
		case KeyFunctionality.KeyColour.Black:
			value = "Gizmo_Key.png";
			break;
		case KeyFunctionality.KeyColour.Cyan:
			value = "Gizmo_KeyCyan.png";
			break;
		case KeyFunctionality.KeyColour.Red:
			value = "Gizmo_KeyRed.png";
			break;
		case KeyFunctionality.KeyColour.Green:
			value = "Gizmo_KeyGreen.png";
			break;
		case KeyFunctionality.KeyColour.Yellow:
			value = "Gizmo_KeyYellow.png";
			break;
		case KeyFunctionality.KeyColour.Purple:
			value = "Gizmo_KeyPurple.png";
			break;
		}
		if (!string.IsNullOrEmpty(value))
		{
			Gizmos.DrawIcon(base.transform.position - new Vector3(0f, 0f, 1.5f), value, allowScaling: true);
		}
		if (m_OneWayWhenLocked != 0)
		{
			Vector3 vector = base.transform.position - new Vector3(0f, 0f, 1.5f);
			switch (m_OneWayWhenLocked)
			{
			case DoorOneWayDirection.LeftToRight:
				Gizmos.DrawLine(vector - new Vector3(0.3f, 0f), vector + new Vector3(0.4f, 0f));
				Gizmos.DrawLine(vector + new Vector3(0.15f, 0.2f), vector + new Vector3(0.4f, 0f));
				Gizmos.DrawLine(vector + new Vector3(0.15f, -0.2f), vector + new Vector3(0.4f, 0f));
				break;
			case DoorOneWayDirection.RightToLeft:
				Gizmos.DrawLine(vector + new Vector3(0.3f, 0f), vector - new Vector3(0.4f, 0f));
				Gizmos.DrawLine(vector - new Vector3(0.15f, 0.2f), vector - new Vector3(0.4f, 0f));
				Gizmos.DrawLine(vector - new Vector3(0.15f, -0.2f), vector - new Vector3(0.4f, 0f));
				break;
			case DoorOneWayDirection.TopToBottom:
				Gizmos.DrawLine(vector + new Vector3(0f, 0.3f), vector - new Vector3(0f, 0.4f));
				Gizmos.DrawLine(vector - new Vector3(0.2f, 0.15f), vector - new Vector3(0f, 0.4f));
				Gizmos.DrawLine(vector - new Vector3(-0.2f, 0.15f), vector - new Vector3(0f, 0.4f));
				break;
			case DoorOneWayDirection.BottomToTop:
				Gizmos.DrawLine(vector - new Vector3(0f, 0.3f), vector + new Vector3(0f, 0.4f));
				Gizmos.DrawLine(vector + new Vector3(0.2f, 0.15f), vector + new Vector3(0f, 0.4f));
				Gizmos.DrawLine(vector + new Vector3(-0.2f, 0.15f), vector + new Vector3(0f, 0.4f));
				break;
			}
		}
	}
}

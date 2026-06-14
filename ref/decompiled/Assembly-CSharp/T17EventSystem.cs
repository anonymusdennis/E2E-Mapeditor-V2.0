using System.Collections.Generic;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RewiredStandaloneInputModule))]
public class T17EventSystem : EventSystem
{
	public enum InputCateogryStates
	{
		Assignment,
		MainFrontend,
		Frontend,
		InGame,
		InGameMenu,
		InGamePaused,
		InGameMainMap,
		Cutscenes,
		InputField,
		Loading,
		Disabled,
		PlayerSelect,
		Dialogbox,
		LevelEditor,
		InGameMouseOnHUD
	}

	public const string RW_DEFAULT = "Default";

	public const string RW_ASSIGN = "Assignment";

	public const string RW_FRONTEND = "Frontend";

	public const string RW_DEBUGKEYS = "DebugKeys";

	public const string RW_INGAME = "InGame";

	public const string RW_UI = "UI";

	public const string RW_MINIGAMES = "MiniGames";

	public const string RW_MAINMAP = "MainMap";

	public const string RW_CUTSCENES = "Cutscenes";

	public const string RW_INPUTFIELD = "InputField";

	public const string RW_PLAYERSELECT = "PlayerSelect";

	public const string RW_LEVELEDITOR = "LevelEditor";

	public const string RW_INGAMEMOUSE = "InGameMouse";

	public const string RW_PCPAUSE = "PCPause";

	public static readonly string[] Map_Categories = new string[13]
	{
		"Default", "Assignment", "Frontend", "UI", "InputField", "PlayerSelect", "InGame", "MainMap", "MiniGames", "Cutscenes",
		"DebugKeys", "LevelEditor", "InGameMouse"
	};

	public static bool DEBUG_LogSelectedObjects = false;

	private static Dictionary<int, InputCateogryStates> s_PreviousState = new Dictionary<int, InputCateogryStates>();

	private static Dictionary<int, InputCateogryStates> s_CurrentState = new Dictionary<int, InputCateogryStates>();

	private static Dictionary<int, InputCateogryStates> s_LastRequestedState = new Dictionary<int, InputCateogryStates>();

	[HideInInspector]
	public RewiredStandaloneInputModule m_RewiredInputModule;

	private GameObject m_LastRequestedSelectedGameobject;

	private GameObject m_CurrentPointerOverGameobject;

	private Gamer m_AssignedGamer;

	public Gamer AssignedGamer => m_AssignedGamer;

	public static void ApplyCategories(int id, InputCateogryStates state)
	{
		Rewired.Player player = ReInput.players.GetPlayer(id);
		if (player != null)
		{
			ApplyCategories(player, state);
		}
	}

	public static void ResetCategoryData()
	{
		s_PreviousState.Clear();
		s_CurrentState.Clear();
		s_LastRequestedState.Clear();
	}

	public static void ApplyCategories(Rewired.Player player, InputCateogryStates state)
	{
		if (player == null)
		{
			return;
		}
		if (state == InputCateogryStates.Assignment)
		{
			Platform.GetInstance().RemoveRewiredPlayer(player.id);
		}
		if (s_CurrentState.ContainsKey(player.id))
		{
			if (s_CurrentState[player.id] == state)
			{
				InternalApply(player, state);
				return;
			}
			if (state != InputCateogryStates.Dialogbox)
			{
				if (state != 0)
				{
					s_LastRequestedState[player.id] = state;
				}
				if (s_CurrentState[player.id] == InputCateogryStates.Dialogbox)
				{
					return;
				}
				s_PreviousState[player.id] = s_CurrentState[player.id];
			}
		}
		else if (state != InputCateogryStates.Dialogbox)
		{
			if (state != 0)
			{
				s_LastRequestedState[player.id] = state;
			}
			s_PreviousState[player.id] = state;
		}
		InternalApply(player, state);
	}

	private static void InternalApply(Rewired.Player player, InputCateogryStates state)
	{
		s_CurrentState[player.id] = state;
		Rewired.Player.ControllerHelper.MapHelper maps = player.controllers.maps;
		maps.SetAllMapsEnabled(state: false);
		maps.SetMapsEnabled(state: true, "DebugKeys");
		switch (state)
		{
		case InputCateogryStates.Assignment:
			maps.SetMapsEnabled(state: true, "Assignment");
			break;
		case InputCateogryStates.MainFrontend:
			maps.SetMapsEnabled(state: true, "Frontend");
			maps.SetMapsEnabled(state: true, "UI");
			break;
		case InputCateogryStates.Frontend:
			maps.SetMapsEnabled(state: true, "UI");
			break;
		case InputCateogryStates.InGame:
			maps.SetMapsEnabled(state: true, "Default");
			maps.SetMapsEnabled(state: true, "UI");
			maps.SetMapsEnabled(state: true, "InGame");
			maps.SetMapsEnabled(state: true, "MainMap");
			maps.SetMapsEnabled(state: true, "MiniGames");
			maps.SetMapsEnabled(state: true, "InGameMouse");
			maps.SetMapsEnabled(state: true, "PCPause");
			break;
		case InputCateogryStates.InGameMenu:
		case InputCateogryStates.InGamePaused:
			maps.SetMapsEnabled(state: true, "Default");
			maps.SetMapsEnabled(state: true, "UI");
			break;
		case InputCateogryStates.InGameMainMap:
			maps.SetMapsEnabled(state: true, "Default");
			maps.SetMapsEnabled(state: true, "UI");
			maps.SetMapsEnabled(state: true, "MainMap");
			break;
		case InputCateogryStates.Cutscenes:
			maps.SetMapsEnabled(state: true, "Cutscenes");
			break;
		case InputCateogryStates.InputField:
			maps.SetMapsEnabled(state: true, "InputField");
			break;
		case InputCateogryStates.PlayerSelect:
			maps.SetMapsEnabled(state: true, "PlayerSelect");
			break;
		case InputCateogryStates.Dialogbox:
			maps.SetMapsEnabled(state: true, "UI");
			break;
		case InputCateogryStates.LevelEditor:
			maps.SetMapsEnabled(state: true, "UI");
			maps.SetMapsEnabled(state: true, "LevelEditor");
			break;
		case InputCateogryStates.InGameMouseOnHUD:
			maps.SetMapsEnabled(state: true, "Default");
			maps.SetMapsEnabled(state: true, "UI");
			maps.SetMapsEnabled(state: true, "InGame");
			maps.SetMapsEnabled(state: true, "MainMap");
			maps.SetMapsEnabled(state: true, "MiniGames");
			maps.SetMapsEnabled(state: true, "PCPause");
			break;
		case InputCateogryStates.Loading:
		case InputCateogryStates.Disabled:
			break;
		}
	}

	public static void SetCategoriesBackToPreviousState(int id)
	{
		Rewired.Player player = ReInput.players.GetPlayer(id);
		if (player != null)
		{
			SetCategoriesBackToPreviousState(player);
		}
	}

	public static void SetCategoriesBackToPreviousState(Rewired.Player player)
	{
		if (player != null && s_PreviousState.ContainsKey(player.id))
		{
			ApplyCategories(player, s_PreviousState[player.id]);
		}
	}

	public static void ReApplyCurrentState(int id)
	{
		Rewired.Player player = ReInput.players.GetPlayer(id);
		if (player != null)
		{
			ReApplyCurrentState(player);
		}
	}

	public static void ReApplyCurrentState(Rewired.Player player)
	{
		if (player != null && s_CurrentState.ContainsKey(player.id))
		{
			ApplyCategories(player, s_CurrentState[player.id]);
		}
	}

	public static void ApplyLastRequestedStateSinceDialogboxWasUp(Rewired.Player player)
	{
		if (player != null)
		{
			if (s_LastRequestedState.ContainsKey(player.id))
			{
				InternalApply(player, s_LastRequestedState[player.id]);
			}
			else
			{
				SetCategoriesBackToPreviousState(player);
			}
		}
	}

	public static InputCateogryStates GetStateForRewiredPlayer(int id)
	{
		Rewired.Player player = ReInput.players.GetPlayer(id);
		if (player != null)
		{
			return GetStateForRewiredPlayer(player);
		}
		return InputCateogryStates.Disabled;
	}

	public static InputCateogryStates GetStateForRewiredPlayer(Rewired.Player player)
	{
		if (player == null)
		{
			return InputCateogryStates.Disabled;
		}
		if (s_CurrentState.ContainsKey(player.id))
		{
			return s_CurrentState[player.id];
		}
		return InputCateogryStates.Disabled;
	}

	public static InputCateogryStates GetPrevStateForRewiredPlayer(Rewired.Player player)
	{
		if (player == null)
		{
			return InputCateogryStates.Disabled;
		}
		if (s_PreviousState.ContainsKey(player.id))
		{
			return s_PreviousState[player.id];
		}
		return InputCateogryStates.Disabled;
	}

	public static InputCateogryStates GetLastReqStateForRewiredPlayer(Rewired.Player player)
	{
		if (player == null)
		{
			return InputCateogryStates.Disabled;
		}
		if (s_LastRequestedState.ContainsKey(player.id))
		{
			return s_LastRequestedState[player.id];
		}
		return InputCateogryStates.Disabled;
	}

	public static void SetPrevStateForRewiredPlayer(Rewired.Player player, InputCateogryStates state)
	{
		if (player != null && s_PreviousState.ContainsKey(player.id))
		{
			s_PreviousState[player.id] = state;
		}
	}

	public static void SetLastReqStateForRewiredPlayer(Rewired.Player player, InputCateogryStates state)
	{
		if (player != null && s_LastRequestedState.ContainsKey(player.id))
		{
			s_LastRequestedState[player.id] = state;
		}
	}

	protected override void OnEnable()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		m_RewiredInputModule = GetComponent<RewiredStandaloneInputModule>();
		T17EventSystemsManager.Instance.RegisterEventSystem(this);
	}

	public void ResetSystem()
	{
		m_AssignedGamer = null;
		m_RewiredInputModule.enabled = false;
		Gamer.OnUpdated -= GamerUpdated;
	}

	protected override void Update()
	{
		EventSystem eventSystem = EventSystem.current;
		EventSystem.current = this;
		base.Update();
		EventSystem.current = eventSystem;
	}

	protected override void OnApplicationFocus(bool bHasFocus)
	{
		InGameMenuFlow instance = InGameMenuFlow.Instance;
		if (!(instance == null))
		{
			if (!bHasFocus)
			{
				instance.OnPlatformPauseRequest();
			}
			else
			{
				instance.OnPlatformUnpauseRequest();
			}
		}
	}

	public void SetAssignedGamer(Gamer gamer)
	{
		m_AssignedGamer = gamer;
		if (gamer == null)
		{
			m_RewiredInputModule.enabled = false;
			return;
		}
		m_RewiredInputModule.enabled = true;
		m_RewiredInputModule.RewiredPlayerIds = new int[1] { gamer.m_iControllerIndex };
		Gamer.OnUpdated -= GamerUpdated;
		Gamer.OnUpdated += GamerUpdated;
	}

	private void GamerUpdated(Gamer gamer)
	{
		if (m_AssignedGamer == gamer)
		{
			m_RewiredInputModule.enabled = true;
			m_RewiredInputModule.RewiredPlayerIds = new int[1] { gamer.m_iControllerIndex };
		}
	}

	public void ForceDeselectSelectionObject()
	{
		base.SetSelectedGameObject((GameObject)null);
		m_LastRequestedSelectedGameobject = null;
	}

	public void SetSelectedGameObject(GameObject target, bool forceSet)
	{
		if (forceSet)
		{
			base.SetSelectedGameObject(target);
		}
		else
		{
			SetSelectedGameObject(target);
		}
	}

	public new void SetSelectedGameObject(GameObject target)
	{
		if (T17RewiredStandaloneInputModule.IsControllerDrivingInput(m_AssignedGamer))
		{
			base.SetSelectedGameObject(target);
		}
		if (target != null)
		{
			IT17EventHelper component = target.GetComponent<IT17EventHelper>();
			if (component == null || component.CanReselectOnMouseDisable())
			{
				m_LastRequestedSelectedGameobject = target;
			}
		}
		else
		{
			m_LastRequestedSelectedGameobject = target;
		}
	}

	public new void SetSelectedGameObject(GameObject selected, BaseEventData pointer)
	{
		base.SetSelectedGameObject(selected, pointer);
	}

	public GameObject GetLastRequestedSelectedGameobject()
	{
		return m_LastRequestedSelectedGameobject;
	}

	public void SetCurrentPointerOverGameobject(GameObject pointerOver)
	{
		m_CurrentPointerOverGameobject = pointerOver;
	}

	public GameObject GetCurrentPointerOverGameobject()
	{
		return m_CurrentPointerOverGameobject;
	}

	public static bool Debug_SetLogSelectedGameobjects(bool value, bool readOnly)
	{
		if (!readOnly)
		{
			DEBUG_LogSelectedObjects = value;
		}
		return DEBUG_LogSelectedObjects;
	}

	private void DEBUG_EventSystemLog(string message)
	{
		if (DEBUG_LogSelectedObjects)
		{
			string message2 = base.transform.name + ": " + message;
			Debug.LogError(message2);
		}
	}
}

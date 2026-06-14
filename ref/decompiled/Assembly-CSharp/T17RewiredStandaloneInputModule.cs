using System.Collections.Generic;
using System.Linq;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(T17EventSystem))]
public class T17RewiredStandaloneInputModule : RewiredStandaloneInputModule
{
	public delegate void InputMethodHandler();

	private static List<string> m_Action1Keys = new List<string>();

	private static List<string> m_Action2Keys = new List<string>();

	private static string m_Action1LastInputAction = string.Empty;

	private static string m_Action2LastInputAction = string.Empty;

	private bool m_bWasUsingController = true;

	private static bool m_bRefreshAllText = false;

	public static List<T17Text> m_TextToRefresh = new List<T17Text>();

	private static ControlSetting m_CurrentPCControllerMode = ControlSetting.KeyboardAndPad;

	public const string ACTION_ASSIGNMENT_START = "Start";

	public const string ACTION_CUTSCENE_SKIP = "Skip";

	public const string ACTION_UI_CLOSE = "UI_Close";

	public const string ACTION_UI_DROP = "UI_Drop";

	public const string ACTION_UI_HUD_DROP = "HUD_Drop";

	public const string ACTION_MAINMAP_CLOSEMAP = "CloseMap";

	public const string ACTION_INPUTFIELD_CANCEL = "Cancel";

	public const string ACTION_INPUTFIELD_ACCEPT = "Accept";

	public const string ACTION_PLAYERSELECT_SELECT = "PS_SelectPlayer";

	private bool m_bMouseActive = true;

	private static T17RewiredStandaloneInputModule s_TheMouseOwner = null;

	private static T17RewiredStandaloneInputModule s_ActiveModule = null;

	private GameObject m_LastControllerSelected;

	private GameObject m_LastMouseHoveredObject;

	private T17EventSystem m_t17EventSystem;

	public bool m_bCanBeMouseOwner;

	public static event InputMethodHandler InputMethodChangedEvent;

	protected override void Awake()
	{
		base.Awake();
		m_t17EventSystem = GetComponent<T17EventSystem>();
		base.allowMouseInputIfTouchSupported = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (s_TheMouseOwner == this)
		{
			s_TheMouseOwner = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		InitialiseMouseActive();
	}

	public void InitialiseMouseActive()
	{
		if (m_bCanBeMouseOwner)
		{
			m_bMouseActive = Cursor.visible;
		}
	}

	private void DisableMouse()
	{
		m_bMouseActive = false;
		Cursor.visible = false;
		m_bProcessMouseEvents = false;
		if (m_t17EventSystem != null)
		{
			m_t17EventSystem.SetCurrentPointerOverGameobject(null);
		}
		if (!base.isMouseSupported)
		{
			return;
		}
		T17InputField t17InputField = null;
		if (base.eventSystem.currentSelectedGameObject != null)
		{
			t17InputField = base.eventSystem.currentSelectedGameObject.GetComponent<T17InputField>();
		}
		if (t17InputField == null || !t17InputField.isFocused)
		{
			GameObject lastRequestedSelectedGameobject = m_t17EventSystem.GetLastRequestedSelectedGameobject();
			ClearSelection();
			if (m_LastMouseHoveredObject != null && m_LastMouseHoveredObject.activeInHierarchy)
			{
				base.eventSystem.SetSelectedGameObject(m_LastMouseHoveredObject);
			}
			else if (m_LastControllerSelected != null && m_LastControllerSelected.activeInHierarchy)
			{
				base.eventSystem.SetSelectedGameObject(m_LastControllerSelected);
			}
			else
			{
				base.eventSystem.SetSelectedGameObject(lastRequestedSelectedGameobject);
			}
		}
	}

	private void EnableMouse()
	{
		m_bMouseActive = true;
		Cursor.visible = true;
		m_bProcessMouseEvents = true;
		if (base.eventSystem.currentSelectedGameObject != null)
		{
			IT17EventHelper component = base.eventSystem.currentSelectedGameObject.GetComponent<IT17EventHelper>();
			if (component == null || component.CanReselectOnMouseDisable())
			{
				m_LastControllerSelected = base.eventSystem.currentSelectedGameObject;
			}
		}
		else
		{
			m_LastControllerSelected = base.eventSystem.currentSelectedGameObject;
		}
		T17InputField t17InputField = null;
		if (base.eventSystem.currentSelectedGameObject != null)
		{
			t17InputField = base.eventSystem.currentSelectedGameObject.GetComponent<T17InputField>();
		}
		if (t17InputField == null || !t17InputField.isFocused)
		{
			base.eventSystem.SetSelectedGameObject(null);
		}
	}

	private void UpdateActiveConfig()
	{
		bool flag = false;
		if (!m_bCanBeMouseOwner || (!(s_TheMouseOwner == this) && !(s_TheMouseOwner == null)))
		{
			return;
		}
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (IsMouseActive())
		{
			if (s_TheMouseOwner == null)
			{
				s_TheMouseOwner = this;
			}
			flag2 = true;
		}
		if (IsControllerActive())
		{
			flag3 = true;
		}
		if (IsKeyboardActive())
		{
			flag4 = true;
		}
		bool flag5 = false;
		bool flag6 = true;
		Rewired.Player rewiredPlayer = m_t17EventSystem.AssignedGamer.m_RewiredPlayer;
		if (rewiredPlayer != null && rewiredPlayer.controllers.hasMouse)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(rewiredPlayer);
			if (stateForRewiredPlayer == T17EventSystem.InputCateogryStates.InGame || stateForRewiredPlayer == T17EventSystem.InputCateogryStates.InGameMouseOnHUD)
			{
				flag6 = false;
			}
		}
		if (m_bMouseActive && !flag2 && (flag3 || (flag4 && flag6)) && !T17BlockKeyboardAutoFocus.IsAutoFocusBlocked())
		{
			DisableMouse();
			if (T17RewiredStandaloneInputModule.InputMethodChangedEvent != null)
			{
				T17RewiredStandaloneInputModule.InputMethodChangedEvent();
			}
		}
		if (flag2 && !m_bMouseActive)
		{
			EnableMouse();
			if (T17RewiredStandaloneInputModule.InputMethodChangedEvent != null)
			{
				T17RewiredStandaloneInputModule.InputMethodChangedEvent();
			}
		}
		bool flag7 = false;
		if (rewiredPlayer != null)
		{
			Rewired.Player.ControllerHelper controllers = rewiredPlayer.controllers;
			if (controllers != null)
			{
				Controller lastActiveController = controllers.GetLastActiveController();
				if (lastActiveController != null)
				{
					bool bWasUsingController = m_bWasUsingController;
					m_bWasUsingController = lastActiveController.type == ControllerType.Joystick && !m_bMouseActive;
					flag7 = bWasUsingController != m_bWasUsingController;
				}
			}
		}
		if (!flag7 && !m_bRefreshAllText)
		{
			return;
		}
		m_bRefreshAllText = false;
		for (int num = m_TextToRefresh.Count - 1; num >= 0; num--)
		{
			if (m_TextToRefresh[num] == null)
			{
				m_TextToRefresh.RemoveAt(num);
			}
			else if (m_TextToRefresh[num].GetOwnersRewiredId() == rewiredPlayer.id)
			{
				if (m_TextToRefresh[num].m_bTagFound)
				{
					m_TextToRefresh[num].Convert();
				}
				else
				{
					m_TextToRefresh[num].CheckMarkup();
				}
				m_TextToRefresh[num].SetVerticesDirty();
			}
		}
	}

	public override void Process()
	{
		s_ActiveModule = this;
		base.Process();
		UpdateActiveConfig();
		s_ActiveModule = null;
	}

	public static void ForceTextRefresh()
	{
		m_bRefreshAllText = true;
	}

	public void ClearPreviousSelectables()
	{
		m_LastMouseHoveredObject = null;
		m_LastControllerSelected = null;
		m_t17EventSystem.SetSelectedGameObject(null, forceSet: true);
	}

	public static bool IsControllerDrivingInput(Gamer theGamer)
	{
		if (s_TheMouseOwner == null)
		{
			return true;
		}
		if (theGamer == null || theGamer.m_RewiredPlayer == null)
		{
			return false;
		}
		if (s_TheMouseOwner.IsModuleWatchingPlayer(theGamer.m_RewiredPlayer))
		{
			return !s_TheMouseOwner.m_bMouseActive;
		}
		return true;
	}

	public static void EventHelperPointerEntered(IT17EventHelper eventHelper)
	{
		if (s_TheMouseOwner != null && eventHelper.GetDomain() == s_TheMouseOwner.m_t17EventSystem && eventHelper.CanReselectOnMouseDisable())
		{
			GameObject gameobject = eventHelper.GetGameobject();
			if (gameobject != null && gameobject.GetComponent<Selectable>() != null)
			{
				s_TheMouseOwner.m_LastMouseHoveredObject = eventHelper.GetGameobject();
			}
		}
	}

	public static void EventHelperPointerExited(IT17EventHelper eventHelper)
	{
	}

	public static bool ShouldSelectableAcceptPointer(IT17EventHelper eventHelper)
	{
		if (s_TheMouseOwner != null)
		{
			T17EventSystem domain = eventHelper.GetDomain();
			if (domain == null)
			{
				return true;
			}
			if (domain == s_TheMouseOwner.m_t17EventSystem)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool ShouldSelectableAcceptSubmit(IT17EventHelper eventHelper)
	{
		if (s_ActiveModule == null || eventHelper.GetDomain() == null)
		{
			return true;
		}
		if (s_ActiveModule.m_t17EventSystem == eventHelper.GetDomain())
		{
			return true;
		}
		return false;
	}

	public static bool IsCurrentActiveModuleUsingController()
	{
		if (s_ActiveModule != null && s_ActiveModule != s_TheMouseOwner)
		{
			return true;
		}
		if (s_ActiveModule != null)
		{
			return !s_ActiveModule.m_bMouseActive;
		}
		return false;
	}

	public static bool IsCurrentActiveModuleUsingKeyboard()
	{
		if (s_ActiveModule == null)
		{
			return false;
		}
		return s_ActiveModule.IsKeyboardActive();
	}

	public static void GetKeyboardInputsForAction(Rewired.Player player, ref List<string> actionInputList, string actionName)
	{
		actionInputList.Clear();
		IEnumerable<ActionElementMap> source = player.controllers.maps.ButtonMapsWithAction(actionName, skipDisabledMaps: true);
		for (int i = 0; i < source.Count(); i++)
		{
			ActionElementMap actionElementMap = source.ElementAt(i);
			if (actionElementMap.keyboardKeyCode != 0)
			{
				actionInputList.Add(actionElementMap.elementIdentifierName);
			}
		}
	}

	public static bool UsedSharedKeyboardAction(string action1, string action2, Rewired.Player rewiredPlayer)
	{
		if (string.IsNullOrEmpty(action1) || string.IsNullOrEmpty(action2) || rewiredPlayer == null)
		{
			return false;
		}
		bool flag = false;
		IList<InputActionSourceData> currentInputSources = rewiredPlayer.GetCurrentInputSources(action1);
		for (int num = currentInputSources.Count() - 1; num >= 0; num--)
		{
			if (currentInputSources[num].controllerType == ControllerType.Joystick)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return false;
		}
		if (m_Action1Keys.Count == 0 || m_Action1LastInputAction != action1)
		{
			GetKeyboardInputsForAction(rewiredPlayer, ref m_Action1Keys, action1);
			m_Action1LastInputAction = action1;
		}
		if (m_Action2Keys.Count == 0 || m_Action2LastInputAction != action2)
		{
			GetKeyboardInputsForAction(rewiredPlayer, ref m_Action2Keys, action2);
			m_Action2LastInputAction = action2;
		}
		List<string> list = new List<string>();
		for (int num2 = currentInputSources.Count() - 1; num2 >= 0; num2--)
		{
			if (currentInputSources[num2].controllerType == ControllerType.Keyboard)
			{
				list.Add(currentInputSources[num2].elementIdentifierName);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		for (int num3 = m_Action1Keys.Count() - 1; num3 >= 0; num3--)
		{
			for (int num4 = m_Action2Keys.Count() - 1; num4 >= 0; num4--)
			{
				if (m_Action1Keys[num3] == m_Action2Keys[num4])
				{
					for (int num5 = list.Count() - 1; num5 >= 0; num5--)
					{
						if (m_Action1Keys[num3] == list[num5])
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public static void SetPCKeyboardMode(ControlSetting setting)
	{
		m_CurrentPCControllerMode = setting;
		switch (setting)
		{
		case ControlSetting.KeyboardAndPad:
		{
			IList<Rewired.Player> players2 = ReInput.players.Players;
			int num2 = players2.FindIndex((Rewired.Player x) => x.controllers.hasMouse);
			if (num2 == -1)
			{
				break;
			}
			Rewired.Player player3 = players2[num2];
			if (player3.controllers.joystickCount != 0)
			{
				break;
			}
			int j = 0;
			for (int count2 = players2.Count; j < count2; j++)
			{
				Rewired.Player player4 = players2[j];
				if (player4 != player3 && player4.controllers.joystickCount > 0)
				{
					player3.controllers.AddController(player4.controllers.Joysticks[0], removeFromOtherPlayers: true);
					T17EventSystem.ReApplyCurrentState(player3);
					break;
				}
			}
			break;
		}
		case ControlSetting.Keyboard:
		{
			IList<Rewired.Player> players = ReInput.players.Players;
			int num = players.FindIndex((Rewired.Player x) => x.controllers.hasMouse);
			if (num == -1)
			{
				break;
			}
			Rewired.Player player = players[num];
			if (player.controllers.joystickCount == 0)
			{
				break;
			}
			int i = 0;
			for (int count = players.Count; i < count; i++)
			{
				Rewired.Player player2 = players[i];
				if (player2 != player && player2.controllers.Joysticks.Count == 0)
				{
					player2.controllers.AddController(player.controllers.Joysticks[0], removeFromOtherPlayers: true);
					T17EventSystem.ReApplyCurrentState(player2);
					break;
				}
			}
			break;
		}
		}
	}

	public static ControlSetting GetCurrentPCKeyboardMode()
	{
		return m_CurrentPCControllerMode;
	}

	private static void LogWithObjectTrace(string message, Transform transform = null)
	{
		string text = "T17RewiredStandaloneInputModule: " + message;
		if (transform != null)
		{
			text = text + " Object Path: " + GetObjectTraceForTransform(transform) + "\n\n";
		}
	}

	private static string GetObjectTraceForTransform(Transform transform)
	{
		string text = transform.transform.name;
		Transform parent = transform.transform;
		while (parent.parent != null)
		{
			text = parent.parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}
}

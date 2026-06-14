using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class T17ControlMapper
{
	private struct SharedInputAction
	{
		public KeyCode m_KeyCode;

		public Pole m_AxisContribution;

		public bool m_bIsAxisMapoping;

		public List<InputAction> m_SharedActions;

		public SharedInputAction(KeyCode keyCode, Pole axisContrib, bool bIsAxisMapping, List<InputAction> actions)
		{
			m_KeyCode = keyCode;
			m_AxisContribution = axisContrib;
			m_bIsAxisMapoping = bIsAxisMapping;
			m_SharedActions = actions;
		}
	}

	public delegate void T17ControlMapperIteratorDelegate(Rewired.Player rewiredPlayer, Controller controller, ControllerMap controllerMap, InputAction action, ActionElementMap aem);

	private bool m_bHasSetUpSharedActions;

	private List<SharedInputAction> m_SharedInputActions;

	private Dictionary<Rewired.Player, List<InputMapData>> m_PlayerInputMapData;

	private static T17ControlMapper m_sInstance;

	public static T17ControlMapper Instance
	{
		get
		{
			if (m_sInstance == null)
			{
				m_sInstance = new T17ControlMapper();
			}
			return m_sInstance;
		}
	}

	private T17ControlMapper()
	{
		m_bHasSetUpSharedActions = false;
		m_SharedInputActions = new List<SharedInputAction>();
		m_PlayerInputMapData = new Dictionary<Rewired.Player, List<InputMapData>>();
	}

	public void SetUpSharedMaps()
	{
		if (m_bHasSetUpSharedActions)
		{
			return;
		}
		T17ControlMapperIteratorDelegate iteratorFunc = delegate(Rewired.Player rewiredPlayer, Controller controller, ControllerMap controllerMap, InputAction action, ActionElementMap aem)
		{
			int num = m_SharedInputActions.FindIndex((SharedInputAction sharedAction) => sharedAction.m_KeyCode == aem.keyCode && (action.type != 0 || !sharedAction.m_bIsAxisMapoping || (action.type == InputActionType.Axis && sharedAction.m_bIsAxisMapoping && sharedAction.m_AxisContribution == aem.axisContribution)));
			if (num == -1)
			{
				m_SharedInputActions.Add(new SharedInputAction(aem.keyCode, aem.axisContribution, action.type == InputActionType.Axis, new List<InputAction>()));
				if (!m_SharedInputActions[m_SharedInputActions.Count - 1].m_SharedActions.Contains(action))
				{
					m_SharedInputActions[m_SharedInputActions.Count - 1].m_SharedActions.Add(action);
				}
			}
			else if (!m_SharedInputActions[num].m_SharedActions.Contains(action))
			{
				m_SharedInputActions[num].m_SharedActions.Add(action);
			}
		};
		IterateOverAllMappings(ReInput.players.Players[0], ControllerType.Keyboard, -1, bUserAssignableOnly: false, iteratorFunc);
		m_bHasSetUpSharedActions = true;
	}

	public bool OnKeyboardElementAssignmentPollingUpdate(Rewired.Player player, int inputMapID)
	{
		if (player == null)
		{
			return false;
		}
		InputMapData mappingData = GetInputMapData(player, inputMapID);
		if (mappingData == null)
		{
			return false;
		}
		return OnKeyboardElementAssignmentPollingUpdate(ref mappingData);
	}

	private bool OnKeyboardElementAssignmentPollingUpdate(ref InputMapData mappingData)
	{
		PollKeyboardForAssignment(out var pollingInfo, out var _);
		if (!pollingInfo.success)
		{
			return false;
		}
		Rewired.Player rewiredPlayer = mappingData.m_RewiredPlayer;
		if (rewiredPlayer == null)
		{
			return false;
		}
		if (rewiredPlayer.GetButtonDown("UI_Close"))
		{
			return false;
		}
		mappingData.m_PollingInfo = pollingInfo;
		ElementAssignment assignment = CreateElementAssignment(mappingData, pollingInfo, ModifierKeyFlags.None);
		Rewired.Player.ControllerHelper controllers = rewiredPlayer.controllers;
		if (controllers == null)
		{
			return false;
		}
		Keyboard keyboard = controllers.Keyboard;
		if (keyboard == null)
		{
			return false;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		Rewired.Player.ControllerHelper.MapHelper maps = controllers.maps;
		if (mapCategories == null || maps == null)
		{
			return false;
		}
		List<ActionElementMap> conflictingMaps = null;
		if (HasElementAssignmentConflicts(mappingData, assignment, ref conflictingMaps) && conflictingMaps != null && conflictingMaps.Count > 0)
		{
			for (int num = mapCategories.Count - 1; num >= 0; num--)
			{
				if (mapCategories[num].userAssignable)
				{
					ControllerMap map = maps.GetMap(ControllerType.Keyboard, keyboard.id, mapCategories[num].name, "Default");
					if (map != null)
					{
						for (int i = conflictingMaps.Count - 1; i >= 0; i--)
						{
							if (map.AllMaps.FindIndex((ActionElementMap aem) => aem.id == conflictingMaps[i].id) == -1)
							{
								continue;
							}
							InputAction action2 = ReInput.mapping.GetAction(conflictingMaps[i].actionId);
							if (action2 != null && action2.userAssignable)
							{
								InputMapData inputMapData = new InputMapData(-1, rewiredPlayer, action2.name, map, conflictingMaps[i], ControllerType.Keyboard, action2.type == InputActionType.Axis, action2.negativeDescriptiveName, action2.positiveDescriptiveName);
								ElementAssignment elementAssignment = CreateElementAssignment(inputMapData, inputMapData.m_PollingInfo, ModifierKeyFlags.None);
								if (!map.ReplaceElementMap(elementAssignment))
								{
								}
								List<InputMapData> value2 = null;
								if (m_PlayerInputMapData.TryGetValue(rewiredPlayer, out value2))
								{
									InputMapData inputMapData2 = value2.Find((InputMapData value) => value.m_ActionElementMap.id == conflictingMaps[i].id);
									InputAction action3 = ReInput.mapping.GetAction(conflictingMaps[i].actionId);
									ActionElementMap[] elementMapsWithAction = map.GetElementMapsWithAction(action3.id);
									bool bIsAxisAction = action3.type == InputActionType.Axis;
									int num2 = elementMapsWithAction.FindIndex((ActionElementMap value) => value.keyCode == KeyCode.None && (!bIsAxisAction || (bIsAxisAction && value.axisContribution == conflictingMaps[i].axisContribution)));
									if (inputMapData2 != null && action3 != null && num2 != -1)
									{
										inputMapData2.SetMapInfo(rewiredPlayer, action3.name, map, elementMapsWithAction[num2], map.controllerType, bIsAxisAction, action3.negativeDescriptiveName, action3.positiveDescriptiveName);
									}
								}
							}
							conflictingMaps.RemoveAt(i);
						}
					}
				}
			}
		}
		for (int num3 = mapCategories.Count - 1; num3 >= 0; num3--)
		{
			if (mapCategories[num3].userAssignable)
			{
				ControllerMap map2 = maps.GetMap(ControllerType.Keyboard, keyboard.id, mapCategories[num3].name, "Default");
				if (map2 != null && map2.ContainsAction(assignment.actionId) && map2.categoryId != mappingData.m_ControllerMap.categoryId)
				{
					ActionElementMap firstElementMapWithAction = map2.GetFirstElementMapWithAction(assignment.actionId);
					InputAction action4 = ReInput.mapping.GetAction(firstElementMapWithAction.actionId);
					InputMapData mappingData2 = new InputMapData(-1, mappingData.m_RewiredPlayer, mappingData.m_ActionName, map2, firstElementMapWithAction, mappingData.m_ControllerType, mappingData.m_bIsAxisAction, action4.negativeDescriptiveName, action4.positiveDescriptiveName);
					ElementAssignment elementAssignment2 = CreateElementAssignment(mappingData2, pollingInfo, ModifierKeyFlags.None);
					if (map2.ReplaceElementMap(elementAssignment2))
					{
					}
				}
			}
		}
		if (!mappingData.m_ControllerMap.ReplaceElementMap(assignment))
		{
		}
		ActionElementMap[] buttonMapsWithAction = mappingData.m_ControllerMap.GetButtonMapsWithAction(assignment.actionId);
		int num4 = buttonMapsWithAction.FindIndex((ActionElementMap action) => action.keyCode == assignment.keyboardKey);
		if (num4 != -1)
		{
			mappingData.m_ActionElementMap = buttonMapsWithAction[num4];
		}
		return true;
	}

	public void PollKeyboardForAssignment(out ControllerPollingInfo pollingInfo, out string label)
	{
		pollingInfo = default(ControllerPollingInfo);
		label = string.Empty;
		ControllerPollingInfo controllerPollingInfo = default(ControllerPollingInfo);
		foreach (ControllerPollingInfo item in ReInput.controllers.Keyboard.PollForAllKeys())
		{
			KeyCode keyboardKey = item.keyboardKey;
			if (keyboardKey == KeyCode.AltGr)
			{
				continue;
			}
			controllerPollingInfo = item;
			break;
		}
		if (ReInput.controllers.Keyboard.GetKeyDown(controllerPollingInfo.keyboardKey))
		{
			pollingInfo = controllerPollingInfo;
		}
	}

	public ElementAssignment CreateElementAssignment(InputMapData mappingData, ControllerPollingInfo pollingInfo, ModifierKeyFlags modifierKeyFlags)
	{
		return new ElementAssignment(mappingData.m_ControllerType, pollingInfo.elementType, pollingInfo.elementIdentifierId, mappingData.PolarisedAxisRange, pollingInfo.keyboardKey, modifierKeyFlags, mappingData.m_ActionElementMap.actionId, mappingData.m_ActionElementMap.axisContribution, invert: false, mappingData.m_ActionElementMap.id);
	}

	public bool HasElementAssignmentConflicts(InputMapData mappingData, ElementAssignment assignment, ref List<ActionElementMap> conflictingMaps, bool skipOtherPlayers = true)
	{
		if (mappingData.m_RewiredPlayer == null)
		{
			return false;
		}
		if (!CreateConflictCheck(mappingData, assignment, out var conflictCheck))
		{
			return false;
		}
		if (skipOtherPlayers)
		{
			if (CheckForConflict(conflictCheck, ReInput.players.SystemPlayer, out conflictingMaps))
			{
				return true;
			}
			if (CheckForConflict(conflictCheck, mappingData.m_RewiredPlayer, out conflictingMaps))
			{
				return true;
			}
			return false;
		}
		return ReInput.controllers.conflictChecking.DoesElementAssignmentConflict(conflictCheck);
	}

	public bool CreateConflictCheck(InputMapData mappingData, ElementAssignment assignment, out ElementAssignmentConflictCheck conflictCheck)
	{
		if (mappingData.m_RewiredPlayer == null)
		{
			conflictCheck = default(ElementAssignmentConflictCheck);
			return false;
		}
		conflictCheck = assignment.ToElementAssignmentConflictCheck();
		conflictCheck.playerId = mappingData.m_RewiredPlayer.id;
		conflictCheck.controllerType = mappingData.m_ControllerType;
		conflictCheck.controllerId = mappingData.m_ControllerId;
		conflictCheck.controllerMapId = mappingData.m_ControllerMap.id;
		conflictCheck.controllerMapCategoryId = mappingData.m_ControllerMap.categoryId;
		conflictCheck.elementMapId = mappingData.m_ActionElementMap.id;
		return true;
	}

	public bool CheckForConflict(ElementAssignmentConflictCheck conflictCheck, Rewired.Player playerToCheckAgainst, out List<ActionElementMap> conflictingMapsOut)
	{
		Rewired.Player rewiredPlayer = playerToCheckAgainst;
		List<ActionElementMap> conflictingMaps = new List<ActionElementMap>();
		conflictingMapsOut = new List<ActionElementMap>();
		bool bFoundConflict = false;
		T17ControlMapperIteratorDelegate iteratorFunc = delegate(Rewired.Player player, Controller controller, ControllerMap controllerMap, InputAction action, ActionElementMap aem)
		{
			if (aem.keyCode == conflictCheck.keyboardKey)
			{
				for (int num = m_SharedInputActions.Count - 1; num >= 0; num--)
				{
					if (m_SharedInputActions[num].m_SharedActions.FindIndex((InputAction sharedAction) => sharedAction.id == conflictCheck.actionId) != -1)
					{
						if (m_SharedInputActions[num].m_SharedActions.FindIndex((InputAction sharedAction) => sharedAction.id == aem.actionId) == -1)
						{
							if (rewiredPlayer.id == conflictCheck.playerId)
							{
								conflictingMaps.Add(aem);
							}
							bFoundConflict = true;
						}
						InputAction action2 = ReInput.mapping.GetAction(conflictCheck.actionId);
						if (action2.type == InputActionType.Axis && action.type == InputActionType.Axis && m_SharedInputActions[num].m_AxisContribution == conflictCheck.axisContribution && m_SharedInputActions[num].m_AxisContribution != aem.axisContribution)
						{
							if (rewiredPlayer.id == conflictCheck.playerId)
							{
								conflictingMaps.Add(aem);
							}
							bFoundConflict = true;
						}
					}
				}
			}
		};
		IterateOverAllMappings(playerToCheckAgainst, ControllerType.Keyboard, -1, bUserAssignableOnly: false, iteratorFunc);
		conflictingMapsOut = conflictingMaps;
		return bFoundConflict;
	}

	public void IterateOverAllMappings(Rewired.Player player, ControllerType controllerType, int controllerIndex, bool bUserAssignableOnly, T17ControlMapperIteratorDelegate iteratorFunc)
	{
		Rewired.Player.ControllerHelper controllers = player.controllers;
		if (controllers == null)
		{
			return;
		}
		Controller controller = null;
		if (controllerType != 0)
		{
			return;
		}
		controller = controllers.Keyboard;
		if (controller == null || controller == null)
		{
			return;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		Rewired.Player.ControllerHelper.MapHelper maps = controllers.maps;
		if (mapCategories == null || maps == null)
		{
			return;
		}
		int count = mapCategories.Count;
		for (int i = 0; i < count; i++)
		{
			if (bUserAssignableOnly && !mapCategories[i].userAssignable)
			{
				continue;
			}
			ControllerMap map = maps.GetMap(controllerType, controller.id, mapCategories[i].name, "Default");
			if (map == null)
			{
				continue;
			}
			IEnumerable<InputAction> enumerable = ReInput.mapping.ActionsInCategory(mapCategories[i].name, sort: true);
			if (enumerable == null)
			{
				continue;
			}
			IEnumerator<InputAction> enumerator = enumerable.GetEnumerator();
			if (enumerator == null)
			{
				continue;
			}
			while (enumerator.MoveNext())
			{
				InputAction current = enumerator.Current;
				if (current != null && (!bUserAssignableOnly || current.userAssignable))
				{
					ActionElementMap[] elementMapsWithAction = map.GetElementMapsWithAction(current.id);
					int num = elementMapsWithAction.Length;
					for (int j = 0; j < num; j++)
					{
						iteratorFunc(player, controller, map, current, elementMapsWithAction[j]);
					}
				}
			}
		}
	}

	public void ResetPlayerBindingsToDefault(Rewired.Player player)
	{
		Rewired.Player.ControllerHelper controllers = player.controllers;
		if (controllers != null)
		{
			controllers.maps.ClearMaps(ControllerType.Keyboard, userAssignableOnly: true);
			controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
			T17EventSystem.ReApplyCurrentState(player);
			GeneratePlayerInputMapData(player);
			if (ReInput.userDataStore != null)
			{
				ReInput.userDataStore.Save();
			}
		}
	}

	public void GeneratePlayerInputMapData(Rewired.Player player)
	{
		List<InputMapData> mapList = null;
		bool flag = m_PlayerInputMapData.ContainsKey(player);
		if (flag)
		{
			m_PlayerInputMapData.TryGetValue(player, out mapList);
			if (mapList != null)
			{
				mapList.Clear();
			}
		}
		else
		{
			mapList = new List<InputMapData>();
		}
		T17ControlMapperIteratorDelegate iteratorFunc = delegate(Rewired.Player rewiredPlayer, Controller controller, ControllerMap controllerMap, InputAction action, ActionElementMap aem)
		{
			int num = GenerateNewInputMapID(mapList);
			if (num != -1)
			{
				InputMapData item = new InputMapData(num, rewiredPlayer, action.name, controllerMap, aem, controller.type, action.type == InputActionType.Axis, action.negativeDescriptiveName, action.positiveDescriptiveName);
				mapList.Add(item);
			}
		};
		IterateOverAllMappings(player, ControllerType.Keyboard, -1, bUserAssignableOnly: true, iteratorFunc);
		if (!flag)
		{
			m_PlayerInputMapData.Add(player, mapList);
		}
	}

	private int GenerateNewInputMapID(List<InputMapData> mapData)
	{
		if (mapData == null)
		{
			return -1;
		}
		int newID;
		for (newID = 0; mapData.FindIndex((InputMapData value) => value.InputMapDataID == newID) != -1; newID++)
		{
		}
		return newID;
	}

	public InputMapData GetInputMapData(Rewired.Player player, int inputMapDataID)
	{
		if (player == null)
		{
			return null;
		}
		if (!m_PlayerInputMapData.ContainsKey(player))
		{
			return null;
		}
		List<InputMapData> value2 = null;
		if (!m_PlayerInputMapData.TryGetValue(player, out value2))
		{
			return null;
		}
		return value2.Find((InputMapData value) => value.InputMapDataID == inputMapDataID);
	}

	public List<InputMapData> GetAllInputMapDataForPlayer(Rewired.Player player)
	{
		if (player == null)
		{
			return null;
		}
		if (!m_PlayerInputMapData.ContainsKey(player))
		{
			GeneratePlayerInputMapData(player);
		}
		List<InputMapData> value = null;
		if (!m_PlayerInputMapData.TryGetValue(player, out value))
		{
			return null;
		}
		return value;
	}
}

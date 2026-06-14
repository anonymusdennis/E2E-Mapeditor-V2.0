using System.Collections.Generic;
using Rewired;
using Rewired.Data;
using UnityEngine;

internal class T17UserDataStore : UserDataStore
{
	private class SavedControllerMapData
	{
		public string xml;

		public List<int> knownActionIds;

		public SavedControllerMapData(string xml, List<int> knownActionIds)
		{
			this.xml = xml;
			this.knownActionIds = knownActionIds;
		}

		public static List<string> GetXmlStringList(List<SavedControllerMapData> data)
		{
			List<string> list = new List<string>();
			if (data == null)
			{
				return list;
			}
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i] != null && !string.IsNullOrEmpty(data[i].xml))
				{
					list.Add(data[i].xml);
				}
			}
			return list;
		}
	}

	private const string thisScriptName = "T17UserDataStore";

	private const string editorLoadedMessage = "\nIf unexpected input issues occur, the loaded data may be outdated or invalid. Please delete your save data if this is the case.";

	[SerializeField]
	private bool isEnabled = true;

	[SerializeField]
	private bool loadDataOnStart = true;

	[SerializeField]
	private string playerPrefsKeyPrefix = "RewiredSaveData";

	public override void Save()
	{
		if (isEnabled)
		{
			SaveAll();
		}
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (isEnabled)
		{
			SaveControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		if (isEnabled)
		{
			SaveControllerDataNow(controllerType, controllerId);
		}
	}

	public override void SavePlayerData(int playerId)
	{
		if (isEnabled)
		{
			SavePlayerDataNow(playerId);
		}
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (isEnabled)
		{
			SaveInputBehaviorNow(playerId, behaviorId);
		}
	}

	public override void Load()
	{
		if (isEnabled)
		{
			T17ControlMapper.Instance.SetUpSharedMaps();
			LoadAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (isEnabled)
		{
			LoadControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		if (isEnabled)
		{
			LoadControllerDataNow(controllerType, controllerId);
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (isEnabled)
		{
			LoadPlayerDataNow(playerId);
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (isEnabled)
		{
			LoadInputBehaviorNow(playerId, behaviorId);
		}
	}

	protected override void OnInitialize()
	{
		if (loadDataOnStart)
		{
			Load();
		}
	}

	protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			LoadJoystickData(args.controllerId);
		}
	}

	protected override void OnControllerPreDiscconnect(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			SaveJoystickData(args.controllerId);
		}
	}

	protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled)
		{
		}
	}

	private int LoadAll()
	{
		int num = 0;
		IList<Rewired.Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			num += LoadPlayerDataNow(allPlayers[i]);
			T17ControlMapper.Instance.GeneratePlayerInputMapData(allPlayers[i]);
		}
		return num + LoadAllJoystickCalibrationData();
	}

	private int LoadPlayerDataNow(int playerId)
	{
		return LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private int LoadPlayerDataNow(Rewired.Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		num += LoadInputBehaviors(player.id);
		num += LoadControllerMaps(player.id, ControllerType.Keyboard, 0);
		num += LoadControllerMaps(player.id, ControllerType.Mouse, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			num += LoadControllerMaps(player.id, ControllerType.Joystick, joystick.id);
		}
		return num;
	}

	private int LoadAllJoystickCalibrationData()
	{
		int num = 0;
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			num += LoadJoystickCalibrationData(joysticks[i]);
		}
		return num;
	}

	private int LoadJoystickCalibrationData(Joystick joystick)
	{
		if (joystick == null)
		{
			return 0;
		}
		return joystick.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)) ? 1 : 0;
	}

	private int LoadJoystickCalibrationData(int joystickId)
	{
		return LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private int LoadJoystickData(int joystickId)
	{
		int num = 0;
		IList<Rewired.Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Rewired.Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				num += LoadControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		return num + LoadJoystickCalibrationData(joystickId);
	}

	private int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0;
		num += LoadControllerMaps(playerId, controllerType, controllerId);
		return num + LoadControllerDataNow(controllerType, controllerId);
	}

	private int LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		int num = 0;
		if (controllerType == ControllerType.Joystick)
		{
			num += LoadJoystickCalibrationData(controllerId);
		}
		return num;
	}

	private int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0;
		Rewired.Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return num;
		}
		Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller == null)
		{
			return num;
		}
		List<SavedControllerMapData> allControllerMapsXml = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, controller);
		if (allControllerMapsXml.Count == 0)
		{
			return num;
		}
		num += player.controllers.maps.AddMapsFromXml(controllerType, controllerId, SavedControllerMapData.GetXmlStringList(allControllerMapsXml));
		AddDefaultMappingsForNewActions(player, allControllerMapsXml, controllerType, controllerId);
		return num;
	}

	private int LoadInputBehaviors(int playerId)
	{
		Rewired.Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
		for (int i = 0; i < inputBehaviors.Count; i++)
		{
			num += LoadInputBehaviorNow(player, inputBehaviors[i]);
		}
		return num;
	}

	private int LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		Rewired.Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
		if (inputBehavior == null)
		{
			return 0;
		}
		return LoadInputBehaviorNow(player, inputBehavior);
	}

	private int LoadInputBehaviorNow(Rewired.Player player, InputBehavior inputBehavior)
	{
		if (player == null || inputBehavior == null)
		{
			return 0;
		}
		string inputBehaviorXml = GetInputBehaviorXml(player, inputBehavior.id);
		if (inputBehaviorXml == null || inputBehaviorXml == string.Empty)
		{
			return 0;
		}
		return inputBehavior.ImportXmlString(inputBehaviorXml) ? 1 : 0;
	}

	private void SaveAll()
	{
		IList<Rewired.Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			SavePlayerDataNow(allPlayers[i]);
		}
		SaveAllJoystickCalibrationData();
		GlobalSave.GetInstance().RequestSave();
	}

	private void SavePlayerDataNow(int playerId)
	{
		SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
		GlobalSave.GetInstance().RequestSave();
	}

	private void SavePlayerDataNow(Rewired.Player player)
	{
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(userAssignableMapsOnly: true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	private void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	private void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
			GlobalSave.GetInstance().Set(joystickCalibrationMapPlayerPrefsKey, calibrationMapSaveData.map.ToXmlString());
		}
	}

	private void SaveJoystickData(int joystickId)
	{
		IList<Rewired.Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Rewired.Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				SaveControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		SaveControllerMaps(playerId, controllerType, controllerId);
		SaveControllerDataNow(controllerType, controllerId);
		GlobalSave.GetInstance().RequestSave();
	}

	private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		if (controllerType == ControllerType.Joystick)
		{
			SaveJoystickCalibrationData(controllerId);
		}
		GlobalSave.GetInstance().RequestSave();
	}

	private void SaveControllerMaps(Rewired.Player player, PlayerSaveData playerSaveData)
	{
		foreach (ControllerMapSaveData allControllerMapSaveDatum in playerSaveData.AllControllerMapSaveData)
		{
			SaveControllerMap(player, allControllerMapSaveDatum);
		}
	}

	private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		Rewired.Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, userAssignableMapsOnly: true);
		if (mapSaveData != null)
		{
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				SaveControllerMap(player, mapSaveData[i]);
			}
		}
	}

	private void SaveControllerMap(Rewired.Player player, ControllerMapSaveData saveData)
	{
		string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, saveData.controller, saveData.categoryId, saveData.layoutId);
		GlobalSave.GetInstance().Set(controllerMapPlayerPrefsKey, saveData.map.ToXmlString());
		controllerMapPlayerPrefsKey = GetControllerMapKnownActionIdsPlayerPrefsKey(player, saveData.controller, saveData.categoryId, saveData.layoutId);
		GlobalSave.GetInstance().Set(controllerMapPlayerPrefsKey, GetAllActionIdsString());
		PlayerPrefs.SetString(controllerMapPlayerPrefsKey, GetAllActionIdsString());
	}

	private void SaveInputBehaviors(Rewired.Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = playerSaveData.inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(player, inputBehaviors[i]);
			}
		}
	}

	private void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		Rewired.Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(player, inputBehavior);
				GlobalSave.GetInstance().RequestSave();
			}
		}
	}

	private void SaveInputBehaviorNow(Rewired.Player player, InputBehavior inputBehavior)
	{
		if (player != null && inputBehavior != null)
		{
			string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, inputBehavior.id);
			GlobalSave.GetInstance().Set(inputBehaviorPlayerPrefsKey, inputBehavior.ToXmlString());
		}
	}

	private string GetBasePlayerPrefsKey(Rewired.Player player)
	{
		string text = playerPrefsKeyPrefix;
		string text2 = text;
		return text2 + "|playerName=" + player.name + player.id;
	}

	private string GetControllerMapPlayerPrefsKey(Rewired.Player player, Controller controller, int categoryId, int layoutId)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + controller.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + controller.hardwareIdentifier;
		if (controller.type == ControllerType.Joystick)
		{
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + ((Joystick)controller).hardwareTypeGuid.ToString();
		}
		return basePlayerPrefsKey;
	}

	private string GetControllerMapKnownActionIdsPlayerPrefsKey(Rewired.Player player, Controller controller, int categoryId, int layoutId)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap_KnownActionIds";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + controller.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + controller.hardwareIdentifier;
		if (controller.type == ControllerType.Joystick)
		{
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + ((Joystick)controller).hardwareTypeGuid.ToString();
		}
		return basePlayerPrefsKey;
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
	{
		string text = playerPrefsKeyPrefix;
		text += "|dataType=CalibrationMap";
		text = text + "|controllerType=" + joystick.type;
		text = text + "|hardwareIdentifier=" + joystick.hardwareIdentifier;
		return text + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
	}

	private string GetInputBehaviorPlayerPrefsKey(Rewired.Player player, int inputBehaviorId)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=InputBehavior";
		return basePlayerPrefsKey + "|id=" + inputBehaviorId;
	}

	private string GetControllerMapXml(Rewired.Player player, Controller controller, int categoryId, int layoutId)
	{
		string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, controller, categoryId, layoutId);
		string value = string.Empty;
		GlobalSave.GetInstance().Get(controllerMapPlayerPrefsKey, out value, string.Empty);
		return value;
	}

	private List<int> GetControllerMapKnownActionIds(Rewired.Player player, Controller controller, int categoryId, int layoutId)
	{
		List<int> list = new List<int>();
		string controllerMapKnownActionIdsPlayerPrefsKey = GetControllerMapKnownActionIdsPlayerPrefsKey(player, controller, categoryId, layoutId);
		GlobalSave.GetInstance().Get(controllerMapKnownActionIdsPlayerPrefsKey, out var value, string.Empty);
		if (string.IsNullOrEmpty(value))
		{
			return list;
		}
		string[] array = value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}

	private List<SavedControllerMapData> GetAllControllerMapsXml(Rewired.Player player, bool userAssignableMapsOnly, Controller controller)
	{
		List<SavedControllerMapData> list = new List<SavedControllerMapData>();
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory inputMapCategory = mapCategories[i];
			if (userAssignableMapsOnly && !inputMapCategory.userAssignable)
			{
				continue;
			}
			IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controller.type);
			for (int j = 0; j < list2.Count; j++)
			{
				InputLayout inputLayout = list2[j];
				string controllerMapXml = GetControllerMapXml(player, controller, inputMapCategory.id, inputLayout.id);
				if (!(controllerMapXml == string.Empty))
				{
					List<int> controllerMapKnownActionIds = GetControllerMapKnownActionIds(player, controller, inputMapCategory.id, inputLayout.id);
					list.Add(new SavedControllerMapData(controllerMapXml, controllerMapKnownActionIds));
				}
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
		string value = string.Empty;
		GlobalSave.GetInstance().Get(joystickCalibrationMapPlayerPrefsKey, out value, string.Empty);
		return value;
	}

	private string GetInputBehaviorXml(Rewired.Player player, int id)
	{
		string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, id);
		string value = string.Empty;
		GlobalSave.GetInstance().Get(inputBehaviorPlayerPrefsKey, out value, string.Empty);
		return value;
	}

	private void AddDefaultMappingsForNewActions(Rewired.Player player, List<SavedControllerMapData> savedData, ControllerType controllerType, int controllerId)
	{
		if (player == null || savedData == null)
		{
			return;
		}
		List<int> allActionIds = GetAllActionIds();
		for (int i = 0; i < savedData.Count; i++)
		{
			SavedControllerMapData savedControllerMapData = savedData[i];
			if (savedControllerMapData == null || savedControllerMapData.knownActionIds == null || savedControllerMapData.knownActionIds.Count == 0)
			{
				continue;
			}
			ControllerMap controllerMap = ControllerMap.CreateFromXml(controllerType, savedData[i].xml);
			if (controllerMap == null)
			{
				continue;
			}
			ControllerMap map = player.controllers.maps.GetMap(controllerType, controllerId, controllerMap.categoryId, controllerMap.layoutId);
			if (map == null)
			{
				continue;
			}
			ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(ReInput.controllers.GetController(controllerType, controllerId), controllerMap.categoryId, controllerMap.layoutId);
			if (controllerMapInstance == null)
			{
				continue;
			}
			List<int> list = new List<int>();
			foreach (int item in allActionIds)
			{
				if (!savedControllerMapData.knownActionIds.Contains(item))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				continue;
			}
			foreach (ActionElementMap allMap in controllerMapInstance.AllMaps)
			{
				if (list.Contains(allMap.actionId) && !map.DoesElementAssignmentConflict(allMap))
				{
					ElementAssignment elementAssignment = new ElementAssignment(controllerType, allMap.elementType, allMap.elementIdentifierId, allMap.axisRange, allMap.keyCode, allMap.modifierKeyFlags, allMap.actionId, allMap.axisContribution, allMap.invert);
					map.CreateElementMap(elementAssignment);
				}
			}
		}
	}

	private List<int> GetAllActionIds()
	{
		List<int> list = new List<int>();
		IList<InputAction> actions = ReInput.mapping.Actions;
		for (int i = 0; i < actions.Count; i++)
		{
			list.Add(actions[i].id);
		}
		return list;
	}

	private string GetAllActionIdsString()
	{
		string text = string.Empty;
		List<int> allActionIds = GetAllActionIds();
		for (int i = 0; i < allActionIds.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += allActionIds[i];
		}
		return text;
	}
}

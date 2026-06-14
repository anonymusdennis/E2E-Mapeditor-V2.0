using System;
using System.Collections.Generic;
using Rewired;
using Rewired.Data.Mapping;
using UnityEngine;

[Serializable]
public class TextIconManager : T17MonoBehaviour
{
	public delegate void TextIconManagerDelegate();

	[Serializable]
	public class ControllerEntry
	{
		public string m_Name;

		public HardwareJoystickMap m_Joystick;

		public HardwareJoystickTemplateMap m_TemplateJoystick;

		public List<IconEntry> m_IconEntries;

		public Sprite m_DefaultSprite;

		public Sprite m_ControllerSprite;

		public IconData GetIconData(int elementIdentifierId, AxisRange axisRange, int? desiredSize = null)
		{
			if (m_IconEntries == null)
			{
				return null;
			}
			for (int i = 0; i < m_IconEntries.Count; i++)
			{
				if (m_IconEntries[i] != null && m_IconEntries[i].m_ElementIdentifierId == elementIdentifierId)
				{
					return m_IconEntries[i].GetGlyph(axisRange, desiredSize);
				}
			}
			return null;
		}

		public IconData GetIconDataByActionName(string actionName, AxisRange axisRange, int? desiredSize = null)
		{
			if (m_IconEntries == null)
			{
				return null;
			}
			for (int i = 0; i < m_IconEntries.Count; i++)
			{
				if (m_IconEntries[i] != null && m_IconEntries[i].m_AssociatedActionNames.FindIndex((string name) => name.ToLower() == actionName.ToLower()) != -1)
				{
					return m_IconEntries[i].GetGlyph(axisRange, desiredSize);
				}
			}
			return null;
		}
	}

	[Serializable]
	public class IconEntry
	{
		public int m_ElementIdentifierId;

		public IconData m_DefaultSprite;

		public IconData m_PositiveSprite;

		public IconData m_NegativeSprite;

		public List<IconData> m_DefaultSpriteAltSizes = new List<IconData>(1);

		public List<string> m_AssociatedActionNames = new List<string>();

		public IconData GetGlyph(AxisRange axisRange, int? desiredSize = null)
		{
			return axisRange switch
			{
				AxisRange.Full => GetClosestSpriteToSize(desiredSize), 
				AxisRange.Positive => (!(m_PositiveSprite.Sprite != null)) ? GetClosestSpriteToSize(desiredSize) : m_PositiveSprite, 
				AxisRange.Negative => (!(m_NegativeSprite.Sprite != null)) ? GetClosestSpriteToSize(desiredSize) : m_NegativeSprite, 
				_ => null, 
			};
		}

		private IconData GetClosestSpriteToSize(int? width)
		{
			IconData iconData = m_DefaultSprite;
			if (width.HasValue && iconData != null && m_DefaultSprite.Sprite != null)
			{
				float num = float.PositiveInfinity;
				if ((float)width.Value <= m_DefaultSprite.Sprite.rect.width)
				{
					num = Mathf.Abs((float)width.Value - m_DefaultSprite.Sprite.rect.width);
				}
				if (m_DefaultSpriteAltSizes != null)
				{
					float num2 = iconData.Sprite.rect.width;
					int? num3 = null;
					for (int i = 0; i < m_DefaultSpriteAltSizes.Count; i++)
					{
						if (m_DefaultSpriteAltSizes[i] != null && m_DefaultSpriteAltSizes[i].Sprite != null)
						{
							float width2 = m_DefaultSpriteAltSizes[i].Sprite.rect.width;
							if (width2 >= (float)width.Value && width2 - (float)width.Value < num)
							{
								num = width2 - (float)width.Value;
								iconData = m_DefaultSpriteAltSizes[i];
							}
							if (width2 > num2)
							{
								num2 = width2;
								num3 = i;
							}
						}
					}
					float? num4 = ((!width.HasValue) ? null : new float?(width.Value));
					if (num4.HasValue && num4.GetValueOrDefault() > num2)
					{
						iconData = ((!num3.HasValue) ? m_DefaultSprite : m_DefaultSpriteAltSizes[num3.Value]);
					}
				}
			}
			return iconData;
		}
	}

	[Serializable]
	public class IconData
	{
		public Sprite Sprite;
	}

	private static TextIconManager m_Instance;

	public TextIconManagerDelegate IconsUpdatedEvent;

	[SerializeField]
	[HideInInspector]
	public ControllerEntry[] m_SwitchControllers = new ControllerEntry[4];

	[SerializeField]
	[HideInInspector]
	public List<ControllerEntry> m_SupportedControllers = new List<ControllerEntry>();

	public InputManager m_RewiredInputManager;

	public HardwareJoystickTemplateMap m_DefaultJoystickTemplateMap;

	private Dictionary<Guid, ActionElementMap> m_SubmitActionElementCache = new Dictionary<Guid, ActionElementMap>();

	private const string SUBMIT_ACTION_ID = "UI_Submit";

	private List<JoystickMap> m_AllJoystickMaps = new List<JoystickMap>();

	public static TextIconManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = (TextIconManager)UnityEngine.Object.FindObjectOfType(typeof(TextIconManager));
			}
			return m_Instance;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void ShowGlyphs()
	{
		Rewired.Player player = ReInput.players.GetPlayer(0);
		Controller lastActiveController = player.controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			if (player.controllers.joystickCount > 0)
			{
				lastActiveController = player.controllers.Joysticks[0];
			}
			else
			{
				lastActiveController = player.controllers.Keyboard;
			}
		}
		string text = string.Empty;
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			foreach (JoystickMap map in player.controllers.maps.GetMaps(ControllerType.Joystick, joystick.id))
			{
				foreach (ActionElementMap buttonMap in map.ButtonMaps)
				{
					string text2 = text;
					text = text2 + buttonMap.elementIdentifierName + " is assigned to Button " + buttonMap.elementIndex + " with the Action " + ReInput.mapping.GetAction(buttonMap.actionId).name + "\n";
				}
				foreach (ActionElementMap axisMap in map.AxisMaps)
				{
					string text2 = text;
					text = text2 + axisMap.elementIdentifierName + " is assigned to Axis " + axisMap.elementIndex + " with the Action " + ReInput.mapping.GetAction(axisMap.actionId).name + "\n";
				}
			}
		}
		GUI.TextArea(new Rect(540f, 50f, 400f, 600f), text);
	}

	private void ShowGlyphHelp(Player p, Controller controller, InputAction action)
	{
	}

	public static void CacheControllerMaps()
	{
		if (Instance == null)
		{
			return;
		}
		Joystick joystick = null;
		Rewired.Player player = ReInput.players.GetPlayer(0);
		Controller lastActiveController = player.controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			if (player.controllers.joystickCount > 0)
			{
				lastActiveController = player.controllers.Joysticks[0];
			}
			else if (ReInput.controllers.joystickCount > 0)
			{
				joystick = ReInput.controllers.Joysticks[0];
				player.controllers.AddController(joystick, removeFromOtherPlayers: false);
			}
		}
		if (player.controllers.joystickCount > 0)
		{
			string text = string.Empty;
			player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
			Joystick joystick2 = player.controllers.Joysticks[0];
			foreach (JoystickMap map in player.controllers.maps.GetMaps(ControllerType.Joystick, joystick2.id))
			{
				m_Instance.m_AllJoystickMaps.Add(map);
			}
			foreach (JoystickMap allJoystickMap in m_Instance.m_AllJoystickMaps)
			{
				foreach (ActionElementMap buttonMap in allJoystickMap.ButtonMaps)
				{
					string text2 = text;
					text = text2 + buttonMap.elementIdentifierName + " is assigned to Button " + buttonMap.elementIndex + " with the Action " + ReInput.mapping.GetAction(buttonMap.actionId).name + "\n";
				}
				foreach (ActionElementMap axisMap in allJoystickMap.AxisMaps)
				{
					string text2 = text;
					text = text2 + axisMap.elementIdentifierName + " is assigned to Axis " + axisMap.elementIndex + " with the Action " + ReInput.mapping.GetAction(axisMap.actionId).name + "\n";
				}
			}
		}
		if (joystick != null)
		{
			player.controllers.RemoveController(joystick);
		}
	}

	public static Sprite GetControllerSprite(int playerID)
	{
		if (Instance == null)
		{
			return null;
		}
		return null;
	}

	public static IconData GetIconDataForRewiredPlayer(int playerID, string actionName, int? desiredSize = null)
	{
		if (Instance == null)
		{
			return null;
		}
		if (Instance.m_SupportedControllers == null)
		{
			return null;
		}
		Rewired.Player player = ReInput.players.GetPlayer(playerID);
		if (player == null)
		{
			return Instance.ReturnDefault();
		}
		InputAction action = ReInput.mapping.GetAction(actionName);
		if (action == null)
		{
			return Instance.ReturnDefault();
		}
		ActionElementMap value = null;
		Guid guid = default(Guid);
		Platform instance = Platform.GetInstance();
		if (instance != null)
		{
			bool flag = instance.ReInput_IsRePlayerControllerDisconnected(player);
			Controller disconnectedControllerForRewiredPlayer = instance.GetDisconnectedControllerForRewiredPlayer(player.id);
			if (flag && disconnectedControllerForRewiredPlayer != null && disconnectedControllerForRewiredPlayer.type == ControllerType.Joystick)
			{
				guid = (disconnectedControllerForRewiredPlayer as Joystick).hardwareTypeGuid;
				if (actionName == "UI_Submit" && m_Instance.m_SubmitActionElementCache.TryGetValue(guid, out value))
				{
					return GetIconDataWithGuid(actionName, guid, value, desiredSize);
				}
			}
		}
		Controller controller = player.controllers.GetLastActiveController();
		if (controller == null && player.controllers.joystickCount > 0)
		{
			controller = player.controllers.Joysticks[0];
		}
		bool flag2 = false;
		if (controller != null && controller.type == ControllerType.Joystick)
		{
			guid = (controller as Joystick).hardwareTypeGuid;
			value = player.controllers.maps.GetFirstElementMapWithAction(controller, action.id, skipDisabledMaps: false);
			flag2 = true;
		}
		if (!flag2)
		{
			for (int i = 0; i < m_Instance.m_AllJoystickMaps.Count; i++)
			{
				value = m_Instance.m_AllJoystickMaps[i].GetFirstElementMapWithAction(action.id);
				if (value != null)
				{
					break;
				}
			}
			guid = Instance.m_SupportedControllers[2].m_Joystick.Guid;
		}
		if (controller != null && controller.type == ControllerType.Joystick && value != null && !m_Instance.m_SubmitActionElementCache.ContainsKey(guid))
		{
			ActionElementMap firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction(controller, "UI_Submit", skipDisabledMaps: false);
			if (firstElementMapWithAction != null)
			{
				m_Instance.m_SubmitActionElementCache.Add(guid, firstElementMapWithAction);
			}
		}
		if (value == null)
		{
			return Instance.ReturnDefault();
		}
		return GetIconDataWithGuid(actionName, guid, value, desiredSize);
	}

	private static IconData GetIconDataWithGuid(string actionName, Guid guid, ActionElementMap aem, int? desiredSize)
	{
		if (Instance.m_SupportedControllers.FindIndex((ControllerEntry ctrl) => ctrl.m_Joystick != null && ctrl.m_Joystick.Guid == guid) == -1)
		{
			return Instance.GetDefaultIconData(actionName, aem.axisRange, desiredSize);
		}
		return GetIconData(guid, aem.elementIdentifierId, aem.axisRange, desiredSize);
	}

	private IconData ReturnDefault()
	{
		IconData iconData = new IconData();
		iconData.Sprite = m_Instance.m_SupportedControllers[2].m_DefaultSprite;
		return iconData;
	}

	private IconData GetDefaultIconData(string actionName, AxisRange axisRange, int? desiredSize = null)
	{
		return GetIconDataByActionName(m_DefaultJoystickTemplateMap.Guid, actionName, axisRange, desiredSize);
	}

	public static IconData GetIconData(Guid joystickGuid, int elementIdentifierId, AxisRange axisRange, int? desiredSize = null)
	{
		if (Instance == null)
		{
			return null;
		}
		if (Instance.m_SupportedControllers == null)
		{
			return null;
		}
		for (int i = 0; i < Instance.m_SupportedControllers.Count; i++)
		{
			if (Instance.m_SupportedControllers[i] != null && !(Instance.m_SupportedControllers[i].m_Joystick == null) && !(Instance.m_SupportedControllers[i].m_Joystick.Guid != joystickGuid))
			{
				return Instance.m_SupportedControllers[i].GetIconData(elementIdentifierId, axisRange, desiredSize);
			}
		}
		return null;
	}

	public static IconData GetIconDataByActionName(Guid joystickGuid, string actionName, AxisRange axisRange, int? desiredSize = null)
	{
		if (Instance == null)
		{
			return null;
		}
		if (Instance.m_SupportedControllers == null)
		{
			return null;
		}
		for (int i = 0; i < Instance.m_SupportedControllers.Count; i++)
		{
			if (Instance.m_SupportedControllers[i] != null && (!(Instance.m_SupportedControllers[i].m_Joystick == null) || !(Instance.m_SupportedControllers[i].m_TemplateJoystick == null)) && (!(Instance.m_SupportedControllers[i].m_Joystick != null) || !(Instance.m_SupportedControllers[i].m_Joystick.Guid != joystickGuid)) && (!(Instance.m_SupportedControllers[i].m_TemplateJoystick != null) || !(Instance.m_SupportedControllers[i].m_TemplateJoystick.Guid != joystickGuid)))
			{
				return Instance.m_SupportedControllers[i].GetIconDataByActionName(actionName, axisRange, desiredSize);
			}
		}
		return null;
	}
}

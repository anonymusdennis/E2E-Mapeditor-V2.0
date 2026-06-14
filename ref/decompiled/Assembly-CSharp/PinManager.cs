using System;
using System.Collections.Generic;
using UnityEngine;

public class PinManager : T17MonoBehaviour
{
	public class Pin
	{
		public enum PinFilterType
		{
			All,
			Characters,
			Shops,
			Favours,
			Objectives,
			Tags,
			Count
		}

		public class PlayerIcons
		{
			public T17Image m_MainMapIcon;

			public T17Image m_MiniMapIcon;

			public IconPool m_MainMapIconPool;

			public IconPool m_MiniMapIconPool;
		}

		public GameObject m_Target;

		public Character m_TargetCharacter;

		public FloorManager.Floor m_Floor;

		public Sprite m_IconSprite;

		public Vector2 m_MapPos = default(Vector2);

		public bool m_UpdatePosition;

		public bool m_Edgable;

		public bool m_FloorTrackable;

		public bool m_Directional;

		public bool m_Animated;

		public SpriteAnimation m_SpriteAnimation;

		public int m_PinID;

		public bool m_bOverrideIconScale;

		public Vector3 m_OverrideIconScale = default(Vector3);

		public Vector2 m_IconMapWorldPositionOffset = Vector2.zero;

		public PinFilterType m_FilterType;

		public Dictionary<int, PlayerIcons> m_PlayerIcons = new Dictionary<int, PlayerIcons>();

		public string m_ToolTipTag = string.Empty;

		public bool m_LocaliseToolTipTag = true;

		public bool m_bForAll;

		public bool m_bIsPlayer;

		public int m_PlayerIDToIgnore = -1;
	}

	[Serializable]
	public class IconPriority
	{
		public Pin.PinFilterType m_Type;

		public int m_Priority;
	}

	public List<Pin> m_MainMapPins = new List<Pin>();

	public List<Pin> m_MiniMapPins = new List<Pin>();

	public List<Pin> m_UpdateablePins = new List<Pin>();

	public Dictionary<int, Pin> m_PinDictionary = new Dictionary<int, Pin>();

	private int m_NextPinID = 1;

	private int m_MapUpdate_Index;

	public int m_MapUpdate_NetViewID = -1;

	private FloorManager m_FloorMan;

	private int m_highestPinPriority = -1;

	public IconPriority[] m_IconPriorities;

	private int m_TileSystemRows;

	private int m_TileSystemCols;

	private int m_TileSystemRows_Half;

	private int m_TileSystemCols_Half;

	private float m_TileSystemRows_Inv;

	private float m_TileSystemCols_Inv;

	private static PinManager m_Instance;

	public static PinManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	private void Start()
	{
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_FloorMan = FloorManager.GetInstance();
		m_FloorMan.GetTileSystemBounds(m_FloorMan.FindFloorbyIndex(0), FloorManager.TileSystem_Type.TileSystem_Ground, out m_TileSystemRows, out m_TileSystemCols);
		m_TileSystemRows_Half = m_TileSystemRows / 2;
		m_TileSystemCols_Half = m_TileSystemCols / 2;
		m_TileSystemRows_Inv = 1f / (float)m_TileSystemRows;
		m_TileSystemCols_Inv = 1f / (float)m_TileSystemCols;
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		m_MainMapPins.Clear();
		m_MiniMapPins.Clear();
		m_UpdateablePins.Clear();
		m_PinDictionary.Clear();
		m_FloorMan = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public int CreatePin(bool bForMainMap, bool bForMiniMap, GameObject target, Sprite icon, bool bUpdatePosition = false, FloorManager.Floor floor = null, Player[] players = null, Pin.PinFilterType filterType = Pin.PinFilterType.All, bool edgable = false, bool floorTrackable = false, bool directional = false, string toolTipTag = "", bool localiseToolTipTag = true, bool bOverrideIconScale = false, Vector3 overrideIconScale = default(Vector3), SpriteAnimation animation = null, Vector3 staticTargetPosition = default(Vector3), bool isPlayer = false, int playerIDToIgnore = -1, float mapOffsetX = 0f, float mapOffsetY = 0f)
	{
		Pin pin = new Pin();
		pin.m_Target = target;
		pin.m_IconSprite = icon;
		pin.m_UpdatePosition = bUpdatePosition;
		pin.m_bIsPlayer = isPlayer;
		pin.m_PlayerIDToIgnore = playerIDToIgnore;
		pin.m_IconMapWorldPositionOffset = new Vector2(mapOffsetX, mapOffsetY);
		pin.m_TargetCharacter = target.GetComponent<Character>();
		if (animation != null)
		{
			pin.m_Animated = true;
			pin.m_SpriteAnimation = new SpriteAnimation(animation);
			if (pin.m_SpriteAnimation.currentSprite != null)
			{
				pin.m_IconSprite = pin.m_SpriteAnimation.currentSprite;
			}
		}
		if (floor == null && target != null)
		{
			floor = m_FloorMan.FindFloorAtZ(target.transform.position.z);
		}
		else if (floor == null)
		{
			floor = m_FloorMan.FindFloorAtZ(staticTargetPosition.z);
		}
		pin.m_Floor = floor;
		if (target != null)
		{
			CalculateMapPos(ref pin.m_MapPos, target.transform.position, pin.m_IconMapWorldPositionOffset);
		}
		else
		{
			CalculateMapPos(ref pin.m_MapPos, staticTargetPosition, pin.m_IconMapWorldPositionOffset);
		}
		pin.m_FloorTrackable = floorTrackable;
		pin.m_Edgable = edgable;
		pin.m_FilterType = filterType;
		pin.m_Directional = directional;
		pin.m_ToolTipTag = toolTipTag;
		pin.m_LocaliseToolTipTag = localiseToolTipTag;
		pin.m_OverrideIconScale = overrideIconScale;
		pin.m_bOverrideIconScale = bOverrideIconScale;
		if (players == null || players.Length == 0)
		{
			pin.m_bForAll = true;
			players = Player.GetAllPlayers().ToArray();
		}
		if (players.Length == 0)
		{
			return 0;
		}
		if (bForMainMap)
		{
			m_MainMapPins.Add(pin);
			if (m_MainMapPins[m_MainMapPins.Count - 1].m_PlayerIcons.Count == 0)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if (null != players[i] && !m_MainMapPins[m_MainMapPins.Count - 1].m_PlayerIcons.ContainsKey(players[i].m_NetView.viewID) && players[i].m_NetView.viewID != m_MainMapPins[m_MainMapPins.Count - 1].m_PlayerIDToIgnore)
					{
						m_MainMapPins[m_MainMapPins.Count - 1].m_PlayerIcons.Add(players[i].m_NetView.viewID, new Pin.PlayerIcons());
					}
				}
			}
		}
		if (bForMiniMap)
		{
			m_MiniMapPins.Add(pin);
			if (m_MiniMapPins[m_MiniMapPins.Count - 1].m_PlayerIcons.Count == 0)
			{
				for (int j = 0; j < players.Length; j++)
				{
					if (null != players[j] && !m_MiniMapPins[m_MiniMapPins.Count - 1].m_PlayerIcons.ContainsKey(players[j].m_NetView.viewID) && players[j].m_NetView.viewID != m_MiniMapPins[m_MiniMapPins.Count - 1].m_PlayerIDToIgnore)
					{
						m_MiniMapPins[m_MiniMapPins.Count - 1].m_PlayerIcons.Add(players[j].m_NetView.viewID, new Pin.PlayerIcons());
					}
				}
			}
		}
		if (bUpdatePosition)
		{
			m_UpdateablePins.Add(pin);
			if (m_UpdateablePins[m_UpdateablePins.Count - 1].m_PlayerIcons.Count == 0)
			{
				for (int k = 0; k < players.Length; k++)
				{
					if (null != players[k] && !m_UpdateablePins[m_UpdateablePins.Count - 1].m_PlayerIcons.ContainsKey(players[k].m_NetView.viewID) && players[k].m_NetView.viewID != m_UpdateablePins[m_UpdateablePins.Count - 1].m_PlayerIDToIgnore)
					{
						m_UpdateablePins[m_UpdateablePins.Count - 1].m_PlayerIcons.Add(players[k].m_NetView.viewID, new Pin.PlayerIcons());
					}
				}
			}
		}
		pin.m_PinID = m_NextPinID;
		m_NextPinID++;
		m_PinDictionary.Add(pin.m_PinID, pin);
		return pin.m_PinID;
	}

	public void CalculateMapPos(ref Vector2 retPos, Vector3 worldPos, Vector2 worldPosOffset)
	{
		retPos.x = (worldPos.x + worldPosOffset.x + (float)m_TileSystemCols_Half) * m_TileSystemCols_Inv;
		retPos.y = (worldPos.y + worldPosOffset.y + (float)m_TileSystemRows_Half) * m_TileSystemRows_Inv;
	}

	private void Update()
	{
		if (!(m_FloorMan != null))
		{
			return;
		}
		int count = m_UpdateablePins.Count;
		for (int i = 0; i < count; i++)
		{
			Pin pin = m_UpdateablePins[i];
			if (pin.m_Target != null)
			{
				Character targetCharacter = pin.m_TargetCharacter;
				if (targetCharacter != null && targetCharacter.CurrentFloor != pin.m_Floor)
				{
					pin.m_Floor = targetCharacter.CurrentFloor;
				}
				CalculateMapPos(ref pin.m_MapPos, pin.m_Target.transform.position, pin.m_IconMapWorldPositionOffset);
			}
			if (pin.m_Animated)
			{
				pin.m_SpriteAnimation.Update(UpdateManager.deltaTime);
				pin.m_IconSprite = pin.m_SpriteAnimation.currentSprite;
			}
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		int num = allGamers.Length;
		m_MapUpdate_NetViewID = ((m_MapUpdate_Index >= num || allGamers[m_MapUpdate_Index] == null) ? (-1) : allGamers[m_MapUpdate_Index].m_NetViewID);
		int num2 = 0;
		do
		{
			if (++m_MapUpdate_Index >= num)
			{
				m_MapUpdate_Index = 0;
			}
			num2++;
		}
		while (num2 < num && (allGamers[m_MapUpdate_Index] == null || !allGamers[m_MapUpdate_Index].IsLocal()));
	}

	public GameObject GetCurrentPinTarget(int pinID)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		return value?.m_Target;
	}

	public void UpdatePinTarget(int pinID, GameObject target, Sprite icon = null)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		if (value != null)
		{
			value.m_Target = target;
			if (icon != null)
			{
				value.m_IconSprite = icon;
			}
		}
	}

	public void UpdatePinTooltipTag(int pinID, string tag, bool localise)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		if (value != null)
		{
			value.m_ToolTipTag = tag;
			value.m_LocaliseToolTipTag = localise;
		}
	}

	public void UpdatePinIconSprite(int pinID, Sprite icon, float pinMapOffsetX = 0f, float pinMapOffsetY = 0f)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		if (value != null)
		{
			value.m_IconSprite = icon;
			value.m_IconMapWorldPositionOffset = new Vector2(pinMapOffsetX, pinMapOffsetY);
		}
	}

	public void UpdatePinIconSprite(int pinID, Sprite icon, Pin.PinFilterType filter, SpriteAnimation animation = null, bool showEdge = false, bool floorTrackable = false)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		if (value == null)
		{
			return;
		}
		value.m_FilterType = filter;
		value.m_Edgable = showEdge;
		value.m_FloorTrackable = floorTrackable;
		if (animation != null)
		{
			value.m_Animated = true;
			value.m_SpriteAnimation = new SpriteAnimation(animation);
			if (value.m_SpriteAnimation.currentSprite != null)
			{
				value.m_IconSprite = value.m_SpriteAnimation.currentSprite;
			}
		}
		else if (icon != null)
		{
			value.m_Animated = false;
			value.m_IconSprite = icon;
		}
	}

	public void RemovePin(int pinID)
	{
		Pin value = null;
		m_PinDictionary.TryGetValue(pinID, out value);
		if (value == null)
		{
			return;
		}
		if (value.m_UpdatePosition)
		{
			m_UpdateablePins.Remove(value);
		}
		m_MainMapPins.Remove(value);
		m_MiniMapPins.Remove(value);
		m_PinDictionary.Remove(pinID);
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Pin.PlayerIcons value2 = null;
			value.m_PlayerIcons.TryGetValue(allPlayers[i].m_NetView.viewID, out value2);
			if (value2 != null)
			{
				if (value2.m_MainMapIconPool != null)
				{
					value2.m_MainMapIconPool.FreeObject(value2.m_MainMapIcon.gameObject);
					value2.m_MainMapIcon = null;
					value2.m_MainMapIconPool = null;
				}
				if (value2.m_MiniMapIconPool != null)
				{
					value2.m_MiniMapIconPool.FreeObject(value2.m_MiniMapIcon.gameObject);
					value2.m_MiniMapIcon = null;
					value2.m_MiniMapIconPool = null;
				}
			}
		}
	}

	public void ReassignPins()
	{
		for (int i = 0; i < m_MainMapPins.Count; i++)
		{
			if (!m_MainMapPins[i].m_bForAll)
			{
				continue;
			}
			Pin.PlayerIcons value = null;
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int j = 0; j < allPlayers.Count; j++)
			{
				if (allPlayers[j].m_NetView.viewID != m_MainMapPins[i].m_PlayerIDToIgnore)
				{
					m_MainMapPins[i].m_PlayerIcons.TryGetValue(allPlayers[j].m_NetView.viewID, out value);
					if (value == null)
					{
						m_MainMapPins[i].m_PlayerIcons.Add(allPlayers[j].m_NetView.viewID, new Pin.PlayerIcons());
					}
				}
			}
		}
		for (int k = 0; k < m_MiniMapPins.Count; k++)
		{
			if (!m_MiniMapPins[k].m_bForAll)
			{
				continue;
			}
			Pin.PlayerIcons value2 = null;
			List<Player> allPlayers2 = Player.GetAllPlayers();
			for (int l = 0; l < allPlayers2.Count; l++)
			{
				if (allPlayers2[l].m_NetView.viewID != m_MiniMapPins[k].m_PlayerIDToIgnore)
				{
					m_MiniMapPins[k].m_PlayerIcons.TryGetValue(allPlayers2[l].m_NetView.viewID, out value2);
					if (value2 == null)
					{
						m_MiniMapPins[k].m_PlayerIcons.Add(allPlayers2[l].m_NetView.viewID, new Pin.PlayerIcons());
					}
				}
			}
		}
	}

	public float GetIconPriorityMultiplier(Pin.PinFilterType type)
	{
		for (int i = 0; i < m_IconPriorities.Length; i++)
		{
			if (m_IconPriorities[i].m_Type == type)
			{
				return (float)m_IconPriorities[i].m_Priority / (float)GetHighestPriority();
			}
		}
		return 1f;
	}

	private int GetHighestPriority()
	{
		if (m_highestPinPriority <= 0)
		{
			m_highestPinPriority = 1;
			for (int i = 0; i < m_IconPriorities.Length; i++)
			{
				if (m_IconPriorities[i].m_Priority > m_highestPinPriority)
				{
					m_highestPinPriority = m_IconPriorities[i].m_Priority;
				}
			}
		}
		return m_highestPinPriority;
	}
}

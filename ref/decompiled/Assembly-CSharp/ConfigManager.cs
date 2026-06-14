public class ConfigManager : T17MonoBehaviour
{
	private PrisonConfig m_ActiveConfig;

	private static ConfigManager m_Instance;

	public GlobalCombatConfig combatConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_CombatConfig;
			}
			return null;
		}
	}

	public AIConfig aiConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_AIConfig;
			}
			return null;
		}
	}

	public JobConfig jobConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_JobConfig;
			}
			return null;
		}
	}

	public CharacterConfig playerConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_PlayerConfig;
			}
			return null;
		}
	}

	public CharacterConfig inmateConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_InmateConfig;
			}
			return null;
		}
	}

	public CharacterConfig dogConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_DogConfig;
			}
			return null;
		}
	}

	public RoutineConfig routineConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_RoutineConfig;
			}
			return null;
		}
	}

	public OpinionConfig opinionConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_OpinionConfig;
			}
			return null;
		}
	}

	public VendorConfig vendorConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_VendorConfig;
			}
			return null;
		}
	}

	public QuestConfig questConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_QuestConfig;
			}
			return null;
		}
	}

	public MinigameConfig minigameConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_MinigameConfig;
			}
			return null;
		}
	}

	public GeneralMinigameConfig GeneralMinigameConfigs
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_GeneralPrisonMinigameConfig;
			}
			return null;
		}
	}

	public ScoreSystemConfig ScoreSystemConfig
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_ScoreConfig;
			}
			return null;
		}
	}

	public PrisonConfig.ConfigType gameType
	{
		get
		{
			if (m_ActiveConfig != null)
			{
				return m_ActiveConfig.m_ConfigType;
			}
			return PrisonConfig.ConfigType.Versus;
		}
	}

	public static ConfigManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void GetVersusDuration(out int days, out int hours, out int minutes)
	{
		if (m_ActiveConfig != null)
		{
			days = m_ActiveConfig.m_VersusDays;
			hours = m_ActiveConfig.m_VersusHours;
			minutes = m_ActiveConfig.m_VersusMinutes;
		}
		else
		{
			days = 1;
			hours = 0;
			minutes = 0;
		}
	}

	public bool SetActiveConfig(int configID)
	{
		if (LevelScript.GetInstance() == null)
		{
			return false;
		}
		PrisonData levelSetup = LevelScript.GetInstance().m_LevelSetup;
		if (levelSetup == null)
		{
			return false;
		}
		if (configID < 0 || configID >= levelSetup.m_Configs.Count)
		{
			return false;
		}
		PrisonConfig prisonConfig = levelSetup.m_Configs[configID];
		if (prisonConfig == null)
		{
			return false;
		}
		m_ActiveConfig = prisonConfig;
		return true;
	}

	public PrisonConfig GetActiveConfig()
	{
		return m_ActiveConfig;
	}

	public bool HasActiveConfig()
	{
		return null != m_ActiveConfig;
	}

	public CharacterConfig GetGuardConfig(PrisonAlertness starRating)
	{
		if (m_ActiveConfig == null)
		{
			return null;
		}
		if ((int)starRating >= 10)
		{
			return m_ActiveConfig.m_RiotGuardConfig;
		}
		return m_ActiveConfig.m_GuardConfig;
	}

	public bool HasItemOverrideConfig(int itemDataID)
	{
		return GetItemOverrideConfig(itemDataID) != null;
	}

	public bool HasAIEventOverride(AIEvent.EventType eventType)
	{
		return GetAIEventOverride(eventType) != null;
	}

	public ItemDataConfig GetItemOverrideConfig(int itemDataID)
	{
		if (m_ActiveConfig == null)
		{
			return null;
		}
		for (int i = 0; i < m_ActiveConfig.m_ItemDataOverrides.Count; i++)
		{
			ItemDataConfig itemDataConfig = m_ActiveConfig.m_ItemDataOverrides[i];
			if (itemDataConfig != null && itemDataConfig.m_ItemDataID == itemDataID)
			{
				return itemDataConfig;
			}
		}
		return null;
	}

	public AIEventData GetAIEventOverride(AIEvent.EventType eventType)
	{
		if (m_ActiveConfig == null)
		{
			return null;
		}
		for (int i = 0; i < m_ActiveConfig.m_AIEventOverrides.Count; i++)
		{
			AIEventData aIEventData = m_ActiveConfig.m_AIEventOverrides[i];
			if (aIEventData != null && aIEventData.m_eEventType == eventType)
			{
				return aIEventData;
			}
		}
		return null;
	}

	public AIEventData ApplyAIEventOverride(AIEventData originalEvent)
	{
		if (originalEvent != null)
		{
			AIEventData aIEventOverride = GetAIEventOverride(originalEvent.m_eEventType);
			return aIEventOverride ?? originalEvent;
		}
		return originalEvent;
	}

	public ItemContainerConfig GetItemContainerOverride(ItemContainer container)
	{
		if (container.m_ContainerType == ItemContainer.ItemContainerType.Desk || container.m_ContainerType == ItemContainer.ItemContainerType.DeskInmate || container.m_ContainerType == ItemContainer.ItemContainerType.DeskGuard)
		{
			DeskInteraction deskInteraction = container.GetDeskInteraction();
			if (deskInteraction != null)
			{
				Character owner = deskInteraction.GetOwner();
				if (owner != null)
				{
					ItemContainerConfig result = null;
					if (owner.m_CharacterStats.m_bIsPlayer)
					{
						result = m_ActiveConfig.m_PlayerDeskConfig;
					}
					else
					{
						switch (owner.m_CharacterRole)
						{
						case CharacterRole.Inmate:
							result = m_ActiveConfig.m_InmateDeskConfig;
							break;
						case CharacterRole.Guard:
							result = m_ActiveConfig.m_GuardDeskConfig;
							break;
						}
					}
					return result;
				}
			}
		}
		return null;
	}
}

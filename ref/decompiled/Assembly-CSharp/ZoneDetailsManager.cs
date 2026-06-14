using System;
using UnityEngine;

public class ZoneDetailsManager : MonoBehaviour
{
	public enum ZoneTypes
	{
		INVALID,
		InmateCell,
		MealHall,
		Gym,
		RollCall,
		Shower,
		Library,
		Solitary,
		Infirmary,
		JobOffice,
		ControlRoom,
		ContrabandRoom,
		Kitchen,
		Kennels,
		WardensOffice,
		GuardQuarters,
		Maintenance,
		GuardRoom,
		SocialArea,
		JobRoom,
		Generators,
		Job_Woodwork,
		Job_Blacksmith,
		TOTAL
	}

	public enum LimitationType
	{
		FixedSize,
		FromBlockGroup
	}

	public enum MustBeReachableBy
	{
		Player,
		Anyone
	}

	[Serializable]
	public class LayerLight
	{
		[Flags]
		public enum LightRule
		{
			AboveBlockGroup = 1,
			DistanceFromStuff = 2
		}

		public bool m_bValid;

		public int m_Size;

		public LightRule m_Type = LightRule.DistanceFromStuff;

		public GameObject m_LightObject;

		public string m_BlockGroup = string.Empty;

		private int m_BlockGroupIndex = -1;

		public int m_NearestAllowedToEdge = -1;

		public int m_NearestAllowLight = -1;

		public int m_NearestAllowedWall = -1;

		public int m_NearestAllowOutside = -1;

		[NonSerialized]
		public int m_Limit;

		public void Copy(LayerLight copyFrom)
		{
			if (copyFrom != null)
			{
				m_bValid = copyFrom.m_bValid;
				m_Size = copyFrom.m_Size;
				m_Type = copyFrom.m_Type;
				m_LightObject = copyFrom.m_LightObject;
				m_BlockGroup = copyFrom.m_BlockGroup;
				m_BlockGroupIndex = copyFrom.m_BlockGroupIndex;
				m_NearestAllowedToEdge = copyFrom.m_NearestAllowedToEdge;
				m_NearestAllowLight = copyFrom.m_NearestAllowLight;
				m_NearestAllowedWall = copyFrom.m_NearestAllowedWall;
				m_NearestAllowOutside = copyFrom.m_NearestAllowOutside;
			}
		}

		public static LayerLight CloneLight(LayerLight copyFrom)
		{
			LayerLight layerLight = new LayerLight();
			layerLight.Copy(copyFrom);
			return layerLight;
		}

		public int GetBlockGroupIndex()
		{
			if (m_BlockGroupIndex == -1)
			{
				BuildingBlockGroupManager instance = BuildingBlockGroupManager.GetInstance();
				if (instance != null)
				{
					m_BlockGroupIndex = instance.GetGroupIndexByName(m_BlockGroup);
				}
			}
			return m_BlockGroupIndex;
		}
	}

	[Serializable]
	public class ZoneDetails
	{
		public bool m_bValid;

		public ZoneTypes m_ZoneType;

		public string m_OurName = "NEW";

		public string m_NameResource = string.Empty;

		private string m_NameResourceTranslation = string.Empty;

		public string m_ErrorResource = string.Empty;

		private string m_ErrorResourceTranslation = string.Empty;

		public int m_ErrorID = -1;

		public string m_CantReachErrorResource = string.Empty;

		private string m_CantReachErrorResourceTranslation = string.Empty;

		public string m_ZoneOverNothingResource = string.Empty;

		private string m_ZoneOverNothingResourceTranslation = string.Empty;

		public int m_CantReachErrorID = -1;

		public string m_ToolTipResource = string.Empty;

		private string m_ToolTipResourceTranslation = string.Empty;

		public long m_Family;

		public Material m_ZoneImage;

		public Material m_ZoneImageInvalid;

		public int m_LimitationGroup = -1;

		public LimitationType m_LimitationType;

		public int m_FixedSize = 1;

		public string m_LimitationBlockGroup = string.Empty;

		private int m_LimitationBlockGroupIndex = -1;

		public int m_LimitationCount;

		public bool m_InmateSafeSpace;

		public RoomBlob.RoomAffinity m_InmateRoomAffinity = RoomBlob.RoomAffinity.Meh;

		public bool m_GuardSafeSpace;

		public RoomBlob.RoomAffinity m_GuardRoomAffinity = RoomBlob.RoomAffinity.Meh;

		public bool m_SupportSafeSpace;

		public RoomBlob.RoomAffinity m_SupportRoomAffinity = RoomBlob.RoomAffinity.Meh;

		public BuildingBlock_Room.LabelTypes m_LabelType;

		public RoomBlob.RoomSubIdentity_Rules m_SubRules;

		public bool m_bAllowSniping;

		public RoomBlob.eLocation m_BlobLocation;

		public JobType m_JobType;

		public MustBeReachableBy m_MustBeReachableBy = MustBeReachableBy.Anyone;

		public LayerLight[] m_Lighting = new LayerLight[0];

		public ZoneRequirement[] m_Requirements = new ZoneRequirement[0];

		public int[] m_ImportantLimitationGroups = new int[0];

		public int GetLimitationBlockGroupIndex()
		{
			if (m_LimitationBlockGroupIndex == -1)
			{
				BuildingBlockGroupManager instance = BuildingBlockGroupManager.GetInstance();
				if (instance != null)
				{
					m_LimitationBlockGroupIndex = instance.GetGroupIndexByName(m_LimitationBlockGroup);
				}
			}
			return m_LimitationBlockGroupIndex;
		}

		public string GetCantReachErrorText()
		{
			return GetTranslation(m_CantReachErrorResource, ref m_CantReachErrorResourceTranslation);
		}

		public string GetStandardErrorText()
		{
			return GetTranslation(m_ErrorResource, ref m_ErrorResourceTranslation);
		}

		public string GetZoneNameText()
		{
			return GetTranslation(m_NameResource, ref m_NameResourceTranslation);
		}

		public string GetOverNothingText()
		{
			return GetTranslation(m_ZoneOverNothingResource, ref m_ZoneOverNothingResourceTranslation);
		}

		public string GetToolTipText()
		{
			return GetTranslation(m_ToolTipResource, ref m_ToolTipResourceTranslation);
		}

		private string GetTranslation(string strSource, ref string strTranslated)
		{
			if (string.IsNullOrEmpty(strTranslated))
			{
				if (!string.IsNullOrEmpty(strSource))
				{
					string localized = string.Empty;
					if (!Localization.Get(strSource, out localized))
					{
						strTranslated = "[" + strSource + "][" + m_ZoneType.ToString() + "]";
					}
					else
					{
						strTranslated = localized;
					}
				}
				else
				{
					strTranslated = "[NOT SET YET][" + m_ZoneType.ToString() + "]";
				}
			}
			return strTranslated;
		}

		public bool DoesRequireLimitationGroup(int iLimitationGroup)
		{
			if (m_bValid)
			{
				for (int num = m_ImportantLimitationGroups.Length - 1; num >= 0; num--)
				{
					if (m_ImportantLimitationGroups[num] == iLimitationGroup)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	private static ZoneDetailsManager m_Instance;

	public ZoneDetails[] m_Zones = new ZoneDetails[0];

	public static ZoneDetailsManager GetInstance()
	{
		return m_Instance;
	}

	protected virtual void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public ZoneDetails GetZoneDetails(ZoneTypes zoneType)
	{
		for (int num = m_Zones.Length - 1; num >= 0; num--)
		{
			if (m_Zones[num] != null && m_Zones[num].m_ZoneType == zoneType)
			{
				return m_Zones[num];
			}
		}
		return null;
	}
}

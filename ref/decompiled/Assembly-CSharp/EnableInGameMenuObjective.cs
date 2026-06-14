using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class EnableInGameMenuObjective : BaseObjective
{
	public int m_MenusToEnable;

	public int m_MenusToDisable;

	public bool m_bHasSetMenus;

	private const int BYTE_LENGTH = 8;

	protected override void Child_PickAllTargets()
	{
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		InGameMenuFlow.Instance.GetCorrectIGMData(m_PlayerOwner.m_PlayerCameraManagerBindingID, out var data);
		if (data != null && data.m_PlayerRootMenu != null)
		{
			int num = 32;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 = 1 << i;
				if (num2 >= 16385)
				{
					break;
				}
				bool flag = (num2 & m_MenusToEnable) > 0;
				bool flag2 = (num2 & m_MenusToDisable) > 0;
				if (flag || flag2)
				{
					BaseMenuBehaviour.InGameMenuTypes menuType = (BaseMenuBehaviour.InGameMenuTypes)num2;
					if (flag)
					{
						data.m_PlayerRootMenu.SetMenuEnabled(menuType, enabled: true);
					}
					else if (flag2)
					{
						data.m_PlayerRootMenu.SetMenuEnabled(menuType, enabled: false);
					}
				}
			}
		}
		m_bHasSetMenus = true;
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bHasSetMenus;
	}

	protected override bool Child_EvaluateStatus()
	{
		return Child_EvaluateDependencies();
	}

	public override int SetHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		return 0;
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("HasSetMenus", m_bHasSetMenus));
		}
		baseObj.Add(new JProperty("ToEnable", m_MenusToEnable));
		baseObj.Add(new JProperty("ToDisable", m_MenusToDisable));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("HasSetMenus");
			if (jProperty != null)
			{
				m_bHasSetMenus = (bool)jProperty.Value;
			}
		}
		JProperty jProperty2 = json.Property("ToEnable");
		if (jProperty2 != null)
		{
			m_MenusToEnable = (int)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("ToDisable");
		if (jProperty3 != null)
		{
			m_MenusToDisable = (int)jProperty3.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.EnableInGameMenuObjective;
	}
}

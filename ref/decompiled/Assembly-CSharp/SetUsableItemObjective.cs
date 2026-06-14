using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class SetUsableItemObjective : BaseObjective
{
	public ItemData m_UsableItem;

	public bool m_bHasSetInputs;

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
		if (m_PlayerOwner != null)
		{
			m_PlayerOwner.SetAllowedItem(m_UsableItem);
		}
		m_bHasSetInputs = true;
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bHasSetInputs;
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
			baseObj.Add(new JProperty("HasSetInputs", m_bHasSetInputs));
		}
		int num = ((!(m_UsableItem == null)) ? m_UsableItem.m_ItemDataID : (-1));
		baseObj.Add(new JProperty("UsableItem", num));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("HasSetInputs");
			if (jProperty != null)
			{
				m_bHasSetInputs = (bool)jProperty.Value;
			}
		}
		JProperty jProperty2 = json.Property("UsableItem");
		if (jProperty2 == null)
		{
			return;
		}
		int usableItemDataId = (int)jProperty2.Value;
		if (usableItemDataId == -1)
		{
			m_UsableItem = null;
			return;
		}
		m_UsableItem = Resources.LoadAll<ItemData>("Prefabs/Items").ToList().FirstOrDefault((ItemData id) => id.m_ItemDataID == usableItemDataId);
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SetUsableItemObjective;
	}
}

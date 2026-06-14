using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class SpawnVendorObjective : BaseObjective
{
	public bool m_bRandomInmate = true;

	public ObjectiveSceneElement m_SceneTarget;

	public bool m_bIsObjective;

	public bool m_bSpawnsQuestItems;

	public List<ItemData> m_ItemsToSell = new List<ItemData>();

	private Character m_TargetInmate;

	private const string VENDORTARGET = "$VendorTarget";

	private int m_QuestItemGroupID = -1;

	protected override void Child_PickAllTargets()
	{
		if (m_bRandomInmate || m_SceneTarget == null)
		{
			m_TargetInmate = null;
		}
		else if (m_SceneTarget != null && m_SceneTarget.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.Character)
		{
			m_TargetInmate = m_SceneTarget.GetComponentInParent<Character>();
		}
		InternalTokenUpdate("$VendorTarget", m_TargetInmate.m_CharacterCustomisation.m_DisplayName, string.Empty);
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$VendorTarget", Localization.TokenReplaceType.Character);
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_PreAction()
	{
		if ((bool)VendorManager.GetInstance())
		{
			VendorManager.GetInstance().AssignQuestVendor(m_TargetInmate, m_ItemsToSell);
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_TargetInmate.m_bIsVendor)
		{
			bool success;
			Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(m_TargetInmate, out success);
			if (success)
			{
				if (m_bSpawnsQuestItems)
				{
					List<Item> items = new List<Item>();
					vendorForCharacter.GetItemContainer().GetItems(ref items);
					for (int i = 0; i < items.Count; i++)
					{
						items[i].SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
					}
				}
				vendorForCharacter.SetIsObjective(isObjective: true);
			}
			return true;
		}
		return false;
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
		if (ingameSave && m_TargetInmate != null)
		{
			baseObj.Add(new JProperty("TargetInmate", m_TargetInmate.m_NetView.viewID));
		}
		baseObj.Add(new JProperty("Random", m_bRandomInmate));
		baseObj.Add(new JProperty("IsObjective", m_bIsObjective));
		baseObj.Add(new JProperty("SpawnsQuestItems", m_bSpawnsQuestItems));
		if (m_SceneTarget != null)
		{
			baseObj.Add(new JProperty("SceneTarget", m_SceneTarget.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneTarget_Scene", m_SceneTarget.m_UsedInScene));
		}
		JProperty jProperty = new JProperty("ItemsNeeded");
		JArray jArray = new JArray();
		for (int i = 0; i < m_ItemsToSell.Count; i++)
		{
			jArray.Add(m_ItemsToSell[i].m_ItemDataID);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("TargetInmate");
			if (jProperty != null)
			{
				int viewID = (int)jProperty.Value;
				m_TargetInmate = PhotonView.Find(viewID).GetComponent<Character>();
			}
		}
		JProperty jProperty2 = json.Property("Random");
		if (jProperty2 != null)
		{
			m_bRandomInmate = (bool)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("IsObjective");
		if (jProperty3 != null)
		{
			m_bIsObjective = (bool)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("SpawnsQuestItems");
		if (jProperty4 != null)
		{
			m_bSpawnsQuestItems = (bool)jProperty4.Value;
		}
		JProperty jProperty5 = json.Property("SceneTarget");
		if (jProperty5 != null)
		{
			string text = (string)json.Property("SceneTarget_Scene").Value;
			int id2 = (int)jProperty5.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_SceneTarget = ObjectiveSceneElement.FindSceneReference(id2);
			}
		}
		JProperty jProperty6 = json.Property("ItemsNeeded");
		if (jProperty6 == null || jProperty6.Value.Type != JTokenType.Array)
		{
			return;
		}
		m_ItemsToSell.Clear();
		JArray source = (JArray)jProperty6.Value;
		List<int> itemsNeededIds = source.Select((JToken c) => (int)c).ToList();
		List<ItemData> source2 = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		for (int i = 0; i < itemsNeededIds.Count; i++)
		{
			m_ItemsToSell.Add(source2.FirstOrDefault((ItemData id) => id.m_ItemDataID == itemsNeededIds[i]));
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SpawnVendorObjective;
	}
}

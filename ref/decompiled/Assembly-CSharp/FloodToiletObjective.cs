using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class FloodToiletObjective : BaseObjective
{
	public bool m_bRandomInmateToilet = true;

	public ObjectiveSceneElement m_SceneTarget;

	public bool m_bToiletAlwaysFloods;

	private Character m_TargetInmate;

	private ToiletInteraction m_ToiletToObserve;

	private int m_ToiletHUDPin = -1;

	private bool m_bSetToAlwaysFlood;

	private bool m_bFloodedToilet;

	private const string FLOODTARGET = "$FloodTarget";

	protected override void Child_PickAllTargets()
	{
		if (m_bRandomInmateToilet || m_SceneTarget == null)
		{
			m_TargetInmate = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver);
		}
		else if (m_SceneTarget != null && m_SceneTarget.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.Character)
		{
			m_TargetInmate = m_SceneTarget.GetComponent<Character>();
		}
		FindToilet();
		if (!(m_ToiletToObserve == null))
		{
		}
	}

	private void FindToilet()
	{
		if (m_TargetInmate != null)
		{
			RoomBlob myCell = m_TargetInmate.GetMyCell();
			if (myCell != null)
			{
				m_ToiletToObserve = (ToiletInteraction)myCell.GetRoomBlobData<RoomBlob_Cell>().GetCellObject(typeof(ToiletInteraction), m_TargetInmate);
			}
		}
		InternalTokenUpdate("$FloodTarget", m_TargetInmate.m_CharacterCustomisation.m_DisplayName, string.Empty);
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$FloodTarget", Localization.TokenReplaceType.Character);
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_ToiletToObserve != null && m_bToiletAlwaysFloods)
		{
			m_ToiletToObserve.SetMustFloodForPlayer(m_PlayerOwner, flood: true);
			m_bSetToAlwaysFlood = true;
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (m_ToiletToObserve != null)
		{
			return m_ToiletToObserve.IsClogged;
		}
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_ToiletToObserve != null)
		{
			if (m_ToiletToObserve.IsClogged && !m_bFloodedToilet)
			{
				m_bFloodedToilet = m_ToiletToObserve.IsClogged;
			}
		}
		else
		{
			m_bFloodedToilet = true;
		}
		return m_bFloodedToilet;
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (m_ToiletToObserve != null)
		{
			if (on)
			{
				m_ToiletHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_ToiletToObserve.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_ToiletToObserve.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
			}
			else if (m_ToiletHUDPin != -1)
			{
				PinManager.GetInstance().RemovePin(m_ToiletHUDPin);
				m_ToiletHUDPin = -1;
			}
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (!(base.PlayerOwner != null))
		{
			return;
		}
		if (on)
		{
			if (m_ToiletToObserve != null && m_ToiletToObserve.m_NetObjectLock != null && m_ToiletToObserve.m_NetObjectLock.m_NetView != null)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_ToiletToObserve.m_NetObjectLock.m_NetView);
			}
		}
		else
		{
			base.PlayerOwner.CancelObjectiveArrow();
		}
	}

	protected override void Child_PostAction()
	{
		if (m_ToiletToObserve != null && m_bSetToAlwaysFlood)
		{
			m_ToiletToObserve.SetMustFloodForPlayer(m_PlayerOwner, flood: false);
			m_bSetToAlwaysFlood = false;
		}
		m_ToiletToObserve = null;
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			if (m_TargetInmate != null)
			{
				baseObj.Add(new JProperty("TargetInmate", m_TargetInmate.m_NetView.viewID));
			}
			baseObj.Add(new JProperty("Flooded", m_bFloodedToilet));
		}
		baseObj.Add(new JProperty("Random", m_bRandomInmateToilet));
		baseObj.Add(new JProperty("AlwaysFloods", m_bToiletAlwaysFloods));
		if (m_SceneTarget != null)
		{
			baseObj.Add(new JProperty("SceneTarget", m_SceneTarget.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneTarget_Scene", m_SceneTarget.m_UsedInScene));
		}
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
				FindToilet();
			}
			JProperty jProperty2 = json.Property("Flooded");
			if (jProperty2 != null)
			{
				m_bFloodedToilet = (bool)jProperty2.Value;
			}
		}
		JProperty jProperty3 = json.Property("Random");
		if (jProperty3 != null)
		{
			m_bRandomInmateToilet = (bool)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("AlwaysFloods");
		if (jProperty4 != null)
		{
			m_bToiletAlwaysFloods = (bool)jProperty4.Value;
		}
		JProperty jProperty5 = json.Property("SceneTarget");
		if (jProperty5 != null)
		{
			string text = (string)json.Property("SceneTarget_Scene").Value;
			int id = (int)jProperty5.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_SceneTarget = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.ToiletFloodOjbective;
	}
}

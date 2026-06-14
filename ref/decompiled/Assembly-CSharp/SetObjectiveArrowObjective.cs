using Newtonsoft.Json.Linq;
using UnityEngine;

public class SetObjectiveArrowObjective : BaseObjective
{
	public ObjectiveSceneElement m_TargetReference;

	private T17NetView m_ArrowTargetObject;

	private Vector3 m_ArrowTargetPos = Vector3.zero;

	private bool m_bCancelArrow;

	protected virtual void OnDestroy()
	{
		m_ArrowTargetObject = null;
	}

	protected override void Child_PickAllTargets()
	{
		if (m_TargetReference != null)
		{
			m_ArrowTargetObject = m_TargetReference.GetComponentInParent<T17NetView>();
		}
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

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_bCancelArrow)
		{
			base.PlayerOwner.CancelObjectiveArrow();
		}
		else if (base.PlayerOwner != null)
		{
			if (m_ArrowTargetObject != null)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_ArrowTargetObject);
			}
			if (m_ArrowTargetPos != Vector3.zero)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_ArrowTargetPos);
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (!m_bCancelArrow)
		{
			if (m_ArrowTargetObject != null)
			{
				return true;
			}
			if (m_ArrowTargetPos != Vector3.zero)
			{
				return true;
			}
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_bCancelArrow)
		{
			if (base.PlayerOwner.ObjectiveArrowID == -1)
			{
				return true;
			}
			if (m_bCancelArrow)
			{
				base.PlayerOwner.CancelObjectiveArrow();
			}
		}
		else
		{
			if (base.PlayerOwner.ObjectiveArrowID >= 0)
			{
				return true;
			}
			if (base.PlayerOwner != null)
			{
				if (m_ArrowTargetObject != null)
				{
					base.PlayerOwner.SetObjectiveArrowTarget(m_ArrowTargetObject);
				}
				if (m_ArrowTargetPos != Vector3.zero)
				{
					base.PlayerOwner.SetObjectiveArrowTarget(m_ArrowTargetPos);
				}
			}
		}
		return false;
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		baseObj.Add(new JProperty("bCancelArrow", m_bCancelArrow));
		baseObj.Add(new JProperty("ArrowTargetX", m_ArrowTargetPos.x));
		baseObj.Add(new JProperty("ArrowTargetY", m_ArrowTargetPos.y));
		baseObj.Add(new JProperty("ArrowTargetZ", m_ArrowTargetPos.z));
		if (ingameSave && m_ArrowTargetObject != null)
		{
			baseObj.Add(new JProperty("TargetObject", m_ArrowTargetObject.viewID));
		}
		if (m_TargetReference != null)
		{
			baseObj.Add(new JProperty("SceneInteractObject", m_TargetReference.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneInteractObject_Scene", m_TargetReference.m_UsedInScene));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		m_bCancelArrow = (bool)json.Property("bCancelArrow").Value;
		m_ArrowTargetPos.x = (float)json.Property("ArrowTargetX").Value;
		m_ArrowTargetPos.y = (float)json.Property("ArrowTargetY").Value;
		m_ArrowTargetPos.z = (float)json.Property("ArrowTargetZ").Value;
		JProperty jProperty = json.Property("SceneInteractObject");
		if (jProperty != null)
		{
			string text = (string)json.Property("SceneInteractObject_Scene").Value;
			int id = (int)jProperty.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_TargetReference = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		if (!ingameLoad)
		{
			return;
		}
		JProperty jProperty2 = json.Property("TargetObject");
		if (jProperty2 == null)
		{
			return;
		}
		int num = (int)jProperty2.Value;
		if (!(m_TargetReference != null))
		{
			return;
		}
		m_ArrowTargetObject = m_TargetReference.GetComponentInParent<T17NetView>();
		if (m_ArrowTargetObject != null)
		{
			int viewID = m_ArrowTargetObject.viewID;
			if (viewID == num)
			{
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SetObjectiveArrowObjective;
	}
}

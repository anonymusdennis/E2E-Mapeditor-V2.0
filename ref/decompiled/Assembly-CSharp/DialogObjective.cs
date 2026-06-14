using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class DialogObjective : BaseObjective
{
	public string m_TitleLocalizationTag = "Text.Dialog.Title";

	public string m_MessageLocalizationTag = "Text.Dialog.Message";

	private bool m_bHasOpenedDialog;

	private bool m_bHasConfirmedDialog;

	public DialogObjective()
	{
		m_bResetWhenRetriggered = true;
	}

	protected override void Child_PickAllTargets()
	{
	}

	protected override void Child_Reset()
	{
		m_bHasOpenedDialog = false;
		m_bHasConfirmedDialog = false;
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (!m_bHasOpenedDialog)
		{
			T17DialogBox t17DialogBox = null;
			t17DialogBox = ((!(m_PlayerOwner == null)) ? T17DialogBoxManager.GetDialog(forSingleUser: true, m_PlayerOwner) : T17DialogBoxManager.GetDialog(forSingleUser: false));
			if (t17DialogBox != null)
			{
				m_bHasOpenedDialog = true;
				t17DialogBox.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, m_TitleLocalizationTag, m_MessageLocalizationTag, "Text.Dialog.Prompt.Yes", string.Empty, string.Empty);
				T17DialogBox t17DialogBox2 = t17DialogBox;
				t17DialogBox2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(t17DialogBox2.OnConfirm, new T17DialogBox.DialogEvent(OnConfirm));
				t17DialogBox.Show();
			}
			return false;
		}
		return m_bHasConfirmedDialog;
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

	public void OnConfirm(T17DialogBox dialog)
	{
		m_bHasConfirmedDialog = true;
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		baseObj.Add(new JProperty("TitleLoca", m_TitleLocalizationTag));
		baseObj.Add(new JProperty("MsgLoca", m_MessageLocalizationTag));
		if (ingameSave)
		{
			baseObj.Add(new JProperty("HasOpenedDialog", m_bHasOpenedDialog));
			baseObj.Add(new JProperty("HasConfirmedDialog", m_bHasConfirmedDialog));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		m_TitleLocalizationTag = (string)json.Property("TitleLoca").Value;
		m_MessageLocalizationTag = (string)json.Property("MsgLoca").Value;
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("HasOpenedDialog");
			if (jProperty != null)
			{
				m_bHasOpenedDialog = (bool)jProperty.Value;
			}
			JProperty jProperty2 = json.Property("HasConfirmedDialog");
			if (jProperty2 != null)
			{
				m_bHasConfirmedDialog = (bool)jProperty2.Value;
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.DialogObjective;
	}
}

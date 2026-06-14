using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class TutorialSpeechObjective : BaseObjective
{
	public enum Mode
	{
		Immediate,
		Wait,
		PauseOnLast
	}

	public class SpeechData
	{
		public string m_Text = "Text.Speech.Message";

		public int m_Variation = -1;

		public float m_ShowTime;

		public float m_SpeakTime;
	}

	public Mode m_Mode;

	public bool m_bCancelExisting = true;

	public List<SpeechData> m_SpeechLines = new List<SpeechData>();

	private bool[] m_ShownLines;

	private Dictionary<int, int> m_DataIndexsToIDs = new Dictionary<int, int>();

	private TutorialSpeechHUD m_SpeechHUD;

	protected override void Child_PickAllTargets()
	{
		if (HUDMenuFlow.Instance != null)
		{
			m_SpeechHUD = HUDMenuFlow.Instance.GetPlayerTutorialSpeechHUD(m_PlayerOwner.m_PlayerCameraManagerBindingID);
		}
		if (m_SpeechHUD == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
		if (m_ShownLines == null)
		{
			m_ShownLines = new bool[m_SpeechLines.Count];
		}
		if (m_DataIndexsToIDs.Count > 0)
		{
			m_DataIndexsToIDs.Clear();
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

	protected override void Child_PreAction()
	{
		if (m_SpeechHUD == null)
		{
			return;
		}
		if (m_bCancelExisting)
		{
			m_SpeechHUD.ClearSpeechQueue(succeed: true);
		}
		for (int i = 0; i < m_SpeechLines.Count; i++)
		{
			SpeechData speechData = m_SpeechLines[i];
			int num = -1;
			float lineTime = speechData.m_ShowTime;
			if (i == m_SpeechLines.Count - 1 && m_Mode == Mode.PauseOnLast)
			{
				lineTime = 0f;
			}
			if (speechData.m_Variation >= 0)
			{
				string localized = null;
				if (!Localization.Get(speechData.m_Text, out localized, speechData.m_Variation))
				{
					localized = $"{speechData.m_Text} [{speechData.m_Variation}]";
				}
				num = m_SpeechHUD.QueueSpeech(localized, isLocalised: true, lineTime, speechData.m_SpeakTime, OnSpeechLineFinished);
			}
			else
			{
				num = m_SpeechHUD.QueueSpeech(speechData.m_Text, isLocalised: false, lineTime, speechData.m_SpeakTime, OnSpeechLineFinished);
			}
			if (num >= 0)
			{
				m_DataIndexsToIDs.Add(num, i);
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		bool flag = false;
		switch (m_Mode)
		{
		case Mode.Wait:
		{
			bool flag3 = true;
			for (int j = 0; j < m_ShownLines.Length; j++)
			{
				flag3 &= m_ShownLines[j];
			}
			return flag3;
		}
		case Mode.PauseOnLast:
		{
			bool flag2 = true;
			for (int i = 0; i < m_ShownLines.Length - 1; i++)
			{
				flag2 &= m_ShownLines[i];
			}
			return flag2;
		}
		default:
			return true;
		}
	}

	protected override void Child_PostAction()
	{
		m_DataIndexsToIDs.Clear();
	}

	private void OnSpeechLineFinished(int id)
	{
		int value = -1;
		if (m_DataIndexsToIDs.TryGetValue(id, out value) && value >= 0 && value < m_ShownLines.Length)
		{
			m_ShownLines[value] = true;
		}
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave && m_ShownLines != null && m_ShownLines.Length > 0)
		{
			JProperty jProperty = new JProperty("ShownLines");
			JArray jArray = new JArray();
			for (int i = 0; i < m_ShownLines.Length; i++)
			{
				jArray.Add(m_ShownLines[i]);
			}
			jProperty.Add(jArray);
			baseObj.Add(jProperty);
		}
		baseObj.Add("Mode", (int)m_Mode);
		baseObj.Add("CancelQueued", m_bCancelExisting);
		baseObj.Add("LineCount", m_SpeechLines.Count);
		JProperty jProperty2 = new JProperty("SpeechText");
		JArray jArray2 = new JArray();
		for (int j = 0; j < m_SpeechLines.Count; j++)
		{
			jArray2.Add(m_SpeechLines[j].m_Text);
		}
		jProperty2.Add(jArray2);
		baseObj.Add(jProperty2);
		JProperty jProperty3 = new JProperty("Variations");
		jArray2 = new JArray();
		for (int k = 0; k < m_SpeechLines.Count; k++)
		{
			jArray2.Add(m_SpeechLines[k].m_Variation);
		}
		jProperty3.Add(jArray2);
		baseObj.Add(jProperty3);
		JProperty jProperty4 = new JProperty("ShowTimes");
		jArray2 = new JArray();
		for (int l = 0; l < m_SpeechLines.Count; l++)
		{
			jArray2.Add(m_SpeechLines[l].m_ShowTime);
		}
		jProperty4.Add(jArray2);
		baseObj.Add(jProperty4);
		JProperty jProperty5 = new JProperty("SpeakTimes");
		jArray2 = new JArray();
		for (int m = 0; m < m_SpeechLines.Count; m++)
		{
			jArray2.Add(m_SpeechLines[m].m_SpeakTime);
		}
		jProperty5.Add(jArray2);
		baseObj.Add(jProperty5);
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("ShownLines");
			if (jProperty != null)
			{
				JArray jArray = (JArray)jProperty.Value;
				if (jArray != null)
				{
					m_ShownLines = jArray.Select((JToken c) => (bool)c).ToArray();
				}
			}
		}
		JProperty jProperty2 = json.Property("Mode");
		if (jProperty2 != null)
		{
			int mode = (int)jProperty2.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty3 = json.Property("CancelQueued");
		if (jProperty3 != null)
		{
			m_bCancelExisting = (bool)jProperty3.Value;
		}
		JProperty jProperty4 = json.Property("LineCount");
		if (jProperty4 == null)
		{
			return;
		}
		bool flag = false;
		int num = (int)jProperty4.Value;
		if (num > 0)
		{
			m_SpeechLines.Clear();
			List<string> list = null;
			List<int> list2 = null;
			List<float> list3 = null;
			List<float> list4 = null;
			JProperty jProperty5 = json.Property("SpeechText");
			if (jProperty5 != null)
			{
				JArray jArray2 = (JArray)jProperty5.Value;
				if (jArray2 != null)
				{
					list = jArray2.Select((JToken c) => (string)c).ToList();
				}
			}
			JProperty jProperty6 = json.Property("Variations");
			if (jProperty6 != null)
			{
				JArray jArray3 = (JArray)jProperty6.Value;
				if (jArray3 != null)
				{
					list2 = jArray3.Select((JToken c) => (int)c).ToList();
				}
			}
			JProperty jProperty7 = json.Property("ShowTimes");
			if (jProperty7 != null)
			{
				JArray jArray4 = (JArray)jProperty7.Value;
				if (jArray4 != null)
				{
					list3 = jArray4.Select((JToken c) => (float)c).ToList();
				}
			}
			JProperty jProperty8 = json.Property("SpeakTimes");
			if (jProperty8 != null)
			{
				JArray jArray5 = (JArray)jProperty8.Value;
				if (jArray5 != null)
				{
					list4 = jArray5.Select((JToken c) => (float)c).ToList();
				}
			}
			if (list != null && list2 != null && list3 != null && list4 != null)
			{
				int num2 = Mathf.Min(list.Count, list2.Count, list3.Count, list4.Count);
				if (num2 == num)
				{
					for (int i = 0; i < num; i++)
					{
						SpeechData speechData = new SpeechData();
						speechData.m_Text = list[i];
						speechData.m_Variation = list2[i];
						speechData.m_ShowTime = list3[i];
						speechData.m_SpeakTime = list4[i];
						SpeechData item = speechData;
						m_SpeechLines.Add(item);
					}
					flag = true;
				}
			}
		}
		if (flag)
		{
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.TutorialSpeechObjective;
	}
}

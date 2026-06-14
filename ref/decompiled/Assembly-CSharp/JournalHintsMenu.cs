using UnityEngine;

public class JournalHintsMenu : GameMenuBehaviour
{
	public GameObject m_HintItemPrefab;

	public T17ScrollView m_HintList;

	private int m_ButtonCount;

	private bool m_bHintsSaveLoaded;

	protected override void Awake()
	{
		base.Awake();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (!m_bHintsSaveLoaded)
		{
			LoadHintsFromSave(currentGamer);
			m_bHintsSaveLoaded = true;
		}
		return true;
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void AddItem(string hintLocalisationTag)
	{
		if (!(m_HintItemPrefab != null))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(m_HintItemPrefab);
		m_HintList.AddNewObject(gameObject);
		T17Text component = gameObject.GetComponent<T17Text>();
		if (component != null)
		{
			component.SetNewLocalizationTag(hintLocalisationTag);
		}
		T17Button component2 = gameObject.GetComponent<T17Button>();
		if (component2 != null)
		{
			component2.SetGamerForEventSystem(base.CurrentGamer, base.CachedEventSystem);
			component2.onClick.RemoveAllListeners();
			if (m_ButtonCount == 0)
			{
				m_TopSelectable = component2;
			}
			m_ButtonCount++;
		}
	}

	private void LoadHintsFromSave(Gamer gamer)
	{
		GlobalHintManager.GetInstance().GetHintBitfield(gamer, LevelScript.GetCurrentLevelInfo().m_PrisonEnum, out var bitfield);
		for (int i = 0; i < GlobalHintManager.GetInstance().GetTotalHintCount(LevelScript.GetCurrentLevelInfo().m_PrisonEnum); i++)
		{
			if ((bitfield & (1 << i)) > 0)
			{
				HintConfig.HintData hintData = GlobalHintManager.GetInstance().GetHintData(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, i);
				if (hintData != null)
				{
					AddItem(hintData.m_FullHint);
				}
			}
		}
	}
}

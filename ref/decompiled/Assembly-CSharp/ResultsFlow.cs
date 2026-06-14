using UnityEngine;

public class ResultsFlow : BaseFlowBehaviour
{
	private enum MODE
	{
		MODE_INIT,
		MODE_SHOW_MENUS,
		MODE_RUNNING
	}

	public static ResultsFlow Instance;

	public ResultsRootMenu m_MainMenu;

	private MODE m_ResultsFlow;

	private bool m_bExitResultsRequested;

	protected override void Awake()
	{
		base.Awake();
		if (Instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	protected override void Start()
	{
		base.Start();
		m_ResultsFlow = MODE.MODE_INIT;
		HideMenus();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		switch (m_ResultsFlow)
		{
		case MODE.MODE_SHOW_MENUS:
		{
			ConfigManager instance = ConfigManager.GetInstance();
			ResultsRootMenu.ResultsMenuTypeToOpen typeToOpen = ResultsRootMenu.ResultsMenuTypeToOpen.CoopResults;
			if (instance != null)
			{
				switch (instance.gameType)
				{
				case PrisonConfig.ConfigType.Versus:
					typeToOpen = ResultsRootMenu.ResultsMenuTypeToOpen.VersusResults;
					break;
				case PrisonConfig.ConfigType.Cooperative:
				case PrisonConfig.ConfigType.Singleplayer:
					typeToOpen = ResultsRootMenu.ResultsMenuTypeToOpen.CoopResults;
					break;
				}
			}
			ShowMenus(typeToOpen);
			if (GlobalSave.GetInstance() != null)
			{
				GlobalSave.GetInstance().RequestSave();
			}
			m_ResultsFlow = MODE.MODE_RUNNING;
			break;
		}
		}
	}

	public void StartResults()
	{
		m_ResultsFlow = MODE.MODE_SHOW_MENUS;
	}

	public void ShowMenus(ResultsRootMenu.ResultsMenuTypeToOpen typeToOpen)
	{
		if (m_MainMenu != null)
		{
			m_MainMenu.InitializeData();
			m_MainMenu.SetResultsMenuTypeToOpen(typeToOpen);
			m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		}
	}

	public void HideMenus()
	{
		if (m_MainMenu != null)
		{
			m_MainMenu.Hide();
		}
	}

	public void SetExitRequested()
	{
		m_bExitResultsRequested = true;
	}

	public bool ExitResultsRequested()
	{
		return m_bExitResultsRequested;
	}

	public void ReturnToLobby()
	{
		SetExitRequested();
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null)
		{
			instance.m_ReturnToFrontendRoute = GlobalStart.ReturnToFrontendRoutes.VersusLobby;
		}
	}

	public void ReturnToMainMenu()
	{
		SetExitRequested();
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null)
		{
			instance.m_ReturnToFrontendRoute = GlobalStart.ReturnToFrontendRoutes.None;
		}
	}
}

using UnityEngine;

public class Results_VersusMenu : BaseResultsScreen
{
	[Header("Results Versus Menu")]
	public float m_ScreenShowDuration = 15f;

	private float m_TimestampForClose = -1f;

	public string m_NoCharactersEscapedText = "TBT: Time's Up";

	public string m_YouLostText = "TBT: You Lost";

	public string m_YouWonText = "TBT: You Won";

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		Gamer gamer = null;
		if (EscapePrisonFunctionality.GetInstance() != null)
		{
			Character escapingCharacter = EscapePrisonFunctionality.GetInstance().GetEscapingCharacter();
			if (escapingCharacter != null)
			{
				Player component = escapingCharacter.GetComponent<Player>();
				if (component != null)
				{
					gamer = component.m_Gamer;
				}
			}
		}
		if (gamer != null && gamer.IsLocal())
		{
			Platform.GetInstance().SetPresenceTag("Text.Presence.VersusWin");
			if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus && T17NetManager.IsConnectedOnline())
			{
				StatSystem.GetInstance().IncStat(10, 1f, gamer, string.Empty);
				Localization.Get("Text.ActivityFeed.VersusWin.Cap", out var localized);
				Localization.Get("Text.ActivityFeed.VersusWin.Con", out var localized2);
				Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Won_Versus, localized, localized2, 0u);
			}
		}
		else
		{
			Platform.GetInstance().SetPresenceTag("Text.Presence.VersusLoss");
		}
		m_TimestampForClose = Time.time + m_ScreenShowDuration;
		if (T17NetManager.IsMasterClient)
		{
		}
		return true;
	}

	protected override Player GetWinningPlayer()
	{
		if (EscapePrisonFunctionality.GetInstance() != null)
		{
			Player player = EscapePrisonFunctionality.GetInstance().GetEscapingCharacter() as Player;
			if (player != null)
			{
				return player;
			}
		}
		return base.GetWinningPlayer();
	}

	protected override void Update()
	{
		base.Update();
		if (SafeToTimeout() && m_TimestampForClose > 0f && Time.time > m_TimestampForClose)
		{
			m_TimestampForClose = -1f;
			ReturnToLobby();
		}
	}

	public void RequestExit()
	{
		ReturnToLobby();
	}

	private bool SafeToTimeout()
	{
		bool result = true;
		T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
		if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue) && (outValue == T17NetRoomGameView.GameRoomType.Public || outValue == T17NetRoomGameView.GameRoomType.Private))
		{
			result = T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom();
		}
		return result;
	}

	protected override string GetMainTitleText()
	{
		Character escapingCharacter = EscapePrisonFunctionality.GetInstance().GetEscapingCharacter();
		Character currentGamePlayer = base.CurrentGamePlayer;
		if (escapingCharacter == currentGamePlayer || Gamer.GetGamerCount() == 1)
		{
			return m_YouWonText;
		}
		if (escapingCharacter != null)
		{
			return m_YouLostText;
		}
		return m_NoCharactersEscapedText;
	}

	protected override string GetGradedScore(ScoreManager.PlayerScorePODO scorePodo, out Sprite theGradeSprite, out int gradeLevel)
	{
		Character escapingCharacter = EscapePrisonFunctionality.GetInstance().GetEscapingCharacter();
		Character currentGamePlayer = base.CurrentGamePlayer;
		if (escapingCharacter == currentGamePlayer || Gamer.GetGamerCount() == 1)
		{
			return ScoreManager.GetGradedScore(scorePodo.m_IngameSecondsTakenToEscape, out theGradeSprite, out gradeLevel);
		}
		theGradeSprite = null;
		gradeLevel = int.MaxValue;
		return string.Empty;
	}
}

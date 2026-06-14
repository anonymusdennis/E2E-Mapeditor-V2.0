using UnityEngine;

public class MinigameMap : MonoBehaviour
{
	public enum MinigameTypes
	{
		ST_FireBreathing,
		ST_HulaHoops,
		ST_Juggling,
		ST_Unicycle,
		Job_Stonemason
	}

	[Header("Minigame masher settings")]
	public AlternateButtonMasher.AlternateMasherSettings m_AlternateMasherSettings;

	public GymMasher_KettleBelts.HoldingMasherSettings m_HoldingMasherSetings;

	public GymMasher_Pullup.PullupMasherSettings m_PullUpsMasherSettings;

	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_TreadmillMasherSettings;

	public GymMasher_Pommel_Footbag.PommelMasherSettings m_PommelHorseMasherSettings;

	public SolitaryPotatoMasher.MasherSettings m_SoltiaryMasherSettings;

	public ReadingMasher.MasherSettings m_ReadingMasherSettings;

	[Header("Minigame Requirements")]
	public MinigameCompletionHelper m_MinigameCompletionHelper;

	public MinigameTypes m_MinigameType;

	public bool m_bIsInfinite;

	protected IMinigameMasher m_ButtonMasher;

	private bool m_bMinigameSet;

	private bool m_bHasCompletedMinigame;

	private bool m_bHasCompletedRep;

	private bool m_bIsSignificantMinigameMoment;

	protected IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		switch (m_MinigameType)
		{
		case MinigameTypes.ST_FireBreathing:
			m_ButtonMasher = trackedUIElements.GetShowTimeFireBreathingMasher();
			break;
		case MinigameTypes.ST_HulaHoops:
			m_ButtonMasher = trackedUIElements.GetShowTimeHulaHoopsMasher();
			break;
		case MinigameTypes.ST_Juggling:
			m_ButtonMasher = trackedUIElements.GetShowTimeJugglingMasher();
			break;
		case MinigameTypes.ST_Unicycle:
			m_ButtonMasher = trackedUIElements.GetShowTimeUnicycleMasher();
			break;
		case MinigameTypes.Job_Stonemason:
			m_ButtonMasher = trackedUIElements.GetStonemasonCarvingMasher();
			break;
		}
		switch (m_MinigameType)
		{
		case MinigameTypes.ST_FireBreathing:
		case MinigameTypes.ST_HulaHoops:
		case MinigameTypes.ST_Juggling:
		case MinigameTypes.ST_Unicycle:
		case MinigameTypes.Job_Stonemason:
		{
			SolitaryPotatoMasher solitaryPotatoMasher = m_ButtonMasher as SolitaryPotatoMasher;
			solitaryPotatoMasher.SetMasherSettings(m_SoltiaryMasherSettings);
			break;
		}
		}
		m_bMinigameSet = m_ButtonMasher != null;
		return m_ButtonMasher;
	}

	protected void OnDestroy()
	{
		m_ButtonMasher = null;
	}

	public IMinigameMasher RestAndShowMinigame(Character localCharacter)
	{
		m_MinigameCompletionHelper.ResetForNewUser(localCharacter);
		if (localCharacter.IsPlayer())
		{
			((Player)localCharacter).OnMinigameEntered();
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(((Player)localCharacter).m_PlayerCameraManagerBindingID);
			m_ButtonMasher = SetupButtonMasher(playerTrackedUIElements);
			m_ButtonMasher.EnableForPlayer(localCharacter as Player);
			return m_ButtonMasher;
		}
		return null;
	}

	public void DisableMinigameHud(Character localCharacter)
	{
		if (localCharacter != null)
		{
			if (localCharacter.IsPlayer() && m_ButtonMasher != null)
			{
				m_ButtonMasher.Disable();
				m_ButtonMasher = null;
				m_bMinigameSet = false;
			}
			if (localCharacter.IsPlayer())
			{
				Player player = (Player)localCharacter;
				player.OnMinigameExited();
			}
		}
	}

	public void UpdateInteraction(Character interacter, out bool hasCompletedRep, out bool hasCompletedMinigame)
	{
		bool flag = interacter.IsPlayer();
		hasCompletedRep = flag && m_ButtonMasher.HasCompletedRep();
		if (m_MinigameCompletionHelper.UpdateUser(hasCompletedRep))
		{
			hasCompletedMinigame = !m_bIsInfinite;
		}
		else
		{
			hasCompletedMinigame = false;
		}
		m_bHasCompletedMinigame = hasCompletedMinigame;
		m_bHasCompletedRep = hasCompletedRep;
		m_bIsSignificantMinigameMoment = flag && m_ButtonMasher.IsSignificantMomentInMinigame();
	}

	public bool IsReadyForUpdate()
	{
		return m_bMinigameSet;
	}

	public bool HasCompletedMinigame()
	{
		return m_bHasCompletedMinigame;
	}

	public bool HasCompletedRep()
	{
		return m_bHasCompletedRep;
	}

	public bool IsSignificantMomentInMinigame()
	{
		return m_bIsSignificantMinigameMoment;
	}

	public IMinigameMasher GetLinkedMasher()
	{
		return m_ButtonMasher;
	}
}

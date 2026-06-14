using System;
using UnityEngine;

public class PlayerStatsHUD : BaseMenuBehaviour
{
	public T17Button m_StatsButton;

	public T17Text m_HealthStat;

	public T17Text m_EnergyStat;

	public T17Text m_HeatStat;

	private Animator m_HealthLabelAnimator;

	private Animator m_EnergyLabelAnimator;

	private Animator m_HeatLabelAnimator;

	public Animator m_HealthImageAnimator;

	public Animator m_EnergyImageAnimator;

	public Animator m_HeatImageAnimator;

	public int m_HealthForPulse = 30;

	public int m_EnergyForPulse = 30;

	public int m_HeatForPulse = 70;

	private bool m_bHealthPulseActive;

	private bool m_bEnergyPulseActive;

	private bool m_bHeatPulseActive;

	public string m_TextPulseTrigger = "Pulse";

	public float m_LowEnergyFlashLength;

	private float m_LowEnergyFlashTimer;

	private Item_Combat m_UnarmedConfig;

	private float m_PreviousHealth = -999f;

	private float m_PreviousEnergy = -999f;

	private float m_PreviousHeat = -999f;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		m_HealthLabelAnimator = m_HealthStat.GetComponent<Animator>();
		m_EnergyLabelAnimator = m_EnergyStat.GetComponent<Animator>();
		m_HeatLabelAnimator = m_HeatStat.GetComponent<Animator>();
		if (m_HealthStat != null)
		{
			m_HealthStat.m_bNeedsLocalization = false;
		}
		if (m_EnergyStat != null)
		{
			m_EnergyStat.m_bNeedsLocalization = false;
		}
		if (m_HeatStat != null)
		{
			m_HeatStat.m_bNeedsLocalization = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamePlayer != null)
		{
			UpdateStatsSliders();
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (base.CurrentGamePlayer != null)
		{
			SetupStatsSliders();
		}
		if (m_StatsButton != null)
		{
			T17Button statsButton = m_StatsButton;
			statsButton.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(statsButton.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(OnPointerEnter));
			T17Button statsButton2 = m_StatsButton;
			statsButton2.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(statsButton2.OnButtonPointerExit, new T17Button.T17ButtonDelegate(OnPointerExit));
			m_StatsButton.m_CanUIReselectDelegate = () => false;
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_StatsButton != null)
		{
			T17Button statsButton = m_StatsButton;
			statsButton.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Remove(statsButton.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(OnPointerEnter));
			T17Button statsButton2 = m_StatsButton;
			statsButton2.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Remove(statsButton2.OnButtonPointerExit, new T17Button.T17ButtonDelegate(OnPointerExit));
			m_StatsButton.m_CanUIReselectDelegate = null;
		}
		return true;
	}

	public override void SetGamePlayer(Player gamePlayer)
	{
		base.SetGamePlayer(gamePlayer);
		if (base.CurrentGamePlayer != null)
		{
			SetupStatsSliders();
		}
	}

	private void SetupStatsSliders()
	{
		if (!(base.CurrentGamePlayer.m_CharacterStats == null))
		{
			if (m_HealthStat != null)
			{
				m_HealthStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Health), bSingleAs2: false);
			}
			if (m_EnergyStat != null)
			{
				m_EnergyStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Energy), bSingleAs2: false);
			}
			if (m_HeatStat != null)
			{
				m_HeatStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Heat), bSingleAs2: false);
			}
			m_UnarmedConfig = ConfigManager.GetInstance().combatConfig.m_UnarmedCombatConfig;
		}
	}

	private void PlayPulseIfSignificantChange(float currentStat, float threshold, ref bool isFlagSet, Animator animatorsA, Animator animatorsB)
	{
		bool flag = currentStat < threshold;
		if (flag != isFlagSet)
		{
			if (animatorsA != null)
			{
				animatorsA.SetBool(m_TextPulseTrigger, flag);
			}
			if (animatorsB != null)
			{
				animatorsB.SetBool(m_TextPulseTrigger, flag);
			}
			isFlagSet = flag;
		}
	}

	private void UpdateStatsSliders()
	{
		if (base.CurrentGamePlayer.m_CharacterStats == null)
		{
			return;
		}
		if (m_HealthStat != null && !Mathf.Approximately(m_PreviousHealth, base.CurrentGamePlayer.m_CharacterStats.Health))
		{
			m_HealthStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Health), bSingleAs2: false);
			m_PreviousHealth = base.CurrentGamePlayer.m_CharacterStats.Health;
			PlayPulseIfSignificantChange(base.CurrentGamePlayer.m_CharacterStats.Health, m_HealthForPulse, ref m_bHealthPulseActive, m_HealthLabelAnimator, m_HealthImageAnimator);
		}
		if (m_EnergyStat != null && !Mathf.Approximately(m_PreviousEnergy, base.CurrentGamePlayer.m_CharacterStats.Energy))
		{
			m_EnergyStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Energy), bSingleAs2: false);
			EnergyModifier energyLevel = base.CurrentGamePlayer.m_CharacterStats.EnergyLevel;
			float normalAttackEnergyCost = m_UnarmedConfig.m_CombatConfig.GetNormalAttackEnergyCost(energyLevel);
			if (!base.CurrentGamePlayer.m_CharacterStats.HasEnoughEnergyForTask(normalAttackEnergyCost))
			{
				m_LowEnergyFlashTimer -= UpdateManager.deltaTime;
				if (m_LowEnergyFlashTimer < 0f)
				{
					m_LowEnergyFlashTimer = m_LowEnergyFlashLength;
				}
			}
			m_PreviousEnergy = base.CurrentGamePlayer.m_CharacterStats.Energy;
			PlayPulseIfSignificantChange(base.CurrentGamePlayer.m_CharacterStats.Energy, m_EnergyForPulse, ref m_bEnergyPulseActive, m_EnergyLabelAnimator, m_EnergyImageAnimator);
		}
		if (m_HeatStat != null && !Mathf.Approximately(m_PreviousHeat, base.CurrentGamePlayer.m_CharacterStats.Heat))
		{
			m_HeatStat.text = NumberToStringCache.GetIntAsString(Mathf.CeilToInt(base.CurrentGamePlayer.m_CharacterStats.Heat), bSingleAs2: false);
			m_PreviousHeat = base.CurrentGamePlayer.m_CharacterStats.Heat;
			PlayPulseIfSignificantChange(100f - base.CurrentGamePlayer.m_CharacterStats.Heat, 100f - (float)m_HeatForPulse, ref m_bHeatPulseActive, m_HeatLabelAnimator, m_HeatImageAnimator);
		}
	}

	public void OnStatsHUDClicked()
	{
		if (base.CurrentGamePlayer != null)
		{
			base.CurrentGamePlayer.RequestToOpenInventory();
		}
	}

	public void OnPointerEnter(T17Button sender)
	{
		if (base.CurrentGamer != null && base.CurrentGamer.m_PlayerObject != null)
		{
			HUDMenuFlow.Instance.AddMouseHUDItem(base.CurrentGamer.m_PlayerObject.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}

	public void OnPointerExit(T17Button sender)
	{
		if (base.CurrentGamer != null && base.CurrentGamer.m_PlayerObject != null)
		{
			HUDMenuFlow.Instance.RemoveMouseHUDItem(base.CurrentGamer.m_PlayerObject.m_PlayerCameraManagerBindingID, base.gameObject);
		}
	}
}

using UnityEngine;

public class GymMasherBase : MasherBase, IMinigameMasher
{
	public Vector2 m_CharacterOffset = new Vector2(0f, 0f);

	[SerializeField]
	public WorldSpaceHudScalePODO m_WorldSpacePositionInfo;

	protected AlternateButtonMasher.MasherState m_MasherState;

	private bool m_bRepACompleted;

	private bool m_bRepBCompleted;

	protected virtual void Awake()
	{
	}

	protected virtual void Update()
	{
		PositionForPlayer();
	}

	private void PositionForPlayer()
	{
		if (m_RewiredPlayer != null && !(m_Player == null))
		{
			float z = base.transform.position.z;
			Vector3 position = new Vector3(m_Player.m_Transform.position.x + m_CharacterOffset.x, m_Player.m_Transform.position.y + m_CharacterOffset.y, z);
			m_WorldSpacePositionInfo.PositionTransform(base.transform, position, HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_Player.m_PlayerCameraManagerBindingID));
		}
	}

	protected void OnRepACompleted()
	{
		m_bRepACompleted = true;
	}

	protected void OnRepBCompleted()
	{
		m_bRepBCompleted = true;
	}

	public bool ConsumeIsRepACompleted()
	{
		if (m_bRepACompleted)
		{
			m_bRepACompleted = false;
			return true;
		}
		return false;
	}

	public bool ConsumeIsRepBCompleted()
	{
		if (m_bRepBCompleted)
		{
			m_bRepBCompleted = false;
			return true;
		}
		return false;
	}

	public override void SetPlayerToCheck(Player player)
	{
		base.SetPlayerToCheck(player);
		if (player != null)
		{
			PositionForPlayer();
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_Player = null;
		m_RewiredPlayer = null;
	}

	public virtual AlternateButtonMasher.MasherState GetMasherState()
	{
		return m_MasherState;
	}

	protected virtual void Setup()
	{
	}

	public virtual bool StaminaSpent()
	{
		return false;
	}

	public bool HasCompletedRep()
	{
		return GetMasherState() == AlternateButtonMasher.MasherState.Valid;
	}

	public void EnableForPlayer(Player thePlayer)
	{
		SetPlayerToCheck(thePlayer);
		base.gameObject.SetActive(value: true);
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public bool IsEnabled()
	{
		return base.gameObject.activeSelf;
	}

	public bool IsSignificantMomentInMinigame()
	{
		return StaminaSpent();
	}
}

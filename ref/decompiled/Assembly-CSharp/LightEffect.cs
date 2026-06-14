public abstract class LightEffect
{
	public enum Effects
	{
		Lockdown,
		LockdownFadeOut,
		Party,
		TEMP,
		ShowTimeEffect
	}

	protected LightingManager.LightGroup m_ParentGroup;

	protected bool m_bIsActive;

	public bool IsActive => m_bIsActive;

	public abstract Effects GetEffectType();

	public virtual bool Init(LightingManager.LightGroup group)
	{
		m_ParentGroup = group;
		return true;
	}

	public virtual void UpdateEffect(float deltaTime)
	{
	}

	public virtual void OnGroupUpdated(float deltaTime)
	{
	}

	public virtual void OnGroupTurnedOn()
	{
	}

	public virtual void OnGroupTurnedOff()
	{
	}

	public abstract string Serialize();

	public abstract bool Deserialize(string data);

	public static LightEffect CreateNewEffectInstance(Effects type)
	{
		LightEffect result = null;
		switch (type)
		{
		case Effects.Lockdown:
			result = new LightEffect_Lockdown();
			break;
		case Effects.LockdownFadeOut:
			result = new LightEffect_LockdownFadeOut();
			break;
		case Effects.Party:
			result = new LightEffect_Party();
			break;
		case Effects.ShowTimeEffect:
			result = new LightEffect_ShowTime();
			break;
		}
		return result;
	}
}

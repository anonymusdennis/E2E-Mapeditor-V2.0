public class FakeCharacter : T17MonoBehaviour, IControlledUpdate
{
	public CharacterSpeechBubbleHandler m_SpeechBubbleHandler;

	private T17NetView m_NetView;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		UpdateManager.GetInstance().Register(this, UpdateCategory.FakeCharacter);
		m_NetView = GetComponent<T17NetView>();
		return base.StartInit();
	}

	public virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.FakeCharacter);
		}
	}

	public virtual void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public void SaySomethingLocally(SpeechPODO speech)
	{
		if (m_SpeechBubbleHandler != null && speech.IsSet())
		{
			Localization.Get(speech.m_TextId, out var localized);
			m_SpeechBubbleHandler.NewSpeech(localized, speech.m_SpeechTone, speech.m_Duration, speech.m_Priority, bAllowTextColourControl: false);
		}
	}

	public void SaySomethingRPC(SpeechPODO speech)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_SaySomething", NetTargets.All, speech.m_TextId, speech.m_SpeechTone, speech.m_Duration, speech.m_Priority);
		}
	}

	[PunRPC]
	protected void RPC_SaySomething(string speechId, SpeechTone tone, float speechDuration, int priority)
	{
		Localization.Get(speechId, out var localized);
		m_SpeechBubbleHandler.NewSpeech(localized, tone, speechDuration, priority, bAllowTextColourControl: false);
	}
}

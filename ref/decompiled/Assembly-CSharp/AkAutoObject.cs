using UnityEngine;

public class AkAutoObject
{
	private GameObject m_gameObj;

	public AkAutoObject(GameObject GameObj)
	{
		m_gameObj = GameObj;
		AkSoundEngine.RegisterGameObj(GameObj, "AkAutoObject.cs", 1u);
	}

	~AkAutoObject()
	{
		AkSoundEngine.UnregisterGameObj(m_gameObj);
	}
}

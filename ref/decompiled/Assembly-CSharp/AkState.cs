using UnityEngine;

[AddComponentMenu("Wwise/AkState")]
public class AkState : AkUnityEventHandler
{
	public int groupID;

	public int valueID;

	public override void HandleEvent(GameObject in_gameObject)
	{
		AkSoundEngine.SetState((uint)groupID, (uint)valueID);
	}
}

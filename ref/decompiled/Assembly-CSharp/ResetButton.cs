using UnityEngine;

public class ResetButton : MonoBehaviour
{
	private void Start()
	{
	}

	public void DoTheReset()
	{
		if (ItemContainerManager.GetInstance() != null)
		{
			ItemContainerManager.GetInstance().RefreshAllItemContainers(stagger: false);
		}
	}

	public void DoSomethingElse()
	{
	}
}

using UnityEngine;

public class T17NetDirectoryService : MonoBehaviour
{
	private static T17NetDirectoryService s_TheInstance;

	public static T17NetDirectoryService theInstance => s_TheInstance;

	public static bool hasRecievedURLs => true;

	public static string keyDistributionServerURL => string.Empty;

	public static string photonAppId => string.Empty;

	private void Awake()
	{
		s_TheInstance = this;
		Object.DontDestroyOnLoad(this);
	}

	public void BeginRetrieveURLs()
	{
	}
}

using UnityEngine;

public class T17NetEncryptionKeys : MonoBehaviour
{
	public delegate void OnKeysRetrievedEvent();

	public delegate void OnAuthTokenValidatedEvent(bool bSuccess);

	public static T17NetEncryptionKeys Instance;

	private bool m_IsPlatformAuthTokenValid;

	public bool isPlatformAuthTokenokenValid => m_IsPlatformAuthTokenValid;

	public static event OnKeysRetrievedEvent OnKeysRetrieved;

	public static event OnAuthTokenValidatedEvent OnAuthTokenValidateCompleted;

	private void Awake()
	{
		Instance = this;
		Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public bool ValidatePlatformAuthToken(string theToken)
	{
		if (T17NetEncryptionKeys.OnAuthTokenValidateCompleted != null)
		{
			T17NetEncryptionKeys.OnAuthTokenValidateCompleted(bSuccess: false);
		}
		return false;
	}

	public bool OnEnteredNewSession(string sessionName)
	{
		if (T17NetEncryptionKeys.OnKeysRetrieved != null)
		{
			T17NetEncryptionKeys.OnKeysRetrieved();
		}
		return true;
	}

	public static object Encrypt(object input)
	{
		return input;
	}

	public static object Decrypt(object input)
	{
		return input;
	}

	public static bool HaveEncryptionKey()
	{
		return true;
	}

	public void ClearKeys()
	{
	}
}

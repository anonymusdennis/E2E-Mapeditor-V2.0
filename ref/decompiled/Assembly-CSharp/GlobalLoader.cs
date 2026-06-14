using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalLoader : MonoBehaviour
{
	private void Start()
	{
		AssetManager.instance.LoadSceneAsync("Global", LoadSceneMode.Single);
	}
}

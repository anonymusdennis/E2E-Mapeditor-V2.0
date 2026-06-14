using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUp : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(LoadBootScene());
	}

	private IEnumerator LoadBootScene()
	{
		Application.backgroundLoadingPriority = ThreadPriority.High;
		yield return AssetManager.instance.LoadSceneAsync("Global", LoadSceneMode.Single);
	}
}

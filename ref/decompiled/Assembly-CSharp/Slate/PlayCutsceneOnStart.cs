using UnityEngine;
using UnityEngine.Events;

namespace Slate;

[AddComponentMenu("SLATE/Play Cutscene On Start")]
public class PlayCutsceneOnStart : MonoBehaviour
{
	public Cutscene cutscene;

	public UnityEvent onFinish;

	private void Start()
	{
		if (cutscene == null)
		{
			Debug.LogError("Cutscene is not provided", base.gameObject);
			return;
		}
		cutscene.Play(delegate
		{
			onFinish.Invoke();
		});
	}

	public static GameObject Create()
	{
		return new GameObject("Cutscene Starter").AddComponent<PlayCutsceneOnStart>().gameObject;
	}
}

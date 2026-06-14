using UnityEngine;
using UnityEngine.Events;

namespace Slate;

[AddComponentMenu("SLATE/Play Cutscene On Trigger")]
public class PlayCutsceneOnTrigger : MonoBehaviour
{
	public Cutscene cutscene;

	public bool checkSpecificTagOnly = true;

	public string tagName = "Player";

	public bool once;

	public UnityEvent onFinish;

	private void OnTriggerEnter(Collider other)
	{
		if (cutscene == null)
		{
			Debug.LogError("Cutscene is not provided", base.gameObject);
			return;
		}
		if (checkSpecificTagOnly && !string.IsNullOrEmpty(tagName))
		{
			if (other.gameObject.tag == tagName)
			{
				cutscene.Play(delegate
				{
					onFinish.Invoke();
				});
			}
		}
		else
		{
			cutscene.Play(delegate
			{
				onFinish.Invoke();
			});
		}
		if (once)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Reset()
	{
		Collider collider = GetComponent<Collider>();
		if (collider == null)
		{
			collider = base.gameObject.AddComponent<BoxCollider>();
		}
		collider.isTrigger = true;
	}

	public static GameObject Create()
	{
		return new GameObject("Cutscene Trigger").AddComponent<PlayCutsceneOnTrigger>().gameObject;
	}
}

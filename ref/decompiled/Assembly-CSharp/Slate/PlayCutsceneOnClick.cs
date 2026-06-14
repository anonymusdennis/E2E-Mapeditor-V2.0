using UnityEngine;
using UnityEngine.Events;

namespace Slate;

[AddComponentMenu("SLATE/Play Cutscene On Click")]
public class PlayCutsceneOnClick : MonoBehaviour
{
	public Cutscene cutscene;

	public UnityEvent onFinish;

	private void OnMouseDown()
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

	private void Reset()
	{
		Collider component = GetComponent<Collider>();
		if (component == null)
		{
			component = base.gameObject.AddComponent<BoxCollider>();
		}
	}

	public static GameObject Create()
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.name = "Cutscene Click Trigger";
		gameObject.AddComponent<PlayCutsceneOnClick>();
		return gameObject;
	}
}

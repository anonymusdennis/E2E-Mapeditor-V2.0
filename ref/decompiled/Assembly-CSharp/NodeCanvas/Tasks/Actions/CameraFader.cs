using System.Collections;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class CameraFader : MonoBehaviour
{
	private static CameraFader _current;

	private float alpha;

	private Texture2D _blackTexture;

	private Texture2D blackTexture
	{
		get
		{
			if (_blackTexture == null)
			{
				_blackTexture = new Texture2D(1, 1);
				_blackTexture.SetPixel(1, 1, Color.black);
				_blackTexture.Apply();
			}
			return _blackTexture;
		}
	}

	public static CameraFader current
	{
		get
		{
			if (_current == null)
			{
				_current = Object.FindObjectOfType<CameraFader>();
			}
			if (_current == null)
			{
				_current = new GameObject("_CameraFader").AddComponent<CameraFader>();
			}
			return _current;
		}
	}

	public void FadeIn(float time)
	{
		StartCoroutine(CoroutineFadeIn(time));
	}

	public void FadeOut(float time)
	{
		StartCoroutine(CoroutineFadeOut(time));
	}

	private IEnumerator CoroutineFadeIn(float time)
	{
		for (alpha = 1f; alpha > 0f; alpha -= 1f / time * UpdateManager.deltaTime)
		{
			yield return null;
		}
	}

	private IEnumerator CoroutineFadeOut(float time)
	{
		for (alpha = 0f; alpha < 1f; alpha += 1f / time * UpdateManager.deltaTime)
		{
			yield return null;
		}
	}
}

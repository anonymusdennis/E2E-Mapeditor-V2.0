using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slate;

public class CutsceneSequencePlayer : MonoBehaviour
{
	public bool playOnStart = true;

	public List<Cutscene> cutscenes;

	public UnityEvent onFinish;

	private int currentIndex;

	private bool isPlaying;

	private void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		if (isPlaying)
		{
			Debug.LogWarning("Sequence is already playing", base.gameObject);
			return;
		}
		if (cutscenes.Count == 0)
		{
			Debug.LogError("No Cutscenes provided", base.gameObject);
			return;
		}
		isPlaying = true;
		currentIndex = 0;
		MoveNext();
	}

	public void Stop()
	{
		if (isPlaying)
		{
			isPlaying = false;
			cutscenes[currentIndex].Stop();
		}
	}

	private void MoveNext()
	{
		if (!isPlaying || currentIndex >= cutscenes.Count)
		{
			isPlaying = false;
			onFinish.Invoke();
			return;
		}
		Cutscene cutscene = cutscenes[currentIndex];
		if (cutscene == null)
		{
			Debug.LogError("Cutscene is null in Cutscene Sequencer", base.gameObject);
			return;
		}
		currentIndex++;
		cutscene.Play(delegate
		{
			MoveNext();
		});
	}

	public static GameObject Create()
	{
		GameObject gameObject = new GameObject("CutsceneSequencePlayer");
		gameObject.AddComponent<CutsceneSequencePlayer>();
		return gameObject;
	}
}

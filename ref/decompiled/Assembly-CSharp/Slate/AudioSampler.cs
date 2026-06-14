using System.Collections.Generic;
using UnityEngine;

namespace Slate;

public static class AudioSampler
{
	private static readonly string ROOT_NAME = "_AudioSources";

	private static GameObject root;

	private static Dictionary<object, AudioSource> sources = new Dictionary<object, AudioSource>();

	public static AudioSource GetSourceForID(object keyID)
	{
		AudioSource value = null;
		if (sources.TryGetValue(keyID, out value) && value != null)
		{
			return value;
		}
		if (root == null)
		{
			root = GameObject.Find(ROOT_NAME);
			if (root == null)
			{
				root = new GameObject(ROOT_NAME);
			}
		}
		AudioSource audioSource = new GameObject("_AudioSource").AddComponent<AudioSource>();
		audioSource.transform.SetParent(root.transform);
		audioSource.playOnAwake = false;
		AudioSource audioSource2 = audioSource;
		sources[keyID] = audioSource2;
		return audioSource2;
	}

	public static void ReleaseSourceForID(object keyID)
	{
		AudioSource value = null;
		if (sources.TryGetValue(keyID, out value))
		{
			if (value != null)
			{
				Object.DestroyImmediate(value.gameObject);
			}
			sources.Remove(keyID);
		}
		if (sources.Count == 0)
		{
			Object.DestroyImmediate(root);
			root = null;
		}
	}

	public static void SampleForID(object keyID, AudioClip clip, float time, float previousTime, float volume, bool ignoreTimescale = false)
	{
		AudioSource sourceForID = GetSourceForID(keyID);
		Sample(sourceForID, clip, time, previousTime, volume, ignoreTimescale);
	}

	public static void Sample(AudioSource source, AudioClip clip, float time, float previousTime, float volume, bool ignoreTimescale = false)
	{
		if (source == null)
		{
			return;
		}
		if (previousTime == time)
		{
			source.Stop();
			return;
		}
		source.clip = clip;
		source.volume = volume;
		source.pitch = ((!ignoreTimescale) ? Time.timeScale : 1f);
		if (!source.isPlaying)
		{
			source.Play();
		}
		time = Mathf.Repeat(time, clip.length - 0.001f);
		if (Mathf.Abs(source.time - time) > 0.1f * Time.timeScale)
		{
			source.time = time;
		}
	}
}

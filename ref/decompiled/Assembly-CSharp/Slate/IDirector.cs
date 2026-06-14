using System.Collections.Generic;
using UnityEngine;

namespace Slate;

public interface IDirector
{
	GameObject context { get; }

	float length { get; }

	float currentTime { get; set; }

	float previousTime { get; }

	float playbackSpeed { get; set; }

	bool isReSampleFrame { get; }

	IEnumerable<GameObject> GetAffectedActors();

	void Play();

	void Pause();

	void Stop();

	void Sample(float time);

	void ReSample();

	void Validate();

	void SendGlobalMessage(string message, object value);
}

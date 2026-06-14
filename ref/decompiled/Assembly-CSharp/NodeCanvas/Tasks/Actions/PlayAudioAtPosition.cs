using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Audio")]
public class PlayAudioAtPosition : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<AudioClip> audioClip;

	[SliderField(0, 1)]
	public float volume = 1f;

	public bool waitActionFinish;

	protected override string info => "PlayAudio " + audioClip.ToString();

	protected override void OnExecute()
	{
		AudioSource.PlayClipAtPoint(audioClip.value, base.agent.position, volume);
		if (!waitActionFinish)
		{
			EndAction();
		}
	}

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= audioClip.value.length)
		{
			EndAction();
		}
	}
}

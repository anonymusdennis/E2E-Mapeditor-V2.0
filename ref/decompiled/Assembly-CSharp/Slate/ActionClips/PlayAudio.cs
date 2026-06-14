using System;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[]
{
	typeof(ActorAudioTrack),
	typeof(DirectorAudioTrack)
})]
[Name("Audio Clip")]
[Description("The audio clip will be send to the AudioMixer selected in it's track if any. You can trim or loop the audio by scaling the clip and you can optionaly show subtitles as well.")]
public class PlayAudio : ActionClip, ISubClipContainable, IDirectable
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.25f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.25f;

	[Required]
	public AudioClip audioClip;

	[AnimatableParameter(0f, 1f)]
	public float volume = 1f;

	[AnimatableParameter(-1f, 1f)]
	public float stereoPan;

	public float clipOffset;

	[Multiline(5)]
	public string subtitlesText;

	public Color subtitlesColor = Color.white;

	float ISubClipContainable.subClipOffset
	{
		get
		{
			return clipOffset;
		}
		set
		{
			clipOffset = value;
		}
	}

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public override float blendIn
	{
		get
		{
			return _blendIn;
		}
		set
		{
			_blendIn = value;
		}
	}

	public override float blendOut
	{
		get
		{
			return _blendOut;
		}
		set
		{
			_blendOut = value;
		}
	}

	public override bool isValid => audioClip != null;

	public override string info => (!isValid) ? base.info : ((!string.IsNullOrEmpty(subtitlesText)) ? $"<i>'{subtitlesText}'</i>" : audioClip.name);

	private AudioTrack track => (AudioTrack)base.parent;

	private AudioSource source => track.source;

	protected override void OnEnter()
	{
		if (source != null)
		{
			source.clip = audioClip;
		}
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (source != null)
		{
			float num = Easing.Ease(EaseType.QuadraticInOut, 0f, 1f, GetClipWeight(time));
			float num2 = num * volume * track.weight;
			AudioSampler.Sample(source, audioClip, time - clipOffset, previousTime - clipOffset, num2);
			source.panStereo = Mathf.Clamp01(stereoPan + track.stereoPan);
			if (!string.IsNullOrEmpty(subtitlesText))
			{
				Color color = subtitlesColor;
				color.a = num;
				DirectorGUI.UpdateSubtitles(string.Format("{0}{1}", (!(base.parent is ActorAudioTrack)) ? string.Empty : (base.actor.name + ": "), subtitlesText), color);
			}
		}
	}

	protected override void OnExit()
	{
		if (source != null)
		{
			source.clip = null;
		}
	}

	protected override void OnReverse()
	{
		if (source != null)
		{
			source.clip = null;
		}
	}
}

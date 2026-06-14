using UnityEngine;
using UnityEngine.Audio;

namespace Slate;

[Name("Audio Track")]
[Description("All audio clips played by this track will be send to the selected AudioMixer if any.")]
[Icon("AudioClip Icon")]
public abstract class AudioTrack : CutsceneTrack
{
	[SerializeField]
	private AudioMixerGroup _outputMixer;

	[Range(0f, 1f)]
	[SerializeField]
	private float _masterVolume = 1f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _stereoPan;

	[Range(0f, 1f)]
	[SerializeField]
	private float _spatialBlend;

	public override string info => string.Format("Mixer: {0} ({1})", (!(mixer != null)) ? "NONE" : mixer.name, weight.ToString("0.0"));

	public AudioSource source { get; private set; }

	public float weight => _masterVolume;

	public AudioMixerGroup mixer
	{
		get
		{
			return _outputMixer;
		}
		set
		{
			_outputMixer = value;
		}
	}

	public float stereoPan
	{
		get
		{
			return _stereoPan;
		}
		set
		{
			_stereoPan = value;
		}
	}

	public float spatialBlend
	{
		get
		{
			return _spatialBlend;
		}
		set
		{
			_spatialBlend = value;
		}
	}

	public virtual bool useAudioSourceOnActor => false;

	protected override void OnEnter()
	{
		Enable();
	}

	protected override void OnReverseEnter()
	{
		Enable();
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (!useAudioSourceOnActor && source != null && !(base.parent is DirectorGroup))
		{
			source.transform.position = base.actor.transform.position;
		}
	}

	protected override void OnExit()
	{
		Disable();
	}

	protected override void OnReverse()
	{
		Disable();
	}

	private void Enable()
	{
		if (useAudioSourceOnActor)
		{
			source = base.actor.GetComponent<AudioSource>();
		}
		if (source == null)
		{
			source = AudioSampler.GetSourceForID(this);
		}
		ApplySettings();
	}

	private void Disable()
	{
		if (!useAudioSourceOnActor)
		{
			AudioSampler.ReleaseSourceForID(this);
		}
		source = null;
	}

	private void ApplySettings()
	{
		if (source != null)
		{
			source.outputAudioMixerGroup = mixer;
			source.volume = weight;
			source.spatialBlend = spatialBlend;
			source.panStereo = stereoPan;
		}
	}
}

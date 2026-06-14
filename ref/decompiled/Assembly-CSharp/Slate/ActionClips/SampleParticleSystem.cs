using UnityEngine;

namespace Slate.ActionClips;

[Category("Control")]
public class SampleParticleSystem : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[Required]
	public ParticleSystem particles;

	private ParticleSystem.EmissionModule em;

	public override string info => string.Format("Particles ({0})\n{1}", (!particles || !loop) ? "OneShot" : "Looping", (!particles) ? "NONE" : particles.gameObject.name);

	public override bool isValid => particles != null;

	public override float length
	{
		get
		{
			return (!(particles == null) && !loop) ? (duration + startLifetime) : _length;
		}
		set
		{
			_length = value;
		}
	}

	public override float blendOut => (!isValid || loop) ? 0.1f : startLifetime;

	private bool loop => particles != null && particles.main.loop;

	private float duration => (!(particles != null)) ? 0f : particles.main.duration;

	private float startLifetime => (!(particles != null)) ? 0f : particles.main.startLifetimeMultiplier;

	protected override void OnEnter()
	{
		em = particles.emission;
		em.enabled = true;
		particles.Play();
	}

	protected override void OnUpdate(float time)
	{
		if (!Application.isPlaying)
		{
			em.enabled = time < length;
			particles.Simulate(time);
		}
	}

	protected override void OnExit()
	{
		em.enabled = false;
		particles.Stop();
	}
}

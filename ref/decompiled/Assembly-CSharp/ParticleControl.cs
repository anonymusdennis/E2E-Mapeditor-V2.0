using UnityEngine;

public class ParticleControl : MonoBehaviour
{
	public ParticleSystem Particle;

	public bool bPlaying;

	public bool bVisible;

	public bool isPlaying => Particle.isPlaying;

	public new Transform transform => Particle.transform;

	public new string name
	{
		get
		{
			return Particle.name;
		}
		set
		{
			Particle.name = value;
		}
	}

	public ParticleControl(ParticleSystem particle)
	{
		bPlaying = particle.isPlaying;
	}

	public void Simulate(float t, bool bWithChildren, bool bRestart)
	{
		Particle.Simulate(t, bWithChildren, bRestart);
	}

	public void Play()
	{
		if (bVisible)
		{
			Particle.Play();
		}
		bPlaying = true;
	}

	public void Stop()
	{
		if (bVisible)
		{
			Particle.Stop();
		}
		bPlaying = false;
	}
}

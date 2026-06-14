using UnityEngine;
using UnityEngine.UI;

public class VideoDrone : MonoBehaviour
{
	public delegate void MovieDelegate();

	public VideoPlaybackSettings m_Settings;

	protected bool m_bPlaying;

	protected Texture m_OutputTexture;

	protected Material m_OutputMaterial;

	public bool IsPlaying => m_bPlaying;

	public Texture OutputTexture => m_OutputTexture;

	public Material OutputMaterial => m_OutputMaterial;

	public event MovieDelegate OnMovieEnd;

	public static VideoDrone CreateDrone(GameObject parent, VideoPlaybackSettings settings, RawImage rawImage = null, MeshRenderer meshRenderer = null)
	{
		PCVideoDrone pCVideoDrone = parent.AddComponent<PCVideoDrone>();
		pCVideoDrone.m_Settings = settings;
		return pCVideoDrone;
	}

	protected virtual void Awake()
	{
	}

	protected virtual void Update()
	{
	}

	public virtual bool Play(bool videoLoops = false, bool audioOn = true)
	{
		if (m_bPlaying)
		{
			return false;
		}
		return true;
	}

	public virtual void StopVideo()
	{
	}

	protected void SignalVideoEnded()
	{
		if (this.OnMovieEnd != null)
		{
			this.OnMovieEnd();
		}
	}
}

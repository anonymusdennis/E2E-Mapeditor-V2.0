using UnityEngine;

[RequireComponent(typeof(CanvasAlphaChanger))]
public class CutsceneUIFadingListener : T17MonoBehaviour
{
	private CanvasAlphaChanger m_CanvasFader;

	protected override void Awake()
	{
		base.Awake();
		m_CanvasFader = GetComponent<CanvasAlphaChanger>();
	}

	private void Start()
	{
		CutsceneManagerBase.PrepareForCutsceneEvent += CutsceneManagerBase_PrepareForCutscene;
		CutsceneManagerBase.CutsceneFinishedEvent += CutsceneManagerBase_CutsceneFinishedEvent;
	}

	protected virtual void OnDestroy()
	{
		CutsceneManagerBase.PrepareForCutsceneEvent -= CutsceneManagerBase_PrepareForCutscene;
		CutsceneManagerBase.CutsceneFinishedEvent -= CutsceneManagerBase_CutsceneFinishedEvent;
		m_CanvasFader = null;
	}

	private void CutsceneManagerBase_CutsceneFinishedEvent(float timeUntilCurtainRaised)
	{
		m_CanvasFader.FadeToOpaque(timeUntilCurtainRaised);
	}

	private void CutsceneManagerBase_PrepareForCutscene(float timeUntilStart)
	{
		m_CanvasFader.FadeToTransparent(timeUntilStart);
	}
}

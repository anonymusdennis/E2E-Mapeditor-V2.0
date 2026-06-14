using UnityEngine;

public class RenderingBenchmark : MonoBehaviour
{
	public delegate void BenchmarkComplete(float benchmarkTime, int benchmarkFramerate);

	public int framesToBenchmark = 10;

	private float m_benchmarkStartTime;

	private bool m_benchmarkEnabled;

	private int m_currentFramesBenchmarked;

	private int m_oldVSync;

	public event BenchmarkComplete OnBenchmarkComplete;

	public bool IsRunning()
	{
		return m_benchmarkEnabled;
	}

	public bool IsDone()
	{
		return !m_benchmarkEnabled && m_currentFramesBenchmarked >= framesToBenchmark;
	}

	public void StartBenchmark()
	{
		m_benchmarkStartTime = Time.time;
		m_oldVSync = QualitySettings.vSyncCount;
		QualitySettings.vSyncCount = 0;
		m_currentFramesBenchmarked = 0;
		m_benchmarkEnabled = true;
		base.gameObject.SetActive(value: true);
	}

	private float StopBenchmark()
	{
		QualityManager.SetVsyncCount(m_oldVSync);
		m_benchmarkEnabled = false;
		base.gameObject.SetActive(value: false);
		return Time.time - m_benchmarkStartTime;
	}

	private void Update()
	{
		if (!m_benchmarkEnabled)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (++m_currentFramesBenchmarked >= framesToBenchmark)
		{
			float benchmarkTime = StopBenchmark();
			int benchmarkAverageFPS = GetBenchmarkAverageFPS(benchmarkTime);
			if (this.OnBenchmarkComplete != null)
			{
				this.OnBenchmarkComplete(benchmarkTime, benchmarkAverageFPS);
			}
		}
	}

	private int GetBenchmarkAverageFPS(float benchmarkTime)
	{
		return Mathf.RoundToInt((float)framesToBenchmark / benchmarkTime);
	}
}

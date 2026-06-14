public class FPS_No_String_Allocs
{
	private string[] m_PreAllocatedReturnStrings;

	private string[] m_PreAllocatedMinimumReturnStrings;

	private string[] m_PreAllocatedAbsMinimumReturnStrings;

	private const float MIN_FPS = 1f;

	private const float MAX_FPS = 120f;

	private const float STEP = 0.064f;

	private const int NUM_VALUES = 1859;

	private const float INV_STEP = 15.624999f;

	private const int NUM_FPS_SAMPLES = 10;

	private float[] m_FPS = new float[10];

	private float m_AveFPS;

	private int m_OldFPSSlot;

	private bool m_bCanRecordMinimums;

	private float m_bDelayUntilRecording = 2f;

	private float m_MinimumAverageFPS = float.MaxValue;

	private float m_MaximumDT;

	public FPS_No_String_Allocs()
	{
		AllocateStrings(out m_PreAllocatedReturnStrings, "FPS");
		AllocateStrings(out m_PreAllocatedMinimumReturnStrings, "Min Avg FPS");
		AllocateStrings(out m_PreAllocatedAbsMinimumReturnStrings, "Abs Min FPS");
	}

	private void AllocateStrings(out string[] strings, string fpsText)
	{
		strings = new string[1861];
		float num = 1f;
		for (int i = 0; i < 1859; i++)
		{
			strings[i] = string.Format(fpsText + ": {0:F2}", num);
			num += 0.064f;
		}
		strings[1859] = fpsText + ": ^^^^^ ";
		strings[1860] = fpsText + ": _____ ";
	}

	public void Update()
	{
		m_FPS[m_OldFPSSlot] = UpdateManager.deltaTime;
		float num = 0f;
		for (int i = 0; i < 10; i++)
		{
			float num2 = m_FPS[i];
			num += num2;
			if (m_bCanRecordMinimums && num2 > m_MaximumDT)
			{
				m_MaximumDT = num2;
			}
		}
		num = 10f / num;
		m_OldFPSSlot++;
		m_AveFPS = num;
		if (m_OldFPSSlot >= 10)
		{
			m_OldFPSSlot = 0;
		}
		if (m_bCanRecordMinimums)
		{
			if (m_AveFPS < m_MinimumAverageFPS)
			{
				m_MinimumAverageFPS = m_AveFPS;
			}
		}
		else if (m_bDelayUntilRecording > 0f)
		{
			m_bDelayUntilRecording -= UpdateManager.deltaTime;
			if (m_bDelayUntilRecording <= 0f)
			{
				m_bCanRecordMinimums = true;
			}
		}
	}

	public string GetAverageFPSString()
	{
		int num = (int)((m_AveFPS - 1f) * 15.624999f);
		if (num > 1859)
		{
			num = 1859;
		}
		else if (num < 0)
		{
			num = 1860;
		}
		return m_PreAllocatedReturnStrings[num];
	}

	public string GetAverageMinimumString()
	{
		int num = (int)((m_MinimumAverageFPS - 1f) * 15.624999f);
		if (num > 1859)
		{
			num = 1859;
		}
		else if (num < 0)
		{
			num = 1860;
		}
		return m_PreAllocatedMinimumReturnStrings[num];
	}

	public string GetAbsoluteMinimumString()
	{
		int num = (int)((1f / m_MaximumDT - 1f) * 15.624999f);
		if (num > 1859)
		{
			num = 1859;
		}
		else if (num < 0)
		{
			num = 1860;
		}
		return m_PreAllocatedAbsMinimumReturnStrings[num];
	}

	public float AverageFPS()
	{
		return m_AveFPS;
	}

	public void ResetMinimums(float delayRecordingTime = 0f)
	{
		m_MaximumDT = 0f;
		m_MinimumAverageFPS = float.MaxValue;
		m_bDelayUntilRecording = delayRecordingTime;
		m_bCanRecordMinimums = false;
	}
}
